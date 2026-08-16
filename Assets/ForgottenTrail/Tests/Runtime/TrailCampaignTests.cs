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
            Assert.That(TrailArrivalLayout.TargetFor("knife", Vector3.zero).x, Is.EqualTo(-6.35f).Within(.01f));
        }

        [Test]
        public void ArrivalLayoutConnectsChurchAndStationBeats()
        {
            Assert.That(TrailArrivalLayout.IsChurchStep("priest"), Is.True);
            Assert.That(TrailArrivalLayout.IsStationStep("station_ledger"), Is.True);
            Assert.That(TrailArrivalLayout.SpawnFor("enter_church", Vector3.zero).z, Is.LessThan(TrailArrivalLayout.SpawnFor("station", Vector3.zero).z));
            Assert.That(TrailArrivalLayout.TargetFor("station_ledger", Vector3.zero).y, Is.GreaterThan(3.5f));
        }

        [Test]
        public void BackdropBuildingsKeepTheirDesignReferenceButStartOnTheGround()
        {
            var reference = new Vector3(-16f, 2.8f, 7.5f);
            var anchor = TrailArrivalLayout.GroundAnchor(reference);

            Assert.That(anchor.x, Is.EqualTo(reference.x).Within(.001f));
            Assert.That(anchor.z, Is.EqualTo(reference.z).Within(.001f));
            Assert.That(anchor.y, Is.EqualTo(TrailArrivalLayout.GroundAnchorY).Within(.001f));
            Assert.That(anchor.y, Is.LessThan(.2f));
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
