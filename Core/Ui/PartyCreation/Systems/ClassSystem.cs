using System.Collections.Generic;
using System.Text;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f7a14)]
    internal class ClassSystem : IChargenSystem
    {
        public string HelpTopic => "TAG_CHARGEN_CLASS";

        public ChargenStages Stage => ChargenStages.CG_Stage_Class;

        public WidgetContainer Container { get; private set; }

        private readonly SelectionList<Stat?> _classList = new SelectionList<Stat?>();

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x10188910)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:131")]
        public ClassSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/class_ui.json");
            Container = doc.GetRootContainer();
            Container.Visible = false;

            Container.Add(_classList.Container);

            _classList.OnSelectedItemChanged += () =>
            {
                _pkt.classCode = _classList.SelectedItem.GetValueOrDefault();
                UpdateDescriptionBox();
                UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Class);
                UpdateActiveClass();
            };
            _classList.OnItemHovered += classId =>
            {
                if (classId.HasValue)
                {
                    ShowClassHelp(classId.Value);
                }
                else
                {
                    UpdateDescriptionBox();
                }
            };
        }

        private void UpdateActiveClass()
        {
            if (_pkt.classCode == 0)
            {
                _classList.SelectedItem = null;
                return;
            }

            _classList.SelectedItem = _pkt.classCode;
        }

        private void UpdateClassButtons()
        {
            var playerObj = UiSystems.PCCreation.EditedChar;

            _classList.Clear();
            var requirementBuilder = new StringBuilder();
            foreach (var (classId, classSpec) in D20ClassSystem.Classes)
            {
                requirementBuilder.Clear();
                requirementBuilder.Append("Requirements not met:");
                var requirementsNotMet = !AreRequirementsMet(playerObj, classId, requirementBuilder);
                var name = GameSystems.Stat.GetStatName(classSpec.classEnum).ToUpper();
                string tooltip = null;
                if (requirementsNotMet)
                {
                    tooltip = requirementBuilder.ToString();
                }

                _classList.AddItem(name, classId, requirementsNotMet, tooltip);
            }

            UpdateActiveClass();
        }

        private bool AreRequirementsMet(GameObjectBody playerObj, Stat classId, StringBuilder reasonList = null)
        {
            var isValid = true;
            var classSpec = D20ClassSystem.Classes[classId];
            foreach (var requirement in classSpec.Requirements)
            {
                if (!requirement.FullfillsRequirements(playerObj))
                {
                    isValid = false;
                    if (reasonList != null)
                    {
                        reasonList.Append("\n");
                        requirement.DescribeRequirement(reasonList);
                    }
                }
            }

            return isValid;
        }

        [TempleDllLocation(0x101b05d0)]
        public void Reset(CharEditorSelectionPacket selPkt)
        {
            _pkt = selPkt;
            selPkt.classCode = 0;
        }

        [TempleDllLocation(0x101885d0)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:133")]
        public void Activate()
        {
            // TODO chargen.SetIsNewChar(true);
            UpdateClassButtons();
        }

        [TempleDllLocation(0x101b0620)]
        [TemplePlusLocation("ui_char_editor.cpp:3270")]
        public bool CheckComplete()
        {
            return _pkt.classCode != 0;
        }

        [TempleDllLocation(0x10188110)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:137")]
        public void Finalize(CharEditorSelectionPacket selPkt, ref GameObjectBody playerObj)
        {
            playerObj.ClearArray(obj_f.critter_level_idx);
            playerObj.SetInt32(obj_f.critter_level_idx, 0, (int) selPkt.classCode);
            GameSystems.D20.Status.D20StatusRefresh(playerObj);
            GameSystems.Critter.GenerateHp(playerObj);
        }

        [TempleDllLocation(0x10188260)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:139")]
        private void UpdateDescriptionBox()
        {
            if (_pkt.classCode != 0)
            {
                ShowClassHelp(_pkt.classCode);
            }
            else
            {
                UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
            }
        }

        private void ShowClassHelp(Stat classCode)
        {
            var text = GameSystems.Stat.GetClassShortDesc(classCode);
            UiSystems.PCCreation.ShowHelpText(text);
        }

        public bool CompleteForTesting(Dictionary<string, object> props)
        {
            if (props.ContainsKey("class"))
            {
                _pkt.classCode = (Stat) props["class"];
                return true;
            }

            return false;
        }
    }
}