using System.Collections.Generic;

namespace ForgottenTrail
{
    public sealed class JournalModel
    {
        private readonly List<string> ids = new();
        private readonly Dictionary<string, JournalEntry> entries = new();
        public IReadOnlyList<string> Ids => ids;

        public JournalModel()
        {
            Add(new JournalEntry { id="arrival", titlePt="A entrada de Ash Creek", titleEn="The Ash Creek gate", momentPt="Noite — o portão", momentEn="Night — the gate", textPt="A última carta de Layla veio de Ash Creek há três semanas. Alguma coisa na cidade escuta tudo.", textEn="Layla's last letter came from Ash Creek three weeks ago. Something in town hears everything." });
            Add(new JournalEntry { id="saloon", titlePt="O saloon silencioso", titleEn="The silent saloon", momentPt="Saloon — térreo", momentEn="Saloon — ground floor", textPt="A cozinha foi deixada no meio do preparo. Ouvi um gemido no andar de cima.", textEn="The kitchen was abandoned mid-preparation. I heard a moan upstairs." });
            Add(new JournalEntry { id="church", titlePt="Um abrigo improvisado", titleEn="An improvised shelter", momentPt="Igreja", momentEn="Church", textPt="Elias confirmou que Layla seguiu com os feridos para o celeiro.", textEn="Elias confirmed Layla went with the injured to the barn." });
            Add(new JournalEntry { id="station", titlePt="O nome de Layla", titleEn="Layla's name", momentPt="Delegacia", momentEn="Station", textPt="O registro vermelho confirma a transferência para o celeiro.", textEn="The red ledger confirms the transfer to the barn." });
            Add(new JournalEntry { id="barn", titlePt="Layla está viva", titleEn="Layla is alive", momentPt="Celeiro", momentEn="Barn", textPt="Encontrei Layla. O Imitador usa vozes conhecidas para separar os vivos.", textEn="I found Layla. The Mimic uses familiar voices to separate the living." });
            Add(new JournalEntry { id="mine", titlePt="As vozes da mina", titleEn="The mine voices", momentPt="Galerias", momentEn="Galleries", textPt="Os mineiros abriram uma passagem e libertaram algo que aprendeu a lembrar.", textEn="The miners opened a passage and freed something that learned to remember." });
            Add(new JournalEntry { id="reunion", titlePt="A âncora", titleEn="The anchor", momentPt="Câmara", momentEn="Chamber", textPt="Layla ainda tem vontade própria, mas a criatura usa sua memória como âncora.", textEn="Layla still has her own will, but the creature uses her memory as an anchor." });
            Add(new JournalEntry { id="shared", titlePt="Rastro compartilhado", titleEn="Shared trail", momentPt="Depois da mina", momentEn="After the mine", textPt="Saímos juntos. O Imitador não morreu, mas sua voz ficou distante.", textEn="We left together. The Mimic did not die, but its voice grew distant." });
            Add(new JournalEntry { id="silence", titlePt="Silêncio definitivo", titleEn="Definitive silence", momentPt="Depois da mina", momentEn="After the mine", textPt="Mantive a Campainha ativa. Layla ficou para trás e as vozes cessaram.", textEn="I kept the Bell active. Layla stayed behind and the voices stopped." });
        }

        private void Add(JournalEntry entry) => entries[entry.id] = entry;
        public void RecordForStep(string step)
        {
            var id = step switch
            {
                "arrival" or "footprints" => "arrival", "meal" or "diary" or "window" => "saloon",
                "priest" or "enter_church" => "church", "station_ledger" or "station_key" => "station",
                "barn_layla" or "barn_map" => "barn", "mine_records" or "mine_bell" => "mine",
                "mine_reunion" or "final_chamber" => "reunion", _ => null
            };
            if (id != null && !ids.Contains(id)) ids.Add(id);
        }
        public void AddEnding(TrailEnding ending) { var id = ending == TrailEnding.SharedTrail ? "shared" : "silence"; if (!ids.Contains(id)) ids.Add(id); }
        public List<JournalEntry> Entries(bool english)
        {
            var result = new List<JournalEntry>(); foreach (var id in ids) if (entries.ContainsKey(id)) result.Add(entries[id]); return result;
        }
        public void Restore(IEnumerable<string> saved) { ids.Clear(); foreach (var id in saved) if (entries.ContainsKey(id) && !ids.Contains(id)) ids.Add(id); }
        public List<string> Snapshot() => new(ids);
    }
}
