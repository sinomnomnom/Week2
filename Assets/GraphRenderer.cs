using System;
using System.Collections.Generic;
using OccaSoftware.DebugDraw;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class Node
{
    public enum NodeType
    {
        INPUT_ON, INPUT_OFF, NORMAL, OUTPUT_ON, OUTPUT_OFF
    }
    public NodeType nodeType;
    public Vector2 position;
    public List<Node> connectedNodes;

    public bool active = false;
    public bool wasActive = false;

    public void Activate()
    {
        if (!active)
        {
            wasActive = active;
            active = true;
        }
    }

    public void Deactivate()
    {
        wasActive = active;
        active = false;
    }

    public void Simulate1()
    {
        wasActive = active;
    }

    public void Simulate2()
    {
        switch (nodeType)
        {
            case NodeType.NORMAL or NodeType.OUTPUT_ON or NodeType.OUTPUT_OFF:
                int count = 0;
                foreach (Node node in connectedNodes)
                {
                    if (node.wasActive)
                    {
                        count++;
                    }
                }

                

                if (count % 2 == 0)
                {
                    Deactivate();
                }
                else
                {
                    Activate();
                }
                break;
            case NodeType.INPUT_ON:
                Activate();
                break;
            case NodeType.INPUT_OFF:
                Deactivate();
                break;
        }
    }
    
    public Node(Vector2 position,  List<Node> connectedNodes = null, NodeType nodeType = NodeType.NORMAL)
    {
        this.position = position;
        if (connectedNodes != null)
        {
            this.connectedNodes = connectedNodes;
        }
        else
        {
            this.connectedNodes = new List<Node>();
        }

        if (connectedNodes != null && connectedNodes.Count != 0)
        {
            foreach (Node node in connectedNodes)
            {
                node.AddConnection(this);
            }
        }
        
        this.nodeType = nodeType;
    }

    public void AddConnection(Node node)
    {
        if (!connectedNodes.Contains(node) && node != this) connectedNodes.Add(node);
    }

    public void RemoveConnection(Node node)
    {
        connectedNodes.Remove(node);
    }
}

public class Graph
{
    public List<Node> nodes;

    public Graph(List<Node> nodes = null)
    {
        if (nodes == null) this.nodes = new List<Node>();
        else this.nodes = nodes;
    }

    public Node AddConnectedNode(Node connectedNode, Vector2 position)
    {
        Node newNode = new Node(position);
        nodes.Add(newNode);
        AddConnection(newNode, connectedNode);
        return newNode;
    }

    public void RemoveNode(Node node)
    {
        nodes.Remove(node);
        foreach (Node otherNode in nodes)
        {
            otherNode.RemoveConnection(node);
        }
    }

    public void AddConnection(Node node1, Node node2)
    {
        node1.AddConnection(node2);
        node2.AddConnection(node1);
    }

    public void RemoveConnection(Node node1, Node node2)
    {
        node1.RemoveConnection(node2);
        node2.RemoveConnection(node1);
    }

    public Node GetRandomNode()
    {
        return nodes[UnityEngine.Random.Range(0, nodes.Count)];
    }

    public void Deactivate()
    {
        foreach (Node node in nodes)
        {
            node.Deactivate();
        }
    }
}
public class GraphRenderer : MonoBehaviour
{
    public Graph graph;
    public Color color = Color.white;
    public Color handleColor = Color.gray;
    public bool showNodes = true;
    float nodeSize = 0.2f;
    public float waveStrength = 1.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        RenderGraph();
    }

    private void RenderGraph()
    {
        foreach (Node from in graph.nodes)
        {
            foreach (Node to in from.connectedNodes)
            {
                Draw.Line(WavePos(from.position), WavePos(to.position), color);
            }
            DrawNode(from);
        }
    }

    public void DrawNode(Node node)
    {
        if(showNodes) Draw.Box(node.position,new Vector3(nodeSize,nodeSize,nodeSize), handleColor);
        if (node.active)
        {
            Draw.Sphere(WavePos(node.position),nodeSize/2, color);
        }

        switch (node.nodeType)
        {
            case Node.NodeType.INPUT_ON:
                Draw.Sphere(WavePos(node.position),(MathF.Sin(Time.realtimeSinceStartup)+2)*nodeSize/2, Color.lawnGreen);
                break;
            case Node.NodeType.INPUT_OFF:
                Draw.Sphere(WavePos(node.position),(MathF.Sin(Time.realtimeSinceStartup)+2)*nodeSize/2, Color.darkRed);
                break;
            case Node.NodeType.OUTPUT_OFF:
                Draw.Box(WavePos(node.position),new Vector3((MathF.Sin(Time.realtimeSinceStartup)+1)*nodeSize/2,(MathF.Sin(Time.realtimeSinceStartup)+2)*nodeSize), Color.darkRed);
                break;
            case Node.NodeType.OUTPUT_ON:
                Draw.Box(WavePos(node.position),new Vector3((MathF.Sin(Time.realtimeSinceStartup)+1)*nodeSize/2,(MathF.Sin(Time.realtimeSinceStartup)+2)*nodeSize), Color.lawnGreen);
                break;
                
        }
    }

    public void SetRenderNodes(bool renderNodes)
    {
        showNodes = renderNodes;
    }

    private Vector2 WavePos(Vector2 position)
    {
        return position + new Vector2(Mathf.Cos(Time.time + position.x), Mathf.Cos(Time.time + position.y))*waveStrength;
    }
}
