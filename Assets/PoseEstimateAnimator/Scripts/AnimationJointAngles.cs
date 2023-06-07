
using UnityEngine;

namespace PoseEstimateAnimator
{
    [System.Serializable]
    public class AnimationJointAngles
    {
        public string animationName;
        public JointAngles[] animationAngles;
        public AnimationLandmark anim;
        public float frameRate;
        public int poseCount { get { return animationAngles[0].angles.Length; } }

        [System.Serializable]
        public class JointAngles
        {
            public string jointID;
            public Vector3[] angles;
        }

        public Vector3[] GetJointAngles(string jointID)
        {
            foreach (JointAngles jointAngles in animationAngles)
            {
                if (jointAngles.jointID == jointID) return jointAngles.angles;
            }
            return null;
        }
    }
}