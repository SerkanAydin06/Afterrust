using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Afterrust
{
    /// <summary>
    /// Day/night cycle and bleak weather: permanently overcast, dark storm clouds, rain showers
    /// with occasional lightning. Drives the sun/moon light, sky, ambient, fog, cloud layers,
    /// rain particles and the lamps (on when it is dark).
    /// </summary>
    public sealed class DayNightWeather : MonoBehaviour
    {
        public static DayNightWeather Instance { get; private set; }

        [Header("Time")]
        [Tooltip("Real minutes for a full 24 h day.")]
        public float dayLengthMinutes = 24f;
        [Range(0f, 24f)] public float timeOfDay = 7.5f;
        public int day = 1;

        [Header("Weather")]
        public bool raining;
        public Vector2 dryMinutes = new Vector2(2.5f, 6f);
        public Vector2 rainMinutes = new Vector2(1.5f, 4f);
        public Vector2 lightningSeconds = new Vector2(14f, 40f);
        [Tooltip("Remaining seconds of the current weather state.")]
        public float weatherTimer = 120f;

        [Header("Scene references")]
        public Light sun;
        public Light lightning;
        public Renderer[] cloudLayers = new Renderer[0];
        public ParticleSystem rain;
        public Light[] lamps = new Light[0];
        public Renderer[] lampBulbs = new Renderer[0];
        public float rainRate = 2600f;

        private Material sky;
        private Material[] cloudMats;
        private float rainBlend;          // 0 dry .. 1 full rain (smoothed)
        private float flash;              // lightning brightness 0..1
        private float nextLightning;

        public bool IsRaining => raining;
        public float Darkness { get; private set; }

        public string ClockText
        {
            get
            {
                int h = Mathf.FloorToInt(timeOfDay);
                int m = Mathf.FloorToInt((timeOfDay - h) * 60f);
                return "Gün " + day + "  " + h.ToString("00") + ":" + m.ToString("00");
            }
        }

        public string WeatherText => raining ? (rainBlend > 0.6f ? "Sağanak" : "Yağmur başlıyor") : "Kapalı";

        private void Awake()
        {
            Instance = this;
            if (RenderSettings.skybox != null)
            {
                sky = new Material(RenderSettings.skybox);   // runtime copy, asset untouched
                RenderSettings.skybox = sky;
            }
            cloudMats = new Material[cloudLayers.Length];
            for (int i = 0; i < cloudLayers.Length; i++)
                if (cloudLayers[i] != null) cloudMats[i] = cloudLayers[i].material;
            if (lightning != null) lightning.intensity = 0f;
            rainBlend = raining ? 1f : 0f;
            nextLightning = Time.time + Random.Range(lightningSeconds.x, lightningSeconds.y);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            timeOfDay += dt * 24f / (dayLengthMinutes * 60f);
            if (timeOfDay >= 24f)
            {
                timeOfDay -= 24f;
                day++;
            }

            weatherTimer -= dt;
            if (weatherTimer <= 0f)
            {
                raining = !raining;
                Vector2 range = raining ? rainMinutes : dryMinutes;
                weatherTimer = Random.Range(range.x, range.y) * 60f;
            }
            rainBlend = Mathf.MoveTowards(rainBlend, raining ? 1f : 0f, dt / 20f);

            if (rainBlend > 0.6f && Time.time >= nextLightning)
            {
                StartCoroutine(Lightning());
                nextLightning = Time.time + Random.Range(lightningSeconds.x, lightningSeconds.y);
            }

            Apply();
        }

        private IEnumerator Lightning()
        {
            flash = 1f; yield return new WaitForSeconds(0.07f);
            flash = 0.15f; yield return new WaitForSeconds(0.06f);
            flash = 0.8f; yield return new WaitForSeconds(0.12f);
            flash = 0f;
        }

        /// <summary>Restores time/weather from a save.</summary>
        public void Restore(float time, int savedDay, bool isRaining, float timer)
        {
            timeOfDay = Mathf.Repeat(time, 24f);
            day = Mathf.Max(1, savedDay);
            raining = isRaining;
            weatherTimer = timer;
            rainBlend = raining ? 1f : 0f;
        }

        private void Apply()
        {
            // Sun elevation: 0 deg at 06:00, 90 at 12:00, 180 at 18:00.
            float elevation = timeOfDay / 24f * 360f - 90f;
            float sinElev = Mathf.Sin(elevation * Mathf.Deg2Rad);
            float dayF = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.12f, 0.3f, sinElev));
            float rainDim = Mathf.Lerp(1f, 0.55f, rainBlend);
            Darkness = 1f - dayF * Mathf.Lerp(1f, 0.7f, rainBlend);

            if (sun != null)
            {
                bool isDay = sinElev > -0.05f;
                sun.transform.rotation = isDay
                    ? Quaternion.Euler(elevation, -60f, 0f)
                    : Quaternion.Euler(elevation + 180f, 120f, 0f);           // moon opposite
                sun.color = isDay ? new Color(0.78f, 0.8f, 0.84f) : new Color(0.55f, 0.62f, 0.8f);
                sun.intensity = isDay ? Mathf.Lerp(0.05f, 0.55f, dayF) * rainDim : 0.05f * (1f - rainBlend * 0.6f);
                sun.shadowStrength = Mathf.Lerp(0.2f, 0.45f, dayF) * rainDim;
            }

            float bright = Mathf.Max(dayF * rainDim, 0.04f) + flash * 0.9f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(0.03f, 0.035f, 0.05f), new Color(0.44f, 0.46f, 0.5f), bright);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(0.025f, 0.025f, 0.03f), new Color(0.36f, 0.36f, 0.36f), bright);
            RenderSettings.ambientGroundColor = Color.Lerp(new Color(0.01f, 0.01f, 0.012f), new Color(0.16f, 0.15f, 0.14f), bright);

            Color dayFog = Color.Lerp(new Color(0.44f, 0.46f, 0.48f), new Color(0.32f, 0.34f, 0.36f), rainBlend);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = Color.Lerp(new Color(0.03f, 0.035f, 0.045f), dayFog, Mathf.Max(dayF, flash * 0.6f));
            RenderSettings.fogDensity = Mathf.Lerp(0.015f, 0.03f, rainBlend);

            if (sky != null)
            {
                if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", Mathf.Lerp(0.03f, 0.5f, dayF) * rainDim + flash * 0.8f);
                if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", new Color(0.4f, 0.42f, 0.45f));
                if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", new Color(0.25f, 0.25f, 0.25f));
                if (sky.HasProperty("_SunDisk")) sky.SetFloat("_SunDisk", 0f);
                if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 0.9f);
            }

            for (int i = 0; i < cloudMats.Length; i++)
            {
                Material m = cloudMats[i];
                if (m == null) continue;
                float level = Mathf.Lerp(0.015f, 0.2f, dayF) * Mathf.Lerp(1f, 0.6f, rainBlend) + flash * 0.25f;
                // Legacy particle shader doubles the tint: 0.5 = texture colour.
                m.SetColor("_TintColor", new Color(level, level * 1.02f, level * 1.06f, Mathf.Lerp(0.85f, 1f, rainBlend) * 0.5f));
                m.mainTextureOffset += new Vector2(0.004f + i * 0.002f, 0.0015f) * Time.deltaTime * Mathf.Lerp(1f, 2.2f, rainBlend);
            }

            if (rain != null)
            {
                ParticleSystem.EmissionModule em = rain.emission;
                em.rateOverTime = rainRate * rainBlend * rainBlend;
            }
            if (lightning != null) lightning.intensity = flash * 2.2f;

            bool lampsOn = Darkness > 0.55f;
            foreach (Light l in lamps) if (l != null && l.enabled != lampsOn) l.enabled = lampsOn;
            foreach (Renderer r in lampBulbs) if (r != null && r.enabled != lampsOn) r.enabled = lampsOn;
        }
    }
}
