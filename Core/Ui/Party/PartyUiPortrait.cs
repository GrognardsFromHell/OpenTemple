using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Party
{
    public class PartyUiPortrait : IDisposable
    {
        public GameObjectBody PartyMember { get; }

        public WidgetContainer Widget { get; }

        public PartyUiPortrait(GameObjectBody partyMember, WidgetContainer widget)
        {
            PartyMember = partyMember;
            Widget = widget;
        }

        public void Dispose()
        {
            Widget.Dispose();
        }

        public int Field0 { get; set; } = -1;

        public int Flags { get; set; }

        public int Field8 { get; set; }

        public bool IsActive { get; set; }

        public int PartyUiMain { get; set; }

        public int PartyUiPortraitButton { get; set; }

        public int HpButton { get; set; }

        public int SubdualButton { get; set; }

        public List<int> Buffs { get; set; } = new List<int>();

        public List<int> Ailments { get; set; } = new List<int>();

        public List<int> Conditions { get; set; } = new List<int>();

        public int DismissButton { get; set; }

        public int LevelUpButton { get; set; }

        public int BuffDebuffPkt { get; set; }

    }
}