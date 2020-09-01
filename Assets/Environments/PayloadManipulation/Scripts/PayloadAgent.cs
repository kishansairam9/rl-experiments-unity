using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PayloadAgent : Agent
{
    public GameObject arena;
    public GameObject goalPrefab;
    public ActionRange linearVelocityRange;
    public ActionRange angularVelocityXRange;
    public ActionRange angularVelocityYRange;
    public ActionRange angularVelocityZRange;
    public ActionRange rollRange;
    public ActionRange pitchRange;

    private Rigidbody rb;
    private GameObject goal;
    private Vector3 selfSize;
    private Vector3 arenaSize;
    private float distanceToGoal;
    private float[] defaultAction;
    private Collider goalCollider;
    private Vector3 relativeGoalPosition;
    private ActionRange[] actionRange;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        arenaSize = Vector3.Scale(
            arena.transform.localScale,
            arena.transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size
        );
        selfSize = Vector3.Scale(
            transform.localScale,
            transform.GetComponent<MeshFilter>().mesh.bounds.size
        );
        goal = Instantiate(goalPrefab);
        goalCollider = goal.GetComponent<Collider>();
        defaultAction = new float[5];
        defaultAction[0] = linearVelocityRange.getDefaultInRange();
        defaultAction[1] = linearVelocityRange.getDefaultInRange();
        defaultAction[2] = angularVelocityXRange.getDefaultInRange();
        defaultAction[3] = angularVelocityYRange.getDefaultInRange();
        defaultAction[4] = angularVelocityZRange.getDefaultInRange();
        actionRange = new ActionRange[] { 
            linearVelocityRange,
            linearVelocityRange,
            angularVelocityXRange,
            angularVelocityYRange,
            angularVelocityZRange
        };
    }

    public void MoveAgent(float[] actions)
    {
        /*
         * actions is List of 5 floats in the interval [-1, 1] representing:
         * linear velocity in x direction (local)
         * linear velocity in y direction (local)
         * angular velocity along x, y and z axis (local)
         * 
         * The actions are scaled to the actual interval specified!
         */

        for (int i = 0; i < actions.Length; i++)
            actions[i] = ScaleAction(actions[i], actionRange[i].min, actionRange[i].max);
        rb.velocity = new Vector3(actions[0], 0f, actions[1]);

        // check if the rotation has exceeded the limits
        actions[3] = (
            (actions[3] < 0 && Pi2Pi(transform.eulerAngles.x) <= rollRange.min) ? 0f :
            (actions[3] > 0 && Pi2Pi(transform.eulerAngles.x) >= rollRange.max) ? 0f : actions[3]
        );
        actions[4] = (
            (actions[4] < 0 && Pi2Pi(transform.eulerAngles.z) <= pitchRange.min) ? 0f :
            (actions[4] > 0 && Pi2Pi(transform.eulerAngles.z) >= pitchRange.max) ? 0f : actions[4]
        );
        rb.angularVelocity = (
            transform.up * actions[2] +
            transform.right * actions[3] + 
            transform.forward * actions[4]
        );
    }

    public static float Pi2Pi(float angle)
    {
        if (angle < -180F)
            angle += 360F;
        if (angle > 180F)
            angle -= 360F;
        return angle;
    }

    public override void OnActionReceived(float[] actions)
    {
        MoveAgent(actions);
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = (
            Input.GetKey(KeyCode.W) ? +1f :
            Input.GetKey(KeyCode.S) ? -1f : defaultAction[0]
        );
        actions[1] = (
            Input.GetKey(KeyCode.A) ? -1f :
            Input.GetKey(KeyCode.D) ? +1f : defaultAction[1]
        );
        actions[2] = (
            Input.GetKey(KeyCode.Q) ? -1f :
            Input.GetKey(KeyCode.E) ? +1f : defaultAction[2]
        );
        actions[3] = (
            Input.GetKey(KeyCode.Z) ? -1f :
            Input.GetKey(KeyCode.C) ? +1f : defaultAction[3]
        );
        actions[4] = (
            Input.GetKey(KeyCode.O) ? -1f :
            Input.GetKey(KeyCode.P) ? +1f : defaultAction[4]
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
        RandomSpawn();
        SetRandomGoal();
        relativeGoalPosition = transform.InverseTransformDirection(
            goal.transform.position - transform.position
        );
        print("Ralative Goal Position: " + relativeGoalPosition);
        distanceToGoal = relativeGoalPosition.magnitude;
    }

    Vector3 GetRandomValidPositionInArena()
    {
        var tries = 0;
        var position = Vector3.zero;
        while (tries++ < 10)
        {
            var randomVector = new Vector3(Random.Range(0f, 1f), 0f, Random.Range(0f, 1f));
            position = (Vector3.Scale(randomVector, arenaSize) - arenaSize / 2f);
            if (!Physics.CheckBox(position, selfSize / 2f))
                break;
        }
        position.y = 1.3f;
        return position;
    }

    void RandomSpawn()
    {
        transform.position = GetRandomValidPositionInArena();
        transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
    }
    void SetRandomGoal()
    {
        goal.transform.position = GetRandomValidPositionInArena();
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("DISTANCE2GOAL: " + distanceToGoal);
        if (other == goalCollider && distanceToGoal < 0.1)
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
