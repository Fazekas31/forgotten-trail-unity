using System.Collections.Generic;
using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailUI : MonoBehaviour
    {
        public bool MenuOpen { get; private set; } = true;
        public bool Paused { get; private set; }
        public bool InventoryOpen { get; private set; }
        public bool JournalOpen { get; private set; }
        public bool ChoiceOpen { get; private set; }
        public bool BlocksPlayer => MenuOpen || Paused || InventoryOpen || JournalOpen || ChoiceOpen || InspectionVisible;
        public bool InspectionVisible { get; private set; }
        private string inspectionTitle, inspectionText, toast;
        private float toastUntil;
        private GUIStyle title, body, small, panel;

        public void ShowMenu() { MenuOpen = true; Paused = false; CloseOverlays(); Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        public void StartGame() { MenuOpen = false; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        public void TogglePause() { if (MenuOpen) return; Paused = !Paused; Cursor.lockState = Paused ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = Paused; }
        public void ToggleInventory() { if (MenuOpen || InspectionVisible) return; InventoryOpen = !InventoryOpen; JournalOpen = false; Cursor.lockState = InventoryOpen ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = InventoryOpen; }
        public void ToggleJournal() { if (MenuOpen || InspectionVisible) return; JournalOpen = !JournalOpen; InventoryOpen = false; Cursor.lockState = JournalOpen ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = JournalOpen; }
        public void ShowInspection(string heading, string text) { inspectionTitle = heading; inspectionText = text; InspectionVisible = true; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        public void CloseInspection() { InspectionVisible = false; if (!BlocksPlayer) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; } }
        public void ShowChoice() { ChoiceOpen = true; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        public void CloseChoice() { ChoiceOpen = false; }
        public void Toast(string message) { toast = message; toastUntil = Time.unscaledTime + 2.5f; }
        private void CloseOverlays() { Paused = InventoryOpen = JournalOpen = ChoiceOpen = InspectionVisible = false; }

        private void EnsureStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            body = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true, richText = true };
            small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, richText = true };
            panel = new GUIStyle(GUI.skin.box) { padding = new RectOffset(18, 18, 14, 14) };
        }

        private void OnGUI()
        {
            EnsureStyles();
            GUI.color = new Color(0.95f, 0.82f, 0.62f, 1f);
            if (MenuOpen) { DrawMenu(); return; }
            var game = TrailGame.Instance; if (game == null) return;
            DrawHud(game);
            if (InspectionVisible) DrawInspection();
            else if (InventoryOpen) DrawInventory(game);
            else if (JournalOpen) DrawJournal(game);
            else if (Paused) DrawPause(game);
            else if (ChoiceOpen) DrawChoice();
            if (!string.IsNullOrEmpty(toast) && Time.unscaledTime < toastUntil) GUI.Label(new Rect(Screen.width / 2 - 260, 80, 520, 30), toast, body);
        }

        private void DrawMenu()
        {
            GUI.Box(new Rect(Screen.width * .5f - 190, Screen.height * .5f - 155, 380, 310), "", panel);
            GUI.Label(new Rect(Screen.width * .5f - 180, Screen.height * .5f - 125, 360, 60), "FORGOTTEN TRAIL", title);
            GUI.Label(new Rect(Screen.width * .5f - 180, Screen.height * .5f - 62, 360, 24), "ATO I  •  A CHEGADA E O SILÊNCIO", small);
            if (GUI.Button(new Rect(Screen.width * .5f - 120, Screen.height * .5f + 5, 240, 36), TrailGame.Instance?.Localization.LabelNewGame ?? "NOVO JOGO")) TrailGame.Instance.StartNewGame();
            if (GUI.Button(new Rect(Screen.width * .5f - 120, Screen.height * .5f + 48, 240, 36), TrailGame.Instance?.Localization.LabelContinue ?? "CONTINUAR")) TrailGame.Instance.ContinueGame();
            if (GUI.Button(new Rect(Screen.width * .5f - 120, Screen.height * .5f + 91, 240, 36), TrailGame.Instance?.Localization.LabelQuit ?? "SAIR")) Application.Quit();
        }

        private void DrawHud(TrailGame game)
        {
            var player = game.Player; GUI.color = new Color(.82f, .70f, .50f, 1);
            GUI.Box(new Rect(12, 12, 300, 52), "", panel); GUI.Label(new Rect(24, 18, 276, 42), game.Localization.LabelObjective + "\n" + game.Localization.Objective(game.Campaign.CurrentStep), small);
            GUI.Label(new Rect(12, Screen.height - 30, 640, 22), "[Shift] correr   [Ctrl] agachar   [F] lampião   [I] recursos   [J] diário   [Esc] pausa   [F8] visão aérea", small);
            if (game.LayoutPreviewActive)
                GUI.Label(new Rect(Screen.width / 2 - 220, Screen.height - 62, 440, 24), "VISÃO AÉREA DA PLANTA  •  pressione F8 para voltar", body);
            if (player != null && player.Focused != null) GUI.Label(new Rect(Screen.width / 2 - 180, Screen.height - 74, 360, 30), "[E] " + player.Focused.Prompt, body);
        }

        private void DrawInspection() { GUI.color = new Color(.92f, .84f, .68f, 1); GUI.Box(new Rect(55, Screen.height - 185, Screen.width - 110, 140), "", panel); GUI.Label(new Rect(76, Screen.height - 168, Screen.width - 150, 25), inspectionTitle, body); GUI.Label(new Rect(76, Screen.height - 133, Screen.width - 150, 75), inspectionText + "\n\n[E] fechar", small); }
        private void DrawInventory(TrailGame game)
        {
            GUI.Box(new Rect(Screen.width / 2 - 230, Screen.height / 2 - 190, 460, 380), "", panel); GUI.Label(new Rect(Screen.width / 2 - 205, Screen.height / 2 - 168, 410, 28), game.Localization.LabelInventory, body);
            var y = Screen.height / 2 - 125; foreach (var pair in game.Inventory.Quantities) { var item = TrailContent.Items[pair.Key]; GUI.Label(new Rect(Screen.width / 2 - 205, y, 410, 44), $"{game.Localization.ItemName(item)}  ×{pair.Value}\n{game.Localization.ItemDescription(item)}", small); y += 54; }
            if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height / 2 + 145, 160, 30), game.Localization.LabelClose + " [I]")) ToggleInventory();
        }
        private void DrawJournal(TrailGame game)
        {
            GUI.Box(new Rect(Screen.width / 2 - 270, 45, 540, Screen.height - 90), "", panel); GUI.Label(new Rect(Screen.width / 2 - 245, 65, 490, 28), game.Localization.LabelJournal, body); var y = 108;
            foreach (var entry in game.Journal.Entries(game.Localization.English)) { GUI.Label(new Rect(Screen.width / 2 - 245, y, 490, 68), $"<b>{entry.Title(game.Localization.English)}</b>\n{entry.Moment(game.Localization.English)}\n{entry.Text(game.Localization.English)}", small); y += 78; if (y > Screen.height - 90) break; }
            if (GUI.Button(new Rect(Screen.width / 2 - 80, Screen.height - 78, 160, 30), game.Localization.LabelClose + " [J]")) ToggleJournal();
        }
        private void DrawPause(TrailGame game) { GUI.Box(new Rect(Screen.width / 2 - 170, Screen.height / 2 - 100, 340, 200), "", panel); GUI.Label(new Rect(Screen.width / 2 - 140, Screen.height / 2 - 75, 280, 30), game.Localization.LabelPause, title); if (GUI.Button(new Rect(Screen.width / 2 - 110, Screen.height / 2 - 8, 220, 32), game.Localization.LabelContinue)) TogglePause(); if (GUI.Button(new Rect(Screen.width / 2 - 110, Screen.height / 2 + 34, 220, 32), "VOLTAR AO MENU")) game.ReturnToMenu(); }
        private void DrawChoice() { GUI.Box(new Rect(Screen.width / 2 - 270, Screen.height / 2 - 135, 540, 270), "", panel); GUI.Label(new Rect(Screen.width / 2 - 240, Screen.height / 2 - 110, 480, 55), "A VERDADE COBRA ALGO", title); GUI.Label(new Rect(Screen.width / 2 - 240, Screen.height / 2 - 42, 480, 70), "[1] Retirar Layla e deixar a câmara aberta\n[2] Manter a Campainha ativa e selar a passagem", body); }
        public void DrawEnding(TrailEnding ending)
        {
            CloseOverlays(); MenuOpen = false; InspectionVisible = true; inspectionTitle = ending == TrailEnding.SharedTrail ? "RASTRO COMPARTILHADO" : "SILÊNCIO DEFINITIVO"; inspectionText = ending == TrailEnding.SharedTrail ? "Layla sobrevive. Ash Creek permanece uma ferida aberta, mas vocês saem juntos." : "Layla fica para trás. O Imitador é contido e, pela primeira vez, a cidade fica em silêncio.";
        }
    }
}
