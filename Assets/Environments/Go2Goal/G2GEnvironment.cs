using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgentsExamples;

public class G2GEnvironment : Area
{
    GameObject arena;
    List<MinionG2G> agents;
    
    void Start()
    {
        // initializeAgents();
        // Reset(); 
    }

    // public void Register()

    private void initializeAgents()
    {   
        agents = new List<MinionG2G>();
        foreach (Transform child in transform)
        {   
            MinionG2G possibleAgent;
            possibleAgent = child.GetComponent<MinionG2G>();
            if (possibleAgent != null)
            {
                print("Adding agent: " + child.name);
                possibleAgent.ReadyUp();
                agents.Add(possibleAgent);
                possibleAgent.SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.75f, 1f));
            }
        }
    }

    public void Reset()
    {
        foreach (MinionG2G agent in agents)
        {
            agent.RandomSpawn();
            agent.SetRandomGoal();
        }
    }


    public override void ResetArea()
    {
    }
}
