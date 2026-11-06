#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Keeps AfterrustPrototype.unity up to date without menus: after every Pull / script reload
    /// or when the scene is opened, pending update steps are applied once and the scene is saved.
    /// The applied version is stored on the "[Scene Build Info]" object (EditorOnly).
    ///
    /// Adding new content = add a new "if (current &lt; N)" block and raise Latest to N.
    /// </summary>
    [InitializeOnLoad]
    internal static class AfterrustSceneSync
    {
        private const string ScenePath = "Assets/Scenes/AfterrustPrototype.unity";
        private const int Latest = 11;

        private static bool scheduled;
        private static bool allStepsSucceeded;

        static AfterrustSceneSync()
        {
            EditorSceneManager.sceneOpened += (scene, mode) => Schedule();
            EditorApplication.playModeStateChanged += CheckBeforePlay;
            Schedule();
        }

        // Stop Play before a stale scene can run: the delayed builder cannot safely
        // modify scene objects once Unity is already entering Play mode.
        private static void CheckBeforePlay(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath) return;
            SceneBuildInfo info = UnityEngine.Object.FindFirstObjectByType<SceneBuildInfo>(FindObjectsInactive.Include);
            if (info != null && info.version >= Latest) return;

            EditorApplication.isPlaying = false;
            Debug.LogError("[Afterrust] Play cancelled: scene version is outdated or the scene update failed. " +
                           "Wait for '[Afterrust] Scene updated' in Console, fix any errors, then press Play again.");
            Schedule();
        }

        private static void Schedule()
        {
            if (scheduled) return;
            scheduled = true;
            EditorApplication.update += WaitUntilIdle;
        }

        private static void WaitUntilIdle()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= WaitUntilIdle;
            scheduled = false;
            Run();
        }

        private static void Run()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath) return;

            SceneBuildInfo info = UnityEngine.Object.FindFirstObjectByType<SceneBuildInfo>(FindObjectsInactive.Include);
            int current = info != null ? info.version : 0;
            if (current >= Latest) return;

            Debug.Log("[Afterrust] Updating scene from version " + current + " to " + Latest + "...");
            allStepsSucceeded = true;

            if (current < 1)
            {
                Step("Yard and fence", ScrapyardEnvironmentBuilder.Build);
                Step("Repair shop", ShelterBuilder.Build);
                Step("Walking scavenger", ScavengerWalkSetup.Setup);
                Step("Character1", Character1Setup.Add);
                Step("Neutral figure", NeutralFigureSetup.Add);
                Step("Remove hidden legacy models", RemoveHiddenLegacy);
                Step("Lighting and post-processing", ScrapyardAtmosphereSetup.ApplyGoldenHour);
            }

            if (current < 2)
            {
                Step("Survival systems (vitals, HUD, loot, workbench, rain barrel)", SurvivalSetup.Apply);
            }

            if (current < 3)
            {
                // Rebuild yard and shop without overlapping (flickering) faces, then re-wire survival.
                Step("Yard and fence (no overlaps)", ScrapyardEnvironmentBuilder.Build);
                Step("Repair shop (no overlaps)", ShelterBuilder.Build);
                Step("Survival systems", SurvivalSetup.Apply);
                Step("Day/night, storm weather, rain, flashlight, save system", WeatherSetup.Apply);
            }

            if (current < 4)
            {
                // Repair shop becomes the survivor's shelter with an opening door.
                Step("Shelter (door, bed, stove; engine and trade stall moved outside)", ShelterBuilder.Build);
                Step("Survival systems", SurvivalSetup.Apply);
            }

            if (current < 5)
            {
                Step("Shelter (woodpile)", ShelterBuilder.Build);
                Step("Survival systems", SurvivalSetup.Apply);
                Step("Stove fire, sleeping, warmth and energy", FireSleepSetup.Apply);
            }

            if (current < 6)
            {
                Step("Ground to the horizon", ScrapyardEnvironmentBuilder.Build);
                Step("Distant ruins, power poles, water tower", WorldBackdropBuilder.Build);
                Step("Shelter lamps cast shadows (no light leaks)", WorldBackdropBuilder.FixLampLeaks);
                Step("Scavenger becomes the trader; extra NPCs removed", WorldBackdropBuilder.ScavengerBecomesTrader);
            }

            if (current < 7)
            {
                Step("First expedition: roadside checkpoint, supplies and contamination", FirstExpeditionBuilder.Build);
                Step("Survivor audio: footsteps, wind, rain and feedback", FirstExpeditionBuilder.EnableAudio);
            }

            if (current < 8)
            {
                // Existing v7 roofs are visual-only; add collision without rebuilding
                // loot, door, stove or any persistent gameplay hierarchy.
                Step("Weatherproof shelter and trade stall roofs", ShelterBuilder.EnsureRainColliders);
            }

            if (current < 9)
            {
                // Work on the sheltered starting yard first. Do NOT rebuild the whole
                // fence or shelter here: old saves rely on the existing hierarchy.
                Step("Close south safe-zone perimeter with an interactive double gate",
                    ScrapyardEnvironmentBuilder.EnsureSafeZoneGate);
                Step("Align trader canopy with all four support posts",
                    ShelterBuilder.AlignTradeStallRoof);
                Step("Pause outside-expedition objective while the safe yard is refined",
                    FirstExpeditionBuilder.PauseExpedition);
            }

            if (current < 10)
            {
                // Keep shelter/loot/save hierarchies intact. Only replace the old
                // three outdoor oil drums with one empty open-top rain collector.
                Step("Single open rain collector (rain-only slow filling)",
                    RainCollectorBuilder.Apply);
            }

            if (current < 11)
            {
                // v10 generated scene has an oversized name plate and no trader lamp.
                // Adjust only these small fixtures; do not regenerate the yard, the
                // gate or any saved loot and water container hierarchies.
                Step("Fit safe-zone lettering to entrance sign",
                    ScrapyardEnvironmentBuilder.CorrectSafeZoneSign);
                Step("Give the trader stall a night lamp",
                    ShelterBuilder.EnsureTraderNightLight);
            }

            if (!allStepsSucceeded)
            {
                Debug.LogError("[Afterrust] Scene update incomplete; it will retry on the next reload. See errors above.");
                return;
            }

            if (info == null)
            {
                GameObject go = new GameObject("[Scene Build Info]") { tag = "EditorOnly" };
                info = go.AddComponent<SceneBuildInfo>();
            }
            info.version = Latest;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Afterrust] Scene updated to version " + Latest + " and saved.");
        }

        private static void Step(string name, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                allStepsSucceeded = false;
                Debug.LogError("[Afterrust] Scene update step '" + name + "' failed: " + e);
            }
        }

        private static void RemoveHiddenLegacy()
        {
            if (ScavengerWalkSetup.FindInScene("Meshy Scavenger - Rigged") == null) return;
            Transform staticModel = ScavengerWalkSetup.FindInScene("Meshy Scavenger - Imported Model");
            if (staticModel != null) UnityEngine.Object.DestroyImmediate(staticModel.gameObject);
        }
    }
}
#endif
