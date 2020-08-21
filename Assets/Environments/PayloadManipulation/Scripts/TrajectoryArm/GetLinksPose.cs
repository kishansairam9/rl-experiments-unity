using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GetLinksPose : MonoBehaviour
{
    [ReadOnly]
    public Vector3 position;
    [ReadOnly]
    public Vector3 orientation;
    private bool track = false;
    public Dictionary<string, float> jointStates;
    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<Rigidbody>() != null)
            {
                child.gameObject.AddComponent<GetLinksPose>();
                print("Adding to " + GetFullName(child.gameObject));
            }
        }
    }

    void Update()
    {
        position = transform.position;
        orientation = transform.eulerAngles;
    }

    private string GetFullName(GameObject go)
    {
        string name = go.name;
        while (go.transform.parent != null)
        {

            go = go.transform.parent.gameObject;
            name = go.name + "/" + name;
        }
        return name;
    }
}
