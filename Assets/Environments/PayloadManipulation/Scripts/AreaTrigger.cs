using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    public int resolution;
    public float areaSize;
    public Vector2 position;

    private SimpleTrigger[,] triggers;
    public void Init(int resolution, float areaSize, Vector2 position)
    {
        this.position = position;
        this.areaSize = areaSize;
        this.resolution = resolution;
    }

    public void populateTriggers(string colliderName)
    {
        var triggerSize = areaSize / resolution;
        Debug.Log("Making collider for " + colliderName);
        triggers = new SimpleTrigger[resolution, resolution];
        for (var i = 0; i < resolution; i++)
        {
            for (var j = 0; j < resolution; j++)
            {
                var name = String.Format("T ({0}, {1})", i, j);
                var obj = new GameObject(name);
                triggers[i, j] = obj.AddComponent<SimpleTrigger>();
                var pose = new Vector3(
                    i * triggerSize + triggerSize / 2 - areaSize / 2 + position.x, 1f,
                    j * triggerSize + triggerSize / 2 - areaSize / 2 + position.y);
                var dims = new Vector3(triggerSize, 2f, triggerSize);
                triggers[i, j].Init(pose, dims, this.transform, colliderName, name);
            }
        }
    }

    public int getNumberOfHits()
    {
        int c = 0;
        for (var i = 0; i < resolution; i++)
            for (var j = 0; j < resolution; j++)
                if (this.triggers[i, j].isActive())
                    c++;
        return c;
    }
}
