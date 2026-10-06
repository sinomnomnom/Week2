using UnityEngine;

namespace OccaSoftware.DebugDraw
{
    // Persistent debug drawing that renders in PLAYER BUILDS and the Game view, not
    // just the editor Scene view (unlike UnityEngine.Debug.DrawLine). Call from
    // anywhere — no setup, no prefab. Every call optionally persists for `duration`
    // seconds and can be depth-tested (occluded by geometry) or drawn on top.
    public static class Draw
    {
        public static void Line(Vector3 a, Vector3 b, Color color, float duration = 0f, bool depthTest = true)
            => DebugDrawRenderer.Instance.AddLine(a, b, color, duration, depthTest);

        public static void Ray(Vector3 origin, Vector3 direction, Color color, float duration = 0f, bool depthTest = true)
            => DebugDrawRenderer.Instance.AddLine(origin, origin + direction, color, duration, depthTest);

        public static void Box(Vector3 center, Vector3 size, Color color, float duration = 0f, bool depthTest = true)
            => Box(center, size, Quaternion.identity, color, duration, depthTest);

        public static void Box(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration = 0f, bool depthTest = true)
        {
            Vector3 h = size * 0.5f;
            Vector3[] c = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3((i & 1) == 0 ? -h.x : h.x, (i & 2) == 0 ? -h.y : h.y, (i & 4) == 0 ? -h.z : h.z);
                c[i] = center + rotation * corner;
            }
            var r = DebugDrawRenderer.Instance;
            // bottom (y-), top (y+), verticals
            r.AddLine(c[0], c[1], color, duration, depthTest); r.AddLine(c[1], c[5], color, duration, depthTest);
            r.AddLine(c[5], c[4], color, duration, depthTest); r.AddLine(c[4], c[0], color, duration, depthTest);
            r.AddLine(c[2], c[3], color, duration, depthTest); r.AddLine(c[3], c[7], color, duration, depthTest);
            r.AddLine(c[7], c[6], color, duration, depthTest); r.AddLine(c[6], c[2], color, duration, depthTest);
            r.AddLine(c[0], c[2], color, duration, depthTest); r.AddLine(c[1], c[3], color, duration, depthTest);
            r.AddLine(c[5], c[7], color, duration, depthTest); r.AddLine(c[4], c[6], color, duration, depthTest);
        }

        public static void Sphere(Vector3 center, float radius, Color color, float duration = 0f, bool depthTest = true, int segments = 24)
        {
            var r = DebugDrawRenderer.Instance;
            float step = Mathf.PI * 2f / segments;
            Vector3 px = Vector3.zero, py = Vector3.zero, pz = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float a = i * step; float s = Mathf.Sin(a) * radius, c = Mathf.Cos(a) * radius;
                Vector3 nx = center + new Vector3(0, s, c);   // YZ plane
                Vector3 ny = center + new Vector3(s, 0, c);   // XZ plane
                Vector3 nz = center + new Vector3(s, c, 0);   // XY plane
                if (i > 0) { r.AddLine(px, nx, color, duration, depthTest); r.AddLine(py, ny, color, duration, depthTest); r.AddLine(pz, nz, color, duration, depthTest); }
                px = nx; py = ny; pz = nz;
            }
        }

        public static void Text(Vector3 position, string text, Color color, float duration = 0f)
            => DebugDrawRenderer.Instance.AddText(position, text, color, duration);
    }
}
