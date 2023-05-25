using UnityEngine;

public class Vector : MonoBehaviour
{
    //Use these to get the GameObject's positions
    Vector2 m_MyFirstVector;
    Vector2 m_MySecondVector;

    float m_Angle;

    //You must assign to these two GameObjects in the Inspector
    public GameObject m_MyObject;
    public GameObject m_MyOtherObject;

    void Start()
    {
        //Initialise the Vector
        m_MyFirstVector = Vector2.zero;
        m_MySecondVector = Vector2.zero;
        m_Angle = 0.0f;
        Debug.Log(Utility.AngleBetweenPoints(1, 0));
        Debug.Log(Utility.AngleBetweenPoints(1, 1));
        Debug.Log(Utility.AngleBetweenPoints(0, 1));
        Debug.Log(Utility.AngleBetweenPoints(-1, 1));
        Debug.Log(Utility.AngleBetweenPoints(-1, 0));
        Debug.Log(Utility.AngleBetweenPoints(-1, -1));
        Debug.Log(Utility.AngleBetweenPoints(0, -1));
        Debug.Log(Utility.AngleBetweenPoints(1, -1));
    }
    int x = 0, y = 0;
    void Update()
    {
        
    }

    private void HandleVectorDisplay()
    {
        //Fetch the first GameObject's position
        m_MyFirstVector = new Vector2(m_MyObject.transform.position.x, m_MyObject.transform.position.y);
        //Fetch the second GameObject's position
        m_MySecondVector = new Vector2(m_MyOtherObject.transform.position.x, m_MyOtherObject.transform.position.y);
        //Find the angle for the two Vectors
        m_Angle = Vector2.Angle(m_MyFirstVector, m_MySecondVector);

        //Draw lines from origin point to Vectors
        Debug.DrawLine(Vector2.zero, m_MyFirstVector, Color.magenta);
        Debug.DrawLine(Vector2.zero, m_MySecondVector, Color.blue);

        //Log values of Vectors and angle in Console
        Debug.Log("MyFirstVector: " + m_MyFirstVector);
        Debug.Log("MySecondVector: " + m_MySecondVector);
        Debug.Log("Angle Between Objects: " + m_Angle);
    }

    void OnGUI()
    {
        //Output the angle found above
        GUI.Label(new Rect(25, 25, 200, 40), "Angle Between Objects" + m_Angle);
    }
}