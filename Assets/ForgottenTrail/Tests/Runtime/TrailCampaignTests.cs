using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ForgottenTrail.Tests
{
    public sealed class TrailCampaignTests
    {
        private static readonly string[] Events =
        {
            "intro_finished", "footprints_found", "threshold_checked", "saloon_entered", "meal_inspected",
            "door_inspected", "diary_inspected", "message_inspected", "window_inspected", "downstairs_noise_checked",
            "knife_collected", "saloon_exited", "church_tracks_checked", "church_entered", "church_interior_checked",
            "priest_spoken", "station_entered", "station_ledger_read", "hale_spoken", "barn_key_collected",
            "station_left", "returned_to_priest", "barn_opened", "yard_crossed", "infected_distracted",
            "layla_found", "map_recovered", "barn_escaped", "mine_entered", "gallery_crossed", "mine_records_read",
            "bell_recovered", "layla_reunited", "chamber_reached"
        };

        [Test]
        public void CampaignRejectsOutOfOrderEventsAndAdvancesTheCanonicalPath()
        {
            var campaign = new TrailCampaign();

            Assert.That(campaign.CurrentStep, Is.EqualTo("arrival"));
            Assert.That(campaign.Report("footprints_found"), Is.False);
            Assert.That(campaign.Report(Events[0]), Is.True);
            Assert.That(campaign.CurrentStep, Is.EqualTo("footprints"));

            for (var i = 1; i < Events.Length; i++) Assert.That(campaign.Report(Events[i]), Is.True, "event index " + i);

            Assert.That(campaign.CurrentStep, Is.EqualTo("final_choice"));
            Assert.That(campaign.ChooseEnding(TrailEnding.SharedTrail), Is.True);
            Assert.That(campaign.IsComplete, Is.True);
            Assert.That(campaign.Ending, Is.EqualTo(TrailEnding.SharedTrail));
        }

        [Test]
        public void SnapshotRestoresDomainModelsAndLanternState()
        {
            var campaign = new TrailCampaign();
            var inventory = new InventoryModel();
            var journal = new JournalModel();
            var lantern = new LanternModel();
            inventory.Add("lantern");
            inventory.Add("ammo", 0);
            journal.RecordForStep("arrival");
            lantern.Acquire();
            lantern.Toggle();
            campaign.Report("intro_finished");

            var snapshot = campaign.Snapshot(inventory, journal, lantern);
            var restoredCampaign = new TrailCampaign();
            var restoredInventory = new InventoryModel();
            var restoredJournal = new JournalModel();
            var restoredLantern = new LanternModel();
            restoredCampaign.Restore(snapshot, restoredInventory, restoredJournal, restoredLantern);

            Assert.That(restoredCampaign.CurrentStep, Is.EqualTo("footprints"));
            Assert.That(restoredInventory.Quantity("lantern"), Is.EqualTo(1));
            Assert.That(restoredJournal.Ids, Does.Contain("arrival"));
            Assert.That(restoredLantern.Available, Is.True);
            Assert.That(restoredLantern.Lit, Is.False);
        }

        [Test]
        public void SaveStorePersistsFirstSaveAndRejectsUnknownSchema()
        {
            var store = new TrailSaveStore();
            store.Clear();
            var snapshot = new CampaignSnapshot { stepId = "mine_bell", checkpointId = "mine_bell", schemaVersion = 1, inventory = new List<string> { "ventilation_bell" } };

            Assert.That(store.Save(snapshot), Is.True);
            var loaded = store.Load();
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.stepId, Is.EqualTo("mine_bell"));
            Assert.That(loaded.inventory, Does.Contain("ventilation_bell"));
            store.Clear();
        }

        [Test]
        public void WeaponModelMatchesKnifeAndRevolverRules()
        {
            var weapons = new TrailWeaponModel();
            Assert.That(weapons.Equip("knife"), Is.True);
            var knife = weapons.TryAttack(0);
            Assert.That(knife.Accepted, Is.True);
            Assert.That(knife.AttackKind, Is.EqualTo("melee"));
            weapons.FinishAttack();
            Assert.That(weapons.Equip("revolver"), Is.True);
            Assert.That(weapons.TryAttack(0).Reason, Is.EqualTo("empty_ammo"));
            var shot = weapons.TryAttack(1);
            Assert.That(shot.Accepted, Is.True);
            Assert.That(shot.AmmoUsed, Is.EqualTo(1));
        }

        [Test]
        public void ArrivalLayoutFollowsTheSaloonInvestigationRoute()
        {
            var arrival = TrailArrivalLayout.SpawnFor("arrival", Vector3.zero);
            var footprints = TrailArrivalLayout.SpawnFor("footprints", Vector3.zero);
            var threshold = TrailArrivalLayout.SpawnFor("threshold", Vector3.zero);
            var groundFloor = TrailArrivalLayout.SpawnFor("meal", Vector3.zero);
            var upperFloor = TrailArrivalLayout.SpawnFor("diary", Vector3.zero);

            Assert.That(arrival.z, Is.LessThan(footprints.z));
            Assert.That(footprints.z, Is.LessThan(threshold.z));
            Assert.That(TrailArrivalLayout.IsSaloonStep("meal"), Is.True);
            Assert.That(TrailArrivalLayout.IsSaloonStep("diary"), Is.True);
            Assert.That(groundFloor.y, Is.LessThan(upperFloor.y));
            Assert.That(TrailArrivalLayout.TargetFor("knife", Vector3.zero).x, Is.EqualTo(-10.2f).Within(.01f));
        }

        [Test]
        public void ArrivalLayoutConnectsChurchAndStationBeats()
        {
            Assert.That(TrailArrivalLayout.IsChurchStep("priest"), Is.True);
            Assert.That(TrailArrivalLayout.IsStationStep("station_ledger"), Is.True);
            Assert.That(TrailArrivalLayout.SpawnFor("station", Vector3.zero).x, Is.LessThan(TrailArrivalLayout.SpawnFor("enter_church", Vector3.zero).x));
            Assert.That(TrailArrivalLayout.SpawnFor("station", Vector3.zero).x, Is.LessThan(-4f));
            Assert.That(TrailArrivalLayout.TargetFor("station_ledger", Vector3.zero).y, Is.GreaterThan(3.5f));
        }

        [Test]
        public void BackdropBuildingsKeepTheirDesignReferenceButStartOnTheGround()
        {
            var reference = TrailArrivalLayout.AuthoredArchitecturePositions["ARCH_BoardingHouse_Pivot"];
            var anchor = TrailArrivalLayout.GroundAnchor(reference);

            Assert.That(anchor.x, Is.EqualTo(reference.x).Within(.001f));
            Assert.That(anchor.z, Is.EqualTo(reference.z).Within(.001f));
            Assert.That(anchor.y, Is.EqualTo(TrailArrivalLayout.GroundAnchorY).Within(.001f));
            Assert.That(anchor.y, Is.LessThan(.2f));
        }

        [Test]
        public void AshCreekTownPlanKeepsTheNarrativeOrderAndRoadOpen()
        {
            Assert.That(TrailArrivalLayout.IsValidTownPlan(out var error), Is.True, error);
            Assert.That(TrailArrivalLayout.SaloonCenter.x, Is.LessThan(-4f));
            Assert.That(TrailArrivalLayout.StationCenter.x, Is.LessThan(-4f));
            Assert.That(TrailArrivalLayout.ChurchCenter.x, Is.GreaterThan(4f));
            Assert.That(TrailArrivalLayout.BarnCenter.z, Is.GreaterThan(TrailArrivalLayout.CemeteryCenter.z));
            Assert.That(TrailArrivalLayout.CemeteryCenter.z, Is.GreaterThan(TrailArrivalLayout.ChurchCenter.z));

            foreach (var tree in TrailArrivalLayout.PerimeterTreePositions)
            {
                Assert.That(Mathf.Abs(tree.x), Is.GreaterThan(9f), "A perimeter tree entered the central avenue.");
                Assert.That(tree.z, Is.LessThan(62f), "A perimeter tree is outside the authored town bounds.");
            }
        }

        [Test]
        public void BlenderArchitectureAssetIsImportedWithAuthoredLandmarks()
        {
            var architecture = Resources.Load<GameObject>("Environment/AshCreek_Architecture");

            Assert.That(architecture, Is.Not.Null);
            Assert.That(architecture.GetComponentsInChildren<MeshRenderer>(true).Length, Is.GreaterThan(100));
            Assert.That(architecture.GetComponentsInChildren<Transform>(true), Has.Some.Property("name").EqualTo("ARCH_Saloon_Foundation"));
            Assert.That(architecture.GetComponentsInChildren<Transform>(true), Has.Some.Property("name").EqualTo("ARCH_Church_Tower"));
            Assert.That(architecture.GetComponentsInChildren<Transform>(true), Has.Some.Property("name").EqualTo("ARCH_Station_Sign_Board"));
        }

        [Test]
        public void AuthoredArchitectureBackdropPivotsDoNotRollBuildings()
        {
            var architecture = Resources.Load<GameObject>("Environment/AshCreek_Architecture");
            Assert.That(architecture, Is.Not.Null);

            var pivots = new List<Transform>();
            foreach (var transform in architecture.GetComponentsInChildren<Transform>(true))
                if (transform.name.EndsWith("_Pivot")) pivots.Add(transform);

            Assert.That(pivots, Is.Not.Empty);
            foreach (var pivot in pivots)
            {
                var angles = pivot.localEulerAngles;
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(angles.x, 0f)), Is.LessThan(.01f), pivot.name + " has pitch");
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(angles.z, 0f)), Is.LessThan(.01f), pivot.name + " has roll");
            }
        }

        [Test]
        public void AuthoredArchitectureConversionKeepsLandmarksUpright()
        {
            var prefab = Resources.Load<GameObject>("Environment/AshCreek_Architecture");
            Assert.That(prefab, Is.Not.Null);

            var instance = Object.Instantiate(prefab);
            instance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            instance.transform.localScale = new Vector3(-1f, 1f, 1f);
            foreach (Transform child in instance.transform)
            {
                child.localRotation = child.name.EndsWith("_Pivot")
                    ? Quaternion.identity
                    : Quaternion.Euler(-90f, 0f, 0f) * child.localRotation;
            }
            TrailArrivalLayout.ApplyAuthoredArchitectureLayout(instance.transform);
            var saloonFoundation = FindChild(instance.transform, "ARCH_Saloon_Foundation");
            var churchFoundation = FindChild(instance.transform, "ARCH_Church_Foundation");
            var stationFoundation = FindChild(instance.transform, "ARCH_Station_Foundation");

            Assert.That(saloonFoundation, Is.Not.Null);
            Assert.That(Vector3.Angle(saloonFoundation.up, Vector3.up), Is.LessThan(.01f));
            Assert.That(Vector3.Angle(churchFoundation.up, Vector3.up), Is.LessThan(.01f));
            Assert.That(Vector3.Angle(stationFoundation.up, Vector3.up), Is.LessThan(.01f));
            Assert.That(saloonFoundation.position.x, Is.EqualTo(TrailArrivalLayout.SaloonCenter.x).Within(.01f));
            Assert.That(saloonFoundation.position.y, Is.EqualTo(TrailArrivalLayout.SaloonCenter.y).Within(.01f));
            Assert.That(saloonFoundation.position.z, Is.EqualTo(TrailArrivalLayout.SaloonCenter.z).Within(.01f));
            Assert.That(churchFoundation.position.x, Is.EqualTo(TrailArrivalLayout.ChurchCenter.x).Within(.01f));
            Assert.That(churchFoundation.position.y, Is.EqualTo(TrailArrivalLayout.ChurchCenter.y).Within(.01f));
            Assert.That(churchFoundation.position.z, Is.EqualTo(TrailArrivalLayout.ChurchCenter.z).Within(.01f));
            Assert.That(stationFoundation.position.x, Is.EqualTo(TrailArrivalLayout.StationCenter.x).Within(.01f));
            Assert.That(stationFoundation.position.y, Is.EqualTo(.16f).Within(.01f));
            Assert.That(stationFoundation.position.z, Is.EqualTo(TrailArrivalLayout.StationCenter.z).Within(.01f));
            Object.DestroyImmediate(instance);
        }

        [Test]
        public void AuthoredBackdropPivotsPreserveTheReferenceStreetLayout()
        {
            var prefab = Resources.Load<GameObject>("Environment/AshCreek_Architecture");
            Assert.That(prefab, Is.Not.Null);

            var instance = Object.Instantiate(prefab);
            instance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            instance.transform.localScale = new Vector3(-1f, 1f, 1f);
            foreach (Transform child in instance.transform)
                child.localRotation = child.name.EndsWith("_Pivot")
                    ? Quaternion.identity
                    : Quaternion.Euler(-90f, 0f, 0f) * child.localRotation;
            TrailArrivalLayout.ApplyAuthoredArchitectureLayout(instance.transform);

            var expected = new Dictionary<string, Vector3>
            {
                ["ARCH_BoardingHouse_Pivot"] = new Vector3(-20f, 0f, 8f),
                ["ARCH_Mercantile_Pivot"] = new Vector3(20f, 0f, 10f),
                ["ARCH_Blacksmith_Pivot"] = new Vector3(-20f, 0f, 37f),
                ["ARCH_DoctorHouse_Pivot"] = new Vector3(20f, 0f, 34f),
                ["ARCH_NorthCabin_Pivot"] = new Vector3(-16f, 0f, 48f),
                ["ARCH_EastCabin_Pivot"] = new Vector3(18f, 0f, 48f)
            };

            foreach (var pair in expected)
            {
                var pivot = FindChild(instance.transform, pair.Key);
                Assert.That(pivot, Is.Not.Null, pair.Key + " is missing");
                Assert.That(pivot.position.x, Is.EqualTo(pair.Value.x).Within(.6f), pair.Key + " x");
                Assert.That(pivot.position.y, Is.EqualTo(pair.Value.y).Within(.2f), pair.Key + " y");
                Assert.That(pivot.position.z, Is.EqualTo(pair.Value.z).Within(.6f), pair.Key + " z");
            }

            var stationFoundation = FindChild(instance.transform, "ARCH_Station_Foundation");
            Assert.That(stationFoundation, Is.Not.Null);
            Assert.That(stationFoundation.position.x, Is.EqualTo(-11f).Within(.6f));
            Assert.That(stationFoundation.position.z, Is.EqualTo(28f).Within(.6f));
            Assert.That(FindChild(instance.transform, "ARCH_Station_LayoutAnchor"), Is.Not.Null);

            Object.DestroyImmediate(instance);
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }

        [Test]
        public void AssetStoreTerrainAndDirtRoadHooksAreAvailable()
        {
            var terrain = Resources.Load<GameObject>("Environment/AssetStoreTerrainHigh");
            var dirtRoad = Resources.Load<Texture2D>("Environment/AssetStoreRoadTextures/dirtRoad_A");
            var dirtRoadNormal = Resources.Load<Texture2D>("Environment/AssetStoreRoadTextures/dirtRoad_N");

            Assert.That(terrain, Is.Not.Null);
            Assert.That(terrain.GetComponentsInChildren<Terrain>(true).Length, Is.EqualTo(16));
            Assert.That(dirtRoad, Is.Not.Null);
            Assert.That(dirtRoadNormal, Is.Not.Null);
        }

        [Test]
        public void StartingTheGameClosesTheMenuAndUnblocksInput()
        {
            var host = new GameObject("TrailUITest");
            var ui = host.AddComponent<TrailUI>();

            Assert.That(ui.MenuOpen, Is.True);
            ui.StartGame();

            Assert.That(ui.MenuOpen, Is.False);
            Assert.That(ui.BlocksPlayer, Is.False);
            Object.DestroyImmediate(host);
        }
    }
}
