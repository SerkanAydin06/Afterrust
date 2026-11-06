using UnityEngine;
using UnityEngine.InputSystem;

namespace Afterrust
{
    /// <summary>Hand torch on the camera; L toggles it.</summary>
    [RequireComponent(typeof(Light))]
    public sealed class Flashlight : MonoBehaviour
    {
        private Light torch;

        private void Awake()
        {
            torch = GetComponent<Light>();
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.lKey.wasPressedThisFrame) torch.enabled = !torch.enabled;
        }

        public bool On
        {
            get => torch != null && torch.enabled;
            set { if (torch != null) torch.enabled = value; }
        }
    }
}
