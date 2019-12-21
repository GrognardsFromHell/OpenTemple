using System;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.CharSheet.Spells
{
    public class KnownSpellButton : WidgetButtonBase
    {
        private readonly bool _spellOpposesAlignment;

        private readonly SpellStoreData _spell;

        private readonly WidgetText _nameLabel;

        /// <summary>
        /// Invoked when player indicates they want to memorize this spell into a specific slot.
        /// If the spell should be memorized in the best suitable slot, the button argument will be null.
        /// </summary>
        public event Action<SpellStoreData, MemorizedSpellButton> OnMemorizeSpell;

        public KnownSpellButton(Rectangle rect,
            bool spellOpposesAlignment,
            SpellStoreData spell) : base(rect)
        {
            _spellOpposesAlignment = spellOpposesAlignment;
            _spell = spell;

            var spellName = GameSystems.Spell.GetSpellName(spell.spellEnum);

            if (GameSystems.Spell.IsDomainSpell(spell.classCode))
            {
                var domainName = GameSystems.Spell.GetSpellDomainName(spell.classCode);
                spellName += " (" + domainName + ")";
            }

            var style = spellOpposesAlignment ? "char-spell-grey" : "char-spell-body";
            _nameLabel = new WidgetText(spellName, style);
            AddContent(_nameLabel);

            OnMouseEnter += ShowSpellHelp;
            OnMouseExit += HideSpellHelp;

            TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            if (spellOpposesAlignment)
            {
                TooltipText = "#{char_ui_spells:12}";
            }
            else
            {
                TooltipText = GameSystems.Spell.GetSpellName(_spell.spellEnum);
                OnRightClick += (x, y) => OnMemorizeSpell?.Invoke(_spell, null);
            }
        }

        [TempleDllLocation(0x101b78f0)]
        private void DrawSpellNameUnderMouse(int x, int y, object arg)
        {
            Tig.Fonts.PushFont(PredefinedFont.ARIAL_BOLD_10);
            var textStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White));
            textStyle.shadowColor = new ColorRect(PackedLinearColorA.Black);
            textStyle.flags = TigTextStyleFlag.TTSF_TRUNCATE | TigTextStyleFlag.TTSF_DROP_SHADOW;
            textStyle.kerning = 1;
            textStyle.tracking = 5;

            var caster = UiSystems.CharSheet.CurrentCritter;
            if (GameSystems.Spell.GetSchoolSpecialization(caster, out var specializedSchool, out _, out _)
                && GameSystems.Spell.GetSpellSchoolEnum(_spell.spellEnum) == specializedSchool)
            {
                textStyle.textColor = new ColorRect(new PackedLinearColorA(0xFFFFFF80));
            }

            string displayName;
            if (!GameSystems.Spell.IsDomainSpell(_spell.classCode))
            {
                displayName = GameSystems.Spell.GetSpellName(_spell.spellEnum);
            }
            else
            {
                var spellName = GameSystems.Spell.GetSpellName(_spell.spellEnum);
                var domainName = GameSystems.Spell.GetSpellDomainName(_spell.classCode);
                displayName = $"{spellName} ({domainName})";
            }

            var extents = new Rectangle();
            extents.X = x;
            extents.Y = y - Height;
            extents.Width = Width;
            extents.Height = 0;

            Tig.Fonts.RenderText(displayName, extents, textStyle);
            Tig.Fonts.PopFont();
        }

        public override bool HandleMessage(Message msg)
        {
            // No interaction when the spell opposes the caster's alignment
            if (_spellOpposesAlignment)
            {
                return base.HandleMessage(msg);
            }

            void StartDragging()
            {
                _nameLabel.Visible = false;
                Tig.Mouse.SetCursorDrawCallback(DrawSpellNameUnderMouse);
                Globals.UiManager.IsDragging = true;
            }

            void StopDragging()
            {
                _nameLabel.Visible = true;
                Tig.Mouse.SetCursorDrawCallback(null);
                Globals.UiManager.IsDragging = false;
            }

            if (msg.type == MessageType.WIDGET)
            {
                var widgetArgs = msg.WidgetArgs;
                if (widgetArgs.widgetEventType == TigMsgWidgetEvent.Clicked)
                {
                    if (UiSystems.HelpManager.IsSelectingHelpTarget)
                    {
                        var spellHelpTopic = GameSystems.Spell.GetSpellHelpTopic(_spell.spellEnum);
                        GameSystems.Help.ShowTopic(spellHelpTopic);
                    }
                    else
                    {
                        StartDragging();
                    }

                    return true;
                }
                else if (widgetArgs.widgetEventType == TigMsgWidgetEvent.MouseReleased)
                {
                    StopDragging();
                    return true;
                }
                else if (widgetArgs.widgetEventType == TigMsgWidgetEvent.MouseReleasedAtDifferentButton)
                {
                    var otherWidget = Globals.UiManager.GetAdvancedWidgetAt(widgetArgs.x, widgetArgs.y);
                    if (otherWidget is MemorizedSpellButton memorizedSpellButton)
                    {
                        OnMemorizeSpell?.Invoke(_spell, memorizedSpellButton);
                    }

                    StopDragging();
                    return true;
                }
            }

            return base.HandleMessage(msg);
        }

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            // Forward scroll wheel messages to the parent (which will forward it to the scrollbar)
            if (mParent != null && (msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
            {
                return mParent.HandleMouseMessage(msg);
            }

            return base.HandleMouseMessage(msg);
        }

        [TempleDllLocation(0x101b85a0)]
        private void ShowSpellHelp(MessageWidgetArgs obj)
        {
            var helpText = GameSystems.Spell.GetSpellDescription(_spell.spellEnum);
            UiSystems.CharSheet.Help.SetHelpText(helpText);
        }

        private void HideSpellHelp(MessageWidgetArgs obj)
        {
            UiSystems.CharSheet.Help.ClearHelpText();
        }
    }
}