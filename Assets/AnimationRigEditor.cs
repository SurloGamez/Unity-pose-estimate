using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationRig))]
public class AnimationRigEditor : Editor
{
    AnimationRig targetManager;

    private void OnEnable()
    {
        targetManager = (AnimationRig)target;
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        if (GUILayout.Button("-", GUILayout.Height(35)))
        {
            targetManager.DecrementPose();
        }
        if (GUILayout.Button("+", GUILayout.Height(35)))
        {
            targetManager.IncrementPose();
        }
        if (GUILayout.Button("Set Frame", GUILayout.Height(35)))
        {
            targetManager.SetFrame();
        }
        if (GUILayout.Button("Save Landmark Changes", GUILayout.Height(35)))
        {
            targetManager.SaveLandmarkChanges();
        }
    }
}
