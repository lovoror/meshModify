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
        if (handleExample == null)
        {
            return;
        }

        Handles.BeginGUI();
        input = GUILayout.TextField(input);
        if(input != oldInput)
        {
            int.TryParse(input, out range);
            handleExample.Init();
            oldInput = input;
        }
        Handles.EndGUI();

        List<int> indexArray = handleExample.CanGoIndex(range);

        Handles.color = Color.blue;
      
            

            for (var j = 0; j < indexArray.Count; j++)
            {
            
                    var v1 = handleExample.VertexArray[indexArray[j]];
                    Handles.Label(v1, indexArray[j].ToString());
                
              
            }
             
              
        
        
        Handles.EndGUI();
       



    }
}