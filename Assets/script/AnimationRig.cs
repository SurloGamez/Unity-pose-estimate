using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRig : MonoBehaviour
{
    public bool useNormalizeAppendage;
    
    private AnimationLandmark anim;
    public LandmarkHandler landmarkHandler;
    public string walkAnim = "walk", runAnim = "run", jumpAnim = "jump";

    public int poseIndex = 0;
    public float poseScale = 1;
    public GameObject landmarkPointPrefab;
    private GameObject[] landmarkPoints = new GameObject[33];
    public Vector2Int[] lineBones;

    //public float frameRate = 4; //frames per second
    public int referenceLandmarkIndex = 24;
    public int referenceLandmarkPoseIndex = 0;
    private Vector3 referenceLandmark;

    public Joint shoulder_right, shoulder_left, elbow_right, elbow_left, hip_right, hip_left, knee_right, knee_left;
    public JointCompound body;
    private List<Joint> joints = new List<Joint>();
    public Transform targetTransform;
    private Transform bodyTransform;
    private Vector3 bodyRotationOffset;

    private string jointRigParentSuffix = "_parent";

    public bool smoothRotate = true;

    private System.Action animationCompleteCallback;


    public bool splitJoint = false;
    public bool absolute = false;

    // Start is called before the first frame update
    void Start()
    {
        anim = landmarkHandler.GetAnimationLandmark(walkAnim);
        animationCompleteCallback = defaultAnimationEndCallback;
        SetupModel();
        StartCoroutine(Animate());
    }

    //for AnimationRigEditor
    public void Setup()
    {
        animationCompleteCallback = defaultAnimationEndCallback;
        anim = landmarkHandler.GetAnimationLandmark(walkAnim);
        //animationCompleteCallback = defaultAnimationEndCallback;
        SetupModel();
    }
    //for AnimationRigEditor
    public void CleanUp()
    {
        //remove all _parent objects from joints
        foreach(Joint joint in joints)
        {
            Debug.Log($"Checking joint: {joint.joint.name}");
            while (joint.joint.name.Contains(jointRigParentSuffix) && joint.joint.childCount == 1)
            {
                Transform oldJoint = joint.joint;
                joint.joint = oldJoint.GetChild(0);
                oldJoint.GetChild(0).parent = oldJoint.parent;
                DestroyImmediate(oldJoint.gameObject);
            }
        }
    }
    private void SetupModel()
    {
        bodyTransform = body.joint;
        bodyRotationOffset = targetTransform.localEulerAngles - bodyTransform.localEulerAngles;

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
            if (!joint.joint.name.Contains( jointRigParentSuffix))
            {
                GameObject newJoint = new GameObject(joint.joint.name + jointRigParentSuffix);
                GameObject newJointX = new GameObject(joint.joint.name +"X" + jointRigParentSuffix);
                newJoint.transform.position = joint.joint.position;
                newJointX.transform.position = joint.joint.position;

                //newJoint.transform.forward = targetTransform.forward;
                //newJointX.transform.forward = targetTransform.forward;
                if (joint.appendage)
                {
                    newJoint.transform.up = joint.joint.position - joint.appendage.position;
                    newJointX.transform.up = joint.joint.position - joint.appendage.position;
                    //joint.lastAppendageAngle = newJoint.transform.up;
                }
                newJointX.transform.parent = newJoint.transform;
                newJoint.transform.parent = joint.joint.parent;
                joint.joint.parent = newJointX.transform;
                joint.joint = newJoint.transform;
                joint.jointX = newJointX.transform;
                joint.anim = this;
                joint.defaultAngle = joint.joint.localEulerAngles;
            }
            
        }

        GameObject landmarkContainer = new GameObject();
        landmarkContainer.name = "LandmarkPoints";
        for (int i = 0; i < landmarkPoints.Length; i += 1)
        {
            landmarkPoints[i] = Instantiate(landmarkPointPrefab);
            landmarkPoints[i].name = i.ToString();
            landmarkPoints[i].transform.parent = landmarkContainer.transform;
        }
        //NormalizeLandmarks(24);
        SetPose(poseIndex);
    }

    public bool autoUpdate = true;
    private void Update()
    {
        if (autoUpdate) AnimUpdate();
    }

    // Update is called once per frame
    public void AnimUpdate()
    {
        if (smoothRotate)
        {
            foreach (Joint joint in joints) joint.SmoothRotate();
        }
        foreach (Vector2Int points in lineBones)
        {
            DrawLine(landmarkPoints[points.x].transform.position, landmarkPoints[points.y].transform.position, Color.red);
        }

        DrawLine(landmarkPoints[24].transform.position, landmarkPoints[24].transform.position + anim.poseForward * poseScale, Color.blue);
    }
    [HideInInspector]public float nextFrame = 0;
    private IEnumerator Animate()
    {
        while (true)
        {
            if (anim.frameRate > 0)
            {
                nextFrame = Time.time + 1 / anim.frameRate;
                yield return new WaitForSeconds(1 / anim.frameRate);
                IncrementPose();
            }
            else
            {
                yield return null;
            }

        }
    }

    public void UpdatePose()
    {
        SetPose(poseIndex);
    }
    public bool useVectors;
    private void SetPose(int poseIndex)
    {

        Landmark[] landmarks = anim.poses.poses[poseIndex].landmarks;
        for (int i = 0; i < anim.poses.poses[poseIndex].landmarks.Length; i += 1)
        {
            float x = landmarks[i].x;
            float y = -landmarks[i].y;
            float z = landmarks[i].z;
            landmarkPoints[i].transform.position = (new Vector3(x, y, z) + referenceLandmark) * poseScale;
        }

        
        foreach (Joint joint in joints)
        {
            if (useVectors)
            {
                joint.SetJointAngle(landmarkPoints);
            }
            else
            {
                joint.SetJointAngle(landmarkPoints,anim.poseForward,anim.poseRight,anim.poseUp);
            }
            
        }
        bodyTransform.localEulerAngles = targetTransform.localEulerAngles - bodyRotationOffset;


    }

    public void DecrementPose()
    {
        poseIndex -= 1;
        if (poseIndex < 0) poseIndex = anim.poses.poses.Length - 1;
        SetPose(poseIndex);
    }

    public void IncrementPose()
    {
        poseIndex = (poseIndex + 1); //% anim.poses.poses.Length;
        if(poseIndex >= anim.poses.poses.Length)
        {
            animationCompleteCallback();
            poseIndex = 0;
        }
        SetPose(poseIndex);
    }

    public void SetFrame()
    {
        Landmark[] landmarks = anim.poses.poses[poseIndex].landmarks;
        for (int i = 0; i < anim.poses.poses[poseIndex].landmarks.Length; i += 1)
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
        string json = JsonUtility.ToJson(anim.poses);
        Utility.CreateFile(json, anim.jsonFile.name.Replace(".json", "") + "_changes.json");
    }

    private void DrawLine(Vector3 a, Vector3 b, Color color)
    {
        Debug.DrawLine(a, b, color);
    }

    public Vector3 NormalizeAppendage(Vector3 appendage, Transform targetTransform)
    {
        //map pose onto world space
        float forward = Vector3.Dot(anim.poseForward, appendage);
        float up = Vector3.Dot(anim.poseUp, appendage);
        float right = Vector3.Dot(anim.poseRight, appendage);

        Debug.Log($"{right} {up} {forward}");

        return (targetTransform.forward * forward + targetTransform.up * up + targetTransform.right * right).normalized;
    }

    void defaultAnimationEndCallback() { }
    public void ChangeAnimation(string name)
    {
        ChangeAnimation(name, defaultAnimationEndCallback);
    }

    public void ChangeAnimation(string name, System.Action callBack)
    {
        if(anim.name != name)
        {
            if(name == "walk")
            {
                anim = landmarkHandler.GetAnimationLandmark(walkAnim);
                poseIndex = 0;
            }else if(name == "run")
            {
                anim = landmarkHandler.GetAnimationLandmark(runAnim);
                poseIndex = 0;
            }else if(name == "jump")
            {
                anim = landmarkHandler.GetAnimationLandmark(jumpAnim);
                poseIndex = 0;
            }
        }
        animationCompleteCallback = callBack;
    }

    private void NormalizeLandmarks(int refrenceLandmarkIndex)
    {
        for (int poseIndex = 0; poseIndex < anim.poses.poses.Length; poseIndex += 1)
        {
            for (int i = 0; i < anim.poses.poses[poseIndex].landmarks.Length; i += 1)
            {
                if (i != refrenceLandmarkIndex)
                {
                    Landmark refrenceLandmark = anim.poses.poses[i].landmarks[refrenceLandmarkIndex];
                    Landmark[] landmarks = anim.poses.poses[poseIndex].landmarks;
                    landmarks[i].x = landmarks[i].x - refrenceLandmark.x;
                    landmarks[i].y = landmarks[i].y - refrenceLandmark.y;
                    landmarks[i].z = landmarks[i].z - refrenceLandmark.z;
                }
                else
                {
                    Landmark[] landmarks = anim.poses.poses[poseIndex].landmarks;
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

