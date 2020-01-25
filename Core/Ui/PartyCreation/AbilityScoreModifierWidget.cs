using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation
{
    public class AbilityScoreModifierWidget : WidgetBase
    {
        private const string LabelStyle = "charGenAssignedStatMod";

        private readonly Func<int> _assignedValueGetter;

        private readonly WidgetText _label;

        public AbilityScoreModifierWidget(Size size, Func<int> assignedValueGetter)
        {
            _assignedValueGetter = assignedValueGetter;
            SetSize(size);
            AddContent(new WidgetRectangle
            {
                Pen = new PackedLinearColorA(0xFF43586E)
            });
            _label = new WidgetText("", LabelStyle);
            AddContent(_label);
        }

        public override void Render()
        {
            var currentStatValue = _assignedValueGetter();
            if (currentStatValue == -1)
            {
                _label.Visible = false;
            }
            else
            {
                _label.Visible = true;
                var modifier = D20StatSystem.GetModifierForAbilityScore(currentStatValue);
                if (modifier > 0)
                {
                    _label.SetText("+" + modifier);
                }
                else
                {
                    _label.SetText(modifier.ToString());
                }
            }

            base.Render();
        }
    }
}