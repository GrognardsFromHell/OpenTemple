using System;
using System.Drawing;
using SharpDX.DirectWrite;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.Dialog
{
    public class DialogResponseButton : WidgetButtonBase
    {

        private static TigTextStyle CreateTextStyle(PackedLinearColorA color) => new TigTextStyle(new ColorRect(color))
        {
            kerning = 1,
            tracking = 3
        };

        private static readonly TigTextStyle NormalStyle = CreateTextStyle(new PackedLinearColorA(0xFF00FF00));
        private static readonly TigTextStyle HoverStyle = CreateTextStyle(new PackedLinearColorA(0xFF99FF99));
        private static readonly TigTextStyle PressedStyle = CreateTextStyle(new PackedLinearColorA(0xFFFFFFFF));

        private static readonly TigTextStyle SkillNormalStyle = CreateTextStyle(new PackedLinearColorA(0xFFE4CE3E));
        private static readonly TigTextStyle SkillHoverStyle = CreateTextStyle(new PackedLinearColorA(0xFFFEFE19));
        private static readonly TigTextStyle SkillPressedStyle = CreateTextStyle(new PackedLinearColorA(0xFFFEFF00));

        private readonly WidgetLegacyText _numberLabel;

        private readonly WidgetLegacyText _label;

        private readonly DialogSkill _skillUsed;

        public DialogResponseButton(Rectangle rect, string numberText, string text, DialogSkill skillUsed) : base(rect)
        {
            _skillUsed = skillUsed;

            _label = new WidgetLegacyText(text, DialogUi.Font, NormalStyle);
            _label.SetX(36);
            _label.SetFixedWidth(559);
            AddContent(_label);

            _numberLabel = new WidgetLegacyText(numberText, DialogUi.Font, NormalStyle);
            _numberLabel.SetX(17);
            AddContent(_numberLabel);

            if (skillUsed != DialogSkill.None)
            {
                var icon = new WidgetImage(GetSkillTexture(skillUsed));
                icon.FixedSize = new Size(15, 15);
                icon.SetX(2);
                icon.SetY(1);
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


        private TigTextStyle GetTextStyle()
        {
            // Update text style based on button state
            switch (ButtonState)
            {
                case LgcyButtonState.Hovered:
                    return _skillUsed == DialogSkill.None ? HoverStyle : SkillHoverStyle;
                case LgcyButtonState.Down:
                    return _skillUsed == DialogSkill.None ? PressedStyle : SkillPressedStyle;
                default:
                    return _skillUsed == DialogSkill.None ? NormalStyle : SkillNormalStyle;
            }
        }

        [TempleDllLocation(0x1014c520)]
        public override void Render()
        {
            var textStyle = GetTextStyle();
            _label.TextStyle = textStyle;
            _numberLabel.TextStyle = textStyle;
            base.Render();
        }
    }
}