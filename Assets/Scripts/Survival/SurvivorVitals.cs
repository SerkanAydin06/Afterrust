using System;
using UnityEngine;

namespace Afterrust
{
    /// <summary>Health, hunger, thirst and stamina. Values are 0..100.</summary>
    public sealed class SurvivorVitals : MonoBehaviour
    {
        public const float Max = 100f;

        [Range(0f, 100f)] public float health = 100f;
        [Range(0f, 100f)] public float hunger = 100f;
        [Range(0f, 100f)] public float thirst = 100f;
        [Range(0f, 100f)] public float stamina = 100f;

        [Header("Rates")]
        [Tooltip("Hunger lost per minute (100 / 4 = 25 minutes).")]
        public float hungerPerMinute = 4f;
        [Tooltip("Thirst lost per minute (100 / 6.5 = ~15 minutes).")]
        public float thirstPerMinute = 6.5f;
        public float sprintNeedMultiplier = 1.6f;
        [Tooltip("Health lost per second while hunger or thirst is empty (each).")]
        public float deprivationDamagePerSecond = 0.35f;
        [Tooltip("Health regained per second while hunger and thirst are above 60.")]
        public float regenPerSecond = 0.15f;
        public float staminaDrainPerSecond = 18f;
        public float staminaRegenPerSecond = 14f;
        public float jumpStaminaCost = 10f;

        public bool sprinting;
        public bool IsDead => health <= 0f;
        public event Action Died;

        private bool deathRaised;

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            if (IsDead) return;
            float needScale = sprinting ? sprintNeedMultiplier : 1f;
            hunger = Mathf.Max(0f, hunger - hungerPerMinute / 60f * dt * needScale);
            thirst = Mathf.Max(0f, thirst - thirstPerMinute / 60f * dt * needScale);

            float damage = 0f;
            if (hunger <= 0f) damage += deprivationDamagePerSecond;
            if (thirst <= 0f) damage += deprivationDamagePerSecond;
            if (damage > 0f) health -= damage * dt;
            else if (hunger > 60f && thirst > 60f) health += regenPerSecond * dt;
            health = Mathf.Clamp(health, 0f, Max);

            stamina = sprinting
                ? Mathf.Max(0f, stamina - staminaDrainPerSecond * dt)
                : Mathf.Min(Max, stamina + staminaRegenPerSecond * dt);

            if (IsDead && !deathRaised)
            {
                deathRaised = true;
                Died?.Invoke();
            }
        }

        public void Apply(float dHunger, float dThirst, float dHealth)
        {
            if (IsDead) return;
            hunger = Mathf.Clamp(hunger + dHunger, 0f, Max);
            thirst = Mathf.Clamp(thirst + dThirst, 0f, Max);
            health = Mathf.Clamp(health + dHealth, 0f, Max);
            if (IsDead && !deathRaised)
            {
                deathRaised = true;
                Died?.Invoke();
            }
        }

        public bool TryUseStamina(float amount)
        {
            if (stamina < amount) return false;
            stamina -= amount;
            return true;
        }

        /// <summary>Slower when badly hurt or exhausted.</summary>
        public float MovementMultiplier => health < 25f ? 0.8f : 1f;
    }
}
