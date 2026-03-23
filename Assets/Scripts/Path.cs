using UnityEditor;
using UnityEngine;

public class Path : MonoBehaviour
{
    public GameObject[] Waypoints;

    public Vector3 GetPosition(int index)
    {
        return Waypoints[index].transform.position;
    }
    private void OnDrawGizmos()
    {
        // A1: neu co waypoint thi ve duong di giua cac waypoint
        if (Waypoints.Length > 0)
        {
            for (int i = 0; i < Waypoints.Length; i++)
            {
                //A2:Code hien chu waypoint tren scene
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                Handles.Label
                (Waypoints[i].transform.position +
                Vector3.up * 0.7f, Waypoints[i].name, style);

                //A3: Code ve duong di giua cac waypoint
                if (i < Waypoints.Length - 1)
                {
                    Gizmos.color = Color.red; // Mau do
                    Gizmos.DrawLine(Waypoints[i].transform.position,
                    Waypoints[i + 1].transform.position);
                }

            }
        }
    }

}
