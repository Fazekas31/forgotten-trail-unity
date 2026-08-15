using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForgottenTrail
{
    public enum TrailAct { Arrival = 1, Barn = 2, Mine = 3, Final = 4 }
    public enum TrailEnding { None, SharedTrail, DefinitiveSilence }
    public enum InteractionKind { Inspect, Collect, Dialogue, Transition, Choice, Ending }

    [Serializable]
    public sealed class CampaignSnapshot
    {
        public int schemaVersion = 1;
        public string stepId = "arrival";
        public string checkpointId = "arrival";
        public TrailEnding ending = TrailEnding.None;
        public bool lanternAvailable;
        public bool lanternLit;
        public List<string> inventory = new();
        public List<string> journal = new();
    }

    [Serializable]
    public sealed class TrailItem
    {
        public string id;
        public string namePt;
        public string nameEn;
        public string descriptionPt;
        public string descriptionEn;
        public string category;
        public int maxStack;
    }

    [Serializable]
    public sealed class JournalEntry
    {
        public string id;
        public string titlePt;
        public string titleEn;
        public string momentPt;
        public string momentEn;
        public string textPt;
        public string textEn;

        public string Title(bool english) => english ? titleEn : titlePt;
        public string Moment(bool english) => english ? momentEn : momentPt;
        public string Text(bool english) => english ? textEn : textPt;
    }

    public static class TrailContent
    {
        public static readonly string[] StepOrder =
        {
            "arrival", "footprints", "threshold", "enter_saloon", "meal", "broken_door",
            "diary", "message", "window", "downstairs_noise", "knife", "exit_saloon",
            "church_approach", "enter_church", "church_interior", "priest", "station",
            "station_ledger", "station_hale", "station_key", "leave_station", "return_church",
            "barn", "barn_yard", "barn_noise", "barn_layla", "barn_map", "barn_collapse",
            "mine_entrance", "mine_galleries", "mine_records", "mine_bell", "mine_reunion",
            "final_chamber", "final_choice", "complete"
        };

        public static readonly Dictionary<string, string> ObjectivesPt = new()
        {
            ["arrival"] = "Encontre o rastro", ["footprints"] = "Examine as pegadas",
            ["threshold"] = "Siga o rastro até o saloon", ["enter_saloon"] = "Entre no saloon",
            ["meal"] = "Investigue a cozinha", ["broken_door"] = "Examine a porta destruída",
            ["diary"] = "Examine o diário de bolso", ["message"] = "Leia o aviso ensanguentado",
            ["window"] = "Investigue a janela", ["downstairs_noise"] = "Examine os destroços",
            ["knife"] = "Pegue a faca sobre a mesa", ["exit_saloon"] = "Saia do saloon",
            ["church_approach"] = "Examine as pegadas diante da igreja", ["enter_church"] = "Entre na igreja",
            ["church_interior"] = "Investigue o altar", ["priest"] = "Fale com o padre Elias",
            ["station"] = "Vá até a delegacia", ["station_ledger"] = "Procure o registro vermelho",
            ["station_hale"] = "Encontre o xerife Hale", ["station_key"] = "Pegue a chave do celeiro",
            ["leave_station"] = "Saia da delegacia", ["return_church"] = "Volte para o padre Elias",
            ["barn"] = "Abra o celeiro", ["barn_yard"] = "Atravesse o pátio sem fazer barulho",
            ["barn_noise"] = "Desvie a atenção dos infectados", ["barn_layla"] = "Encontre Layla",
            ["barn_map"] = "Recupere o mapa de ventilação", ["barn_collapse"] = "Escape pelo túnel de serviço",
            ["mine_entrance"] = "Entre na mina", ["mine_galleries"] = "Siga as vozes pela galeria",
            ["mine_records"] = "Descubra como a mina foi aberta", ["mine_bell"] = "Recupere a Campainha de Ventilação",
            ["mine_reunion"] = "Encontre Layla novamente", ["final_chamber"] = "Chegue à câmara profunda",
            ["final_choice"] = "Escolha o destino da câmara", ["complete"] = "Campanha concluída"
        };

        public static readonly Dictionary<string, string> ObjectivesEn = new()
        {
            ["arrival"] = "Find the trail", ["footprints"] = "Examine the footprints",
            ["threshold"] = "Follow the trail to the saloon", ["enter_saloon"] = "Enter the saloon",
            ["meal"] = "Investigate the kitchen", ["broken_door"] = "Examine the broken door",
            ["diary"] = "Examine the pocket diary", ["message"] = "Read the bloody warning",
            ["window"] = "Investigate the window", ["downstairs_noise"] = "Examine the debris",
            ["knife"] = "Take the knife from the table", ["exit_saloon"] = "Leave the saloon",
            ["church_approach"] = "Examine the footprints by the church", ["enter_church"] = "Enter the church",
            ["church_interior"] = "Investigate the altar", ["priest"] = "Speak with Father Elias",
            ["station"] = "Go to the station", ["station_ledger"] = "Find the red ledger",
            ["station_hale"] = "Find Sheriff Hale", ["station_key"] = "Take the barn key",
            ["leave_station"] = "Leave the station", ["return_church"] = "Return to Father Elias",
            ["barn"] = "Open the barn", ["barn_yard"] = "Cross the yard quietly",
            ["barn_noise"] = "Distract the infected", ["barn_layla"] = "Find Layla",
            ["barn_map"] = "Recover the ventilation map", ["barn_collapse"] = "Escape through the service tunnel",
            ["mine_entrance"] = "Enter the mine", ["mine_galleries"] = "Follow the voices through the gallery",
            ["mine_records"] = "Discover how the mine was opened", ["mine_bell"] = "Recover the Ventilation Bell",
            ["mine_reunion"] = "Find Layla again", ["final_chamber"] = "Reach the deep chamber",
            ["final_choice"] = "Choose the chamber's fate", ["complete"] = "Campaign complete"
        };

        public static readonly Dictionary<string, TrailItem> Items = new()
        {
            ["lantern"] = new() { id="lantern", namePt="Lampião", nameEn="Lantern", descriptionPt="Equipamento de viagem. [F] acende ou apaga.", descriptionEn="Travel equipment. [F] toggles it.", category="equipment", maxStack=1 },
            ["knife"] = new() { id="knife", namePt="Faca de cozinha", nameEn="Kitchen knife", descriptionPt="Defesa de curto alcance.", descriptionEn="Short-range defense.", category="melee_weapon", maxStack=1 },
            ["revolver_ammo"] = new() { id="revolver_ammo", namePt="Cartuchos de revólver", nameEn="Revolver rounds", descriptionPt="Munição seca.", descriptionEn="Dry ammunition.", category="ammo", maxStack=24 },
            ["deputy_badge"] = new() { id="deputy_badge", namePt="Distintivo de delegado", nameEn="Deputy's badge", descriptionPt="Uma identificação entregue por Elias.", descriptionEn="Identification given by Elias.", category="key_item", maxStack=1 },
            ["red_ledger"] = new() { id="red_ledger", namePt="Registro vermelho", nameEn="Red ledger", descriptionPt="O nome de Layla aparece entre os sobreviventes.", descriptionEn="Layla's name appears among the survivors.", category="key_item", maxStack=1 },
            ["barn_key"] = new() { id="barn_key", namePt="Chave do celeiro", nameEn="Barn key", descriptionPt="A única chave do celeiro.", descriptionEn="The only key to the barn.", category="key_item", maxStack=1 },
            ["ventilation_map"] = new() { id="ventilation_map", namePt="Mapa de ventilação", nameEn="Ventilation map", descriptionPt="Rotas antigas entre o celeiro e a mina.", descriptionEn="Old routes between the barn and mine.", category="key_item", maxStack=1 },
            ["ventilation_bell"] = new() { id="ventilation_bell", namePt="Campainha de Ventilação", nameEn="Ventilation Bell", descriptionPt="Uma frequência que interrompe o Imitador.", descriptionEn="A frequency that interrupts the Mimic.", category="equipment", maxStack=1 }
        };
    }
}
