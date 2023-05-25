using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//FileHandler
using System.IO;
using System.Threading;


public class LandmarkProcesser : MonoBehaviour
{
    string processorURL = "http://3.135.214.5/fetch_pose";
    public string videoPath;
    public string poseJsonFilename = "test.json";

    Thread generationThread = null;
    public bool generating = false;
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
