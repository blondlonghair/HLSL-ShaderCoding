using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MyAwesomeEditorWindow : EditorWindow
{
    private int clickCounter;
    private float mySliderValue = 0.5f;
    
    [MenuItem("Window/My Awesome Editor")]
    static void Init()
    {
        var myWindow = EditorWindow.GetWindow<MyAwesomeEditorWindow>();
        myWindow.Show();
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        GUI.skin.button.fontSize = 36;
        if (GUILayout.Button("Click Me!"))
        {
            ++clickCounter;
        }

        GUI.skin.label.fontSize = 36;
        string displayClicks = $"I have been clicked {clickCounter} times";
        GUILayout.Label(displayClicks);

        GUILayout.BeginHorizontal();

        GUI.skin.label.fontSize = 12;
        GUILayout.Label($"My Slider Value : {mySliderValue}");
        mySliderValue = GUILayout.HorizontalSlider(mySliderValue, 0.0f, 1.0f);
        
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
    }
}
