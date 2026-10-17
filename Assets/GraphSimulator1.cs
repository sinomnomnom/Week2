using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Random = UnityEngine.Random;

public class GraphSimulator
{
    public Graph graph;
    public Node[] inputNodes;
    public Node[] outputNodes;
    
    public bool[][] inputOutputs;
    
    public int inputIndex = 0;

    public int timeSteps = 0;
    public int changeInputInterval = 50;
    public int lastTotalScore = 0;
    
    public GraphSimulator(Graph graph)
    {
        this.graph = graph;
        InitializeInputOutputs();
    }

    public bool[][] GetAllOutputs()
    {
        bool[][] allOutputs = new bool[(int)Math.Pow(2, outputNodes.Length)][];
        for (int i = 0; i < Math.Pow(2, outputNodes.Length); i++)
        {
            allOutputs[i] = new bool[outputNodes.Length];
            for (int j = 0; j < outputNodes.Length; j++)
            {
                allOutputs[i][j] = (i / (int)Math.Pow(2, j) % 2) == 0;
            }
        }
        return allOutputs;
    }

    public void CreateInputOutputTable()
    {
        inputOutputs = new bool[(int)Math.Pow(2, inputNodes.Length)-1][];
        bool[][] outputs = GetAllOutputs();
        for (int i = 0; i < Math.Pow(2, inputNodes.Length)-1; i++)
        {
            inputOutputs[i] = outputs[i%outputs.Length];
        }
    }

    public void Evolve(int generationSize)
    {
        int hiScore = TotalScore();
        List<Node> bestMutation = new List<Node>();
        for (int i = 0; i < generationSize; i++)
        {
            List<Node> mutation  = Mutate(2, 0);
            int newScore = TotalScore();
            
            if (newScore > hiScore)
            {
                bestMutation = mutation;
                hiScore = newScore;
            }
            Unmutate(mutation);
            graph.Deactivate();
        }
        Mutate(bestMutation);
        graph.Deactivate();
    }

    public void Mutate(List<Node> mutation)
    {
        foreach (Node node in mutation)
        {
            graph.AddNode(node);
        }
    }

    public List<Node> Mutate(int additions, int removals)
    {
        List<Node> mutatedNodes = new List<Node>();
        for (int i = 0; i < additions; i++)
        {
            Node newNode = new Node(RandomNodePosition());
            graph.nodes.Add(newNode);
            graph.AddConnection(newNode, graph.GetRandomNode());
            graph.AddConnection(newNode, graph.GetRandomNode());
            if (newNode.connectedNodes.Count < 1)
            {
                graph.RemoveNode(newNode);
                continue;
            }
            mutatedNodes.Add(newNode);
        }
        return mutatedNodes;
    }

    public Vector2 RandomNodePosition()
    {
        return new Vector2((Random.value-0.5f)*2f*3f, (Random.value-0.5f)*2f*3f);
    }

    public void Unmutate(List<Node> mutation)
    {
        foreach (Node node in mutation)
        {
            graph.RemoveNode(node);
        }
    }

    public void InitializeInputOutputs()
    {
        InitializeArrays();
        CreateInputOutputTable();
        SetInputIndex(0);
    }

    public void SetInputIndex(int index)
    {
        for (int i = 0; i< outputNodes.Length; i++)
        {
            outputNodes[i].nodeType = inputOutputs[index][i] ? Node.NodeType.OUTPUT_ON : Node.NodeType.OUTPUT_OFF;
        }

        for (int i = 0; i < inputNodes.Length; i++)
        {
            if (((index+1) / (int)Math.Pow(2, i)) % 2 == 0)
            {
                inputNodes[i].nodeType = Node.NodeType.INPUT_OFF;
            }
            else
            {
                inputNodes[i].nodeType = Node.NodeType.INPUT_ON;
            }
        }
    }

    public void InitializeArrays()
    {
        List<Node> tempInputs = new List<Node>();
        List<Node> tempOutputs = new List<Node>();
        
        foreach (Node node in graph.nodes)
        {
            if (node.nodeType == Node.NodeType.INPUT_ON | node.nodeType == Node.NodeType.INPUT_OFF)
            {
                tempInputs.Add(node);
            }
            if (node.nodeType == Node.NodeType.OUTPUT_ON | node.nodeType == Node.NodeType.OUTPUT_OFF)
            {
                tempOutputs.Add(node);
            }
        }
        
        inputNodes = tempInputs.ToArray();
        outputNodes = tempOutputs.ToArray();
    }

    public int Score()
    {
        int score = 0;
        for (int i = 0; i < outputNodes.Length; i++)
        {
            if (outputNodes[i].active == inputOutputs[inputIndex][i])
            {
                score++;
            }
        }
        return score;
    }

    public int TotalScore()
    {
        timeSteps = 0;
        inputIndex = 0;
        SetInputIndex(0);
        graph.Deactivate();

        int score = 0;
        int steps = changeInputInterval * ((int)Math.Pow(2, inputNodes.Length)-1);
        for (int i = 0; i < steps; i++)
        {
            score += Step();
        }
        timeSteps = 0;
        inputIndex = 0;
        SetInputIndex(0);
        graph.Deactivate();
        return score;
    }
    
    private int Step()
    {
        if (timeSteps >= changeInputInterval)
        {
            timeSteps = 0;
            inputIndex++;
            SetInputIndex(inputIndex);
        }
        timeSteps++;

        foreach (Node node in graph.nodes) node.Simulate1();
        foreach (Node node in graph.nodes) node.Simulate2();
        return Score();
    }

    public float Timestep()
    {
        int end = (int)Math.Pow(2, inputNodes.Length) - 2;

        if (timeSteps >= changeInputInterval && inputIndex == end)
        {
            Evolve(1000);
            timeSteps = 0;
            inputIndex = 0;
            SetInputIndex(0);
            lastTotalScore = TotalScore();
        }

        Step();
        return lastTotalScore/(float)(outputNodes.Length*(Math.Pow(2, inputNodes.Length) - 1)*changeInputInterval);
    }
}
