//Create a folder and call it "Editor" if one doesn't already exist. Place this script in it.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// Create a 180 degrees wire arc with a ScaleValueHandle attached to the disc
// lets you visualize some info of the transform

[CustomEditor(typeof(HandleLabel))]
class HandleLabelEditor : Editor
{
    int range = 10;
  
    string input;
    string oldInput;
    void OnSceneGUI()
    {
        HandleLabel handleExample = (HandleLabel)target;
        if (handleExample == null ) 
        {
            return;
        }

        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize =10;

        Handles.BeginGUI();
        input = GUILayout.TextField(input);
        if(input != oldInput)
        {
            int.TryParse(input, out range);
            handleExample.Init();
            oldInput = input;
        }
        Handles.EndGUI();

        //List<int> indexArray = handleExample.CanGoIndex(range);

        Handles.color = Color.blue;
      
        if(handleExample.VertexArray == null)
            return;
        // if(range < handleExample.VertexArray.Length)
        // {
        //     var v1 = handleExample.VertexArray[range];
        //      Handles.Label(v1, range.ToString(), textStyle);
        // }
        List<Vector3> showList = new List<Vector3>();
        if(range < handleExample.VertexArray.Length)
        {
        
            for (var j = 700; j < 1100; j++)
            {
                var v1 = handleExample.VertexArray[j];
                if(showList.Contains(v1)){
                     Handles.Label(new Vector3(v1.x, v1.y + 0.005f, v1.z), j.ToString());
                }else{
                   Handles.Label(new Vector3(v1.x, v1.y, v1.z), j.ToString());
                   showList.Add(v1);      
                }
               
            }
        }
             
              
        
        
        Handles.EndGUI();
       



    }
}