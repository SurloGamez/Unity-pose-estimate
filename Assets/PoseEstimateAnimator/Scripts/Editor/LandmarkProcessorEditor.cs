using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PoseEstimateAnimator
{
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
            if (GUILayout.Button("Upload and Process Video", GUILayout.Height(35)))
            {
                targetManager.Generate();
            }

            if (targetManager.poseSetup)
            {
                if (GUILayout.Button("-", GUILayout.Height(35)))
                {
                    targetManager.DecrementPose();
                }
                if (GUILayout.Button("+", GUILayout.Height(35)))
                {
                    targetManager.IncrementPose();
                }
                if (GUILayout.Button("Clear Pose Display", GUILayout.Height(35)))
                {
                    //targetManager.SetupPoseDisplay();
                    targetManager.ClearPoseDisplay();
                }
            }
            else
            {
                if (GUILayout.Button("Setup Pose Display", GUILayout.Height(35)))
                {
                    targetManager.SetupPoseDisplay();
                }
            }



        }
    }

}
