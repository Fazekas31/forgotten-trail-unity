using UnityEngine;

namespace ForgottenTrail
{
    /// <summary>
    /// Builds the authored spatial language of Act I. TrailWorldBuilder owns the
    /// campaign lifecycle; this module owns the physical town and its story props.
    /// </summary>
    public sealed class TrailArrivalBuilder
    {
        private readonly Transform root;
        private readonly Material ground;
        private readonly Material road;
        private readonly Material wood;
        private readonly Material darkWood;
        private readonly Material brick;
        private readonly Material rust;
        private readonly Material gold;
        private readonly Material foliage;
        private readonly Material trunk;
        private readonly Material stone;
        private readonly Material blood;

        private Material trim;
        private Material roof;
        private Material plaster;
        private Material glass;
        private Material paper;
        private Material cloth;
        private Material leather;
        private Material candle;
        private Material black;
        private Material assetStoreRoad;

        public TrailArrivalBuilder(Transform root, Material ground, Material road, Material wood, Material darkWood,
            Material brick, Material rust, Material gold, Material foliage, Material trunk, Material stone, Material blood)
        {
            this.root = root;
            this.ground = ground;
            this.road = road;
            this.wood = wood;
            this.darkWood = darkWood;
            this.brick = brick;
            this.rust = rust;
            this.gold = gold;
            this.foliage = foliage;
            this.trunk = trunk;
            this.stone = stone;
            this.blood = blood;
            trim = Tint(wood, "ArchitecturalTrim", new Color(.38f, .19f, .08f));
            roof = Tint(darkWood, "TarredRoof", new Color(.07f, .055f, .045f));
            plaster = Tint(brick, "SunbakedPlaster", new Color(.42f, .24f, .14f));
            glass = Tint(darkWood, "SmokyGlass", new Color(.035f, .07f, .075f));
            paper = Tint(ground, "AgedPaper", new Color(.64f, .48f, .28f));
            cloth = Tint(wood, "FadedCloth", new Color(.20f, .24f, .22f));
            leather = Tint(darkWood, "Leather", new Color(.16f, .055f, .025f));
            candle = Tint(gold, "CandleWax", new Color(.85f, .62f, .32f));
            black = Tint(darkWood, "Silhouette", new Color(.006f, .004f, .004f));
            assetStoreRoad = CreateAssetStoreRoadMaterial();
            if (glass.HasProperty("_EmissionColor"))
            {
                glass.EnableKeyword("_EMISSION");
                glass.SetColor("_EmissionColor", new Color(.18f, .035f, .008f));
            }
        }

        public void Build(string step)
        {
            BuildAssetStoreTerrainBackdrop();
            BuildArrivalRoad();
            BuildGateAndApproach();
            BuildBackdropBuildings();
            BuildStreetFurniture();
            BuildSaloon(step);
            BuildChurch(step);
            BuildStation(step);
            HideProceduralArchitectureMeshes();
            BuildImportedArchitecture();
            BuildImportedTrees();
            BuildHorseAndWoundedMan();
            BuildAtmosphericProps(step);
        }

        private void BuildImportedArchitecture()
        {
            var prefab = Resources.Load<GameObject>("Environment/AshCreek_Architecture");
            if (prefab == null)
            {
                Debug.LogWarning("Ash Creek architecture GLB was not imported. Falling back to the procedural shells.");
                return;
            }

            var architecture = Object.Instantiate(prefab, root);
            architecture.name = "AshCreek_RealisticArchitecture_Blender";
            architecture.transform.localPosition = Vector3.zero;
            // The authored Blender file uses Y as its construction height and
            // the GLB importer stores that authored axis as the local Z axis.
            // Keep the root conversion so positions land on the Ash Creek
            // street, then compensate each direct mesh node so it stays upright.
            architecture.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            architecture.transform.localScale = new Vector3(-1f, 1f, 1f);

            // The old showcase pivots made the backdrop read as leaning
            // buildings from first person. They are still useful for their
            // authored positions, but all backdrop pivots are street-aligned.
            foreach (Transform child in architecture.transform)
            {
                child.localRotation = child.name.EndsWith("_Pivot")
                    ? Quaternion.identity
                    : Quaternion.Euler(-90f, 0f, 0f) * child.localRotation;
            }
        }

        private void BuildImportedTrees()
        {
            var treePrefabs = new[]
            {
                Resources.Load<GameObject>("Environment/AssetStoreTrees/Pine_A"),
                Resources.Load<GameObject>("Environment/AssetStoreTrees/Pine_B"),
                Resources.Load<GameObject>("Environment/AssetStoreTrees/Cypress_Forest_Desktop")
            };
            var positions = new[]
            {
                new Vector3(-22f, 0f, -8f), new Vector3(22f, 0f, -6f),
                new Vector3(-22f, 0f, 6f), new Vector3(22f, 0f, 10f),
                new Vector3(-23f, 0f, 23f), new Vector3(23f, 0f, 27f),
                new Vector3(-18f, 0f, 45f), new Vector3(19f, 0f, 46f),
                new Vector3(-29f, 0f, 16f), new Vector3(29f, 0f, 20f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var prefab = treePrefabs[i % treePrefabs.Length];
                if (prefab == null) continue;
                var tree = Object.Instantiate(prefab, root);
                tree.name = "AshCreek_AssetStoreTree_" + i;
                tree.transform.localPosition = positions[i];
                tree.transform.localRotation = Quaternion.Euler(0f, (i * 37f) % 360f, 0f);
                tree.transform.localScale = Vector3.one * (i % 3 == 0 ? 1.15f : .92f);
                foreach (var speedTree in tree.GetComponentsInChildren<Tree>(true))
                    Object.Destroy(speedTree);
                RepairImportedTreeMaterials(tree);
            }
        }

        private void RepairImportedTreeMaterials(GameObject tree)
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;

            foreach (var renderer in tree.GetComponentsInChildren<Renderer>(true))
            {
                var sourceMaterials = renderer.sharedMaterials;
                var repairedMaterials = new Material[sourceMaterials.Length];
                for (var i = 0; i < sourceMaterials.Length; i++)
                {
                    var source = sourceMaterials[i];
                    var repaired = new Material(shader)
                    {
                        name = "AshCreek_AssetStoreTree_" + (source != null ? source.name : "Material")
                    };
                    if (source != null)
                    {
                        var albedo = source.GetTexture("_BaseMap") ?? source.GetTexture("_MainTex");
                        var textureProperty = repaired.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
                        if (albedo != null && repaired.HasProperty(textureProperty))
                        {
                            repaired.SetTexture(textureProperty, albedo);
                            repaired.SetTextureScale(textureProperty, source.GetTextureScale(source.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex"));
                        }
                        if (source.HasProperty("_BumpMap") && repaired.HasProperty("_BumpMap"))
                        {
                            repaired.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
                            repaired.EnableKeyword("_NORMALMAP");
                        }
                        var sourceColorProperty = source.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                        var repairedColorProperty = repaired.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                        if (source.HasProperty(sourceColorProperty) && repaired.HasProperty(repairedColorProperty))
                            repaired.SetColor(repairedColorProperty, source.GetColor(sourceColorProperty));

                        var alphaClip = source.HasProperty("_AlphaClip") && source.GetFloat("_AlphaClip") > .5f;
                        if (source.name.Contains("Billboard")) alphaClip = true;
                        if (alphaClip)
                        {
                            if (repaired.HasProperty("_Mode")) repaired.SetFloat("_Mode", 1f);
                            if (repaired.HasProperty("_Surface")) repaired.SetFloat("_Surface", 0f);
                            if (repaired.HasProperty("_AlphaClip")) repaired.SetFloat("_AlphaClip", 1f);
                            if (repaired.HasProperty("_Cutoff")) repaired.SetFloat("_Cutoff", .30f);
                            if (repaired.HasProperty("_Cutoff")) repaired.SetFloat("_AlphaClipThreshold", .30f);
                            repaired.EnableKeyword("_ALPHATEST_ON");
                            repaired.renderQueue = 2450;
                        }
                    }
                    repairedMaterials[i] = repaired;
                }
                renderer.sharedMaterials = repairedMaterials;
            }
        }

        private void BuildAssetStoreTerrainBackdrop()
        {
            var prefab = Resources.Load<GameObject>("Environment/AssetStoreTerrainHigh");
            if (prefab == null)
            {
                Debug.LogWarning("Asset Store terrain prefab is not imported. Using the authored town ground only.");
                return;
            }

            var terrainRoot = Object.Instantiate(prefab, root);
            terrainRoot.name = "AshCreek_AssetStoreTerrainBackdrop";
            const float terrainScale = .05f;
            terrainRoot.transform.localPosition = new Vector3(-75f, -20f, -50f);
            terrainRoot.transform.localScale = Vector3.one * terrainScale;

            // The imported demo is a 4x4 kilometre landscape. At 5% scale it
            // becomes a 200m backdrop. Keep the six tiles touching the playable
            // town empty so the authored roads, buildings and interiors remain
            // grounded and readable.
            foreach (var terrain in terrainRoot.GetComponentsInChildren<Terrain>(true))
            {
                var parts = terrain.name.Split('_');
                if (parts.Length < 3 || !int.TryParse(parts[1], out var tileX) || !int.TryParse(parts[2], out var tileZ))
                    continue;

                var isTownCutout = (tileX is 1 or 2) && (tileZ is 0 or 1 or 2);
                terrain.gameObject.SetActive(!isTownCutout);
                // The package's SpeedTree materials target a different render
                // pipeline and turn magenta in this URP scene. We place the
                // same package tree meshes explicitly below after repairing
                // their materials, while keeping the terrain relief/rocks.
                terrain.drawTreesAndFoliage = false;
            }
        }

        private void HideProceduralArchitectureMeshes()
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name.StartsWith("ARCH_")) continue;
                if (IsProceduralArchitecturePart(renderer.gameObject.name)) renderer.enabled = false;
            }
        }

