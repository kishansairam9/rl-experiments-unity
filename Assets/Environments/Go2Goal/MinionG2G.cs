using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MinionG2G : Agent
{
    public GameObject area;
    public GameObject goalPrefab;
    public float maxLinearVelocity = 1f;
    public float minLinearVelocity = 0f;
    public float maxAngularVelocityMagnitude = 3f;
    public float[] defaultAction;

    bool ready = false;
    GameObject goal;
    Transform agent;
    Rigidbody agentRb;
    Vector3 areaCenter;
    Material agentMaterial;
    Color agentColor;
    Vector3 relativePosition;
    Vector2 currRelativePosition;
    Vector2 prevRelativePosition;


    void Start()
    {
        ReadyUp();
    }

    public void ReadyUp()
    {
        if (!ready)
        {
            goal = Instantiate(goalPrefab);
            agent = GetComponent<Transform>();
            areaCenter = area.GetComponent<Transform>().position;
            ready = true;
            agentRb = agent.GetComponent<Rigidbody>();
            SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.75f, 1f));
        }
    }

    public override void Initialize()
    {
        ReadyUp();
    }

    public void SetColor(Color color)
    {
        // print("Set Color" + color);
        // print(agent.GetChild(0));
        agentColor = color;
        agentMaterial = agent.GetChild(0).GetComponent<Renderer>().material;agentMaterial.color = agentColor;
        goal.GetComponent<Renderer>().material = agentMaterial;
    }

    public void RandomSpawn()
    {
        float x, z;
        do
        {
            x = Random.Range(areaCenter.x - 50f, areaCenter.x + 50f);
            z = Random.Range(areaCenter.y - 50f, areaCenter.y + 50f);

        } while(
            Physics.CheckBox(
                new Vector3(x, 0.65f, z),
                new Vector3(0.55f, 0.55f, 0.55f)
            )
        );
        agent.position = new Vector3(x, 0.65f, z);
    }

    public void SetRandomGoal()
    {
        // Set some random goal inside the bounds of the arena!
        float x, z;
        do
        {
            x = Random.Range(areaCenter.x - 50f, areaCenter.x + 50f);
            z = Random.Range(areaCenter.y - 50f, areaCenter.y + 50f);

        } while(
            Physics.CheckBox(
                new Vector3(x, 0.5f, z),
                new Vector3(0.7f, 0.1f, 0.7f)
            )
        );
        goal.transform.position = new Vector3(x, 0.025f, z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // VectorSensor.size = 8
        relativePosition = goal.transform.position - agent.position;
        // Not necessary to give color codes because the relative pos vec of the goal is provided!
        // sensor.AddObservation(agentColor.r);
        // sensor.AddObservation(agentColor.g);
        // sensor.AddObservation(agentColor.b);
        sensor.AddObservation(relativePosition.x);
        sensor.AddObservation(relativePosition.y);
        sensor.AddObservation(agent.InverseTransformDirection(agentRb.velocity));
        sensor.AddObservation(agent.InverseTransformDirection(agentRb.angularVelocity));
    }

    public void MoveAgent(float[] action)
    {
        var locVel = transform.InverseTransformDirection(agentRb.velocity);
        locVel.z = action[0];
        agentRb.velocity = agent.TransformDirection(locVel);
        agent.Rotate(agent.up, Time.deltaTime * action[1]);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        BoundAndClipAcitons(vectorAction);
        MoveAgent(vectorAction);
        // AddReward(prevRelativePosition.magnitude - currRelativePosition.magnitude);
    }

    public override void Heuristic(float[] actionsOut)
    {
        Array.Copy(defaultAction, actionsOut, defaultAction.Length);
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = +15f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = -15f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = +1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = -1f;
        }
    }

    public override void OnEpisodeBegin()
    {
        agentRb.velocity = Vector3.zero;
        RandomSpawn();
        SetRandomGoal();
    }

    private void BoundAndClipAcitons(float[] actions)
    {
        actions[0] = (actions[0] + 1)/2.0f;
        actions[0] *= (maxLinearVelocity - minLinearVelocity);
        actions[0] += minLinearVelocity;
        actions[1] *= maxAngularVelocityMagnitude;
    }

}
