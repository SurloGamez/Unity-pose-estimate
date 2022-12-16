using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRig : MonoBehaviour
{
    public Vector3 poseForward, poseRight, poseUp;
    public string filename = "poseJson.json";
    public Poses poses;

    public int poseIndex = 0;
    public float poseScale = 1;
    public GameObject landmarkPointPrefab;
    private GameObject[] landmarkPoints = new GameObject[33];
    public Vector2Int[] lineBones;

    public float frameRate = 4; //frames per second
    public int referenceLandmarkIndex = 24;
    public int referenceLandmarkPoseIndex = 0;
    private Vector3 referenceLandmark;

    [System.Serializable]
    public class Joint
    {
        public int A, B, C; // A: Root  B: Joint  C: appendage
        public Transform joint;
        public Transform appendage;
        public AnimationRig anim;
        
        public virtual void SetJointAngle(GameObject[] landmarkPoints)
        {
            //Vector3 root = (landmarkPoints[B].transform.position - landmarkPoints[A].transform.position).normalized;
            Vector3 appendage = (landmarkPoints[C].transform.position - landmarkPoints[B].transform.position).normalized;
            appendage = anim.NormalizeAppendage(appendage, anim.targetTransform);
            joint.transform.up = -appendage;
        }
    }
    [System.Serializable]
    public class JointCompound : Joint
    {
        public int D; // A: Root  B: Joint  C: appendage

        public override void SetJointAngle(GameObject[] landmarkPoints)
        {
            //Vector3 root = (landmarkPoints[B].transform.position - landmarkPoints[A].transform.position).normalized;
            Vector3 CD = (Vector3.Distance(landmarkPoints[C].transform.position, landmarkPoints[D].transform.position) / 2) * 
                (landmarkPoints[D].transform.position - landmarkPoints[C].transform.position).normalized + landmarkPoints[C].transform.position;
            Vector3 AB = (Vector3.Distance(landmarkPoints[A].transform.position, landmarkPoints[B].transform.position) / 2) *
                (landmarkPoints[B].transform.position - landmarkPoints[A].transform.position).normalized + landmarkPoints[A].transform.position;

            Vector3 appendage = (AB - CD).normalized;
            appendage = anim.NormalizeAppendage(appendage, anim.targetTransform);
            joint.transform.up = appendage;
        }
    }
    public Joint shoulder_right, shoulder_left, elbow_right, elbow_left, hip_right, hip_left, knee_right, knee_left;
    public JointCompound body;
    private List<Joint> joints = new List<Joint>();
    public Transform targetTransform;

    // Start is called before the first frame update
    void Start()
    {
        joints.Add(body);
        joints.Add(shoulder_right);
        joints.Add(shoulder_left);
        joints.Add(elbow_right);
        joints.Add(elbow_left);
        joints.Add(hip_right);
        joints.Add(hip_left);
        joints.Add(knee_right);
        joints.Add(knee_left);

        for( int i = 0; i < joints.Count; i+=1)
        {
            Joint joint = joints[i];
            GameObject newJoint = new GameObject(joint.joint.name + "_parent");
            newJoint.transform.position = joint.joint.position;
            if (joint.appendage)
            {
                newJoint.transform.up = joint.joint.position - joint.appendage.position;
            }

            newJoint.transform.parent = joint.joint.parent;
            joint.joint.parent = newJoint.transform;
            joint.joint = newJoint.transform;
            joint.anim = this;
        }

        poses = Utility.GetJsonObject<Poses>(filename);
        for(int i = 0; i < landmarkPoints.Length; i += 1)
        {
            landmarkPoints[i] = Instantiate(landmarkPointPrefab);
        }
        //NormalizeLandmarks(24);
        SetPose(poseIndex);
        StartCoroutine(Animate());
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Vector2Int points in lineBones)
        {
            DrawLine(landmarkPoints[points.x].transform.position, landmarkPoints[points.y].transform.position, Color.red);
        }

        DrawLine(landmarkPoints[24].transform.position, landmarkPoints[24].transform.position  + poseForward * poseScale, Color.blue);
    }

    private IEnumerator Animate()
    {
        while (true)
        {
            if(frameRate > 0)
            {
                yield return new WaitForSeconds(1 / frameRate);
                IncrementPose();
            }
            else
            {
                yield return null;
            }
            
        }
    }

    private void SetPose(int poseIndex)
    {
        Landmark[] landmarks = poses.poses[poseIndex].landmarks;
        for (int i = 0; i < poses.poses[poseIndex].landmarks.Length; i += 1)
        {
            float x = landmarks[i].x;
            float y = -landmarks[i].y;
            float z = landmarks[i].z;
            landmarkPoints[i].transform.position = (new Vector3(x, y, z) + referenceLandmark) * poseScale ;
        }

        foreach(Joint joint in joints) { joint.SetJointAngle(landmarkPoints); }
    }

    public void DecrementPose()
    {
        poseIndex -= 1;
        if (poseIndex < 0) poseIndex = poses.poses.Length - 1;
        SetPose(poseIndex);
    }

    public void IncrementPose()
    {
        poseIndex = (poseIndex + 1) % poses.poses.Length;
        SetPose(poseIndex);
    }

    public void SetFrame()
    {
        Landmark[] landmarks = poses.poses[poseIndex].landmarks;
        for (int i = 0; i < poses.poses[poseIndex].landmarks.Length; i += 1)
        {
            float x = landmarkPoints[i].transform.position.x / poseScale - referenceLandmark.x;
            float y = -landmarkPoints[i].transform.position.y / poseScale - referenceLandmark.y;
            float z = landmarkPoints[i].transform.position.z / poseScale - referenceLandmark.z;
            landmarks[i].x = x;
            landmarks[i].y = y;
            landmarks[i].z = z;
        }
    }

    public void SaveLandmarkChanges()
    {
        string json = JsonUtility.ToJson(poses);
        Utility.CreateFile(json, filename.Replace(".json", "") + "_changes.json");
    }

    private void DrawLine(Vector3 a, Vector3 b, Color color)
    {
        Debug.DrawLine(a, b, color);
    }

    private Vector3 NormalizeAppendage(Vector3 appendage, Transform targetTransform)
    {
        //map pose onto world space
        float forward = Vector3.Dot(poseForward, appendage);
        float up = Vector3.Dot(poseUp, appendage);
        float right = Vector3.Dot(poseRight, appendage);

        //Debug.Log($"{right} {up} {forward}");

        return (targetTransform.forward * forward + targetTransform.up * up + targetTransform.right * right).normalized;
    }

    private void NormalizeLandmarks(int refrenceLandmarkIndex)
    {
        for(int poseIndex = 0; poseIndex < poses.poses.Length; poseIndex += 1)
        {
            for (int i = 0; i < poses.poses[poseIndex].landmarks.Length; i += 1)
            {
                if(i != refrenceLandmarkIndex)
                {
                    Landmark refrenceLandmark = poses.poses[i].landmarks[refrenceLandmarkIndex];
                    Landmark[] landmarks = poses.poses[poseIndex].landmarks;
                    landmarks[i].x = landmarks[i].x - refrenceLandmark.x;
                    landmarks[i].y = landmarks[i].y - refrenceLandmark.y;
                    landmarks[i].z = landmarks[i].z - refrenceLandmark.z;
                }
                else 
                {
                    Landmark[] landmarks = poses.poses[poseIndex].landmarks;
                    if (poseIndex == referenceLandmarkPoseIndex)
                    {
                        referenceLandmark = new Vector3(landmarks[i].x, landmarks[i].y, landmarks[i].z);
                    }
                    
                    landmarks[i].x = 0;
                    landmarks[i].y = 0;
                    landmarks[i].z = 0;
                }
            }
        }
        
    }
}
