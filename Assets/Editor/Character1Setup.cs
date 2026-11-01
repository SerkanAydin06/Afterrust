#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Adds the Mixamo character (character1/Ch21_nonPBR.fbx) to the open scene as a
    /// walking NPC. It has no own walk clip, so the shared Humanoid Idle/Walk controller
    /// of the scavenger is retargeted onto it. Re-running replaces the previous instance.
    /// </summary>
    public static class Character1Setup
    {
        private const string ModelPath = "Assets/Arts/Characters/character1/Ch21_nonPBR.fbx";
        private const string RigFbxPath = ScavengerWalkSetup.RiggedFolder + "/Scavenger_Rigged.fbx";
        private const string RootName = "Character1 (Mixamo Ch21)";
        private const float TargetHeight = 1.78f;

        // In front-left of the player spawn (0, 0, -9), clear of the car and trade post.
        private static readonly Vector3 Spot = new Vector3(-4.5f, 0f, -4f);
        private static readonly Vector3[] Waypoints =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(-2.5f, 0f, 0f),
            new Vector3(-2.5f, 0f, 2.5f),
            new Vector3(0f, 0f, 2.5f)
        };

        [MenuItem("Afterrust/Add Character1 (Mixamo, walking)")]
        public static void Add()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Afterrust] Stop Play mode first.");
                return;
            }

            ModelImporter importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError("[Afterrust] Model not found: " + ModelPath + " (Git LFS pulled?)");
                return;
            }
            if (RiggedCharacterImport.ApplyModelSettings(importer)) importer.SaveAndReimport();

            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(ModelPath);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                Debug.LogWarning("[Afterrust] Character1 Humanoid avatar invalid. FBX > Rig > Configure.");

            AnimatorController controller = LoadOrBuildController();
            if (controller == null) return;

            Transform old = ScavengerWalkSetup.FindInScene(RootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Add Character1");
            root.transform.position = Spot;

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath));
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            if (ScavengerWalkSetup.TryGetBounds(model, out Bounds b) && b.size.y > 0.001f)
                model.transform.localScale *= TargetHeight / b.size.y;

            Vector3 forward = ScavengerWalkSetup.EstimateForward(model.transform, importer);
            if (forward.sqrMagnitude > 0.001f)
                model.transform.rotation = Quaternion.FromToRotation(forward, root.transform.forward) *
                                           model.transform.rotation;

            if (ScavengerWalkSetup.TryGetBounds(model, out b))
                model.transform.position += new Vector3(Spot.x - b.center.x, -b.min.y, Spot.z - b.center.z);

            Animator animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            if (avatar != null) animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            ScavengerPatrol patrol = root.AddComponent<ScavengerPatrol>();
            patrol.relativeWaypoints = (Vector3[])Waypoints.Clone();
            patrol.animator = animator;

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Afterrust] Character1 added at " + Spot + " with Idle/Walk patrol. Save scene (Ctrl+S) and press Play.");
        }

        private static AnimatorController LoadOrBuildController()
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(ScavengerWalkSetup.ControllerPath);
            if (controller != null) return controller;

            AnimationClip walk = null, idle = null;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(RigFbxPath))
            {
                AnimationClip clip = asset as AnimationClip;
                if (clip == null || clip.name.StartsWith("__preview__")) continue;
                string n = clip.name.ToLowerInvariant();
                if (n.Contains("walk")) walk = clip;
                else if (n.Contains("idle")) idle = clip;
            }
            if (walk == null)
            {
                Debug.LogError("[Afterrust] Walk clip not found. Run Afterrust > Setup Walking Scavenger (Rigged) first.");
                return null;
            }
            return ScavengerWalkSetup.BuildController(walk, idle, 1.2f);
        }
    }
}
#endif
