using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailCampaign
    {
        private readonly Dictionary<string, string> requiredEvents = new()
        {
            ["arrival"] = "intro_finished", ["footprints"] = "footprints_found", ["threshold"] = "threshold_checked",
            ["enter_saloon"] = "saloon_entered", ["meal"] = "meal_inspected", ["broken_door"] = "door_inspected",
            ["diary"] = "diary_inspected", ["message"] = "message_inspected", ["window"] = "window_inspected",
            ["downstairs_noise"] = "downstairs_noise_checked", ["knife"] = "knife_collected", ["exit_saloon"] = "saloon_exited",
            ["church_approach"] = "church_tracks_checked", ["enter_church"] = "church_entered", ["church_interior"] = "church_interior_checked",
            ["priest"] = "priest_spoken", ["station"] = "station_entered", ["station_ledger"] = "station_ledger_read",
            ["station_hale"] = "hale_spoken", ["station_key"] = "barn_key_collected", ["leave_station"] = "station_left",
            ["return_church"] = "returned_to_priest", ["barn"] = "barn_opened", ["barn_yard"] = "yard_crossed",
            ["barn_noise"] = "infected_distracted", ["barn_layla"] = "layla_found", ["barn_map"] = "map_recovered",
            ["barn_collapse"] = "barn_escaped", ["mine_entrance"] = "mine_entered", ["mine_galleries"] = "gallery_crossed",
            ["mine_records"] = "mine_records_read", ["mine_bell"] = "bell_recovered", ["mine_reunion"] = "layla_reunited",
            ["final_chamber"] = "chamber_reached"
        };

        private int index;
        public string CurrentStep => TrailContent.StepOrder[Mathf.Clamp(index, 0, TrailContent.StepOrder.Length - 1)];
        public TrailAct CurrentAct => CurrentStep switch
        {
            "barn" or "barn_yard" or "barn_noise" or "barn_layla" or "barn_map" or "barn_collapse" => TrailAct.Barn,
            "mine_entrance" or "mine_galleries" or "mine_records" or "mine_bell" or "mine_reunion" => TrailAct.Mine,
            "final_chamber" or "final_choice" or "complete" => TrailAct.Final,
            _ => TrailAct.Arrival
        };
        public string Checkpoint { get; private set; } = "arrival";
        public TrailEnding Ending { get; private set; }
        public event Action<string> StepChanged;
        public event Action<string> CheckpointReached;
        public event Action<TrailEnding> Completed;

        public bool IsComplete => CurrentStep == "complete";
        public bool HasReached(string step)
        {
            var target = Array.IndexOf(TrailContent.StepOrder, step);
            return target >= 0 && target <= index;
        }

        public bool Report(string eventId)
        {
            if (IsComplete || !requiredEvents.TryGetValue(CurrentStep, out var expected) || expected != eventId) return false;
            index++;
            if (CurrentStep is "arrival" or "enter_saloon" or "broken_door" or "knife" or "exit_saloon" or "enter_church" or "station" or "station_key" or "leave_station" or "return_church" or "barn" or "barn_collapse" or "mine_entrance" or "mine_bell" or "final_chamber")
            {
                Checkpoint = CurrentStep;
                CheckpointReached?.Invoke(Checkpoint);
            }
            StepChanged?.Invoke(CurrentStep);
            return true;
        }

        public bool ChooseEnding(TrailEnding ending)
        {
            if (CurrentStep != "final_choice" || ending == TrailEnding.None) return false;
            Ending = ending;
            index = Array.IndexOf(TrailContent.StepOrder, "complete");
            StepChanged?.Invoke(CurrentStep);
            Completed?.Invoke(ending);
            return true;
        }

        public CampaignSnapshot Snapshot(InventoryModel inventory, JournalModel journal, LanternModel lantern) => new()
        {
            stepId = CurrentStep, checkpointId = Checkpoint, ending = Ending,
            lanternAvailable = lantern.Available, lanternLit = lantern.Lit,
            inventory = inventory.Snapshot(), journal = journal.Snapshot()
        };

        public void Restore(CampaignSnapshot snapshot, InventoryModel inventory, JournalModel journal, LanternModel lantern)
        {
            if (snapshot == null) return;
            var restored = Array.IndexOf(TrailContent.StepOrder, snapshot.stepId);
            index = restored >= 0 ? restored : 0;
            Checkpoint = string.IsNullOrEmpty(snapshot.checkpointId) ? CurrentStep : snapshot.checkpointId;
            Ending = snapshot.ending;
            inventory.Restore(snapshot.inventory ?? new List<string>());
            journal.Restore(snapshot.journal ?? new List<string>());
            lantern.Restore(snapshot.lanternAvailable, snapshot.lanternLit);
            StepChanged?.Invoke(CurrentStep);
        }
    }
}