        private static bool IsProceduralArchitecturePart(string name)
        {
            return name.StartsWith("BoardingHouse") || name.StartsWith("Mercantile") ||
                name.StartsWith("Blacksmith") || name.StartsWith("DoctorHouse") ||
                name.StartsWith("NorthCabin") || name.StartsWith("EastCabin") ||
                name.StartsWith("SaloonGroundFloor") || name.StartsWith("SaloonUpperFloor") ||
                name.StartsWith("SaloonWall") || name.StartsWith("SaloonFront") ||
                name.StartsWith("SaloonUpper") || name.StartsWith("SaloonRoof") ||
                name.StartsWith("SaloonSign") || name.StartsWith("SaloonTrimFront") ||
                name.StartsWith("SaloonPorch") || name.StartsWith("ChurchFloor") ||
                name.StartsWith("ChurchWall") || name.StartsWith("ChurchFront") ||
                name.StartsWith("ChurchTower") || name.StartsWith("ChurchRoof") ||
                name.StartsWith("StationFloor") || name.StartsWith("StationWall") ||
                name.StartsWith("StationFront") || name.StartsWith("StationLintel") ||
                name.StartsWith("StationUpper") || name.StartsWith("StationRoof") ||
                name.StartsWith("StationSign") || name.StartsWith("Window") || name.StartsWith("Door") ||
                name.StartsWith("Porch");
        }

