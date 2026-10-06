using UnityEngine;

namespace OccaSoftware.DebugDraw.Demo
{
    // Sample: draws a few primitives every frame to show Debug Draw rendering in the
    // Game view / player build. Delete this Demo folder to strip.
    public sealed class DebugDrawDemo : MonoBehaviour
    {
        void Update()
        {
            float t = Time.time;
            Draw.Box(Vector3.zero, Vector3.one, Quaternion.Euler(0f, t * 40f, 0f), Color.cyan);
            Draw.Sphere(new Vector3(2.5f, 0f, 0f), 0.6f, Color.yellow);
            Draw.Ray(new Vector3(-2.5f, 0f, 0f), Quaternion.Euler(0f, t * 90f, 0f) * Vector3.forward * 1.5f, Color.red);
            if (Time.frameCount % 6 == 0)
                Draw.Sphere(new Vector3(Mathf.Sin(t) * 3f, Mathf.Cos(t) * 1.2f + 1.2f, 0f), 0.06f, Color.green, 1.5f);
            Draw.Text(Vector3.up * 1.3f, "Debug Draw", Color.white);
        }
    }
}
