#if UNITY_EDITOR
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
                throw new System.InvalidOperationException("V15.2: Scrapyard Lamps not found.");

            Transform workbench = FindWorkbenchLamp(root);
            Transform pendant = root.Find("Pendant Lamp");
            if (workbench == null || pendant == null)
                throw new System.InvalidOperationException("V15.2: interior wall lamp or pendant not found.");

            Light workLight = workbench.Find("Light") != null
                ? workbench.Find("Light").GetComponent<Light>() : null;
            Light pendantLight = pendant.Find("Light") != null
                ? pendant.Find("Light").GetComponent<Light>() : null;
            if (workLight == null || pendantLight == null)
                throw new System.InvalidOperationException("V15.2: interior lamp Light components missing.");

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

            ApplyBulb(workbench.Find("Bulb"), "M_V15_WorkbenchBulb",
                new Color(0.95f, 0.77f, 0.55f), new Color(1.7f, 1.1f, 0.5f));
            ApplyBulb(pendant.Find("Bulb"), "M_V15_PendantBulb",
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
            Debug.Log("[Afterrust] AR-1502: shelter lighting retuned. " +
                      "No extra lights; one fewer shadow-casting wall light.");
        }

        private static Transform FindWorkbenchLamp(Transform lamps)
        {
            for (int i = 0; i < lamps.childCount; i++)
            {
                Transform candidate = lamps.GetChild(i);
                if (candidate.name != "Wall Lamp") continue;
                if ((candidate.position - WorkbenchLampPosition).sqrMagnitude < 0.35f * 0.35f)
                    return candidate;
            }
            return null;
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
