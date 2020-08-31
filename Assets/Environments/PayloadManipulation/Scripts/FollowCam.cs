using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public GameObject subject;
    public float height = 5f;
    
    void Update()
    {
        var pos = subject.transform.position;
        var rot = transform.eulerAngles;
        pos.y = height;
        rot.z = -subject.transform.eulerAngles.y;
        rot.y = 0f;
        transform.position = pos;
        transform.eulerAngles = rot;
    }
}
