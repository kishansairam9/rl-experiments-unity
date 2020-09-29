using UnityEngine;
using Random = UnityEngine.Random;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
public class PayloadAgentGetOut : Agent
{
    public GameObject arena;
    public ActionRange linearVelocityRange;
    public ActionRange angularVelocityYRange;

    public float minDoorWidth = 2f, maxDoorWidth = 4f;
    public GameObject wallPrefab;
    public GameObject roomFloorPrefab;

    public float roomExitReward = 5f;
    public float roomStayPenalty = -0.01f;
    public float collisionPenalty = -5f;


    private Rigidbody rb;
    private Vector3 selfSize;
    private Vector3 arenaSize;
    private float[] defaultAction;
    private ActionRange[] actionRange;
    private BadRoom room;
    // Start is called before the first frame update
    void Start()
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
        defaultAction = new float[3];
        defaultAction[0] = linearVelocityRange.getDefaultInRange();
        defaultAction[1] = linearVelocityRange.getDefaultInRange();
        defaultAction[2] = angularVelocityYRange.getDefaultInRange();
        actionRange = new ActionRange[] {
            linearVelocityRange,
            linearVelocityRange,
            angularVelocityYRange
        };
        room = gameObject.AddComponent<BadRoom>();
        room.Init(minDoorWidth, maxDoorWidth, wallPrefab, roomFloorPrefab);
    }

    public void MoveAgent(float[] actions)
    {
        /*
         * actions is List of 3 floats in the interval [-1, 1] representing:
         * linear velocity in x direction (local)
         * linear velocity in y direction (local)
         * angular velocity along yaw axis (local)
         * 
         * The actions are scaled to the actual interval specified!
         */

        for (int i = 0; i < actions.Length; i++)
            actions[i] = ScaleAction(actions[i], actionRange[i].min, actionRange[i].max);
        rb.velocity = new Vector3(actions[0], 0f, actions[1]);

        rb.angularVelocity = transform.up * actions[2];
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
    }


    Vector3 GetRandomValidPositionInArena()
    {
        var tries = 0;
        var position = Vector3.zero;
        while (tries++ < 10)
        {
            var randomVector = new Vector3(Random.Range(0f, 1f), 0f, Random.Range(0f, 1f));
            position = (Vector3.Scale(randomVector, arenaSize/2f) - arenaSize / 4f);
            print(position);
            if (!Physics.CheckBox(position, selfSize / 2f))
                break;
        }
        position.y = 1.3f;
        return position;
    }

    void RandomSpawn()
    {
        transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
        transform.position = GetRandomValidPositionInArena();
        print(transform.position);
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin");
        rb.angularVelocity = Vector3.zero;
        RandomSpawn();
        room.Clear();
        room.CreateMap("Some Map");
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
            AddReward(collisionPenalty);
            return true;
        }
        return false;
    }

    private void OnTriggerStay(Collider other)
    {
        AddReward(roomStayPenalty);
    }

    private void OnTriggerExit(Collider other)
    {
        AddReward(roomExitReward);
        EndEpisode();
    }

}
