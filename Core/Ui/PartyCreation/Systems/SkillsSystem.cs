using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f7af0)]
    internal class SkillsSystem : IChargenSystem
    {
        public string HelpTopic => "TAG_CHARGEN_SKILLS";

        public ChargenStages Stage => ChargenStages.CG_Stage_Skills;

        public WidgetContainer Container { get; private set; }

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x10c36248)]
        private bool _uiPcCreationSkillsActivated = false;

        [TempleDllLocation(0x10c37218)]
        private SkillId? skillIdxMax;

        [TempleDllLocation(0x10c379b8)]
        [TempleDllLocation(0x10c37910)]
        private readonly List<SkillId> chargenSkills = new List<SkillId>();

        [TempleDllLocation(0x10181b70)]
        public SkillsSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/skills_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            int result;
            string meslineValue;
            int meslineKey;
            TigFontMetrics metrics;

            //  TODO stru_10C371B8 /*0x10c371b8*/.flags = 0x4000;
            //  TODO stru_10C37858 /*0x10c37858*/.flags = 0x4000;
            //  TODO stru_10C379C0 /*0x10c379c0*/.flags = 0x4000;
            //  TODO stru_10C371B8 /*0x10c371b8*/.textColor = (ColorRect*) &unk_102FD3E0 /*0x102fd3e0*/;
            //  TODO stru_10C371B8 /*0x10c371b8*/.colors4 = (ColorRect*) &unk_102FD3E0 /*0x102fd3e0*/;
            //  TODO stru_10C371B8 /*0x10c371b8*/.colors2 = (ColorRect*) &unk_102FD3E0 /*0x102fd3e0*/;
            //  TODO stru_10C379C0 /*0x10c379c0*/.textColor = (ColorRect*) &unk_102FD420 /*0x102fd420*/;
            //  TODO stru_10C379C0 /*0x10c379c0*/.colors4 = (ColorRect*) &unk_102FD420 /*0x102fd420*/;
            //  TODO stru_10C379C0 /*0x10c379c0*/.colors2 = (ColorRect*) &unk_102FD420 /*0x102fd420*/;
            //  TODO stru_10C36910 /*0x10c36910*/.textColor = (ColorRect*) &unk_102FD420 /*0x102fd420*/;
            //  TODO stru_10C36910 /*0x10c36910*/.colors4 = (ColorRect*) &unk_102FD420 /*0x102fd420*/;
            //  TODO stru_10C36910 /*0x10c36910*/.colors2 = (ColorRect*) &unk_102FD420 /*0x102fd420*/;
            //  TODO stru_10C37858 /*0x10c37858*/.textColor = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C37858 /*0x10c37858*/.colors4 = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C37858 /*0x10c37858*/.colors2 = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C37A18 /*0x10c37a18*/.textColor = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C37A18 /*0x10c37a18*/.colors4 = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C37A18 /*0x10c37a18*/.colors2 = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C378C0 /*0x10c378c0*/.textColor = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C378C0 /*0x10c378c0*/.colors4 = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C378C0 /*0x10c378c0*/.colors2 = (ColorRect*) &unk_102FD400 /*0x102fd400*/;
            //  TODO stru_10C37A18 /*0x10c37a18*/.flags = 0x4000;
            //  TODO stru_10C378C0 /*0x10c378c0*/.flags = 0x4000;
            //  TODO stru_10C371B8 /*0x10c371b8*/.field2c = -1;
            //  TODO stru_10C371B8 /*0x10c371b8*/.shadowColor = (ColorRect*) &unk_102FD3F0 /*0x102fd3f0*/;
            //  TODO stru_10C371B8 /*0x10c371b8*/.field0 = 0;
            //  TODO stru_10C371B8 /*0x10c371b8*/.kerning = 1;
            //  TODO stru_10C371B8 /*0x10c371b8*/.leading = 0;
            //  TODO stru_10C371B8 /*0x10c371b8*/.tracking = 3;
            //  TODO stru_10C37858 /*0x10c37858*/.field2c = -1;
            //  TODO stru_10C37858 /*0x10c37858*/.shadowColor = (ColorRect*) &unk_102FD3F0 /*0x102fd3f0*/;
            //  TODO stru_10C37858 /*0x10c37858*/.field0 = 0;
            //  TODO stru_10C37858 /*0x10c37858*/.kerning = 1;
            //  TODO stru_10C37858 /*0x10c37858*/.leading = 0;
            //  TODO stru_10C37858 /*0x10c37858*/.tracking = 3;
            //  TODO stru_10C379C0 /*0x10c379c0*/.field2c = -1;
            //  TODO stru_10C379C0 /*0x10c379c0*/.shadowColor = (ColorRect*) &unk_102FD3F0 /*0x102fd3f0*/;
            //  TODO stru_10C379C0 /*0x10c379c0*/.field0 = 0;
            //  TODO stru_10C379C0 /*0x10c379c0*/.kerning = 1;
            //  TODO stru_10C379C0 /*0x10c379c0*/.leading = 0;
            //  TODO stru_10C379C0 /*0x10c379c0*/.tracking = 3;
            //  TODO stru_10C36910 /*0x10c36910*/.flags = 0x4000;
            //  TODO stru_10C36910 /*0x10c36910*/.field2c = -1;
            //  TODO stru_10C36910 /*0x10c36910*/.shadowColor = (ColorRect*) &unk_102FD3F0 /*0x102fd3f0*/;
            //  TODO stru_10C36910 /*0x10c36910*/.field0 = 0;
            //  TODO stru_10C36910 /*0x10c36910*/.kerning = 1;
            //  TODO stru_10C36910 /*0x10c36910*/.leading = 0;
            //  TODO stru_10C36910 /*0x10c36910*/.tracking = 3;
            //  TODO stru_10C37A18 /*0x10c37a18*/.field2c = -1;
            //  TODO stru_10C37A18 /*0x10c37a18*/.shadowColor = (ColorRect*) &unk_102FD3F0 /*0x102fd3f0*/;
            //  TODO stru_10C37A18 /*0x10c37a18*/.field0 = 0;
            //  TODO stru_10C37A18 /*0x10c37a18*/.kerning = 1;
            //  TODO stru_10C37A18 /*0x10c37a18*/.leading = 0;
            //  TODO stru_10C37A18 /*0x10c37a18*/.tracking = 3;
            //  TODO stru_10C378C0 /*0x10c378c0*/.field2c = -1;
            //  TODO stru_10C378C0 /*0x10c378c0*/.shadowColor = (ColorRect*) &unk_102FD3F0 /*0x102fd3f0*/;
            //  TODO stru_10C378C0 /*0x10c378c0*/.field0 = 0;
            //  TODO stru_10C378C0 /*0x10c378c0*/.kerning = 1;
            //  TODO stru_10C378C0 /*0x10c378c0*/.leading = 0;
            //  TODO stru_10C378C0 /*0x10c378c0*/.tracking = 3;
            //  TODO meslineKey = 20000;
            //  TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
            //  TODO dword_10C36854 /*0x10c36854*/ = (string) meslineValue;
            //  TODO dword_10C36868 /*0x10c36868*/ = &stru_10C371B8 /*0x10c371b8*/;
            //  TODO Tig.Fonts.PushFont(PredefinedFont.PRIORY_12);
            //  TODO metrics.height = 0;
            //  TODO metrics.width = 0;
            //  TODO metrics.text = dword_10C36854 /*0x10c36854*/;
            //  TODO Tig.Fonts.Measure(dword_10C36868 /*0x10c36868*/, &metrics);
            //  TODO stru_10C36C1C /*0x10c36c1c*/.width = metrics.width;
            //  TODO stru_10C36C1C /*0x10c36c1c*/.x = 431 - metrics.width;
            //  TODO stru_10C36C1C /*0x10c36c1c*/.y = 221;
            //  TODO stru_10C36C1C /*0x10c36c1c*/.height = metrics.height;
            //  TODO meslineKey = 20001;
            //  TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
            //  TODO dword_10C379BC /*0x10c379bc*/ = (string) meslineValue;
            //  TODO dword_10C36BF4 /*0x10c36bf4*/ = &stru_10C37858 /*0x10c37858*/;
            //  TODO metrics.height = 0;
            //  TODO metrics.width = 0;
            //  TODO metrics.text = meslineValue;
            //  TODO Tig.Fonts.Measure(&stru_10C37858 /*0x10c37858*/, &metrics);
            //  TODO stru_10C3686C /*0x10c3686c*/.x = 350 - metrics.width;
            //  TODO stru_10C3686C /*0x10c3686c*/.y = 27;
            //  TODO stru_10C3686C /*0x10c3686c*/.width = metrics.width;
            //  TODO stru_10C3686C /*0x10c3686c*/.height = metrics.height;
            //  TODO Tig.Fonts.PopFont();
            //  TODO meslineKey = 20002;
            //  TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
            //  TODO dword_10C378A8 /*0x10c378a8*/ = (string) meslineValue;
            //  TODO dword_10C3624C /*0x10c3624c*/ = &stru_10C378C0 /*0x10c378c0*/;
            //  TODO meslineKey = 20003;
            //  TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
            //  TODO dword_10C36C60 /*0x10c36c60*/ = (string) meslineValue;
            //  TODO dword_10C36850 /*0x10c36850*/ = &stru_10C378C0 /*0x10c378c0*/;
            //  TODO meslineKey = 20004;
            //  TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
            //  TODO dword_10C36BF8 /*0x10c36bf8*/ = (string) meslineValue;
            //  TODO dword_10C377E4 /*0x10c377e4*/ = &stru_10C378C0 /*0x10c378c0*/;
            //  TODO meslineKey = 20005;
            //  TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
            //  TODO dword_10C3684C /*0x10c3684c*/ = (string) meslineValue;
            //  TODO dword_10C36C88 /*0x10c36c88*/ = &stru_10C37858 /*0x10c37858*/;
            //  TODO if (RegisterUiTexture /*0x101ee7b0*/("art\\interface\\pc_creation\\SkillId.breakdown.tga",
            //  TODO         &dword_10C36C3C /*0x10c36c3c*/)
            //  TODO     || RegisterUiTexture /*0x101ee7b0*/("art\\interface\\pc_creation\\SkillId.availbox.tga",
            //  TODO         &dword_10C36C84 /*0x10c36c84*/)
            //  TODO     || RegisterUiTexture /*0x101ee7b0*/("art\\interface\\pc_creation\\SkillId.buttonbox.tga",
            //  TODO         &dword_10C37A68 /*0x10c37a68*/))
            //  TODO {
            //  TODO     result = 0;
            //  TODO }
            //  TODO else
            //  TODO {
            //  TODO     result = UiPcCreationSkillWidgetsInit /*0x10181700*/(a1.width, a1.height) != 0;
            //  TODO }
