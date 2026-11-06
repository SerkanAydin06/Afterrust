using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Afterrust
{
    /// <summary>
    /// Survival HUD (IMGUI prototype): vital bars, crosshair, prompts with hold progress,
    /// feedback, inventory (Tab) and crafting (workbench) panels, death screen (R restarts).
    /// </summary>
    public sealed class SurvivalHud : MonoBehaviour
    {
        public AfterrustInteraction interaction;
        public AfterrustInventory inventory;
        public SurvivorVitals vitals;
        public AfterrustFirstPersonController controller;

        private enum Panel { None, Inventory, Crafting }
        private Panel panel = Panel.None;
        private Vector2 scroll;
        private int openedFrame = -1;
        private CraftStation station = CraftStation.Workbench;
        private Stove stove;
        private SleepSystem sleep;

        private Texture2D white;
        private GUIStyle label, labelSmall, labelCenter, title, button, box, prompt, rightSmall;
        private static readonly Color Ink = new Color(0.92f, 0.9f, 0.84f);
        private static readonly Color Dim = new Color(0.62f, 0.6f, 0.55f);
        private static readonly Color Panel1 = new Color(0.07f, 0.065f, 0.06f, 0.88f);
        private static readonly Color Accent = new Color(0.86f, 0.55f, 0.22f);

        public bool IsPanelOpen => panel != Panel.None;
        /// <summary>Frame a panel last opened/closed; input that frame belongs to the panel.</summary>
        public int PanelChangedFrame => openedFrame;

        private void Awake()
        {
            if (interaction == null) interaction = GetComponent<AfterrustInteraction>();
            if (inventory == null) inventory = GetComponent<AfterrustInventory>();
            if (vitals == null) vitals = GetComponent<SurvivorVitals>();
            if (controller == null) controller = GetComponent<AfterrustFirstPersonController>();
            sleep = GetComponent<SleepSystem>();
        }

        public void ToggleInventory() { SetPanel(panel == Panel.Inventory ? Panel.None : Panel.Inventory); }
        public void OpenCrafting() { OpenStation(CraftStation.Workbench, null); }

        public void OpenStation(CraftStation station, Stove stove)
        {
            this.station = station;
            this.stove = stove;
            SetPanel(Panel.Crafting);
        }

        private void SetPanel(Panel p)
        {
            panel = p;
            scroll = Vector2.zero;
            openedFrame = Time.frameCount;
            if (controller != null) controller.SetUiMode(p != Panel.None);
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (vitals != null && vitals.IsDead)
            {
                if (panel != Panel.None) SetPanel(Panel.None);
                if (controller != null) controller.SetUiMode(true);
                if (kb.rKey.wasPressedThisFrame) SaveSystem.ReloadFromSave();
                else if (kb.nKey.wasPressedThisFrame) SaveSystem.NewGame();
                return;
            }
            if (sleep != null && sleep.Sleeping) return;
            if (Time.frameCount == openedFrame) return;   // opened this frame by the same key press
            if (kb.tabKey.wasPressedThisFrame)
            {
                ToggleInventory();
                return;
            }
            if (panel != Panel.None && (kb.escapeKey.wasPressedThisFrame ||
                (panel == Panel.Crafting && kb.eKey.wasPressedThisFrame)))
                SetPanel(Panel.None);
        }

        // ------------------------------------------------------------------ drawing

        private void Styles()
        {
            if (white != null) return;
            white = Texture2D.whiteTexture;
            label = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true };
            label.normal.textColor = Ink;
            labelSmall = new GUIStyle(label) { fontSize = 13 };
            labelSmall.normal.textColor = Dim;
            labelCenter = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter };
            rightSmall = new GUIStyle(labelSmall) { alignment = TextAnchor.MiddleRight };
            prompt = new GUIStyle(labelCenter) { fontSize = 18, fontStyle = FontStyle.Bold };
            title = new GUIStyle(label) { fontSize = 22, fontStyle = FontStyle.Bold };
            title.normal.textColor = Accent;
            button = new GUIStyle(GUI.skin.button) { fontSize = 14 };
            box = new GUIStyle(GUI.skin.box);
        }

        private void Fill(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, white);
            GUI.color = old;
        }

        private void Bar(float x, float y, string name, float value, Color color)
        {
            const float w = 210f, h = 12f;
            GUI.Label(new Rect(x, y - 2f, 90f, 18f), name, labelSmall);
            Fill(new Rect(x + 90f, y, w, h), new Color(0f, 0f, 0f, 0.55f));
            bool critical = value < 20f && Mathf.Repeat(Time.time, 1f) < 0.5f;
            Fill(new Rect(x + 91f, y + 1f, (w - 2f) * Mathf.Clamp01(value / 100f), h - 2f),
                critical ? Color.Lerp(color, Color.white, 0.4f) : color);
        }

        private void OnGUI()
        {
            Styles();
            float sw = Screen.width, sh = Screen.height;

            if (vitals != null && vitals.IsDead)
            {
                Fill(new Rect(0, 0, sw, sh), new Color(0.05f, 0f, 0f, 0.82f));
                GUI.Label(new Rect(0, sh * 0.4f, sw, 40f), "HAYATTA KALAMADIN", new GUIStyle(title) { alignment = TextAnchor.MiddleCenter, fontSize = 34 });
                GUI.Label(new Rect(0, sh * 0.4f + 50f, sw, 30f),
                    SaveSystem.HasSave ? "R - Son kayıttan devam    N - Yeni oyun" : "R / N - Yeni oyun", labelCenter);
                return;
            }

            // Vitals (bottom-left)
            if (vitals != null)
            {
                float y = sh - 148f;
                Fill(new Rect(14f, y - 10f, 326f, 140f), new Color(0f, 0f, 0f, 0.35f));
                Bar(24f, y, "Sağlık", vitals.health, new Color(0.78f, 0.2f, 0.18f));
                Bar(24f, y + 20f, "Açlık", vitals.hunger, new Color(0.85f, 0.55f, 0.18f));
                Bar(24f, y + 40f, "Susuzluk", vitals.thirst, new Color(0.25f, 0.55f, 0.85f));
                Bar(24f, y + 60f, "Isı", vitals.warmth, new Color(0.9f, 0.42f, 0.25f));
                Bar(24f, y + 80f, "Enerji", vitals.energy, new Color(0.6f, 0.5f, 0.8f));
                Bar(24f, y + 100f, "Dayanıklılık", vitals.stamina, new Color(0.45f, 0.72f, 0.35f));
                string status = vitals.IsCold ? "Üşüyorsun" : vitals.Tired ? "Çok yorgunsun" :
                    vitals.warmthTarget > vitals.warmth + 5f ? "Isınıyorsun" : "";
                if (vitals.sheltered) status += (status.Length > 0 ? "  •  " : "") + "Çatı altında";
                if (status.Length > 0) GUI.Label(new Rect(24f, y - 32f, 320f, 20f), status, labelSmall);
            }

            // Weight / credits (top-left)
            if (inventory != null)
            {
                Fill(new Rect(14f, 14f, 250f, 30f), new Color(0f, 0f, 0f, 0.35f));
                GUI.Label(new Rect(24f, 18f, 240f, 24f),
                    "Yük " + inventory.CurrentMassKg.ToString("0.0") + " / " + inventory.capacityKg.ToString("0") +
                    " kg    Kredi " + inventory.credits, labelSmall);
            }

            // Clock and weather (top-right)
            DayNightWeather world = DayNightWeather.Instance;
            if (world != null)
            {
                Fill(new Rect(sw - 214f, 14f, 200f, 30f), new Color(0f, 0f, 0f, 0.35f));
                GUI.Label(new Rect(sw - 204f, 18f, 190f, 24f), world.ClockText + "   " + world.WeatherText, labelSmall);
            }

            // Quick keys (bottom-right)
            GUI.Label(new Rect(sw - 520f, sh - 34f, 510f, 24f),
                "1 Ye   2 İç   3 Bandaj   Tab Envanter   L Fener   F5 Kaydet   F9 Yükle",
                rightSmall);

            if (panel == Panel.None)
            {
                // Crosshair
                Fill(new Rect(sw / 2f - 2f, sh / 2f - 2f, 4f, 4f), new Color(1f, 1f, 1f, 0.85f));
                if (interaction != null && !string.IsNullOrEmpty(interaction.Prompt))
                {
                    GUI.Label(new Rect(0, sh / 2f + 24f, sw, 26f), interaction.Prompt, prompt);
                    if (interaction.HoldProgress > 0f)
                    {
                        Fill(new Rect(sw / 2f - 80f, sh / 2f + 54f, 160f, 6f), new Color(0f, 0f, 0f, 0.6f));
                        Fill(new Rect(sw / 2f - 80f, sh / 2f + 54f, 160f * interaction.HoldProgress, 6f), Accent);
                    }
                }
            }

            // Feedback (fades after 5 s)
            if (interaction != null && !string.IsNullOrEmpty(interaction.Feedback))
            {
                float age = Time.time - interaction.FeedbackTime;
                float a = interaction.FeedbackTime <= 0f ? 1f : Mathf.Clamp01(1f - (age - 4f));
                if (a > 0f)
                {
                    Color old = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, a);
                    GUI.Label(new Rect(0, sh - 150f, sw, 26f), interaction.Feedback, labelCenter);
                    GUI.color = old;
                }
            }

            if (sleep != null && sleep.Fade > 0f)
            {
                Fill(new Rect(0, 0, sw, sh), new Color(0f, 0f, 0f, sleep.Fade));
                if (sleep.Fade > 0.6f) GUI.Label(new Rect(0, sh * 0.45f, sw, 30f), "Uyuyorsun...", labelCenter);
                return;
            }

            if (panel == Panel.Inventory) DrawInventory(sw, sh);
            else if (panel == Panel.Crafting) DrawCrafting(sw, sh);
        }

        private Rect PanelRect(float sw, float sh, string heading, string hint)
        {
            Rect r = new Rect(sw / 2f - 300f, sh / 2f - 230f, 600f, 460f);
            Fill(r, Panel1);
            Fill(new Rect(r.x, r.y, r.width, 3f), Accent);
            GUI.Label(new Rect(r.x + 20f, r.y + 14f, 400f, 30f), heading, title);
            GUI.Label(new Rect(r.x + 20f, r.yMax - 30f, r.width - 40f, 22f), hint, labelSmall);
            return r;
        }

        private void DrawInventory(float sw, float sh)
        {
            Rect r = PanelRect(sw, sh, "Envanter", "Tab / Esc kapat");
            if (inventory == null) return;
            string tools = "Aletler: anahtar" + (inventory.hasScrewdriver ? ", tornavida" : "") +
                           (inventory.hasWireCutter ? ", kablo kesici" : "") +
                           (inventory.hasLargeBackpack ? "   |   Büyük çanta" : "");
            GUI.Label(new Rect(r.x + 20f, r.y + 48f, r.width - 40f, 22f), tools, labelSmall);

            List<AfterrustInventory.Group> groups = inventory.Grouped();
            Rect view = new Rect(r.x + 20f, r.y + 76f, r.width - 40f, r.height - 116f);
            Rect content = new Rect(0, 0, view.width - 20f, Mathf.Max(view.height, groups.Count * 34f));
            scroll = GUI.BeginScrollView(view, scroll, content);
            if (groups.Count == 0) GUI.Label(new Rect(0, 0, content.width, 24f), "Çantan boş. Kapları ara (E basılı tut).", labelSmall);
            for (int i = 0; i < groups.Count; i++)
            {
                AfterrustInventory.Group g = groups[i];
                float y = i * 34f;
                if (i % 2 == 0) Fill(new Rect(0, y, content.width, 32f), new Color(1f, 1f, 1f, 0.04f));
                GUI.Label(new Rect(8f, y + 6f, 300f, 22f), g.displayName + (g.count > 1 ? "  x" + g.count : ""), label);
                GUI.Label(new Rect(310f, y + 8f, 90f, 22f), g.massKg.ToString("0.0") + " kg", labelSmall);
                ItemDef def = ItemDatabase.Get(g.id);
                if (def != null && def.IsConsumable &&
                    GUI.Button(new Rect(content.width - 100f, y + 4f, 90f, 24f), def.category == ItemCategory.Drink ? "İç" :
                        def.category == ItemCategory.Food ? "Ye" : "Kullan", button))
                    interaction?.Notify(inventory.Consume(g.id, vitals));
            }
            GUI.EndScrollView();
        }

        private void DrawCrafting(float sw, float sh)
        {
            bool atStove = station == CraftStation.Stove;
            Rect r = PanelRect(sw, sh, atStove ? "Soba" : "Tezgâh", "E / Esc kapat");
            if (inventory == null) return;
            float top = r.y + 56f;

            bool ready = !atStove || (stove != null && stove.IsLit);
            if (atStove && stove != null)
            {
                GUI.Label(new Rect(r.x + 20f, top, 300f, 24f),
                    stove.IsLit ? "Ateş yanıyor: " + stove.TimeLeftText + " kaldı" : "Soba sönük. Yakıt ekle:", label);
                GUI.enabled = inventory.CountOf("wood") > 0;
                if (GUI.Button(new Rect(r.xMax - 260f, top, 115f, 26f), "Odun (" + inventory.CountOf("wood") + ")", button))
                    interaction?.Notify(stove.AddFuel(inventory, "wood"));
                GUI.enabled = inventory.CountOf("charcoal") > 0;
                if (GUI.Button(new Rect(r.xMax - 135f, top, 115f, 26f), "Kömür (" + inventory.CountOf("charcoal") + ")", button))
                    interaction?.Notify(stove.AddFuel(inventory, "charcoal"));
                GUI.enabled = true;
                top += 36f;
            }

            List<Recipe> recipes = new List<Recipe>(Crafting.For(station));
            Rect view = new Rect(r.x + 20f, top, r.width - 40f, r.yMax - 40f - top);
            Rect content = new Rect(0, 0, view.width - 20f, Mathf.Max(view.height, recipes.Count * 66f));
            scroll = GUI.BeginScrollView(view, scroll, content);
            for (int i = 0; i < recipes.Count; i++)
            {
                Recipe rec = recipes[i];
                float y = i * 66f;
                bool available = Crafting.IsAvailable(rec, inventory);
                bool can = ready && available && Crafting.CanCraft(rec, inventory);
                Fill(new Rect(0, y, content.width, 62f), new Color(1f, 1f, 1f, can ? 0.07f : 0.03f));
                GUI.Label(new Rect(10f, y + 4f, 420f, 22f), "<b>" + rec.name + "</b>  <color=#9a958a>" + rec.description + "</color>", label);
                GUI.Label(new Rect(10f, y + 32f, 400f, 22f),
                    !available ? "Zaten sahipsin." : !ready ? "Önce ateş yak.  " + Crafting.Describe(rec, inventory)
                        : Crafting.Describe(rec, inventory), labelSmall);
                GUI.enabled = can;
                if (GUI.Button(new Rect(content.width - 100f, y + 17f, 90f, 28f), atStove ? "Pişir" : "Üret", button))
                    interaction?.Notify(Crafting.Craft(rec, inventory));
                GUI.enabled = true;
            }
            GUI.EndScrollView();
        }
    }
}
