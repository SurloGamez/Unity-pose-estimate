using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;
using System.IO;

[CustomEditor(typeof(python_manager))]
public class python_manager_editor : Editor
{
    python_manager targetManager;

    private void OnEnable()
    {
        targetManager = (python_manager)target;
    }

    public override void OnInspectorGUI()
    {
        
        if(GUILayout.Button("Launch Python Script", GUILayout.Height(35)))
        {
            string path = Application.dataPath + "/Python/mediapipe.py";
            Debug.Log(path);
            PythonRunner.RunFile(path);
        }

    }
}
