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

            // Keep the south opening protected even when the yard is rebuilt manually.
            EnsureSafeZoneGate();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Adds a closed, interactive 8 m double gate at the existing south opening.
        /// Existing fence objects and their sibling indices are never rebuilt here.
        /// Safe to call when migrating an already-built Unity scene.
        /// </summary>
        public static void EnsureSafeZoneGate()
        {
            Transform yard = ScavengerWalkSetup.FindInScene(SceneRootName);
            if (yard == null)
                throw new System.InvalidOperationException("Cannot add yard gate: Scrapyard Environment was not found.");

            Transform existing = yard.Find("South Safe Zone Gate");
            if (existing != null)
            {
                // Respect the current gate and saved states, if this is run twice.
                SafeZoneGate gate = existing.GetComponent<SafeZoneGate>();
                if (gate == null)
                    throw new System.InvalidOperationException("Existing south gate has no SafeZoneGate component.");
                return;
            }

            Material gateSheet = MaterialKit.Lit("M_SafeGate_CorrugatedRust", "corrugated_rust", 2f, 0.22f, 0.35f);
            Material frame = MaterialKit.Lit("M_SafeGate_SteelFrame", "rusty_steel", 2f, 0.24f, 0.55f);
            Material caution = MaterialKit.Plain("M_SafeGate_Caution", new Color(0.85f, 0.63f, 0.23f), 0.25f, 0.2f);

            GameObject root = new GameObject("South Safe Zone Gate");
            root.transform.SetParent(yard, false);
            root.transform.localPosition = new Vector3(0f, 0f, ZMin);
            Undo.RegisterCreatedObjectUndo(root, "Add double safe zone gate");
            SafeZoneGate gateComponent = root.AddComponent<SafeZoneGate>();
            gateComponent.openAngle = 100f;
            gateComponent.degreesPerSecond = 115f;
            gateComponent.isOpen = false;

            Transform left = Group(root, "Left Hinge");
            Transform right = Group(root, "Right Hinge");
            left.localPosition = new Vector3(-GateHalfWidth, 0f, 0f);
            right.localPosition = new Vector3(GateHalfWidth, 0f, 0f);
            gateComponent.leftHinge = left;
            gateComponent.rightHinge = right;

            // Leaves fill the 8 m clear space, with a small clearance at the centre.
            // Both rotate INTO the yard (+z); existing perimeter posts act as jambs.
            AddGateLeaf(left, "Left Gate Leaf", gateSheet, frame, caution, true);
            AddGateLeaf(right, "Right Gate Leaf", gateSheet, frame, caution, false);

            // Crossbar is flush with the tops of the existing 2.7 m gateposts.
            GateBox(root.transform, "Gate Lintel", frame,
                new Vector3(0f, 2.59f, 0f), new Vector3(8.06f, 0.18f, 0.18f), true);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Remove only the decorative sign board and lettering from existing gates.
        /// The gate root, hinges, colliders, lintel and saved open state stay intact.
        /// Safe to run repeatedly after a scene refresh.
        /// </summary>
        public static void RemoveSafeZoneSign()
        {
            Transform yard = ScavengerWalkSetup.FindInScene(SceneRootName);
            Transform gate = yard != null ? yard.Find("South Safe Zone Gate") : null;
            if (gate == null)
                throw new System.InvalidOperationException("South safe-zone gate missing; cannot clean up sign.");

            bool removed = false;
            foreach (string name in new[] { "Safe Zone Name", "Entrance Name Plate" })
            {
                Transform child = gate.Find(name);
                if (child == null) continue;
                Undo.DestroyObjectImmediate(child.gameObject);
                removed = true;
            }

            if (removed) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void AddGateLeaf(Transform hinge, string name, Material sheet,
            Material frame, Material caution, bool left)
        {
            float direction = left ? 1f : -1f;
            Transform panel = Group(hinge.gameObject, name);
            panel.localPosition = new Vector3(direction * 1.97f, 0.04f, 0f);
            panel.gameObject.AddComponent<MeshFilter>().sharedMesh =
                MeshKit.Corrugated("Corrugated_SafeGate_3_94x2_34", 3.94f, 2.34f, 48, 0.018f, 0f);
            panel.gameObject.AddComponent<MeshRenderer>().sharedMaterial = sheet;
            BoxCollider collider = panel.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 1.17f, 0f);
            collider.size = new Vector3(3.94f, 2.34f, 0.14f);

            // A welded perimeter, middle rail and diagonal stiffener, slightly ahead
            // of the corrugation, make both doors read as structural metal gate leaves.
            foreach (float x in new[] { -1.86f, 1.86f })
                GateBox(panel, "Vertical frame", frame,
                    new Vector3(x, 1.17f, 0.07f), new Vector3(0.12f, 2.32f, 0.12f), false);
            foreach (float y in new[] { 0.085f, 1.17f, 2.255f })
                GateBox(panel, "Horizontal frame", frame,
                    new Vector3(0f, y, 0.07f), new Vector3(3.81f, 0.12f, 0.12f), false);
            GameObject brace = GateBox(panel, "Diagonal brace", frame,
                new Vector3(0f, 1.17f, 0.13f), new Vector3(4.15f, 0.12f, 0.10f), false);
            brace.transform.localEulerAngles = new Vector3(0f, 0f, left ? 31f : -31f);
            GateBox(panel, "Central latch", caution,
                new Vector3(direction * 1.66f, 1.14f, 0.19f), new Vector3(0.17f, 0.34f, 0.10f), false);
        }

        private static GameObject GateBox(Transform parent, string name, Material material,
            Vector3 position, Vector3 scale, bool fixedPart)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            // The leaf's single collider handles picking/physics; decorative frame
            // colliders must not intercept interaction rays.
            if (!fixedPart) Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            go.isStatic = fixedPart;
            return go;
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
