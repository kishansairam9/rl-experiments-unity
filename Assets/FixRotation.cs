using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{
    float yaw;
    void Update()
    {
        transform.eulerAngles = new Vector3(90f, 0f, -transform.parent.eulerAngles.y);
    }
}
