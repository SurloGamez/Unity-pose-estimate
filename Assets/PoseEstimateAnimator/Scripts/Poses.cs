using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseEstimateAnimator
{
    [System.Serializable]
    public class Poses
    {
        public Pose[] poses;

    }
    [System.Serializable]
    public class Pose
    {
        public Landmark[] landmarks;
    }
    [System.Serializable]
    public class Landmark
    {
        public float x;
        public float y;
        public float z;
        public float visibility;
    }



}