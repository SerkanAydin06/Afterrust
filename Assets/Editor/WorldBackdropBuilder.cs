#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Scene update v6 visuals: distant ruined city around the yard (combined meshes, fog
    /// layers them into silhouettes), power poles with wires, a water tower; shelter lamps
    /// cast shadows (no light through walls); the out-of-place NPCs are removed and the
    /// scavenger becomes the trader standing behind the trade stall.
    /// </summary>
    internal static class WorldBackdropBuilder
    {
        private const string RootName = "Distant Ruins";

        private struct Ruin
        {
            public float x, z, yaw, w, d, h, topW, topD, topH, topX, topZ, cutW, cutH;
            public Ruin(float x, float z, float yaw, float w, float d, float h, float topW, float topD, float topH,
                float topX, float topZ, float cutW, float cutH)
            {
                this.x = x; this.z = z; this.yaw = yaw; this.w = w; this.d = d; this.h = h;
                this.topW = topW; this.topD = topD; this.topH = topH; this.topX = topX; this.topZ = topZ;
                this.cutW = cutW; this.cutH = cutH;
            }
        }

        // Generated once (tools: ruins_layout), ring 48-88 m around the yard centre (0, 6).
        private static readonly Ruin[] Ruins =
        {
            new Ruin(68.9f,6.0f,100.6f,8f,12f,22f,4f,9f,2f,-1.5f,-0.5f,3f,11f),
            new Ruin(52.1f,24.0f,76.0f,16f,8f,18f,11f,3f,6f,0.1f,-1.1f,6f,9f),
            new Ruin(53.3f,50.6f,47.7f,8f,12f,12f,5f,7f,4f,-0.3f,1.9f,2f,7f),
            new Ruin(40.8f,64.0f,36.2f,14f,10f,26f,5f,4f,6f,0.7f,3.0f,5f,14f),
            new Ruin(24.8f,84.3f,18.0f,14f,8f,18f,7f,4f,2f,2.5f,-1.0f,4f,9f),
            new Ruin(2.5f,66.3f,2.8f,14f,12f,30f,6f,7f,2f,-2.2f,0.2f,4f,11f),
            new Ruin(-22.9f,77.0f,-25.6f,16f,8f,30f,6f,6f,4f,2.8f,-0.7f,6f,17f),
            new Ruin(-48.6f,70.0f,-28.0f,14f,8f,26f,7f,5f,4f,-1.5f,-1.3f,6f,15f),
            new Ruin(-64.1f,58.6f,-44.4f,8f,8f,12f,4f,4f,4f,0.3f,1.1f,3f,7f),
            new Ruin(-52.3f,28.9f,-63.7f,12f,12f,22f,4f,8f,2f,-3.9f,-0.4f,5f,13f),
            new Ruin(-79.8f,10.6f,-78.5f,10f,10f,14f,4f,7f,6f,2.9f,0.8f,4f,5f),
            new Ruin(-58.6f,-9.3f,-104.7f,12f,12f,10f,6f,8f,6f,2.3f,-0.6f,5f,5f),
            new Ruin(-71.4f,-45.4f,-122.2f,16f,12f,14f,10f,5f,2f,2.5f,-0.2f,5f,5f),
            new Ruin(-54.8f,-57.4f,-127.7f,10f,12f,30f,5f,8f,4f,0.2f,0.2f,3f,14f),
            new Ruin(-38.0f,-69.4f,-148.3f,10f,10f,18f,6f,7f,4f,-0.1f,-1.0f,3f,6f),
            new Ruin(-7.8f,-50.0f,-175.2f,16f,12f,12f,8f,6f,4f,-0.7f,1.0f,7f,4f),
            new Ruin(11.6f,-62.8f,-192.1f,14f,10f,18f,6f,6f,6f,0.9f,0.4f,5f,6f),
            new Ruin(28.4f,-57.4f,-213.7f,16f,10f,14f,7f,5f,6f,2.4f,-2.5f,6f,8f),
            new Ruin(44.5f,-52.6f,-206.3f,12f,12f,22f,6f,8f,2f,-2.8f,1.8f,4f,10f),
            new Ruin(64.8f,-45.5f,-230.8f,10f,8f,18f,6f,5f,2f,-0.8f,-1.4f,3f,9f),
            new Ruin(61.2f,-12.8f,-243.7f,10f,12f,18f,4f,9f,2f,-2.6f,0.4f,4f,10f),
            new Ruin(75.0f,2.6f,-258.7f,8f,8f,14f,5f,5f,6f,-1.1f,-0.2f,2f,6f)
        };

        public static void Build()
        {
            Transform old = ScavengerWalkSetup.FindInScene(RootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Distant ruins");

            Material facade = MaterialKit.Lit("M_RuinFacade", "ruin_facade", 0f, 0.05f, 0f, null, new Vector2(1f / 8f, 1f / 6f));
            Material steel = MaterialKit.Lit("M_RustySteel", "rusty_steel", 2f, 0.25f, 0.5f);
            Material wood = MaterialKit.Lit("M_WoodPlanks", "wood_planks", 1f, 0.12f, 0f);
            Material cable = MaterialKit.Plain("M_Cable", new Color(0.04f, 0.04f, 0.04f), 0.35f, 0f);

            // Buildings: main block (one corner collapsed lower) + a broken chunk on the roof.
            List<CombineInstance> blocks = new List<CombineInstance>();
            foreach (Ruin r in Ruins)
            {
                Matrix4x4 m = Matrix4x4.TRS(new Vector3(r.x, 0f, r.z), Quaternion.Euler(0f, r.yaw, 0f), Vector3.one);
                float mainW = r.w - r.cutW;
                Add(blocks, m, new Vector3(mainW, r.h, r.d), new Vector3(-r.cutW * 0.5f, 0f, 0f));
                Add(blocks, m, new Vector3(r.cutW, r.cutH, r.d), new Vector3(mainW * 0.5f, 0f, 0f));
                float topX = Mathf.Clamp(r.topX - r.cutW * 0.5f, -mainW * 0.5f + r.topW * 0.5f, mainW * 0.5f - r.topW * 0.5f);
                Add(blocks, m, new Vector3(Mathf.Min(r.topW, mainW), r.topH, r.topD), new Vector3(topX, r.h, r.topZ));
            }
            Combined(root.transform, "Ruined Buildings", "DistantRuins", blocks, facade, true);

            // Power poles: one line down the road from the gate, one behind the north fence.
            List<CombineInstance> poles = new List<CombineInstance>();
            List<CombineInstance> wires = new List<CombineInstance>();
            System.Random rnd = new System.Random(77);
            List<Vector3> road = new List<Vector3>();
            for (float z = -30f; z >= -120f; z -= 28f) road.Add(new Vector3(7f, 0f, z));
            List<Vector3> north = new List<Vector3>();
            for (float x = -96f; x <= 96f; x += 32f) north.Add(new Vector3(x, 0f, 44f));
            foreach (List<Vector3> line in new[] { road, north })
            {
                Vector3? prevL = null, prevR = null;
                foreach (Vector3 p in line)
                {
                    Quaternion lean = Quaternion.Euler((float)(rnd.NextDouble() - 0.5) * 10f, 0f, (float)(rnd.NextDouble() - 0.5) * 10f);
                    Vector3 along = line == road ? Vector3.forward : Vector3.right;
                    Quaternion facing = Quaternion.LookRotation(along);   // cross-arm (local X) across the line
                    Matrix4x4 m = Matrix4x4.TRS(p, lean * facing, Vector3.one);
                    Add(poles, m, new Vector3(0.28f, 9f, 0.28f), Vector3.zero);
                    Add(poles, m, new Vector3(2.4f, 0.2f, 0.2f), new Vector3(0f, 8.3f, 0f));
                    Vector3 l = m.MultiplyPoint3x4(new Vector3(-1.05f, 8.5f, 0f));
                    Vector3 rr = m.MultiplyPoint3x4(new Vector3(1.05f, 8.5f, 0f));
                    if (prevL.HasValue) { Wire(wires, prevL.Value, l); Wire(wires, prevR.Value, rr); }
                    prevL = l; prevR = rr;
                }
            }
            Combined(root.transform, "Power Poles", "PowerPoles", poles, wood, false);
            Combined(root.transform, "Power Lines", "PowerLines", wires, cable, false);

            // Water tower on four legs, north-west.
            List<CombineInstance> tower = new List<CombineInstance>();
            Matrix4x4 t = Matrix4x4.TRS(new Vector3(-62f, 0f, 50f), Quaternion.Euler(0f, 20f, 0f), Vector3.one);
            foreach (float x in new[] { -2.6f, 2.6f })
            foreach (float z in new[] { -2.6f, 2.6f })
                Add(tower, t, new Vector3(0.4f, 14f, 0.4f), new Vector3(x, 0f, z));
            Add(tower, t, new Vector3(5.6f, 0.25f, 0.25f), new Vector3(0f, 7f, -2.6f));
            Add(tower, t, new Vector3(5.6f, 0.25f, 0.25f), new Vector3(0f, 7f, 2.6f));
            tower.Add(new CombineInstance
            {
                mesh = MeshKit.Cylinder(null, 4f, 5f, 28),
                transform = t * Matrix4x4.Translate(new Vector3(0f, 14f, 0f))
            });
            Combined(root.transform, "Water Tower", "WaterTower", tower, steel, true);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void Add(List<CombineInstance> list, Matrix4x4 parent, Vector3 size, Vector3 bottomCentre)
        {
            list.Add(new CombineInstance
            {
                mesh = MeshKit.Box(null, size, new Vector3(0f, size.y * 0.5f, 0f)),
                transform = parent * Matrix4x4.Translate(bottomCentre)
            });
        }

        private static void Wire(List<CombineInstance> list, Vector3 a, Vector3 b)
        {
            Vector3 dir = b - a;
            Matrix4x4 m = Matrix4x4.TRS((a + b) * 0.5f, Quaternion.LookRotation(dir.normalized), Vector3.one);
            list.Add(new CombineInstance { mesh = MeshKit.Box(null, new Vector3(0.04f, 0.04f, dir.magnitude), Vector3.zero), transform = m });
        }

        private static void Combined(Transform parent, string objectName, string meshName, List<CombineInstance> parts,
            Material mat, bool collider)
        {
            Mesh mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
            mesh.CombineMeshes(parts.ToArray(), true, true);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            foreach (CombineInstance c in parts) Object.DestroyImmediate(c.mesh);
            mesh = MeshKit.SaveAsset(meshName, mesh);

            GameObject go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;   // far beyond the shadow distance anyway
            if (collider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            go.isStatic = true;
        }

        // ------------------------------------------------------------------ lights and NPCs

        /// <summary>Lamps on/in the shelter cast shadows so light no longer leaks through walls.</summary>
        public static void FixLampLeaks()
        {
            Transform lamps = ScavengerWalkSetup.FindInScene("Scrapyard Lamps");
            if (lamps == null) return;
            foreach (Light l in lamps.GetComponentsInChildren<Light>(true))
            {
                if (l.type != LightType.Point || l.transform.position.z < -10f) continue;   // gate lamps stay cheap
                Undo.RecordObject(l, "Lamp shadows");
                if (l.shadows == LightShadows.None) l.shadows = LightShadows.Hard;
                l.range = Mathf.Min(l.range, 6f);
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        /// <summary>Removes the two out-of-place walkers; the scavenger stands behind the trade stall.</summary>
        public static void ScavengerBecomesTrader()
        {
            foreach (string name in new[] { "Character1 (Mixamo Ch21)", "Neutral Figure (Meshy + Mixamo)" })
            {
                Transform t = ScavengerWalkSetup.FindInScene(name);
                if (t != null) Undo.DestroyObjectImmediate(t.gameObject);
            }

            Transform rigged = ScavengerWalkSetup.FindInScene("Meshy Scavenger - Rigged");
            if (rigged == null) return;
            ScavengerPatrol patrol = rigged.GetComponentInParent<ScavengerPatrol>();
            Transform root = patrol != null ? patrol.transform : rigged.parent;
            if (root == null) return;
            Undo.RecordObject(root, "Scavenger trader");
            if (patrol != null)
            {
                Undo.RecordObject(patrol, "Scavenger trader");
                patrol.relativeWaypoints = new[] { Vector3.zero };   // single point = stand idle
            }

            // Face the forecourt (-z), then stand behind the counter at (10.5, 0, -12.6).
            root.rotation = Quaternion.Euler(0f, 180f, 0f);
            if (ScavengerWalkSetup.TryGetBounds(rigged.gameObject, out Bounds b))
            {
                Vector3 target = new Vector3(10.5f, 0f, -11.75f);
                root.position += new Vector3(target.x - b.center.x, 0f, target.z - b.center.z);
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
#endif
