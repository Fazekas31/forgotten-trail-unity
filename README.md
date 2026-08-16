# Forgotten Trail — Unity

Conversão do projeto Godot de `Documents/forgotten-trail` para Unity `6000.3.22f1`.

## Direção de fidelidade

- primeira pessoa, resolução-base 640×360 escalável para 1280×720;
- paleta quente de madeira, poeira, ferrugem e azul lunar;
- iluminação noturna com contraste baixo, neblina e luzes práticas;
- realismo estilizado western com geometria authored, paisagem procedural e filtro VHS, granulação e scanlines discretos;
- investigação, Diário de jornada e defesa limitada continuam sendo o centro do jogo;
- PT-BR é o idioma padrão; inglês está previsto na mesma interface.

## Estrutura

O código em `Assets/ForgottenTrail/Runtime` separa os módulos de domínio dos adaptadores Unity. A campanha é guiada por Etapas e eventos, e o estado pode ser salvo por checkpoint sem depender da cena ativa.

O projeto original permanece em `/Users/leo/Documents/forgotten-trail` e não é modificado por esta conversão.

## Executar

Abra o projeto no Unity `6000.3.22f1`, carregue `Assets/ForgottenTrail/Scenes/ForgottenTrail.unity` e pressione Play. No menu, escolha **NOVO JOGO**.

Controles: WASD, mouse, Shift para correr, Ctrl para agachar, F para o lampião, I para inventário, J para diário, E para interagir e botão esquerdo para usar a arma equipada.

O cenário authored, cabeça do protagonista, revólver, munição, faca, sons e textura do diário vieram da fonte Godot e estão organizados em `Assets/ForgottenTrail/Resources`. A chegada combina esse cenário com estrada de terra, bancos de terreno, árvores, arbustos, capim seco, LOD e luzes práticas gerados pelo `TrailWorldBuilder`.

O catálogo de origem, licenças e convenções de importação está em [`Documentation/ArtAssetCatalog.md`](Documentation/ArtAssetCatalog.md).

## Verificação

Os testes EditMode estão em `Assets/ForgottenTrail/Tests/Runtime` e cobrem progressão dos quatro atos, snapshot/save e regras de armas. O último ciclo validado pelo Unity MCP passou com 4 testes.

## MCP da Unity

Ao abrir este projeto no Editor, conecte a instância ao Unity MCP antes de executar operações de cena, importação ou testes. O fluxo de trabalho usa `read_console` após alterações C# e mantém a instância `MedievalSurvival` fora do escopo.
