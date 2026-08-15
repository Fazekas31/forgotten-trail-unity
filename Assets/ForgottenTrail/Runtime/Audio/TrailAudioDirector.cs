using System.Collections;
using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailAudioDirector : MonoBehaviour
    {
        [Header("Importar pelo Unity MCP")]
        public AudioClip wind;
        public AudioClip footstepDirt;
        public AudioClip footstepWood;
        public AudioClip churchBell;
        public AudioClip heartbeat;
        public AudioClip creak;
        public AudioClip evidence;
        public AudioClip impact;
        private AudioSource ambience;
        private AudioSource oneShot;
        private float windTime;

        private void Awake()
        {
            ambience = gameObject.AddComponent<AudioSource>(); ambience.loop = true; ambience.volume = 0.18f; ambience.spatialBlend = 0f;
            oneShot = gameObject.AddComponent<AudioSource>(); oneShot.spatialBlend = 0.1f;
            if (wind != null) { ambience.clip = wind; ambience.Play(); }
        }

        private void Update()
        {
            windTime += Time.deltaTime;
            if (ambience != null && ambience.isPlaying) ambience.volume = 0.14f + Mathf.Sin(windTime * 0.17f) * 0.025f;
        }

        public void Footstep(string surface, string movement)
        {
            var clip = surface == "wood" ? footstepWood : footstepDirt;
            if (clip == null) return;
            oneShot.clip = clip; oneShot.volume = movement == "running" ? 0.9f : movement == "crouching" ? 0.18f : 0.55f;
            oneShot.pitch = movement == "running" ? 1.08f : movement == "crouching" ? 0.86f : 0.98f; oneShot.Play();
        }

        public void PlayCue(string cue)
        {
            var clip = cue switch { "church_bell" => churchBell, "evidence" or "journal" => evidence, "creak" => creak, "impact" => impact, "heartbeat" => heartbeat, _ => null };
            if (clip == null) return;
            oneShot.clip = clip; oneShot.volume = cue == "heartbeat" ? 0.7f : 0.55f; oneShot.pitch = cue == "church_bell" ? 0.82f : 1f; oneShot.Play();
        }

        public void PlayDelayed(string cue, float seconds) { StartCoroutine(DelayedCue(cue, seconds)); }
        private IEnumerator DelayedCue(string cue, float seconds) { yield return new WaitForSeconds(seconds); PlayCue(cue); }
    }
}
