using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Afterrust
{
    /// <summary>
    /// Single-slot save (JSON in persistentDataPath). F5 save, F9 load, autosave every few
    /// minutes and on quit. On start the game continues from the save if one exists.
    /// World objects are identified by their hierarchy path (name#siblingIndex/...).
    /// </summary>
    public sealed class SaveSystem : MonoBehaviour
    {
        public const int FormatVersion = 2;

        [Serializable]
        public sealed class ContainerState { public string id; public bool rolled; public List<string> contents = new List<string>(); }

        [Serializable]
        public sealed class WaterState { public string id; public int units; }

        [Serializable]
        public sealed class DoorState { public string id; public bool open; }

        [Serializable]
        public sealed class StoveState { public string id; public float burnSeconds; }

        [Serializable]
        public sealed class SaveData
        {
            public int version = FormatVersion;
            public string savedAt;
            public Vector3 position;
            public float yaw, pitch;
            public float health, hunger, thirst, stamina, warmth, energy;
            public List<AfterrustInventory.ItemStack> items = new List<AfterrustInventory.ItemStack>();
            public float capacityKg;
            public int credits;
            public bool wrench, screwdriver, wireCutter, largeBackpack;
            public List<ContainerState> containers = new List<ContainerState>();
            public List<string> salvaged = new List<string>();
            public List<WaterState> water = new List<WaterState>();
            public List<DoorState> doors = new List<DoorState>();
            public List<StoveState> stoves = new List<StoveState>();
            public float timeOfDay;
            public int day;
            public bool raining;
            public float weatherTimer;
        }

        public bool continueOnStart = true;
        public float autosaveSeconds = 180f;

        private static bool loadAfterReload;
        private static bool newGameAfterReload;

        private AfterrustInventory inventory;
        private SurvivorVitals vitals;
        private AfterrustFirstPersonController controller;
        private AfterrustInteraction interaction;
        private float nextAutosave;

        public static string FilePath => Path.Combine(Application.persistentDataPath, "afterrust_save.json");
        public static bool HasSave => File.Exists(FilePath);

        private void Awake()
        {
            inventory = GetComponent<AfterrustInventory>();
            vitals = GetComponent<SurvivorVitals>();
            controller = GetComponent<AfterrustFirstPersonController>();
            interaction = GetComponent<AfterrustInteraction>();
        }

        private void Start()
        {
            nextAutosave = Time.time + autosaveSeconds;
            if (newGameAfterReload)
            {
                newGameAfterReload = false;
                interaction?.Notify("Yeni oyun. Hayatta kal.");
                return;
            }
            if ((loadAfterReload || continueOnStart) && HasSave)
            {
                loadAfterReload = false;
                if (Load()) interaction?.Notify("Kayıttan devam ediliyor.");
            }
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            bool dead = vitals != null && vitals.IsDead;
            if (kb != null)
            {
                if (kb.f5Key.wasPressedThisFrame && !dead) interaction?.Notify(Save() ? "Oyun kaydedildi." : "Kayıt başarısız.");
                if (kb.f9Key.wasPressedThisFrame) ReloadFromSave();
            }
            if (!dead && Time.time >= nextAutosave)
            {
                nextAutosave = Time.time + autosaveSeconds;
                if (Save()) interaction?.Notify("Otomatik kayıt.");
            }
        }

        private void OnApplicationQuit()
        {
            if (vitals == null || !vitals.IsDead) Save();
        }

        // ------------------------------------------------------------------ public API

        /// <summary>Reloads the scene and continues from the save (or starts fresh if none).</summary>
        public static void ReloadFromSave()
        {
            loadAfterReload = HasSave;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Deletes the save and restarts the scene.</summary>
        public static void NewGame()
        {
            try { if (HasSave) File.Delete(FilePath); }
            catch (Exception e) { Debug.LogWarning("[Afterrust] Could not delete save: " + e.Message); }
            newGameAfterReload = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public bool Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(Capture(), true), Encoding.UTF8);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[Afterrust] Save failed: " + e);
                return false;
            }
        }

        public bool Load()
        {
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath, Encoding.UTF8));
                if (data == null || data.version != FormatVersion) return false;
                Restore(data);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[Afterrust] Load failed: " + e);
                return false;
            }
        }

        // ------------------------------------------------------------------ capture / restore

        public SaveData Capture()
        {
            SaveData d = new SaveData { savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm") };
            d.position = transform.position;
            d.yaw = transform.eulerAngles.y;
            d.pitch = controller != null ? controller.Pitch : 0f;
            if (vitals != null)
            {
                d.health = vitals.health; d.hunger = vitals.hunger; d.thirst = vitals.thirst; d.stamina = vitals.stamina;
                d.warmth = vitals.warmth; d.energy = vitals.energy;
            }
            if (inventory != null)
            {
                foreach (AfterrustInventory.ItemStack s in inventory.items)
                    d.items.Add(new AfterrustInventory.ItemStack
                        { id = s.id, displayName = s.displayName, massKg = s.massKg, saleValue = s.saleValue, condition = s.condition });
                d.capacityKg = inventory.capacityKg;
                d.credits = inventory.credits;
                d.wrench = inventory.hasWrench;
                d.screwdriver = inventory.hasScrewdriver;
                d.wireCutter = inventory.hasWireCutter;
                d.largeBackpack = inventory.hasLargeBackpack;
            }
            foreach (LootContainer c in FindObjectsByType<LootContainer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                d.containers.Add(new ContainerState { id = PathOf(c.transform), rolled = c.Rolled, contents = new List<string>(c.Contents) });
            foreach (SalvageablePart p in FindObjectsByType<SalvageablePart>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (p.wasSalvaged) d.salvaged.Add(PathOf(p.transform));
            foreach (WaterSource w in FindObjectsByType<WaterSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                d.water.Add(new WaterState { id = PathOf(w.transform), units = w.Units });
            foreach (Stove st in FindObjectsByType<Stove>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                d.stoves.Add(new StoveState { id = PathOf(st.transform), burnSeconds = st.burnSecondsLeft });
            foreach (Door door in FindObjectsByType<Door>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                d.doors.Add(new DoorState { id = PathOf(door.transform), open = door.isOpen });
            DayNightWeather world = DayNightWeather.Instance;
            if (world != null)
            {
                d.timeOfDay = world.timeOfDay; d.day = world.day; d.raining = world.raining; d.weatherTimer = world.weatherTimer;
            }
            return d;
        }

        private void Restore(SaveData d)
        {
            if (controller != null)
            {
                controller.Teleport(d.position, d.yaw);
                controller.Pitch = d.pitch;
            }
            else transform.SetPositionAndRotation(d.position, Quaternion.Euler(0f, d.yaw, 0f));

            if (vitals != null)
            {
                vitals.health = d.health; vitals.hunger = d.hunger; vitals.thirst = d.thirst; vitals.stamina = d.stamina;
                vitals.warmth = d.warmth; vitals.energy = d.energy;
            }
            if (inventory != null)
            {
                inventory.items = new List<AfterrustInventory.ItemStack>(d.items);
                inventory.capacityKg = d.capacityKg > 0f ? d.capacityKg : inventory.capacityKg;
                inventory.credits = d.credits;
                inventory.hasWrench = d.wrench;
                inventory.hasScrewdriver = d.screwdriver;
                inventory.hasWireCutter = d.wireCutter;
                inventory.hasLargeBackpack = d.largeBackpack;
            }

            Dictionary<string, ContainerState> containers = new Dictionary<string, ContainerState>();
            foreach (ContainerState c in d.containers) containers[c.id] = c;
            foreach (LootContainer c in FindObjectsByType<LootContainer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (containers.TryGetValue(PathOf(c.transform), out ContainerState s)) c.Restore(s.rolled, s.contents);

            HashSet<string> salvaged = new HashSet<string>(d.salvaged);
            foreach (SalvageablePart p in FindObjectsByType<SalvageablePart>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!salvaged.Contains(PathOf(p.transform))) continue;
                p.wasSalvaged = true;
                p.gameObject.SetActive(false);
            }

            Dictionary<string, int> water = new Dictionary<string, int>();
            foreach (WaterState w in d.water) water[w.id] = w.units;
            foreach (WaterSource w in FindObjectsByType<WaterSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (water.TryGetValue(PathOf(w.transform), out int units)) w.Restore(units);

            Dictionary<string, float> stoves = new Dictionary<string, float>();
            if (d.stoves != null) foreach (StoveState ss in d.stoves) stoves[ss.id] = ss.burnSeconds;
            foreach (Stove st in FindObjectsByType<Stove>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (stoves.TryGetValue(PathOf(st.transform), out float burn)) st.Restore(burn);

            Dictionary<string, bool> doors = new Dictionary<string, bool>();
            if (d.doors != null) foreach (DoorState ds in d.doors) doors[ds.id] = ds.open;
            foreach (Door door in FindObjectsByType<Door>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (doors.TryGetValue(PathOf(door.transform), out bool open)) door.SetOpen(open, true);

            if (DayNightWeather.Instance != null)
                DayNightWeather.Instance.Restore(d.timeOfDay, d.day, d.raining, d.weatherTimer);
        }

        public static string PathOf(Transform t)
        {
            StringBuilder sb = new StringBuilder();
            for (Transform x = t; x != null; x = x.parent)
            {
                // Root objects by name only (scene updates add roots); children also by index
                // because several props share a name.
                string part = x.parent == null ? x.name : x.name + "#" + x.GetSiblingIndex();
                sb.Insert(0, sb.Length > 0 ? part + "/" : part);
            }
            return sb.ToString();
        }
    }
}
