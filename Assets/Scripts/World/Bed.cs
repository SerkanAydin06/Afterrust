using UnityEngine;

namespace Afterrust
{
    /// <summary>E on the bed: sleep (handled by the player's SleepSystem).</summary>
    public sealed class Bed : MonoBehaviour, IInteractable
    {
        public float HoldSeconds => 0f;

        public string GetPrompt(AfterrustInteraction player) { return "E - Uyu"; }

        public string Interact(AfterrustInteraction player)
        {
            SleepSystem sleep = player != null ? player.GetComponent<SleepSystem>() : null;
            return sleep != null ? sleep.TrySleep(this) : "Uyuyamazsın.";
        }
    }
}
