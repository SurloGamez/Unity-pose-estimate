using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LandmarkProcesser))]
public class LandmarkProcessorEditor : Editor
{
    LandmarkProcesser targetManager;

    private void OnEnable()
    {
        targetManager = (LandmarkProcesser)target;
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();
        if (GUILayout.Button("Generate", GUILayout.Height(35)))
        {
            targetManager.Generate();
        }
       
    }
}
