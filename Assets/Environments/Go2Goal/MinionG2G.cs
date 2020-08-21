using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


public class MinionG2G : Agent
{
    public GameObject area;
    public GameObject goalPrefab;
    public ActionRange[] actionRange;
    public int minTimeInGoal = 100;
    [ReadOnly]
    public float[] defaultAction;
    public PID pidVelocityController;
    public PID pidRotationController;

    private bool ready = false;
    private float goalRadius;
    private GameObject goal;
    private Transform agent;
    private Rigidbody agentRb;
    private Vector3 areaCenter;
    private Vector3 areaDimensions;
    private Material agentMaterial;
    private Color agentColor;
    private Vector3 relativePosition;
    private float currRelativeDistance;
    private Collider selfGoalCollider;
    private int timeInGoal = 0;
    private int numSuccess = 0;
    private float currAngularVelocity = 0f;

    void Start()
    {
        ReadyUp();
    }

    public void ReadyUp()
    {
        if (!ready)
        {
            defaultAction = new float[actionRange.Length];
            for(var i = 0; i < actionRange.Length; i++)
                defaultAction[i] = actionRange[i].getDefaultInRange();
            goal = Instantiate(goalPrefab);
            agent = GetComponent<Transform>();
            areaCenter = area.GetComponent<Transform>().position;
            agentRb = agent.GetComponent<Rigidbody>();
            SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.75f, 1f));
            ready = true;
            // agentRb.maxAngularVelocity = 1.0f;
            selfGoalCollider = goal.GetComponent<BoxCollider>();
            pidRotationController.Reset();
            pidVelocityController.Reset();
            goalRadius = goal.GetComponent<SphereCollider>().radius;
            areaDimensions = Vector3.Scale(
                area.transform.localScale,
                area.transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size
            );
        }
    }

    public override void Initialize()
    {
        ReadyUp();
    }

    public void SetColor(Color color)
    {
        agentColor = color;
        agentMaterial = agent.GetChild(0).GetComponent<Renderer>().material;
        agentMaterial.color = agentColor;
        goal.GetComponent<Renderer>().material = agentMaterial;
    }

    public void RandomSpawn()
    {
        float x, z;
        do
        {
            x = Random.Range(areaCenter.x - areaDimensions.x/2f, areaCenter.x + areaDimensions.z/2f);
            z = Random.Range(areaCenter.y - areaDimensions.x/2f, areaCenter.y + areaDimensions.z/2f);

        } while(
            Physics.CheckBox(
                new Vector3(x, 0.65f, z),
                new Vector3(0.55f, 0.55f, 0.55f)
            )
        );
        agent.position = new Vector3(x, .05f, z);
        var a = Random.Range(-1.0f, 1.0f);
        var b = Random.Range(-1.0f, 1.0f);
        var c = a*agent.forward + b*agent.right;
        agent.rotation = Quaternion.LookRotation(c, agent.up);
    }

    public void SetRandomGoal()
    {
        var tries = 0;
        float x, z;
        // Set some random goal inside the bounds of the arena!
        do
        {
            x = Random.Range(areaCenter.x - areaDimensions.x/2f, areaCenter.x + areaDimensions.z/2f);
            z = Random.Range(areaCenter.y - areaDimensions.x/2f, areaCenter.y + areaDimensions.z/2f);
        } while(
            Physics.CheckSphere(
                new Vector3(x, goal.transform.position.y, z),
                goalRadius
            ) && tries++ < 10
        );
        print("Tried "+ tries + " times!");
        goal.transform.position = new Vector3(x, 0.025f, z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // VectorSensor.size = 5
        if (goal != null)
        {
            relativePosition = agent.InverseTransformDirection(
                goal.transform.position - agent.position
            );
            var relPose = relativePosition.normalized;
            var relMag = Mathf.Min(relativePosition.magnitude, 15.0f)/15f;
            sensor.AddObservation(relPose.x);
            sensor.AddObservation(relPose.z);
            sensor.AddObservation(relMag);
            sensor.AddObservation(agent.InverseTransformDirection(agentRb.velocity));
            sensor.AddObservation(agentRb.angularVelocity);
            // print(agent.InvexrseTransformDirection(agentRb.velocity));
            // var s = "";
            // // List<float> x = sensor.m_Observations;
            // foreach (var val in GetObservations())
            // {
            //     s = s + "  " + val;
            // }
            // Debug.Log(s);
        }
        Debug.Log(agentRb.angularVelocity);
    }

    public void MoveAgent(float[] action)
    {
        // agentRb.velocity = agent.forward * action[0];
        // agentRb.AddForce();
        agentRb.velocity = agent.forward * 45f * Mathf.Deg2Rad;
        agentRb.angularVelocity = agent.up * 45f * Mathf.Deg2Rad;
        print("Action1: " + action[1]);
        return;
        var force = pidVelocityController.Update(
            action[0], agent.InverseTransformDirection(agentRb.velocity).z,
            Time.deltaTime
        );
        var torque = pidRotationController.Update(
            action[1], currAngularVelocity, Time.deltaTime
        );
        print("TORQUE: " + torque);
        agentRb.AddForce(agent.forward * force, ForceMode.Acceleration);
        // agentRb.AddForce(agent.forward * action[0], ForceMode.VelocityChange);
        agentRb.AddTorque(agent.up * torque, ForceMode.Acceleration);
    }

    void FixedUpdate(){
        currAngularVelocity = agentRb.angularVelocity.y;
        print(agentRb.angularVelocity.y);
        // DebugGraph.Log("omega", currAngularVelocity);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var currDistance = relativePosition.magnitude;
        if (currRelativeDistance != currDistance)
        {
            AddReward(currRelativeDistance - currDistance);
            currRelativeDistance = currDistance;
        }
        for (var i = 0; i < vectorAction.Length; i++)
        {
            vectorAction[i] = ScaleAction(vectorAction[i], actionRange[i].min, actionRange[i].max);
        }
        MoveAgent(vectorAction);
    }

    public override void Heuristic(float[] actionsOut)
    {
        Array.Copy(defaultAction, actionsOut, defaultAction.Length);
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = +1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = -1f;
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
        relativePosition = goal.transform.position - agent.position;
        currRelativeDistance = relativePosition.magnitude;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == selfGoalCollider)
        {
            AddReward(2f);
            timeInGoal = 0;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other == selfGoalCollider)
        {
            AddReward(2f);
            if (timeInGoal++ > minTimeInGoal)
            {
                numSuccess++;
                EndEpisode();
            }
                
        }
    }

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.name != "Floor")
        {
            AddReward(-5f);
            EndEpisode();
        }
    }
}
