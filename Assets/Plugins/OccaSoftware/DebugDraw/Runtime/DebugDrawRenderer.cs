using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OccaSoftware.DebugDraw
{
    // Runtime singleton that collects queued primitives and renders them.
    //
    // Shapes are batched into a dynamic line mesh and submitted with
    // Graphics.RenderMesh. That deliberately avoids GL immediate mode: GL drawing
    // from Camera.onPostRender / RenderPipelineManager.endCameraRendering only
    // lands in the built-in pipeline. Under URP the callback fires but the bound
    // target is no longer the final framebuffer, so the shapes silently vanish
    // (verified in a URP player build). RenderMesh goes through the normal
    // renderer path, which every pipeline honours.
    //
    // Auto-creates on first Draw call; zero cost if never used. Text uses IMGUI
    // world-to-screen, which is pipeline-independent already.
    [AddComponentMenu("")]
    public sealed class DebugDrawRenderer : MonoBehaviour
    {
        struct LineItem { public Vector3 a, b; public Color color; public bool depthTest; public float expiry; }
        struct TextItem { public Vector3 pos; public string text; public Color color; public float expiry; }

        static DebugDrawRenderer _instance;
        public static DebugDrawRenderer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[DebugDraw]") { hideFlags = HideFlags.HideAndDontSave };
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<DebugDrawRenderer>();
                }
                return _instance;
            }
        }

        readonly List<LineItem> _lines = new List<LineItem>(256);
        readonly List<TextItem> _texts = new List<TextItem>(32);
        GUIStyle _style;

        // One material/mesh per depth mode: depth-tested shapes are occluded by
        // scene geometry, overlay shapes always draw on top.
        Material _matDepthTested, _matOverlay;
        Mesh _meshDepthTested, _meshOverlay;

        // Scratch buffers, reused every frame so rebuilding the mesh does not
        // allocate (this is a debug tool; it must not create the garbage that
        // people use debug tools to hunt down).
        readonly List<Vector3> _verts = new List<Vector3>(512);
        readonly List<Color> _colors = new List<Color>(512);
        readonly List<int> _indices = new List<int>(512);

        // Debug shapes are positioned in world space and must never be culled by
        // a camera's frustum test against the mesh bounds.
        static readonly Bounds AlwaysVisible = new Bounds(Vector3.zero, Vector3.one * 1e9f);

        public void AddLine(Vector3 a, Vector3 b, Color color, float duration, bool depthTest)
            => _lines.Add(new LineItem { a = a, b = b, color = color, depthTest = depthTest, expiry = Time.unscaledTime + Mathf.Max(0f, duration) });

        public void AddText(Vector3 pos, string text, Color color, float duration)
            => _texts.Add(new TextItem { pos = pos, text = text, color = color, expiry = Time.unscaledTime + Mathf.Max(0f, duration) });

        void OnEnable()
        {
            // Shipped in a Resources folder so it is always included in a player
            // build. Shader.Find on a built-in shader is not safe here: those get
            // stripped, and the shapes then never draw in a build.
            var shader = Resources.Load<Shader>("DebugDrawLines");
            if (shader == null)
            {
                Debug.LogError("[DebugDraw] line shader missing; shapes cannot be drawn.");
                return;
            }

            // Queue after opaque geometry so depth-tested lines test against a
            // complete depth buffer; the overlay pass goes later still.
            _matDepthTested = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = (int)RenderQueue.Transparent,
            };
            _matDepthTested.SetInt("_ZTest", (int)CompareFunction.LessEqual);

            _matOverlay = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = (int)RenderQueue.Overlay,
            };
            _matOverlay.SetInt("_ZTest", (int)CompareFunction.Always);

            _meshDepthTested = NewMesh();
            _meshOverlay = NewMesh();
        }

        static Mesh NewMesh()
        {
            var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            mesh.MarkDynamic(); // rebuilt every frame
            return mesh;
        }

        void OnDisable()
        {
            if (_matDepthTested != null) DestroyImmediate(_matDepthTested);
            if (_matOverlay != null) DestroyImmediate(_matOverlay);
            if (_meshDepthTested != null) DestroyImmediate(_meshDepthTested);
            if (_meshOverlay != null) DestroyImmediate(_meshOverlay);
        }

        void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _lines.Count - 1; i >= 0; i--) if (now > _lines[i].expiry) _lines.RemoveAt(i);
            for (int i = _texts.Count - 1; i >= 0; i--) if (now > _texts[i].expiry) _texts.RemoveAt(i);
        }

        // Submitted from LateUpdate so every shape queued during this frame's
        // Update is included.
        void LateUpdate()
        {
            if (_lines.Count == 0) return;
            SubmitPass(true, _meshDepthTested, _matDepthTested);
            SubmitPass(false, _meshOverlay, _matOverlay);
        }

        void SubmitPass(bool depthTested, Mesh mesh, Material material)
        {
            if (mesh == null || material == null) return;

            _verts.Clear();
            _colors.Clear();
            _indices.Clear();

            for (int i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                if (line.depthTest != depthTested) continue;
                _indices.Add(_verts.Count);
                _verts.Add(line.a);
                _indices.Add(_verts.Count);
                _verts.Add(line.b);
                _colors.Add(line.color);
                _colors.Add(line.color);
            }

            mesh.Clear();
            if (_verts.Count == 0) return;

            mesh.SetVertices(_verts);
            mesh.SetColors(_colors);
            mesh.SetIndices(_indices, MeshTopology.Lines, 0);
            mesh.bounds = AlwaysVisible;

            var parameters = new RenderParams(material)
            {
                worldBounds = AlwaysVisible,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
            };
            Graphics.RenderMesh(parameters, mesh, 0, Matrix4x4.identity);
        }

        void OnGUI()
        {
            if (_texts.Count == 0) return;
            var cam = Camera.main;
            if (cam == null) return;
            if (_style == null) { _style = new GUIStyle(GUI.skin.label); _style.alignment = TextAnchor.MiddleCenter; _style.fontSize = 12; }
            for (int i = 0; i < _texts.Count; i++)
            {
                var t = _texts[i];
                Vector3 sp = cam.WorldToScreenPoint(t.pos);
                if (sp.z <= 0f) continue;
                _style.normal.textColor = t.color;
                var content = new GUIContent(t.text);
                Vector2 size = _style.CalcSize(content);
                GUI.Label(new Rect(sp.x - size.x * 0.5f, (Screen.height - sp.y) - size.y * 0.5f, size.x, size.y), content, _style);
            }
        }
    }
}
