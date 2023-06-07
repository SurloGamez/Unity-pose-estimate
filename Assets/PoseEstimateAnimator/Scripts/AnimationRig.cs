using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseEstimateAnimator
{
    public class AnimationRig : MonoBehaviour
    {

        public AnimationRigHandler animationRigHandler;
        private AnimationJointAngles animAngles;

        [HideInInspector] public int poseIndex = 0;

        public bool smoothRotate = true;

        public Joint shoulder_right, shoulder_left, elbow_right, elbow_left, hip_right, hip_left, knee_right, knee_left;
        public JointCompound body;
        private List<Joint> joints = new List<Joint>();

        public bool normalizeAppendage = true;
        public Transform targetTransform;

        // Start is called before the first frame update
        private void StartAnimation()
        {

            SetPose(poseIndex);
            StartCoroutine(Animate());
        }
        public void StartAnimation(string animationName)
        {
            SetupJoints();
            SetAnimation(animationRigHandler.GetAnimationJointAngles(animationName));
            StartAnimation();
        }
        public void StartAnimation(AnimationJointAngles animAngles)
        {
            SetupJoints();
            SetAnimation(animAngles);
            StartAnimation();
        }

        public void ChangeAnimation(string animationName)
        {
            if (animAngles.animationName != animationName)
            {
                poseIndex = 0;
                SetAnimation(animationRigHandler.GetAnimationJointAngles(animationName));
                SetPose(poseIndex);
            }

        }

        public void AnimUpdate()
        {
            if (smoothRotate)
            {
                foreach (Joint joint in joints) joint.SmoothRotate();
            }

        }
        [HideInInspector] public float nextFrameTime = 0;

        private IEnumerator Animate()
        {
            while (true)
            {
                if (animAngles.frameRate > 0)
                {
                    nextFrameTime = Time.time + 1 / animAngles.frameRate;
                    yield return new WaitForSeconds(1 / animAngles.frameRate);
                    IncrementPose();
                }
                else
                {
                    yield return null;
                }

            }
        }

        public void SetupRig()
        {
            SetupJoints();
        }

        protected virtual void SetupJoints()
        {
            body.jointID = "body";
            shoulder_right.jointID = "shoulder_right";
            shoulder_left.jointID = "shoulder_left";
            elbow_right.jointID = "elbow_right";
            elbow_left.jointID = "elbow_left";
            hip_right.jointID = "hip_right";
            hip_left.jointID = "hip_left";
            knee_right.jointID = "knee_right";
            knee_left.jointID = "knee_left";
            joints.Add(body);
            joints.Add(shoulder_right);
            joints.Add(shoulder_left);
            joints.Add(elbow_right);
            joints.Add(elbow_left);
            joints.Add(hip_right);
            joints.Add(hip_left);
            joints.Add(knee_right);
            joints.Add(knee_left);

            for (int i = 0; i < joints.Count; i += 1)
            {
                Joint joint = joints[i];
                if (!joint.joint.name.Contains("_parent"))
                {
                    GameObject newJoint = new GameObject(joint.joint.name + "_parent");
                    newJoint.transform.position = joint.joint.position;
                    if (joint.appendage)
                    {
                        newJoint.transform.up = joint.joint.position - joint.appendage.position;
                    }

                    newJoint.transform.parent = joint.joint.parent;
                    joint.joint.parent = newJoint.transform;
                    joint.joint = newJoint.transform;
                }

                joint.animRig = this;

            }
        }


        public void SetAnimation(AnimationJointAngles anim)
        {
            this.animAngles = anim;
            foreach (Joint joint in joints)
            {
                joint.localAngles = anim.GetJointAngles(joint.jointID);
            }
        }

        private void SetPose(int poseIndex)
        {
            foreach (Joint joint in joints) { joint.SetJointAngle(poseIndex); }
        }


        AnimationJointAngles jointAngles;

        public AnimationJointAngles GenerateJointAngles(int poseCount)
        {
            jointAngles = new AnimationJointAngles();
            jointAngles.animationAngles = new AnimationJointAngles.JointAngles[joints.Count];
            for (int i = 0; i < joints.Count; i++)
            {
                string jointID = joints[i].jointID;
                jointAngles.animationAngles[i] = new AnimationJointAngles.JointAngles();
                jointAngles.animationAngles[i].jointID = jointID;
                jointAngles.animationAngles[i].angles = new Vector3[poseCount];

            }

            return jointAngles;
        }
        /// <summary>
        /// for generation jointAngles only
        /// </summary>
        /// <param name="landmarkPoints"></param>
        /// <param name="poseIndex"></param>
        public void SetJointAngles(GameObject[] landmarkPoints, int poseIndex, AnimationLandmark anim)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                jointAngles.animationAngles[i].angles[poseIndex] = joints[i].GetJointAngle(landmarkPoints, anim);
            }
        }

        public void SetJointAngles(GameObject[] landmarkPoints, AnimationLandmark anim)
        {
            for (int i = 0; i < joints.Count; i++)
            {

                joints[i].GetJointAngle(landmarkPoints, anim);

            }

        }

        public void DecrementPose()
        {
            poseIndex -= 1;
            if (poseIndex < 0) poseIndex = animAngles.poseCount - 1;
            SetPose(poseIndex);
        }

        public void IncrementPose()
        {
            poseIndex = (poseIndex + 1) % animAngles.poseCount;
            SetPose(poseIndex);
        }

        //------------------------------------- Joint ----------------------------------------------------------
        [System.Serializable]
        public class Joint
        {
            public string jointID { get { return _jointID; } set { _jointID = value; } }
            protected string _jointID;
            public int A, B, C; // A: Root  B: Joint  C: appendage
            public Transform joint;
            public Transform appendage;
            public AnimationRig animRig;
            public Vector3[] localAngles;
            public Vector3 lastAppendageAngle;

            public virtual void SetJointAngle(int poseIndex)
            {
                if (animRig.smoothRotate) lastAppendageAngle = localAngles[poseIndex];
                else joint.localEulerAngles = localAngles[poseIndex];
            }

            public virtual Vector3 GetJointAngle(GameObject[] landmarkPoints, AnimationLandmark anim)
            {
                Vector3 appendage = (landmarkPoints[C].transform.position - landmarkPoints[B].transform.position).normalized;
                if (animRig.normalizeAppendage) appendage = NormalizeAppendage(appendage, animRig.targetTransform, anim);
                joint.transform.up = -appendage;
                return joint.localEulerAngles;
            }

            protected Vector3 NormalizeAppendage(Vector3 appendage, Transform targetTransform, AnimationLandmark anim)
            {
                //map pose onto world space
                float forward = Vector3.Dot(anim.poseForward, appendage);
                float up = Vector3.Dot(anim.poseUp, appendage);
                float right = Vector3.Dot(anim.poseRight, appendage);

                return (targetTransform.forward * forward + targetTransform.up * up + targetTransform.right * right).normalized;
            }

            public virtual void SmoothRotate()
            {
                float t = Time.deltaTime;
                Vector3 nextAngle = lastAppendageAngle;
                if (t < animRig.nextFrameTime - Time.time)
                {
                    float timeRemaining = animRig.nextFrameTime - Time.time;
                    float xDelta = Mathf.LerpAngle(joint.transform.localEulerAngles.x, lastAppendageAngle.x, t / timeRemaining);
                    float yDelta = Mathf.LerpAngle(joint.transform.localEulerAngles.y, lastAppendageAngle.y, t / timeRemaining);
                    float zDelta = Mathf.LerpAngle(joint.transform.localEulerAngles.z, lastAppendageAngle.z, t / timeRemaining);
                    nextAngle = new Vector3(xDelta, yDelta, zDelta);
                }

                joint.transform.localEulerAngles = nextAngle;
            }
        }
        [System.Serializable]
        public class JointCompound : Joint
        {
            public int D; // A: Root  B: Joint  C: appendage

            public override Vector3 GetJointAngle(GameObject[] landmarkPoints, AnimationLandmark anim)
            {
                Vector3 CD = (Vector3.Distance(landmarkPoints[C].transform.position, landmarkPoints[D].transform.position) / 2) *
                    (landmarkPoints[D].transform.position - landmarkPoints[C].transform.position).normalized + landmarkPoints[C].transform.position;
                Vector3 AB = (Vector3.Distance(landmarkPoints[A].transform.position, landmarkPoints[B].transform.position) / 2) *
                    (landmarkPoints[B].transform.position - landmarkPoints[A].transform.position).normalized + landmarkPoints[A].transform.position;

                Vector3 appendage = (AB - CD).normalized;
                if (animRig.normalizeAppendage) appendage = NormalizeAppendage(appendage, animRig.targetTransform, anim);
                joint.transform.up = appendage;

                return joint.localEulerAngles;
            }
        }




    }
}