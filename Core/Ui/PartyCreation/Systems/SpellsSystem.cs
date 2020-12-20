using OpenTemple.Core.GameObject;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f7b1c)]
    internal class SpellsSystem : IChargenSystem
    {

        public string HelpTopic => "TAG_CHARGEN_SPELLS";

        public ChargenStages Stage => ChargenStages.CG_Stage_Spells;

        public WidgetContainer Container { get; private set; }

        [TempleDllLocation(0x10c0eefc)]
        private bool chargenSpellsReseted;

        [TempleDllLocation(0x101800e0)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:176")]
        public SpellsSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/spells_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            // TODO int v1;
            // TODO int v3;
            // TODO int v4;
            // TODO string meslineValue;
            // TODO int meslineKey;
// TODO
            // TODO v1 = 0;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.textColor = &chargenSpellLevelLabelStyle_0 /*0x102fd1a8*/;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.colors4 = &chargenSpellLevelLabelStyle_0 /*0x102fd1a8*/;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.colors2 = &chargenSpellLevelLabelStyle_0 /*0x102fd1a8*/;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.textColor = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.colors4 = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.colors2 = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO stru_10C360B8 /*0x10c360b8*/.textColor = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO stru_10C360B8 /*0x10c360b8*/.colors4 = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO stru_10C360B8 /*0x10c360b8*/.colors2 = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.textColor = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.colors4 = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.colors2 = (ColorRect*) &unk_102FD188 /*0x102fd188*/;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.shadowColor = (ColorRect*) &dword_102FD178 /*0x102fd178*/;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.shadowColor = (ColorRect*) &dword_102FD178 /*0x102fd178*/;
            // TODO stru_10C34950 /*0x10c34950*/.shadowColor = (ColorRect*) &dword_102FD178 /*0x102fd178*/;
            // TODO stru_10C36060 /*0x10c36060*/.shadowColor = (ColorRect*) &dword_102FD178 /*0x102fd178*/;
            // TODO stru_10C360B8 /*0x10c360b8*/.shadowColor = (ColorRect*) &dword_102FD178 /*0x102fd178*/;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.shadowColor = (ColorRect*) &dword_102FD178 /*0x102fd178*/;
            // TODO stru_10C34950 /*0x10c34950*/.textColor = (ColorRect*) &unk_102FD198 /*0x102fd198*/;
            // TODO stru_10C34950 /*0x10c34950*/.colors4 = (ColorRect*) &unk_102FD198 /*0x102fd198*/;
            // TODO stru_10C34950 /*0x10c34950*/.colors2 = (ColorRect*) &unk_102FD198 /*0x102fd198*/;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.flags = 0;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.field2c = -1;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.field0 = 0;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.kerning = 0;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.leading = 0;
            // TODO chargenSpellsPerDayStyle /*0x10c34a60*/.tracking = 4;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.flags = 0;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.field2c = -1;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.field0 = 0;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.kerning = 0;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.leading = 0;
            // TODO chargenLevelLabelStyle /*0x10c35738*/.tracking = 4;
            // TODO stru_10C34950 /*0x10c34950*/.flags = 0;
            // TODO stru_10C34950 /*0x10c34950*/.field2c = -1;
            // TODO stru_10C34950 /*0x10c34950*/.field0 = 0;
            // TODO stru_10C34950 /*0x10c34950*/.kerning = 0;
            // TODO stru_10C34950 /*0x10c34950*/.leading = 0;
            // TODO stru_10C34950 /*0x10c34950*/.tracking = 4;
            // TODO stru_10C36060 /*0x10c36060*/.flags = 0;
            // TODO stru_10C36060 /*0x10c36060*/.field2c = -1;
            // TODO stru_10C36060 /*0x10c36060*/.textColor = &stru_102FD1B8 /*0x102fd1b8*/;
            // TODO stru_10C36060 /*0x10c36060*/.colors4 = &stru_102FD1B8 /*0x102fd1b8*/;
            // TODO stru_10C36060 /*0x10c36060*/.colors2 = &stru_102FD1B8 /*0x102fd1b8*/;
            // TODO stru_10C36060 /*0x10c36060*/.field0 = 0;
            // TODO stru_10C36060 /*0x10c36060*/.kerning = 0;
            // TODO stru_10C36060 /*0x10c36060*/.leading = 0;
            // TODO stru_10C36060 /*0x10c36060*/.tracking = 4;
            // TODO stru_10C360B8 /*0x10c360b8*/.flags = 0;
            // TODO stru_10C360B8 /*0x10c360b8*/.field2c = -1;
            // TODO stru_10C360B8 /*0x10c360b8*/.field0 = 0;
            // TODO stru_10C360B8 /*0x10c360b8*/.kerning = 0;
            // TODO stru_10C360B8 /*0x10c360b8*/.leading = 0;
            // TODO stru_10C360B8 /*0x10c360b8*/.tracking = 4;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.flags = 0;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.field2c = -1;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.field0 = 0;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.kerning = 0;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.leading = 0;
            // TODO chargenSpellsPerDayStyle_0 /*0x10c361c0*/.tracking = 4;
            // TODO meslineKey = 21000;
            // TODO if (Mesfile_GetLine /*0x101e6760*/(pc_creationMes /*0x11e72ef0*/, &mesline))
            // TODO {
            // TODO     chargenSpellsAvailableTitle /*0x10c36108*/ = (string) meslineValue;
            // TODO     meslineKey = 21001;
            // TODO     if (Mesfile_GetLine /*0x101e6760*/(pc_creationMes /*0x11e72ef0*/, &mesline))
            // TODO     {
            // TODO         chargenSpellsChosenTitle /*0x10c0eef8*/ = (string) meslineValue;
            // TODO         meslineKey = 21002;
            // TODO         if (Mesfile_GetLine /*0x101e6760*/(pc_creationMes /*0x11e72ef0*/, &mesline))
            // TODO         {
            // TODO             chargenSpellsPerDayTitle /*0x10c34e48*/ = (string) meslineValue;
            // TODO             v3 = 21200;
            // TODO             while (1)
            // TODO             {
            // TODO                 meslineKey = v3 - 100;
            // TODO                 if (!Mesfile_GetLine /*0x101e6760*/(pc_creationMes /*0x11e72ef0*/, &mesline))
            // TODO                 {
            // TODO                     break;
            // TODO                 }
// TODO
            // TODO                 v4 = pc_creationMes /*0x11e72ef0*/;
            // TODO                 chargenSpellLevelLabels_0 /*0x10c35720*/[v1] = (int) meslineValue;
            // TODO                 meslineKey = v3;
            // TODO                 if (!Mesfile_GetLine /*0x101e6760*/(v4, &mesline))
            // TODO                 {
            // TODO                     break;
            // TODO                 }
// TODO
            // TODO                 ++v3;
            // TODO                 chargenLevelLabels /*0x10c34938*/[v1] = (int) meslineValue;
            // TODO                 ++v1;
            // TODO                 if ((int) (v3 - 21200) >= 6)
            // TODO                 {
            // TODO                     return ChargenSpellsWidgetsInit /*0x1017fcc0*/(a1.width, a1.height) != 0;
            // TODO                 }
            // TODO             }
            // TODO         }
            // TODO     }
            // TODO }
// TODO
            // TODO return 0;
        }

        [TempleDllLocation(0x1017eae0)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:179")]
        public void Reset(CharEditorSelectionPacket spec)
        {
        }

        [TempleDllLocation(0x101804a0)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:178")]
        public void Activate()
        {
        }

        [TempleDllLocation(0x1017eb80)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:184")]
        public bool CheckComplete()
        {
            return false;
        }

        [TempleDllLocation(0x1017f0a0)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:183")]
        public void Finalize(CharEditorSelectionPacket selPkt, ref GameObjectBody a2)
        {
        }

        [TempleDllLocation(0x1017ebd0)]
        public void ChargenSpellsButtonEntered()
        {
        }
    }
}