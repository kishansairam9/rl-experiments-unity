using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grippper : MonoBehaviour
{

    public bool openGripper;
    //public bool openGripper;
    private ConfigurableJoint gripper;
    private Rigidbody rb;
    
    void Start()
    {
        gripper = GetComponent<ConfigurableJoint>();
        rb = GetComponent<Rigidbody>();
        
    }

    void Update()
    {
        print("Gripper: " + openGripper);
        if (openGripper)
        {
            rb.AddRelativeForce(gripper.axis*10f, ForceMode.Force);
        }
        else
        {
            rb.AddRelativeForce(gripper.axis*-10f, ForceMode.Force);
        }

    }
}
