using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailPlayerController : MonoBehaviour
    {
        public float walkSpeed = 2.8f;
        public float runMultiplier = 1.75f;
        public float crouchMultiplier = 0.48f;
        public float acceleration = 11f;
        public float jumpVelocity = 4.6f;
        public float staminaSeconds = 6f;
        public float mouseSensitivity = 2.2f;
        public Camera PlayerCamera { get; private set; }
        public TrailInteractable Focused { get; private set; }
        public string CurrentWeaponId => weaponModel.CurrentWeaponId;
        public float StaminaRatio => staminaSeconds <= 0 ? 1f : stamina / staminaSeconds;
        public string MovementMode { get; private set; } = "walking";
        private CharacterController controller;
        private float stamina;
        private float recovery;
        private float pitch;
        private Vector3 velocity;
        private float footstepDistance;
        private bool initialized;
        private TrailWeaponModel weaponModel;
        private GameObject weaponViewModel;
        private float weaponCooldown;

        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            controller = gameObject.GetComponent<CharacterController>();
            if (controller == null) controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.72f; controller.radius = 0.32f; controller.center = new Vector3(0, 0.86f, 0);
            controller.slopeLimit = 50f; controller.stepOffset = .28f; controller.skinWidth = .04f;
            controller.minMoveDistance = .001f; controller.detectCollisions = true;
            var head = new GameObject("Head"); head.transform.SetParent(transform); head.transform.localPosition = new Vector3(0, 1.58f, 0);
            PlayerCamera = head.AddComponent<Camera>(); PlayerCamera.fieldOfView = 68f; PlayerCamera.nearClipPlane = 0.04f;
            PlayerCamera.farClipPlane = 110f; PlayerCamera.tag = "MainCamera"; head.AddComponent<AudioListener>();
            stamina = staminaSeconds; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
            weaponModel = new TrailWeaponModel();
        }

        private void Update()
        {
            if (!initialized || TrailGame.Instance == null || !TrailGame.Instance.IsRunning || TrailGame.Instance.UI.BlocksPlayer) { ClearFocus(); return; }
            Look(); Move(); FindFocus();
            if (Input.GetKeyDown(KeyCode.E) && Focused != null) Focused.Interact();
            if (Input.GetKeyDown(KeyCode.F)) TrailGame.Instance.ToggleLantern();
            if (Input.GetMouseButtonDown(0)) PerformWeaponAttack();
            if (weaponCooldown > 0) weaponCooldown -= Time.deltaTime;
        }

        private void Look()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * mouseSensitivity);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -82f, 82f);
            PlayerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        private void Move()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); input = Vector2.ClampMagnitude(input, 1f);
            var crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            var wantsRun = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var running = wantsRun && input.sqrMagnitude > 0.1f && !crouching && stamina > 0;
            MovementMode = crouching ? "crouching" : running ? "running" : "walking";
            var speed = walkSpeed * (crouching ? crouchMultiplier : running ? runMultiplier : 1f);
            if (running) { stamina = Mathf.Max(0, stamina - Time.deltaTime); recovery = 0.9f; }
            else { recovery = Mathf.Max(0, recovery - Time.deltaTime); if (recovery <= 0) stamina = Mathf.Min(staminaSeconds, stamina + 1.75f * Time.deltaTime); }
            var direction = (transform.right * input.x + transform.forward * input.y) * speed;
            velocity.x = Mathf.MoveTowards(velocity.x, direction.x, acceleration * Time.deltaTime);
            velocity.z = Mathf.MoveTowards(velocity.z, direction.z, acceleration * Time.deltaTime);
            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space)) velocity.y = jumpVelocity;
            velocity.y += Physics.gravity.y * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            if (input.sqrMagnitude > 0.1f && controller.isGrounded)
            {
                footstepDistance += direction.magnitude * Time.deltaTime;
                var threshold = running ? 0.85f : crouching ? 1.25f : 1.05f;
                if (footstepDistance >= threshold) { footstepDistance = 0; NoiseSystem.Emit(transform.position, running ? 9f : crouching ? 2.5f : 5f, MovementMode); TrailGame.Instance.Audio.Footstep("dirt", MovementMode); }
            }
        }

        private void FindFocus()
        {
            TrailInteractable candidate = null;
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out var hit, 3.2f)) candidate = hit.collider.GetComponentInParent<TrailInteractable>();
            if (candidate == null)
            {
                var nearest = float.MaxValue;
                foreach (var item in Object.FindObjectsByType<TrailInteractable>(FindObjectsSortMode.None))
                {
                    var distance = Vector3.Distance(transform.position, item.transform.position);
                    if (distance < nearest && distance < 2.3f && !string.IsNullOrEmpty(item.Prompt)) { nearest = distance; candidate = item; }
                }
            }
            Focused = candidate;
        }

        private void ClearFocus() => Focused = null;
        public bool EquipWeapon(string weaponId)
        {
            if (weaponModel == null || !weaponModel.Equip(weaponId)) return false;
            if (weaponViewModel != null) Destroy(weaponViewModel);
            var resource = weaponId == "knife" ? "Props/PSX_KitchenKnife" : "Props/PSX_Revolver";
            var prefab = Resources.Load<GameObject>(resource);
            if (prefab == null) return true;
            weaponViewModel = Instantiate(prefab, PlayerCamera.transform);
            weaponViewModel.name = "Held_" + weaponId;
            weaponViewModel.transform.localPosition = weaponId == "knife" ? new Vector3(.27f, -.3f, -.58f) : new Vector3(.34f, -.36f, -.74f);
            weaponViewModel.transform.localRotation = Quaternion.Euler(weaponId == "knife" ? new Vector3(-8f, -18f, -12f) : new Vector3(-7f, 180f, -3f));
            weaponViewModel.transform.localScale = Vector3.one * (weaponId == "knife" ? .72f : .34f);
            return true;
        }
        private void PerformWeaponAttack()
        {
            if (weaponModel == null || weaponCooldown > 0 || TrailGame.Instance == null) return;
            var availableAmmo = string.IsNullOrEmpty(weaponModel.CurrentWeaponId) ? 0 : TrailGame.Instance.Inventory.Quantity("revolver_ammo");
            var attack = weaponModel.TryAttack(availableAmmo);
            if (!attack.Accepted)
            {
                if (attack.Reason == "unarmed") TrailGame.Instance.UI.Toast("A mão está vazia. Encontre a faca antes de atacar.");
                else if (attack.Reason == "empty_ammo") TrailGame.Instance.UI.Toast("Sem cartuchos.");
                return;
            }
            if (attack.AmmoUsed > 0) TrailGame.Instance.Inventory.Consume(attack.AmmoItem, attack.AmmoUsed);
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out var hit, attack.Range))
            {
                var infected = hit.collider.GetComponentInParent<InfectedAI>();
                if (infected != null) infected.ApplyDamage(attack.Damage);
            }
            TrailGame.Instance.Audio.PlayCue(attack.AttackKind == "hitscan" ? "impact" : "creak");
            weaponCooldown = attack.Cooldown;
            Invoke(nameof(FinishWeaponAttack), attack.Cooldown);
        }
        private void FinishWeaponAttack() => weaponModel?.FinishAttack();
        public void Teleport(Vector3 position, float yaw = 0) { if (controller != null) controller.enabled = false; transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0)); if (controller != null) controller.enabled = true; velocity = Vector3.zero; }
        public void RestoreStamina() => stamina = staminaSeconds;
    }
}
