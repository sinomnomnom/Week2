using System.Collections.Generic;
using OccaSoftware.DebugDraw;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GraphEditor : MonoBehaviour
{
    public enum EditMode
    {
        MOVE, ADD
    }

    public Dictionary<EditMode, Color> EditColors = new Dictionary<EditMode, Color>
    {
        { EditMode.MOVE, Color.darkGreen },
        { EditMode.ADD, Color.yellow },
    };
    
    public Graph graph;
    public bool editable = true;
    public float editRadius = 0.5f;
    public EditMode editMode = EditMode.MOVE;
    
    private Node hoveredNode;
    private Node selectedNode;
    private Camera cam;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (editable)
        {
            Vector3 mousePos = GetMousePosition();
            
            float minRadius = editRadius;
            hoveredNode = null;
            foreach (Node node in graph.nodes)
            {
                if (Vector2.Distance(mousePos, node.position) < minRadius)
                {
                    minRadius = Vector2.Distance(mousePos, node.position);
                    hoveredNode = node;
                }
            }

            if (hoveredNode != null)
            {
                Draw.Sphere(hoveredNode.position, editRadius, EditColors[editMode]);
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    selectedNode = hoveredNode;
                }
            }

            if (selectedNode != null)
            {
                switch (editMode)
                {
                    case EditMode.ADD:
                        Draw.Line(selectedNode.position, mousePos, EditColors[EditMode.MOVE]);
                        break;
                    case EditMode.MOVE:
                        selectedNode.position = mousePos;
                        break;
                    default:
                        break;
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    switch (editMode)
                    {
                        case EditMode.ADD:
                            graph.AddConnectedNode(selectedNode,mousePos);
                            break;
                        case EditMode.MOVE:
                            break;
                        default:
                            break;
                    }
                    selectedNode = null;
                }
            }
        }
    }

    public Vector3 GetMousePosition()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;
        return mousePos;
    }
}
