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
        public bool LayoutPreviewActive => layoutPreviewActive;
        private TrailWorldBuilder world;
        private Light lanternLight;
        private Light layoutPreviewLight;
        private Camera layoutPreviewCamera;
        private bool layoutPreviewActive;
        private bool layoutPreviewFogWasEnabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() { if (FindFirstObjectByType<TrailGame>() == null) new GameObject("ForgottenTrail").AddComponent<TrailGame>(); }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject);
            ResetModels(); UI = gameObject.AddComponent<TrailUI>(); Audio = gameObject.AddComponent<TrailAudioDirector>(); world = gameObject.AddComponent<TrailWorldBuilder>();
            gameObject.AddComponent<TrailVhsOverlay>();
            var playerObject = new GameObject("Levon"); playerObject.transform.SetParent(transform); playerObject.AddComponent<CharacterController>(); Player = playerObject.AddComponent<TrailPlayerController>(); Player.Initialize();
            lanternLight = Player.PlayerCamera.gameObject.AddComponent<Light>(); lanternLight.type = LightType.Spot; lanternLight.range = 16f; lanternLight.spotAngle = 54f; lanternLight.intensity = 2.3f; lanternLight.color = new Color(1f,.59f,.25f); lanternLight.enabled = false;
        }

        private void Start()
        {
            // Play Mode is the fastest way to test the demo in the Unity Editor.
            // Keep the menu available after ReturnToMenu, but boot the first session automatically.
            if (Application.isPlaying && !IsRunning && UI != null) StartNewGame();
        }

        private void ResetModels() { Campaign = new TrailCampaign(); Inventory = new InventoryModel(); Journal = new JournalModel(); Lantern = new LanternModel(); Save = new TrailSaveStore(); Campaign.StepChanged += OnStepChanged; Campaign.CheckpointReached += OnCheckpoint; Campaign.Completed += OnCompleted; }
        private void Update()
        {
            if (!IsRunning) return;
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F8)) { ToggleLayoutPreview(); return; }
#endif
            if (layoutPreviewActive) return;
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
            var completedStep = Campaign.CurrentStep;
            if (target.kind == InteractionKind.Choice) { UI.ShowChoice(); return; }
            var eventId = target.eventId; var itemId = target.itemId; var targetTitle = target.title; var targetText = target.inspectionText;
            if (!Campaign.Report(eventId)) { UI.Toast(Localization.Text("O rastro ainda não leva até aqui.", "The trail does not lead here yet.")); return; }
            if (!string.IsNullOrEmpty(itemId)) { Inventory.Add(itemId); if (itemId == "lantern") Lantern.Acquire(); if (itemId == "knife") Player.EquipWeapon("knife"); }
            Journal.RecordForStep(Campaign.CurrentStep);
            UI.ShowInspection(Localization.Text(targetTitle, targetTitle), Localization.Text(targetText, targetText));
            Audio.PlayCue(itemId != null ? "evidence" : "creak");
            PlayNarrativeBeat(completedStep);
            SaveSnapshot();
        }
        public void ToggleLantern() { if (!Lantern.Available) { UI.Toast(Localization.Text("Ainda não tenho um lampião.", "I do not have a lantern yet.")); return; } Lantern.Toggle(); lanternLight.enabled = Lantern.Lit; Audio.PlayCue("creak"); }
        private void ChooseEnding(TrailEnding ending) { if (!Campaign.ChooseEnding(ending)) return; Journal.AddEnding(ending); UI.CloseChoice(); UI.DrawEnding(ending); SaveSnapshot(); }
        private void OnStepChanged(string step) { if (IsRunning) { Journal.RecordForStep(step); BuildCurrentAct(); } }
        private void OnCheckpoint(string checkpoint) { if (IsRunning) SaveSnapshot(); }
        private void OnCompleted(TrailEnding ending) { IsRunning = true; }
        private void BuildCurrentAct()
        {
            if (layoutPreviewActive) SetLayoutPreview(false);
            world.Build(Campaign.CurrentAct, Campaign.CurrentStep);
            Player.Teleport(world.SpawnPoint);
            lanternLight.enabled = Lantern.Lit;
        }

        private void ToggleLayoutPreview()
        {
            if (Campaign == null || Campaign.CurrentAct != TrailAct.Arrival) return;
            if (layoutPreviewCamera == null)
            {
                var previewObject = new GameObject("AshCreek_LayoutPreviewCamera");
                layoutPreviewCamera = previewObject.AddComponent<Camera>();
                layoutPreviewCamera.name = "AshCreek_LayoutPreviewCamera";
                layoutPreviewCamera.orthographic = true;
                layoutPreviewCamera.orthographicSize = 58f;
                layoutPreviewCamera.nearClipPlane = .1f;
                layoutPreviewCamera.farClipPlane = 180f;
                layoutPreviewCamera.clearFlags = CameraClearFlags.SolidColor;
                layoutPreviewCamera.backgroundColor = new Color(.026f, .045f, .07f);
                layoutPreviewCamera.transform.position = new Vector3(0f, 80f, 16f);
                layoutPreviewCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                layoutPreviewCamera.enabled = false;

                var lightObject = new GameObject("AshCreek_LayoutPreviewLight");
                lightObject.transform.SetParent(previewObject.transform);
                lightObject.transform.localRotation = Quaternion.Euler(50f, -30f, 0f);
                layoutPreviewLight = lightObject.AddComponent<Light>();
                layoutPreviewLight.type = LightType.Directional;
                layoutPreviewLight.intensity = 1.35f;
                layoutPreviewLight.color = new Color(1f, .82f, .66f);
                layoutPreviewLight.shadows = LightShadows.None;
                layoutPreviewLight.enabled = false;
            }

            SetLayoutPreview(!layoutPreviewActive);
        }

        private void SetLayoutPreview(bool enabled)
        {
            if (enabled) layoutPreviewFogWasEnabled = RenderSettings.fog;
            RenderSettings.fog = enabled ? false : layoutPreviewFogWasEnabled;
            layoutPreviewActive = enabled;
            if (layoutPreviewCamera != null) layoutPreviewCamera.enabled = enabled;
            if (layoutPreviewLight != null) layoutPreviewLight.enabled = enabled;
            if (Player != null)
            {
                Player.enabled = !enabled;
                if (Player.PlayerCamera != null) Player.PlayerCamera.enabled = !enabled;
            }

            Cursor.lockState = enabled ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = enabled;
        }
        private void PlayNarrativeBeat(string completedStep)
        {
            switch (completedStep)
            {
                case "meal":
                    // The upper-floor moan is the first sound that makes the
                    // player commit to the staircase.
                    Audio.PlayDelayed("creak", .32f);
                    UI.Toast("Um gemido vem do andar de cima. Não parece humano.");
                    break;
                case "broken_door":
                    Audio.PlayCue("creak");
                    UI.Toast("A madeira range. A passagem para o andar de cima está aberta.");
                    break;
                case "diary":
                    Audio.PlayCue("heartbeat");
                    UI.Toast("Os batimentos aumentam. Alguma coisa observa a janela.");
                    world.PlayWindowWatcherBeat();
                    break;
                case "message":
                    Audio.PlayCue("heartbeat");
                    UI.Toast("O aviso confirma: eles escutam tudo.");
                    world.PlayWindowWatcherBeat();
                    break;
                case "window":
                    Audio.PlayCue("impact");
                    Audio.PlayDelayed("creak", .22f);
                    UI.Toast("Alguma coisa quebrou no andar de baixo.");
                    break;
                case "downstairs_noise":
                    Audio.PlayCue("impact");
                    Audio.PlayDelayed("creak", .18f);
                    UI.Toast("As pegadas recentes seguem para a porta.");
                    break;
                case "knife":
                    Audio.PlayCue("evidence");
                    UI.Toast("A faca está disponível como defesa emergencial.");
                    break;
                case "exit_saloon":
                    Audio.PlayDelayed("church_bell", .35f);
                    UI.Toast("O sino da igreja toca uma única vez.");
                    break;
                case "priest":
                    Audio.PlayCue("heartbeat");
                    break;
            }
        }
        private void SaveSnapshot() { if (Save != null) Save.Save(Campaign.Snapshot(Inventory, Journal, Lantern)); }
    }
}
