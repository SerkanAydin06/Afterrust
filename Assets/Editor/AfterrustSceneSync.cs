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
        private const int Latest = 3;

        private static bool scheduled;
        private static bool allStepsSucceeded;

        static AfterrustSceneSync()
        {
            EditorSceneManager.sceneOpened += (scene, mode) => Schedule();
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
                Step("Repair shop", RepairShopBuilder.Build);
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
                Step("Repair shop (no overlaps)", RepairShopBuilder.Build);
                Step("Survival systems", SurvivalSetup.Apply);
                Step("Day/night, storm weather, rain, flashlight, save system", WeatherSetup.Apply);
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
