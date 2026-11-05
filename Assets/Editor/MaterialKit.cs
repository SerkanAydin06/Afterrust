#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Afterrust.EditorTools
{
    /// <summary>Standard-shader materials (built-in pipeline) created or updated in place.</summary>
    internal static class MaterialKit
    {
        internal const string TexDir = "Assets/Arts/Environment/Textures";
        internal const string MatDir = "Assets/Arts/Environment/Materials";

        /// <summary>Textured material for meter-UV meshes: tiling = 1 / worldSize.</summary>
        internal static Material Lit(string name, string texture, float worldSize, float smoothness, float metallic,
            Color? tint = null, Vector2? tiling = null)
        {
            Material m = Get(MatDir, name);
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(TexDir + "/" + texture + "_albedo.jpg");
            Texture2D normal = NormalMap(TexDir + "/" + texture + "_normal.jpg");
            m.SetTexture("_MainTex", albedo);
            m.SetTextureScale("_MainTex", tiling ?? Vector2.one / Mathf.Max(worldSize, 0.0001f));
            m.color = tint ?? Color.white;
            SetNormal(m, normal);
            m.SetFloat("_Glossiness", smoothness);
            m.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(m);
            return m;
        }

        internal static Material Plain(string name, Color color, float smoothness, float metallic)
        {
            Material m = Get(MatDir, name);
            m.SetTexture("_MainTex", null);
            m.color = color;
            SetNormal(m, null);
            m.SetFloat("_Glossiness", smoothness);
            m.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(m);
            return m;
        }

        internal static Material Emissive(string name, Color color, Color emission)
        {
            Material m = Plain(name, color, 0.3f, 0f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Character material from explicit texture paths (0..1 UVs).</summary>
        internal static Material Character(string folder, string name, string albedoPath, string normalPath,
            bool cutout, float smoothness)
        {
            Material m = Get(folder, name);
            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            if (cutout && albedo != null)
            {
                TextureImporter ti = AssetImporter.GetAtPath(albedoPath) as TextureImporter;
                if (ti != null && !ti.alphaIsTransparency)
                {
                    ti.alphaIsTransparency = true;
                    ti.SaveAndReimport();
                    albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
                }
            }
            m.SetTexture("_MainTex", albedo);
            m.SetTextureScale("_MainTex", Vector2.one);
            m.color = Color.white;
            SetNormal(m, string.IsNullOrEmpty(normalPath) ? null : NormalMap(normalPath));
            m.SetFloat("_Glossiness", smoothness);
            m.SetFloat("_Metallic", 0f);
            SetCutout(m, cutout);
            EditorUtility.SetDirty(m);
            return m;
        }

        internal static Texture2D NormalMap(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void SetNormal(Material m, Texture2D normal)
        {
            m.SetTexture("_BumpMap", normal);
            if (normal != null) m.EnableKeyword("_NORMALMAP");
            else m.DisableKeyword("_NORMALMAP");
        }

        private static void SetCutout(Material m, bool cutout)
        {
            m.SetFloat("_Mode", cutout ? 1f : 0f);
            m.SetOverrideTag("RenderType", cutout ? "TransparentCutout" : "");
            m.SetInt("_SrcBlend", (int)BlendMode.One);
            m.SetInt("_DstBlend", (int)BlendMode.Zero);
            m.SetInt("_ZWrite", 1);
            m.DisableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if (cutout) m.EnableKeyword("_ALPHATEST_ON");
            else m.DisableKeyword("_ALPHATEST_ON");
            m.SetFloat("_Cutoff", 0.35f);
            m.renderQueue = cutout ? (int)RenderQueue.AlphaTest : -1;
        }

        private static Material Get(string folder, string name)
        {
            EnsureFolder(folder);
            string path = folder + "/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader standard = Shader.Find("Standard");
            if (m == null)
            {
                m = new Material(standard) { name = name };
                AssetDatabase.CreateAsset(m, path);
            }
            else if (m.shader != standard) m.shader = standard;
            return m;
        }

        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
#endif
