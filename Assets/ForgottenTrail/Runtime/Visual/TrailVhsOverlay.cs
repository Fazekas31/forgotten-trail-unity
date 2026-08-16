using UnityEngine;

namespace ForgottenTrail
{
    /// <summary>
    /// Screen-space presentation layer matching the Godot prototype's analog-horror pass:
    /// amber phosphor tint, restrained scanlines, vignette, and deterministic noise.
    /// </summary>
    public sealed class TrailVhsOverlay : MonoBehaviour
    {
        private Texture2D pixel;
        private float noiseClock;

        private void Awake()
        {
            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = "ForgottenTrail_VhsPixel" };
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply(false, true);
        }

        private void Update() => noiseClock += Time.unscaledDeltaTime;

        private void OnGUI()
        {
            if (pixel == null || Screen.width < 2 || Screen.height < 2) return;
            var previous = GUI.color;

            GUI.color = new Color(0.09f, 0.035f, 0.015f, 0.035f);
            for (var y = 0; y < Screen.height; y += 4)
                GUI.DrawTexture(new Rect(0, y, Screen.width, 1), pixel);

            var noise = Mathf.PerlinNoise(noiseClock * 1.7f, 0.13f) * 0.022f;
            GUI.color = new Color(0.7f, 0.34f, 0.13f, 0.025f + noise);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pixel);

            GUI.color = new Color(0f, 0f, 0f, 0.18f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, 18), pixel);
            GUI.DrawTexture(new Rect(0, Screen.height - 18, Screen.width, 18), pixel);
            GUI.DrawTexture(new Rect(0, 0, 18, Screen.height), pixel);
            GUI.DrawTexture(new Rect(Screen.width - 18, 0, 18, Screen.height), pixel);

            GUI.color = previous;
        }

        private void OnDestroy()
        {
            if (pixel != null) Destroy(pixel);
        }
    }
}
