using UnityEngine;
using UnityEngine.InputSystem;

namespace Afterrust
{
    public sealed class AfterrustInteraction : MonoBehaviour
    {
        public Camera viewCamera;
        public float maxDistance = 3.5f;
        public LayerMask interactableLayers = ~0;
        public AfterrustInventory inventory;

        private SalvageablePart target;
        private bool nearTrader;
        private string feedback = "Terk edilmis sanayide ilk hurda seferine hos geldin.";

        private void Awake()
        {
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            if (inventory == null) inventory = GetComponent<AfterrustInventory>();
        }

        private void Update()
        {
            target = null;
            nearTrader = false;
            if (viewCamera != null && Physics.Raycast(viewCamera.transform.position,
                viewCamera.transform.forward, out RaycastHit hit, maxDistance, interactableLayers))
            {
                target = hit.collider.GetComponentInParent<SalvageablePart>();
                nearTrader = hit.collider.GetComponentInParent<AfterrustTrader>() != null;
            }

            if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;
            if (target != null)
            {
                if (target.TrySalvage(inventory))
                    feedback = "Sokuldu: " + target.displayName + " (+" + target.SaleValue + " K)";
                else
                    feedback = "Sokulemedi: alet eksik veya cantan dolu.";
            }
            else if (nearTrader && inventory != null)
                feedback = "Satis tamamlandi: +" + inventory.SellAll() + " K";
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2f - 15f, Screen.height / 2f - 15f, 30f, 30f), "+", style);
            string hint = target != null ? "E - Sok: " + target.displayName + " (%" + target.condition + ")"
                : nearTrader ? "E - Cantadaki hurdalari sat" : "WASD hareket | Fare bakis | Shift kos | Space zipla | ESC fare serbest";
            GUI.Label(new Rect(0f, Screen.height - 100f, Screen.width, 28f), hint, style);
            GUI.Label(new Rect(0f, Screen.height - 64f, Screen.width, 30f), feedback, style);
            if (inventory != null)
            {
                GUI.Label(new Rect(12f, 12f, 460f, 27f), "Yuk: " +
                    inventory.CurrentMassKg.ToString("0.0") + "/" + inventory.capacityKg +
                    " kg    Kredi: " + inventory.credits, GUI.skin.box);
            }
        }
    }

}
