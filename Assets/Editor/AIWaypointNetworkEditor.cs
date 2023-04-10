using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(AIWaypointNetwork))]
public class AIWaypointNetworkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        AIWaypointNetwork network = (AIWaypointNetwork) target;
        network.PathDisplayMode =
            (PathDisplayMode) EditorGUILayout.EnumPopup("Path Display Mode:", network.PathDisplayMode);

        if (network.PathDisplayMode == PathDisplayMode.Paths)
        {
            network.UIStart = EditorGUILayout.IntSlider("Path Start", network.UIStart, 0, network.Waypoints.Count - 1);
            network.UIEnd = EditorGUILayout.IntSlider("Path Start", network.UIEnd, 0, network.Waypoints.Count - 1);
        }

        // this is needed for editor ui to update after value has been changed from the editor
        if (EditorGUI.EndChangeCheck())
        {
            EditorApplication.QueuePlayerLoopUpdate();
            GUI.FocusControl(null);
        }

        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork) target;

        for (int i = 0; i < network.Waypoints.Count; i++)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 20;
            Handles.Label(network.Waypoints[i].position, i.ToString(), style);
        }

        if (network.PathDisplayMode == PathDisplayMode.Connections)
        {
            List<Vector3> egg = network.Waypoints.Where(wp => wp != null).Select(wp => wp.position).ToList();
            egg.Add(network.Waypoints[0].position);
            Handles.DrawAAPolyLine(egg.ToArray());
        }
        else if (network.PathDisplayMode == PathDisplayMode.Paths)
        {
            NavMeshPath path = new NavMeshPath();
            if (network.Waypoints[network.UIStart] != null && network.Waypoints[network.UIEnd] != null)
            {
                Vector3 start = network.Waypoints[network.UIStart].position;
                Vector3 end = network.Waypoints[network.UIEnd].position;

                NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

                Handles.color = Color.yellow;
                Handles.DrawAAPolyLine(path.corners);
            }
        }
    }
}