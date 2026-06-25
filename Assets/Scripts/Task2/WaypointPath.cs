using System.Collections.Generic;
using UnityEngine;

namespace Game.Task2
{
    public class WaypointPath : MonoBehaviour
    {
        [Header("Path Settings")]
        [SerializeField] private bool isLooping = false;
        [SerializeField] private Color pathColor = Color.cyan;
        [SerializeField] private float waypointSize = 0.5f;

        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        public bool IsLooping => isLooping;
        public int WaypointCount => waypoints.Count;

        private void OnDrawGizmos()
        {

            if (waypoints.Count == 0 && transform.childCount > 0)
            {
                PopulateFromChildren();
            }

            if (waypoints.Count == 0) return;

            Gizmos.color = pathColor;

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.DrawSphere(waypoints[i].position, waypointSize);

                if (i < waypoints.Count - 1)
                {
                    if (waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
                else if (isLooping)
                {
                    if (waypoints[0] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                    }
                }
            }
        }

        [ContextMenu("Populate From Children")]
        public void PopulateFromChildren()
        {
            waypoints.Clear();
            foreach (Transform child in transform)
            {
                waypoints.Add(child);
            }
        }

        public Transform GetWaypoint(int index)
        {
            if (index < 0 || index >= waypoints.Count) return null;
            return waypoints[index];
        }

        public int GetNextWaypointIndex(int currentIndex)
        {
            int nextIndex = currentIndex + 1;
            if (nextIndex >= waypoints.Count)
            {
                return isLooping ? 0 : -1;
            }
            return nextIndex;
        }

        public Vector3 GetPoint(int index)
        {
            Transform wp = GetWaypoint(index);
            return wp != null ? wp.position : Vector3.zero;
        }
    }
}