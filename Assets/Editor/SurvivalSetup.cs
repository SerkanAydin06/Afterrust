#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Scene update v2 (survival): adds vitals + HUD to the player and turns repair-shop props
    /// into searchable loot containers, a rain barrel and the crafting workbench.
    /// </summary>
    internal static class SurvivalSetup
    {
        public static void Apply()
        {
            SetupPlayer();

            Transform shop = ScavengerWalkSetup.FindInScene("Shelter");
            if (shop == null)
            {
                Debug.LogWarning("[Afterrust] Shelter not found; loot containers skipped.");
                return;
            }

            LootEntry[] toolbox =
            {
                new LootEntry("duct_tape", 0.75f, 1, 2), new LootEntry("wire", 0.5f, 1, 2),
                new LootEntry("scrap", 0.3f, 1, 1), new LootEntry("cloth", 0.3f, 1, 1)
            };
            LootEntry[] cardboard =
            {
                new LootEntry("cloth", 0.6f, 1, 2), new LootEntry("crackers", 0.45f, 1, 2),
                new LootEntry("canned_food", 0.3f, 1, 1), new LootEntry("charcoal", 0.3f, 1, 1),
                new LootEntry("water_clean", 0.2f, 1, 1)
            };
            LootEntry[] crate =
            {
                new LootEntry("charcoal", 0.7f, 1, 2), new LootEntry("cloth", 0.4f, 1, 1),
                new LootEntry("wood", 0.6f, 1, 2)
            };
            LootEntry[] woodpile = { new LootEntry("wood", 1f, 5, 8) };
            LootEntry[] pallets = { new LootEntry("wood", 1f, 3, 5) };
            LootEntry[] container =
            {
                new LootEntry("canned_food", 0.6f, 1, 2), new LootEntry("water_clean", 0.6f, 1, 2),
                new LootEntry("cloth", 0.7f, 1, 3), new LootEntry("duct_tape", 0.5f, 1, 1),
                new LootEntry("wire", 0.4f, 1, 2), new LootEntry("bandage", 0.3f, 1, 1),
                new LootEntry("crackers", 0.5f, 1, 2), new LootEntry("wood", 0.5f, 1, 3)
            };

            foreach (Transform t in shop.GetComponentsInChildren<Transform>(true))
            {
                switch (t.name)
                {
                    case "Toolbox": Loot(t, "Alet Kutusu", 1.4f, toolbox, 0, 0); break;
                    case "Box Under Bench A":
                    case "Box Under Bench B":
                    case "Box": Loot(t, "Karton Kutu", 1.2f, cardboard, 0, 0); break;
                    case "Battery Crate": Loot(t, "Tahta Sandık", 1.5f, crate, 0, 0); break;
                    case "Woodpile": Loot(t, "Odun Yığını", 2.0f, woodpile, 0, 0); break;
                    case "Pallet Stack": Loot(t, "Palet Yığını (sök)", 2.5f, pallets, 0, 0); break;
                    case "Shipping Container": Loot(t, "Konteyner", 2.5f, container, 5, 20); break;
                    case "Bench Top": if (t.GetComponent<Workbench>() == null) t.gameObject.AddComponent<Workbench>(); break;
                }
            }
            // RainCollectorBuilder owns the single open collector and its WaterSource.
            // Do not turn any barrel into randomized loot or initialize free water.

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void SetupPlayer()
        {
            AfterrustInteraction interaction = Object.FindFirstObjectByType<AfterrustInteraction>(FindObjectsInactive.Include);
            if (interaction == null)
            {
                Debug.LogWarning("[Afterrust] Player (AfterrustInteraction) not found.");
                return;
            }
            GameObject player = interaction.gameObject;
            SurvivorVitals vitals = Get<SurvivorVitals>(player);
            SurvivalHud hud = Get<SurvivalHud>(player);
            AfterrustInventory inventory = Get<AfterrustInventory>(player);
            AfterrustFirstPersonController controller = player.GetComponent<AfterrustFirstPersonController>();

            interaction.inventory = inventory;
            interaction.vitals = vitals;
            interaction.hud = hud;
            interaction.maxDistance = 3.2f;
            hud.interaction = interaction;
            hud.inventory = inventory;
            hud.vitals = vitals;
            hud.controller = controller;

            // Survival start: basic hand tools, nothing else.
            inventory.items.Clear();
            inventory.credits = 0;
            inventory.capacityKg = 35f;
            inventory.hasWrench = true;
            inventory.hasScrewdriver = true;
            inventory.hasWireCutter = false;
            inventory.hasLargeBackpack = false;
            EditorUtility.SetDirty(player);
        }

        private static T Get<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        private static void Loot(Transform t, string name, float seconds, LootEntry[] table, int creditsMin, int creditsMax)
        {
            if (t.GetComponent<Collider>() == null)
            {
                MeshFilter mf = t.GetComponent<MeshFilter>();
                BoxCollider box = t.gameObject.AddComponent<BoxCollider>();
                if (mf != null && mf.sharedMesh != null)
                {
                    box.center = mf.sharedMesh.bounds.center;
                    box.size = mf.sharedMesh.bounds.size;
                }
            }
            t.gameObject.isStatic = false;
            LootContainer lc = t.GetComponent<LootContainer>();
            if (lc == null) lc = t.gameObject.AddComponent<LootContainer>();
            lc.displayName = name;
            lc.searchSeconds = seconds;
            lc.table = (LootEntry[])table.Clone();
            lc.creditsMin = creditsMin;
            lc.creditsMax = creditsMax;
        }
    }
}
#endif
