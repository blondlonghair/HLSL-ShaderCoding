using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGUI : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "Click Me!"))
        {
            Debug.Log("I have been clicked!");
        }
    }
}
