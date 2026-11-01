#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Replaces the static scavenger with the rigged one from
    /// Assets/Arts/Characters/Scavenger/Rigged/, builds an Idle/Walk locomotion
    /// controller and connects it to ScavengerPatrol. Scene layout is otherwise kept.
    /// </summary>
    public static class ScavengerWalkSetup
    {
        public const string RiggedFolder = "Assets/Arts/Characters/Scavenger/Rigged";
        internal const string ControllerPath = RiggedFolder + "/ScavengerLocomotion.controller";
        private const string RiggedInstanceName = "Meshy Scavenger - Rigged";
        private const string StaticModelName = "Meshy Scavenger - Imported Model";
        private const float TargetHeight = 1.75f;

        [MenuItem("Afterrust/Setup Walking Scavenger (Rigged)")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Afterrust] Stop Play mode first.");
                return;
            }
            if (!AssetDatabase.IsValidFolder(RiggedFolder))
            {
                Debug.LogError("[Afterrust] Put the rigged FBX and walk/idle FBX files in " + RiggedFolder);
                return;
            }

            // 1. Import settings (Humanoid + looping in-place clips).
            string characterPath = null;
            List<string> fbxPaths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { RiggedFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!RiggedCharacterImport.IsRiggedCharacterPath(path)) continue;
                fbxPaths.Add(path);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;
                bool changed = RiggedCharacterImport.ApplyModelSettings(importer);
                changed |= RiggedCharacterImport.ApplyClipSettings(importer);
                if (changed) importer.SaveAndReimport();
            }
            foreach (string path in fbxPaths)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                bool animOnly = name.Contains("walk") || name.Contains("idle") || name.Contains("run");
                if (!animOnly) { characterPath = path; break; }
            }
            if (characterPath == null && fbxPaths.Count > 0) characterPath = fbxPaths[0];
            if (characterPath == null)
            {
                Debug.LogError("[Afterrust] No FBX found in " + RiggedFolder);
                return;
            }

            AnimationClip walk = FindClip(fbxPaths, "walk");
            AnimationClip idle = FindClip(fbxPaths, "idle");
            if (walk == null)
            {
                Debug.LogError("[Afterrust] No walking clip found (file or clip name must contain 'walk').");
                return;
            }

            GameObject characterAsset = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
            ModelImporter characterImporter = (ModelImporter)AssetImporter.GetAtPath(characterPath);
            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(characterPath);
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
                Debug.LogWarning("[Afterrust] Humanoid avatar is not valid. Open the FBX > Rig > Configure and fix bone mapping.");

            // 2. Find patrol root in the scene.
            ScavengerPatrol patrol = Object.FindFirstObjectByType<ScavengerPatrol>(FindObjectsInactive.Include);
            Transform root = patrol != null ? patrol.transform : FindInScene("Scavenger Display Alignment");
            if (root == null)
            {
                Debug.LogError("[Afterrust] Scavenger root not found. Run Afterrust > Create or Rebuild Prototype Scene first.");
                return;
            }
            float patrolSpeed = patrol != null ? patrol.moveSpeed : 1.2f;

            // 3. Locomotion controller: Speed 0 = idle, Speed = patrol speed -> walk.
            AnimatorController controller = BuildController(walk, idle, patrolSpeed);

            // 4. Hide static model, remember where it stood.
            Vector3 standPoint = new Vector3(root.position.x, 0f, root.position.z);
            Transform staticModel = FindChild(root, StaticModelName);
            Transform previous = FindChild(root, RiggedInstanceName);
            if (previous != null && previous.gameObject.activeInHierarchy && TryGetBounds(previous.gameObject, out Bounds prev))
                standPoint = new Vector3(prev.center.x, 0f, prev.center.z);
            else if (staticModel != null && staticModel.gameObject.activeInHierarchy &&
                     TryGetBounds(staticModel.gameObject, out Bounds old))
                standPoint = new Vector3(old.center.x, 0f, old.center.z);
            if (staticModel != null)
            {
                Undo.RecordObject(staticModel.gameObject, "Hide static scavenger");
                staticModel.gameObject.SetActive(false);
            }
            if (previous != null) Undo.DestroyObjectImmediate(previous.gameObject);

            // A walking NPC leaves the preview pedestal; stand on the floor instead.
            Transform platform = FindInScene("Character Preview Platform");
            if (platform != null)
            {
                Undo.RecordObject(platform.gameObject, "Hide preview platform");
                platform.gameObject.SetActive(false);
            }

            // 5. Instantiate rigged character, scale to 1.75 m, face root forward, feet on floor.
            GameObject rigged = (GameObject)PrefabUtility.InstantiatePrefab(characterAsset);
            Undo.RegisterCreatedObjectUndo(rigged, "Add rigged scavenger");
            rigged.name = RiggedInstanceName;
            rigged.transform.SetParent(root, false);
            rigged.transform.localPosition = Vector3.zero;
            rigged.transform.localRotation = Quaternion.identity;
            rigged.transform.localScale = Vector3.one;

            if (TryGetBounds(rigged, out Bounds b) && b.size.y > 0.001f)
                rigged.transform.localScale *= TargetHeight / b.size.y;

            Vector3 modelForward = EstimateForward(rigged.transform, characterImporter);
            Vector3 rootForward = Vector3.ProjectOnPlane(root.forward, Vector3.up);
            if (modelForward.sqrMagnitude > 0.001f && rootForward.sqrMagnitude > 0.001f)
                rigged.transform.rotation = Quaternion.FromToRotation(modelForward, rootForward.normalized) *
                                            rigged.transform.rotation;

            if (TryGetBounds(rigged, out b))
                rigged.transform.position += new Vector3(standPoint.x - b.center.x, 0f - b.min.y, standPoint.z - b.center.z);

            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Scavenger.mat");
            if (material != null)
            {
                foreach (Renderer r in rigged.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = material;
                    r.sharedMaterials = mats;
                }
            }

            // 6. Animator + patrol.
            Animator animator = rigged.GetComponent<Animator>();
            if (animator == null) animator = Undo.AddComponent<Animator>(rigged);
            if (avatar != null) animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (patrol == null) patrol = Undo.AddComponent<ScavengerPatrol>(root.gameObject);
            Undo.RecordObject(patrol, "Link patrol animator");
            patrol.animator = animator;

            Selection.activeGameObject = rigged;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Afterrust] Walking scavenger ready: " + characterPath + " | walk=" + walk.name +
                      " | idle=" + (idle != null ? idle.name : "(frozen walk frame)") + ". Save scene (Ctrl+S) and press Play.");
        }

        private static AnimationClip FindClip(List<string> paths, string keyword)
        {
            foreach (string path in paths)
            {
                bool fileMatches = System.IO.Path.GetFileName(path).ToLowerInvariant().Contains(keyword);
                foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    AnimationClip clip = asset as AnimationClip;
                    if (clip == null || clip.name.StartsWith("__preview__")) continue;
                    if (fileMatches || clip.name.ToLowerInvariant().Contains(keyword)) return clip;
                }
            }
            return null;
        }

        internal static AnimatorController BuildController(AnimationClip walk, AnimationClip idle, float walkSpeed)
        {
            return BuildController(walk, idle, walkSpeed, ControllerPath);
        }

        internal static AnimatorController BuildController(AnimationClip walk, AnimationClip idle, float walkSpeed, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(idle != null ? idle : walk, 0f);
            tree.AddChild(walk, Mathf.Max(0.1f, walkSpeed));
            if (idle == null)
            {
                // No idle clip: hold the first walk frame while standing.
                ChildMotion[] children = tree.children;
                children[0].timeScale = 0f;
                tree.children = children;
            }
            AssetDatabase.SaveAssets();
            return controller;
        }

        /// <summary>Character forward from the humanoid hip mapping (left->right hip, cross up).</summary>
        internal static Vector3 EstimateForward(Transform model, ModelImporter importer)
        {
            string leftName = null, rightName = null;
            foreach (HumanBone bone in importer.humanDescription.human)
            {
                if (bone.humanName == "LeftUpperLeg") leftName = bone.boneName;
                else if (bone.humanName == "RightUpperLeg") rightName = bone.boneName;
            }
            Transform left = leftName != null ? FindChild(model, leftName) : null;
            Transform right = rightName != null ? FindChild(model, rightName) : null;
            if (left == null || right == null) return Vector3.zero;
            Vector3 side = Vector3.ProjectOnPlane(right.position - left.position, Vector3.up);
            return Vector3.Cross(side, Vector3.up).normalized;
        }

        internal static bool TryGetBounds(GameObject go, out Bounds bounds)
        {
            bounds = new Bounds();
            bool found = false;
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (!found) { bounds = r.bounds; found = true; }
                else bounds.Encapsulate(r.bounds);
            }
            return found;
        }

        internal static Transform FindInScene(string name)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform t = FindChild(root.transform, name);
                if (t != null) return t;
            }
            return null;
        }

        internal static Transform FindChild(Transform node, string name)
        {
            if (node.name == name) return node;
            for (int i = 0; i < node.childCount; i++)
            {
                Transform result = FindChild(node.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
#endif
