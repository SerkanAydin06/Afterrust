#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Builds the survivor's shelter: a small run-down cinder-block building (8.5 x 6 m) whose
    /// old garage opening is bricked up around a hinged door (E opens/closes). Inside: bed,
    /// table, stove, workbench and shelves. Outside: scrap corner with the salvageable engine,
    /// trade stall by the forecourt, tires, drums, pallets, containers. Gameplay objects keep
    /// their components/data, only visuals change. Re-running replaces the "Shelter" root
    /// (and removes the legacy "Repair Shop" root).
    /// </summary>
    public static class ShelterBuilder
    {
        private const string RootName = "Shelter";
        private const string LegacyRootName = "Repair Shop";
        private const float RoofFront = 3.15f, RoofDrop = 0.1f, Depth = 6f;

        private static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();
        private static Material block, slab, steel, sheet, wood, rubber, peg, cast;
        private static Material paintGreen, paintRed, paintBlue, paintYellow, containerRed, containerBlue, containerGreen;
        private static Material plastic, terminalRed, lead, cable, cardboard, paper;

        public static void Build()
        {
            Cache.Clear();
            CreateMaterials();
            RemoveLegacyPrimitives();

            foreach (string name in new[] { RootName, LegacyRootName })
            {
                Transform old = ScavengerWalkSetup.FindInScene(name);
                if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            }
            Transform root = new GameObject(RootName).transform;
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Build shelter");

            BuildShell(Group(root, "Shell"));
            BuildRoof(Group(root, "Roof"));
            BuildFront(Group(root, "Front Wall And Door"));
            BuildWorkbench(Group(root, "Workbench"));
            BuildShelving(Group(root, "Shelving"));
            BuildEngineStand(Group(root, "Scrap Corner"));
            BuildLivingArea(Group(root, "Living Area"));
            BuildTradeStall(Group(root, "Trade Stall"));
            BuildExterior(Group(root, "Yard Props"));
            RestyleSalvageParts();
            RestyleTrader();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // ------------------------------------------------------------------ materials

        private static void CreateMaterials()
        {
            block = MaterialKit.Lit("M_ConcreteBlock_Painted", "concrete_block", 2f, 0.08f, 0f);
            slab = MaterialKit.Lit("M_ConcreteSlab", "concrete_slab", 4f, 0.18f, 0f);
            steel = MaterialKit.Lit("M_RustySteel", "rusty_steel", 2f, 0.25f, 0.5f);
            sheet = MaterialKit.Lit("M_CorrugatedRust", "corrugated_rust", 2f, 0.3f, 0.35f);
            wood = MaterialKit.Lit("M_WoodPlanks", "wood_planks", 1f, 0.12f, 0f);
            rubber = MaterialKit.Lit("M_TireRubber", "rubber_tire", 1f, 0.2f, 0f, null, Vector2.one);
            peg = MaterialKit.Lit("M_Pegboard", "pegboard", 1f, 0.1f, 0f);
            cast = MaterialKit.Lit("M_CastMetal", "cast_metal", 1f, 0.35f, 0.6f);
            paintGreen = MaterialKit.Lit("M_Paint_Green", "chipped_paint", 1f, 0.35f, 0.2f, new Color(0.42f, 0.52f, 0.45f));
            paintRed = MaterialKit.Lit("M_Paint_Red", "chipped_paint", 1f, 0.4f, 0.2f, new Color(0.72f, 0.16f, 0.12f));
            paintBlue = MaterialKit.Lit("M_Paint_Blue", "chipped_paint", 1f, 0.35f, 0.2f, new Color(0.2f, 0.32f, 0.52f));
            paintYellow = MaterialKit.Lit("M_Paint_Yellow", "chipped_paint", 1f, 0.35f, 0.2f, new Color(0.78f, 0.6f, 0.18f));
            containerRed = MaterialKit.Lit("M_Container_Red", "chipped_paint", 2f, 0.3f, 0.3f, new Color(0.5f, 0.2f, 0.15f));
            containerBlue = MaterialKit.Lit("M_Container_Blue", "chipped_paint", 2f, 0.3f, 0.3f, new Color(0.22f, 0.3f, 0.42f));
            containerGreen = MaterialKit.Lit("M_Container_Green", "chipped_paint", 2f, 0.3f, 0.3f, new Color(0.3f, 0.4f, 0.3f));
            plastic = MaterialKit.Plain("M_BatteryPlastic", new Color(0.07f, 0.07f, 0.075f), 0.45f, 0f);
            terminalRed = MaterialKit.Plain("M_TerminalRed", new Color(0.6f, 0.08f, 0.06f), 0.4f, 0f);
            lead = MaterialKit.Plain("M_Lead", new Color(0.55f, 0.55f, 0.56f), 0.55f, 0.8f);
            cable = MaterialKit.Plain("M_Cable", new Color(0.04f, 0.04f, 0.04f), 0.35f, 0f);
            cardboard = MaterialKit.Plain("M_Cardboard", new Color(0.55f, 0.42f, 0.28f), 0.05f, 0f);
            paper = MaterialKit.Plain("M_Paper", new Color(0.82f, 0.8f, 0.72f), 0.05f, 0f);
        }

        // ------------------------------------------------------------------ shell

        private static void BuildShell(Transform p)
        {
            // Floor starts at z = 0 so it never overlaps the forecourt tiles.
            Part(p, "Floor", BoxC(8.5f, 0.15f, 6.125f), slab, new Vector3(0f, -0.045f, 3.0625f));
            Part(p, "Back Wall", BoxB(8.5f, 3f, 0.25f), block, new Vector3(0f, 0f, 6f));

            // Left wall: back half intact, front half broken down with a ragged top.
            // Walls only touch, never overlap: coplanar overlapping faces z-fight (flicker).
            // Side walls run between the front (z <= 0.125) and back (z >= 5.875) walls.
            Part(p, "Left Wall Back", BoxB(0.25f, 3f, 2.875f), block, new Vector3(-4.125f, 0f, 4.4375f));
            Part(p, "Left Wall Front (broken)", BoxB(0.25f, 2.3f, 2.875f), block, new Vector3(-4.125f, 0f, 1.5625f));
            Part(p, "Broken Edge A", BoxB(0.25f, 0.45f, 0.6f), block, new Vector3(-4.125f, 2.3f, 2.7f));
            Part(p, "Broken Edge B", BoxB(0.25f, 0.2f, 0.4f), block, new Vector3(-4.125f, 2.3f, 2.2f));
            System.Random rnd = new System.Random(11);
            for (int i = 0; i < 6; i++)
            {
                Vector3 pos = new Vector3(-4.6f - (float)rnd.NextDouble() * 0.6f, 0.1f, 1.2f + (float)rnd.NextDouble() * 1.6f);
                Part(p, "Rubble " + i, BoxC(0.4f, 0.2f, 0.2f), block, pos,
                    new Vector3((float)rnd.NextDouble() * 20f, (float)rnd.NextDouble() * 180f, (float)rnd.NextDouble() * 15f));
            }

            // Right wall with a broken window boarded up from outside.
            Part(p, "Right Wall Low", BoxB(0.25f, 1f, 5.75f), block, new Vector3(4.125f, 0f, 3f));
            Part(p, "Right Wall High", BoxB(0.25f, 0.8f, 5.75f), block, new Vector3(4.125f, 2.2f, 3f));
            Part(p, "Right Wall Pier A", BoxB(0.25f, 1.2f, 1.875f), block, new Vector3(4.125f, 1f, 1.0625f));
            Part(p, "Right Wall Pier B", BoxB(0.25f, 1.2f, 1.875f), block, new Vector3(4.125f, 1f, 4.9375f));
            float[] boardY = { 1.3f, 1.65f, 2.0f };
            float[] boardTilt = { 9f, -6f, 4f };
            for (int i = 0; i < boardY.Length; i++)
                Part(p, "Window Board " + i, BoxC(0.03f, 0.16f, 2.2f), wood, new Vector3(4.275f, boardY[i], 3f),
                    new Vector3(boardTilt[i], 0f, 0f));

            // Front: two piers and a lintel around a 4.4 x 2.8 m door opening.
            Part(p, "Front Pier Left", BoxB(2.05f, 3f, 0.25f), block, new Vector3(-3.225f, 0f, 0f));
            Part(p, "Front Pier Right", BoxB(2.05f, 3f, 0.25f), block, new Vector3(3.225f, 0f, 0f));
            Part(p, "Lintel", BoxB(4.4f, 0.2f, 0.25f), block, new Vector3(0f, 2.8f, 0f));
        }

        private static float RoofTop(float z) { return RoofFront - RoofDrop * z / Depth; }

        private static void BuildRoof(Transform p)
        {
            float[] beamZ = { 0.15f, 3f, 5.85f };
            foreach (float z in beamZ)
                // 20 cm deep so every beam clearly sinks into the wall tops (no near-coplanar contact).
                Part(p, "Beam", BoxC(8.7f, 0.2f, 0.12f), steel, new Vector3(0f, RoofTop(z) - 0.1f, z));

            float slope = Mathf.Atan2(RoofDrop, Depth) * Mathf.Rad2Deg;
            Mesh roofSheet = MeshKit.Corrugated("Corrugated_Roof_4_3x2_4", 4.3f, 2.4f, 56, 0.018f, 0f);
            float[] rowStart = { 6.3f, 4.2f, 2.1f };
            float[] colX = { -2.15f, 2.15f };
            System.Random rnd = new System.Random(3);
            for (int r = 0; r < rowStart.Length; r++)
            for (int c = 0; c < colX.Length; c++)
            {
                float z = rowStart[r];
                Part(p, "Roof Sheet", roofSheet, sheet, new Vector3(colX[c], RoofTop(z) + 0.02f + r * 0.035f, z),
                    new Vector3(-90f + slope, (float)(rnd.NextDouble() - 0.5) * 2f, (float)(rnd.NextDouble() - 0.5) * 1.5f),
                    false);
            }
            // The visual corrugated sheets have no colliders. A continuous invisible
            // sloped roof blocks rain (and overhead raycasts) without z-fighting.
            AddRainBarrier(p, "Shelter Roof Weather Barrier",
                new Vector3(0f, RoofTop(3f) + 0.03f, 3f),
                new Vector3(Mathf.Atan2(RoofDrop, Depth) * Mathf.Rad2Deg, 0f, 0f),
                new Vector3(8.7f, 0.12f, 6.4f));
            // A spare sheet leans against the right wall outside.
            GameObject leaning = Part(p, "Fallen Roof Sheet", roofSheet, sheet, new Vector3(4.75f, 0f, 4.4f), Vector3.zero);
            leaning.transform.rotation = Quaternion.AngleAxis(12f, Vector3.forward) * Quaternion.Euler(0f, 90f, 0f);
        }

        private static void BuildFront(Transform p)
        {
            // Old 4.4 m garage opening (x -2.2..2.2, up to the lintel at 2.8) bricked up around a
            // 1.1 x 2.1 m door. Pieces only touch each other (no coplanar overlaps).
            Part(p, "Infill Left", BoxB(1.55f, 2.8f, 0.25f), block, new Vector3(-1.425f, 0f, 0f));
            Part(p, "Infill Right", BoxB(1.55f, 2.8f, 0.25f), block, new Vector3(1.425f, 0f, 0f));
            Part(p, "Infill Above Door", BoxB(1.3f, 0.5f, 0.25f), block, new Vector3(0f, 2.3f, 0f));
            Part(p, "Door Post L", BoxB(0.1f, 2.2f, 0.25f), wood, new Vector3(-0.6f, 0f, 0f));
            Part(p, "Door Post R", BoxB(0.1f, 2.2f, 0.25f), wood, new Vector3(0.6f, 0f, 0f));
            Part(p, "Door Header", BoxB(1.3f, 0.1f, 0.25f), wood, new Vector3(0f, 2.2f, 0f));

            // Hinged door: pivot on the left jamb, opens inwards (towards +z).
            Transform hinge = new GameObject("Shelter Door").transform;
            hinge.SetParent(p, false);
            hinge.localPosition = new Vector3(-0.55f, 0f, 0f);
            hinge.gameObject.AddComponent<Door>();
            GameObject leaf = Part(hinge, "Door Leaf", BoxC(1.09f, 2.09f, 0.05f), wood, new Vector3(0.55f, 1.05f, 0f));
            Part(leaf.transform, "Strap Top", BoxC(1.0f, 0.06f, 0.012f), steel, new Vector3(0f, 0.6f, -0.031f), Vector3.zero, false);
            Part(leaf.transform, "Strap Bottom", BoxC(1.0f, 0.06f, 0.012f), steel, new Vector3(0f, -0.55f, -0.031f), Vector3.zero, false);
            Part(leaf.transform, "Brace", BoxC(0.06f, 1.3f, 0.012f), steel, new Vector3(0f, 0.02f, -0.031f), new Vector3(0f, 0f, 38f), false);
            Part(leaf.transform, "Handle Out", BoxC(0.03f, 0.16f, 0.03f), steel, new Vector3(0.42f, 0f, -0.05f), Vector3.zero, false);
            Part(leaf.transform, "Handle In", BoxC(0.03f, 0.16f, 0.03f), steel, new Vector3(0.42f, 0f, 0.05f), Vector3.zero, false);
            foreach (Transform t in hinge.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = false;

            // Corrugated patch over the broken top of the left wall.
            Mesh roofSheet = MeshKit.Corrugated("Corrugated_Roof_4_3x2_4", 4.3f, 2.4f, 56, 0.018f, 0f);
            // 3 m wide (z 0.05..3.05), 1 m tall, 6 cm outside the wall face.
            GameObject patch = Part(p, "Wall Patch", roofSheet, sheet, new Vector3(-4.31f, 2.2f, 1.55f), new Vector3(0f, 90f, 0f), false);
            patch.transform.localScale = new Vector3(0.7f, 0.42f, 1f);
        }

        // ------------------------------------------------------------------ interior

        private static void BuildWorkbench(Transform p)
        {
            p.localPosition = new Vector3(-1.6f, 0f, 5.45f);
            Part(p, "Bench Top", BoxC(2.4f, 0.06f, 0.75f), wood, new Vector3(0f, 0.93f, 0f));
            Part(p, "Bench Shelf", BoxC(2.3f, 0.03f, 0.65f), wood, new Vector3(0f, 0.28f, 0f), Vector3.zero, false);
            foreach (float x in new[] { -1.15f, 1.15f })
            foreach (float z in new[] { -0.32f, 0.32f })
                Part(p, "Leg", BoxB(0.06f, 0.9f, 0.06f), steel, new Vector3(x, 0f, z), Vector3.zero, false);

            GameObject toolbox = Part(p, "Toolbox", BoxB(0.5f, 0.22f, 0.24f), paintRed, new Vector3(0.65f, 0.96f, 0.05f), new Vector3(0f, -8f, 0f));
            Part(toolbox.transform, "Handle", BoxC(0.3f, 0.03f, 0.03f), steel, new Vector3(0f, 0.26f, 0f), Vector3.zero, false);
            Part(p, "Vise Body", BoxB(0.2f, 0.12f, 0.2f), steel, new Vector3(-1.0f, 0.96f, -0.22f));
            Part(p, "Vise Jaw", BoxB(0.2f, 0.1f, 0.05f), steel, new Vector3(-1.0f, 1.08f, -0.32f), Vector3.zero, false);
            Part(p, "Piston A", Cyl(0.045f, 0.1f, 14), cast, new Vector3(-0.1f, 0.96f, -0.15f), Vector3.zero, false);
            Part(p, "Piston B", Cyl(0.045f, 0.1f, 14), cast, new Vector3(0.03f, 0.96f, -0.2f), Vector3.zero, false);
            Part(p, "Gear", Cyl(0.09f, 0.03f, 20), cast, new Vector3(-0.4f, 0.96f, 0.1f), Vector3.zero, false);
            Part(p, "Oil Can", Cyl(0.05f, 0.14f, 14), paintYellow, new Vector3(0.25f, 0.96f, -0.25f), Vector3.zero, false);
            Part(p, "Box Under Bench A", BoxB(0.5f, 0.3f, 0.4f), cardboard, new Vector3(-0.6f, 0.295f, 0f), new Vector3(0f, 6f, 0f), false);
            Part(p, "Box Under Bench B", BoxB(0.45f, 0.25f, 0.35f), cardboard, new Vector3(0.2f, 0.295f, 0.05f), new Vector3(0f, -4f, 0f), false);

            // Pegboard with tools on the back wall (inner face z = 5.875).
            // 1.5 cm off the wall; tools hang 2 mm in front of the board.
            Part(p, "Pegboard", BoxC(2.4f, 1.0f, 0.02f), peg, new Vector3(0f, 1.75f, 0.40f), Vector3.zero, false);
            for (int i = 0; i < 5; i++)
                Part(p, "Wrench " + i, BoxC(0.03f, 0.2f + i * 0.03f, 0.008f), steel,
                    new Vector3(-1.0f + i * 0.11f, 1.78f, 0.384f), new Vector3(0f, 0f, 4f - i * 2f), false);
            Part(p, "Hammer Handle", BoxC(0.03f, 0.3f, 0.03f), wood, new Vector3(-0.25f, 1.68f, 0.385f), Vector3.zero, false);
            Part(p, "Hammer Head", BoxC(0.12f, 0.04f, 0.04f), steel, new Vector3(-0.25f, 1.84f, 0.385f), Vector3.zero, false);
            Part(p, "Saw", BoxC(0.45f, 0.12f, 0.004f), steel, new Vector3(0.45f, 1.9f, 0.386f), new Vector3(0f, 0f, -3f), false);
            for (int i = 0; i < 4; i++)
                Part(p, "Screwdriver " + i, Cyl(0.012f, 0.1f, 10), i % 2 == 0 ? paintRed : paintYellow,
                    new Vector3(0.2f + i * 0.08f, 1.55f, 0.385f), Vector3.zero, false);
        }

        private static void BuildShelving(Transform p)
        {
            p.localPosition = new Vector3(-3.72f, 0f, 1.9f);
            foreach (float x in new[] { -0.23f, 0.23f })
            foreach (float z in new[] { -0.88f, 0.88f })
                Part(p, "Upright", BoxB(0.04f, 2f, 0.04f), steel, new Vector3(x, 0f, z), Vector3.zero, false);
            float[] levels = { 0.3f, 0.85f, 1.4f, 1.95f };
            foreach (float y in levels)
                Part(p, "Shelf", BoxC(0.5f, 0.03f, 1.8f), steel, new Vector3(0f, y, 0f));

            Material[] cans = { paintBlue, paintRed, paintYellow, paintGreen };
            System.Random rnd = new System.Random(5);
            for (int s = 0; s < levels.Length; s++)
            {
                float y = levels[s] + 0.015f;
                for (int i = 0; i < 4; i++)
                {
                    float z = -0.7f + i * 0.45f + (float)(rnd.NextDouble() - 0.5) * 0.1f;
                    if ((s + i) % 3 == 0)
                        Part(p, "Box", BoxB(0.35f, 0.25f, 0.3f), cardboard, new Vector3(0f, y, z),
                            new Vector3(0f, (float)rnd.NextDouble() * 10f, 0f), false);
                    else
                        Part(p, "Can", Cyl(0.07f, 0.18f, 14), cans[(s + i) % cans.Length],
                            new Vector3((float)(rnd.NextDouble() - 0.5) * 0.15f, y, z), Vector3.zero, false);
                }
            }
        }

        private static void BuildEngineStand(Transform p)
        {
            p.localPosition = new Vector3(6.6f, 0f, 1.2f);   // outside, right of the shelter
            Part(p, "Drip Tray", BoxB(0.7f, 0.02f, 0.5f), steel, new Vector3(0f, 0.03f, 0f), Vector3.zero, false);
            Part(p, "Stand Base X", BoxB(0.9f, 0.06f, 0.08f), paintRed, new Vector3(-0.2f, 0.03f, 0f), Vector3.zero, false);
            Part(p, "Stand Base Z", BoxB(0.08f, 0.06f, 0.9f), paintRed, new Vector3(-0.55f, 0.03f, 0f), Vector3.zero, false);
            Part(p, "Stand Post", BoxB(0.08f, 0.86f, 0.08f), paintRed, new Vector3(-0.55f, 0.09f, 0f));
            Part(p, "Stand Arm", BoxC(0.27f, 0.07f, 0.07f), paintRed, new Vector3(-0.43f, 0.95f, 0f), Vector3.zero, false);
            Part(p, "Stand Plate", BoxC(0.04f, 0.3f, 0.3f), steel, new Vector3(-0.32f, 0.95f, 0f), Vector3.zero, false);
            Part(p, "Engine Block", BoxC(0.6f, 0.42f, 0.45f), cast, new Vector3(0f, 0.95f, 0f));
            Part(p, "Cylinder Head", BoxC(0.62f, 0.12f, 0.38f), cast, new Vector3(0f, 1.22f, 0f), Vector3.zero, false);
            Part(p, "Valve Cover", BoxC(0.58f, 0.07f, 0.3f), paintRed, new Vector3(0f, 1.315f, 0f), Vector3.zero, false);
            Part(p, "Oil Pan", BoxC(0.5f, 0.12f, 0.35f), steel, new Vector3(0f, 0.68f, 0f), Vector3.zero, false);
            Part(p, "Crank Pulley", Cyl(0.09f, 0.04f, 20), cast, new Vector3(0.3f, 0.85f, 0.05f), new Vector3(0f, 0f, -90f), false);
            Part(p, "Exhaust Manifold", BoxC(0.45f, 0.07f, 0.08f), steel, new Vector3(0f, 1.05f, 0.27f), Vector3.zero, false);
            // Battery crate next to the engine (battery itself is a salvage part).
            Part(p, "Battery Crate", BoxB(0.55f, 0.45f, 0.45f), wood, new Vector3(1.1f, 0f, -0.6f), new Vector3(0f, 8f, 0f));
        }

        private static void BuildLivingArea(Transform p)
        {
            Material fabric = MaterialKit.Plain("M_Mattress", new Color(0.42f, 0.4f, 0.34f), 0.05f, 0f);
            Material blanket = MaterialKit.Plain("M_Blanket", new Color(0.22f, 0.26f, 0.22f), 0.05f, 0f);

            // Bed: two pallets + mattress along the left wall, back corner.
            Transform bed = Group(p, "Bed");
            bed.localPosition = new Vector3(-3.45f, 0f, 4.78f);
            bed.localEulerAngles = new Vector3(0f, 90f, 0f);
            for (int k = 0; k < 2; k++)
            {
                float y = 0.03f + k * 0.13f;
                foreach (float z in new[] { -0.4f, 0f, 0.4f })
                    Part(bed, "Pallet Stringer", BoxB(1.9f, 0.1f, 0.08f), wood, new Vector3(0f, y, z), Vector3.zero, false);
                for (int b = 0; b < 6; b++)
                    Part(bed, "Pallet Board", BoxB(0.14f, 0.02f, 0.88f), wood, new Vector3(-0.82f + b * 0.328f, y + 0.1f, 0f), Vector3.zero, false);
            }
            Part(bed, "Mattress", BoxB(1.92f, 0.16f, 0.9f), fabric, new Vector3(0f, 0.28f, 0f));
            Part(bed, "Blanket", BoxB(1.1f, 0.03f, 0.92f), blanket, new Vector3(0.36f, 0.44f, 0f), new Vector3(0f, 0f, -1f), false);
            Part(bed, "Pillow", BoxB(0.3f, 0.1f, 0.55f), fabric, new Vector3(-0.75f, 0.44f, 0f), Vector3.zero, false);

            // Table and chair where the engine used to stand.
            Transform table = Group(p, "Table");
            table.localPosition = new Vector3(1.6f, 0f, 3.6f);
            table.localEulerAngles = new Vector3(0f, -6f, 0f);
            Part(table, "Table Top", BoxB(1.1f, 0.04f, 0.7f), wood, new Vector3(0f, 0.72f, 0f));
            foreach (float x in new[] { -0.5f, 0.5f })
            foreach (float z in new[] { -0.3f, 0.3f })
                Part(table, "Table Leg", BoxB(0.05f, 0.72f, 0.05f), wood, new Vector3(x, 0f, z), Vector3.zero, false);
            Part(table, "Can", Cyl(0.05f, 0.11f, 14), paintYellow, new Vector3(0.25f, 0.76f, 0.1f), Vector3.zero, false);
            Part(table, "Map", BoxB(0.42f, 0.004f, 0.3f), paper, new Vector3(-0.2f, 0.76f, -0.05f), new Vector3(0f, 9f, 0f), false);
            Transform chair = Group(table, "Chair");
            chair.localPosition = new Vector3(0f, 0f, -0.7f);
            chair.localEulerAngles = new Vector3(0f, 12f, 0f);
            Part(chair, "Seat", BoxB(0.45f, 0.04f, 0.42f), wood, new Vector3(0f, 0.44f, 0f));
            Part(chair, "Back", BoxB(0.45f, 0.45f, 0.04f), wood, new Vector3(0f, 0.48f, -0.19f), Vector3.zero, false);
            foreach (float x in new[] { -0.19f, 0.19f })
            foreach (float z in new[] { -0.17f, 0.17f })
                Part(chair, "Chair Leg", BoxB(0.04f, 0.44f, 0.04f), wood, new Vector3(x, 0f, z), Vector3.zero, false);

            // Drum stove with a flue through the roof (fire comes later).
            Transform stove = Group(p, "Stove");
            stove.localPosition = new Vector3(3.4f, 0f, 5.1f);
            Part(stove, "Stove Body", Cyl(0.28f, 0.65f, 24), steel, new Vector3(0f, 0.03f, 0f));
            Part(stove, "Stove Door", BoxC(0.2f, 0.16f, 0.02f), cast, new Vector3(-0.2f, 0.36f, -0.2f), new Vector3(0f, 45f, 0f), false);
            Part(stove, "Flue", Cyl(0.06f, 2.5f, 14), steel, new Vector3(0f, 0.68f, 0f), Vector3.zero, false);
        }

        private static void BuildTradeStall(Transform p)
        {
            // Tarp-roofed barter stall by the forecourt; the trade counter is placed under it.
            p.localPosition = new Vector3(10.5f, 0f, -12.6f);
            foreach (float x in new[] { -1.9f, 1.9f })
            foreach (float z in new[] { -1.0f, 1.0f })
                Part(p, "Stall Post", BoxB(0.08f, 2.4f, 0.08f), steel, new Vector3(x, 0f, z));
            Mesh roofSheet = MeshKit.Corrugated("Corrugated_Roof_4_3x2_4", 4.3f, 2.4f, 56, 0.018f, 0f);
            // The old -85 degree pitch left the rear of the canopy ~20 cm above
            // the 2.4 m posts. A level roof touches both rows of supports.
            Part(p, "Stall Roof", roofSheet, sheet, new Vector3(0f, 2.42f, 1.2f),
                new Vector3(-90f, 0f, 0f), false);
            EnsureTradeStallSupports(p);
            AddRainBarrier(p, "Trade Stall Weather Barrier", new Vector3(0f, 2.42f, 0f),
                Vector3.zero, new Vector3(4.4f, 0.12f, 2.5f));
        }

        /// <summary>
        /// Fixes a visual and physical mismatch in previously generated trade stalls.
        /// Changes only the roof transform, weather collider and two supporting crossbeams;
        /// trader/NPC positions, shelter props and saved inventory remain unchanged.
        /// </summary>
        public static void AlignTradeStallRoof()
        {
            Transform shelter = ScavengerWalkSetup.FindInScene(RootName);
            Transform stall = shelter != null ? shelter.Find("Trade Stall") : null;
            if (stall == null)
                throw new System.InvalidOperationException("Trade Stall was not found under Shelter.");

            Transform roof = stall.Find("Stall Roof");
            if (roof == null)
                throw new System.InvalidOperationException("Stall Roof mesh is missing.");
            roof.localPosition = new Vector3(0f, 2.42f, 1.2f);
            roof.localEulerAngles = new Vector3(-90f, 0f, 0f);
            EnsureTradeStallSupports(stall);
            AddRainBarrier(stall, "Trade Stall Weather Barrier",
                new Vector3(0f, 2.42f, 0f), Vector3.zero,
                new Vector3(4.4f, 0.12f, 2.5f));
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void EnsureTradeStallSupports(Transform stall)
        {
            // Migration runs after script reload; static material fields may be unset.
            Material supportSteel = steel != null ? steel :
                MaterialKit.Lit("M_RustySteel", "rusty_steel", 2f, 0.25f, 0.5f);
            // Horizontal beams meet the four post caps at y=2.4 m. Keep
            // their names stable for repeated scene migration.
            foreach (float z in new[] { -1f, 1f })
            {
                string name = z < 0f ? "Rear Canopy Support" : "Front Canopy Support";
                if (stall.Find(name) != null) continue;
                Part(stall, name, BoxC(4.05f, 0.11f, 0.13f), supportSteel,
                    new Vector3(0f, 2.34f, z), Vector3.zero, false);
            }
        }

        /// <summary>
        /// Migrate previously generated v7 scenes without rebuilding the shelter, which
        /// would otherwise replace gameplay objects and invalidate parts of saved state.
        /// </summary>
        public static void EnsureRainColliders()
        {
            Transform shelter = ScavengerWalkSetup.FindInScene(RootName);
            if (shelter == null)
                throw new System.InvalidOperationException("Shelter root missing; cannot install roof rain blockers.");
            Transform roof = shelter.Find("Roof");
            Transform stall = shelter.Find("Trade Stall");
            if (roof == null || stall == null)
                throw new System.InvalidOperationException("Shelter roof/trade stall missing.");
            AddRainBarrier(roof, "Shelter Roof Weather Barrier",
                new Vector3(0f, RoofTop(3f) + 0.03f, 3f),
                new Vector3(Mathf.Atan2(RoofDrop, Depth) * Mathf.Rad2Deg, 0f, 0f),
                new Vector3(8.7f, 0.12f, 6.4f));
            AddRainBarrier(stall, "Trade Stall Weather Barrier",
                new Vector3(0f, 2.42f, 0f), Vector3.zero,
                new Vector3(4.4f, 0.12f, 2.5f));
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void AddRainBarrier(Transform parent, string name, Vector3 position,
            Vector3 euler, Vector3 size)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = euler;
            BoxCollider collider = go.GetComponent<BoxCollider>();
            if (collider == null) collider = go.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = size;
            collider.isTrigger = false;
            go.isStatic = true;
        }

        // ------------------------------------------------------------------ exterior

        private static void BuildExterior(Transform p)
        {
            Mesh tire = MeshKit.Torus("Tire_0_68", 0.235f, 0.2f, 0.21f, 36, 14, 0.55f);
            float[] yaw = { 0f, 23f, 51f, 12f };
            for (int i = 0; i < 4; i++)
                Part(p, "Tire Stack " + i, tire, rubber,
                    new Vector3(-3.3f + (i % 2) * 0.04f, 0.105f + i * 0.21f, -1.1f + (i % 3) * 0.03f), new Vector3(0f, yaw[i], 0f));
            Part(p, "Tire Leaning", tire, rubber, new Vector3(-3.7f, 0.34f, -0.35f), new Vector3(78f, 15f, 0f));
            Part(p, "Tire Inside", tire, rubber, new Vector3(3.75f, 0.34f, 1.2f), new Vector3(80f, -70f, 0f));

            // Single uncovered upright rain collector replaces the three discarded oil drums.
            RainCollectorBuilder.Place(p);

            Transform pallets = Group(p, "Pallet Stack");
            pallets.localPosition = new Vector3(-5.9f, 0f, 3.6f);
            pallets.localEulerAngles = new Vector3(0f, 8f, 0f);
            for (int k = 0; k < 3; k++)
            {
                float y = k * 0.13f;
                foreach (float x in new[] { -0.55f, 0f, 0.55f })
                    Part(pallets, "Stringer", BoxB(0.08f, 0.1f, 1.0f), wood, new Vector3(x, y, 0f), new Vector3(0f, k * 3f, 0f), false);
                for (int b = 0; b < 5; b++)
                    Part(pallets, "Board", BoxB(1.2f, 0.02f, 0.14f), wood, new Vector3(0f, y + 0.1f, -0.43f + b * 0.215f),
                        new Vector3(0f, k * 3f, 0f), false);
            }
            // Split logs stacked against the left wall outside (fuel for the stove).
            Transform woodpile = Group(p, "Woodpile");
            woodpile.localPosition = new Vector3(-4.75f, 0f, 0.6f);
            System.Random logs = new System.Random(19);
            for (int row = 0; row < 3; row++)
            for (int i = 0; i < 5 - row; i++)
            {
                float z = -0.42f + i * 0.21f + row * 0.105f;
                Part(woodpile, "Log", Cyl(0.1f, 0.9f, 10), wood,
                    new Vector3(0.45f, 0.1f + row * 0.18f, z),
                    new Vector3((float)(logs.NextDouble() - 0.5) * 4f, 0f, 90f), false);
            }
            BoxCollider woodCollider = woodpile.gameObject.AddComponent<BoxCollider>();
            woodCollider.center = new Vector3(0f, 0.3f, 0f);
            woodCollider.size = new Vector3(0.95f, 0.6f, 1.15f);

            BoxCollider palletCollider = pallets.gameObject.AddComponent<BoxCollider>();
            palletCollider.center = new Vector3(0f, 0.2f, 0f);
            palletCollider.size = new Vector3(1.25f, 0.4f, 1.05f);

            Container(p, containerRed, new Vector3(-13f, 0f, 6f), 90f);
            Container(p, containerBlue, new Vector3(-13f, 2.6f, 6.3f), 86f);
            Container(p, containerGreen, new Vector3(14f, 0f, 4f), 80f);
        }

        private static void Container(Transform p, Material paint, Vector3 pos, float yaw)
        {
            GameObject c = Part(p, "Shipping Container", BoxB(6.06f, 2.59f, 2.44f), paint, pos, new Vector3(0f, yaw, 0f));
            foreach (float x in new[] { -2.98f, 2.98f })
            foreach (float z in new[] { -1.17f, 1.17f })
                Part(c.transform, "Corner Post", BoxB(0.12f, 2.6f, 0.12f), steel, new Vector3(x, 0f, z), Vector3.zero, false);
            for (int i = 0; i < 5; i++)
                Part(c.transform, "Door Bar", Cyl(0.02f, 2.4f, 8), steel, new Vector3(3.05f, 0.1f, -0.9f + i * 0.45f), Vector3.zero, false);
        }

        // ------------------------------------------------------------------ gameplay objects

        private static void RestyleSalvageParts()
        {
            SalvageablePart clamp = Salvage("Battery Clamp", "battery-clamp", "Aku Baglantisi", 0.5f, 15, 80, SalvageTool.Screwdriver);
            SalvageablePart battery = Salvage("Car Battery", "battery", "Arac Akusu", 14f, 80, 72, SalvageTool.Wrench);
            SalvageablePart wire = Salvage("Alternator Wire", "alternator-wire", "Alternator Kablosu", 0.3f, 20, 60, SalvageTool.WireCutter);
            SalvageablePart alternator = Salvage("Alternator", "alternator", "Alternator", 6f, 170, 91, SalvageTool.Wrench);
            SalvageablePart scrap = Salvage("Loose Metal", "scrap", "Metal Hurda", 3f, 15, 60, SalvageTool.None);
            battery.removeFirst = new[] { clamp };
            alternator.removeFirst = new[] { wire };

            // Battery on its crate next to the engine stand outside (crate top: y 0.45).
            Place(battery, new Vector3(7.7f, 0.45f, 0.6f), 8f, new Vector3(0f, 0.11f, 0f), new Vector3(0.32f, 0.22f, 0.2f));
            Part(battery.transform, "Case", BoxB(0.3f, 0.2f, 0.18f), plastic, Vector3.zero, Vector3.zero, false);
            Part(battery.transform, "Terminal +", Cyl(0.016f, 0.025f, 10), terminalRed, new Vector3(0.09f, 0.2f, 0.04f), Vector3.zero, false);
            Part(battery.transform, "Terminal -", Cyl(0.016f, 0.025f, 10), lead, new Vector3(-0.09f, 0.2f, 0.04f), Vector3.zero, false);

            // Clamp sits on the + terminal and pokes out above the case so it is hit first.
            Place(clamp, new Vector3(7.79f, 0.67f, 0.64f), 8f, new Vector3(0f, 0.03f, 0f), new Vector3(0.12f, 0.08f, 0.12f));
            Part(clamp.transform, "Clamp", BoxB(0.05f, 0.035f, 0.06f), terminalRed, Vector3.zero, Vector3.zero, false);
            Part(clamp.transform, "Cable", Cyl(0.007f, 0.32f, 8), cable, new Vector3(0f, 0.02f, 0f), new Vector3(0f, -20f, 100f), false);

            // Alternator bolted to the engine side, wire running to the block.
            Place(alternator, new Vector3(6.88f, 1.08f, 0.9f), 0f, Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f));
            Part(alternator.transform, "Body", Cyl(0.085f, 0.16f, 18), cast, new Vector3(-0.08f, 0f, 0f), new Vector3(0f, 0f, -90f), false);
            Part(alternator.transform, "Pulley", Cyl(0.04f, 0.03f, 14), steel, new Vector3(0.08f, 0f, 0f), new Vector3(0f, 0f, -90f), false);

            Place(wire, new Vector3(6.72f, 1.2f, 0.88f), 0f, Vector3.zero, new Vector3(0.36f, 0.06f, 0.06f));
            Part(wire.transform, "Cable", Cyl(0.008f, 0.36f, 8), cable, new Vector3(0.18f, 0f, 0f), new Vector3(0f, 0f, 90f), false);

            // Scrap pile outside, right of the door.
            Place(scrap, new Vector3(6.0f, 0f, -1.0f), 25f, new Vector3(0f, 0.17f, 0f), new Vector3(0.95f, 0.35f, 0.75f));
            Part(scrap.transform, "Plate A", BoxC(0.6f, 0.02f, 0.4f), steel, new Vector3(0f, 0.05f, 0f), new Vector3(0f, 10f, 6f), false);
            Part(scrap.transform, "Plate B", BoxC(0.5f, 0.02f, 0.35f), steel, new Vector3(0.1f, 0.12f, 0.05f), new Vector3(8f, -25f, -12f), false);
            Part(scrap.transform, "Plate C", BoxC(0.45f, 0.02f, 0.3f), sheet, new Vector3(-0.12f, 0.2f, -0.05f), new Vector3(-15f, 40f, 9f), false);
            Part(scrap.transform, "Pipe", Cyl(0.04f, 0.9f, 12), steel, new Vector3(-0.4f, 0.06f, 0.2f), new Vector3(0f, 30f, 90f), false);
        }

        private static void RestyleTrader()
        {
            Transform t = ScavengerWalkSetup.FindInScene("Trade Post - E to Sell");
            GameObject go;
            if (t != null) go = t.gameObject;
            else
            {
                AfterrustTrader existing = Object.FindFirstObjectByType<AfterrustTrader>(FindObjectsInactive.Include);
                go = existing != null ? existing.gameObject : new GameObject("Trade Post - E to Sell");
            }
            if (go.GetComponent<AfterrustTrader>() == null) go.AddComponent<AfterrustTrader>();
            go.name = "Trade Post - E to Sell";
            Strip(go);
            go.SetActive(true);
            go.transform.SetPositionAndRotation(new Vector3(10.5f, 0f, -12.6f), Quaternion.Euler(0f, 180f, 0f));
            go.transform.localScale = Vector3.one;
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.52f, 0f);
            box.size = new Vector3(1.7f, 1.05f, 0.65f);

            Part(go.transform, "Counter Body", BoxB(1.6f, 0.95f, 0.55f), wood, Vector3.zero, Vector3.zero, false);
            Part(go.transform, "Counter Top", BoxB(1.7f, 0.05f, 0.65f), steel, new Vector3(0f, 0.95f, 0f), Vector3.zero, false);
            Part(go.transform, "Cash Box", BoxB(0.3f, 0.12f, 0.22f), paintGreen, new Vector3(0.45f, 1.0f, 0f), new Vector3(0f, -6f, 0f), false);
            Part(go.transform, "Ledger", BoxB(0.2f, 0.012f, 0.28f), paper, new Vector3(-0.25f, 1.0f, 0.05f), new Vector3(0f, 12f, 0f), false);
            Part(go.transform, "Radio", BoxB(0.3f, 0.18f, 0.12f), paintBlue, new Vector3(-0.6f, 1.0f, 0.15f), new Vector3(0f, 4f, 0f), false);
        }

        private static SalvageablePart Salvage(string name, string id, string title, float mass, int value,
            int condition, SalvageTool tool)
        {
            Transform t = ScavengerWalkSetup.FindInScene(name);
            GameObject go = t != null ? t.gameObject : new GameObject(name);
            if (t == null) Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            SalvageablePart part = go.GetComponent<SalvageablePart>();
            if (part == null)
            {
                part = go.AddComponent<SalvageablePart>();
                part.partId = id;
                part.displayName = title;
                part.massKg = mass;
                part.baseValue = value;
                part.condition = condition;
                part.requiredTool = tool;
            }
            part.wasSalvaged = false;
            Strip(go);
            go.SetActive(true);
            go.transform.localScale = Vector3.one;
            return part;
        }

        private static void Place(Component c, Vector3 position, float yaw, Vector3 colliderCenter, Vector3 colliderSize)
        {
            c.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            BoxCollider box = c.gameObject.AddComponent<BoxCollider>();
            box.center = colliderCenter;
            box.size = colliderSize;
        }

        /// <summary>Removes old primitive visuals/colliders and children; keeps gameplay components.</summary>
        private static void Strip(GameObject go)
        {
            for (int i = go.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(go.transform.GetChild(i).gameObject);
            foreach (Collider col in go.GetComponents<Collider>()) Object.DestroyImmediate(col);
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) Object.DestroyImmediate(mr);
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null) Object.DestroyImmediate(mf);
        }

        private static void RemoveLegacyPrimitives()
        {
            List<GameObject> doomed = new List<GameObject>();
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                string n = root.name;
                if (n.StartsWith("Abandoned Industrial Block") || n == "Abandoned Car - Chassis (prototype)" ||
                    n == "Character Preview Platform" || n == "Scavenger Placeholder - Replace with Meshy FBX")
                    doomed.Add(root);
            }
            foreach (GameObject go in doomed) Undo.DestroyObjectImmediate(go);
        }

        // ------------------------------------------------------------------ helpers

        private static string Key(string kind, params float[] values)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(kind);
            foreach (float v in values) sb.Append('_').Append(v.ToString("0.###", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        /// <summary>Box with its pivot at the bottom centre.</summary>
        private static Mesh BoxB(float x, float y, float z)
        {
            string key = Key("Box_B", x, y, z);
            if (!Cache.TryGetValue(key, out Mesh m))
                Cache[key] = m = MeshKit.Box(key, new Vector3(x, y, z), new Vector3(0f, y * 0.5f, 0f));
            return m;
        }

        /// <summary>Box with its pivot at the centre.</summary>
        private static Mesh BoxC(float x, float y, float z)
        {
            string key = Key("Box_C", x, y, z);
            if (!Cache.TryGetValue(key, out Mesh m))
                Cache[key] = m = MeshKit.Box(key, new Vector3(x, y, z), Vector3.zero);
            return m;
        }

        /// <summary>Cylinder along local Y, pivot at the bottom centre.</summary>
        private static Mesh Cyl(float radius, float height, int segments)
        {
            string key = Key("Cyl", radius, height, segments);
            if (!Cache.TryGetValue(key, out Mesh m))
                Cache[key] = m = MeshKit.Cylinder(key, radius, height, segments);
            return m;
        }

        private static Transform Group(Transform parent, string name)
        {
            Transform g = new GameObject(name).transform;
            g.SetParent(parent, false);
            return g;
        }

        private static GameObject Part(Transform parent, string name, Mesh mesh, Material mat, Vector3 localPos,
            Vector3 localEuler = default, bool collider = true)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = localEuler;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            if (collider)
            {
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = mesh.bounds.center;
                box.size = mesh.bounds.size;
            }
            bool gameplay = parent != null && parent.GetComponentInParent<SalvageablePart>() != null;
            go.isStatic = !gameplay;
            return go;
        }
    }
}
#endif
