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
    /// Scene update v3: bleak overcast world with day/night cycle, storm cloud layers, rain
    /// (collides with roofs), lightning, flashlight on the player camera, save system and a
    /// cold "Storm" post-processing grade. Re-running replaces the "World Environment" root.
    /// </summary>
    internal static class WeatherSetup
    {
        private const string RootName = "World Environment";
        private const string TexDir = MaterialKit.TexDir;
        private const string MatDir = MaterialKit.MatDir;

        public static void Apply()
        {
            Transform old = ScavengerWalkSetup.FindInScene(RootName);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "World environment");

            AfterrustInteraction player = Object.FindFirstObjectByType<AfterrustInteraction>(FindObjectsInactive.Include);
            DayNightWeather world = root.AddComponent<DayNightWeather>();
            world.timeOfDay = 7.5f;
            world.day = 1;
            world.raining = false;
            world.weatherTimer = 90f;

            // Sun / moon
            world.sun = FindSun();
            if (world.sun == null)
            {
                world.sun = new GameObject("Sun").AddComponent<Light>();
                world.sun.type = LightType.Directional;
            }
            world.sun.shadows = LightShadows.Soft;
            world.sun.transform.rotation = Quaternion.Euler(22.5f, -60f, 0f);
            world.sun.color = new Color(0.78f, 0.8f, 0.84f);
            world.sun.intensity = 0.5f;
            world.sun.shadowStrength = 0.4f;

            // Lightning: extra directional light, off until a strike.
            Light bolt = new GameObject("Lightning").AddComponent<Light>();
            bolt.transform.SetParent(root.transform, false);
            bolt.transform.rotation = Quaternion.Euler(62f, 25f, 0f);
            bolt.type = LightType.Directional;
            bolt.color = new Color(0.78f, 0.84f, 1f);
            bolt.intensity = 0f;
            bolt.shadows = LightShadows.None;
            world.lightning = bolt;

            // Overcast sky (runtime copy is driven by DayNightWeather).
            Material sky = SkyMaterial();
            RenderSettings.skybox = sky;
            RenderSettings.sun = world.sun;

            // Storm cloud layers. Low enough that distance fog does not erase them.
            Material clouds = CloudMaterial();
            Mesh plane = CloudPlane();
            world.cloudLayers = new[]
            {
                CloudLayer(root.transform, "Clouds Low", plane, clouds, 55f, new Vector3(1f, 1f, 1f), 0f),
                CloudLayer(root.transform, "Clouds High", plane, clouds, 85f, new Vector3(1.6f, 1f, 1.6f), 37f)
            };

            // Rain follows the player; drops die on contact with roofs and ground.
            if (player != null)
            {
                Transform oldRain = player.transform.Find("Rain");
                if (oldRain != null) Undo.DestroyObjectImmediate(oldRain.gameObject);
                world.rain = Rain(player.transform);
                AddFlashlight(player);
                if (player.GetComponent<SaveSystem>() == null) player.gameObject.AddComponent<SaveSystem>();
            }

            // Lamps switch on when it gets dark.
            Transform lampsRoot = ScavengerWalkSetup.FindInScene("Scrapyard Lamps");
            List<Light> lamps = new List<Light>();
            List<Renderer> bulbs = new List<Renderer>();
            if (lampsRoot != null)
            {
                lamps.AddRange(lampsRoot.GetComponentsInChildren<Light>(true));
                foreach (Renderer r in lampsRoot.GetComponentsInChildren<Renderer>(true))
                    if (r.name == "Bulb") bulbs.Add(r);
            }
            world.lamps = lamps.ToArray();
            world.lampBulbs = bulbs.ToArray();

            // Editor view: overcast morning (play mode takes over every frame).
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.4f, 0.42f, 0.46f);
            RenderSettings.ambientEquatorColor = new Color(0.33f, 0.33f, 0.33f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.14f, 0.13f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.42f, 0.44f, 0.46f);
            RenderSettings.fogDensity = 0.016f;

            ScrapyardPostProcessSetup.Apply(ScrapyardPostProcessSetup.Storm);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static Light FindSun()
        {
            Light fallback = null;
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (l.type != LightType.Directional || l.name == "Lightning") continue;
                if (l.name == "Sun") return l;
                if (fallback == null) fallback = l;
            }
            return fallback;
        }

        private static Material SkyMaterial()
        {
            MaterialKit.EnsureFolder(MatDir);
            string path = MatDir + "/SKY_Scrapyard.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Skybox/Procedural")) { name = "SKY_Scrapyard" };
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetFloat("_SunDisk", 0f);
            m.SetFloat("_AtmosphereThickness", 0.9f);
            m.SetColor("_SkyTint", new Color(0.4f, 0.42f, 0.45f));
            m.SetColor("_GroundColor", new Color(0.25f, 0.25f, 0.25f));
            m.SetFloat("_Exposure", 0.5f);
            EditorUtility.SetDirty(m);
            return m;
        }

        internal static Material ParticleMaterial(string name, string texturePath, Color tint)
        {
            TextureImporter ti = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (ti != null && (!ti.alphaIsTransparency || ti.wrapMode != TextureWrapMode.Repeat))
            {
                ti.alphaIsTransparency = true;
                ti.wrapMode = TextureWrapMode.Repeat;
                ti.SaveAndReimport();
            }
            string path = MatDir + "/" + name + ".mat";
            Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            else if (m.shader != shader) m.shader = shader;
            m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            m.SetColor("_TintColor", tint);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Material CloudMaterial()
        {
            return ParticleMaterial("M_StormClouds", TexDir + "/storm_clouds.png", new Color(0.2f, 0.2f, 0.21f, 0.45f));
        }

        /// <summary>1200 m quad facing down, texture repeating every 150 m.</summary>
        private static Mesh CloudPlane()
        {
            const float s = 600f, tiles = 8f;
            Mesh mesh = new Mesh { name = "CloudPlane" };
            mesh.vertices = new[]
            {
                new Vector3(-s, 0f, -s), new Vector3(s, 0f, -s), new Vector3(s, 0f, s), new Vector3(-s, 0f, s)
            };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(tiles, 0f), new Vector2(tiles, tiles), new Vector2(0f, tiles) };
            mesh.normals = new[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };   // Cross(B-A, C-A) points down: visible from below
            mesh.RecalculateBounds();

            MaterialKit.EnsureFolder(MeshKit.MeshDir);
            string path = MeshKit.MeshDir + "/CloudPlane.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            return existing;
        }

        private static Renderer CloudLayer(Transform parent, string name, Mesh mesh, Material mat, float height,
            Vector3 scale, float yaw)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, height, 0f);
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return r;
        }

        private static ParticleSystem Rain(Transform player)
        {
            GameObject go = new GameObject("Rain");
            go.transform.SetParent(player, false);
            go.transform.localPosition = new Vector3(0f, 14f, 0f);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 1.1f;
            main.startSpeed = 22f;
            main.startSize = 0.025f;
            main.startColor = new Color(0.78f, 0.82f, 0.88f, 0.6f);
            main.gravityModifier = 0.3f;
            main.maxParticles = 6000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.rotation = new Vector3(90f, 0f, 0f);   // emit straight down
            shape.scale = new Vector3(36f, 36f, 0.1f);

            ParticleSystem.CollisionModule collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.quality = ParticleSystemCollisionQuality.Low;
            collision.lifetimeLoss = 1f;
            collision.bounce = 0f;

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.035f;
            renderer.lengthScale = 1f;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = ParticleMaterial("M_Rain", TexDir + "/rain_streak.png", new Color(0.5f, 0.5f, 0.5f, 0.35f));
            return ps;
        }

        private static void AddFlashlight(AfterrustInteraction player)
        {
            Camera cam = player.viewCamera != null ? player.viewCamera : player.GetComponentInChildren<Camera>(true);
            if (cam == null) return;
            Transform oldTorch = cam.transform.Find("Flashlight");
            if (oldTorch != null) Undo.DestroyObjectImmediate(oldTorch.gameObject);
            GameObject go = new GameObject("Flashlight");
            go.transform.SetParent(cam.transform, false);
            go.transform.localPosition = new Vector3(0.18f, -0.15f, 0.1f);
            Light torch = go.AddComponent<Light>();
            torch.type = LightType.Spot;
            torch.range = 24f;
            torch.spotAngle = 58f;
            torch.innerSpotAngle = 28f;
            torch.intensity = 1.8f;
            torch.color = new Color(1f, 0.95f, 0.85f);
            torch.shadows = LightShadows.Hard;
            torch.enabled = false;
            go.AddComponent<Flashlight>();
        }
    }
}
#endif
