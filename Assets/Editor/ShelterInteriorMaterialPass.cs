#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// V15.1 safe-shelter PBR material pass. Reuses existing albedo + normal maps,
    /// creates a small set of shared interior-only materials, and never changes
    /// geometry, colliders, positions, scripts or save-identifying hierarchy paths.
    /// Exterior shared materials remain untouched.
    /// </summary>
    internal static class ShelterInteriorMaterialPass
    {
        private struct Palette
        {
            public Material floor, wallWarm, wallCool, wallShadow;
            public Material wood, wornWood, iron, pegboard, cardboard;
            public Material mattress, blanket, stove;
        }

        public static void Apply()
        {
            Transform shelter = ScavengerWalkSetup.FindInScene("Shelter");
            if (shelter == null)
                throw new System.InvalidOperationException("Cannot retouch shelter materials: Shelter root missing.");

            Transform shell = Require(shelter, "Shell");
            Transform front = Require(shelter, "Front Wall And Door");
            Transform workbench = Require(shelter, "Workbench");
            Transform shelving = Require(shelter, "Shelving");
            Transform living = Require(shelter, "Living Area");
            Transform storage = Require(shelter, "Personal Storage");

            Palette m = CreatePalette();
            int changed = 0;
            changed += ApplyShell(shell, m);
            changed += ApplyFront(front, m);
            changed += ApplyWorkbench(workbench, m);
            changed += ApplyShelving(shelving, m);
            changed += ApplyLivingArea(living, m);
            changed += ApplyStorage(storage, m);

            // Persist generated material assets so scene references survive restart.
            AssetDatabase.SaveAssets();
            if (changed > 0) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Afterrust] v15 interior materials: " + changed +
                      " surfaces refreshed. No geometry or colliders rebuilt.");
        }

        private static Transform Require(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found == null)
                throw new System.InvalidOperationException("Shelter missing " + name +
                    "; refusing incomplete material migration.");
            return found;
        }

        private static Palette CreatePalette()
        {
            // New names are intentional: never mutate the shared M_WoodPlanks /
            // M_RustySteel / M_ConcreteSlab used across the yard and expedition.
            return new Palette
            {
                floor = MaterialKit.Lit("M_V15_Interior_DustySlab", "concrete_slab",
                    2.1f, 0.11f, 0.03f, new Color(0.82f, 0.80f, 0.76f)),
                wallWarm = MaterialKit.Lit("M_V15_Interior_WarmCinder", "concrete_block",
                    1.6f, 0.07f, 0f, new Color(0.91f, 0.85f, 0.76f)),
                wallCool = MaterialKit.Lit("M_V15_Interior_ColdCinder", "concrete_block",
                    1.6f, 0.07f, 0f, new Color(0.77f, 0.81f, 0.82f)),
                wallShadow = MaterialKit.Lit("M_V15_Interior_AgedBlock", "concrete_wall",
                    1.6f, 0.06f, 0f, new Color(0.78f, 0.77f, 0.73f)),
                wood = MaterialKit.Lit("M_V15_Interior_WornWorkWood", "wood_planks",
                    0.9f, 0.13f, 0f, new Color(0.87f, 0.77f, 0.64f)),
                wornWood = MaterialKit.Lit("M_V15_Interior_DarkReclaimedWood", "wood_planks",
                    0.8f, 0.09f, 0f, new Color(0.68f, 0.64f, 0.56f)),
                iron = MaterialKit.Lit("M_V15_Interior_MatteOxidizedIron", "rusty_steel",
                    0.85f, 0.12f, 0.42f, new Color(0.72f, 0.74f, 0.75f)),
                pegboard = MaterialKit.Lit("M_V15_Interior_DustedPegboard", "pegboard",
                    0.85f, 0.07f, 0.04f, new Color(0.78f, 0.74f, 0.67f)),
                cardboard = MaterialKit.Plain("M_V15_Interior_FadedCardboard",
                    new Color(0.44f, 0.37f, 0.28f), 0.05f, 0f),
                mattress = MaterialKit.Plain("M_V15_Interior_WeatheredCanvas",
                    new Color(0.48f, 0.45f, 0.39f), 0.04f, 0f),
                blanket = MaterialKit.Plain("M_V15_Interior_OliveBlanket",
                    new Color(0.27f, 0.34f, 0.28f), 0.04f, 0f),
                stove = MaterialKit.Lit("M_V15_Interior_BlackenedStove", "cast_metal",
                    0.95f, 0.19f, 0.58f, new Color(0.48f, 0.48f, 0.48f))
            };
        }

        private static int ApplyShell(Transform group, Palette m)
        {
            int count = 0;
            foreach (MeshRenderer renderer in group.GetComponentsInChildren<MeshRenderer>(true))
            {
                string name = renderer.gameObject.name;
                Material target = null;
                if (name == "Floor") target = m.floor;
                else if (name == "Back Wall" || name.StartsWith("Broken Edge")) target = m.wallShadow;
                else if (name.StartsWith("Left Wall")) target = m.wallWarm;
                else if (name.StartsWith("Right Wall")) target = m.wallCool;
                else if (name.StartsWith("Front Pier") || name == "Lintel") target = m.wallWarm;
                else if (name.StartsWith("Window Board")) target = m.wornWood;
                count += Assign(renderer, target);
            }
            return count;
        }

        private static int ApplyFront(Transform group, Palette m)
        {
            int count = 0;
            foreach (MeshRenderer renderer in group.GetComponentsInChildren<MeshRenderer>(true))
            {
                string name = renderer.gameObject.name;
                Material target = null;
                if (name.StartsWith("Infill")) target = m.wallWarm;
                else if (name.StartsWith("Door Post") || name == "Door Header" ||
                         name == "Door Leaf") target = m.wornWood;
                else if (name.StartsWith("Strap") || name == "Brace" ||
                         name.StartsWith("Handle")) target = m.iron;
                count += Assign(renderer, target);
            }
            return count;
        }

        private static int ApplyWorkbench(Transform group, Palette m)
        {
            int count = 0;
            foreach (MeshRenderer renderer in group.GetComponentsInChildren<MeshRenderer>(true))
            {
                string name = renderer.gameObject.name;
                Material target = null;
                if (name.StartsWith("Bench")) target = m.wood;
                else if (name == "Pegboard") target = m.pegboard;
                else if (name.StartsWith("Box Under Bench")) target = m.cardboard;
                else if (name == "Leg" || name.StartsWith("Vise") ||
                         name.StartsWith("Wrench") || name.StartsWith("Hammer Head") ||
                         name == "Saw") target = m.iron;
                count += Assign(renderer, target);
            }
            return count;
        }

        private static int ApplyShelving(Transform group, Palette m)
        {
            int count = 0;
            foreach (MeshRenderer renderer in group.GetComponentsInChildren<MeshRenderer>(true))
            {
                string name = renderer.gameObject.name;
                Material target = name == "Shelf" || name == "Upright" ? m.iron :
                    name == "Box" ? m.cardboard : null;
                count += Assign(renderer, target);
            }
            return count;
        }

        private static int ApplyLivingArea(Transform group, Palette m)
        {
            int count = 0;
            foreach (MeshRenderer renderer in group.GetComponentsInChildren<MeshRenderer>(true))
            {
                string name = renderer.gameObject.name;
                Material target = null;
                if (name.StartsWith("Pallet")) target = m.wornWood;
                else if (name == "Mattress" || name == "Pillow") target = m.mattress;
                else if (name == "Blanket") target = m.blanket;
                else if (name == "Table Top" || name == "Table Leg" || name == "Seat" ||
                         name == "Back" || name == "Chair Leg") target = m.wood;
                else if (name == "Stove Body" || name == "Stove Door" ||
                         name == "Flue") target = m.stove;
                count += Assign(renderer, target);
            }
            return count;
        }

        private static int ApplyStorage(Transform group, Palette m)
        {
            int count = 0;
            foreach (MeshRenderer renderer in group.GetComponentsInChildren<MeshRenderer>(true))
            {
                string name = renderer.gameObject.name;
                Material target = name == "Wooden chest body" || name == "Lid"
                    ? m.wornWood
                    : name.StartsWith("Steel") || name == "Front lock plate"
                        ? m.iron
                        : null;
                count += Assign(renderer, target);
            }
            return count;
        }

        private static int Assign(MeshRenderer renderer, Material material)
        {
            if (renderer == null || material == null || renderer.sharedMaterial == material) return 0;
            Undo.RecordObject(renderer, "Apply v15 interior material");
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
            return 1;
        }
    }
}
#endif
