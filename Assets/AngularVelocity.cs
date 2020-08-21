using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularVelocity : MonoBehaviour
{
    private Vector3 rotationLast;
    public Vector3 rotationDelta;
    private Rigidbody rigidBody;
    private float previousRealTime;
    public Vector3 angularVelocityManual, linearVelocityManual;
    private Vector3 previousPosition = Vector3.zero;
    public Vector3 angularVelocityTDT, linearVelocityTDT;
    public float manualDeltaTime;
    public float fixedDeltaTime;
    public float deltaTime;

    void Start()
    {
        rotationLast = transform.rotation.eulerAngles;
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    { 
        manualDeltaTime = Time.realtimeSinceStartup - previousRealTime;
        rotationDelta = transform.rotation.eulerAngles - rotationLast;
        rotationLast = transform.rotation.eulerAngles;
        // Manually Computed Time
        angularVelocityManual = rotationDelta*Mathf.Deg2Rad / manualDeltaTime;
        linearVelocityManual = (transform.position - previousPosition) / manualDeltaTime;
        // Using Time.deltaTime!
        angularVelocityTDT = rotationDelta * Mathf.Deg2Rad / Time.deltaTime;
        linearVelocityTDT = (transform.position - previousPosition) / Time.deltaTime;
        fixedDeltaTime = Time.fixedDeltaTime;
        deltaTime = Time.deltaTime;
        previousPosition = transform.position;
        previousRealTime = Time.realtimeSinceStartup;
        if (Mathf.Abs(transform.rotation.eulerAngles.y) < 1f)
        {
            print(Time.realtimeSinceStartup);
        }
    }
}
