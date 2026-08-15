using System;
using UnityEngine;

namespace ForgottenTrail
{
    public readonly struct NoiseEvent
    {
        public readonly Vector3 Position; public readonly float Radius; public readonly string Source;
        public NoiseEvent(Vector3 position, float radius, string source) { Position = position; Radius = radius; Source = source; }
    }

    public static class NoiseSystem
    {
        public static event Action<NoiseEvent> Emitted;
        public static void Emit(Vector3 position, float radius, string source) => Emitted?.Invoke(new NoiseEvent(position, radius, source));
    }
}
