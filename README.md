# Forgotten Trail — Unity

Conversão do projeto Godot de `Documents/forgotten-trail` para Unity 6.3.22f1.

## Direção de fidelidade

- primeira pessoa, resolução-base 640×360 escalável para 1280×720;
- paleta quente de madeira, poeira, ferrugem e azul lunar;
- iluminação noturna com contraste baixo, neblina e luzes práticas;
- leitura low-poly/PS1 com filtro VHS, granulação e scanlines discretos;
- investigação, Diário de jornada e defesa limitada continuam sendo o centro do jogo;
- PT-BR é o idioma padrão; inglês está previsto na mesma interface.

## Estrutura

O código em `Assets/ForgottenTrail/Runtime` separa os módulos de domínio dos adaptadores Unity. A campanha é guiada por Etapas e eventos, e o estado pode ser salvo por checkpoint sem depender da cena ativa.

O projeto original permanece em `/Users/leo/Documents/forgotten-trail` e não é modificado por esta conversão.

## MCP da Unity

Ao abrir este projeto no Editor, conecte a instância ao Unity MCP antes de executar operações de cena, importação ou testes. O fluxo de trabalho usa `read_console` após alterações C# e mantém a instância `MedievalSurvival` fora do escopo.
