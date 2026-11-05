namespace Afterrust
{
    /// <summary>Anything the player can look at and use with E.</summary>
    public interface IInteractable
    {
        /// <summary>Text shown under the crosshair while looking at it.</summary>
        string GetPrompt(AfterrustInteraction player);

        /// <summary>Seconds E must be held (0 = instant press).</summary>
        float HoldSeconds { get; }

        /// <summary>Performs the action; returns feedback text (or null).</summary>
        string Interact(AfterrustInteraction player);
    }
}
