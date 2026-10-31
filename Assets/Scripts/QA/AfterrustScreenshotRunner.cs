#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Afterrust.QA
{
    /// <summary>
    /// Captures planned viewpoints whenever Play mode starts in the Editor.
    /// Captures do not move the player and only replace files in QA/Screenshots/latest.
    /// </summary>
    public sealed class AfterrustScreenshotRunner : MonoBehaviour
    {
        [Serializable]
        private sealed class ScreenshotPlan
        {
            public bool enabled = true;
            public int width = 1280;
            public int height = 720;
            public float delaySeconds = 2f;
            public ScreenshotView[] views;
        }

        [Serializable]
        private sealed class ScreenshotView
        {
            public string id;
            public string description;
            public Vector3 position;
            public Vector3 rotation;
            public float fieldOfView = 65f;
        }

        [Serializable]
        private sealed class CaptureManifest
        {
            public string capturedAtUtc;
            public string unityVersion;
            public string scene;
            public string planAsset;
            public int imageCount;
            public List<CaptureEntry> images = new List<CaptureEntry>();
        }

        [Serializable]
        private sealed class CaptureEntry
        {
            public string file;
            public string description;
            public Vector3 position;
            public Vector3 rotation;
        }

        private const string PlanResourceName = "qa_screenshot_plan";
        private const string OutputFolder = "QA/Screenshots/latest";
        private static AfterrustScreenshotRunner active;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (active != null) return;
            GameObject host = new GameObject("Afterrust QA Screenshot Runner (Editor Only)");
            DontDestroyOnLoad(host);
            active = host.AddComponent<AfterrustScreenshotRunner>();
        }

        private void Start()
        {
            StartCoroutine(CaptureOnStart());
        }

        private IEnumerator CaptureOnStart()
        {
            TextAsset asset = Resources.Load<TextAsset>(PlanResourceName);
            if (asset == null)
            {
                Debug.LogWarning("[Afterrust QA] Missing Resources/qa_screenshot_plan.json.");
                yield break;
            }

            ScreenshotPlan plan;
            try
            {
                plan = JsonUtility.FromJson<ScreenshotPlan>(asset.text);
            }
            catch (Exception e)
            {
                Debug.LogError("[Afterrust QA] Screenshot plan JSON invalid: " + e.Message);
                yield break;
            }

            if (plan == null || !plan.enabled || plan.views == null || plan.views.Length == 0)
                yield break;

            yield return new WaitForSecondsRealtime(Mathf.Clamp(plan.delaySeconds, 0.1f, 30f));
            Capture(plan);
        }

        private static void Capture(ScreenshotPlan plan)
        {
            Camera source = Camera.main;
            if (source == null)
            {
                Debug.LogWarning("[Afterrust QA] No MainCamera available. No screenshots taken.");
                return;
            }

            int width = Mathf.Clamp(plan.width, 320, 1920);
            int height = Mathf.Clamp(plan.height, 180, 1080);
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputFolder));

            GameObject cameraObject = new GameObject("QA Temporary Capture Camera");
            Camera shotCamera = cameraObject.AddComponent<Camera>();
            shotCamera.CopyFrom(source);
            shotCamera.enabled = false;
            shotCamera.targetTexture = null;

            RenderTexture renderTexture = null;
            Texture2D screenshot = null;
            RenderTexture previous = RenderTexture.active;

            try
            {
                // Only clean our own generated screenshots. Other files are never deleted.
                Directory.CreateDirectory(output);
                foreach (string path in Directory.GetFiles(output, "*.png", SearchOption.TopDirectoryOnly))
                    File.Delete(path);
                string manifestPath = Path.Combine(output, "manifest.json");
                if (File.Exists(manifestPath)) File.Delete(manifestPath);

                renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
                shotCamera.targetTexture = renderTexture;

                CaptureManifest manifest = new CaptureManifest
                {
                    capturedAtUtc = DateTime.UtcNow.ToString("o"),
                    unityVersion = Application.unityVersion,
                    scene = SceneManager.GetActiveScene().name,
                    planAsset = "Assets/Resources/qa_screenshot_plan.json"
                };

                HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < plan.views.Length; i++)
                {
                    ScreenshotView view = plan.views[i];
                    if (view == null) continue;
                    string id = Regex.Replace(view.id ?? "", "[^a-zA-Z0-9_-]", "_");
                    if (string.IsNullOrWhiteSpace(id)) id = "view_" + (i + 1);
                    if (!usedNames.Add(id)) id += "_" + i;

                    shotCamera.transform.SetPositionAndRotation(
                        view.position, Quaternion.Euler(view.rotation));
                    shotCamera.fieldOfView = Mathf.Clamp(view.fieldOfView, 25f, 100f);
                    shotCamera.Render();
                    RenderTexture.active = renderTexture;
                    screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                    screenshot.Apply(false, false);

                    string name = id + ".png";
                    File.WriteAllBytes(Path.Combine(output, name), screenshot.EncodeToPNG());
                    manifest.images.Add(new CaptureEntry
                    {
                        file = name,
                        description = view.description,
                        position = view.position,
                        rotation = view.rotation
                    });
                }

                manifest.imageCount = manifest.images.Count;
                File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
                Debug.Log("[Afterrust QA] Saved " + manifest.imageCount +
                    " screenshots to " + output +
                    ". GitHub Desktop will show added/modified/deleted files.");
            }
            catch (Exception e)
            {
                Debug.LogError("[Afterrust QA] Capture failed: " + e);
            }
            finally
            {
                RenderTexture.active = previous;
                if (shotCamera != null) shotCamera.targetTexture = null;
                if (renderTexture != null) { renderTexture.Release(); Destroy(renderTexture); }
                if (screenshot != null) Destroy(screenshot);
                if (cameraObject != null) Destroy(cameraObject);
            }
        }
    }
}
#endif
