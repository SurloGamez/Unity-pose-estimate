using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationRig2))]
public class AnimationRig2Editor : Editor
{
    AnimationRig2 targetManager;

    private void OnEnable()
    {
        targetManager = (AnimationRig2)target;
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();
        //if (GUILayout.Button("Set up animaiton rig", GUILayout.Height(35)))
        //{
        //    targetManager.Setup();
        //}
        //if(GUILayout.Button("Clean up animaiton rig", GUILayout.Height(35)))
        //{
        //    //remove '_parent' objects
        //    Debug.Log("Cleaning Up joints");
        //    targetManager.CleanUp();
        //}
        //if (GUILayout.Button("-", GUILayout.Height(35)))
        //{
        //    targetManager.DecrementPose();
        //}
        //if (GUILayout.Button("+", GUILayout.Height(35)))
        //{
        //    targetManager.IncrementPose();
        //}
        //if (GUILayout.Button("Set Frame", GUILayout.Height(35)))
        //{
        //    targetManager.SetFrame();
        //}
        //if (GUILayout.Button("Save Landmark Changes", GUILayout.Height(35)))
        //{
        //    targetManager.SaveLandmarkChanges();
        //}
    }
}
