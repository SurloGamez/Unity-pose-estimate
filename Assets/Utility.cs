using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Utility 
{
    static string pathToData = @"Resources";

    private static string GetRelativePath(string filename)
    {
        string relativePathToData;
        if (Application.isEditor)
        {
            relativePathToData = Path.Combine("Assets", pathToData, filename);
        }
        else
        {
            relativePathToData = filename;
        }
        return relativePathToData;
    }
    public static void CreateFile(string content, string filename)
    { 
        string relativePathToData = GetRelativePath(filename);
        using (StreamWriter streamWriter = File.CreateText(relativePathToData))
        {
            streamWriter.Write(content);
        }
    }
    public static string ReadFile(string filename)
    {
        
        string relativePathToData = GetRelativePath(filename);
        

        return File.ReadAllText(relativePathToData);
    }
    public static T GetJsonObject<T>(string filename)
    {
        return GetJson<T>(ReadFile(filename));
    }

    public static T GetJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
}
