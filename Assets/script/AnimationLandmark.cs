using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimationLandmark
{
    public string name;
    public TextAsset jsonFile;
    [SerializeField] float frameSpeed = 1;
    public float frameRate;
    public Poses poses { get { return GetPoses(); } }
    public Vector3 poseForward, poseRight, poseUp;

    private Poses _poses;
    private Poses GetPoses()
    {
        if(_poses == null)
            _poses = Utility.GetJsonObject<Poses>(jsonFile.name + ".json");
        return _poses;
    }

}
