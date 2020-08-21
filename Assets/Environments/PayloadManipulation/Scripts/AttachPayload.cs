using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachPayload : MonoBehaviour
{
    public Go2GoalSimple[] minions;

    void Start()
    {
    }

    void Update()
    {
        UpdateVertices();
    }

    void UpdateVertices()
    {
        var i = 0;
        foreach (Transform peg in transform)
        {
            minions[i].goal.x = peg.position.x;
            minions[i].goal.y = peg.position.z;
            i++;
            print(peg.position);
        }
    }
}