        private void BuildArrivalRoad()
        {
            var roadObject = new GameObject("RoadSurface_EasyRoadsDirt");
            roadObject.transform.SetParent(root);
            var mesh = new Mesh { name = "AshCreek_EasyRoadsDirtMesh" };
            const int segments = 12;
            var vertices = new Vector3[(segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[segments * 6];
            for (var i = 0; i <= segments; i++)
            {
                var t = i / (float)segments;
                var z = -20f + t * 72f;
                var center = Mathf.Sin(t * 1.35f) * .32f;
                var width = 7.15f + Mathf.Sin(t * 2.1f) * .25f;
                vertices[i * 2] = new Vector3(center - width * .5f, .045f + Mathf.Sin(t * 7f) * .008f, z);
                vertices[i * 2 + 1] = new Vector3(center + width * .5f, .045f + Mathf.Cos(t * 6f) * .008f, z);
                uvs[i * 2] = new Vector2(0f, t * 9f);
                uvs[i * 2 + 1] = new Vector2(1f, t * 9f);
                if (i == segments) continue;
                var baseIndex = i * 6;
                var vertexIndex = i * 2;
                triangles[baseIndex] = vertexIndex;
                triangles[baseIndex + 1] = vertexIndex + 2;
                triangles[baseIndex + 2] = vertexIndex + 1;
                triangles[baseIndex + 3] = vertexIndex + 1;
                triangles[baseIndex + 4] = vertexIndex + 2;
                triangles[baseIndex + 5] = vertexIndex + 3;
            }
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            roadObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            roadObject.AddComponent<MeshRenderer>().sharedMaterial = assetStoreRoad ?? road;
            roadObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            Box("RoadShoulderLeft", new Vector3(-5.9f, .02f, 14f), new Vector3(4.0f, .06f, 72f), ground, false);
            Box("RoadShoulderRight", new Vector3(5.9f, .02f, 14f), new Vector3(4.0f, .06f, 72f), ground, false);
            Box("RuttLeft", new Vector3(-1.25f, .095f, 14f), new Vector3(.16f, .018f, 70f), darkWood, false);
            Box("RuttRight", new Vector3(1.25f, .097f, 14f), new Vector3(.16f, .018f, 70f), darkWood, false);
            for (var i = 0; i < 12; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var z = -20f + i * 5.7f;
                Box("RoadStone", new Vector3(side * (3.8f + (i % 3) * .5f), .09f, z), new Vector3(.18f, .08f, .42f), stone, false)
                    .transform.Rotate(0f, i * 23f, i * 11f);
            }
        }

        private void BuildGateAndApproach()
        {
            var gateZ = -4.5f;
            Box("GatePostLeft", new Vector3(-4.25f, 2.2f, gateZ), new Vector3(.56f, 4.4f, .56f), trim);
            Box("GatePostRight", new Vector3(4.25f, 2.2f, gateZ), new Vector3(.56f, 4.4f, .56f), trim);
            Box("GateBeam", new Vector3(0f, 4.1f, gateZ), new Vector3(9.1f, .48f, .56f), darkWood);
            Box("GateBeamCap", new Vector3(0f, 4.48f, gateZ), new Vector3(7.6f, .13f, .34f), rust, false);
            for (var i = -3; i <= 3; i++)
                Box("GateSlat", new Vector3(i * 1.15f, 2.15f + Mathf.Abs(i) * .12f, gateZ + .02f), new Vector3(.16f, 3.15f, .28f), wood, false)
                    .transform.Rotate(0f, 0f, i * 1.8f);
            Box("AshCreekSign", new Vector3(0f, 3.32f, gateZ - .38f), new Vector3(4.5f, 1.0f, .16f), darkWood, false);
            Text("Ash Creek", new Vector3(0f, 3.30f, gateZ - .49f), 46, new Color(.72f, .53f, .28f), Quaternion.Euler(0f, 180f, 0f));

            Box("EntryMarkerLeft", new Vector3(-6.6f, .65f, -4.2f), new Vector3(.18f, 1.3f, .18f), rust, false);
            Box("EntryMarkerRight", new Vector3(6.6f, .65f, -4.2f), new Vector3(.18f, 1.3f, .18f), rust, false);
            for (var i = 0; i < 9; i++)
            {
                var z = -2.8f + i * 1.05f;
                var x = Mathf.Sin(i * 1.7f) * .32f;
                Box("BootPrint", new Vector3(x, .11f, z), new Vector3(.16f, .018f, .38f), darkWood, false)
                    .transform.Rotate(0f, i % 2 == 0 ? -18f : 18f, 0f);
            }
        }

        private void BuildBackdropBuildings()
        {
            FrontierShell("BoardingHouse", new Vector3(-16f, 2.8f, 7.5f), 7.5f, 6.4f, plaster, roof, true);
            FrontierShell("Mercantile", new Vector3(16f, 2.4f, 11f), 8.2f, 5.7f, brick, roof, false);
            FrontierShell("Blacksmith", new Vector3(-16f, 2.6f, 26f), 8.4f, 6.0f, darkWood, roof, false);
            FrontierShell("DoctorHouse", new Vector3(16f, 2.8f, 29f), 8.0f, 6.3f, plaster, roof, true);
            FrontierShell("NorthCabin", new Vector3(-13.5f, 2.2f, 43f), 7.0f, 5.0f, wood, roof, false);
            FrontierShell("EastCabin", new Vector3(14f, 2.2f, 43f), 7.0f, 5.0f, wood, roof, false);
        }

        private void BuildStreetFurniture()
        {
            BuildWell(new Vector3(7.0f, .05f, 4.0f));
            BuildWagon(new Vector3(4.2f, .04f, 12.0f), 12f);
            BuildWagon(new Vector3(-14.2f, .04f, 20.0f), -20f);
            BuildFence(new Vector3(-17f, .02f, 17f), 8f, 3);
            BuildFence(new Vector3(16f, .02f, 22f), 7f, 3);
            CreateCactus(new Vector3(-18f, .05f, -1f), 1.2f);
            CreateCactus(new Vector3(18f, .05f, 4f), .9f);
            CreateCactus(new Vector3(-18f, .05f, 34f), 1.35f);
        }

        private void BuildSaloon(string step)
        {
            const float left = -13.1f;
            const float right = -2.1f;
            const float front = 7.0f;
            const float back = 16.5f;
            const float width = 11.0f;
            const float depth = 9.5f;
            const float floorHeight = 3.25f;

            Box("SaloonGroundFloor", new Vector3(-7.6f, .06f, 11.75f), new Vector3(width, .12f, depth), wood);
            Box("SaloonUpperFloor", new Vector3(-7.6f, floorHeight, 11.75f), new Vector3(width - .18f, .18f, depth - .18f), darkWood);
            Box("SaloonWallLeft", new Vector3(left, 1.65f, 11.75f), new Vector3(.28f, 3.2f, depth), wood);
            Box("SaloonWallRight", new Vector3(right, 1.65f, 11.75f), new Vector3(.28f, 3.2f, depth), wood);
            Box("SaloonWallBack", new Vector3(-7.6f, 1.65f, back), new Vector3(width, 3.2f, .28f), wood);
            Box("SaloonFrontLeft", new Vector3(-10.4f, 1.65f, front), new Vector3(5.4f, 3.2f, .28f), wood);
            Box("SaloonFrontRight", new Vector3(-3.2f, 1.65f, front), new Vector3(2.2f, 3.2f, .28f), wood);
            Box("SaloonFrontLintel", new Vector3(-6.1f, 2.92f, front), new Vector3(2.4f, .48f, .34f), trim);

            Box("SaloonUpperWallLeft", new Vector3(left, 4.85f, 11.75f), new Vector3(.28f, 3.0f, depth), darkWood);
            Box("SaloonUpperWallRight", new Vector3(right, 4.85f, 11.75f), new Vector3(.28f, 3.0f, depth), darkWood);
            Box("SaloonUpperWallBack", new Vector3(-7.6f, 4.85f, back), new Vector3(width, 3.0f, .28f), darkWood);
            Box("SaloonUpperFrontLeft", new Vector3(-11.1f, 4.85f, front), new Vector3(4.0f, 3.0f, .28f), darkWood);
            Box("SaloonUpperFrontMid", new Vector3(-7.6f, 4.85f, front), new Vector3(1.9f, 3.0f, .28f), darkWood);
            Box("SaloonUpperFrontRight", new Vector3(-3.1f, 4.85f, front), new Vector3(2.0f, 3.0f, .28f), darkWood);

            BuildRoof("SaloonRoof", new Vector3(-7.6f, 8.03f, 11.75f), width + 1.0f, depth + .9f, 31f);
            BuildPorch(new Vector3(-7.6f, .12f, 6.25f), width + .8f, 1.6f);
            BuildDoubleDoor(new Vector3(-6.1f, 1.25f, 6.82f));
            Window(new Vector3(-10.8f, 1.72f, 6.78f), 1.45f, 1.45f, false, glass);
            Window(new Vector3(-3.35f, 1.72f, 6.78f), 1.45f, 1.45f, false, glass);
            Window(new Vector3(-10.8f, 4.9f, 6.78f), 1.5f, 1.5f, false, glass);
            Window(new Vector3(-3.25f, 4.9f, 6.78f), 1.5f, 1.5f, false, glass);
            Window(new Vector3(-1.95f, 4.82f, 13.2f), 1.65f, 1.6f, true, glass);
            Window(new Vector3(-1.95f, 4.82f, 15.1f), 1.35f, 1.4f, true, glass);

            Box("SaloonSign", new Vector3(-6.1f, 4.12f, 6.47f), new Vector3(4.6f, 1.0f, .16f), darkWood, false);
            Text("THE DUSTY SPOON", new Vector3(-6.1f, 4.05f, 6.36f), 28, new Color(.82f, .59f, .28f), Quaternion.Euler(0f, 180f, 0f));
            Box("SaloonTrimFront", new Vector3(-7.6f, 2.9f, 6.77f), new Vector3(10.7f, .11f, .12f), trim, false);
            CreatePracticalLight("SaloonEntryLight", new Vector3(-6.1f, 2.72f, 6.45f), new Color(1f, .34f, .12f), 7.5f, 2.4f);
            CreatePracticalLight("SaloonMainLight", new Vector3(-6.5f, 2.65f, 11.8f), new Color(1f, .28f, .09f), 8.0f, 2.0f);
            CreatePracticalLight("SaloonUpperLamp", new Vector3(-6.8f, 5.8f, 12.4f), new Color(1f, .30f, .10f), 7.5f, 2.2f);

            BuildSaloonGroundProps(step);
            BuildStairCase();
            BuildSaloonUpperProps(step);
        }

        private void BuildSaloonGroundProps(string step)
        {
            BuildKitchen();
            BuildDiningTable(new Vector3(-6.5f, .16f, 11.0f));
            BuildDiningTable(new Vector3(-6.0f, .16f, 13.0f));
            BuildBar();
            BuildBottleLine(new Vector3(-3.75f, 1.0f, 14.9f), 5);
            BuildBloodTrail(new Vector3(-8.7f, .14f, 12.8f), 4);
            if (step is "downstairs_noise" or "knife" or "exit_saloon") BuildCrashDebris();
            BuildKnife(new Vector3(-6.35f, 1.12f, 12.55f), step is "knife" or "exit_saloon");
        }

        private void BuildSaloonUpperProps(string step)
        {
            Box("UpperDesk", new Vector3(-7.2f, 4.02f, 12.35f), new Vector3(1.45f, .12f, .65f), wood, false);
            Box("UpperDeskLegA", new Vector3(-7.7f, 3.68f, 12.1f), new Vector3(.12f, .68f, .12f), darkWood, false);
            Box("UpperDeskLegB", new Vector3(-6.7f, 3.68f, 12.1f), new Vector3(.12f, .68f, .12f), darkWood, false);
            Box("PocketDiary", new Vector3(-7.2f, 4.16f, 12.35f), new Vector3(.42f, .08f, .30f), leather, false)
                .transform.Rotate(0f, 11f, -8f);
            Box("LoosePage", new Vector3(-6.7f, 4.16f, 12.45f), new Vector3(.25f, .025f, .32f), paper, false)
                .transform.Rotate(0f, -15f, 4f);
            Box("WarningPaper", new Vector3(-3.0f, 5.15f, 13.22f), new Vector3(1.0f, 1.1f, .025f), paper, false);
            Text("QUIET\nTHEY HEAR", new Vector3(-3.0f, 5.15f, 13.17f), 18, new Color(.19f, .035f, .02f), Quaternion.Euler(0f, 180f, 0f));
            Box("UpperBed", new Vector3(-10.2f, 3.75f, 14.5f), new Vector3(2.2f, .35f, 1.15f), darkWood, false);
            Box("UpperMattress", new Vector3(-10.2f, 4.03f, 14.5f), new Vector3(2.05f, .20f, 1.05f), cloth, false);
            Box("UpperPillow", new Vector3(-10.85f, 4.2f, 14.5f), new Vector3(.45f, .18f, .82f), paper, false);
            BuildBloodTrail(new Vector3(-7.5f, 3.47f, 14.0f), 3);
            if (step is "message" or "window") BuildWindowWatcher();
        }

        private void BuildKitchen()
        {
            Box("KitchenCounter", new Vector3(-11.35f, .68f, 14.2f), new Vector3(2.2f, 1.2f, .72f), darkWood);
            Box("KitchenCounterTop", new Vector3(-11.35f, 1.34f, 14.2f), new Vector3(2.35f, .12f, .82f), trim, false);
            Box("KitchenShelfBack", new Vector3(-12.78f, 2.0f, 14.2f), new Vector3(.12f, 2.7f, 2.25f), wood, false);
            for (var i = 0; i < 4; i++)
            {
                Box("KitchenShelf", new Vector3(-12.63f, 1.35f + i * .45f, 14.2f), new Vector3(.62f, .08f, 2.1f), trim, false);
                Bottle(new Vector3(-12.2f, 1.55f + i * .45f, 13.7f + (i % 2) * .45f), .22f);
            }
            Bottle(new Vector3(-11.9f, 1.55f, 14.45f), .28f);
            Box("CastIronPan", new Vector3(-10.7f, 1.48f, 14.15f), new Vector3(.72f, .07f, .48f), rust, false);
            Box("PanHandle", new Vector3(-10.15f, 1.51f, 14.15f), new Vector3(.75f, .08f, .10f), rust, false);
            CreatePracticalLight("SaloonKitchenLamp", new Vector3(-10.9f, 2.65f, 14.0f), new Color(1f, .31f, .12f), 4.2f, 1.1f);
        }

        private void BuildBar()
        {
            Box("SaloonBarBody", new Vector3(-3.65f, .95f, 14.45f), new Vector3(2.35f, 1.75f, .75f), darkWood);
            Box("SaloonBarTop", new Vector3(-3.65f, 1.9f, 14.45f), new Vector3(2.65f, .16f, .92f), trim, false);
            Box("BarBack", new Vector3(-3.65f, 1.9f, 15.85f), new Vector3(2.4f, 3.0f, .18f), wood, false);
            for (var i = 0; i < 4; i++)
            {
                Box("BarShelf", new Vector3(-3.65f, 1.0f + i * .58f, 15.65f), new Vector3(2.05f, .09f, .45f), trim, false);
                Bottle(new Vector3(-4.3f + i * .42f, 1.2f + i * .58f, 15.35f), .24f);
            }
        }

        private void BuildBottleLine(Vector3 start, int count)
        {
            for (var i = 0; i < count; i++) Bottle(start + new Vector3(i * .38f, 0f, (i % 2) * .12f), .18f + (i % 3) * .025f);
        }

        private void BuildDiningTable(Vector3 position)
        {
            Box("SaloonTable", position + new Vector3(0f, .82f, 0f), new Vector3(1.65f, .12f, 1.05f), wood, false);
            for (var x = -1; x <= 1; x += 2)
                for (var z = -1; z <= 1; z += 2)
                    Box("TableLeg", position + new Vector3(x * .58f, .4f, z * .32f), new Vector3(.12f, .8f, .12f), darkWood, false);
            Box("Plate", position + new Vector3(-.3f, .91f, -.1f), new Vector3(.34f, .035f, .34f), paper, false);
            Bottle(position + new Vector3(.3f, 1.08f, .12f), .23f);
            Box("TippedChair", position + new Vector3(0f, .45f, .85f), new Vector3(.65f, .10f, .65f), wood, false)
                .transform.Rotate(20f, 0f, 74f);
        }

        private void BuildStairCase()
        {
            for (var i = 0; i < 8; i++)
                Box("Stair", new Vector3(-9.85f, .25f + i * .38f, 9.1f + i * .42f), new Vector3(2.0f, .30f, .55f), wood, true);
            Box("DamagedStairDoor", new Vector3(-9.7f, 1.45f, 10.1f), new Vector3(1.9f, 2.35f, .13f), darkWood, false)
                .transform.Rotate(0f, 0f, -12f);
            for (var i = 0; i < 5; i++)
                Box("BrokenDoorSlat", new Vector3(-9.7f + (i - 2) * .35f, 1.45f, 10.0f), new Vector3(.08f, 2.1f, .24f), trim, false)
                    .transform.Rotate(0f, 0f, -18f + i * 9f);
        }

        private void BuildCrashDebris()
        {
            Box("BrokenBottleGlass", new Vector3(-7.0f, .12f, 11.45f), new Vector3(.85f, .025f, .08f), glass, false)
                .transform.Rotate(0f, 28f, 0f);
            Box("BrokenChairSeat", new Vector3(-5.6f, .35f, 12.05f), new Vector3(.72f, .10f, .72f), wood, false)
                .transform.Rotate(14f, 26f, 31f);
            Box("BrokenChairLeg", new Vector3(-5.4f, .42f, 12.25f), new Vector3(.08f, .85f, .08f), darkWood, false)
                .transform.Rotate(12f, 20f, 42f);
            Box("BrokenDoorPanel", new Vector3(-6.05f, 1.35f, 6.88f), new Vector3(.65f, 2.25f, .10f), darkWood, false)
                .transform.Rotate(0f, 0f, -14f);
        }

        private void BuildKnife(Vector3 position, bool visible)
        {
            var knife = new GameObject("SaloonKnife");
            knife.transform.SetParent(root);
            knife.transform.localPosition = position;
            knife.transform.localRotation = Quaternion.Euler(0f, 12f, 8f);
            var handle = Part("KnifeHandle", PrimitiveType.Cube, knife.transform, new Vector3(-.18f, 0f, 0f), new Vector3(.42f, .08f, .11f), leather, Quaternion.identity, false);
            var blade = Part("KnifeBlade", PrimitiveType.Cube, knife.transform, new Vector3(.18f, .01f, 0f), new Vector3(.38f, .035f, .16f), rust, Quaternion.Euler(0f, 0f, -8f), false);
            knife.SetActive(visible);
        }

        private void BuildWindowWatcher()
        {
            var watcher = new GameObject("WindowWatcher_Silhouette");
            watcher.transform.SetParent(root);
            watcher.transform.localPosition = new Vector3(-1.42f, 4.75f, 13.22f);
            var body = Part("WatcherBody", PrimitiveType.Capsule, watcher.transform, Vector3.zero, new Vector3(.42f, 1.25f, .18f), black, Quaternion.Euler(90f, 0f, 0f), false);
            Part("WatcherHead", PrimitiveType.Sphere, watcher.transform, new Vector3(0f, .86f, 0f), new Vector3(.45f, .45f, .20f), black, Quaternion.identity, false);
            Part("WatcherShoulder", PrimitiveType.Cube, watcher.transform, new Vector3(0f, .22f, 0f), new Vector3(1.15f, .25f, .22f), black, Quaternion.identity, false);
        }

        private void BuildChurch(string step)
        {
            const float centerX = 7.0f;
            const float front = 20.5f;
            const float back = 34.5f;
            Box("ChurchFloor", new Vector3(centerX, .06f, 27.5f), new Vector3(8.2f, .12f, 14.2f), stone);
            Box("ChurchWallLeft", new Vector3(2.8f, 3.0f, 27.5f), new Vector3(.28f, 5.9f, 14.2f), plaster);
            Box("ChurchWallRight", new Vector3(11.2f, 3.0f, 27.5f), new Vector3(.28f, 5.9f, 14.2f), plaster);
            Box("ChurchWallBack", new Vector3(centerX, 3.0f, back), new Vector3(8.4f, 5.9f, .28f), plaster);
            Box("ChurchFrontLeft", new Vector3(4.55f, 3.0f, front), new Vector3(3.5f, 5.9f, .28f), plaster);
            Box("ChurchFrontRight", new Vector3(9.45f, 3.0f, front), new Vector3(3.5f, 5.9f, .28f), plaster);
            Box("ChurchFrontLintel", new Vector3(centerX, 5.6f, front), new Vector3(2.0f, .52f, .32f), trim);
            BuildRoof("ChurchRoof", new Vector3(centerX, 7.27f, 27.5f), 9.4f, 15.3f, 28f);
            BuildDoubleDoor(new Vector3(centerX, 1.35f, 20.3f));
            Window(new Vector3(4.0f, 3.35f, 20.3f), 1.25f, 2.2f, false, glass);
            Window(new Vector3(10.0f, 3.35f, 20.3f), 1.25f, 2.2f, false, glass);
            Box("ChurchTower", new Vector3(7.0f, 6.3f, 19.7f), new Vector3(3.2f, 7.2f, 3.2f), brick);
            BuildRoof("ChurchTowerRoof", new Vector3(7.0f, 10.75f, 19.7f), 3.7f, 3.7f, 35f);
            Text("✝", new Vector3(7.0f, 8.0f, 17.95f), 58, new Color(.66f, .45f, .25f), Quaternion.Euler(0f, 180f, 0f));
            BuildChurchInterior(step);
        }

        private void BuildChurchInterior(string step)
        {
            for (var row = 0; row < 4; row++)
            {
                var z = 23.2f + row * 2.0f;
                Pew(new Vector3(5.0f, .18f, z));
                Pew(new Vector3(9.0f, .18f, z));
            }
            Box("ChurchAltar", new Vector3(7.0f, 1.25f, 32.6f), new Vector3(2.8f, 2.5f, .75f), darkWood);
            Box("ChurchAltarCloth", new Vector3(7.0f, 2.55f, 32.2f), new Vector3(2.5f, .16f, .9f), cloth, false);
            Box("OpenBible", new Vector3(7.0f, 2.78f, 32.05f), new Vector3(.72f, .06f, .48f), paper, false)
                .transform.Rotate(0f, 8f, 0f);
            Candle(new Vector3(5.95f, 2.98f, 32.1f), "ChurchCandleLeft");
            Candle(new Vector3(8.05f, 2.98f, 32.1f), "ChurchCandleRight");
            Box("Confessional", new Vector3(10.0f, 1.7f, 29.8f), new Vector3(1.25f, 3.4f, 1.65f), darkWood);
            Box("Trapdoor", new Vector3(8.7f, .15f, 32.7f), new Vector3(1.25f, .06f, 1.55f), trim, false);
            if (step is "priest" or "church_interior")
            {
                var priest = Part("FatherElias", PrimitiveType.Capsule, root, new Vector3(7.0f, 1.65f, 31.15f), new Vector3(.5f, 1.45f, .5f), cloth, Quaternion.identity, false);
                Part("FatherEliasHead", PrimitiveType.Sphere, root, new Vector3(7.0f, 3.22f, 31.15f), new Vector3(.42f, .42f, .42f), paper, Quaternion.identity, false);
                Part("FatherEliasCandle", PrimitiveType.Cylinder, root, new Vector3(6.55f, 2.0f, 31.1f), new Vector3(.08f, .45f, .08f), candle, Quaternion.identity, false);
            }
            CreatePracticalLight("ChurchAltarLight", new Vector3(7f, 3.0f, 32f), new Color(1f, .47f, .16f), 6f, step is "priest" ? 1.8f : 1.0f);
        }

        private void BuildStation(string step)
        {
            const float left = -5.2f;
            const float right = 5.2f;
            const float front = 31.0f;
            const float back = 41.5f;
            Box("StationFloor", new Vector3(0f, .06f, 36.25f), new Vector3(10.4f, .12f, 10.7f), wood);
            Box("StationWallLeft", new Vector3(left, 1.65f, 36.25f), new Vector3(.3f, 3.2f, 10.7f), darkWood);
            Box("StationWallRight", new Vector3(right, 1.65f, 36.25f), new Vector3(.3f, 3.2f, 10.7f), darkWood);
            Box("StationWallBack", new Vector3(0f, 1.65f, back), new Vector3(10.4f, 3.2f, .3f), darkWood);
            Box("StationFrontLeft", new Vector3(-2.8f, 1.65f, front), new Vector3(4.8f, 3.2f, .3f), darkWood);
            Box("StationFrontRight", new Vector3(3.2f, 1.65f, front), new Vector3(4.0f, 3.2f, .3f), darkWood);
            Box("StationLintel", new Vector3(.2f, 2.9f, front), new Vector3(2.2f, .45f, .35f), trim);
            Box("StationUpperFloor", new Vector3(0f, 3.27f, 36.25f), new Vector3(10.2f, .18f, 10.5f), darkWood);
            Box("StationUpperLeft", new Vector3(left, 4.8f, 36.25f), new Vector3(.3f, 3.0f, 10.7f), darkWood);
            Box("StationUpperRight", new Vector3(right, 4.8f, 36.25f), new Vector3(.3f, 3.0f, 10.7f), darkWood);
            Box("StationUpperBack", new Vector3(0f, 4.8f, back), new Vector3(10.4f, 3.0f, .3f), darkWood);
            BuildRoof("StationRoof", new Vector3(0f, 7.94f, 36.25f), 11.4f, 11.7f, 29f);
            BuildDoubleDoor(new Vector3(.2f, 1.25f, 30.8f));
            Window(new Vector3(-3.8f, 1.7f, 30.8f), 1.35f, 1.5f, false, glass);
            Window(new Vector3(3.8f, 1.7f, 30.8f), 1.35f, 1.5f, false, glass);
            Box("StationSign", new Vector3(.2f, 4.0f, 30.5f), new Vector3(3.6f, .9f, .16f), darkWood, false);
            Text("SHERIFF", new Vector3(.2f, 3.93f, 30.39f), 30, new Color(.78f, .53f, .25f), Quaternion.Euler(0f, 180f, 0f));
            BuildStationInterior(step);
        }

        private void BuildStationInterior(string step)
        {
            Box("SheriffDesk", new Vector3(1.0f, .85f, 34.2f), new Vector3(2.4f, 1.55f, 1.0f), darkWood);
            Box("StationMap", new Vector3(-2.0f, 1.65f, 34.6f), new Vector3(2.1f, .04f, 1.15f), paper, false)
                .transform.Rotate(0f, 5f, 0f);
            Text("MINE  /  BARN", new Vector3(-2.0f, 1.69f, 34.6f), 13, new Color(.24f, .08f, .04f), Quaternion.Euler(90f, 0f, 0f));
            for (var i = 0; i < 4; i++)
                Box("CellBar", new Vector3(-3.8f + i * .75f, 1.2f, 36.8f), new Vector3(.12f, 2.4f, .12f), rust, false);
            Box("CellBack", new Vector3(-2.7f, 1.25f, 38.4f), new Vector3(2.8f, 2.5f, .18f), darkWood, false);
            Box("KeyBoard", new Vector3(3.5f, 1.8f, 34.0f), new Vector3(1.3f, 1.8f, .12f), wood, false);
            for (var i = 0; i < 5; i++)
                Box("MissingKeyHook", new Vector3(3.1f + i * .22f, 1.3f + (i % 2) * .45f, 33.9f), new Vector3(.05f, .24f, .05f), rust, false);
            Box("LaylaScarf", new Vector3(1.05f, 1.68f, 34.1f), new Vector3(.65f, .035f, .22f), cloth, false)
                .transform.Rotate(0f, 18f, 0f);
            Box("UpperDeskStation", new Vector3(.2f, 4.0f, 38.15f), new Vector3(1.8f, .12f, .8f), wood, false);
            Box("RedLedger", new Vector3(.2f, 4.16f, 38.15f), new Vector3(.75f, .09f, .52f), Tint(wood, "LedgerRed", new Color(.32f, .045f, .025f)), false);
            Box("TunnelMap", new Vector3(2.0f, 4.16f, 38.2f), new Vector3(1.1f, .035f, .75f), paper, false);
            Text("MINE\nSHAFT", new Vector3(2.0f, 4.19f, 38.2f), 12, new Color(.24f, .08f, .04f), Quaternion.Euler(90f, 0f, 0f));
            if (step is "station_hale" or "station_key")
            {
                Part("SheriffHale", PrimitiveType.Capsule, root, new Vector3(2.4f, 1.65f, 37.0f), new Vector3(.52f, 1.35f, .52f), leather, Quaternion.identity, false);
                Part("SheriffHaleHead", PrimitiveType.Sphere, root, new Vector3(2.4f, 3.12f, 37.0f), new Vector3(.42f, .42f, .42f), paper, Quaternion.identity, false);
            }
        }

        private void BuildHorseAndWoundedMan()
        {
            var horsePrefab = Resources.Load<GameObject>("Art/Characters/Horse");
            if (horsePrefab != null)
            {
                var horse = Object.Instantiate(horsePrefab, root);
                horse.name = "ArrivalHorse";
                horse.transform.localPosition = new Vector3(2.8f, .05f, -12.4f);
                horse.transform.localRotation = Quaternion.Euler(0f, 170f, 0f);
                horse.transform.localScale = Vector3.one * .42f;
            }
            var wounded = new GameObject("WoundedManAtGate");
            wounded.transform.SetParent(root);
            wounded.transform.localPosition = new Vector3(-1.65f, .45f, -4.35f);
            wounded.transform.localRotation = Quaternion.Euler(0f, 18f, 72f);
            Part("WoundedBody", PrimitiveType.Capsule, wounded.transform, new Vector3(0f, .55f, 0f), new Vector3(.52f, 1.0f, .35f), cloth, Quaternion.identity, false);
            Part("WoundedHead", PrimitiveType.Sphere, wounded.transform, new Vector3(0f, 1.25f, 0f), new Vector3(.34f, .34f, .34f), paper, Quaternion.identity, false);
            Part("WoundedBlood", PrimitiveType.Cube, wounded.transform, new Vector3(.24f, .82f, -.12f), new Vector3(.22f, .18f, .03f), blood, Quaternion.Euler(0f, 24f, 0f), false);
            Lantern(new Vector3(-1.05f, .48f, -4.45f), "DroppedLampAtGate");
        }

        private void BuildAtmosphericProps(string step)
        {
            for (var i = 0; i < 20; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var x = side * (7.0f + (i % 4) * 1.6f);
                var z = -1f + i * 2.45f;
                Box("DryGrass", new Vector3(x, .18f, z), new Vector3(.12f, .38f, .12f), foliage, false)
                    .transform.Rotate(0f, i * 19f, i % 2 == 0 ? -14f : 14f);
            }
            if (step is "church_approach" or "enter_church")
            {
                for (var i = 0; i < 7; i++)
                    Box("TracksToChurch", new Vector3(5.65f + Mathf.Sin(i) * .28f, .11f, 17.0f + i * .6f), new Vector3(.15f, .018f, .36f), darkWood, false)
                        .transform.Rotate(0f, i % 2 == 0 ? -18f : 18f, 0f);
            }
        }

        private void BuildWell(Vector3 position)
        {
            for (var i = 0; i < 12; i++)
            {
                var angle = i * 30f * Mathf.Deg2Rad;
                var offset = new Vector3(Mathf.Cos(angle) * 1.15f, .45f, Mathf.Sin(angle) * 1.15f);
                var block = Box("WellStone", position + offset, new Vector3(.72f, .75f, .42f), stone, false);
                block.transform.localRotation = Quaternion.Euler(0f, -i * 30f, 0f);
            }
            Box("WellWater", position + new Vector3(0f, .84f, 0f), new Vector3(1.55f, .04f, 1.55f), glass, false);
            Box("WellPostLeft", position + new Vector3(-1.18f, 2.2f, 0f), new Vector3(.22f, 2.7f, .22f), darkWood, false);
            Box("WellPostRight", position + new Vector3(1.18f, 2.2f, 0f), new Vector3(.22f, 2.7f, .22f), darkWood, false);
            Box("WellRoofBeam", position + new Vector3(0f, 3.4f, 0f), new Vector3(3.0f, .18f, .25f), trim, false);
            BuildRoof("WellRoof", position + new Vector3(0f, 4.5f, 0f), 3.1f, 2.0f, 30f);
        }

        private void BuildWagon(Vector3 position, float yaw)
        {
            var wagon = new GameObject("AbandonedWagon");
            wagon.transform.SetParent(root);
            wagon.transform.localPosition = position;
            wagon.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Part("WagonBed", PrimitiveType.Cube, wagon.transform, new Vector3(0f, .75f, 0f), new Vector3(2.3f, .25f, 4.1f), wood, Quaternion.identity, false);
            Part("WagonSideLeft", PrimitiveType.Cube, wagon.transform, new Vector3(-1.1f, 1.2f, 0f), new Vector3(.16f, .8f, 4.1f), trim, Quaternion.identity, false);
            Part("WagonSideRight", PrimitiveType.Cube, wagon.transform, new Vector3(1.1f, 1.2f, 0f), new Vector3(.16f, .8f, 4.1f), trim, Quaternion.identity, false);
            for (var x = -1; x <= 1; x += 2)
                for (var z = -1; z <= 1; z += 2)
                    Part("WagonWheel", PrimitiveType.Cylinder, wagon.transform, new Vector3(x * 1.35f, .7f, z * 1.35f), new Vector3(.65f, .15f, .65f), rust, Quaternion.Euler(90f, 0f, 0f), false);
            Part("WagonCanopy", PrimitiveType.Cube, wagon.transform, new Vector3(0f, 2.7f, .35f), new Vector3(2.35f, .12f, 2.2f), cloth, Quaternion.Euler(0f, 0f, 3f), false);
        }

        private void BuildFence(Vector3 start, float length, int posts)
        {
            for (var i = 0; i < posts; i++)
            {
                var z = start.z + i * length / (posts - 1f);
                Box("FencePost", new Vector3(start.x, .8f, z), new Vector3(.18f, 1.6f, .18f), wood, false);
                if (i < posts - 1) Box("FenceRail", new Vector3(start.x, 1.0f, z + length / (posts * 2f)), new Vector3(.18f, .14f, length / (posts - 1f)), trim, false);
            }
        }

        private void FrontierShell(string name, Vector3 center, float width, float height, Material wall, Material roofMaterial, bool windows)
        {
            // The caller's Y is a design reference, not an elevation. Using it as the
            // building origin made the entire backdrop float 2-3 metres above the road.
            var basePosition = TrailArrivalLayout.GroundAnchor(center);
            const float foundationHeight = .28f;
            const float floorHeight = .14f;
            const float wallBottom = .28f;
            const float depth = 5.6f;

            Box(name + "Foundation", basePosition + new Vector3(0f, foundationHeight / 2f, 0f),
                new Vector3(width + .34f, foundationHeight, depth + .34f), stone, false);
            Box(name + "Floor", basePosition + new Vector3(0f, wallBottom + floorHeight / 2f, 0f),
                new Vector3(width, floorHeight, depth), wall, false);
            var wallCenter = basePosition + new Vector3(0f, wallBottom + height / 2f, 0f);
            Box(name + "Back", wallCenter + new Vector3(0f, 0f, depth / 2f), new Vector3(width, height, .25f), wall);
            Box(name + "Left", wallCenter + new Vector3(-width / 2f, 0f, 0f), new Vector3(.25f, height, depth), wall);
            Box(name + "Right", wallCenter + new Vector3(width / 2f, 0f, 0f), new Vector3(.25f, height, depth), wall);
            Box(name + "Front", wallCenter + new Vector3(0f, 0f, -depth / 2f), new Vector3(width, height, .25f), wall);

            // A continuous fascia and a real gable roof make the silhouettes read as
            // buildings instead of disconnected floating primitives.
            Box(name + "FrontFascia", basePosition + new Vector3(0f, wallBottom + height - .16f, -depth / 2f - .04f),
                new Vector3(width + .12f, .18f, .28f), trim, false);
            Box(name + "Door", basePosition + new Vector3(0f, 1.35f, -depth / 2f - .16f),
                new Vector3(1.15f, 2.25f, .12f), darkWood, false);
            Box(name + "DoorStep", basePosition + new Vector3(0f, .18f, -depth / 2f - .42f),
                new Vector3(1.55f, .18f, .68f), stone, false);
            for (var i = -1; i <= 1; i++)
                Box(name + "FrontStud" + i, basePosition + new Vector3(i * width * .31f, wallBottom + height / 2f, -depth / 2f - .16f),
                    new Vector3(.12f, height - .18f, .12f), trim, false);

            var roofRise = Mathf.Clamp((width + .8f) * .14f, .85f, 2.2f);
            BuildRoof(name + "Roof", basePosition + new Vector3(0f, wallBottom + height + roofRise, 0f), width + .8f, 6.3f, 27f, roofMaterial);
            if (windows)
            {
                Window(basePosition + new Vector3(-width * .23f, wallBottom + 1.8f, -depth / 2f - .16f), 1.2f, 1.35f, false, glass);
                Window(basePosition + new Vector3(width * .23f, wallBottom + 1.8f, -depth / 2f - .16f), 1.2f, 1.35f, false, glass);
                CreatePracticalLight(name + "WindowLight", basePosition + new Vector3(0f, 2.0f, -depth / 2f + .2f), new Color(1f, .38f, .12f), 4.5f, .65f);
            }
        }

        private void BuildRoof(string name, Vector3 center, float width, float depth, float _angle, Material roofMaterial = null)
        {
            roofMaterial ??= roof;
            // Gable faces the street: the ridge runs along the building depth,
            // matching the western silhouettes in the Ash Creek reference.
            var halfWidth = width / 2f;
            var rise = Mathf.Clamp(width * .14f, .85f, 2.2f);
            var slopeLength = Mathf.Sqrt(halfWidth * halfWidth + rise * rise);
            var slopeAngle = Mathf.Atan2(rise, halfWidth) * Mathf.Rad2Deg;
            var slopeCenterY = center.y - rise / 2f;
            var slopeCenterX = halfWidth / 2f;

            Part(name + "SlopeA", PrimitiveType.Cube, root,
                center + new Vector3(-slopeCenterX, slopeCenterY - center.y, 0f),
                new Vector3(slopeLength, .24f, depth), roofMaterial, Quaternion.Euler(0f, 0f, slopeAngle), false);
            Part(name + "SlopeB", PrimitiveType.Cube, root,
                center + new Vector3(slopeCenterX, slopeCenterY - center.y, 0f),
                new Vector3(slopeLength, .24f, depth), roofMaterial, Quaternion.Euler(0f, 0f, -slopeAngle), false);
            Box(name + "Ridge", center, new Vector3(.25f, .18f, depth), trim, false);
            Box(name + "EaveA", center + new Vector3(-halfWidth, -rise, 0f), new Vector3(.18f, .14f, depth), trim, false);
            Box(name + "EaveB", center + new Vector3(halfWidth, -rise, 0f), new Vector3(.18f, .14f, depth), trim, false);
        }

        private void BuildPorch(Vector3 center, float width, float depth)
        {
            Box("SaloonPorchFloor", center, new Vector3(width, .16f, depth), trim, false);
            for (var x = -1; x <= 1; x += 2)
            {
                Box("PorchPost", center + new Vector3(x * (width / 2f - .3f), 1.65f, -.55f), new Vector3(.24f, 3.3f, .24f), trim, false);
                Box("PorchPostBack", center + new Vector3(x * (width / 2f - .3f), 1.65f, .55f), new Vector3(.24f, 3.3f, .24f), trim, false);
            }
            Box("PorchBeam", center + new Vector3(0f, 3.2f, 0f), new Vector3(width, .22f, 1.65f), roof, false);
            for (var i = -4; i <= 4; i++) Box("PorchRafter", center + new Vector3(i * 1.15f, 3.38f, 0f), new Vector3(.12f, .16f, 1.6f), wood, false);
        }

        private void BuildDoubleDoor(Vector3 position)
        {
            Box("DoorLeft", position + new Vector3(-.48f, 0f, 0f), new Vector3(.86f, 2.25f, .12f), darkWood, false)
                .transform.Rotate(0f, -13f, 0f);
            Box("DoorRight", position + new Vector3(.48f, 0f, .02f), new Vector3(.86f, 2.25f, .12f), darkWood, false)
                .transform.Rotate(0f, 13f, 0f);
            Box("DoorTrimLeft", position + new Vector3(-1.02f, 0f, 0f), new Vector3(.12f, 2.45f, .22f), trim, false);
            Box("DoorTrimRight", position + new Vector3(1.02f, 0f, 0f), new Vector3(.12f, 2.45f, .22f), trim, false);
        }

        private void Window(Vector3 position, float width, float height, bool sideWall, Material windowMaterial)
        {
            var rotation = sideWall ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            var axis = sideWall ? Vector3.forward : Vector3.right;
            Box("WindowGlass", position, new Vector3(sideWall ? .045f : width, height, sideWall ? width : .045f), windowMaterial, false);
            Part("WindowFrameTop", PrimitiveType.Cube, root, position + new Vector3(0f, height / 2f, 0f), sideWall ? new Vector3(.15f, .14f, width + .18f) : new Vector3(width + .18f, .14f, .15f), trim, rotation, false);
            Part("WindowFrameBottom", PrimitiveType.Cube, root, position + new Vector3(0f, -height / 2f, 0f), sideWall ? new Vector3(.15f, .14f, width + .18f) : new Vector3(width + .18f, .14f, .15f), trim, rotation, false);
            Part("WindowFrameA", PrimitiveType.Cube, root, position + (sideWall ? new Vector3(0f, 0f, -width / 2f) : new Vector3(-width / 2f, 0f, 0f)), sideWall ? new Vector3(.15f, height, .14f) : new Vector3(.14f, height, .15f), trim, rotation, false);
            Part("WindowFrameB", PrimitiveType.Cube, root, position + (sideWall ? new Vector3(0f, 0f, width / 2f) : new Vector3(width / 2f, 0f, 0f)), sideWall ? new Vector3(.15f, height, .14f) : new Vector3(.14f, height, .15f), trim, rotation, false);
            Part("WindowCross", PrimitiveType.Cube, root, position, sideWall ? new Vector3(.16f, height, .08f) : new Vector3(.08f, height, .16f), trim, rotation, false);
            if (!sideWall) Part("WindowCrossHorizontal", PrimitiveType.Cube, root, position, new Vector3(width, .08f, .16f), trim, rotation, false);
        }

        private void Pew(Vector3 position)
        {
            Box("ChurchPewSeat", position + new Vector3(0f, .75f, 0f), new Vector3(2.5f, .18f, .48f), wood, false);
            Box("ChurchPewBack", position + new Vector3(0f, 1.25f, .18f), new Vector3(2.5f, .9f, .16f), wood, false);
            Box("ChurchPewLegA", position + new Vector3(-1.0f, .38f, 0f), new Vector3(.12f, .75f, .18f), darkWood, false);
            Box("ChurchPewLegB", position + new Vector3(1.0f, .38f, 0f), new Vector3(.12f, .75f, .18f), darkWood, false);
        }

        private void Bottle(Vector3 position, float scale)
        {
            Part("Bottle", PrimitiveType.Cylinder, root, position, new Vector3(scale, scale * 1.2f, scale), glass, Quaternion.identity, false);
            Part("BottleNeck", PrimitiveType.Cylinder, root, position + new Vector3(0f, scale * 1.25f, 0f), new Vector3(scale * .42f, scale * .55f, scale * .42f), glass, Quaternion.identity, false);
        }

        private void Lantern(Vector3 position, string name)
        {
            Part(name, PrimitiveType.Cylinder, root, position, new Vector3(.22f, .35f, .22f), rust, Quaternion.identity, false);
            Part(name + "Glow", PrimitiveType.Sphere, root, position + new Vector3(0f, .15f, 0f), new Vector3(.14f, .14f, .14f), candle, Quaternion.identity, false);
        }

        private void Candle(Vector3 position, string name)
        {
            Part(name, PrimitiveType.Cylinder, root, position, new Vector3(.07f, .35f, .07f), candle, Quaternion.identity, false);
            CreatePracticalLight(name + "Light", position + new Vector3(0f, .4f, 0f), new Color(1f, .43f, .14f), 3.4f, .55f);
        }

        private void BuildBloodTrail(Vector3 start, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var mark = Box("BloodMark", start + new Vector3(Mathf.Sin(i * 1.9f) * .34f, 0f, i * .58f), new Vector3(.22f + i * .03f, .018f, .10f), blood, false);
                mark.transform.Rotate(0f, i * 27f, 0f);
            }
        }

        private void CreateTree(string name, Vector3 position, float scale)
        {
            var tree = new GameObject(name);
            tree.transform.SetParent(root);
            tree.transform.localPosition = position;
            tree.transform.localScale = Vector3.one * scale;
            Part("Trunk", PrimitiveType.Cylinder, tree.transform, new Vector3(0f, 2.2f, 0f), new Vector3(.34f, 2.2f, .34f), trunk, Quaternion.identity, false);
            Part("BranchA", PrimitiveType.Cylinder, tree.transform, new Vector3(-.55f, 3.35f, 0f), new Vector3(.15f, 1.15f, .15f), trunk, Quaternion.Euler(0f, 0f, -38f), false);
            Part("BranchB", PrimitiveType.Cylinder, tree.transform, new Vector3(.65f, 3.55f, .1f), new Vector3(.13f, 1.25f, .13f), trunk, Quaternion.Euler(0f, 0f, 42f), false);
            Part("CanopyA", PrimitiveType.Sphere, tree.transform, new Vector3(-.85f, 4.35f, 0f), new Vector3(1.75f, 1.25f, 1.55f), foliage, Quaternion.identity, false);
            Part("CanopyB", PrimitiveType.Sphere, tree.transform, new Vector3(.45f, 4.45f, .2f), new Vector3(1.95f, 1.35f, 1.75f), foliage, Quaternion.identity, false);
            Part("CanopyC", PrimitiveType.Sphere, tree.transform, new Vector3(1.15f, 4.0f, -.35f), new Vector3(1.35f, 1.0f, 1.3f), foliage, Quaternion.identity, false);
        }

        private void CreateCactus(Vector3 position, float scale)
        {
            var cactus = new GameObject("Cactus");
            cactus.transform.SetParent(root);
            cactus.transform.localPosition = position;
            cactus.transform.localScale = Vector3.one * scale;
            Part("CactusStem", PrimitiveType.Capsule, cactus.transform, new Vector3(0f, 1.0f, 0f), new Vector3(.32f, 1.0f, .32f), foliage, Quaternion.identity, false);
            Part("CactusArmL", PrimitiveType.Capsule, cactus.transform, new Vector3(-.45f, 1.1f, 0f), new Vector3(.18f, .55f, .18f), foliage, Quaternion.Euler(0f, 0f, 90f), false);
            Part("CactusArmR", PrimitiveType.Capsule, cactus.transform, new Vector3(.42f, .72f, 0f), new Vector3(.18f, .48f, .18f), foliage, Quaternion.Euler(0f, 0f, -90f), false);
        }

        private void CreatePracticalLight(string name, Vector3 position, Color color, float range, float intensity)
        {
            Part(name + "Lantern", PrimitiveType.Sphere, root, position, Vector3.one * .12f, candle, Quaternion.identity, false);
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(root);
            lightObject.transform.localPosition = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
        }

        private Material CreateAssetStoreRoadMaterial()
        {
            var albedo = Resources.Load<Texture2D>("Environment/AssetStoreRoadTextures/dirtRoad_A");
            var normal = Resources.Load<Texture2D>("Environment/AssetStoreRoadTextures/dirtRoad_N");
            if (albedo == null || road == null) return road;

            // Clone the already working project road material so the render
            // pipeline/shader variant is preserved, then swap only the Asset
            // Store surface maps. This avoids an unsupported magenta shader
            // when the URP shader is not included by name in a build.
            var material = new Material(road) { name = "AshCreek_AssetStoreRoad" };
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", albedo);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", albedo);
            if (normal != null && material.HasProperty("_BumpMap"))
            {
                material.EnableKeyword("_NORMALMAP");
                material.SetTexture("_BumpMap", normal);
                material.SetFloat("_BumpScale", .65f);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", new Color(.48f, .31f, .17f, 1f));
            if (material.HasProperty("_Color")) material.SetColor("_Color", new Color(.48f, .31f, .17f, 1f));
            if (material.HasProperty("_BaseMap")) material.SetTextureScale("_BaseMap", new Vector2(1.35f, 8f));
            if (material.HasProperty("_MainTex")) material.SetTextureScale("_MainTex", new Vector2(1.35f, 8f));
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .12f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", .02f);
            return material;
        }

        private void Text(string value, Vector3 position, int size, Color color, Quaternion rotation)
        {
            var textObject = new GameObject("SignText_" + value.Replace(" ", "_"));
            textObject.transform.SetParent(root);
            textObject.transform.localPosition = position;
            textObject.transform.localRotation = rotation;
            var mesh = textObject.AddComponent<TextMesh>();
            mesh.text = value;
            mesh.fontSize = size;
            mesh.characterSize = .035f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
        }

        private GameObject Box(string name, Vector3 position, Vector3 scale, Material material, bool collision = true)
            => Part(name, PrimitiveType.Cube, root, position, scale, material, Quaternion.identity, collision);

        private GameObject Part(string name, PrimitiveType primitive, Transform parent, Vector3 position, Vector3 scale, Material material, Quaternion rotation, bool collision)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision)
            {
                var collider = part.GetComponent<Collider>();
                if (collider != null) Object.Destroy(collider);
            }
            return part;
        }

        private static Material Tint(Material source, string name, Color color)
        {
            var material = new Material(source) { name = name };
            var property = material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            if (material.HasProperty(property)) material.SetColor(property, color);
            return material;
        }
    }
}
