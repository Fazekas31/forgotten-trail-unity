# Forgotten Trail — Catálogo de assets visuais

Este arquivo é a fonte de verdade para a primeira fatia visual realista-estilizada de Ash Creek. Nenhum asset externo é adicionado ao projeto sem origem, licença e regra de redistribuição registradas aqui.

## Fundação da chegada

| Asset / sistema | Uso | Origem | Licença / distribuição |
| --- | --- | --- | --- |
| `TrailWorldBuilder` — `BuildArrivalScenery` | estrada de terra, bancos de terreno, árvores mesquite, arbustos, capim seco e luzes práticas | geometria procedural original do projeto | código original de Forgotten Trail; livre para uso dentro deste projeto |
| `Ground076L`, `Cactus`, `Planks023A`, `Bricks096`, `Metal041C`, `PavingStones115B` | materiais da estrada, terreno, madeira, tijolo, ferrugem, vegetação e pedras | texturas que já pertenciam ao projeto Godot; `WesternThemeBuildingPack/WesternTheme/README.txt` declara origem em cc0textures.com | CC0 conforme declaração da fonte; manter este registro ao redistribuir o projeto |
| `ForgottenTrail_MainEnvironment.glb` | núcleo authored da cidade e composição dos edifícios | asset convertido do projeto Godot original | sujeito aos créditos/licenças da fonte; não redistribuir isoladamente |

## Convenção de importação

- 1 unidade Unity = 1 metro; a chegada usa uma faixa jogável de aproximadamente 44 m × 48 m.
- Estruturas authored mantêm a escala original; a paisagem procedural usa a mesma escala e fica sob `AshCreek_Act_0`.
- Elementos decorativos não recebem collider. O chão base mantém o collider de navegação do trecho.
- Árvores possuem dois níveis de detalhe (`LOD0` e `LOD1`); o LOD distante usa uma silhueta simples para reduzir custo durante a navegação.
- Materiais usam o shader disponível no projeto (`Standard`, com fallback para `Universal Render Pipeline/Lit`) e tiling explícito por função.
- Luzes práticas da chegada não projetam sombra; a sombra principal vem da luz direcional lunar, preservando a meta de 60 FPS em PC.

## Assets gratuitos aprovados para as próximas fatias

Estes assets já possuem crédito na fonte Godot, mas ainda não são necessários para a fundação da chegada. Só devem ser ativados depois de uma validação de escala e de compatibilidade visual:

- Kenney Prototype Kit / `animal-horse.glb` — Kenney — CC0 1.0.
- `Horse.fbx` — Quaternius Farm Animal Pack — CC0 1.0.
- Free Characters — Katcho — CC0 1.0.
- Characters PSX — Elbolilloduro — CC0 1.0.

As licenças completas e URLs estão preservadas em `Documentation/Source` na árvore de origem e no `CREDITS.md` do projeto Godot. O catálogo será atualizado no mesmo commit que introduzir cada novo arquivo binário.
