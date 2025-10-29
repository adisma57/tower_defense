using UnityEditor;
using UnityEngine;

public class Path : MonoBehaviour
{
    public GameObject[] Waypoints;

    public Vector3 GetWaypointPosition(int index)
    {
        if (index < 0 || index >= Waypoints.Length)
        {
            Debug.LogError("Waypoint index out of range.");
            return Vector3.zero;
        }
        return Waypoints[index].transform.position;
    }


    // Draw lines between waypoints in the editor for visualization
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int i = 0; i < Waypoints.Length; i++)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(Waypoints[i].transform.position + Vector3.up * 0.7f, Waypoints[i].name, style);

            if (i < Waypoints.Length - 1)
            {
                Gizmos.DrawLine(Waypoints[i].transform.position, Waypoints[i + 1].transform.position);
            }
        }
    }
}
