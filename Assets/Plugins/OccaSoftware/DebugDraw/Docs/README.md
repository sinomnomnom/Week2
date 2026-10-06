# Debug Draw

Persistent debug drawing that shows up in **player builds and the Game view**, not
just the editor Scene view. Draw lines, rays, boxes, spheres, and labels from
anywhere in your code — no prefab, no setup — and have them render for a frame or
for a set duration, depth-tested or on top.

## Quick start
```csharp
using OccaSoftware.DebugDraw;

Draw.Line(transform.position, target.position, Color.red);        // one frame
Draw.Ray(transform.position, transform.forward * 5f, Color.cyan, 2f); // 2 seconds
Draw.Box(hit.point, Vector3.one * 0.2f, Color.green, 1f);
Draw.Sphere(explosion, radius, Color.yellow, 0f, depthTest:false); // drawn on top
Draw.Text(enemy.position + Vector3.up * 2f, enemy.name, Color.white, 1f);
```
That's it — the renderer auto-creates on first use and costs nothing if unused.

## API
`Draw.Line · Draw.Ray · Draw.Box · Draw.Sphere · Draw.Text` — each takes a color,
an optional `duration` (0 = one frame), and (shapes) a `depthTest` flag.

## Works everywhere
Renders via GL in the **Built-in pipeline and URP / HDRP** (all render pipelines),
in the editor and in Development or release builds. Namespaced + asmdef-isolated.

---
Building runtime tools? Pair this with **Ninja Profiler Pro** — an on-screen FPS,
memory, GC, and draw-call HUD plus a runtime debug console.
https://assetstore.unity.com/packages/slug/391394
