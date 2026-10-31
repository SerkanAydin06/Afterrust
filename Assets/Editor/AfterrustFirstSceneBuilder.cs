#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    [InitializeOnLoad]
    public static class AfterrustFirstSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/AfterrustPrototype.unity";
        private const string PrefKey = "Afterrust.SceneInitialized.1";

        static AfterrustFirstSceneBuilder()
        {
            EditorApplication.delayCall += TryInitialize;
        }

        private static void TryInitialize()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;
            if (SessionState.GetBool(PrefKey, false)) return;
            SessionState.SetBool(PrefKey, true);
            BuildScene();
        }

        [MenuItem("Afterrust/Create or Rebuild Prototype Scene")]
        public static void BuildScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AssetDatabase.CreateFolder("Assets", "Scenes");

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Industrial Ground";
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(65f, 1f, 65f);

            GameObject player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 1.2f, -9f);
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.75f;
            cc.radius = 0.32f;
            cc.center = new Vector3(0f, 0.875f, 0f);

            GameObject cameraGo = new GameObject("First Person Camera");
            cameraGo.transform.SetParent(player.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            Camera cam = cameraGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cameraGo.AddComponent<AudioListener>();

            AfterrustFirstPersonController controller = player.AddComponent<AfterrustFirstPersonController>();
            controller.viewCamera = cam;
            AfterrustInventory inventory = player.AddComponent<AfterrustInventory>();
            AfterrustInteraction interaction = player.AddComponent<AfterrustInteraction>();
            interaction.viewCamera = cam;
            interaction.inventory = inventory;

            GameObject lightObject = new GameObject("Sun");
            Light sunlight = lightObject.AddComponent<Light>();
            sunlight.type = LightType.Directional;
            sunlight.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.46f, 0.48f, 0.5f);

            CreateSalvage("Car Battery", new Vector3(-1.2f, 0.7f, 0f), new Vector3(0.7f, 0.5f, 0.5f),
                "battery", "Arac Akusu", 14f, 80, 72, true);
            CreateSalvage("Alternator", new Vector3(0f, 0.7f, 0f), new Vector3(0.5f, 0.5f, 0.5f),
                "alternator", "Alternator", 6f, 170, 91, true);
            CreateSalvage("Loose Metal", new Vector3(1.2f, 0.5f, 0f), new Vector3(0.7f, 0.2f, 0.7f),
                "scrap", "Metal Hurda", 3f, 15, 60, false);

            GameObject trader = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            trader.name = "Trade Post - E to Sell";
            trader.transform.position = new Vector3(5f, 1f, 0f);
            trader.AddComponent<AfterrustTrader>();

            for (int i = 0; i < 10; i++)
            {
                GameObject ruin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ruin.name = "Abandoned Industrial Block " + i;
                ruin.transform.position = new Vector3(i % 5 * 8f - 16f, 1.5f, i / 5 * 12f + 12f);
                ruin.transform.localScale = new Vector3(5f, 3f, 5f);
            }

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Selection.activeGameObject = player;
            Debug.Log("[Afterrust] Prototype scene created. Press Play. Controls: WASD, mouse, E.");
        }

        private static void CreateSalvage(string name, Vector3 position, Vector3 scale,
            string id, string title, float mass, int value, int condition, bool wrench)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            SalvageablePart part = obj.AddComponent<SalvageablePart>();
            part.partId = id;
            part.displayName = title;
            part.massKg = mass;
            part.baseValue = value;
            part.condition = condition;
            part.requiresWrench = wrench;
        }
    }
}
#endif
