using UnityEngine;

namespace Afterrust
{
    /// <summary>Crafting station: E opens the crafting panel.</summary>
    public sealed class Workbench : MonoBehaviour, IInteractable
    {
        public string displayName = "Tezgâh";
        public float HoldSeconds => 0f;

        public string GetPrompt(AfterrustInteraction player) { return "E - Üret: " + displayName; }

        public string Interact(AfterrustInteraction player)
        {
            if (player != null && player.hud != null) player.hud.OpenCrafting();
            return null;
        }
    }
}
