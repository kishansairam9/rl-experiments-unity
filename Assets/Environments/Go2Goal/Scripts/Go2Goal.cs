using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class Go2Goal : MinionBase
{
    public GameObject arena;
    public GameObject goalPrefab;

    private GameObject goal;
    private Vector3 arenaSize;
    private float distanceToGoal;
    private Collider goalCollider;
    private Vector3 relativeGoalPosition;
    
    public override void Initialize()
    {
        base.Initialize();
        arenaSize = Vector3.Scale(
            arena.transform.localScale,
            arena.transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size
        );
        goal = Instantiate(goalPrefab);
        goalCollider = goal.GetComponent<Collider>();
        goal.GetComponent<Renderer>().material = agentMaterial;
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
        rigidBody.angularVelocity = Vector3.zero;
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
        return (Vector3.Scale(randomVector, arenaSize) - arenaSize / 2f);
    }

    void RandomSpawn()
    {
        var tries = 0;
        var position = Vector3.zero;
        while (tries++ < 10)
        {
            position = GetRandomVectorInArena();
            if (!Physics.CheckBox(position, selfSize / 2f))
                break;
        }
        transform.position = position;
        transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
    }

    void SetRandomGoal()
    {
        var tries = 0;
        var position = Vector3.zero;
        while (tries++ < 10)
        {
            position = GetRandomVectorInArena();
            if (!Physics.CheckBox(position, selfSize / 2f))
                break;
        }
        print("tried " + tries + " times");
        print("ended up with this position -> " + position);
        goal.transform.position = position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == goalCollider)
        {
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
        if (other.tag == "Wall")
        {
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
