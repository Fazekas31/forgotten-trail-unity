# Forgotten Trail — Manifesto de migração

Fonte: `/Users/leo/Documents/forgotten-trail`

## Regras

- o Godot original permanece intacto;
- modelos, texturas, áudio e fontes licenciadas são importados preservando créditos;
- `.godot`, `.import`, `.uid`, builds e caches não entram no projeto Unity;
- arquivos de documentação e créditos acompanham a migração em `Documentation/Source`;
- modelos GLB/GLTF são importados pelo pipeline glTFast via Unity MCP;
- cada lote de assets será validado por escala, materiais, rig e colisão.

## Lote visual importado

- `Resources/Environment/ForgottenTrail_MainEnvironment.glb`: cenário principal authored;
- `Resources/Characters/CowboyLeo_Head.glb`: cabeça do protagonista;
- `Resources/Props/PSX_Revolver.fbx`, `PSX_Ammo.glb` e `PSX_KitchenKnife.glb`: props e viewmodels;
- `Resources/Audio/*`: vento, sinos, passos, madeira, batimento, impacto e gemidos;
- `Art/Textures/DiaryCover.png`: capa usada na linguagem visual do diário;
- `Shaders/Source/analog_horror_vhs.gdshader`: shader-fonte preservado para futuras conversões URP; o runtime usa um overlay compatível com IMGUI para manter a apresentação funcional em qualquer pipeline.

## Mapeamento de sistemas

| Godot | Unity |
| --- | --- |
| `ActDirector` | `TrailCampaign` |
| `InventoryModel` | `InventoryModel` |
| `JournalModel` | `JournalModel` |
| `LanternModel` | `LanternModel` |
| `SaveCodec` / `SaveManager` | `TrailSaveStore` |
| `player.gd` | `TrailPlayerController` |
| `interactable.gd` | `TrailInteractable` |
| `tension_audio.gd` | `TrailAudioDirector` + `NoiseSystem` |
| `game_hud.gd` | `TrailUI` |
| `environment_populator.gd` | `TrailWorldBuilder` |

## Estado atual

- [x] projeto Unity e módulos de domínio iniciais;
- [x] jogador e interação;
- [x] mundo visual authored, paisagem procedural da chegada e filtro VHS;
- [x] catálogo de assets, licenças, escala, materiais, colisão e LOD em `Documentation/ArtAssetCatalog.md`;
- [x] importação dos assets principais;
- [x] Atos I–IV jogáveis;
- [x] testes Unity (EditMode: 4/4);
- [ ] build Windows (não executado neste ambiente macOS).
