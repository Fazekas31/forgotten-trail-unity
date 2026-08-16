using System.Collections.Generic;
using UnityEngine;

namespace ForgottenTrail
{
    /// <summary>
    /// Spatial contract for the first narrative slice. The campaign owns the order;
    /// this module owns where each beat happens in Ash Creek.
    /// </summary>
    public static class TrailArrivalLayout
    {
        public const float GroundAnchorY = .05f;

        private static readonly Dictionary<string, Vector3> SpawnPoints = new()
        {
            ["arrival"] = new Vector3(0f, .05f, -16f),
            ["footprints"] = new Vector3(0f, .05f, -5.6f),
            ["threshold"] = new Vector3(0f, .05f, 3.4f),
            ["enter_saloon"] = new Vector3(-6.1f, .05f, 6.5f),
            ["meal"] = new Vector3(-8.8f, .05f, 12.3f),
            ["broken_door"] = new Vector3(-9.7f, .05f, 10.1f),
            ["diary"] = new Vector3(-7.2f, 3.48f, 12.35f),
            ["message"] = new Vector3(-3.8f, 3.48f, 13.2f),
            ["window"] = new Vector3(-2.7f, 3.48f, 13.2f),
            ["downstairs_noise"] = new Vector3(-6.9f, .05f, 11.2f),
            ["knife"] = new Vector3(-6.4f, .05f, 12.5f),
            ["exit_saloon"] = new Vector3(-6.1f, .05f, 7.15f),
            ["church_approach"] = new Vector3(0f, .05f, 8.8f),
            ["enter_church"] = new Vector3(6.3f, .05f, 22.0f),
            ["church_interior"] = new Vector3(6.3f, .05f, 28.2f),
            ["priest"] = new Vector3(6.3f, .05f, 29.8f),
            ["station"] = new Vector3(.2f, .05f, 35.0f),
            ["station_ledger"] = new Vector3(.2f, 3.48f, 38.2f),
            ["station_hale"] = new Vector3(2.4f, .05f, 36.0f),
            ["station_key"] = new Vector3(2.4f, .05f, 36.0f),
            ["leave_station"] = new Vector3(.2f, .05f, 31.0f),
            ["return_church"] = new Vector3(6.3f, .05f, 21.0f),
            ["barn"] = new Vector3(0f, .05f, 47.0f)
        };

        private static readonly Dictionary<string, Vector3> TargetPoints = new()
        {
            ["arrival"] = new Vector3(-1.65f, .8f, -4.35f),
            ["footprints"] = new Vector3(.1f, .08f, -1.2f),
            ["threshold"] = new Vector3(-3.8f, .08f, 4.8f),
            ["enter_saloon"] = new Vector3(-6.1f, 1.05f, 7.05f),
            ["meal"] = new Vector3(-10.2f, .86f, 13.4f),
            ["broken_door"] = new Vector3(-9.65f, 1.0f, 10.05f),
            ["diary"] = new Vector3(-7.2f, 4.02f, 12.35f),
            ["message"] = new Vector3(-3.0f, 4.65f, 13.25f),
            ["window"] = new Vector3(-1.9f, 4.65f, 13.25f),
            ["downstairs_noise"] = new Vector3(-7.15f, .68f, 11.35f),
            ["knife"] = new Vector3(-6.35f, 1.22f, 12.55f),
            ["exit_saloon"] = new Vector3(-6.1f, 1.05f, 7.05f),
            ["church_approach"] = new Vector3(5.8f, .08f, 18.2f),
            ["enter_church"] = new Vector3(6.3f, 1.05f, 21.1f),
            ["church_interior"] = new Vector3(6.3f, 1.05f, 28.0f),
            ["priest"] = new Vector3(6.3f, 1.45f, 30.0f),
            ["station"] = new Vector3(.2f, 1.05f, 31.05f),
            ["station_ledger"] = new Vector3(.2f, 4.02f, 38.15f),
            ["station_hale"] = new Vector3(2.35f, 1.2f, 36.0f),
            ["station_key"] = new Vector3(2.35f, 1.2f, 36.0f),
            ["leave_station"] = new Vector3(.2f, 1.05f, 31.05f),
            ["return_church"] = new Vector3(6.3f, 1.05f, 21.1f),
            ["barn"] = new Vector3(0f, 1.0f, 46.0f)
        };

        public static Vector3 SpawnFor(string step, Vector3 fallback)
            => SpawnPoints.TryGetValue(step, out var point) ? point : fallback;

        public static Vector3 TargetFor(string step, Vector3 fallback)
            => TargetPoints.TryGetValue(step, out var point) ? point : fallback;

        public static Vector3 GroundAnchor(Vector3 reference)
            => new Vector3(reference.x, GroundAnchorY, reference.z);

        public static bool IsSaloonStep(string step)
            => step is "enter_saloon" or "meal" or "broken_door" or "diary" or "message" or "window" or "downstairs_noise" or "knife" or "exit_saloon";

        public static bool IsChurchStep(string step)
            => step is "church_approach" or "enter_church" or "church_interior" or "priest" or "return_church";

        public static bool IsStationStep(string step)
            => step is "station" or "station_ledger" or "station_hale" or "station_key" or "leave_station";
    }
}
