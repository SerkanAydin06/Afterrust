#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Version 7: first reachable expedition south of the yard gate. Two enterable
    /// roadside structures, context-appropriate loot and a visible contamination risk.
    /// Rebuilding replaces only this root; existing shelter and world props are untouched.
    /// </summary>
    internal static class FirstExpeditionBuilder
    {
        private const string RootName = "First Expedition - Roadside Checkpoint";

        public static void Build()
        {
            Transform old = ScavengerWalkSetup.FindInScene(RootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "First expedition checkpoint");

            Material concrete = MaterialKit.Lit("M_Expedition_Concrete", "concrete_block", 2f, 0.07f, 0f);
            Material asphalt = MaterialKit.Lit("M_Expedition_Asphalt", "ground_dirt", 5f, 0.08f, 0f);
            Material rust = MaterialKit.Lit("M_Expedition_Rust", "corrugated_rust", 2f, 0.21f, 0.4f);
            Material wood = MaterialKit.Lit("M_Expedition_Wood", "wood_planks", 1f, 0.11f, 0f);
            Material dark = MaterialKit.Plain("M_Expedition_DarkMetal", new Color(0.18f, 0.19f, 0.18f), 0.2f, 0.55f);
            Material warning = MaterialKit.Plain("M_Expedition_Warning", new Color(0.72f, 0.46f, 0.08f), 0.18f, 0.25f);
            Material chemical = MaterialKit.Plain("M_Expedition_Chemical", new Color(0.22f, 0.32f, 0.095f), 0.48f, 0.04f);

            Transform approach = Group(root.transform, "Approach");
            // The existing fence has a broad opening at x=-4..4, z=-20.
            // Keep the middle unobstructed so the player can leave and return safely.
            Box(approach, "Rutted access road", asphalt, new Vector3(0f, 0.015f, -33f),
                new Vector3(6.5f, 0.03f, 27f));
            Box(approach, "Left warning barrier", warning, new Vector3(-5.4f, 0.55f, -25f),
                new Vector3(2.2f, 1.1f, 0.35f));
            Box(approach, "Right warning barrier", warning, new Vector3(5.4f, 0.55f, -25f),
                new Vector3(2.2f, 1.1f, 0.35f));
            Box(approach, "Roadside abandoned tire", dark, new Vector3(-3.8f, 0.24f, -38f),
                new Vector3(0.95f, 0.48f, 0.95f));

            // Left: open-front disaster relief kiosk; reach it from the fence via the road.
            Transform kiosk = Group(root.transform, "Ruined Relief Kiosk");
            Box(kiosk, "Kiosk floor", concrete, new Vector3(-11f, 0.075f, -33.5f),
                new Vector3(8.0f, 0.15f, 8f));
            Box(kiosk, "Back wall", concrete, new Vector3(-11f, 1.45f, -37.5f),
                new Vector3(8f, 2.9f, 0.22f));
            Box(kiosk, "West wall", concrete, new Vector3(-14.9f, 1.45f, -33.5f),
                new Vector3(0.22f, 2.9f, 7.8f));
            Box(kiosk, "East wall", concrete, new Vector3(-7.1f, 1.45f, -33.5f),
                new Vector3(0.22f, 2.9f, 7.8f));
            Box(kiosk, "North wall left", concrete, new Vector3(-13.65f, 1.45f, -29.5f),
                new Vector3(2.7f, 2.9f, 0.22f));
            Box(kiosk, "North wall right", concrete, new Vector3(-8.35f, 1.45f, -29.5f),
                new Vector3(2.7f, 2.9f, 0.22f));
            Box(kiosk, "Bent sheet roof", rust, new Vector3(-11f, 2.93f, -33.5f),
                new Vector3(8.35f, 0.16f, 8.25f));
            Box(kiosk, "Counter", wood, new Vector3(-13.65f, 0.76f, -35.3f),
                new Vector3(1.2f, 0.12f, 2.5f));
            Box(kiosk, "Shelf", dark, new Vector3(-8.1f, 0.8f, -36.4f),
                new Vector3(1.1f, 0.08f, 1.25f));
            Loot(kiosk, "Emergency medicine cabinet", rust,
                new Vector3(-13.5f, 1.1f, -35.3f), new Vector3(0.6f, 0.7f, 0.55f),
                "Yardım Dolabı", 1.7f,
                new LootEntry("bandage", 0.85f, 1, 2),
                new LootEntry("cloth", 1f, 1, 3),
                new LootEntry("water_clean", 0.35f, 1, 1));
            Loot(kiosk, "Sealed ration crate", wood,
                new Vector3(-9.4f, 0.53f, -35.6f), new Vector3(1f, 1f, 0.95f),
                "Acil Gıda Sandığı", 1.8f,
                new LootEntry("canned_food", 1f, 1, 2),
                new LootEntry("crackers", 0.75f, 1, 2),
                new LootEntry("water_clean", 0.65f, 1, 1));

            // Right: an open shipping container. There is no collider blocking its entrance.
            Transform depot = Group(root.transform, "Ransacked Cargo Container");
            Box(depot, "Container floor", rust, new Vector3(14f, 0.09f, -39f),
                new Vector3(7.5f, 0.18f, 8.5f));
            Box(depot, "West side", rust, new Vector3(10.33f, 1.35f, -39f),
                new Vector3(0.16f, 2.7f, 8.5f));
            Box(depot, "East side", rust, new Vector3(17.67f, 1.35f, -39f),
                new Vector3(0.16f, 2.7f, 8.5f));
            Box(depot, "Rear bulkhead", rust, new Vector3(14f, 1.35f, -43.17f),
                new Vector3(7.4f, 2.7f, 0.16f));
            Box(depot, "Top", rust, new Vector3(14f, 2.78f, -39f),
                new Vector3(7.55f, 0.16f, 8.65f));
            Box(depot, "Open left door", dark, new Vector3(9.3f, 1.22f, -34.7f),
                new Vector3(0.16f, 2.44f, 1.9f));
            Box(depot, "Open right door", dark, new Vector3(18.7f, 1.22f, -34.7f),
                new Vector3(0.16f, 2.44f, 1.9f));
            Loot(depot, "Repair parts chest", rust,
                new Vector3(12.3f, 0.56f, -40f), new Vector3(1.1f, 1.05f, 1.0f),
                "Parça Sandığı", 2f,
                new LootEntry("wire", 1f, 1, 3),
                new LootEntry("duct_tape", 0.7f, 1, 2),
                new LootEntry("scrap", 0.55f, 1, 1));
            Loot(depot, "Hidden provisions", wood,
                new Vector3(15.9f, 0.48f, -41.3f), new Vector3(1.1f, 0.88f, 0.95f),
                "Unutulmuş Erzak", 1.5f,
                new LootEntry("canned_food", 0.8f, 1, 2),
                new LootEntry("water_clean", 0.7f, 1, 2));

            // A visible chemical spill between the road and container. The high-value
            // salvage cache is dangerous to approach; it can also be safely bypassed.
            Transform spill = Group(root.transform, "Chemical Spill");
            GameObject stain = Box(spill, "Corrosive residue - keep away", chemical,
                new Vector3(6f, 0.032f, -40f), new Vector3(5.0f, 0.045f, 5.2f));
            ToxicExposure toxic = stain.AddComponent<ToxicExposure>();
            toxic.radius = 3.45f;
            toxic.damagePerSecond = 0.75f;
            for (int i = 0; i < 3; i++)
            {
                float z = -38.1f - i * 1.4f;
                Box(spill, "Leaking barrel " + i, warning, new Vector3(6.1f + i * 0.8f, 0.55f, z),
                    new Vector3(0.7f, 1.1f, 0.7f));
            }
            Loot(spill, "Dangerous salvage case", rust,
                new Vector3(5.1f, 0.6f, -41.3f), new Vector3(0.95f, 1.15f, 0.95f),
                "Tehlikeli Hurda Kutusu", 2.2f,
                new LootEntry("alternator", 0.55f, 1, 1),
                new LootEntry("battery-clamp", 0.8f, 1, 2),
                new LootEntry("charcoal", 0.65f, 1, 2));

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public static void EnableAudio()
        {
            AfterrustInteraction player = Object.FindFirstObjectByType<AfterrustInteraction>(FindObjectsInactive.Include);
            if (player == null)
            {
                throw new System.InvalidOperationException("Player not found; exploration audio was not installed.");
            }
            if (player.GetComponent<AfterrustAudio>() == null)
                player.gameObject.AddComponent<AfterrustAudio>();
            if (player.GetComponent<FirstExpeditionGuide>() == null)
                player.gameObject.AddComponent<FirstExpeditionGuide>();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        /// <summary>Temporarily focus the tutorial on the safe starting yard.</summary>
        public static void PauseExpedition()
        {
            FirstExpeditionGuide guide = Object.FindFirstObjectByType<FirstExpeditionGuide>(FindObjectsInactive.Include);
            if (guide != null)
            {
                guide.enabled = false;
                EditorUtility.SetDirty(guide);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        private static Transform Group(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Box(Transform parent, string name, Material material, Vector3 position, Vector3 size)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.isStatic = true;
            return go;
        }

        private static void Loot(Transform parent, string name, Material mat, Vector3 pos, Vector3 size,
            string displayName, float holdSeconds, params LootEntry[] entries)
        {
            GameObject go = Box(parent, name, mat, pos, size);
            go.isStatic = false;
            LootContainer loot = go.AddComponent<LootContainer>();
            loot.displayName = displayName;
            loot.searchSeconds = holdSeconds;
            loot.table = entries;
        }
    }
}
#endif
