using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPayloadVertices : MonoBehaviour
{

    private Vector3[] vertices;
    void Start()
    {
        vertices = transform.GetComponent<MeshFilter>().mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        print("-------------------------------------");
        foreach (var vertex in vertices){
            print(localToWorld.MultiplyPoint3x4(vertex));
        }
        print("-------------------------------------");
    }
}
