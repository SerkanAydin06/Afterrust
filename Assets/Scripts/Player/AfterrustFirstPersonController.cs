using UnityEngine;
using UnityEngine.InputSystem;

namespace Afterrust
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AfterrustFirstPersonController : MonoBehaviour
    {
        public Camera viewCamera;
        public float walkSpeed = 4f;
        public float sprintSpeed = 6f;
        public float mouseSensitivity = 0.13f;
        public float jumpHeight = 1.1f;
        public float gravity = -20f;
        [Tooltip("m/s^2; how quickly walking speed is reached or lost.")]
        public float acceleration = 14f;
        [Header("Camera feel")]
        public float headBobAmount = 0.03f;
        public float headBobFrequency = 1.9f;
        public float sprintFovBoost = 6f;

        private CharacterController controller;
        private SurvivorVitals vitals;
        private bool uiMode;
        private float pitch;
        private float verticalSpeed;
        private bool cursorLocked = true;
        private Vector3 planarVelocity;
        private Vector3 cameraBasePosition;
        private float baseFov;
        private float bobPhase;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            vitals = GetComponent<SurvivorVitals>();
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            if (viewCamera != null)
            {
                cameraBasePosition = viewCamera.transform.localPosition;
                baseFov = viewCamera.fieldOfView;
            }
            SetCursorLocked(true);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SetCursorLocked(false);
            else if (!uiMode) SetCursorLocked(true);
        }

        /// <summary>UI panels/death screen: free cursor, no movement input or look.</summary>
        public void SetUiMode(bool on)
        {
            if (uiMode == on) return;
            uiMode = on;
            SetCursorLocked(!on);
        }

        private void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || controller == null) return;

            bool inputBlocked = uiMode || (vitals != null && vitals.IsDead);
            if (!inputBlocked)
            {
                if (keyboard.escapeKey.wasPressedThisFrame) SetCursorLocked(false);
                if (!cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    SetCursorLocked(true);
            }

            float x = 0f, z = 0f;
            if (!inputBlocked)
            {
                x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                z = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            }
            Vector3 planar = Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
            bool wantsSprint = !inputBlocked && keyboard.leftShiftKey.isPressed && z > 0.1f;
            bool sprinting = wantsSprint && (vitals == null || vitals.stamina > 1f);
            if (vitals != null) vitals.sprinting = sprinting && controller.isGrounded;
            float speed = (sprinting ? sprintSpeed : walkSpeed) * (vitals != null ? vitals.MovementMultiplier : 1f);
            Vector3 targetVelocity = transform.TransformDirection(planar) * speed;
            // Ease in/out instead of instant start/stop; less control in the air.
            float accel = controller.isGrounded ? acceleration : acceleration * 0.3f;
            planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, accel * Time.deltaTime);
            Vector3 movement = planarVelocity;

            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            if (!inputBlocked && controller.isGrounded && keyboard.spaceKey.wasPressedThisFrame &&
                (vitals == null || vitals.TryUseStamina(vitals.jumpStaminaCost)))
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            movement.y = verticalSpeed;
            controller.Move(movement * Time.deltaTime);
            UpdateCameraFeel(sprinting);

            if (inputBlocked || !cursorLocked || Mouse.current == null || viewCamera == null) return;
            Vector2 look = Mouse.current.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(0f, look.x, 0f);
            pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
            viewCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        private void UpdateCameraFeel(bool sprinting)
        {
            if (viewCamera == null) return;
            Vector3 v = controller.velocity;
            float horizontal = new Vector2(v.x, v.z).magnitude;
            float amount = controller.isGrounded ? Mathf.Clamp01(horizontal / walkSpeed) * headBobAmount : 0f;
            // headBobFrequency = footsteps per second at walking speed.
            bobPhase += Time.deltaTime * headBobFrequency * Mathf.PI * horizontal / walkSpeed;
            Vector3 bob = new Vector3(Mathf.Cos(bobPhase) * amount * 0.5f, Mathf.Abs(Mathf.Sin(bobPhase)) * amount, 0f);
            Transform cam = viewCamera.transform;
            cam.localPosition = Vector3.Lerp(cam.localPosition, cameraBasePosition + bob, 1f - Mathf.Exp(-14f * Time.deltaTime));
            float targetFov = baseFov + (sprinting && horizontal > walkSpeed * 0.9f ? sprintFovBoost : 0f);
            viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, targetFov, 1f - Mathf.Exp(-8f * Time.deltaTime));
        }
    }
}
