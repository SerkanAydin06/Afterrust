#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Adds the Meshy "Neutral T-Pose Figure" rigged in Mixamo (Characters/1/Walking.fbx,
    /// with skin, In Place, embedded base color) as a walking NPC. Idle is the scavenger's
    /// Humanoid Idle retargeted. Re-running replaces the previous instance.
    /// </summary>
    public static class NeutralFigureSetup
    {
        private const string ModelPath = "Assets/Arts/Characters/1/Walking.fbx";
        private const string ControllerPath = "Assets/Arts/Characters/1/NeutralFigureLocomotion.controller";
        private const string RigFbxPath = ScavengerWalkSetup.RiggedFolder + "/Scavenger_Rigged.fbx";
        private const string RootName = "Neutral Figure (Meshy + Mixamo)";
        private const float TargetHeight = 1.8f;
        // Measured stride: ~0.52 x height per 31-frame cycle -> ~1.45 m/s at 1.8 m.
        private const float WalkSpeed = 1.45f;

        // Front-right of the player spawn (0, 0, -9), clear of the trade post (5, 0, 0).
        private static readonly Vector3 Spot = new Vector3(3.5f, 0f, -5.5f);
        private static readonly Vector3[] Waypoints =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(2.5f, 0f, 0f),
            new Vector3(2.5f, 0f, 2.5f),
            new Vector3(0f, 0f, 2.5f)
        };

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
            bool changed = RiggedCharacterImport.ApplyModelSettings(importer);
            changed |= RiggedCharacterImport.ApplyClipSettings(importer);
            if (changed) importer.SaveAndReimport();

            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(ModelPath);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                Debug.LogWarning("[Afterrust] Neutral Figure Humanoid avatar invalid. FBX > Rig > Configure.");

            AnimationClip walk = Character1Setup.FindClip(ModelPath, null);
            if (walk == null)
            {
                Debug.LogError("[Afterrust] No walk clip in " + ModelPath);
                return;
            }
            AnimationClip idle = Character1Setup.FindClip(RigFbxPath, "idle");
            AnimatorController controller = ScavengerWalkSetup.BuildController(walk, idle, WalkSpeed, ControllerPath);

            GameObject root = WalkingNpcSetup.AddWalker(ModelPath, importer, avatar, controller, RootName, Spot,
                Waypoints, WalkSpeed, TargetHeight);
            Material skin = MaterialKit.Character("Assets/Arts/Characters/1/Materials", "M_NeutralFigure",
                "Assets/Arts/Characters/1/Textures/NeutralFigure_albedo.jpg", null, false, 0.2f);
            WalkingNpcSetup.ApplyMaterials(root.transform.GetChild(0).gameObject, (mat, rend) => skin);
            Selection.activeGameObject = root;
            Debug.Log("[Afterrust] Neutral Figure added at " + Spot + ". Save scene (Ctrl+S) and press Play.");
        }
    }
}
#endif
