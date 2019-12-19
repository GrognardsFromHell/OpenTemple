using System;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Party
{
    public class PortraitHealthBar : WidgetButtonBase
    {
        private readonly WidgetImage _fillImage;

        private readonly WidgetImage _hurtImage;

        private readonly WidgetImage _subdualImage;

        private readonly GameObjectBody _partyMember;

        public PortraitHealthBar(GameObjectBody partyMember, Rectangle rectangle) : base(rectangle)
        {
            _partyMember = partyMember;

            var background = new WidgetImage("art/interface/PARTY_UI/Health Bar-empty.tga");
            AddContent(background);

            _fillImage = new WidgetImage("art/interface/PARTY_UI/Health Bar.tga");
            AddContent(_fillImage);

            _subdualImage = new WidgetImage("art/interface/PARTY_UI/subdual_meter_off.tga");
            AddContent(_subdualImage);

            SetClickHandler(ToggleShowHp);

            // Previously @ 0x10132240
            TooltipStyle = UiSystems.Tooltip.DefaultStyle;
            TooltipText = UiSystems.Tooltip.GetString(6038);
        }

        [TempleDllLocation(0x101324f0)]
        [TempleDllLocation(0x10132490)]
        private void ToggleShowHp()
        {
            if ( UiSystems.HelpManager.IsSelectingHelpTarget )
            {
                UiSystems.HelpManager.ShowPredefinedTopic(41);
            }
            else
            {
                GameSystems.Party.ShowHitPoints = !GameSystems.Party.ShowHitPoints;
                // TODO GameConfigSetInt/*0x100871c0*/(&cfgFile/*0x11e726a0*/, "draw_hp", v4);
            }
        }

        [TempleDllLocation(0x10132f70)]
        public override void Render()
        {
            var maxHp = _partyMember.GetStat(Stat.hp_max);
            if (maxHp > 0)
            {
                var currentHp = _partyMember.GetStat(Stat.hp_current);
                var subdualDamage = _partyMember.GetStat(Stat.subdual_damage);
                var healthFraction = (float) currentHp / maxHp;

                var fillWidth = Math.Min(Width, (int) (Width * healthFraction));
                _fillImage.Visible = fillWidth > 0;
                _fillImage.SetFixedWidth(fillWidth);

                UpdateSubdualDamage(subdualDamage, maxHp);
            }
            else
            {
                _fillImage.Visible = false;
                _subdualImage.Visible = false;
            }

            base.Render();
        }

        private void UpdateSubdualDamage(int subdualDamage, int maxHp)
        {
            if (subdualDamage > 0 && maxHp > 0)
            {
                var width = (int) ((subdualDamage / (float) maxHp) * Width);
                width = Math.Min(Width, width);

                _subdualImage.Visible = true;
                _subdualImage.SetFixedWidth(width);
            }
            else
            {
                _subdualImage.Visible = false;
            }
        }
    }
}