using UnityEngine;

namespace ForgottenTrail
{
    public sealed class TrailLocalization
    {
        public bool English { get; private set; }
        public void SetEnglish(bool value) => English = value;
        public string Objective(string step) => English ? TrailContent.ObjectivesEn.GetValueOrDefault(step, step) : TrailContent.ObjectivesPt.GetValueOrDefault(step, step);
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
