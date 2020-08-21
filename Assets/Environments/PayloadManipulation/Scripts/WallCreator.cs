using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WallCreator : MonoBehaviour
{
    public float minDoorWidth = 2f, maxDoorWidth = 4f;
    public GameObject wallPrefab = null;
    public bool makeRoomInstead = false;

    private Vector3 wallPrefabSize;
    void Start()
    {
        MakeMap();
    }

    public GameObject MakeMap(string name = "Map")
    {
        var map = new GameObject();
        for (int i = 0; i < 4; i++)
        {
            var wall = MakeWallWithDoor(10f, 0.2f, 2f, String.Format("Wall with door {0}", i));
            var x = (i % 2 == 0) ? 5f : 0f;
            var y = ((int)(i / 2f) > 0) ? 1f : -1f;
            wall.transform.position = new Vector3(x * y, 0f, (5 - x) * y);
            if (makeRoomInstead)
                wall.transform.eulerAngles = new Vector3(0f, (i % 2 == 0) ? 90f : 0f, 0f);
            else
                wall.transform.eulerAngles = new Vector3(0f, (i % 2 == 0) ? 0f : 90f, 0f);
            wall.transform.SetParent(map.transform);
        }
        map.name = name;
        return map;
    }
    // ----------------------------------------------------------------
    private GameObject MakeWallWithDoor(float wallLength, float wallWidth, float wallHeight, string name)
    {
        var wall1 = GameObject.Instantiate(wallPrefab);
        var wall2 = GameObject.Instantiate(wallPrefab);
        if (wallPrefabSize == null)
            wallPrefabSize = wall1.GetComponent<Renderer>().bounds.size;
        var doorWidth = Random.Range(minDoorWidth, maxDoorWidth);
        var doorPosition = Random.Range(doorWidth / 2f, wallLength - doorWidth / 2f);
        var wall1Length = doorPosition - doorWidth / 2f;
        var wall2Length = wallLength - wall1Length - doorWidth;
        wall1.transform.position = new Vector3(wall1Length / 2f, wallHeight/2f, 0f);
        wall1.transform.localScale = new Vector3(wall1Length, 2f, wallWidth);
        wall2.transform.position = new Vector3(doorPosition + doorWidth/2f + wall2Length/2f, wallHeight/2f, 0f);
        wall2.transform.localScale = new Vector3(wall2Length, 2f, wallWidth);
        var wall = new GameObject();
        wall.name = name;
        wall.transform.position = new Vector3(wallLength / 2f, 0f, 0f);
        wall1.transform.SetParent(wall.transform);
        wall2.transform.SetParent(wall.transform);
        return wall;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
