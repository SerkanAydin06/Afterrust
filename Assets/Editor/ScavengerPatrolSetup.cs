#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    public static class ScavengerPatrolSetup
    {
        [MenuItem("Afterrust/Enable Scavenger Patrol (Keep Scene Layout)")]
        public static void EnablePatrol()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Afterrust] Stop Play mode before adding the patrol component.");
                return;
            }

            Transform characterRoot = null;
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Transform candidate = FindByName(root.transform, "Scavenger Display Alignment");
                if (candidate != null) { characterRoot = candidate; break; }
            }

            if (characterRoot == null)
            {
                // Reuse the artist-corrected parent when possible, without moving the model.
                foreach (GameObject root in roots)
                {
                    Transform candidate = FindByName(root.transform, "Meshy Scavenger - Imported Model");
                    if (candidate == null) continue;
                    characterRoot = candidate.parent != null &&
                        candidate.parent.name != "Scavenger Display Alignment"
                        ? candidate.parent : candidate;
                    break;
                }
            }

            if (characterRoot == null)
            {
                Debug.LogError("[Afterrust] No Scavenger model found in current scene.");
                return;
            }

            ScavengerPatrol patrol = characterRoot.GetComponent<ScavengerPatrol>();
            if (patrol == null) patrol = Undo.AddComponent<ScavengerPatrol>(characterRoot.gameObject);

            Selection.activeGameObject = characterRoot.gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Afterrust] Patrol enabled on " + characterRoot.name +
                ". Scene position, rotation, scale and model settings preserved. Save scene (Ctrl+S).");
        }

        private static Transform FindByName(Transform node, string name)
        {
            if (node.name == name) return node;
            for (int i = 0; i < node.childCount; i++)
            {
                Transform result = FindByName(node.GetChild(i), name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
#endif
