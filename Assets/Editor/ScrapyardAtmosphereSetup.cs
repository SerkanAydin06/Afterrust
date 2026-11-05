#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Lighting and atmosphere presets for the scrapyard (built-in render pipeline):
    /// procedural sky, sun, trilight ambient, distance fog, camera sky, warm workshop lamps and
    /// the matching post-processing grade (see ScrapyardPostProcessSetup).
    /// Only RenderSettings, the "Sun", the player camera and the "Scrapyard Lamps" root are touched.
    /// </summary>
    public static class ScrapyardAtmosphereSetup
    {
        private const string MatDir = "Assets/Arts/Environment/Materials";
        private const string LampsRootName = "Scrapyard Lamps";

        private struct Preset
        {
            public string name;
            public Vector3 sunEuler;
            public Color sunColor;
            public float sunIntensity, shadowStrength;
            public Color skyTint, skyGround;
            public float atmosphere, exposure;
            public Color ambientSky, ambientEquator, ambientGround;
            public Color fogColor;
            public float fogDensity;
            public float lampIntensity;
        }

        private static readonly Preset GoldenHour = new Preset
        {
            name = "Golden Hour",
            sunEuler = new Vector3(16f, -58f, 0f),
            sunColor = new Color(1f, 0.76f, 0.52f),
            sunIntensity = 1.35f,
            shadowStrength = 0.82f,
            skyTint = new Color(0.62f, 0.55f, 0.48f),
            skyGround = new Color(0.36f, 0.31f, 0.26f),
            atmosphere = 1.35f,
            exposure = 1.1f,
            ambientSky = new Color(0.50f, 0.52f, 0.58f),
            ambientEquator = new Color(0.46f, 0.38f, 0.30f),
            ambientGround = new Color(0.20f, 0.17f, 0.14f),
            fogColor = new Color(0.66f, 0.56f, 0.45f),
            fogDensity = 0.011f,
            lampIntensity = 1.2f
        };

        private static readonly Preset Overcast = new Preset
        {
            name = "Overcast",
            sunEuler = new Vector3(55f, -30f, 0f),
            sunColor = new Color(0.80f, 0.84f, 0.88f),
            sunIntensity = 0.55f,
            shadowStrength = 0.35f,
            skyTint = new Color(0.45f, 0.48f, 0.50f),
            skyGround = new Color(0.30f, 0.30f, 0.30f),
            atmosphere = 0.7f,
            exposure = 0.75f,
            ambientSky = new Color(0.46f, 0.49f, 0.53f),
            ambientEquator = new Color(0.36f, 0.37f, 0.38f),
            ambientGround = new Color(0.18f, 0.17f, 0.16f),
            fogColor = new Color(0.47f, 0.49f, 0.51f),
            fogDensity = 0.02f,
            lampIntensity = 1.6f
        };

        [MenuItem("Afterrust/Lighting/Golden Hour (default)")]
        public static void ApplyGoldenHour() { Apply(GoldenHour); }

        [MenuItem("Afterrust/Lighting/Overcast")]
        public static void ApplyOvercast() { Apply(Overcast); }

        /// <summary>One click: environment (if missing) + golden hour lighting + post-processing.</summary>
        [MenuItem("Afterrust/Apply Pro Visual Setup", false, 0)]
        public static void ApplyProVisualSetup()
        {
            if (ScavengerWalkSetup.FindInScene("Scrapyard Environment") == null) ScrapyardEnvironmentBuilder.Build();
            Apply(GoldenHour);
        }

        private static void Apply(Preset p)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Afterrust] Stop Play mode first.");
                return;
            }

            Light sun = FindSun();
            if (sun == null)
            {
                sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional;
                Undo.RegisterCreatedObjectUndo(sun.gameObject, "Create sun");
            }
            Undo.RecordObject(sun, "Sun settings");
            Undo.RecordObject(sun.transform, "Sun rotation");
            sun.transform.rotation = Quaternion.Euler(p.sunEuler);
            sun.color = p.sunColor;
            sun.intensity = p.sunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = p.shadowStrength;

            Material sky = SkyMaterial();
            sky.SetFloat("_SunDisk", 2f);
            sky.SetFloat("_SunSize", 0.035f);
            sky.SetFloat("_SunSizeConvergence", 6f);
            sky.SetFloat("_AtmosphereThickness", p.atmosphere);
            sky.SetColor("_SkyTint", p.skyTint);
            sky.SetColor("_GroundColor", p.skyGround);
            sky.SetFloat("_Exposure", p.exposure);
            EditorUtility.SetDirty(sky);

            RenderSettings.skybox = sky;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = p.ambientSky;
            RenderSettings.ambientEquatorColor = p.ambientEquator;
            RenderSettings.ambientGroundColor = p.ambientGround;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
            RenderSettings.reflectionIntensity = 0.55f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = p.fogColor;
            RenderSettings.fogDensity = p.fogDensity;

            foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Undo.RecordObject(cam, "Camera sky");
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.farClipPlane = Mathf.Max(cam.farClipPlane, 300f);
                cam.allowHDR = true;
                cam.allowMSAA = true;
                if (cam.name == "First Person Camera") cam.fieldOfView = 72f;
            }

            BuildLamps(p.lampIntensity);
            ScrapyardPostProcessSetup.Apply(p.name == Overcast.name
                ? ScrapyardPostProcessSetup.Overcast : ScrapyardPostProcessSetup.GoldenHour);
            DynamicGI.UpdateEnvironment();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Afterrust] Lighting preset applied: " + p.name + ". Save scene (Ctrl+S).");
        }

        private static Light FindSun()
        {
            Light fallback = null;
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (l.type != LightType.Directional) continue;
                if (l.name == "Sun") return l;
                if (fallback == null) fallback = l;
            }
            return fallback;
        }

        private static Material SkyMaterial()
        {
            string path = MatDir + "/SKY_Scrapyard.mat";
            if (!AssetDatabase.IsValidFolder(MatDir)) AssetDatabase.CreateFolder("Assets/Arts/Environment", "Materials");
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Skybox/Procedural")) { name = "SKY_Scrapyard" };
                AssetDatabase.CreateAsset(m, path);
            }
            return m;
        }

        private static Material Mat(string name, Color baseColor, Color emission)
        {
            string path = MatDir + "/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Standard")) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = baseColor;
            if (emission.maxColorComponent > 0f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Warm wall lamps on the workshop walls and gate posts (no shadows, cheap).</summary>
        private static void BuildLamps(float intensity)
        {
            Transform old = ScavengerWalkSetup.FindInScene(LampsRootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(LampsRootName);
            Undo.RegisterCreatedObjectUndo(root, "Scrapyard lamps");

            Material housing = Mat("M_LampHousing", new Color(0.12f, 0.11f, 0.10f), Color.black);
            Material bulb = Mat("M_LampBulb", new Color(1f, 0.85f, 0.6f), new Color(2.2f, 1.5f, 0.8f));

            // position, facing direction (lamp sticks out of the wall along it)
            (Vector3 pos, Vector3 dir)[] lamps =
            {
                (new Vector3(3f, 2.6f, 5.85f), Vector3.back),
                (new Vector3(7f, 2.6f, 5.85f), Vector3.back),
                (new Vector3(8.85f, 2.6f, 0f), Vector3.left),
                (new Vector3(-4f, 2.8f, -19.9f), Vector3.forward),
                (new Vector3(4f, 2.8f, -19.9f), Vector3.forward)
            };
            foreach ((Vector3 pos, Vector3 dir) in lamps)
            {
                GameObject lamp = new GameObject("Wall Lamp");
                lamp.transform.SetParent(root.transform, false);
                lamp.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir, Vector3.up));

                GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = "Housing";
                Object.DestroyImmediate(box.GetComponent<Collider>());
                box.transform.SetParent(lamp.transform, false);
                box.transform.localPosition = new Vector3(0f, 0f, 0.1f);
                box.transform.localScale = new Vector3(0.3f, 0.12f, 0.2f);
                box.GetComponent<Renderer>().sharedMaterial = housing;

                GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glow.name = "Bulb";
                Object.DestroyImmediate(glow.GetComponent<Collider>());
                glow.transform.SetParent(lamp.transform, false);
                glow.transform.localPosition = new Vector3(0f, -0.07f, 0.12f);
                glow.transform.localScale = new Vector3(0.22f, 0.03f, 0.14f);
                glow.GetComponent<Renderer>().sharedMaterial = bulb;
                glow.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

                Light light = new GameObject("Light").AddComponent<Light>();
                light.transform.SetParent(lamp.transform, false);
                light.transform.localPosition = new Vector3(0f, -0.25f, 0.35f);
                light.type = LightType.Point;
                light.range = 7f;
                light.intensity = intensity;
                light.color = new Color(1f, 0.72f, 0.42f);
                light.shadows = LightShadows.None;
            }
        }
    }
}
#endif
