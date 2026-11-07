#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// AR-1502: retune EXISTING shelter lights to make bed/storage/workbench
    /// readable at night. No added real-time lights, shadow maps or colliders.
    /// Exterior yard and gate lights are deliberately left unchanged.
    /// Idempotent for both old scenes and freshly rebuilt lamp sets.
    /// </summary>
    internal static class ShelterInteriorLightingPass
    {
        // ScrapyardAtmosphereSetup builds its workbench lamp here.
        private static readonly Vector3 WorkbenchLampPosition = new Vector3(-1.6f, 2.45f, 5.87f);

        public static void Apply()
        {
            Transform root = ScavengerWalkSetup.FindInScene("Scrapyard Lamps");
            if (root == null)
            {
                // Old or partially upgraded scenes may never have had the
                // atmosphere-created fixtures. Do not rebuild the yard lights:
                // restore only the two inexpensive interior fixtures.
                root = new GameObject("Scrapyard Lamps").transform;
                Undo.RegisterCreatedObjectUndo(root.gameObject, "Restore shelter lamp root");
                Debug.LogWarning("[Afterrust] AR-1502: missing lamp root; created shelter-only fixtures.");
            }

            Transform workbench = FindWorkbenchLamp(root);
            Transform pendant = FindPendantLamp(root);
            bool repaired = false;
            if (workbench == null)
            {
                workbench = MakeFixture(root, "Wall Lamp",
                    WorkbenchLampPosition, false);
                repaired = true;
            }
            if (pendant == null)
            {
                pendant = MakeFixture(root, "Pendant Lamp",
                    new Vector3(0.3f, 2.45f, 3f), true);
                repaired = true;
            }

            Light workLight = EnsureLight(workbench, false);
            Light pendantLight = EnsureLight(pendant, true);
            Transform workBulb = EnsureBulb(workbench, false);
            Transform pendantBulb = EnsureBulb(pendant, true);
            // The day/night arrays were serialized when older lamps were
            // created. Any repaired fixture must be explicitly registered;
            // also drop stale null references left by atmosphere rebuilds.
            SyncWeatherReferences(root);

            // Wall fixture: a directed, shadowless cone, short enough to end well
            // before the shelter's front wall (z=0) or the outdoor forecourt.
            // This replaces an expensive 6m omnidirectional shadowed light.
            Undo.RecordObject(workLight, "Retune v15 workbench light");
            Undo.RecordObject(workLight.transform, "Aim v15 workbench light");
            workLight.type = LightType.Spot;
            workLight.spotAngle = 76f;
            workLight.innerSpotAngle = 40f;
            workLight.range = 3.9f;
            workLight.intensity = 2.0f;
            workLight.color = new Color(1f, 0.76f, 0.52f);
            workLight.shadows = LightShadows.None;
            workLight.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            EditorUtility.SetDirty(workLight);

            // Pendant: a single hard-shadowed, slightly cool neutral key light
            // covers the centre/bed/storage. Hard shadows are cheaper than the
            // original soft-shadowed point light, and prevent wall light leaks.
            Undo.RecordObject(pendantLight, "Retune v15 pendant light");
            pendantLight.type = LightType.Point;
            pendantLight.range = 5.4f;
            pendantLight.intensity = 1.45f;
            pendantLight.color = new Color(0.82f, 0.89f, 1f);
            pendantLight.shadows = LightShadows.Hard;
            pendantLight.shadowStrength = 0.68f;
            EditorUtility.SetDirty(pendantLight);

            ApplyBulb(workBulb, "M_V15_WorkbenchBulb",
                new Color(0.95f, 0.77f, 0.55f), new Color(1.7f, 1.1f, 0.5f));
            ApplyBulb(pendantBulb, "M_V15_PendantBulb",
                new Color(0.83f, 0.89f, 0.95f), new Color(1.1f, 1.4f, 1.8f));

            // Stove: keep its original fuel-dependent and save-dependent enable
            // logic, but focus the hard-shadowed fire on the cooking corner.
            // This also fixes v15.1's old glowOff material reference so the
            // stove door returns to the new blackened metal when fire goes out.
            Stove stove = Object.FindFirstObjectByType<Stove>(FindObjectsInactive.Include);
            if (stove != null)
            {
                Undo.RecordObject(stove, "Retune v15 stove lighting");
                if (stove.fireLight != null)
                {
                    Undo.RecordObject(stove.fireLight, "Focus v15 stove firelight");
                    stove.fireLight.range = 3.8f;
                    stove.fireLight.intensity = 1.8f;
                    stove.fireLight.color = new Color(1f, 0.51f, 0.27f);
                    stove.fireLight.shadows = LightShadows.Hard;
                    stove.fireLight.shadowStrength = 0.7f;
                    EditorUtility.SetDirty(stove.fireLight);
                }

                Transform door = stove.transform.Find("Stove Door");
                Renderer doorRenderer = door != null ? door.GetComponent<Renderer>() : null;
                if (doorRenderer != null)
                {
                    Material matteStove = MaterialKit.Lit(
                        "M_V15_Interior_BlackenedStove", "cast_metal", 0.95f,
                        0.19f, 0.58f, new Color(0.48f, 0.48f, 0.48f));
                    stove.glowOff = matteStove;
                    if (!stove.IsLit)
                    {
                        Undo.RecordObject(doorRenderer, "Fix unlit stove metal");
                        doorRenderer.sharedMaterial = matteStove;
                        EditorUtility.SetDirty(doorRenderer);
                    }
                }
                EditorUtility.SetDirty(stove);
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Afterrust] AR-1502: shelter lighting retuned." +
                      (repaired ? " Missing interior fixtures recovered without rebuilding the yard." :
                          " Existing interior fixtures reused.") +
                      " Only the pendant and optional stove cast shadows.");
        }

        private static Transform FindWorkbenchLamp(Transform lamps)
        {
            Transform best = null;
            float bestDistance = 1.2f * 1.2f;
            foreach (Light light in lamps.GetComponentsInChildren<Light>(true))
            {
                Transform fixture = light.transform.parent;
                if (fixture == null || !fixture.name.StartsWith("Wall Lamp")) continue;
                float distance = (fixture.position - WorkbenchLampPosition).sqrMagnitude;
                if (distance >= bestDistance) continue;
                best = fixture;
                bestDistance = distance;
            }
            // A scene may contain the fixture mesh but not its Light child.
            if (best != null) return best;
            foreach (Transform t in lamps.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Wall Lamp")) continue;
                float distance = (t.position - WorkbenchLampPosition).sqrMagnitude;
                if (distance >= bestDistance) continue;
                best = t;
                bestDistance = distance;
            }
            return best;
        }

        private static Transform FindPendantLamp(Transform lamps)
        {
            Transform pendant = lamps.Find("Pendant Lamp");
            if (pendant != null) return pendant;
            foreach (Transform t in lamps.GetComponentsInChildren<Transform>(true))
            {
                if (t == lamps || !t.name.Contains("Pendant")) continue;
                if ((t.position - new Vector3(0.3f, 2.45f, 3f)).sqrMagnitude < 1.5f * 1.5f)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Only missing shelter fixtures are constructed; existing yard lamps
        /// and their serialized transforms are never destroyed or recreated.
        /// </summary>
        private static Transform MakeFixture(Transform parent, string name, Vector3 position, bool pendant)
        {
            Transform fixture = new GameObject(name).transform;
            fixture.SetParent(parent, false);
            fixture.position = position;
            fixture.rotation = pendant ? Quaternion.identity :
                Quaternion.LookRotation(Vector3.back, Vector3.up);
            Undo.RegisterCreatedObjectUndo(fixture.gameObject, "Restore " + name);

            Material housing = MaterialKit.Plain("M_V15_Interior_LampHousing",
                new Color(0.15f, 0.15f, 0.14f), 0.17f, 0.44f);
            if (pendant)
            {
                MakePart(fixture, "Cord", PrimitiveType.Cylinder, housing,
                    new Vector3(0f, 0.30f, 0f), new Vector3(0.01f, 0.3f, 0.01f));
                MakePart(fixture, "Shade", PrimitiveType.Cylinder, housing,
                    new Vector3(0f, 0.04f, 0f), new Vector3(0.30f, 0.03f, 0.30f));
            }
            else
            {
                MakePart(fixture, "Housing", PrimitiveType.Cube, housing,
                    new Vector3(0f, 0f, 0.10f), new Vector3(0.30f, 0.12f, 0.20f));
            }

            EnsureLight(fixture, pendant);
            EnsureBulb(fixture, pendant);
            return fixture;
        }

        private static Transform MakePart(Transform parent, string name, PrimitiveType primitive,
            Material material, Vector3 pos, Vector3 scale)
        {
            GameObject go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go.transform;
        }

        private static Light EnsureLight(Transform fixture, bool pendant)
        {
            Light existing = fixture.GetComponentInChildren<Light>(true);
            if (existing != null) return existing;
            GameObject go = new GameObject("Light");
            go.transform.SetParent(fixture, false);
            go.transform.localPosition = pendant
                ? new Vector3(0f, -0.10f, 0f)
                : new Vector3(0f, -0.25f, 0.35f);
            Undo.RegisterCreatedObjectUndo(go, "Repair interior light");
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            // The weather controller activates the light at night.
            light.enabled = false;
            return light;
        }

        private static Transform EnsureBulb(Transform fixture, bool pendant)
        {
            Transform existing = fixture.Find("Bulb");
            if (existing != null && existing.GetComponent<Renderer>() != null)
                return existing;

            Material housing = MaterialKit.Plain("M_V15_Interior_LampHousing",
                new Color(0.15f, 0.15f, 0.14f), 0.17f, 0.44f);
            Transform bulb = MakePart(fixture, "Bulb", pendant
                    ? PrimitiveType.Sphere : PrimitiveType.Cube, housing,
                pendant ? Vector3.zero : new Vector3(0f, -0.07f, 0.12f),
                pendant ? Vector3.one * 0.08f : new Vector3(0.22f, 0.03f, 0.14f));
            return bulb;
        }

        private static void SyncWeatherReferences(Transform lampRoot)
        {
            DayNightWeather weather = Object.FindFirstObjectByType<DayNightWeather>(FindObjectsInactive.Include);
            if (weather == null)
            {
                Debug.LogWarning("[Afterrust] AR-1502: no DayNightWeather component; " +
                    "interior lamp control will be unavailable until WeatherSetup runs.");
                return;
            }

            List<Light> lights = new List<Light>();
            foreach (Light light in weather.lamps ?? new Light[0])
                if (light != null && !light.transform.IsChildOf(lampRoot) &&
                    !lights.Contains(light)) lights.Add(light);
            foreach (Light light in lampRoot.GetComponentsInChildren<Light>(true))
                if (light != null && !lights.Contains(light)) lights.Add(light);

            List<Renderer> bulbs = new List<Renderer>();
            foreach (Renderer bulb in weather.lampBulbs ?? new Renderer[0])
                if (bulb != null && !bulb.transform.IsChildOf(lampRoot) &&
                    !bulbs.Contains(bulb)) bulbs.Add(bulb);
            foreach (Renderer bulb in lampRoot.GetComponentsInChildren<Renderer>(true))
                if (bulb.name == "Bulb" && !bulbs.Contains(bulb)) bulbs.Add(bulb);

            Undo.RecordObject(weather, "Synchronize repaired interior lamps with weather");
            weather.lamps = lights.ToArray();
            weather.lampBulbs = bulbs.ToArray();
            EditorUtility.SetDirty(weather);
        }

        private static void ApplyBulb(Transform bulb, string assetName, Color tint, Color emission)
        {
            if (bulb == null)
                throw new System.InvalidOperationException("V15.2: an interior lamp bulb is missing.");
            Renderer renderer = bulb.GetComponent<Renderer>();
            if (renderer == null)
                throw new System.InvalidOperationException("V15.2: bulb has no renderer.");
            Material material = MaterialKit.Emissive(assetName, tint, emission);
            if (renderer.sharedMaterial != material)
            {
                Undo.RecordObject(renderer, "Update shelter lamp glass");
                renderer.sharedMaterial = material;
                EditorUtility.SetDirty(renderer);
            }
        }
    }
}
#endif
