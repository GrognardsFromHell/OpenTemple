using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation
{
    public class StatBlockAbilityScore
    {
        public WidgetContainer Container { get; }

        private readonly WidgetText _caption;
        private readonly WidgetContent _captionBg;

        private readonly WidgetText _modifierLabel;
        private readonly WidgetContent _modifierBorder;

        private readonly WidgetText _valueLabel;
        private readonly WidgetContent _valueBorder;

        public bool IsActive
        {
            set
            {
                _caption.Visible = value;
                _captionBg.Visible = value;
                _modifierLabel.Visible = value;
                _modifierBorder.Visible = value;
                _valueLabel.Visible = value;
                _valueBorder.Visible = value;
            }
        }

        public int AbilityScore
        {
            set
            {
                _valueLabel.SetText(value.ToString());
                var modifier = D20StatSystem.GetModifierForAbilityScore(value);
                _modifierLabel.SetText($"{modifier:+#;-#;0}");
            }
        }

        public StatBlockAbilityScore(Stat ability)
        {
            var doc = WidgetDoc.Load("ui/pc_creation/stat_block_ability_score.json");
            Container = doc.TakeRootContainer();

            _caption = doc.GetTextContent("caption");
            _caption.SetText(GameSystems.Stat.GetStatShortName(ability));
            _captionBg = doc.GetContent("activeCaptionBg");
            _modifierLabel = doc.GetTextContent("modifierLabel");
            _modifierBorder = doc.GetContent("activeModifierBorder");
            _valueLabel = doc.GetTextContent("valueLabel");
            _valueBorder = doc.GetContent("activeValueBorder");
        }
    }
}