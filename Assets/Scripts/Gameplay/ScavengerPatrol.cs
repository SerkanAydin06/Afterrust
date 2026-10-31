using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Simple transform-only NPC movement test. No rig or animation required.
    /// Movement offsets are relative to the character's saved scene position.
    /// </summary>
    public sealed class ScavengerPatrol : MonoBehaviour
    {
        [Min(0.1f)] public float moveSpeed = 1.2f;
        [Min(0f)] public float turnSpeedDegrees = 240f;
        [Min(0f)] public float waypointPauseSeconds = 0.7f;
        [Min(0.05f)] public float arrivalDistance = 0.1f;
        public Vector3[] relativeWaypoints =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(2.5f, 0f, 0f),
            new Vector3(2.5f, 0f, 2.5f),
            new Vector3(0f, 0f, 2.5f)
        };

        private Vector3 startPosition;
        private int waypointIndex = 1;
        private float pauseUntil;
        private Quaternion initialRotation;

        private void Awake()
        {
            startPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void Update()
        {
            if (relativeWaypoints == null || relativeWaypoints.Length < 2) return;
            if (Time.time < pauseUntil) return;

            Vector3 target = startPosition + initialRotation * relativeWaypoints[waypointIndex];
            Vector3 delta = target - transform.position;
            Vector3 flatDirection = Vector3.ProjectOnPlane(delta, Vector3.up);
            if (delta.sqrMagnitude <= arrivalDistance * arrivalDistance)
            {
                waypointIndex = (waypointIndex + 1) % relativeWaypoints.Length;
                pauseUntil = Time.time + waypointPauseSeconds;
                return;
            }

            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            if (flatDirection.sqrMagnitude > 0.001f && turnSpeedDegrees > 0f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation,
                    turnSpeedDegrees * Time.deltaTime);
            }
        }
    }
}
