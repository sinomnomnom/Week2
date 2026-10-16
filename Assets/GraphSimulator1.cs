using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GraphSimulator
{
    public Graph graph;
    public Node[] inputNodes;
    public Node[] outputNodes;
    
    public bool[][] inputOutputs;
    
    public int inputIndex = 0;

    public int timeSteps = 0;
    public int changeInputInterval = 10;


    public GraphSimulator(Graph graph)
    {
        this.graph = graph;
        InitializeInputOutputs();
    }


    public bool[] CreateRandomOutput(int size)
    {
        bool[] randOutput = new bool[size];
        for (int i = 0; i< size; i++)
        {
            randOutput[i] = (Random.Range(0,2) == 0) ? false : true;
        }
        return randOutput;
    }

    public void CreateInputOutputTable()
    {
        inputOutputs = new bool[(int)Math.Pow(2, inputNodes.Length)][];
        for (int i = 0; i < Math.Pow(2, inputNodes.Length); i++)
        {
            bool unique = false;
            bool[] tempOutputs = CreateRandomOutput(outputNodes.Length);
            while (!unique)
            {
                unique = true;
                for (int j = 0; j < i; j++)
                {
                    if (inputOutputs[i] != null && inputOutputs[i] == tempOutputs)
                    {
                        unique = false;
                        tempOutputs = CreateRandomOutput(outputNodes.Length);
                    }
                }
            }
            inputOutputs[i] = tempOutputs;
        }
    }

    public void InitializeInputOutputs()
    {
        InitializeArrays();
        CreateInputOutputTable();
    }

    public void SetInputIndex(int index)
    {
        for (int i = 0; i< outputNodes.Length; i++)
        {
            outputNodes[i].nodeType = inputOutputs[index][i] ?  Node.NodeType.OUTPUT_ON : Node.NodeType.OUTPUT_OFF;
        }

        for (int i = 0; i < inputNodes.Length; i++)
        {
            if (((index)/(int)Math.Pow(2, i))%2 == 0)
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

    public void Timestep()
    {
        timeSteps++;
        if (timeSteps > changeInputInterval)
        {
            timeSteps = 0;
            if (inputIndex < (int)Math.Pow(2, inputNodes.Length) - 1)
            {
                inputIndex++;
                SetInputIndex(inputIndex);
            }
            else
            {
                inputIndex = 0;
            }
            
        }
        foreach (Node node in graph.nodes)
        {
            node.Simulate1();
        }
        foreach (Node node in graph.nodes)
        {
            node.Simulate2();
        }
    }
}
