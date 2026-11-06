#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Adds one usable storage chest against the right interior wall, clear of
    /// the front entrance, workbench, table, stove and bed. Never rebuilds the
    /// shelter or moves existing objects; existing storage contents are preserved.
    /// </summary>
    internal static class ShelterStorageBuilder
    {
        private const string RootName = "Shelter";
        private const string ChestName = "Personal Storage";

        public static void EnsureStorage()
        {
            Transform shelter = ScavengerWalkSetup.FindInScene(RootName);
            if (shelter == null)
                throw new System.InvalidOperationException("Cannot add personal storage: shelter root missing.");

            Transform existing = shelter.Find(ChestName);
            if (existing != null)
            {
                if (existing.GetComponent<ShelterStorage>() == null)
                    throw new System.InvalidOperationException("Personal Storage exists without ShelterStorage component.");
                return; // important: do not erase stored items during scene upgrades
            }

            Material timber = MaterialKit.Lit("M_StorageChest_WornTimber", "wood_planks",
                1.3f, 0.15f, 0.0f);
            Material iron = MaterialKit.Lit("M_StorageChest_RustedIron", "rusty_steel",
                1.7f, 0.20f, 0.55f);
            Material dark = MaterialKit.Plain("M_StorageChest_DarkHinge",
                new Color(0.13f, 0.12f, 0.11f), 0.24f, 0.33f);

            Transform chest = new GameObject(ChestName).transform;
            chest.SetParent(shelter, false);
            chest.localPosition = new Vector3(2.95f, 0f, 1.45f);
            Undo.RegisterCreatedObjectUndo(chest.gameObject, "Install shelter personal storage");

            ShelterStorage storage = chest.gameObject.AddComponent<ShelterStorage>();
            storage.capacityKg = 120f;
            storage.displayName = "Kişisel Depo";

            // Low metal-footed wooden crate with a clearly defined reinforced lid.
            // Keep every decorative piece collider-free: one box on the root owns
            // physical contact and the interaction raycast.
            Box(chest, "Wooden chest body", timber,
                new Vector3(0f, 0.39f, 0f), new Vector3(1.34f, 0.72f, 0.72f));
            Box(chest, "Lid", timber,
                new Vector3(0f, 0.79f, 0f), new Vector3(1.40f, 0.13f, 0.79f));
            foreach (float x in new[] { -0.55f, 0.55f })
            {
                Box(chest, "Steel corner brace", iron,
                    new Vector3(x, 0.42f, -0.38f), new Vector3(0.09f, 0.80f, 0.06f));
                Box(chest, "Steel lid band", iron,
                    new Vector3(x, 0.865f, 0f), new Vector3(0.09f, 0.025f, 0.82f));
                Box(chest, "Foot", dark,
                    new Vector3(x, 0.06f, 0f), new Vector3(0.15f, 0.12f, 0.65f));
            }
            Box(chest, "Front lock plate", iron,
                new Vector3(0f, 0.73f, -0.407f), new Vector3(0.23f, 0.20f, 0.04f));
            Box(chest, "Keyhole", dark,
                new Vector3(0f, 0.73f, -0.436f), new Vector3(0.035f, 0.075f, 0.012f));

            BoxCollider collider = chest.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.44f, 0f);
            collider.size = new Vector3(1.4f, 0.88f, 0.80f);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void Box(Transform parent, string name, Material material,
            Vector3 pos, Vector3 size)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = pos;
            part.transform.localScale = size;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
            Object.DestroyImmediate(part.GetComponent<BoxCollider>());
        }
    }
}
#endif
