using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Go2GoalSimple : MonoBehaviour
{
    public Vector2 goal;
    [Range(0, 2)]
    public float linearVelocityGain;
    [Range(0, 2)]
    public float angularVelocityGain;
    [Range(0, 0.2f)]
    public float thresholdDistance;
    public bool useLimits;
    public ActionRange[] actionRange;
    
    private Vector3 size;
    private Rigidbody rigidBody;
    void Start()
    {
        try
        {
            size = GetComponent<Collider>().bounds.size;
        } catch {
            size = transform.GetComponentInChildren<Renderer>().bounds.size;
        }
        rigidBody = transform.GetComponent<Rigidbody>();
        Debug.Log(size);
        Debug.Log(rigidBody);
    }
    void Update()
    {
        var wordFrameGoal = new Vector3(goal.x, this.size.y / 2f, goal.y);
        var relativeGoal = transform.InverseTransformPoint(wordFrameGoal);
        var distance = relativeGoal.magnitude;
        if (distance >= thresholdDistance)
        {
            var linearVelocity = linearVelocityGain * distance;
            var steeringAngle = Mathf.Atan2(relativeGoal.x, relativeGoal.z);
            var angularVelocity = angularVelocityGain * steeringAngle;
            if (useLimits)
            {
                linearVelocity = Mathf.Clamp(linearVelocity, actionRange[0].min, actionRange[0].max);
                angularVelocity = Mathf.Clamp(angularVelocity, actionRange[1].min, actionRange[1].max);
            }
            rigidBody.transform.Translate(new Vector3(0f, 0f, 1f) * linearVelocity * Time.deltaTime);
            rigidBody.transform.Rotate(transform.up * angularVelocity * Time.deltaTime * Mathf.Rad2Deg);
        }
    }
}
