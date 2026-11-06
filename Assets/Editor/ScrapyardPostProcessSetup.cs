#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Post Processing Stack v2 (built-in pipeline): ACES HDR color grading, bloom, ambient
    /// occlusion, vignette and light film grain on a global volume, plus a PostProcessLayer on
    /// every scene camera. One profile per lighting preset, rebuilt on each run.
    /// </summary>
    public static class ScrapyardPostProcessSetup
    {
        private const string ProfileDir = "Assets/Arts/Environment/PostProcessing";
        private const string VolumeName = "Post Process Volume";
        private const string LayerName = "PostProcessing";
        private const string ResourcesPath = "Packages/com.unity.postprocessing/PostProcessing/PostProcessResources.asset";

        public struct Grade
        {
            public string name;
            public float temperature, tint, saturation, contrast, postExposure;
            public Color lift, gamma, gain;
            public float bloomIntensity, vignette, aoIntensity, grain;
        }

        public static readonly Grade GoldenHour = new Grade
        {
            name = "GoldenHour",
            temperature = 9f, tint = 2f, saturation = -6f, contrast = 14f, postExposure = 0.25f,
            lift = new Color(1f, 0.99f, 1.02f, -0.02f), gamma = new Color(1.02f, 1f, 0.97f, 0f),
            gain = new Color(1.03f, 1f, 0.95f, 0f),
            bloomIntensity = 1.4f, vignette = 0.3f, aoIntensity = 0.65f, grain = 0.14f
        };

        public static readonly Grade Overcast = new Grade
        {
            name = "Overcast",
            temperature = -6f, tint = 0f, saturation = -22f, contrast = 10f, postExposure = 0.35f,
            lift = new Color(0.99f, 1f, 1.03f, -0.01f), gamma = new Color(1f, 1f, 1.02f, 0f),
            gain = new Color(0.98f, 1f, 1.02f, 0f),
            bloomIntensity = 0.8f, vignette = 0.34f, aoIntensity = 0.75f, grain = 0.18f
        };

        /// <summary>Bleak storm look: cold, desaturated, heavier vignette and grain.</summary>
        public static readonly Grade Storm = new Grade
        {
            name = "Storm",
            temperature = -9f, tint = 2f, saturation = -32f, contrast = 16f, postExposure = 0.3f,
            lift = new Color(0.98f, 1f, 1.04f, -0.02f), gamma = new Color(0.99f, 1f, 1.03f, 0f),
            gain = new Color(0.97f, 1f, 1.03f, 0f),
            bloomIntensity = 1.1f, vignette = 0.38f, aoIntensity = 0.75f, grain = 0.2f
        };

        public static void Apply(Grade g)
        {
            PostProcessProfile profile = BuildProfile(g);
            int layer = LayerMask.NameToLayer(LayerName);
            if (layer < 0)
            {
                Debug.LogWarning("[Afterrust] Layer '" + LayerName + "' missing; using Default.");
                layer = 0;
            }

            Transform old = ScavengerWalkSetup.FindInScene(VolumeName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject volumeGo = new GameObject(VolumeName) { layer = layer };
            Undo.RegisterCreatedObjectUndo(volumeGo, "Post process volume");
            PostProcessVolume volume = volumeGo.AddComponent<PostProcessVolume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.sharedProfile = profile;

            PostProcessResources resources = AssetDatabase.LoadAssetAtPath<PostProcessResources>(ResourcesPath);
            foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                PostProcessLayer ppl = cam.GetComponent<PostProcessLayer>();
                if (ppl == null) ppl = Undo.AddComponent<PostProcessLayer>(cam.gameObject);
                if (resources != null) ppl.Init(resources);
                ppl.volumeTrigger = cam.transform;
                ppl.volumeLayer = 1 << layer;
                // MSAA 4x (Quality settings) handles geometry edges; no extra blur.
                ppl.antialiasingMode = PostProcessLayer.Antialiasing.None;
                ppl.stopNaNPropagation = true;
                EditorUtility.SetDirty(ppl);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static PostProcessProfile BuildProfile(Grade g)
        {
            if (!AssetDatabase.IsValidFolder(ProfileDir))
                AssetDatabase.CreateFolder("Assets/Arts/Environment", "PostProcessing");
            string path = ProfileDir + "/PP_" + g.name + ".asset";
            if (AssetDatabase.LoadAssetAtPath<PostProcessProfile>(path) != null) AssetDatabase.DeleteAsset(path);

            PostProcessProfile profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            AssetDatabase.CreateAsset(profile, path);

            ColorGrading cg = Add<ColorGrading>(profile);
            cg.gradingMode.Override(GradingMode.HighDefinitionRange);
            cg.tonemapper.Override(Tonemapper.ACES);
            cg.temperature.Override(g.temperature);
            cg.tint.Override(g.tint);
            cg.saturation.Override(g.saturation);
            cg.contrast.Override(g.contrast);
            cg.postExposure.Override(g.postExposure);
            cg.lift.Override(new Vector4(g.lift.r, g.lift.g, g.lift.b, g.lift.a));
            cg.gamma.Override(new Vector4(g.gamma.r, g.gamma.g, g.gamma.b, g.gamma.a));
            cg.gain.Override(new Vector4(g.gain.r, g.gain.g, g.gain.b, g.gain.a));

            Bloom bloom = Add<Bloom>(profile);
            bloom.intensity.Override(g.bloomIntensity);
            bloom.threshold.Override(1.05f);
            bloom.softKnee.Override(0.6f);
            bloom.diffusion.Override(7f);

            AmbientOcclusion ao = Add<AmbientOcclusion>(profile);
            ao.mode.Override(AmbientOcclusionMode.MultiScaleVolumetricObscurance);
            ao.intensity.Override(g.aoIntensity);
            ao.thicknessModifier.Override(1.2f);

            Vignette vignette = Add<Vignette>(profile);
            vignette.intensity.Override(g.vignette);
            vignette.smoothness.Override(0.45f);
            vignette.rounded.Override(false);

            Grain grain = Add<Grain>(profile);
            grain.intensity.Override(g.grain);
            grain.size.Override(0.7f);
            grain.colored.Override(false);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static T Add<T>(PostProcessProfile profile) where T : PostProcessEffectSettings
        {
            T effect = profile.AddSettings<T>();
            effect.enabled.Override(true);
            effect.name = typeof(T).Name;
            effect.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(effect, profile);
            return effect;
        }
    }
}
#endif
