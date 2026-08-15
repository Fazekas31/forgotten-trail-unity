using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailInteractable : MonoBehaviour
    {
        public string eventId;
        public string prompt = "Examinar";
        public string title = "Pista";
        [TextArea(2, 8)] public string inspectionText = "O cowboy observa em silêncio.";
        public InteractionKind kind = InteractionKind.Inspect;
        public string itemId;
        public bool oneShot = true;
        public bool Consumed { get; private set; }
        public string Prompt => Consumed && oneShot ? string.Empty : prompt;

        public void Interact()
        {
            if (Consumed && oneShot) return;
            TrailGame.Instance?.Interact(this);
        }

        public void Consume() { Consumed = true; if (oneShot) gameObject.SetActive(false); }
    }
}
