#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Scene update v5: stove fire (light, glow, flue smoke), sleeping in the bed, and the
    /// warmth/energy sensors on the player.
    /// </summary>
    internal static class FireSleepSetup
    {
        public static void Apply()
        {
            AfterrustInteraction player = Object.FindFirstObjectByType<AfterrustInteraction>(FindObjectsInactive.Include);
            if (player != null)
            {
                if (player.GetComponent<ThermalSensor>() == null) player.gameObject.AddComponent<ThermalSensor>();
                if (player.GetComponent<SleepSystem>() == null) player.gameObject.AddComponent<SleepSystem>();
                SurvivorVitals v = player.GetComponent<SurvivorVitals>();
                if (v != null) { v.warmth = 70f; v.energy = 100f; }
            }

            Transform shelter = ScavengerWalkSetup.FindInScene("Shelter");
            if (shelter == null)
            {
                Debug.LogWarning("[Afterrust] Shelter not found; stove and bed skipped.");
                return;
            }
            Transform stoveT = null, bedT = null;
            foreach (Transform t in shelter.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Stove") stoveT = t;
                else if (t.name == "Bed") bedT = t;
            }

            if (bedT != null && bedT.GetComponent<Bed>() == null) bedT.gameObject.AddComponent<Bed>();
            if (stoveT != null) SetupStove(stoveT);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void SetupStove(Transform t)
        {
            Stove stove = t.GetComponent<Stove>();
            if (stove == null) stove = t.gameObject.AddComponent<Stove>();
            stove.burnSecondsLeft = 0f;

            Transform door = t.Find("Stove Door");
            if (door != null)
            {
                door.gameObject.isStatic = false;
                Renderer r = door.GetComponent<Renderer>();
                stove.glowRenderer = r;
                stove.glowOff = r != null ? r.sharedMaterial : null;
                stove.glowOn = MaterialKit.Emissive("M_StoveGlow", new Color(0.35f, 0.12f, 0.05f), new Color(2.6f, 0.9f, 0.25f));
            }

            Light fire = new GameObject("Fire Light").AddComponent<Light>();
            fire.transform.SetParent(t, false);
            fire.transform.localPosition = new Vector3(-0.35f, 0.45f, -0.35f);
            fire.type = LightType.Point;
            fire.range = 6f;
            fire.intensity = 1.6f;
            fire.color = new Color(1f, 0.55f, 0.25f);
            fire.shadows = LightShadows.Hard;
            fire.enabled = false;
            stove.fireLight = fire;

            stove.smoke = Smoke(t);
        }

        private static ParticleSystem Smoke(Transform stove)
        {
            GameObject go = new GameObject("Flue Smoke");
            go.transform.SetParent(stove, false);
            go.transform.localPosition = new Vector3(0f, 3.25f, 0f);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);   // cone points up
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 6f;
            main.startSpeed = 0.6f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startColor = new Color(0.36f, 0.36f, 0.37f, 1f);
            main.gravityModifier = -0.02f;
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 5f;
            em.enabled = false;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.05f;

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(3f, AnimationCurve.Linear(0f, 0.35f, 1f, 1f));

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.55f, 0.15f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(g);

            ParticleSystem.ForceOverLifetimeModule force = ps.forceOverLifetime;
            force.enabled = true;
            force.space = ParticleSystemSimulationSpace.World;
            force.x = new ParticleSystem.MinMaxCurve(0.25f);
            force.y = new ParticleSystem.MinMaxCurve(0f);
            force.z = new ParticleSystem.MinMaxCurve(0.08f);

            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = WeatherSetup.ParticleMaterial("M_Smoke", MaterialKit.TexDir + "/smoke_puff.png",
                new Color(0.5f, 0.5f, 0.5f, 0.5f));
            return ps;
        }
    }
}
#endif
