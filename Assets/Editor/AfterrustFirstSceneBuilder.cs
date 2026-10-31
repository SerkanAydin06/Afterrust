#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

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
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");

            Material concrete = GetOrCreateMaterial("M_Concrete", new Color(0.19f, 0.21f, 0.22f));
            Material ruinMetal = GetOrCreateMaterial("M_RuinMetal", new Color(0.28f, 0.29f, 0.27f));
            Material rust = GetOrCreateMaterial("M_Rust", new Color(0.38f, 0.22f, 0.14f));
            Material darkMetal = GetOrCreateMaterial("M_DarkMetal", new Color(0.12f, 0.14f, 0.15f));
            Material traderPaint = GetOrCreateMaterial("M_Trader", new Color(0.33f, 0.36f, 0.26f));

            // Neutral, low-glare lighting; avoid Unity's bright default skybox and white primitives.
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.24f, 0.26f);
            RenderSettings.reflectionIntensity = 0.15f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.28f, 0.30f, 0.31f);
            RenderSettings.fogStartDistance = 28f;
            RenderSettings.fogEndDistance = 85f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Industrial Ground";
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(65f, 1f, 65f);
            ApplyMaterial(floor, concrete);

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
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.28f, 0.30f, 0.31f);
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
            sunlight.intensity = 0.65f;
            sunlight.color = new Color(0.86f, 0.89f, 0.92f);
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            CreateSalvage("Car Battery", new Vector3(-1.2f, 0.7f, 0f), new Vector3(0.7f, 0.5f, 0.5f),
                "battery", "Arac Akusu", 14f, 80, 72, true, darkMetal);
            CreateSalvage("Alternator", new Vector3(0f, 0.7f, 0f), new Vector3(0.5f, 0.5f, 0.5f),
                "alternator", "Alternator", 6f, 170, 91, true, rust);
            CreateSalvage("Loose Metal", new Vector3(1.2f, 0.5f, 0f), new Vector3(0.7f, 0.2f, 0.7f),
                "scrap", "Metal Hurda", 3f, 15, 60, false, ruinMetal);

            GameObject trader = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            trader.name = "Trade Post - E to Sell";
            trader.transform.position = new Vector3(5f, 1f, 0f);
            trader.AddComponent<AfterrustTrader>();
            ApplyMaterial(trader, traderPaint);

            for (int i = 0; i < 10; i++)
            {
                GameObject ruin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ruin.name = "Abandoned Industrial Block " + i;
                ruin.transform.position = new Vector3(i % 5 * 8f - 16f, 1.5f, i / 5 * 12f + 12f);
                ruin.transform.localScale = new Vector3(5f, 3f, 5f);
                ApplyMaterial(ruin, i % 3 == 0 ? rust : ruinMetal);
            }

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Selection.activeGameObject = player;
            Debug.Log("[Afterrust] Industrial prototype rebuilt with dark materials and balanced lighting. Press Play.");
        }

        private static void ApplyMaterial(GameObject obj, Material material)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            string path = "Assets/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null)
                throw new System.InvalidOperationException("Afterrust: No supported material shader found.");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
                material.shader = shader;

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.12f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateSalvage(string name, Vector3 position, Vector3 scale,
            string id, string title, float mass, int value, int condition, bool wrench, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            ApplyMaterial(obj, material);
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
