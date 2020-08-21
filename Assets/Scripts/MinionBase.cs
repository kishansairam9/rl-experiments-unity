using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;

public class MinionBase : Agent
{
    public ActionRange[] actionRange = new ActionRange[2];

    protected Rigidbody rigidBody;
    protected Vector3 selfSize;
    protected float[] defaultAction;
    protected Material agentMaterial;

    public override void Initialize()
    {
        try
        {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.centerOfMass = Vector3.zero;
            print(rigidBody.centerOfMass.y);
        }
        catch { }
        selfSize = Vector3.Scale(
            transform.GetChild(0).localScale,
            transform.GetChild(0).GetComponent<MeshFilter>().mesh.bounds.size
        );
        GenerateDefaultActions();
        AssignColor();
    }

    private void GenerateDefaultActions()
    {
        defaultAction = new float[actionRange.Length];
        for (var i = 0; i < actionRange.Length; i++)
            defaultAction[i] = actionRange[i].getDefaultInRange();
    }

    private void AssignColor()
    {
        var color = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.75f, 1f);
        agentMaterial = transform.GetChild(0).GetComponent<Renderer>().material;
        agentMaterial.color = color;
    }

    public void MoveAgent(float[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
            actions[i] = ScaleAction(actions[i], actionRange[i].min, actionRange[i].max);
        rigidBody.transform.Translate(new Vector3(0f, 0f, 1f) * actions[0] * Time.deltaTime);
        rigidBody.transform.Rotate(transform.up * actions[1] * Time.deltaTime * Mathf.Rad2Deg);
    }

    public override void OnActionReceived(float[] actions)
    {
        MoveAgent(actions);
    }

    public override void Heuristic(float[] actions)
    {
        if (actions == null)
        {
            Debug.LogWarning("Actions are NULL");
            return;
        }
        actions[0] = (
            Input.GetKey(KeyCode.W) ? +1f :
            Input.GetKey(KeyCode.S) ? -1f : defaultAction[0]
        );
        actions[1] = (
            Input.GetKey(KeyCode.A) ? -1f :
            Input.GetKey(KeyCode.D) ? +1f : defaultAction[1]
        );
    }
}