//  TODO
            //  TODO return result;
        }

        [TempleDllLocation(0x10180630)]
        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            _pkt.skillPointsAdded.Clear();
            for (SkillId skillIdx = default; skillIdx < SkillId.count; skillIdx++)
            {
                _pkt.skillPointsAdded[skillIdx] = 0;
            }
            _uiPcCreationSkillsActivated = false;
        }

        [TempleDllLocation(0x10181380)]
        public void Activate()
        {
            skillIdxMax = null;
            chargenSkills.Clear();
            for (SkillId skillIdx = default; skillIdx < SkillId.count; skillIdx++)
            {
                if (GameSystems.Skill.IsEnabled(skillIdx))
                {
                    chargenSkills.Add(skillIdx);
                }

                ++skillIdx;
            }
            chargenSkills.Sort(GameSystems.Skill.SkillNameComparer);

            // j_WidgetCopy /*0x101f87a0*/(uiPcCreationSkillsScrollbarId /*0x10c37a14*/,
                // (LgcyWidget*) &uiPcCreationSkillsScrollbar /*0x10c36780*/);
            // uiPcCreationSkillsScrollbar /*0x10c36780*/.yMax = chargenSkillsAvailableCount /*0x10c379b8*/ - 7;
            // j_ui_widget_set /*0x101f87b0*/(uiPcCreationSkillsScrollbarId /*0x10c37a14*/,
                // &uiPcCreationSkillsScrollbar /*0x10c36780*/);
            if (!_uiPcCreationSkillsActivated)
            {
                _pkt.skillPointsSpent = 0;
                var points = 4 * D20ClassSystem.GetSkillPoints(UiSystems.PCCreation.EditedChar, _pkt.classCode);
                _pkt.availableSkillPoints = points;
                if (_pkt.raceId == RaceId.human)
                {
                    _pkt.availableSkillPoints += 4;
                }
                if (_pkt.availableSkillPoints < 4)
                {
                    _pkt.availableSkillPoints = 4;
                }
            }

            // TODO UiPcCreationSkillTextDraw /*0x10180eb0*/();
            _uiPcCreationSkillsActivated = true;
        }

        [TempleDllLocation(0x10180690)]
        public bool CheckComplete()
        {
            return _uiPcCreationSkillsActivated && _pkt.availableSkillPoints == _pkt.skillPointsSpent;
        }

        [TempleDllLocation(0x101806b0)]
        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            foreach (var (skillId, amount) in charSpec.skillPointsAdded)
            {
                GameSystems.Skill.AddSkillRanks(playerObj, skillId, amount);
            }
        }

        private void ShowSkillHelp(SkillId skillId)
        {
            var helpTopic = GameSystems.Skill.GetHelpTopic(skillId);
            UiSystems.PCCreation.ShowHelpTopic(helpTopic);
        }

        [TempleDllLocation(0x101806f0)]
        private void UpdateDescriptionBox()
        {
            if (skillIdxMax.HasValue)
            {
                ShowSkillHelp(skillIdxMax.Value);
            }
            else
            {
                UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
            }
        }

    }
}