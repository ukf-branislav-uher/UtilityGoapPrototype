using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Vision))]
public class VisionVisualizer : Editor
{
    void OnSceneGUI()
    {
        Vision fow = (Vision) target;
        Handles.color = Color.green;
        if(fow != null)
        {
            Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.visionDistance);
            
            (Vector3 viewAngleA, Vector3 viewAngleB) = fow.DirFromAngle();
            Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.visionAngle);
            Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.visionAngle);

            Handles.color = Color.red;
            foreach (var target in fow.visibleTargets)
            {
                Handles.DrawLine(fow.transform.position, target.position);
            }
        }
    }
}
