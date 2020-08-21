using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


public class Minion : Agent
{
    public GameObject arena;
    public GameObject goalPrefab;
    public ActionRange[] actionRange;

    private Rigidbody rb;
    private GameObject goal;
    private Vector3 selfSize;
    private Vector3 arenaSize;
    private int calledCounter;
    private float distanceToGoal;
    private float[] defaultAction;
    private Collider goalCollider;
    private Vector3 relativeGoalPosition;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        arenaSize = Vector3.Scale(
            arena.transform.localScale,
            arena.transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size
        );
        selfSize = Vector3.Scale(
            transform.GetChild(0).localScale,
            transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size
        );
        goal = Instantiate(goalPrefab);
        goalCollider = goal.GetComponent<Collider>();
        defaultAction = new float[actionRange.Length];
            for(var i = 0; i < actionRange.Length; i++)
                defaultAction[i] = actionRange[i].getDefaultInRange();
        AssignColor();
    }

    void AssignColor()
    {
        var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.75f, 1f);
        var agentMaterial = transform.GetChild(0).GetComponent<Renderer>().material;
        agentMaterial.color = color;
        goal.GetComponent<Renderer>().material = agentMaterial;
    }

    public void MoveAgent(float[] actions)
    {
        for(int i=0; i<actions.Length; i++)
            actions[i] = ScaleAction(actions[i], actionRange[i].min, actionRange[i].max);
        rb.velocity = transform.forward * actions[0];
        rb.angularVelocity = transform.up * actions[1];
    }

    public override void OnActionReceived(float[] actions)
    {
        MoveAgent(actions);
        calledCounter++;
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = (
            Input.GetKey(KeyCode.W) ? +1f: 
            Input.GetKey(KeyCode.S) ? -1f: defaultAction[0]
        );
        actions[1] = (
            Input.GetKey(KeyCode.A) ? -1f:
            Input.GetKey(KeyCode.D) ? +1f: defaultAction[1]
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(relativeGoalPosition.normalized.x);
        sensor.AddObservation(relativeGoalPosition.normalized.z);
        sensor.AddObservation(Mathf.Min(distanceToGoal, 5f));
        // PrintSensor(sensor);
        ComputeReward();
    }

    void ComputeReward()
    {
        var newRelativeGoalPosition = transform.InverseTransformDirection(
            goal.transform.position - transform.position
        );
        var newDistanceToGoal = newRelativeGoalPosition.magnitude;
        var reward = 10f * (distanceToGoal - newDistanceToGoal - 0.02f);
        AddReward(reward);
        // print("Reward ---> " + reward);
        distanceToGoal = newDistanceToGoal;
        relativeGoalPosition = newRelativeGoalPosition;
    }

    public override void OnEpisodeBegin()
    {
        rb.angularVelocity = Vector3.zero;
        calledCounter = 0;
        RandomSpawn();
        SetRandomGoal();
        relativeGoalPosition = transform.InverseTransformDirection(
            goal.transform.position - transform.position
        );
        distanceToGoal = relativeGoalPosition.magnitude;
    }

    Vector3 GetRandomVectorInArena()
    {
        var randomVector = new Vector3(Random.Range(0f, 1f), 0f, Random.Range(0f, 1f));
        return (Vector3.Scale(randomVector, arenaSize) - arenaSize/2f);
    }

    void RandomSpawn()
    {
        var tries = 0;
        var position = Vector3.zero;
        while(tries++ < 10)
        {
            position = GetRandomVectorInArena();
            if (!Physics.CheckBox(position, selfSize/2f))
                break;
        }
        transform.position = position;
        transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
    }

    void SetRandomGoal()
    {
        var tries = 0;
        var position = Vector3.zero;
        while(tries++ < 10)
        {
            position = GetRandomVectorInArena();
            if (!Physics.CheckBox(position, selfSize/2f))
                break;
        }
        print("tried "+ tries + " times");
        print("ended up with this position -> " + position);
        goal.transform.position = position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == goalCollider){
            AddReward(2f);
            EndEpisode();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Floor")
            return;

        // we want to find out whether this guy was responsible for the collision!
        if (PenalizeCollision(other.gameObject))
            SetReward(-5f);
        EndEpisode();
    }

    bool PenalizeCollision(GameObject other)
    {
        // if the collision is with a wall! --> Penalize
        if (other.tag == "Wall"){
            print("Hit wall!");
            return true;
        }
        return false;
    }

    void PrintSensor(VectorSensor sensor)
    {
        var s = "Sensor: [ ";
        foreach (var val in GetObservations())
            s = s + "  " + val;
        print(s + " ]");
    }
}
