using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Hinged door on this transform's local Y axis. E toggles it; it swings smoothly.
    /// The leaf (with its collider) is a child, offset from the hinge.
    /// </summary>
    public sealed class Door : MonoBehaviour, IInteractable
    {
        [Tooltip("Open angle in degrees around the hinge (negative swings inwards here).")]
        public float openAngle = -95f;
        public float degreesPerSecond = 170f;
        public bool isOpen;

        private float closedYaw;
        private float angle;
        private bool initialised;

        public float HoldSeconds => 0f;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (initialised) return;
            initialised = true;
            closedYaw = transform.localEulerAngles.y;
            angle = isOpen ? openAngle : 0f;
            ApplyRotation();
        }

        public string GetPrompt(AfterrustInteraction player)
        {
            return isOpen ? "E - Kapıyı kapat" : "E - Kapıyı aç";
        }

        public string Interact(AfterrustInteraction player)
        {
            SetOpen(!isOpen, false);
            return null;
        }

        public void SetOpen(bool open, bool instant)
        {
            Init();
            isOpen = open;
            if (instant)
            {
                angle = isOpen ? openAngle : 0f;
                ApplyRotation();
            }
        }

        /// <summary>Current swing angle (0 = closed).</summary>
        public float Angle => angle;

        private void Update()
        {
            float target = isOpen ? openAngle : 0f;
            if (Mathf.Approximately(angle, target)) return;
            angle = Mathf.MoveTowards(angle, target, degreesPerSecond * Time.deltaTime);
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            transform.localRotation = Quaternion.Euler(0f, closedYaw + angle, 0f);
        }
    }
}
