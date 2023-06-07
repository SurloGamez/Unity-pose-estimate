using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseEstimateAnimator
{
    [CreateAssetMenu(fileName = "Landmark Handler", menuName = "Landmark Handler")]
    public class LandmarkHandler : ScriptableObject
    {
        [SerializeField] private List<AnimationLandmark> animationLandmarks;

        public AnimationLandmark GetAnimationLandmark(string name)
        {
            foreach (AnimationLandmark animationLandmark in animationLandmarks)
            {
                if (animationLandmark.name == name) return animationLandmark;
            }
            return null;
        }

        public void AddAnimationLandmark(AnimationLandmark animationLandmark)
        {
            animationLandmarks.Add(animationLandmark);
        }
    }
}