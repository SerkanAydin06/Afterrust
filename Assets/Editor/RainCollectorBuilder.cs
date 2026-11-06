#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Replaces the three outdoor oil drums with one upright, open-top rain barrel.
    /// Existing sheltered props / survivor state are not rebuilt during migration.
    /// </summary>
    internal static class RainCollectorBuilder
    {
        private const string Name = "Rain Barrel - Open Collector";

        public static void Apply()
        {
            Transform shelter = ScavengerWalkSetup.FindInScene("Shelter");
            Transform yard = shelter != null ? shelter.Find("Yard Props") : null;
            if (yard == null)
                throw new System.InvalidOperationException("Shelter / Yard Props not found.");

            // Only remove the three old outdoor drums. The stove body and unrelated
            // barrels elsewhere in the world are NOT touched.
            for (int i = yard.childCount - 1; i >= 0; i--)
            {
                Transform child = yard.GetChild(i);
                if (child.name == "Oil Drum" || child.name == "Rain Barrel")
                    Undo.DestroyObjectImmediate(child.gameObject);
            }

            Place(yard);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        /// <summary>Also called during a full rebuild to create the final v10 model.</summary>
        public static void Place(Transform yard)
        {
            if (yard.Find(Name) != null) return;

            Material steel = MaterialKit.Lit("M_RainCollector_Rust", "corrugated_rust", 1.2f, 0.24f, 0.35f);
            Material rim = MaterialKit.Plain("M_RainCollector_Rim", new Color(0.27f, 0.29f, 0.29f), 0.30f, 0.60f);
            Material water = MaterialKit.Plain("M_RainCollector_Water", new Color(0.17f, 0.30f, 0.32f), 0.88f, 0.12f);
            Material gauge = MaterialKit.Plain("M_RainCollector_Gauge", new Color(0.24f, 0.48f, 0.49f), 0.72f, 0.05f);
            Material backing = MaterialKit.Plain("M_RainCollector_GaugeBacking", new Color(0.045f, 0.065f, 0.07f), 0.18f, 0.05f);

            GameObject root = new GameObject(Name);
            root.transform.SetParent(yard, false);
            root.transform.localPosition = new Vector3(3.3f, 0f, -1.4f);
            root.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(root, "Install open rain collector");

            WaterSource source = root.AddComponent<WaterSource>();
            source.displayName = "Açık Yağmur Varili";
            source.maxUnits = 3;
            // Fifteen real minutes of actual rain for a full barrel.
            source.secondsToFill = 900f;

            // A hollow mesh with NO lid. Static non-convex collider conforms to
            // the barrel wall so the roof-aware rain particles can fall into it.
            Mesh openMesh = MeshKit.OpenBarrel("RainCollector_OpenBarrel", 0.36f, 0.035f, 0.96f, 40);
            GameObject body = MeshObject(root.transform, "Open Steel Barrel Body", openMesh, steel);
            MeshCollider collider = body.AddComponent<MeshCollider>();
            collider.sharedMesh = openMesh;
            collider.convex = false;
            body.isStatic = true;

            // Slightly raised ring and two hoops look like a usable scrap-metal drum.
            Mesh ring = MeshKit.Torus("RainCollector_RimAndHoop", 0.36f, 0.028f, 0.036f, 40, 8, 0.45f);
            foreach (float y in new[] { 0.07f, 0.50f, 0.953f })
            {
                GameObject hoop = MeshObject(root.transform, "Steel Reinforcing Ring", ring, rim);
                hoop.transform.localPosition = new Vector3(0f, y, 0f);
            }

            // The blue-grey surface starts invisible at the bottom and steadily rises
            // INSIDE the barrel as the rainwater capacity increases.
            Mesh waterDisc = MeshKit.Cylinder("RainCollector_WaterDisc", 0.308f, 0.009f, 40);
            GameObject surface = MeshObject(root.transform, "Visible Water Surface", waterDisc, water);
            source.waterSurface = surface.transform;

            // A subtle external sight strip reinforces the level when viewed sideways.
            Box(root.transform, "Water Level Backplate", backing,
                new Vector3(0.348f, 0.49f, 0f), new Vector3(0.027f, 0.80f, 0.065f));
            GameObject indicator = Box(root.transform, "Water Level Indicator", gauge,
                new Vector3(0.353f, 0.10f, 0f), new Vector3(0.025f, 0.002f, 0.04f));
            source.levelGauge = indicator.transform;
            surface.GetComponent<MeshRenderer>().enabled = false;
            indicator.GetComponent<MeshRenderer>().enabled = false;

            // No free initial water: the visual objects are initialized empty.
        }

        private static GameObject MeshObject(Transform parent, string name, Mesh mesh, Material material)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        private static GameObject Box(Transform parent, string name, Material mat, Vector3 pos, Vector3 size)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            return go;
        }
    }
}
#endif
