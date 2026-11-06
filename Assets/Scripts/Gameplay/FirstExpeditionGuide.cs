using UnityEngine;

namespace Afterrust
{
    public enum ExpeditionPhase { FindExit, SearchCheckpoint, ReturnToShelter, Complete }

    /// <summary>
    /// Small first-run objective derived from searchable world containers.
    /// No new save fields: a searched checkpoint crate already persists in SaveSystem.
    /// </summary>
    public sealed class FirstExpeditionGuide : MonoBehaviour
    {
        private const string ExpeditionRoot = "First Expedition - Roadside Checkpoint";
        private LootContainer[] containers;
        private AfterrustInteraction interaction;
        private bool reachedCheckpoint;
        private bool completed;
        private float nextCheck;

        public ExpeditionPhase Phase { get; private set; }

        public string ObjectiveText
        {
            get
            {
                switch (Phase)
                {
                    case ExpeditionPhase.FindExit:
                        return "HEDEF: Avlunun güney kapısından çık";
                    case ExpeditionPhase.SearchCheckpoint:
                        return "HEDEF: Yardım büfesi veya konteynerde erzak ara";
                    case ExpeditionPhase.ReturnToShelter:
                        return "HEDEF: Bulduklarınla sığınağa dön (kimyasaldan uzak dur)";
                    default:
                        return "İLK KEŞİF TAMAMLANDI  •  Hayatta kal ve geliş";
                }
            }
        }

        private void Awake()
        {
            interaction = GetComponent<AfterrustInteraction>();
            Phase = ExpeditionPhase.FindExit;
        }

        private void Start()
        {
            containers = FindObjectsByType<LootContainer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            CheckProgress();
        }

        private void Update()
        {
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + 0.5f;
            CheckProgress();
        }

        private void CheckProgress()
        {
            Vector3 p = transform.position;
            if (p.z < -25f) reachedCheckpoint = true;

            bool searched = false;
            if (containers != null)
            {
                foreach (LootContainer c in containers)
                {
                    if (c == null || !c.Rolled || c.transform.root.name != ExpeditionRoot) continue;
                    searched = true;
                    break;
                }
            }
            bool atShelter = new Vector2(p.x, p.z).sqrMagnitude < 100f && p.z >= -5f;
            ExpeditionPhase next = DecidePhase(reachedCheckpoint, searched, atShelter, completed);
            if (!completed && next == ExpeditionPhase.Complete)
            {
                completed = true;
                interaction?.Notify("İlk keşif tamamlandı. Topladıklarınla yeni malzemeler üretebilirsin.");
            }
            Phase = next;
        }

        /// <summary>Pure mission rule used by EditMode regression tests.</summary>
        public static ExpeditionPhase DecidePhase(bool reached, bool searched, bool atShelter, bool completed)
        {
            if (completed || (searched && atShelter)) return ExpeditionPhase.Complete;
            if (searched) return ExpeditionPhase.ReturnToShelter;
            return reached ? ExpeditionPhase.SearchCheckpoint : ExpeditionPhase.FindExit;
        }
    }
}
