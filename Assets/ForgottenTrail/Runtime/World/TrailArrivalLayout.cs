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

        // These are world-space landmarks for the authored town composition.
        // Keeping them in one contract prevents the imported GLB, procedural
        // gameplay props and narrative targets from drifting apart.
        public static readonly Vector3 SaloonCenter = new Vector3(-11f, .16f, 10f);
        public static readonly Vector3 ChurchCenter = new Vector3(10f, .16f, 24f);
        public static readonly Vector3 StationCenter = new Vector3(-11f, 0f, 28f);
        public static readonly Vector3 BarnCenter = new Vector3(8f, 0f, 50f);
        public static readonly Vector3 WellCenter = new Vector3(5f, .05f, 10f);
        public static readonly Vector3 CemeteryCenter = new Vector3(10f, 0f, 38.3f);

        // Origins of the authored procedural shells before they are moved to
        // the street anchors. These are deliberately kept here so a visual
        // rebuild cannot silently move the gameplay geometry away from the
        // imported architecture.
        public static readonly Vector3 GeneratedSaloonOrigin = new Vector3(-7.6f, 0f, 11.75f);
        public static readonly Vector3 GeneratedChurchOrigin = new Vector3(7f, 0f, 27.5f);

        public const float SaloonYaw = -90f;
        public const float ChurchYaw = 90f;
        public const float StationYaw = -90f;
        public const float BarnYaw = 0f;

        // The road is the readable spine of the arrival. The first ring of
        // trees frames the entrance, while the outer ring closes the town in
        // without blocking the saloon, church or sheriff's office approaches.
        public static readonly Vector3[] PerimeterTreePositions =
        {
            new Vector3(-12f, 0f, -18f), new Vector3(12f, 0f, -18f),
            new Vector3(-18f, 0f, -22f), new Vector3(18f, 0f, -22f),
            new Vector3(-25f, 0f, -16f), new Vector3(25f, 0f, -16f),
            new Vector3(-29f, 0f, -5f), new Vector3(29f, 0f, -4f),
            new Vector3(-27f, 0f, 6f), new Vector3(28f, 0f, 8f),
            new Vector3(-29f, 0f, 18f), new Vector3(30f, 0f, 20f),
            new Vector3(-28f, 0f, 31f), new Vector3(30f, 0f, 33f),
            new Vector3(-26f, 0f, 44f), new Vector3(28f, 0f, 46f),
            new Vector3(-21f, 0f, 56f), new Vector3(22f, 0f, 58f),
            new Vector3(-35f, 0f, 2f), new Vector3(35f, 0f, 6f),
            new Vector3(-36f, 0f, 14f), new Vector3(36f, 0f, 18f),
            new Vector3(-36f, 0f, 27f), new Vector3(36f, 0f, 31f),
            new Vector3(-34f, 0f, 41f), new Vector3(34f, 0f, 45f)
        };

        public static readonly Dictionary<string, Vector3> AuthoredArchitecturePositions = new()
        {
            ["ARCH_Saloon"] = SaloonCenter,
            ["ARCH_Church"] = ChurchCenter,
            ["ARCH_Station"] = new Vector3(-11f, .16f, 28f),
            ["ARCH_BoardingHouse_Pivot"] = new Vector3(-20f, 0f, 8f),
            ["ARCH_Mercantile_Pivot"] = new Vector3(20f, 0f, 10f),
            ["ARCH_Blacksmith_Pivot"] = new Vector3(-20f, 0f, 37f),
            ["ARCH_DoctorHouse_Pivot"] = new Vector3(20f, 0f, 34f),
            ["ARCH_NorthCabin_Pivot"] = new Vector3(-16f, 0f, 48f),
            ["ARCH_EastCabin_Pivot"] = new Vector3(18f, 0f, 48f)
        };

        public static readonly Dictionary<string, float> AuthoredArchitectureYaw = new()
        {
            ["ARCH_Saloon"] = SaloonYaw,
            ["ARCH_Church"] = ChurchYaw,
            ["ARCH_Station"] = StationYaw,
            ["ARCH_BoardingHouse_Pivot"] = -90f,
            ["ARCH_Mercantile_Pivot"] = 90f,
            ["ARCH_Blacksmith_Pivot"] = -90f,
            ["ARCH_DoctorHouse_Pivot"] = 90f,
            ["ARCH_NorthCabin_Pivot"] = -90f,
            ["ARCH_EastCabin_Pivot"] = 90f
        };

        /// <summary>
        /// Applies the reference street composition after the GLB axis
        /// conversion. Backdrop houses already have authored pivots; the
        /// three playable landmarks are direct mesh siblings and therefore
        /// need a shared anchor to keep their walls and roofs together.
        /// </summary>
        public static void ApplyAuthoredArchitectureLayout(Transform architecture)
        {
            if (architecture == null) return;

            var pivotNames = new[]
            {
                "ARCH_BoardingHouse_Pivot", "ARCH_Mercantile_Pivot", "ARCH_Blacksmith_Pivot",
                "ARCH_DoctorHouse_Pivot", "ARCH_NorthCabin_Pivot", "ARCH_EastCabin_Pivot"
            };
            foreach (var pivotName in pivotNames)
            {
                var pivot = FindChild(architecture, pivotName);
                if (pivot != null)
                {
                    pivot.position = AuthoredArchitecturePositions[pivotName];
                    pivot.rotation = Quaternion.Euler(0f, AuthoredArchitectureYaw[pivotName], 0f);
                }
            }

            LayoutDirectBuilding(architecture, "ARCH_Saloon", AuthoredArchitecturePositions["ARCH_Saloon"], AuthoredArchitectureYaw["ARCH_Saloon"]);
            LayoutDirectBuilding(architecture, "ARCH_Church", AuthoredArchitecturePositions["ARCH_Church"], AuthoredArchitectureYaw["ARCH_Church"]);
            LayoutDirectBuilding(architecture, "ARCH_Station", AuthoredArchitecturePositions["ARCH_Station"], AuthoredArchitectureYaw["ARCH_Station"]);
        }

        private static void LayoutDirectBuilding(Transform architecture, string prefix, Vector3 target, float yaw)
        {
            var parts = new List<Transform>();
            foreach (Transform child in architecture)
                if (child.name.StartsWith(prefix + "_")) parts.Add(child);

            if (parts.Count == 0) return;

            var anchorObject = new GameObject(prefix + "_LayoutAnchor");
            var anchor = anchorObject.transform;
            anchor.SetParent(architecture, true);
            var foundation = parts.Find(part => part.name == prefix + "_Foundation");
            anchor.position = foundation != null ? foundation.position : parts[0].position;
            foreach (var part in parts) part.SetParent(anchor, true);
            anchor.position = target;
            anchor.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }

        private static readonly Dictionary<string, Vector3> SpawnPoints = new()
        {
            ["arrival"] = new Vector3(0f, .05f, -24f),
            ["footprints"] = new Vector3(0f, .05f, -7.8f),
            ["threshold"] = new Vector3(0f, .05f, 1.5f),
            ["enter_saloon"] = new Vector3(-5.0f, .05f, 10f),
            ["meal"] = new Vector3(-10.4f, .05f, 10.2f),
            ["broken_door"] = new Vector3(-8.5f, .05f, 7.8f),
            ["diary"] = new Vector3(-10.4f, 3.48f, 9.6f),
            ["message"] = new Vector3(-9.5f, 3.48f, 14.6f),
            ["window"] = new Vector3(-9.2f, 3.48f, 14.6f),
            ["downstairs_noise"] = new Vector3(-10.6f, .05f, 10.0f),
            ["knife"] = new Vector3(-10.2f, .05f, 8.75f),
            ["exit_saloon"] = new Vector3(-5.0f, .05f, 10f),
            ["church_approach"] = new Vector3(1.6f, .05f, 24.5f),
            ["enter_church"] = new Vector3(5.5f, .05f, 24f),
            ["church_interior"] = new Vector3(10f, .05f, 29f),
            ["priest"] = new Vector3(10f, .05f, 30.6f),
            ["station"] = new Vector3(-5.0f, .05f, 28f),
            ["station_ledger"] = new Vector3(-10.4f, 3.48f, 27.4f),
            ["station_hale"] = new Vector3(-10.4f, .05f, 30.5f),
            ["station_key"] = new Vector3(-10.4f, .05f, 30.5f),
            ["leave_station"] = new Vector3(-5.0f, .05f, 28f),
            ["return_church"] = new Vector3(5.5f, .05f, 24f),
            ["barn"] = new Vector3(8f, .05f, 47.0f)
        };

        private static readonly Dictionary<string, Vector3> TargetPoints = new()
        {
            ["arrival"] = new Vector3(-1.65f, .8f, -4.35f),
            ["footprints"] = new Vector3(.1f, .08f, -1.2f),
            ["threshold"] = new Vector3(.1f, .08f, 2.6f),
            ["enter_saloon"] = new Vector3(-5.6f, 1.05f, 10f),
            ["meal"] = new Vector3(-10.2f, .86f, 10.2f),
            ["broken_door"] = new Vector3(-8.6f, 1.0f, 7.8f),
            ["diary"] = new Vector3(-10.4f, 4.02f, 9.6f),
            ["message"] = new Vector3(-9.5f, 4.65f, 14.6f),
            ["window"] = new Vector3(-9.2f, 4.65f, 14.6f),
            ["downstairs_noise"] = new Vector3(-10.6f, .68f, 10.0f),
            ["knife"] = new Vector3(-10.2f, 1.22f, 8.75f),
            ["exit_saloon"] = new Vector3(-5.6f, 1.05f, 10f),
            ["church_approach"] = new Vector3(5.5f, .08f, 24f),
            ["enter_church"] = new Vector3(5.8f, 1.05f, 24f),
            ["church_interior"] = new Vector3(10f, 1.05f, 29f),
            ["priest"] = new Vector3(10f, 1.45f, 30.6f),
            ["station"] = new Vector3(-5.8f, 1.05f, 28f),
            ["station_ledger"] = new Vector3(-10.4f, 4.02f, 27.4f),
            ["station_hale"] = new Vector3(-10.4f, 1.2f, 30.5f),
            ["station_key"] = new Vector3(-10.4f, 1.2f, 30.5f),
            ["leave_station"] = new Vector3(-5.8f, 1.05f, 28f),
            ["return_church"] = new Vector3(5.8f, 1.05f, 24f),
            ["barn"] = new Vector3(8f, 1.0f, 47.0f)
        };

        public static Vector3 SpawnFor(string step, Vector3 fallback)
            => SpawnPoints.TryGetValue(step, out var point) ? point : fallback;

        public static Vector3 TargetFor(string step, Vector3 fallback)
            => TargetPoints.TryGetValue(step, out var point) ? point : fallback;

        public static bool IsValidTownPlan(out string error)
        {
            error = null;
            if (SaloonCenter.x >= -4f || StationCenter.x >= -4f)
            {
                error = "The saloon and station must remain on the west side of the road.";
                return false;
            }

            if (ChurchCenter.x <= 4f || BarnCenter.x <= 2f)
            {
                error = "The church and barn must remain on the east side of the road.";
                return false;
            }

            if (Mathf.Abs(SaloonCenter.x - StationCenter.x) > 2f ||
                Mathf.Abs(ChurchCenter.x - CemeteryCenter.x) > 7f)
            {
                error = "The west buildings or the church/cemetery axis drifted apart.";
                return false;
            }

            if (BarnCenter.z <= CemeteryCenter.z || CemeteryCenter.z <= ChurchCenter.z)
            {
                error = "The north progression must be church, cemetery, then barn.";
                return false;
            }

            if (WellCenter.x <= 0f || Mathf.Abs(WellCenter.x) > 8f)
            {
                error = "The well must sit just east of the central road.";
                return false;
            }

            return true;
        }

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
