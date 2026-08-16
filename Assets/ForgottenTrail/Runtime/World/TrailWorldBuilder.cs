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
        private Material ground, road, wood, darkWood, brick, rust, moon, blood, gold, foliage, trunk, stone;
        private Light moonlight;

        public void Build(TrailAct act, string step)
        {
            Clear(); ConfigureAtmosphere(act); CreateMaterials(); root = new GameObject("AshCreek_Act_" + (int)act).transform;
            SpawnPoint = act switch { TrailAct.Arrival => new Vector3(0, 0.05f, -8), TrailAct.Barn => new Vector3(0, 0.05f, -7), TrailAct.Mine => new Vector3(0, 0.05f, -6), _ => new Vector3(0, 0.05f, -5) };
            CreateBox("Ground", new Vector3(0, -0.18f, 14), new Vector3(44, .3f, 72), ground, true);
            if (act == TrailAct.Arrival)
            {
                SpawnPoint = TrailArrivalLayout.SpawnFor(step, SpawnPoint);
                new TrailArrivalBuilder(root, ground, road, wood, darkWood, brick, rust, gold, foliage, trunk, stone, blood).Build(step);
            }
            else switch (act) { case TrailAct.Barn: BuildBarn(); break; case TrailAct.Mine: BuildMine(); break; case TrailAct.Final: BuildFinal(); break; }
            if (events.TryGetValue(step, out var eventId)) CreateTarget(step, eventId, TargetTitle(step), TargetText(step), TargetItem(step));
            else if (step == "final_choice") CreateChoiceTarget();
            if (act is TrailAct.Barn or TrailAct.Mine) { CreateInfected(new Vector3(4, .75f, 12)); CreateInfected(new Vector3(-5, .75f, 22)); }
        }

        private void BuildArrival()
        {
            CreateBox("Saloon", new Vector3(-8, 2.3f, 12), new Vector3(9, 4.6f, 8), wood, false); CreateBox("Church", new Vector3(8, 2.8f, 25), new Vector3(8, 5.6f, 9), brick, false); CreateBox("Station", new Vector3(0, 2.3f, 34), new Vector3(8, 4.6f, 7), darkWood, false);
            CreateTownDetails();
        }
        private void BuildArrivalGameplayMarkers()
        {
            CreateBox("ArrivalBoundary", new Vector3(0, -0.05f, 31), new Vector3(44, .1f, 1), new Material(ground) { color = new Color(.17f, .09f, .06f) }, false);
        }
        private void BuildArrivalScenery()
        {
            CreateBox("RoadSurface", new Vector3(0, .035f, 13), new Vector3(5.6f, .08f, 42), road, false);
            CreateBox("RoadShoulderLeft", new Vector3(-4.1f, .02f, 13), new Vector3(2.5f, .06f, 42), ground, false);
            CreateBox("RoadShoulderRight", new Vector3(4.1f, .02f, 13), new Vector3(2.5f, .06f, 42), ground, false);
            CreateBox("TerrainBankLeft", new Vector3(-17f, .16f, 13), new Vector3(18f, .32f, 42), ground, false);
            CreateBox("TerrainBankRight", new Vector3(17f, .16f, 13), new Vector3(18f, .32f, 42), ground, false);

            var trees = new[]
            {
                new Vector3(-13f, .02f, 4f), new Vector3(13f, .02f, 7f), new Vector3(-15f, .02f, 18f),
                new Vector3(14f, .02f, 21f), new Vector3(-13f, .02f, 29f), new Vector3(14f, .02f, 31f)
            };
            for (var i = 0; i < trees.Length; i++) CreateTree("MesquiteTree_" + i, trees[i], 1f + (i % 3) * .12f);

            var shrubs = new[]
            {
                new Vector3(-8.5f, .02f, 5f), new Vector3(9.2f, .02f, 11f), new Vector3(-11f, .02f, 23f),
                new Vector3(10.7f, .02f, 28f), new Vector3(-6.5f, .02f, 34f), new Vector3(7.8f, .02f, 36f)
            };
            for (var i = 0; i < shrubs.Length; i++) CreateShrub("DustShrub_" + i, shrubs[i], .8f + (i % 2) * .2f);

            for (var i = 0; i < 18; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var x = side * (6.5f + (i % 4) * 1.7f);
                var z = 1.5f + i * 2.05f;
                CreateGrassTuft("DryGrass_" + i, new Vector3(x, .02f, z), .75f + (i % 3) * .18f);
            }

            CreatePracticalLight("SaloonPorchLight", new Vector3(-4.1f, 3.1f, 9.1f), new Color(1f, .34f, .12f), 5.5f, 1.35f);
            CreatePracticalLight("ChurchLanternLight", new Vector3(4.4f, 3.5f, 22.3f), new Color(1f, .48f, .18f), 5f, 1.15f);
            CreatePracticalLight("StationLanternLight", new Vector3(-2.9f, 3.1f, 31.2f), new Color(1f, .38f, .12f), 4.5f, 1.1f);
        }
        private void CreateTree(string name, Vector3 position, float scale)
        {
            var tree = new GameObject(name);
            tree.transform.SetParent(root);
            tree.transform.localPosition = position;
            tree.transform.localScale = Vector3.one * scale;

            var lod0Root = new GameObject("LOD0").transform;
            lod0Root.SetParent(tree.transform);
            var lod0 = new List<Renderer>
            {
                CreateSceneryPart("Trunk", PrimitiveType.Cylinder, lod0Root, new Vector3(0, 2f, 0), new Vector3(.34f, 2f, .34f), trunk).GetComponent<Renderer>(),
                CreateSceneryPart("CanopyMain", PrimitiveType.Capsule, lod0Root, new Vector3(0, 4.1f, 0), new Vector3(2.2f, .9f, 2.2f), foliage).GetComponent<Renderer>(),
                CreateSceneryPart("CanopyLeft", PrimitiveType.Capsule, lod0Root, new Vector3(-.9f, 3.55f, .2f), new Vector3(1.45f, .65f, 1.45f), foliage).GetComponent<Renderer>(),
                CreateSceneryPart("CanopyRight", PrimitiveType.Capsule, lod0Root, new Vector3(.85f, 3.6f, -.15f), new Vector3(1.35f, .62f, 1.35f), foliage).GetComponent<Renderer>()
            };

            var lod1Root = new GameObject("LOD1").transform;
            lod1Root.SetParent(tree.transform);
            var lod1 = new List<Renderer>
            {
                CreateSceneryPart("Trunk", PrimitiveType.Cylinder, lod1Root, new Vector3(0, 1.65f, 0), new Vector3(.45f, 1.65f, .45f), trunk).GetComponent<Renderer>(),
                CreateSceneryPart("Canopy", PrimitiveType.Capsule, lod1Root, new Vector3(0, 3.6f, 0), new Vector3(2.25f, 1.2f, 2.25f), foliage).GetComponent<Renderer>()
            };

            var group = tree.AddComponent<LODGroup>();
            group.SetLODs(new[] { new LOD(.55f, lod0.ToArray()), new LOD(.16f, lod1.ToArray()) });
            group.RecalculateBounds();
        }
        private void CreateShrub(string name, Vector3 position, float scale)
        {
            var shrub = new GameObject(name);
            shrub.transform.SetParent(root);
            shrub.transform.localPosition = position;
            shrub.transform.localScale = Vector3.one * scale;
            CreateSceneryPart("LeafClusterA", PrimitiveType.Capsule, shrub.transform, new Vector3(-.45f, .45f, 0), new Vector3(.85f, .45f, .85f), foliage);
            CreateSceneryPart("LeafClusterB", PrimitiveType.Capsule, shrub.transform, new Vector3(.4f, .55f, .1f), new Vector3(.95f, .55f, .95f), foliage);
            CreateSceneryPart("LeafClusterC", PrimitiveType.Capsule, shrub.transform, new Vector3(0, .72f, -.35f), new Vector3(.7f, .4f, .7f), foliage);
        }
        private void CreateGrassTuft(string name, Vector3 position, float scale)
        {
            var tuft = new GameObject(name);
            tuft.transform.SetParent(root);
            tuft.transform.localPosition = position;
            tuft.transform.localScale = Vector3.one * scale;
            var bladeA = CreateSceneryPart("BladeA", PrimitiveType.Cube, tuft.transform, new Vector3(-.12f, .3f, 0), new Vector3(.08f, .6f, .08f), foliage);
            var bladeB = CreateSceneryPart("BladeB", PrimitiveType.Cube, tuft.transform, new Vector3(.12f, .24f, .02f), new Vector3(.08f, .48f, .08f), foliage);
            bladeA.transform.localRotation = Quaternion.Euler(0, 0, -18f);
            bladeB.transform.localRotation = Quaternion.Euler(0, 0, 16f);
        }
        private void CreatePracticalLight(string name, Vector3 position, Color color, float range, float intensity)
        {
            CreateSceneryPart(name + "_Lantern", PrimitiveType.Sphere, root, position, Vector3.one * .14f, gold);
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(root);
            lightObject.transform.localPosition = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }
        private GameObject CreateSceneryPart(string name, PrimitiveType primitive, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            var collider = part.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            return part;
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
        private void CreateRockClusters() { for (var i = -3; i <= 3; i++) { var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere); rock.name = "MineRock"; rock.transform.SetParent(root); rock.transform.localPosition = new Vector3(i * 2.5f, .55f, 7 + (i % 2) * 4); rock.transform.localScale = new Vector3(2.4f, 1.1f, 1.8f); rock.GetComponent<Renderer>().material = stone; spawned.Add(rock); } }
        private void CreateRitualMarks() { for (var i = 0; i < 6; i++) { var mark = CreateBox("WetMark", new Vector3(Mathf.Sin(i) * 4, .04f, 18 + Mathf.Cos(i) * 4), new Vector3(.15f, .02f, 1.2f), blood, false); mark.transform.Rotate(0, i * 31f, 0); } }
        private void CreateCactus(Vector3 position) { var cactus = GameObject.CreatePrimitive(PrimitiveType.Capsule); cactus.name = "Cactus"; cactus.transform.SetParent(root); cactus.transform.position = position; cactus.transform.localScale = new Vector3(.35f, 1.4f, .35f); cactus.GetComponent<Renderer>().material = new Material(gold) { color = new Color(.18f,.27f,.12f) }; spawned.Add(cactus); }
        private GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material, bool collision) { var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name; box.transform.SetParent(root); box.transform.localPosition = position; box.transform.localScale = scale; box.GetComponent<Renderer>().material = material; if (!collision) Object.Destroy(box.GetComponent<BoxCollider>()); spawned.Add(box); return box; }
        private void CreateTarget(string step, string eventId, string title, string text, string item)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube); target.name = "Investigation_" + step; target.transform.SetParent(root); target.transform.localPosition = TrailArrivalLayout.TargetFor(step, new Vector3(0, .55f, 4.5f)); target.transform.localScale = new Vector3(.5f, .5f, .5f); target.GetComponent<Renderer>().enabled = false; var light = target.AddComponent<Light>(); light.type = LightType.Point; light.range = 1.2f; light.intensity = .12f; light.color = new Color(1f,.55f,.22f); light.enabled = false; var interactable = target.AddComponent<TrailInteractable>(); interactable.eventId = eventId; interactable.title = title; interactable.inspectionText = text; interactable.itemId = item; interactable.kind = item == null ? InteractionKind.Inspect : InteractionKind.Collect; interactable.prompt = item == null ? "Examinar" : "Pegar"; spawned.Add(target);
        }
        private void CreateInfected(Vector3 position) { var infected = GameObject.CreatePrimitive(PrimitiveType.Capsule); infected.name = "Infected"; infected.transform.SetParent(root); infected.transform.localPosition = position; infected.transform.localScale = new Vector3(.55f, 1.1f, .55f); infected.GetComponent<Renderer>().material = new Material(brick) { color = new Color(.18f,.06f,.05f) }; infected.AddComponent<InfectedAI>(); spawned.Add(infected); }
        private void CreateChoiceTarget() { var target = GameObject.CreatePrimitive(PrimitiveType.Cube); target.name = "FinalChoice"; target.transform.SetParent(root); target.transform.localPosition = new Vector3(0, .6f, 31); target.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); target.GetComponent<Renderer>().material = new Material(gold) { color = new Color(.85f,.66f,.18f) }; var interactable = target.AddComponent<TrailInteractable>(); interactable.kind = InteractionKind.Choice; interactable.prompt = "Decidir"; interactable.title = "A decisão"; interactable.inspectionText = "A Campainha está pronta. O destino de Layla depende de uma escolha."; spawned.Add(target); }
        private string TargetTitle(string step) => step switch { "arrival" => "O HOMEM FERIDO", "priest" => "PADRE ELIAS", "station_hale" => "XERIFE HALE", "station_ledger" => "REGISTRO VERMELHO", "barn_layla" or "mine_reunion" => "LAYLA", "final_chamber" => "A CÂMARA", _ => "PISTA" };
        private string TargetText(string step) => step switch
        {
            "arrival" => "Encontrei um homem ferido na entrada de Ash Creek. Ele me entregou seu lampião e disse que alguma coisa na cidade escuta tudo. Não sei o que aconteceu aqui, mas não posso voltar sem descobrir se Layla ainda está viva.",
            "footprints" => "Pegadas de garimpeiros. Algumas são recentes. Eles foram em direção à rua principal.",
            "threshold" => "As pegadas terminam diante do saloon. A porta dupla está entreaberta e há sinais de movimentação lá dentro.",
            "enter_saloon" => "O salão está abandonado, mas a cozinha e o andar superior ainda guardam sinais de quem saiu às pressas.",
            "meal" => "Isso vem do andar de cima. Parece alguém gemendo… mas não soa humano.",
            "broken_door" => "A passagem está destruída. Ainda consigo subir, mas não sei o que está me esperando lá em cima.",
            "diary" => "As pessoas do saloon sabiam que alguma coisa estava observando a construção. Elas evitavam fazer barulho, principalmente durante a noite.",
            "message" => "Não façam barulho. Eles não enxergam como nós, mas escutam tudo. Os sobreviventes foram levados para o celeiro.",
            "window" => "Há marcas do lado de fora. Aquilo estava tentando olhar para dentro.",
            "downstairs_noise" => "Alguma coisa quebrou no andar de baixo.",
            "knife" => "Não vai me proteger de tudo, mas é melhor do que estar desarmado.",
            "exit_saloon" => "O saloon estava vazio, mas eu não estava sozinho. Algo me observou pela janela e outra coisa passou pelo andar de baixo.",
            "church_approach" => "Estas marcas saem da igreja. Não há nenhuma seguindo para dentro.",
            "enter_church" => "A igreja está silenciosa, iluminada por velas. Atrás do altar, um homem ferido segura um castiçal.",
            "church_interior" => "Velas acesas, fotografias de desaparecidos e uma Bíblia aberta. Há murmúrios vindo das paredes.",
            "priest" => "Os garimpeiros trouxeram alguma coisa da mina. Primeiro vieram as febres. Depois, as vozes. Layla ajudou a cuidar dos doentes e foi levada para o celeiro.",
            "station" => "A mina e o celeiro. Alguém marcou os dois lugares no mapa de Ash Creek.",
            "station_ledger" => "LAYLA — transferida para o celeiro. Condição: consciente. Possível exposição. Pediu acesso aos registros da mina.",
            "station_hale" => "Hale está vivo, febril e paranoico. Ele afirma que o padre Elias morreu há três dias.",
            "station_key" => "Eu tranquei o celeiro para impedir que alguma coisa saísse. Não para impedir que entrassem.",
            "leave_station" => "A criatura imita vozes. Se eu ouvir alguém conhecido, não posso confiar no som.",
            "return_church" => "O padre Elias estava morto antes de eu chegar a Ash Creek. Ainda assim, conversei com ele.",
            "barn_layla" => "Layla está viva. A voz que chama do corredor usa as lembranças dela.",
            "mine_bell" => "A Campainha de Ventilação interrompe a influência do Imitador, mas acorda a mina.",
            "final_chamber" => "A água escura se move no fundo da câmara. Layla pede que você decida.",
            _ => "O cowboy observa os vestígios em silêncio."
        };
        private string TargetItem(string step) => step switch { "arrival" => "lantern", "priest" => "deputy_badge", "station_ledger" => "red_ledger", "station_key" => "barn_key", "barn_map" => "ventilation_map", "mine_bell" => "ventilation_bell", "knife" => "knife", _ => null };
        private void CreateMaterials()
        {
            ground = Material("Ground", new Color(.42f, .25f, .12f), "Art/Textures/Main File V1_1_Ground076L_512x512_Color_Orange", new Vector2(6f, 6f));
            road = Material("Road", new Color(.21f, .12f, .07f), "Art/Textures/Main File V1_1_Ground076L_512x512_Color_Orange", new Vector2(2f, 18f));
            wood = Material("Wood", new Color(.23f,.12f,.055f), "Art/Textures/Main File V1_1_Planks023A_512x512_Color", new Vector2(2f, 2f));
            darkWood = Material("DarkWood", new Color(.08f,.045f,.025f), "Art/Textures/Main File V1_1_Planks023A_512x512_Color_Black", new Vector2(2f, 2f));
            brick = Material("Brick", new Color(.22f,.075f,.045f), "Art/Textures/Main File V1_1_Bricks096_512x512_Color", new Vector2(2f, 2f));
            rust = Material("Rust", new Color(.28f,.13f,.07f), "Art/Textures/Main File V1_1_Metal041C_512x512_Color", new Vector2(2f, 2f));
            moon = Material("Moon", new Color(.18f,.24f,.28f));
            blood = Material("Blood", new Color(.18f,.015f,.01f));
            gold = Material("Gold", new Color(.55f,.34f,.12f));
            foliage = Material("Foliage", new Color(.12f,.18f,.08f), "Art/Textures/Main File V1_1_Cactus_512x512_Color", Vector2.one);
            trunk = Material("TreeTrunk", new Color(.16f,.075f,.035f), "Art/Textures/Main File V1_1_Planks023A_512x512_Color", Vector2.one);
            stone = Material("Stone", new Color(.18f,.13f,.10f), "Art/Textures/Main File V1_1_PavingStones115B_512x512_Color", Vector2.one);
        }
        private Material Material(string name, Color color) => Material(name, color, null, Vector2.one);
        private Material Material(string name, Color color, string textureResource, Vector2 textureScale)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = name };
            var colorProperty = material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            if (material.HasProperty(colorProperty)) material.SetColor(colorProperty, color);
            if (!string.IsNullOrEmpty(textureResource))
            {
                var texture = Resources.Load<Texture2D>(textureResource);
                var textureProperty = material.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
                if (texture != null && material.HasProperty(textureProperty))
                {
                    material.SetTexture(textureProperty, texture);
                    material.SetTextureScale(textureProperty, textureScale);
                }
            }
            var normalResource = textureResource?.Replace("_Color_Orange", "_NormalGL").Replace("_Color", "_NormalGL");
            var normal = string.IsNullOrEmpty(normalResource) ? null : Resources.Load<Texture2D>(normalResource);
            if (normal != null && material.HasProperty("_BumpMap"))
            {
                material.SetTexture("_BumpMap", normal);
                if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", .55f);
            }
            var roughResource = textureResource?.Replace("_Color_Orange", "_Roughness").Replace("_Color", "_Metalness");
            var roughness = string.IsNullOrEmpty(roughResource) ? null : Resources.Load<Texture2D>(roughResource);
            if (roughness != null && material.HasProperty("_MetallicGlossMap")) material.SetTexture("_MetallicGlossMap", roughness);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .12f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", .12f);
            return material;
        }
        private void ConfigureAtmosphere(TrailAct act)
        {
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 18f; RenderSettings.fogEndDistance = act == TrailAct.Arrival ? 58f : 42f; RenderSettings.fogColor = new Color(.025f,.018f,.016f); RenderSettings.ambientLight = new Color(.46f,.32f,.25f); RenderSettings.skybox = null;
            if (moonlight == null)
            {
                var lightObject = new GameObject("AshCreek_Moonlight"); lightObject.transform.SetParent(transform); moonlight = lightObject.AddComponent<Light>();
            }
            moonlight.type = LightType.Directional; moonlight.color = new Color(.62f,.68f,.82f); moonlight.intensity = 1.35f; moonlight.shadowStrength = .62f; moonlight.shadows = LightShadows.Soft; moonlight.transform.rotation = Quaternion.Euler(48f,40f,0f); moonlight.enabled = true;
        }
        private void Clear() { if (root != null) Object.Destroy(root.gameObject); foreach (var item in spawned) if (item != null) Object.Destroy(item); spawned.Clear(); }
    }
}
