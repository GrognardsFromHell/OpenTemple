using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Logbook
{
    public class LogbookKeyTranslations
    {
        public string ListCaption { get; private set; }

        public string DetailCaption { get; private set; }

        public string LabelAcquired { get; private set; }

        public string LabelDay { get; private set; }

        public string LabelMonth { get; private set; }

        public string LabelUsed { get; private set; }

        public string NeverUsed { get; private set; }

        public string NoCurrentKeys { get; private set; }

        public string DetailsHelp { get; private set; }

        public string NotificationPopupTitle { get; private set; }

        public string NotificationPopupText { get; private set; }

        public string NotificationPopupPrompt { get; private set; }

        public string NotificationPopupYes { get; private set; }

        public string NotificationPopupNo { get; private set; }

        public LogbookKeyTranslations()
        {
            Load();
        }

        private void Load()
        {
            var translations = Tig.FS.ReadMesFile("mes/logbook_ui_keys_text.mes");
            ListCaption = translations[10];
            DetailCaption = translations[20];
            LabelAcquired = translations[30];
            LabelDay = translations[41];
            LabelMonth = translations[42];
            LabelUsed = translations[43];
            NeverUsed = translations[45];
            NoCurrentKeys = translations[60];
            DetailsHelp = translations[70];
            NotificationPopupTitle = translations[100];
            NotificationPopupText = translations[110];
            NotificationPopupPrompt = translations[115];
            NotificationPopupYes = translations[120];
            NotificationPopupNo = translations[130];
        }
    }
}