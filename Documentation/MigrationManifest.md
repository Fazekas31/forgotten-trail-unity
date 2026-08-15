# Forgotten Trail — Manifesto de migração

Fonte: `/Users/leo/Documents/forgotten-trail`

## Regras

- o Godot original permanece intacto;
- modelos, texturas, áudio e fontes licenciadas são importados preservando créditos;
- `.godot`, `.import`, `.uid`, builds e caches não entram no projeto Unity;
- arquivos de documentação e créditos acompanham a migração em `Documentation/Source`;
- modelos GLB/GLTF são importados pelo pipeline glTFast via Unity MCP;
- cada lote de assets será validado por escala, materiais, rig e colisão.

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
- [ ] jogador e interação;
- [ ] mundo visual e filtro VHS;
- [ ] importação dos assets principais;
- [ ] Atos I–IV jogáveis;
- [ ] testes Unity e build Windows.
