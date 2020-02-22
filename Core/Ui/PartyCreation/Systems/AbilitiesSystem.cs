using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f7a98)]
    internal class AbilitiesSystem : IChargenSystem
    {
        private readonly Dictionary<Stat, IChargenSystem> _featuresByClass = new Dictionary<Stat, IChargenSystem>
        {
            {Stat.level_cleric, new ClericFeaturesUi()},
            {Stat.level_wizard, new WizardFeaturesUi()},
            {Stat.level_ranger, new RangerFeaturesUi()}
        };

        private IChargenSystem _activeFeaturesUi;

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x10c3d0f0)]
        private bool uiChargenAbilitiesActivated;

        [TempleDllLocation(0x10186f90)]
        public AbilitiesSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/abilities_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            foreach (var featuresUi in _featuresByClass.Values)
            {
                Container.Add(featuresUi.Container);
            }
        }

        public string HelpTopic => "TAG_CHARGEN_ABILITIES";

        public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

        public WidgetContainer Container { get; }

        [TempleDllLocation(0x10184b90)]
        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            foreach (var featureUi in _featuresByClass.Values)
            {
                featureUi.Reset(pkt);
            }

            uiChargenAbilitiesActivated = false;
        }

        [TempleDllLocation(0x10185e10)]
        public void Activate()
        {
            uiChargenAbilitiesActivated = true;
        }

        [TempleDllLocation(0x10185670)]
        public void Show()
        {
            Container.Show();

            _featuresByClass.TryGetValue(_pkt.classCode, out _activeFeaturesUi);
            _activeFeaturesUi?.Show();
        }

        public void Hide()
        {
            if (_activeFeaturesUi != null)
            {
                _activeFeaturesUi.Container.Visible = false;
                _activeFeaturesUi = null;
            }

            Container.Hide();
        }

        [TempleDllLocation(0x10184bd0)]
        public bool CheckComplete()
        {
            return _activeFeaturesUi?.CheckComplete() ?? true;
        }

        [TempleDllLocation(0x10184c80)]
        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            _activeFeaturesUi?.Finalize(charSpec, ref playerObj);
        }

        [TempleDllLocation(0x10184c40)]
        public void UpdateDescriptionBox()
        {
            if (_pkt.classCode == Stat.level_wizard)
            {
                UiSystems.PCCreation.ShowHelpTopic("TAG_HMU_CHAR_EDITOR_WIZARD_SPEC");
            }
            else if (_pkt.classCode == Stat.level_cleric)
            {
                UiSystems.PCCreation.ShowHelpTopic("");
            }
        }
    }

    internal class ClericFeaturesUi : IChargenSystem
    {
        private readonly List<WidgetButton> _availableDomainButtons = new List<WidgetButton>();

        private readonly WidgetText _draggedDomainLabel;

        [TempleDllLocation(0x10c3c4e0)]
        private readonly List<DomainId> _selectableDomains = new List<DomainId>();

        private readonly WidgetButton _selectedDomain1;

        private readonly WidgetButton _selectedDomain2;

        private readonly WidgetText _worshipsLabel;

        private CharEditorSelectionPacket _pkt;

        public ClericFeaturesUi()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/abilities_cleric_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            _worshipsLabel = doc.GetTextContent("worshipsLabel");

            for (var i = 0; i < 5; i++)
            {
                var index = i;
                var button = doc.GetButton($"deityDomain{i + 1}");
                button.SetMouseMsgHandler(msg => DeityDomainMouseHandler(msg, button, index));
                _availableDomainButtons.Add(button);
            }

            _selectedDomain1 = doc.GetButton("selectedDomain1");
            _selectedDomain2 = doc.GetButton("selectedDomain2");
            _draggedDomainLabel = new WidgetText("", "domainSelectionText");
        }

        public WidgetContainer Container { get; }

        public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

        public string HelpTopic { get; }

        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            _pkt.domain1 = 0;
            _pkt.domain2 = 0;
        }

        public void Show()
        {
            Container.Visible = true;

            var playerObj = UiSystems.PCCreation.EditedChar;
            var deityId = _pkt.deityId.GetValueOrDefault();
            var alignment = playerObj.GetAlignment();
            if (GameSystems.Deity.GetAlignmentChoice(deityId, alignment) != AlignmentChoice.Undecided)
            {
                // TODO WidgetSetHidden /*0x101f9100*/(chargenAbilitiesSomeWidId_10C3DD94 /*0x10c3dd94*/, 1);
                // TODO WidgetSetHidden /*0x101f9100*/(dword_10C3E740 /*0x10c3e740*/, 1);
            }

            GetDeityDomains();
            UpdateButtons();
        }

        public bool CheckComplete()
        {
            if (_pkt.domain1 != DomainId.None && _pkt.domain2 != DomainId.None)
            {
                return _pkt.alignmentChoice != AlignmentChoice.Undecided;
            }

            return false;
        }

        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            playerObj.SetInt32(obj_f.critter_domain_1, (int) charSpec.domain1);
            playerObj.SetInt32(obj_f.critter_domain_2, (int) charSpec.domain2);
            playerObj.SetInt32(obj_f.critter_alignment_choice, (int) charSpec.alignmentChoice);
        }

        private bool DeityDomainMouseHandler(MessageMouseArgs msg, WidgetButton widget, int index)
        {
            var domain = _selectableDomains[index];

            if (Globals.UiManager.GetMouseCaptureWidget() == widget)
            {
                if ((msg.flags & MouseEventFlag.LeftReleased) != 0)
                {
                    Tig.Mouse.SetCursorDrawCallback(null);
                    Globals.UiManager.UnsetMouseCaptureWidget(widget);
                    widget.Visible = true;

                    var widgetUnderCursor = Globals.UiManager.GetWidgetAt(msg.X, msg.Y);
                    if (widgetUnderCursor == _selectedDomain1)
                    {
                        RemoveSelectedDomain(domain);
                        _pkt.domain1 = domain;
                        OnDomainsChanged();
                    }
                    else if (widgetUnderCursor == _selectedDomain2)
                    {
                        RemoveSelectedDomain(domain);
                        _pkt.domain2 = domain;
                        OnDomainsChanged();
                    }
                }

                return true;
            }

            if (IsDomainSelected(domain))
            {
                // Do not allow interaction with unassigned ability scores
                return true;
            }

            // Allow quickly swapping values between the two columns, but only when we actually have rolled values
            // (not in point buy mode)
            if ((msg.flags & MouseEventFlag.RightClick) != 0)
            {
                if (_pkt.domain1 == DomainId.None)
                {
                    _pkt.domain1 = domain;
                    OnDomainsChanged();
                    return true;
                }

                if (_pkt.domain2 == DomainId.None)
                {
                    _pkt.domain2 = domain;
                    OnDomainsChanged();
                    return true;
                }
            }
            else if ((msg.flags & MouseEventFlag.LeftClick) != 0)
            {
                if (!Globals.UiManager.SetMouseCaptureWidget(widget))
                {
                    // Something else has the mouse capture right now (how are we getting this message then...?)
                    return true;
                }

                // Figure out where in the widget we got clicked so we can draw the dragged text with the proper offset
                var globalContentArea = widget.GetContentArea(true);
                var localX = msg.X - globalContentArea.X;
                var localY = msg.Y - globalContentArea.Y;
                _draggedDomainLabel.SetText(widget.GetText());
                widget.Visible = false;

                // This will draw the ability score being dragged under the mouse cursor
                Tig.Mouse.SetCursorDrawCallback((x, y, arg) =>
                {
                    var point = new Point(x, y);
                    point.Offset(-localX, -localY);
                    var contentArea = new Rectangle(point, widget.GetSize());

                    _draggedDomainLabel.SetContentArea(contentArea);
                    _draggedDomainLabel.Render();
                });
            }

            return true;
        }

        private bool IsDomainSelected(DomainId domain)
        {
            return _pkt.domain1 == domain || _pkt.domain2 == domain;
        }

        private void RemoveSelectedDomain(DomainId domain)
        {
            if (_pkt.domain1 == domain)
            {
                _pkt.domain1 = DomainId.None;
            }

            if (_pkt.domain2 == domain)
            {
                _pkt.domain2 = DomainId.None;
            }
        }

        private void OnDomainsChanged()
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (_pkt.domain1 != DomainId.None)
            {
                _selectedDomain1.SetText(GameSystems.Spell.GetDomainName(_pkt.domain1));
            }
            else
            {
                _selectedDomain1.SetText("");
            }

            if (_pkt.domain2 != DomainId.None)
            {
                _selectedDomain2.SetText(GameSystems.Spell.GetDomainName(_pkt.domain2));
            }
            else
            {
                _selectedDomain2.SetText("");
            }

            for (var i = 0; i < _availableDomainButtons.Count; i++)
            {
                var button = _availableDomainButtons[i];
                if (i >= _selectableDomains.Count)
                {
                    button.Visible = false;
                    continue;
                }

                button.Visible = true;
                var domain = _selectableDomains[i];
                button.SetText(GameSystems.Spell.GetDomainName(domain));
                if (IsDomainSelected(domain))
                {
                    button.SetStyle("deityDomainButtonDisabled");
                }
                else
                {
                    button.SetStyle("deityDomainButton");
                }
            }
        }

        [TempleDllLocation(0x10184da0)]
        private void GetDeityDomains()
        {
            var deityId = _pkt.deityId.GetValueOrDefault();
            var deityName = GameSystems.Deity.GetName(deityId);
            _worshipsLabel.SetText("@1#{pc_creation:18000}@0 " + deityName);

            _selectableDomains.Clear();
            _selectableDomains.AddRange(GameSystems.Deity.GetDomains(deityId));
        }
    }

    internal class WizardFeaturesUi : IChargenSystem
    {
        private CharEditorSelectionPacket _pkt;

        public WizardFeaturesUi()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/abilities_wizard_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;
        }

        public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

        public string HelpTopic { get; }

        public WidgetContainer Container { get; }

        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            _pkt.wizSchool = 0;
            _pkt.forbiddenSchool1 = 0;
            _pkt.forbiddenSchool2 = 0;
        }

        public bool CheckComplete()
        {
            if (_pkt.wizSchool == SchoolOfMagic.None)
            {
                return true;
            }

            if (_pkt.forbiddenSchool1 != SchoolOfMagic.None)
            {
                // Divination only needs to pick a single forbidden school
                return _pkt.wizSchool == SchoolOfMagic.Divination
                       || _pkt.forbiddenSchool2 != SchoolOfMagic.None;
            }

            return false;
        }

        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            GameSystems.Spell.SetSchoolSpecialization(
                playerObj,
                charSpec.wizSchool,
                charSpec.forbiddenSchool1,
                charSpec.forbiddenSchool2
            );
        }
    }

    internal class RangerFeaturesUi : IChargenSystem
    {
        private CharEditorSelectionPacket _pkt;

        public RangerFeaturesUi()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/abilities_ranger_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;
        }

        public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;

        public WidgetContainer Container { get; }

        public string HelpTopic { get; }

        public void Reset(CharEditorSelectionPacket pkt)
        {
            _pkt = pkt;
            _pkt.feat3 = null;
        }

        public bool CheckComplete()
        {
            return _pkt.feat3.HasValue;
        }

        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
        {
            Trace.Assert(charSpec.feat3.HasValue);
            GameSystems.Feat.AddFeat(playerObj, charSpec.feat3.Value);
            GameSystems.D20.Status.D20StatusRefresh(playerObj);
        }
    }
}