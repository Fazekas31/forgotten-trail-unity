using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailGame : MonoBehaviour
    {
        public static TrailGame Instance { get; private set; }
        public TrailCampaign Campaign { get; private set; }
        public InventoryModel Inventory { get; private set; }
        public JournalModel Journal { get; private set; }
        public LanternModel Lantern { get; private set; }
        public TrailLocalization Localization { get; } = new();
        public TrailSaveStore Save { get; private set; }
        public TrailPlayerController Player { get; private set; }
        public TrailAudioDirector Audio { get; private set; }
        public TrailUI UI { get; private set; }
        public bool IsRunning { get; private set; }
        private TrailWorldBuilder world;
        private Light lanternLight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() { if (FindFirstObjectByType<TrailGame>() == null) new GameObject("ForgottenTrail").AddComponent<TrailGame>(); }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject);
            ResetModels(); UI = gameObject.AddComponent<TrailUI>(); Audio = gameObject.AddComponent<TrailAudioDirector>(); world = gameObject.AddComponent<TrailWorldBuilder>();
            var playerObject = new GameObject("Levon"); playerObject.transform.SetParent(transform); Player = playerObject.AddComponent<TrailPlayerController>(); Player.Initialize();
            lanternLight = Player.PlayerCamera.gameObject.AddComponent<Light>(); lanternLight.type = LightType.Spot; lanternLight.range = 16f; lanternLight.spotAngle = 54f; lanternLight.intensity = 2.3f; lanternLight.color = new Color(1f,.59f,.25f); lanternLight.enabled = false;
        }

        private void ResetModels() { Campaign = new TrailCampaign(); Inventory = new InventoryModel(); Journal = new JournalModel(); Lantern = new LanternModel(); Save = new TrailSaveStore(); Campaign.StepChanged += OnStepChanged; Campaign.CheckpointReached += OnCheckpoint; Campaign.Completed += OnCompleted; }
        private void Update()
        {
            if (!IsRunning) return;
            if (UI.InspectionVisible && Input.GetKeyDown(KeyCode.E)) { UI.CloseInspection(); return; }
            if (UI.ChoiceOpen) { if (Input.GetKeyDown(KeyCode.Alpha1)) ChooseEnding(TrailEnding.SharedTrail); if (Input.GetKeyDown(KeyCode.Alpha2)) ChooseEnding(TrailEnding.DefinitiveSilence); return; }
            if (Input.GetKeyDown(KeyCode.Escape)) UI.TogglePause();
            if (Input.GetKeyDown(KeyCode.I)) UI.ToggleInventory();
            if (Input.GetKeyDown(KeyCode.J)) UI.ToggleJournal();
        }

        public void StartNewGame() { Save.Clear(); ResetModels(); BeginGame(); }
        public void ContinueGame() { ResetModels(); var snapshot = Save.Load(); if (snapshot != null) Campaign.Restore(snapshot, Inventory, Journal, Lantern); BeginGame(); }
        private void BeginGame() { IsRunning = true; UI.StartGame(); BuildCurrentAct(); Journal.RecordForStep(Campaign.CurrentStep); SaveSnapshot(); }
        public void ReturnToMenu() { IsRunning = false; UI.ShowMenu(); Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        public void Interact(TrailInteractable target)
        {
            if (!IsRunning || target == null) return;
            if (target.kind == InteractionKind.Choice) { UI.ShowChoice(); return; }
            var eventId = target.eventId; var itemId = target.itemId; var targetTitle = target.title; var targetText = target.inspectionText;
            if (!Campaign.Report(eventId)) { UI.Toast(Localization.Text("O rastro ainda não leva até aqui.", "The trail does not lead here yet.")); return; }
            if (!string.IsNullOrEmpty(itemId)) { Inventory.Add(itemId); if (itemId == "lantern") Lantern.Acquire(); }
            Journal.RecordForStep(Campaign.CurrentStep); UI.ShowInspection(Localization.Text(targetTitle, targetTitle), Localization.Text(targetText, targetText)); Audio.PlayCue(itemId != null ? "evidence" : "creak"); SaveSnapshot();
        }
        public void ToggleLantern() { if (!Lantern.Available) { UI.Toast(Localization.Text("Ainda não tenho um lampião.", "I do not have a lantern yet.")); return; } Lantern.Toggle(); lanternLight.enabled = Lantern.Lit; Audio.PlayCue("creak"); }
        private void ChooseEnding(TrailEnding ending) { if (!Campaign.ChooseEnding(ending)) return; Journal.AddEnding(ending); UI.CloseChoice(); UI.DrawEnding(ending); SaveSnapshot(); }
        private void OnStepChanged(string step) { if (IsRunning) { Journal.RecordForStep(step); BuildCurrentAct(); } }
        private void OnCheckpoint(string checkpoint) { if (IsRunning) SaveSnapshot(); }
        private void OnCompleted(TrailEnding ending) { IsRunning = true; }
        private void BuildCurrentAct() { world.Build(Campaign.CurrentAct, Campaign.CurrentStep); Player.Teleport(world.SpawnPoint); lanternLight.enabled = Lantern.Lit; }
        private void SaveSnapshot() { if (Save != null) Save.Save(Campaign.Snapshot(Inventory, Journal, Lantern)); }
    }
}
