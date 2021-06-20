using System;
using System.Collections.Immutable;
using System.Drawing;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Dialog
{
    public class DialogResponseButton : WidgetButtonBase
    {

        private const string NormalStyle = "dialog-ui-button-normal";
        private const string HoverStyle = "dialog-ui-button-hover";
        private const string PressedStyle = "dialog-ui-button-pressed";

        private const string SkillNormalStyle = "dialog-ui-button-skill-normal";
        private const string SkillHoverStyle = "dialog-ui-button-skill-hover";
        private const string SkillPressedStyle = "dialog-ui-button-skill-pressed";

        private readonly WidgetText _numberLabel;

        private readonly WidgetText _label;

        private readonly DialogSkill _skillUsed;

        public DialogResponseButton(Rectangle rect, string numberText, string text, DialogSkill skillUsed) : base(rect)
        {
            AddStyle("dialog-ui-text");

            _skillUsed = skillUsed;

            _label = new WidgetText(text, NormalStyle);
            _label.X = 36;
            _label.FixedWidth = 559;
            AddContent(_label);

            _numberLabel = new WidgetText(numberText, NormalStyle);
            _numberLabel.X = 17;
            AddContent(_numberLabel);

            if (skillUsed != DialogSkill.None)
            {
                var icon = new WidgetImage(GetSkillTexture(skillUsed));
                icon.FixedSize = new Size(15, 15);
                icon.X = 2;
                icon.Y = 1;
                AddContent(icon);
            }
        }

        private string GetSkillTexture(DialogSkill skill)
        {
            switch (skill)
            {
                case DialogSkill.Bluff:
                    return "art/interface/DIALOG/Icon-Bluff.tga";
                case DialogSkill.Diplomacy:
                    return "art/interface/DIALOG/Icon-Diplomacy.tga";
                case DialogSkill.Intimidate:
                    return "art/interface/DIALOG/Icon-Intimidate.tga";
                case DialogSkill.SenseMotive:
                    return "art/interface/DIALOG/Icon-Sense Motive.tga";
                case DialogSkill.GatherInformation:
                    return "art/interface/DIALOG/Icon-Gather Info.tga";
                default:
                    throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
            }
        }


        private IImmutableList<string> GetTextStyles()
        {
            // Update text style based on button state
            var styleId = ButtonState switch
            {
                LgcyButtonState.Hovered => _skillUsed == DialogSkill.None ? HoverStyle : SkillHoverStyle,
                LgcyButtonState.Down => _skillUsed == DialogSkill.None ? PressedStyle : SkillPressedStyle,
                _ => _skillUsed == DialogSkill.None ? NormalStyle : SkillNormalStyle
            };
            return ImmutableList.Create(styleId);
        }

        [TempleDllLocation(0x1014c520)]
        public override void Render()
        {
            var textStyles = GetTextStyles();
            _label.StyleIds = textStyles;
            _numberLabel.StyleIds = textStyles;
            base.Render();
        }
    }
}