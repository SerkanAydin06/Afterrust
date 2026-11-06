#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Yard environment: dirt ground, concrete forecourt in front of the repair shop and a
    /// corrugated perimeter fence with a gate behind the player spawn (0, 0, -9).
    /// Re-running replaces only the "Scrapyard Environment" root. Run by AfterrustSceneSync.
    /// </summary>
    public static class ScrapyardEnvironmentBuilder
    {
        private const string PrefabDir = "Assets/Arts/Environment/Prefabs";
        private const string SceneRootName = "Scrapyard Environment";
        private const float XMin = -24f, XMax = 24f, ZMin = -20f, ZMax = 32f, GateHalfWidth = 4f;

        public static void Build()
        {
            MaterialKit.EnsureFolder(PrefabDir);

            Material slab = MaterialKit.Lit("M_ConcreteSlab", "concrete_slab", 4f, 0.18f, 0f);
            Material sheet = MaterialKit.Lit("M_CorrugatedRust", "corrugated_rust", 2f, 0.3f, 0.35f);
            Material steel = MaterialKit.Lit("M_RustySteel", "rusty_steel", 2f, 0.25f, 0.5f);
            // Ground runs to the horizon (400 m) so the distant ruins stand on it; 5 m per texture tile.
            Material groundLarge = MaterialKit.Lit("M_GroundDirt_Large", "ground_dirt", 0f, 0.08f, 0f,
                null, new Vector2(80f, 80f));

            GameObject floorPrefab = SavePrefab("Floor_Concrete_4x4",
                MeshKit.Box("Floor_4x4", new Vector3(4f, 0.2f, 4f), new Vector3(0f, -0.1f, 0f)), slab,
                new Vector3(4f, 0.2f, 4f), new Vector3(0f, -0.1f, 0f));
            GameObject fencePrefab = SavePrefab("Fence_Corrugated_4x2_4",
                MeshKit.Corrugated("Corrugated_4x2_4", 4f, 2.4f, 52, 0.018f, 0.05f), sheet,
                new Vector3(4f, 2.4f, 0.08f), new Vector3(0f, 1.25f, 0f));
            GameObject postPrefab = SavePrefab("Fence_Post_Steel",
                MeshKit.Box("Post_0_12x2_7", new Vector3(0.12f, 2.7f, 0.12f), new Vector3(0f, 1.35f, 0f)), steel,
                new Vector3(0.12f, 2.7f, 0.12f), new Vector3(0f, 1.35f, 0f));

            Transform old = ScavengerWalkSetup.FindInScene(SceneRootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(SceneRootName);
            Undo.RegisterCreatedObjectUndo(root, "Build scrapyard environment");

            // Forecourt between the gate and the shop front (shop floor starts at z = 0).
            Transform pad = Group(root, "Concrete Forecourt");
            System.Random random = new System.Random(7);
            for (float x = -6f; x <= 6f; x += 4f)
            for (float z = -10f; z <= -2f; z += 4f)
                Place(floorPrefab, pad, new Vector3(x, 0.01f, z), 90f * random.Next(4));

            Transform fence = Group(root, "Perimeter Fence");
            for (float x = XMin + 2f; x < XMax; x += 4f)
            {
                if (Mathf.Abs(x) >= GateHalfWidth) Place(fencePrefab, fence, new Vector3(x, 0f, ZMin), 0f);
                Place(fencePrefab, fence, new Vector3(x, 0f, ZMax), 180f);
            }
            for (float z = ZMin + 2f; z < ZMax; z += 4f)
            {
                Place(fencePrefab, fence, new Vector3(XMin, 0f, z), 90f);
                Place(fencePrefab, fence, new Vector3(XMax, 0f, z), -90f);
            }
            for (float x = XMin; x <= XMax; x += 4f)
            {
                if (Mathf.Abs(x) >= GateHalfWidth - 0.01f) Place(postPrefab, fence, new Vector3(x, 0f, ZMin), 0f);
                Place(postPrefab, fence, new Vector3(x, 0f, ZMax), 0f);
            }
            for (float z = ZMin + 4f; z < ZMax; z += 4f)
            {
                Place(postPrefab, fence, new Vector3(XMin, 0f, z), 0f);
                Place(postPrefab, fence, new Vector3(XMax, 0f, z), 0f);
            }

            Transform ground = ScavengerWalkSetup.FindInScene("Industrial Ground");
            if (ground != null)
            {
                Undo.RecordObject(ground, "Ground size");
                ground.position = new Vector3(0f, -0.5f, 0f);
                ground.localScale = new Vector3(400f, 1f, 400f);
            }
            if (ground != null && ground.TryGetComponent(out Renderer r))
            {
                Undo.RecordObject(r, "Ground material");
                r.sharedMaterial = groundLarge;
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static GameObject SavePrefab(string name, Mesh mesh, Material material, Vector3 colliderSize,
            Vector3 colliderCenter)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.size = colliderSize;
            box.center = colliderCenter;
            go.isStatic = true;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/" + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static Transform Group(GameObject root, string name)
        {
            Transform g = new GameObject(name).transform;
            g.SetParent(root.transform, false);
            return g;
        }

        private static void Place(GameObject prefab, Transform parent, Vector3 position, float yaw)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        }
    }
}
#endif
