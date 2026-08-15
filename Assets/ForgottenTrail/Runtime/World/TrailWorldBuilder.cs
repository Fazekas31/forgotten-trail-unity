using System.Collections.Generic;
using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailWorldBuilder : MonoBehaviour
    {
        public Vector3 SpawnPoint { get; private set; }
        private readonly Dictionary<string, string> events = new()
        {
            ["arrival"]="intro_finished", ["footprints"]="footprints_found", ["threshold"]="threshold_checked", ["enter_saloon"]="saloon_entered", ["meal"]="meal_inspected", ["broken_door"]="door_inspected", ["diary"]="diary_inspected", ["message"]="message_inspected", ["window"]="window_inspected", ["downstairs_noise"]="downstairs_noise_checked", ["knife"]="knife_collected", ["exit_saloon"]="saloon_exited", ["church_approach"]="church_tracks_checked", ["enter_church"]="church_entered", ["church_interior"]="church_interior_checked", ["priest"]="priest_spoken", ["station"]="station_entered", ["station_ledger"]="station_ledger_read", ["station_hale"]="hale_spoken", ["station_key"]="barn_key_collected", ["leave_station"]="station_left", ["return_church"]="returned_to_priest", ["barn"]="barn_opened", ["barn_yard"]="yard_crossed", ["barn_noise"]="infected_distracted", ["barn_layla"]="layla_found", ["barn_map"]="map_recovered", ["barn_collapse"]="barn_escaped", ["mine_entrance"]="mine_entered", ["mine_galleries"]="gallery_crossed", ["mine_records"]="mine_records_read", ["mine_bell"]="bell_recovered", ["mine_reunion"]="layla_reunited", ["final_chamber"]="chamber_reached"
        };
        private Transform root;
        private readonly List<GameObject> spawned = new();
        private Material ground, wood, darkWood, brick, rust, moon, blood, gold;

        public void Build(TrailAct act, string step)
        {
            Clear(); ConfigureAtmosphere(act); CreateMaterials(); root = new GameObject("AshCreek_Act_" + (int)act).transform;
            SpawnPoint = act switch { TrailAct.Arrival => new Vector3(0, 0.05f, -8), TrailAct.Barn => new Vector3(0, 0.05f, -7), TrailAct.Mine => new Vector3(0, 0.05f, -6), _ => new Vector3(0, 0.05f, -5) };
            CreateBox("Ground", new Vector3(0, -0.18f, 8), new Vector3(44, .3f, 48), ground, true);
            switch (act) { case TrailAct.Arrival: BuildArrival(); break; case TrailAct.Barn: BuildBarn(); break; case TrailAct.Mine: BuildMine(); break; case TrailAct.Final: BuildFinal(); break; }
            if (events.TryGetValue(step, out var eventId)) CreateTarget(step, eventId, TargetTitle(step), TargetText(step), TargetItem(step));
            else if (step == "final_choice") CreateChoiceTarget();
            if (act is TrailAct.Barn or TrailAct.Mine) { CreateInfected(new Vector3(4, .75f, 12)); CreateInfected(new Vector3(-5, .75f, 22)); }
        }

        private void BuildArrival()
        {
            CreateBox("Saloon", new Vector3(-8, 2.3f, 12), new Vector3(9, 4.6f, 8), wood, false); CreateBox("Church", new Vector3(8, 2.8f, 25), new Vector3(8, 5.6f, 9), brick, false); CreateBox("Station", new Vector3(0, 2.3f, 34), new Vector3(8, 4.6f, 7), darkWood, false);
            CreateBox("Road", new Vector3(0, .03f, 13), new Vector3(4, .08f, 42), new Material(ground) { color = new Color(.28f,.18f,.11f) }, false); CreateTownDetails();
        }
        private void BuildBarn()
        {
            CreateBox("SurvivorBarn", new Vector3(0, 3.2f, 18), new Vector3(14, 6.4f, 13), wood, false); CreateBox("BarnDoor", new Vector3(0, 2.5f, 11.4f), new Vector3(6, 5, .25f), darkWood, false);
            for (var i = -3; i <= 3; i++) { CreateBox("Fence", new Vector3(i * 2.2f, .8f, 2), new Vector3(.18f, 1.6f, 8), rust, false); CreateBox("FenceRail", new Vector3(0, 1.15f, 2 + i), new Vector3(8, .18f, .18f), rust, false); }
            CreateBox("ServiceTunnel", new Vector3(0, 1.2f, 31), new Vector3(5, 2.4f, 7), darkWood, false); CreateHayBales();
        }
        private void BuildMine()
        {
            CreateBox("MineFloor", new Vector3(0, -.05f, 14), new Vector3(24, .15f, 38), new Material(ground) { color = new Color(.12f,.10f,.09f) }, false);
            for (var z = 0; z < 38; z += 5) { CreateBox("MineWallL", new Vector3(-9, 2.2f, z + 1), new Vector3(1.2f, 4.4f, 4.8f), darkWood, false); CreateBox("MineWallR", new Vector3(9, 2.2f, z + 1), new Vector3(1.2f, 4.4f, 4.8f), darkWood, false); CreateBox("Support", new Vector3(0, 4.1f, z + 1), new Vector3(18, .4f, .5f), wood, false); }
            CreateBox("VentilationShaft", new Vector3(0, 1.5f, 33), new Vector3(6, 3, 4), rust, false); CreateRockClusters();
        }
        private void BuildFinal()
        {
            CreateBox("ChamberFloor", new Vector3(0, -.08f, 15), new Vector3(28, .2f, 34), new Material(ground) { color = new Color(.06f,.08f,.09f) }, false); CreateBox("FloodedPit", new Vector3(0, .04f, 24), new Vector3(12, .06f, 11), new Material(moon) { color = new Color(.08f,.18f,.22f) }, false);
            for (var i = -2; i <= 2; i++) { var pillar = CreateBox("ChamberPillar", new Vector3(i * 5, 3.2f, 13), new Vector3(1.2f, 6.4f, 1.2f), brick, false); pillar.transform.Rotate(0, i * 7f, i * 3f); }
            CreateBox("BellPlatform", new Vector3(0, 1.2f, 31), new Vector3(8, 2.4f, 4), rust, false); CreateRitualMarks();
        }
        private void CreateTownDetails() { for (var i = -4; i <= 4; i++) { CreateBox("Post", new Vector3(i * 3.5f, 1.2f, 3), new Vector3(.22f, 2.4f, .22f), wood, false); CreateBox("PostRail", new Vector3(i * 3.5f, 1.8f, 3), new Vector3(2.2f, .15f, .15f), wood, false); } CreateCactus(new Vector3(-14, 1, 5)); CreateCactus(new Vector3(13, 1, 15)); }
        private void CreateHayBales() { for (var i = -2; i <= 2; i++) CreateBox("Hay", new Vector3(i * 2.4f, .6f, 18 + (i % 2)), new Vector3(1.8f, 1.2f, 1.4f), new Material(gold) { color = new Color(.48f,.32f,.12f) }, false); }
        private void CreateRockClusters() { for (var i = -3; i <= 3; i++) { var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere); rock.name = "MineRock"; rock.transform.SetParent(root); rock.transform.localPosition = new Vector3(i * 2.5f, .55f, 7 + (i % 2) * 4); rock.transform.localScale = new Vector3(2.4f, 1.1f, 1.8f); rock.GetComponent<Renderer>().material = darkWood; spawned.Add(rock); } }
        private void CreateRitualMarks() { for (var i = 0; i < 6; i++) { var mark = CreateBox("WetMark", new Vector3(Mathf.Sin(i) * 4, .04f, 18 + Mathf.Cos(i) * 4), new Vector3(.15f, .02f, 1.2f), blood, false); mark.transform.Rotate(0, i * 31f, 0); } }
        private void CreateCactus(Vector3 position) { var cactus = GameObject.CreatePrimitive(PrimitiveType.Capsule); cactus.name = "Cactus"; cactus.transform.SetParent(root); cactus.transform.position = position; cactus.transform.localScale = new Vector3(.35f, 1.4f, .35f); cactus.GetComponent<Renderer>().material = new Material(gold) { color = new Color(.18f,.27f,.12f) }; spawned.Add(cactus); }
        private GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material, bool collision) { var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name; box.transform.SetParent(root); box.transform.localPosition = position; box.transform.localScale = scale; box.GetComponent<Renderer>().material = material; if (!collision) Object.Destroy(box.GetComponent<BoxCollider>()); spawned.Add(box); return box; }
        private void CreateTarget(string step, string eventId, string title, string text, string item)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube); target.name = "Investigation_" + step; target.transform.SetParent(root); target.transform.localPosition = new Vector3(0, .55f, 4.5f); target.transform.localScale = new Vector3(.5f, .5f, .5f); target.GetComponent<Renderer>().material = new Material(gold) { color = new Color(.72f,.48f,.18f) }; var light = target.AddComponent<Light>(); light.type = LightType.Point; light.range = 3.3f; light.intensity = .7f; light.color = new Color(1f,.55f,.22f); var interactable = target.AddComponent<TrailInteractable>(); interactable.eventId = eventId; interactable.title = title; interactable.inspectionText = text; interactable.itemId = item; interactable.kind = item == null ? InteractionKind.Inspect : InteractionKind.Collect; interactable.prompt = item == null ? "Examinar" : "Pegar"; spawned.Add(target);
        }
        private void CreateInfected(Vector3 position) { var infected = GameObject.CreatePrimitive(PrimitiveType.Capsule); infected.name = "Infected"; infected.transform.SetParent(root); infected.transform.localPosition = position; infected.transform.localScale = new Vector3(.55f, 1.1f, .55f); infected.GetComponent<Renderer>().material = new Material(brick) { color = new Color(.18f,.06f,.05f) }; infected.AddComponent<InfectedAI>(); spawned.Add(infected); }
        private void CreateChoiceTarget() { var target = GameObject.CreatePrimitive(PrimitiveType.Cube); target.name = "FinalChoice"; target.transform.SetParent(root); target.transform.localPosition = new Vector3(0, .6f, 31); target.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); target.GetComponent<Renderer>().material = new Material(gold) { color = new Color(.85f,.66f,.18f) }; var interactable = target.AddComponent<TrailInteractable>(); interactable.kind = InteractionKind.Choice; interactable.prompt = "Decidir"; interactable.title = "A decisão"; interactable.inspectionText = "A Campainha está pronta. O destino de Layla depende de uma escolha."; spawned.Add(target); }
        private string TargetTitle(string step) => step switch { "priest" => "PADRE ELIAS", "station_hale" => "XERIFE HALE", "barn_layla" or "mine_reunion" => "LAYLA", "final_chamber" => "A CÂMARA", _ => "PISTA" };
        private string TargetText(string step) => step switch { "arrival" => "O homem ferido entrega o lampião. O cavalo se recusa a atravessar o portão.", "priest" => "Elias confirma que Layla foi levada com os feridos. O distintivo dele abre a próxima conversa.", "station_hale" => "Hale está vivo e trancou a si mesmo para não ferir ninguém.", "barn_layla" => "Layla está viva. A voz que chama do corredor usa as lembranças dela.", "mine_bell" => "A Campainha de Ventilação interrompe a influência do Imitador, mas acorda a mina.", "final_chamber" => "A água escura se move no fundo da câmara. Layla pede que você decida.", _ => "O cowboy observa os vestígios em silêncio." };
        private string TargetItem(string step) => step switch { "arrival" => "lantern", "priest" => "deputy_badge", "station_ledger" => "red_ledger", "station_key" => "barn_key", "barn_map" => "ventilation_map", "mine_bell" => "ventilation_bell", "knife" => "knife", _ => null };
        private void CreateMaterials() { ground = Material("Ground", new Color(.20f,.14f,.10f)); wood = Material("Wood", new Color(.23f,.12f,.055f)); darkWood = Material("DarkWood", new Color(.08f,.045f,.025f)); brick = Material("Brick", new Color(.22f,.075f,.045f)); rust = Material("Rust", new Color(.28f,.13f,.07f)); moon = Material("Moon", new Color(.18f,.24f,.28f)); blood = Material("Blood", new Color(.18f,.015f,.01f)); gold = Material("Gold", new Color(.55f,.34f,.12f)); }
        private Material Material(string name, Color color) { var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); var material = new Material(shader) { name = name, color = color }; material.SetFloat("_Smoothness", .12f); return material; }
        private void ConfigureAtmosphere(TrailAct act) { RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 18f; RenderSettings.fogEndDistance = act == TrailAct.Arrival ? 58f : 42f; RenderSettings.fogColor = new Color(.025f,.018f,.016f); RenderSettings.ambientLight = new Color(.075f,.065f,.08f); RenderSettings.skybox = null; }
        private void Clear() { if (root != null) Object.Destroy(root.gameObject); foreach (var item in spawned) if (item != null) Object.Destroy(item); spawned.Clear(); }
    }
}
