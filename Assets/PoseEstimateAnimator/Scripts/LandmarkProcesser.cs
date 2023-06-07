using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//FileHandler
using System.IO;
using System.Threading;

namespace PoseEstimateAnimator
{
    public class LandmarkProcesser : MonoBehaviour
    {
        string processorURL = "http://3.135.214.5/fetch_pose";
        public string videoPath;
        public string poseJsonFilename = "test.json";
        public string animationName = "test";

        public AnimationRig animationRig;
        public LandmarkHandler landmarkHandler;
        public AnimationRigHandler animationRigHandler;

        public int poseIndex = 0;
        public float poseScale = 1;
        public GameObject landmarkPointPrefab;
        private GameObject[] landmarkPoints = new GameObject[33];

        private int[] lineBones = {
        11,12,
        24,23,
        12,14,
        16,14,
        16,22,
        16,18,
        20,18,
        20,16,
        11,13,
        15,13,
        15,21,
        15,17,
        19,17,
        19,15,
        23,25,
        27,25,
        27,29,
        31,29,
        31,27,
        12,24,
        26,24,
        26,28,
        32,28,
        32,30,
        28,30,
        11,23,
        7,3,
        2,3,
        2,1,
        0,1,
        0,4,
        5,4,
        5,6,
        8,6,
        9,10
    };

        private AnimationLandmark anim;

        //Thread generationThread = null;
        //public bool generating = false;
        public void Generate()
        {
            StartCoroutine(Upload());


        }

        IEnumerator Upload()
        {
            WWWForm form = new WWWForm();
            form.AddBinaryData("video", UnityFileHandler.ReadFile(videoPath));
            using (UnityWebRequest req = UnityWebRequest.Post(processorURL, form))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(req.error);
                }
                else
                {
                    Debug.Log("Creating json file");
                    UnityFileHandler.CreateFile(req.downloadHandler.text, poseJsonFilename);

                }
            }
        }

        private void Start()
        {
            SetupPoseDisplay();
            if (generateJointAngles)
            {
                StartCoroutine(GenerateJointAngles());
            }
            else
            {
                StartCoroutine(Animate());
            }

        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < lineBones.Length; i += 2)
            {
                int a = lineBones[i];
                int b = lineBones[i + 1];
                DrawLine(landmarkPoints[a].transform.position, landmarkPoints[b].transform.position, Color.red);
            }

            DrawLine(landmarkPoints[24].transform.position, landmarkPoints[24].transform.position + anim.poseForward * poseScale, Color.blue);
        }

        private IEnumerator Animate()
        {
            while (true)
            {
                if (anim.frameRate > 0)
                {
                    yield return new WaitForSeconds(1 / anim.frameRate);
                    IncrementPose();
                }
                else
                {
                    yield return null;
                }

            }
        }

        [HideInInspector] public bool poseSetup = false;
        public void SetupPoseDisplay()
        {
            anim = landmarkHandler.GetAnimationLandmark(animationName);
            for (int i = 0; i < landmarkPoints.Length; i += 1)
            {
                if (landmarkPoints[i] == null)
                    landmarkPoints[i] = Instantiate(landmarkPointPrefab);
            }
            poseSetup = true;

            if (animationRig)
            {
                animationRig.SetupRig();
                //animationRig.SetAnimation(anim);
            }
        }
        public void ClearPoseDisplay()
        {
            for (int i = 0; i < landmarkPoints.Length; i += 1)
            {
                if (landmarkPoints[i] != null)
                    DestroyImmediate(landmarkPoints[i].gameObject);
            }
            poseSetup = false;
        }

        public bool generateJointAngles;
        private IEnumerator GenerateJointAngles()
        {
            if (animationRig)
            {

                AnimationJointAngles animationJointAngles = animationRig.GenerateJointAngles(anim.poseCount);
                Debug.Log(anim.poseCount);
                animationJointAngles.animationName = anim.name;
                animationJointAngles.anim = anim;
                animationJointAngles.frameRate = anim.frameRate;
                for (int poseIndex = 0; poseIndex < anim.poseCount; poseIndex++)
                {
                    SetPose(poseIndex);
                    animationRig.SetJointAngles(landmarkPoints, poseIndex, anim);
                    yield return null;
                }
                animationRigHandler.AddAnimationJointAngle(animationJointAngles);

                //animationRig.SetAnimation(animationJointAngles);
                animationRig.StartAnimation(animationJointAngles);
            }
            else
            {
                Debug.LogWarning("Please Assign an Animation Rig");
            }
        }

        private void SetPose(int poseIndex)
        {
            Landmark[] landmarks = anim.poses.poses[poseIndex].landmarks;
            for (int i = 0; i < anim.poses.poses[poseIndex].landmarks.Length; i += 1)
            {
                float x = landmarks[i].x;
                float y = -landmarks[i].y;
                float z = landmarks[i].z;
                landmarkPoints[i].transform.position = (new Vector3(x, y, z)) * poseScale;


            }

            //display pose on model
            if (animationRig)
            {
                animationRig.SetJointAngles(landmarkPoints, anim);
            }
        }

        public void DecrementPose()
        {

            poseIndex -= 1;
            if (poseIndex < 0) poseIndex = anim.poses.poses.Length - 1;
            SetPose(poseIndex);
        }

        public void IncrementPose()
        {
            poseIndex = (poseIndex + 1) % anim.poses.poses.Length;
            SetPose(poseIndex);


        }

        private void DrawLine(Vector3 a, Vector3 b, Color color)
        {
            Debug.DrawLine(a, b, color);
        }
    }


    public static class UnityFileHandler
    {
        private static string DataFilePath = @"Data";

        public static string CreateFile(string content, string filename)
        {
            string relativePath = GetRelativePath(filename);


            using (StreamWriter streamWriter = File.CreateText(relativePath))
            {
                streamWriter.Write(content);
            }
            return Path.Combine(DataFilePath, filename);
        }

        public static byte[] ReadFile(string filename)
        {
            string relativePath = GetRelativePath(filename);
            byte[] file = File.ReadAllBytes(relativePath);
            return file;
        }

        private static string GetRelativePath(string filename)
        {
            string relativePath;
            if (Application.isEditor)
            {
                if (!Directory.Exists(Path.Combine("Assets", DataFilePath)))
                {
                    Directory.CreateDirectory(Path.Combine("Assets", DataFilePath));
                }
                relativePath = Path.Combine("Assets", DataFilePath, filename);
            }
            else
            {
                if (!Directory.Exists(DataFilePath))
                {
                    Directory.CreateDirectory(DataFilePath);
                }
                relativePath = Path.Combine(Application.persistentDataPath, DataFilePath, filename);
            }
            return relativePath;
        }
    }
}