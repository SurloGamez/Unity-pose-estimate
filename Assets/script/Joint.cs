using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Joint
{
    public int A, B, C; // A: Root  B: Joint  C: appendage
    public Transform joint;
    public Transform jointX;
    public Transform appendage;
    public AnimationRig anim;
    public Vector3 lastAppendageAngle;
    public Vector3 defaultAngle;

    public Transform weightedTarget;
    [Range(0, 1)]
    public float weight = 0;

    public virtual void SetJointAngle(GameObject[] landmarkPoints)
    {
        //Vector3 root = (landmarkPoints[B].transform.position - landmarkPoints[A].transform.position).normalized;
        Vector3 appendage = (landmarkPoints[C].transform.position - landmarkPoints[B].transform.position).normalized;
        Vector3 newAppendageAngle = appendage;
        if (anim.useNormalizeAppendage)
        {
            appendage = anim.NormalizeAppendage(appendage, anim.targetTransform);
        }

        if (weightedTarget)
        {
            float targetDelta = Vector3.Distance(weightedTarget.position, appendage) * weight;
            appendage = appendage + targetDelta * (weightedTarget.position - appendage).normalized;
        }
        if (!anim.smoothRotate) joint.transform.up = -appendage;// newAppendageAngle;
        lastAppendageAngle = -appendage;


    }

    
    public virtual void SetJointAngle(GameObject[] landmarkPoints, Vector3 forward, Vector3 right, Vector3 up)
    {


        //Vector3 landmarkAngle = AnglesBetweenPoints(landmarkPoints[A].transform.position, landmarkPoints[B].transform.position, landmarkPoints[C].transform.position);
        //joint.transform.localEulerAngles = landmarkAngle /*+ joint.transform.parent.localEulerAngles*/;
        ////joint.transform.rotation = Quaternion.Euler(landmarkAngle.x, landmarkAngle.y, landmarkAngle.z);

        Vector3 appendage = (landmarkPoints[C].transform.position - landmarkPoints[B].transform.position);
        float x = (new Vector3(appendage.x * right.x, appendage.y * right.y, appendage.z * right.z)).magnitude;
        float y = (new Vector3(appendage.x * up.x, appendage.y * up.y, appendage.z * up.z)).magnitude;
        float z = (new Vector3(appendage.x * forward.x, appendage.y * forward.y, appendage.z * forward.z)).magnitude;

        if (anim.splitJoint)
        {
            joint.transform.localEulerAngles = new Vector3(0, 0, AngleBetweenPoints(x, y));
            jointX.transform.localEulerAngles = new Vector3(AngleBetweenPoints(z, y), 0, 0);
        }
        else
        {
            jointX.transform.localEulerAngles = Vector3.zero;
            Vector3 appendageAngle = new Vector3(AngleBetweenPoints(z, y), 0, AngleBetweenPoints(x, y));
            if (anim.absolute)
            {
                joint.transform.rotation = Quaternion.Euler(appendageAngle);
            }
            else
            {
                joint.transform.localEulerAngles = appendageAngle;
            }



        }



        

        //if (lastAppendageAngle != null) joint.transform.Rotate(appendage - lastAppendageAngle);
        //lastAppendageAngle = appendageAngle;
    }

    public virtual void SmoothRotate()
    {
        float t = Time.deltaTime;
        Vector3 nextAngle = lastAppendageAngle;
        if (t < anim.nextFrame - Time.time)
        {
            float distance = Vector3.Distance(joint.transform.up, lastAppendageAngle);
            float timeRemaining = anim.nextFrame - Time.time;
            int stepsRemaining = (int)(timeRemaining / t);
            nextAngle = Vector3.MoveTowards(joint.transform.up, lastAppendageAngle, distance / stepsRemaining);
        }


        joint.transform.up = nextAngle;
        //joint.transform.localEulerAngles = new Vector3(joint.transform.localEulerAngles.x, 0, joint.transform.localEulerAngles.z);
    }

    protected virtual Vector3 AnglesBetweenPoints(Vector3 a, Vector3 b, Vector3 c)
    {

        float xAngle = AngleBetweenPoints(a.y, a.z, b.y, b.z, c.y, c.z);
        float yAngle = AngleBetweenPoints(a.x, a.z, b.x, b.z, c.x, c.z);
        float zAngle = AngleBetweenPoints(a.x, a.y, b.x, b.y, c.x, c.y);
        return new Vector3(xAngle, 0 /*yAngle*/, zAngle);
    }
    float AngleBetweenPoints(float a1, float a2, float b1, float b2, float c1, float c2)
    {
        return Vector2.Angle(new Vector2(a1 - b1, a2 - b2), new Vector2(c1 - b1, c2 - b2));
    }
    float AngleBetweenPoints(float adjecent, float opposite)
    {

        return Utility.AngleBetweenPoints(opposite, adjecent);
    }
}
[System.Serializable]
public class JointCompound : Joint
{
    public int D; // A: Root  B: Joint  C: appendage

    public override void SetJointAngle(GameObject[] landmarkPoints) // sets the body rotaiton
    {
        //Vector3 root = (landmarkPoints[B].transform.position - landmarkPoints[A].transform.position).normalized;
        Vector3 CD = (Vector3.Distance(landmarkPoints[C].transform.position, landmarkPoints[D].transform.position) / 2) *
            (landmarkPoints[D].transform.position - landmarkPoints[C].transform.position).normalized + landmarkPoints[C].transform.position;
        Vector3 AB = (Vector3.Distance(landmarkPoints[A].transform.position, landmarkPoints[B].transform.position) / 2) *
            (landmarkPoints[B].transform.position - landmarkPoints[A].transform.position).normalized + landmarkPoints[A].transform.position;

        Vector3 appendage = (AB - CD).normalized;
        Vector3 newAppendageAngle = appendage;
        if (anim.useNormalizeAppendage)
        {
            appendage = anim.NormalizeAppendage(appendage, anim.targetTransform);

        }

        //if (!anim.smoothRotate) joint.transform.up = appendage;// newAppendageAngle;
        lastAppendageAngle = appendage;
    }

    public override void SetJointAngle(GameObject[] landmarkPoints, Vector3 forward, Vector3 right, Vector3 up)
    {
        //base.SetJointAngle(landmarkPoints, forward, right, up);
    }

}
