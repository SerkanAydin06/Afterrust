#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Procedural meshes with UVs in meters (material tiling = 1 / texture world size),
    /// saved once as assets under Assets/Arts/Environment/Meshes and reused by name.
    /// Winding follows Unity's rule: Cross(B - A, C - A) points along the face normal.
    /// </summary>
    internal static class MeshKit
    {
        internal const string MeshDir = "Assets/Arts/Environment/Meshes";

        internal static Mesh Box(string name, Vector3 size, Vector3 center)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> t = new List<int>();
            Vector3 h = size * 0.5f;

            void Face(Vector3 normal, Vector3 u, Vector3 w, float uLen, float wLen)
            {
                Vector3 a = center + Vector3.Scale(normal, h) - u * (uLen * 0.5f) - w * (wLen * 0.5f);
                int i = v.Count;
                v.Add(a); v.Add(a + u * uLen); v.Add(a + u * uLen + w * wLen); v.Add(a + w * wLen);
                uv.Add(new Vector2(0f, 0f)); uv.Add(new Vector2(uLen, 0f));
                uv.Add(new Vector2(uLen, wLen)); uv.Add(new Vector2(0f, wLen));
                for (int k = 0; k < 4; k++) n.Add(normal);
                Quad(t, i, Vector3.Dot(Vector3.Cross(u, w), normal) > 0f);
            }

            Face(Vector3.right, Vector3.forward, Vector3.up, size.z, size.y);
            Face(Vector3.left, Vector3.back, Vector3.up, size.z, size.y);
            Face(Vector3.forward, Vector3.left, Vector3.up, size.x, size.y);
            Face(Vector3.back, Vector3.right, Vector3.up, size.x, size.y);
            Face(Vector3.up, Vector3.right, Vector3.forward, size.x, size.z);
            Face(Vector3.down, Vector3.right, Vector3.back, size.x, size.z);
            return Save(name, v, n, uv, t);
        }

        /// <summary>Capped cylinder along local Y from y = 0 to y = height.</summary>
        internal static Mesh Cylinder(string name, float radius, float height, int segments)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> t = new List<int>();
            float circumference = Mathf.PI * 2f * radius;
            for (int s = 0; s <= segments; s++)
            {
                float a = s / (float)segments * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                v.Add(dir * radius); n.Add(dir); uv.Add(new Vector2(s / (float)segments * circumference, 0f));
                v.Add(dir * radius + Vector3.up * height); n.Add(dir); uv.Add(new Vector2(s / (float)segments * circumference, height));
            }
            for (int s = 0; s < segments; s++)
            {
                int i = s * 2;
                // i bottom, i+1 top, i+2 next bottom, i+3 next top; outward normal.
                t.Add(i); t.Add(i + 1); t.Add(i + 3);
                t.Add(i); t.Add(i + 3); t.Add(i + 2);
            }
            Cap(v, n, uv, t, radius, height, segments, true);
            Cap(v, n, uv, t, radius, 0f, segments, false);
            FixWinding(v, n, t);
            return Save(name, v, n, uv, t);
        }

        /// <summary>
        /// Torus around local Y (ring in XZ). Cross-section is a superellipse so tires look
        /// boxy rather than tubular. UV: u = 0..1 around the ring, v = 0..1 around the tube.
        /// </summary>
        internal static Mesh Torus(string name, float majorRadius, float tubeWidth, float tubeHeight,
            int ringSegments, int tubeSegments, float squareness)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> t = new List<int>();
            float e = Mathf.Clamp(1f - squareness, 0.2f, 1f);
            for (int i = 0; i <= ringSegments; i++)
            {
                float a = i / (float)ringSegments * Mathf.PI * 2f;
                Vector3 radial = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                for (int j = 0; j <= tubeSegments; j++)
                {
                    float b = j / (float)tubeSegments * Mathf.PI * 2f;
                    float cb = Mathf.Cos(b), sb = Mathf.Sin(b);
                    float px = Mathf.Sign(cb) * Mathf.Pow(Mathf.Abs(cb), e) * tubeWidth * 0.5f;
                    float py = Mathf.Sign(sb) * Mathf.Pow(Mathf.Abs(sb), e) * tubeHeight * 0.5f;
                    v.Add(radial * (majorRadius + px) + Vector3.up * py);
                    n.Add((radial * cb + Vector3.up * sb).normalized);
                    uv.Add(new Vector2(i / (float)ringSegments, j / (float)tubeSegments));
                }
            }
            int stride = tubeSegments + 1;
            for (int i = 0; i < ringSegments; i++)
            for (int j = 0; j < tubeSegments; j++)
            {
                int a = i * stride + j, b = a + stride, c = b + 1, d = a + 1;
                t.Add(a); t.Add(b); t.Add(c);
                t.Add(a); t.Add(c); t.Add(d);
            }
            FixWinding(v, n, t);
            Mesh mesh = Save(name, v, n, uv, t, false);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        /// <summary>Double-sided corrugated sheet: width along X, length along Y, waves in Z.</summary>
        internal static Mesh Corrugated(string name, float width, float length, int waves, float amplitude, float start)
        {
            List<Vector3> v = new List<Vector3>();
            List<Vector3> n = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> t = new List<int>();
            int cols = waves * 6 + 1;
            for (int side = 0; side < 2; side++)
            {
                float s = side == 0 ? 1f : -1f;
                int first = v.Count;
                for (int c = 0; c < cols; c++)
                {
                    float u = c / (float)(cols - 1);
                    float x = (u - 0.5f) * width;
                    float phase = u * waves * Mathf.PI * 2f;
                    float z = Mathf.Sin(phase) * amplitude;
                    float dz = Mathf.Cos(phase) * amplitude * waves * Mathf.PI * 2f / width;
                    Vector3 normal = new Vector3(-dz, 0f, 1f).normalized * s;
                    for (int r = 0; r < 2; r++)
                    {
                        v.Add(new Vector3(x, start + r * length, z));
                        n.Add(normal);
                        uv.Add(new Vector2(u * width, r * length));
                    }
                }
                for (int c = 0; c < cols - 1; c++)
                {
                    int i = first + c * 2;
                    t.Add(i); t.Add(i + 2); t.Add(i + 3);
                    t.Add(i); t.Add(i + 3); t.Add(i + 1);
                }
            }
            FixWinding(v, n, t);
            return Save(name, v, n, uv, t);
        }

        private static void Cap(List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> t,
            float radius, float y, int segments, bool top)
        {
            Vector3 normal = top ? Vector3.up : Vector3.down;
            int centre = v.Count;
            v.Add(new Vector3(0f, y, 0f)); n.Add(normal); uv.Add(new Vector2(radius, radius));
            for (int s = 0; s <= segments; s++)
            {
                float a = s / (float)segments * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * radius, y, Mathf.Sin(a) * radius);
                v.Add(p); n.Add(normal); uv.Add(new Vector2(p.x + radius, p.z + radius));
            }
            for (int s = 0; s < segments; s++)
            {
                t.Add(centre); t.Add(centre + 1 + s); t.Add(centre + 2 + s);
            }
        }

        private static void Quad(List<int> t, int i, bool uCrossWAlongNormal)
        {
            if (uCrossWAlongNormal) { t.Add(i); t.Add(i + 1); t.Add(i + 2); t.Add(i); t.Add(i + 2); t.Add(i + 3); }
            else { t.Add(i); t.Add(i + 2); t.Add(i + 1); t.Add(i); t.Add(i + 3); t.Add(i + 2); }
        }

        /// <summary>Safety net: flips any triangle whose winding disagrees with its vertex normals.</summary>
        private static void FixWinding(List<Vector3> v, List<Vector3> n, List<int> t)
        {
            for (int i = 0; i < t.Count; i += 3)
            {
                Vector3 a = v[t[i]], b = v[t[i + 1]], c = v[t[i + 2]];
                Vector3 avgNormal = n[t[i]] + n[t[i + 1]] + n[t[i + 2]];
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), avgNormal) < 0f)
                {
                    int tmp = t[i + 1];
                    t[i + 1] = t[i + 2];
                    t[i + 2] = tmp;
                }
            }
        }

        private static Mesh Save(string name, List<Vector3> v, List<Vector3> n, List<Vector2> uv, List<int> t,
            bool tangents = true)
        {
            Mesh mesh = new Mesh { name = name };
            if (v.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(v);
            mesh.SetNormals(n);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(t, 0);
            mesh.RecalculateBounds();
            if (tangents) mesh.RecalculateTangents();

            if (!AssetDatabase.IsValidFolder(MeshDir)) AssetDatabase.CreateFolder("Assets/Arts/Environment", "Meshes");
            string path = MeshDir + "/" + name + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, existing);
            existing.name = name;
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(mesh);
            return existing;
        }
    }
}
#endif
