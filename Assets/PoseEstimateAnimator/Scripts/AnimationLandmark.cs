using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseEstimateAnimator
{
    [System.Serializable]
    public class AnimationLandmark
    {
        public string name;
        public TextAsset jsonFile;
        //[SerializeField] float frameSpeed = 1;
        public float frameRate; //frames per second
        public Poses poses { get { return GetPoses(); } }
        public Vector3 poseForward, poseRight, poseUp;
        public int poseCount { get { return GetPoses().poses.Length; } }

        private Poses _poses;
        private Poses GetPoses()
        {
            if (_poses == null || _poses.poses.Length == 0)
            {
                _poses = Utility.GetJsonObject<Poses>(jsonFile.name + ".json");
                Debug.Log($"Getting Pose {_poses.poses.Length}");
            }

            return _poses;
        }


    }
}