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

        private CharacterController controller;
        private float pitch;
        private float verticalSpeed;
        private bool cursorLocked = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            SetCursorLocked(true);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SetCursorLocked(false);
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

            if (keyboard.escapeKey.wasPressedThisFrame) SetCursorLocked(false);
            if (!cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                SetCursorLocked(true);

            float x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            float z = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            Vector3 planar = Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
            float speed = keyboard.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
            Vector3 movement = transform.TransformDirection(planar) * speed;

            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            if (controller.isGrounded && keyboard.spaceKey.wasPressedThisFrame)
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            movement.y = verticalSpeed;
            controller.Move(movement * Time.deltaTime);

            if (!cursorLocked || Mouse.current == null || viewCamera == null) return;
            Vector2 look = Mouse.current.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(0f, look.x, 0f);
            pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
            viewCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }
    }
}
