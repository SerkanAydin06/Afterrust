using UnityEngine;
using UnityEngine.InputSystem;

namespace Afterrust
{
    /// <summary>
    /// Looks at IInteractable objects (loot, salvage, workbench, water, trader) and handles
    /// E (press or hold), F (buy at trader), quick-use keys and Tab. Drawing is done by SurvivalHud.
    /// </summary>
    public sealed class AfterrustInteraction : MonoBehaviour
    {
        public Camera viewCamera;
        public float maxDistance = 3.2f;
        public LayerMask interactableLayers = ~0;
        public AfterrustInventory inventory;
        public SurvivorVitals vitals;
        public SurvivalHud hud;

        public IInteractable Target { get; private set; }
        public string Prompt { get; private set; }
        public float HoldProgress { get; private set; }
        public string Feedback { get; private set; } = "Hayatta kal: yiyecek, su ve malzeme bul.";
        public float FeedbackTime { get; private set; }

        private IInteractable holdTarget;
        private float holdTimer;

        private void Awake()
        {
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            if (inventory == null) inventory = GetComponent<AfterrustInventory>();
            if (vitals == null) vitals = GetComponent<SurvivorVitals>();
            if (hud == null) hud = GetComponent<SurvivalHud>();
        }

        public void Notify(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            Feedback = message;
            FeedbackTime = Time.time;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            Target = null;
            Prompt = null;

            bool blocked = (vitals != null && vitals.IsDead) ||
                           (hud != null && (hud.IsPanelOpen || hud.PanelChangedFrame == Time.frameCount));
            if (blocked || kb == null)
            {
                ResetHold();
                return;
            }

            if (kb.digit1Key.wasPressedThisFrame) Notify(inventory?.ConsumeBest(ItemCategory.Food, vitals));
            if (kb.digit2Key.wasPressedThisFrame) Notify(inventory?.ConsumeBest(ItemCategory.Drink, vitals));
            if (kb.digit3Key.wasPressedThisFrame) Notify(inventory?.ConsumeBest(ItemCategory.Medical, vitals));

            if (viewCamera != null && Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward,
                    out RaycastHit hit, maxDistance, interactableLayers, QueryTriggerInteraction.Ignore))
            {
                Target = hit.collider.GetComponentInParent<IInteractable>();
            }
            if (Target != null) Prompt = Target.GetPrompt(this);

            if (Target is AfterrustTrader trader && kb.fKey.wasPressedThisFrame)
                Notify(trader.BuyWireCutter(inventory));

            if (Target == null || Target != holdTarget) ResetHold();
            if (Target == null) return;

            float needed = Target.HoldSeconds;
            if (needed <= 0f)
            {
                if (kb.eKey.wasPressedThisFrame) Notify(Target.Interact(this));
                return;
            }

            if (kb.eKey.isPressed)
            {
                holdTarget = Target;
                holdTimer += Time.deltaTime;
                HoldProgress = Mathf.Clamp01(holdTimer / needed);
                if (holdTimer >= needed)
                {
                    Notify(Target.Interact(this));
                    ResetHold();
                }
            }
            else ResetHold();
        }

        private void ResetHold()
        {
            holdTarget = null;
            holdTimer = 0f;
            HoldProgress = 0f;
        }
    }
}
