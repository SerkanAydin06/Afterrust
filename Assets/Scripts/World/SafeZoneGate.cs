using UnityEngine;

namespace Afterrust
{
    /// <summary>
    /// Two-leaf metal perimeter gate. The IInteractable lives on the common root,
    /// so pointing at either panel opens/closes BOTH leaves with one E press.
    /// Uses the same save model as shelter doors (via SaveSystem).
    /// </summary>
    public sealed class SafeZoneGate : MonoBehaviour, IInteractable
    {
        public Transform leftHinge;
        public Transform rightHinge;
        [Range(45f, 125f)] public float openAngle = 100f;
        [Min(10f)] public float degreesPerSecond = 115f;
        public bool isOpen;

        private float leftClosedYaw, rightClosedYaw;
        private float leftAngle, rightAngle;
        private bool initialized;

        public float HoldSeconds => 0f;
        public float LeftAngle => leftAngle;
        public float RightAngle => rightAngle;

        private void Awake() { Init(); }

        private void Init()
        {
            if (initialized || leftHinge == null || rightHinge == null) return;
            initialized = true;
            leftClosedYaw = leftHinge.localEulerAngles.y;
            rightClosedYaw = rightHinge.localEulerAngles.y;
            leftAngle = isOpen ? -openAngle : 0f;
            rightAngle = isOpen ? openAngle : 0f;
            ApplyRotation();
        }

        public string GetPrompt(AfterrustInteraction player)
        {
            return isOpen ? "E - Güvenli bölge kapısını kapat" : "E - Güvenli bölge kapısını aç";
        }

        public string Interact(AfterrustInteraction player)
        {
            SetOpen(!isOpen, false);
            return isOpen ? "Güvenli bölge kapısı açıldı." : "Güvenli bölge kapısı kapatıldı.";
        }

        public void SetOpen(bool open, bool instant)
        {
            Init();
            isOpen = open;
            if (!initialized || !instant) return;
            leftAngle = open ? -openAngle : 0f;
            rightAngle = open ? openAngle : 0f;
            ApplyRotation();
        }

        private void Update()
        {
            Init();
            if (!initialized) return;
            float leftTarget = isOpen ? -openAngle : 0f;
            float rightTarget = isOpen ? openAngle : 0f;
            leftAngle = Mathf.MoveTowards(leftAngle, leftTarget, degreesPerSecond * Time.deltaTime);
            rightAngle = Mathf.MoveTowards(rightAngle, rightTarget, degreesPerSecond * Time.deltaTime);
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            if (leftHinge != null)
                leftHinge.localRotation = Quaternion.Euler(0f, leftClosedYaw + leftAngle, 0f);
            if (rightHinge != null)
                rightHinge.localRotation = Quaternion.Euler(0f, rightClosedYaw + rightAngle, 0f);
        }
    }
}
