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
        [Tooltip("Body warmth: falls in rain/night outdoors, rises under a roof or by a fire.")]
        [Range(0f, 100f)] public float warmth = 70f;
        [Tooltip("Rest: falls while awake, restored by sleeping.")]
        [Range(0f, 100f)] public float energy = 100f;

        [Header("Environment (set by ThermalSensor)")]
        [Range(0f, 100f)] public float warmthTarget = 55f;
        public bool sheltered;

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
        [Tooltip("Warmth change per minute towards the environment target.")]
        public float warmthPerMinute = 8f;
        [Tooltip("Energy lost per minute awake (100 / 4.5 = ~22 minutes, about one in-game day).")]
        public float energyPerMinute = 4.5f;
        public float coldDamagePerSecond = 0.2f;
        public float exhaustionDamagePerSecond = 0.1f;

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
            float coldScale = IsCold ? 1.3f : 1f;   // shivering burns food
            hunger = Mathf.Max(0f, hunger - hungerPerMinute / 60f * dt * needScale * coldScale);
            thirst = Mathf.Max(0f, thirst - thirstPerMinute / 60f * dt * needScale);
            energy = Mathf.Max(0f, energy - energyPerMinute / 60f * dt * (sprinting ? 1.5f : 1f));
            warmth = Mathf.MoveTowards(warmth, warmthTarget + (sprinting ? 5f : 0f), warmthPerMinute / 60f * dt);

            float damage = 0f;
            if (hunger <= 0f) damage += deprivationDamagePerSecond;
            if (thirst <= 0f) damage += deprivationDamagePerSecond;
            if (warmth < 15f) damage += coldDamagePerSecond;
            if (energy <= 0f) damage += exhaustionDamagePerSecond;
            if (damage > 0f) health -= damage * dt;
            else if (hunger > 60f && thirst > 60f && warmth > 40f) health += regenPerSecond * dt;
            health = Mathf.Clamp(health, 0f, Max);

            float staminaCap = Tired ? 50f : Max;
            float regen = staminaRegenPerSecond * (Tired ? 0.4f : 1f) * (IsCold ? 0.7f : 1f);
            stamina = sprinting
                ? Mathf.Max(0f, stamina - staminaDrainPerSecond * dt)
                : Mathf.Min(staminaCap, stamina + regen * dt);
            if (stamina > staminaCap) stamina = staminaCap;

            RaiseDeathIfNeeded();
        }

        public bool IsCold => warmth < 30f;
        public bool Tired => energy < 15f;

        private void RaiseDeathIfNeeded()
        {
            if (IsDead && !deathRaised)
            {
                deathRaised = true;
                Died?.Invoke();
            }
        }

        public void ApplyWarmth(float dWarmth)
        {
            if (!IsDead) warmth = Mathf.Clamp(warmth + dWarmth, 0f, Max);
        }

        public void ApplyRest(float dEnergy)
        {
            if (!IsDead) energy = Mathf.Clamp(energy + dEnergy, 0f, Max);
        }

        public void Apply(float dHunger, float dThirst, float dHealth)
        {
            if (IsDead) return;
            hunger = Mathf.Clamp(hunger + dHunger, 0f, Max);
            thirst = Mathf.Clamp(thirst + dThirst, 0f, Max);
            health = Mathf.Clamp(health + dHealth, 0f, Max);
            RaiseDeathIfNeeded();
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
