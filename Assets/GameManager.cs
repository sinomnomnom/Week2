using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Graph activeGraph;
    private bool simulate = false;
    private GraphSimulator activeGraphSimulator;

    public float timer = 0;
    public float timeInterval = 1;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Node> nodes = new List<Node>();
        Node node1 = new Node(new Vector2(-1,0));
        Node node2 = new Node(new Vector2(1,0));
        node1.AddConnection(node2);
        
        nodes.Add(node1);
        nodes.Add(node2);
      
        activeGraph = new Graph(nodes);
        
        GetComponent<GraphEditor>().graph = activeGraph;
        GetComponent<GraphRenderer>().graph = activeGraph;
        
    }

    public void ToggleSimulating()
    {
        simulate = !simulate;
        activeGraphSimulator = new GraphSimulator(activeGraph);
    }

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
                timer = 0;
                activeGraphSimulator.Timestep();
            }
            
        }
    }
}
