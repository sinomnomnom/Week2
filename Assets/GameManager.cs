using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Graph activeGraph;
    private bool simulate = false;
    private GraphSimulator activeGraphSimulator;

    public float timer = 0;
    public float timeInterval = .5f;
    public TextMeshProUGUI scoreText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Node> nodes = new List<Node>();
        Node node1 = new Node(new Vector2(-1,0));
        Node node2 = new Node(new Vector2(1,0));
        Node node3 = new Node(new Vector2(2,0));
        nodes.Add(node1);
        nodes.Add(node2);
        nodes.Add(node3);
        node3.nodeType = Node.NodeType.INPUT_ON;
        node1.nodeType = Node.NodeType.OUTPUT_ON;
        activeGraph = new Graph(nodes);
        activeGraph.AddConnection(node1, node2);
        activeGraph.AddConnection(node2, node3);
        
        
        GetComponent<GraphEditor>().graph = activeGraph;
        GetComponent<GraphRenderer>().graph = activeGraph;
        
    }
    
    public void ReinitializeInputOutputs()
    {
        if (activeGraphSimulator != null)
        {
            activeGraphSimulator.InitializeInputOutputs();
        }
    }

    public void ToggleSimulating()
    {
        simulate = !simulate;
    }

    public void ChangeTimeInterval(float newtimeInterval)
    {
        this.timeInterval = 1 - newtimeInterval;
    }
//hmmmm
    // Update is called once per frame
    void Update()
    {
        if (simulate)
        {
            if (activeGraphSimulator == null)
            {
                activeGraphSimulator = new GraphSimulator(activeGraph);
            }
            
            timer += Time.deltaTime;
            if (timer > timeInterval)
            {
                Debug.Log("timestep");
                timer = 0;
                float score = activeGraphSimulator.Timestep();
                scoreText.text = "total score: " + score.ToString();
            }
            
        }
    }
}
