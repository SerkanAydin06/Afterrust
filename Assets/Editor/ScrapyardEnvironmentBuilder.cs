#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Builds the modular scrapyard environment from the procedural textures in
    /// Assets/Arts/Environment/Textures: materials, meter-UV meshes, prefabs and a layout
    /// (concrete work pad, corrugated perimeter fence with gate, workshop walls).
    /// Existing characters, car and gameplay objects are left untouched; re-running
    /// replaces only the "Scrapyard Environment" root.
    /// </summary>
    public static class ScrapyardEnvironmentBuilder
    {
        private const string Root = "Assets/Arts/Environment";
        private const string TexDir = Root + "/Textures";
        private const string MatDir = Root + "/Materials";
        private const string MeshDir = Root + "/Meshes";
        private const string PrefabDir = Root + "/Prefabs";
        private const string SceneRootName = "Scrapyard Environment";

        // Yard bounds (fence line) and gate opening behind the player spawn (0, 0, -9).
        private const float XMin = -24f, XMax = 24f, ZMin = -20f, ZMax = 32f, GateHalfWidth = 4f;

        [MenuItem("Afterrust/Build Scrapyard Environment")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Afterrust] Stop Play mode first.");
                return;
            }
            EnsureFolder(Root, "Materials");
            EnsureFolder(Root, "Meshes");
            EnsureFolder(Root, "Prefabs");
            ConfigureNormalMaps();

            // Meter-based UVs: tiling = 1 / texture world size.
            Material slab = CreateMaterial("M_ConcreteSlab", "concrete_slab", 4f, 0.18f, 0f);
            Material wall = CreateMaterial("M_ConcreteWall", "concrete_wall", 4f, 0.12f, 0f);
            Material sheet = CreateMaterial("M_CorrugatedRust", "corrugated_rust", 2f, 0.3f, 0.35f);
            Material steel = CreateMaterial("M_RustySteel", "rusty_steel", 2f, 0.25f, 0.5f);
            // Default-UV primitives already in the scene (0..1 per face).
            Material groundLarge = CreateMaterial("M_GroundDirt_Large", "ground_dirt", 0f, 0.08f, 0f, new Vector2(13f, 13f));
            Material ruinBlock = CreateMaterial("M_ConcreteBlock", "concrete_wall", 0f, 0.12f, 0f, new Vector2(1.25f, 0.75f));

            GameObject floorPrefab = SavePrefab("Floor_Concrete_4x4",
                SaveMesh("Floor_4x4", Box(new Vector3(4f, 0.2f, 4f), new Vector3(0f, -0.1f, 0f))), slab,
                new Vector3(4f, 0.2f, 4f), new Vector3(0f, -0.1f, 0f));
            GameObject wallPrefab = SavePrefab("Wall_Concrete_4x3",
                SaveMesh("Wall_4x3", Box(new Vector3(4f, 3f, 0.3f), new Vector3(0f, 1.5f, 0f))), wall,
                new Vector3(4f, 3f, 0.3f), new Vector3(0f, 1.5f, 0f));
            GameObject pillarPrefab = SavePrefab("Pillar_Concrete",
                SaveMesh("Pillar_0_5x3_2", Box(new Vector3(0.5f, 3.2f, 0.5f), new Vector3(0f, 1.6f, 0f))), wall,
                new Vector3(0.5f, 3.2f, 0.5f), new Vector3(0f, 1.6f, 0f));
            GameObject fencePrefab = SavePrefab("Fence_Corrugated_4x2_4",
                SaveMesh("Corrugated_4x2_4", Corrugated(4f, 2.4f, 52, 0.018f, 0.05f)), sheet,
                new Vector3(4f, 2.4f, 0.08f), new Vector3(0f, 1.25f, 0f));
            GameObject postPrefab = SavePrefab("Fence_Post_Steel",
                SaveMesh("Post_0_12x2_7", Box(new Vector3(0.12f, 2.7f, 0.12f), new Vector3(0f, 1.35f, 0f))), steel,
                new Vector3(0.12f, 2.7f, 0.12f), new Vector3(0f, 1.35f, 0f));

            Transform old = ScavengerWalkSetup.FindInScene(SceneRootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(SceneRootName);
            Undo.RegisterCreatedObjectUndo(root, "Build scrapyard environment");

            // Concrete work pad under the car, trader and player spawn; random 90-degree
            // rotations hide the repeat of the stain pattern.
            Transform pad = Group(root, "Concrete Work Pad");
            System.Random random = new System.Random(7);
            for (float x = -6f; x <= 6f; x += 4f)
            for (float z = -10f; z <= 2f; z += 4f)
                Place(floorPrefab, pad, new Vector3(x, 0.01f, z), 90f * random.Next(4));

            // Perimeter fence with gate gap on the south side.
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
                if (Mathf.Abs(x) >= GateHalfWidth - 0.01f)
                    Place(postPrefab, fence, new Vector3(x, 0f, ZMin), 0f);
                Place(postPrefab, fence, new Vector3(x, 0f, ZMax), 0f);
            }
            for (float z = ZMin + 4f; z < ZMax; z += 4f)
            {
                Place(postPrefab, fence, new Vector3(XMin, 0f, z), 0f);
                Place(postPrefab, fence, new Vector3(XMax, 0f, z), 0f);
            }

            // Workshop corner behind the trade post (5, 0, 0).
            Transform workshop = Group(root, "Workshop Walls");
            Place(wallPrefab, workshop, new Vector3(9f, 0f, -2f), 90f);
            Place(wallPrefab, workshop, new Vector3(9f, 0f, 2f), 90f);
            Place(wallPrefab, workshop, new Vector3(7f, 0f, 6f), 0f);
            Place(wallPrefab, workshop, new Vector3(3f, 0f, 6f), 0f);
            Place(pillarPrefab, workshop, new Vector3(9f, 0f, 6f), 0f);
            Place(pillarPrefab, workshop, new Vector3(9f, 0f, -4f), 0f);
            Place(pillarPrefab, workshop, new Vector3(1f, 0f, 6f), 0f);

            // Re-skin existing ground and ruin blocks (materials only, transforms untouched).
            foreach (GameObject sceneRoot in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Renderer r = sceneRoot.GetComponent<Renderer>();
                if (r == null) continue;
                if (sceneRoot.name == "Industrial Ground") SetMaterial(r, groundLarge);
                else if (sceneRoot.name.StartsWith("Abandoned Industrial Block")) SetMaterial(r, ruinBlock);
            }

            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Afterrust] Scrapyard environment built (" + root.GetComponentsInChildren<MeshRenderer>().Length +
                      " pieces). Save scene (Ctrl+S) and press Play.");
        }

        // ---------- materials ----------

        private static void ConfigureNormalMaps()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TexDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("_normal")) continue;
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType == TextureImporterType.NormalMap) continue;
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }

        private static Material CreateMaterial(string name, string texture, float worldSize, float smoothness,
            float metallic, Vector2? tiling = null)
        {
            string path = MatDir + "/" + name + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            else if (m.shader != shader) m.shader = shader;

            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/" + texture + "_albedo.jpg");
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/" + texture + "_normal.jpg");
            Vector2 scale = tiling ?? Vector2.one / worldSize;
            foreach (string prop in new[] { "_MainTex", "_BaseMap" })
            {
                if (!m.HasProperty(prop)) continue;
                m.SetTexture(prop, albedo);
                m.SetTextureScale(prop, scale);
            }
            if (normal != null && m.HasProperty("_BumpMap"))
            {
                m.SetTexture("_BumpMap", normal);
                m.EnableKeyword("_NORMALMAP");
            }
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void SetMaterial(Renderer r, Material m)
        {
            Undo.RecordObject(r, "Apply scrapyard material");
            r.sharedMaterial = m;
        }

        // ---------- meshes (UVs in meters, starting at 0 on each face) ----------

        private static Mesh Box(Vector3 size, Vector3 center)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> t = new List<int>();
            Vector3 h = size * 0.5f;

            void Face(Vector3 normal, Vector3 u, Vector3 w, float uLen, float wLen)
            {
                Vector3 a = center + Vector3.Scale(normal, h) - u * (uLen * 0.5f) - w * (wLen * 0.5f);
                int i = v.Count;
                v.Add(a); v.Add(a + u * uLen); v.Add(a + u * uLen + w * wLen); v.Add(a + w * wLen);
                uv.Add(new Vector2(0f, 0f)); uv.Add(new Vector2(uLen, 0f));
                uv.Add(new Vector2(uLen, wLen)); uv.Add(new Vector2(0f, wLen));
                for (int k = 0; k < 4; k++) n.Add(normal);
                AddQuad(t, i, Vector3.Dot(Vector3.Cross(u, w), normal) > 0f);
            }

            Face(Vector3.right, Vector3.forward, Vector3.up, size.z, size.y);
            Face(Vector3.left, Vector3.back, Vector3.up, size.z, size.y);
            Face(Vector3.forward, Vector3.left, Vector3.up, size.x, size.y);
            Face(Vector3.back, Vector3.right, Vector3.up, size.x, size.y);
            Face(Vector3.up, Vector3.right, Vector3.forward, size.x, size.z);
            Face(Vector3.down, Vector3.right, Vector3.back, size.x, size.z);
            return Finish(v, n, uv, t);
        }

        /// <summary>Double-sided corrugated sheet: width along X, height along Y, waves in Z.</summary>
        private static Mesh Corrugated(float width, float height, int waves, float amplitude, float bottom)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> t = new List<int>();
            int cols = waves * 6 + 1;
            for (int side = 0; side < 2; side++)
            {
                float s = side == 0 ? 1f : -1f;
                int start = v.Count;
                for (int c = 0; c < cols; c++)
                {
                    float u = c / (float)(cols - 1);
                    float x = (u - 0.5f) * width;
                    float phase = u * waves * Mathf.PI * 2f;
                    float z = Mathf.Sin(phase) * amplitude;
                    float dz = Mathf.Cos(phase) * amplitude * waves * Mathf.PI * 2f / width;
                    Vector3 normal = new Vector3(-dz, 0f, 1f).normalized * s;
                    for (int r = 0; r < 2; r++)
                    {
                        float y = bottom + r * height;
                        v.Add(new Vector3(x, y, z));
                        n.Add(normal);
                        uv.Add(new Vector2(u * width, r * height));
                    }
                }
                for (int c = 0; c < cols - 1; c++)
                {
                    int i = start + c * 2;
                    // vertices: i (bottom), i+1 (top), i+2 (next bottom), i+3 (next top)
                    if (side == 0) { t.Add(i); t.Add(i + 2); t.Add(i + 3); t.Add(i); t.Add(i + 3); t.Add(i + 1); }
                    else { t.Add(i); t.Add(i + 3); t.Add(i + 2); t.Add(i); t.Add(i + 1); t.Add(i + 3); }
                }
            }
            return Finish(v, n, uv, t);
        }

        private static void AddQuad(List<int> t, int i, bool uCrossWAlongNormal)
        {
            // Unity front face: Cross(B - A, C - A) points along the face normal.
            if (uCrossWAlongNormal) { t.Add(i); t.Add(i + 1); t.Add(i + 2); t.Add(i); t.Add(i + 2); t.Add(i + 3); }
            else { t.Add(i); t.Add(i + 2); t.Add(i + 1); t.Add(i); t.Add(i + 3); t.Add(i + 2); }
        }

        private static Mesh Finish(List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> t)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(v);
            mesh.SetNormals(n);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(t, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static Mesh SaveMesh(string name, Mesh mesh)
        {
            string path = MeshDir + "/" + name + ".asset";
            mesh.name = name;
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, existing);
            existing.name = name;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        // ---------- prefabs & placement ----------

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

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
