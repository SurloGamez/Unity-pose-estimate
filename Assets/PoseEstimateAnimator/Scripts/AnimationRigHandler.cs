using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseEstimateAnimator
{
    [CreateAssetMenu(fileName = "Animation Rig Handler", menuName = "Pose Estimate/Animation Rig Handler")]
    public class AnimationRigHandler : ScriptableObject
    {
        [SerializeField] private List<AnimationJointAngles> animationJointAngles;

        public AnimationJointAngles GetAnimationJointAngles(string animationName)
        {
            foreach (AnimationJointAngles animation in animationJointAngles)
            {
                if (animation.animationName == animationName) return animation;
            }
            return null;
        }

        public void AddAnimationJointAngle(AnimationJointAngles animationJointAngle)
        {
            animationJointAngles.Add(animationJointAngle);
        }
    }
}