using UnityEngine;

namespace ForgottenTrail
{
    public sealed class InfectedAI : MonoBehaviour
    {
        public float speed = 1.15f;
        public float hearingMultiplier = 1f;
        private Vector3 investigate;
        private bool investigating;
        private float forgetTimer;
        private float health = 100f;

        private void OnEnable() => NoiseSystem.Emitted += OnNoise;
        private void OnDisable() => NoiseSystem.Emitted -= OnNoise;
        private void OnNoise(NoiseEvent noise)
        {
            var distance = Vector3.Distance(transform.position, noise.Position);
            if (distance <= noise.Radius * hearingMultiplier) { investigate = noise.Position; investigating = true; forgetTimer = 5.5f; }
        }
        private void Update()
        {
            if (!investigating) return;
            forgetTimer -= Time.deltaTime; if (forgetTimer <= 0) { investigating = false; return; }
            var flat = investigate - transform.position; flat.y = 0;
            if (flat.sqrMagnitude > 0.6f) transform.position += flat.normalized * speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(flat.sqrMagnitude > 0.01f ? flat : transform.forward);
        }

        public void ApplyDamage(float amount)
        {
            health -= amount;
            if (health <= 0f) Destroy(gameObject);
        }
    }
}
