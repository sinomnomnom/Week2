using System.Collections.Generic;
using OccaSoftware.DebugDraw;
using UnityEngine;
using UnityEditor;

public class Node
{
    public Vector2 position;
    public List<Node> connectedNodes;
    
    public Node(Vector2 position,  List<Node> connectedNodes = null)
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
    }

    public void AddConnection(Node node)
    {
        if (!connectedNodes.Contains(node)) connectedNodes.Add(node);
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
        connectedNode.AddConnection(newNode);
        return newNode;
    }
}
public class GraphRenderer : MonoBehaviour
{
    public Graph graph;
    public Color color = Color.white;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Node> nodes = new List<Node>();
        Node originNode = new Node(new Vector2(0,0));
        Node node2 = new Node(new Vector2(0,1));
        Node node3 = new Node(new Vector2(1,0));
        originNode.AddConnection(node2);
        originNode.AddConnection(node3);
        nodes.Add(originNode);
        nodes.Add(node2);
        nodes.Add(node3);
        graph = new Graph(nodes);
        
        GetComponent<GraphEditor>().graph = graph;
    }

    void Update()
    {
        foreach (Node from in graph.nodes)
        {
            foreach (Node to in from.connectedNodes)
            {
                Draw.Line(from.position, to.position, color);
            }
        }
    }
}
