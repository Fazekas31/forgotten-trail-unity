using UnityEngine;
using System.Collections.Generic;

namespace ForgottenTrail
{
    public sealed class TrailLocalization
    {
        public bool English { get; private set; }
        public void SetEnglish(bool value) => English = value;
        public string Objective(string step) => English ? Lookup(TrailContent.ObjectivesEn, step) : Lookup(TrailContent.ObjectivesPt, step);

        private static string Lookup(IReadOnlyDictionary<string, string> table, string step)
        {
            return table.TryGetValue(step, out var value) ? value : step;
        }
        public string Text(string pt, string en) => English ? en : pt;
        public string LabelObjective => Text("OBJETIVO", "OBJECTIVE");
        public string LabelInventory => Text("RECURSOS", "ITEMS");
        public string LabelJournal => Text("DIÁRIO DE JORNADA", "JOURNEY JOURNAL");
        public string LabelContinue => Text("CONTINUAR", "CONTINUE");
        public string LabelNewGame => Text("NOVO JOGO", "NEW GAME");
        public string LabelOptions => Text("OPÇÕES", "OPTIONS");
        public string LabelQuit => Text("SAIR", "QUIT");
        public string LabelPause => Text("PAUSA", "PAUSE");
        public string LabelClose => Text("FECHAR", "CLOSE");
        public string ItemName(TrailItem item) => English ? item.nameEn : item.namePt;
        public string ItemDescription(TrailItem item) => English ? item.descriptionEn : item.descriptionPt;
    }
}
