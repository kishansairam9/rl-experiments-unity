using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(BadRoom))]
public class BadRoomEditor : Editor
{
    GameObject map;
    private bool isPlaying = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BadRoom badRoom = (BadRoom)target;
        if (GUILayout.Button("Generate!"))
        {
            ClearMaps();
            if (!isPlaying)
                map = badRoom.MakeMap("Map");
        }
        if (GUILayout.Button("Clear") && !isPlaying)
        {
            ClearMaps();
        }
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    public void OnPlayModeChanged(PlayModeStateChange state)
    {
        isPlaying = state.Equals(PlayModeStateChange.EnteredPlayMode);
        ClearMaps();
    }

    private void ClearMaps()
    {
        DestroyImmediate(map);
    }
}
