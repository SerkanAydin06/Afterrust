#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>Shared scene placement for Humanoid walking NPCs (scale, facing, feet on floor, patrol).</summary>
    internal static class WalkingNpcSetup
    {
        internal static GameObject AddWalker(string modelPath, ModelImporter importer, Avatar avatar,
            RuntimeAnimatorController controller, string rootName, Vector3 spot, Vector3[] waypoints,
            float walkSpeed, float targetHeight)
        {
            Transform old = ScavengerWalkSetup.FindInScene(rootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            GameObject root = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(root, "Add " + rootName);
            root.transform.position = spot;

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            if (ScavengerWalkSetup.TryGetBounds(model, out Bounds b) && b.size.y > 0.0001f)
                model.transform.localScale *= targetHeight / b.size.y;

            Vector3 forward = ScavengerWalkSetup.EstimateForward(model.transform, importer);
            if (forward.sqrMagnitude > 0.001f)
                model.transform.rotation = Quaternion.FromToRotation(forward, root.transform.forward) *
                                           model.transform.rotation;

            if (ScavengerWalkSetup.TryGetBounds(model, out b))
                model.transform.position += new Vector3(spot.x - b.center.x, -b.min.y, spot.z - b.center.z);

            Animator animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            if (avatar != null) animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            ScavengerPatrol patrol = root.AddComponent<ScavengerPatrol>();
            patrol.moveSpeed = walkSpeed;
            patrol.relativeWaypoints = (Vector3[])waypoints.Clone();
            patrol.animator = animator;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return root;
        }

        /// <summary>
        /// Replaces the FBX-embedded materials (often imported transparent or untextured from
        /// Mixamo) with explicit Standard materials. pick(original material name, renderer name).
        /// </summary>
        internal static void ApplyMaterials(GameObject model, System.Func<string, string, Material> pick)
        {
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
            {
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material m = pick(mats[i] != null ? mats[i].name : "", r.name);
                    if (m != null) mats[i] = m;
                }
                r.sharedMaterials = mats;
            }
        }
    }
}
#endif
