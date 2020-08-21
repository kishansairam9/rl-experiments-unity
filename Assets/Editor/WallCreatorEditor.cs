using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[CustomEditor(typeof(WallCreator))]
public class WallCreatorEditor : Editor
{
    GameObject map;
    private bool isPlaying = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        WallCreator wallCreator = (WallCreator)target;
        if (GUILayout.Button("Generate!"))
        {
            ClearMaps();
            if (!isPlaying)
                map = wallCreator.MakeMap("Map");
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
