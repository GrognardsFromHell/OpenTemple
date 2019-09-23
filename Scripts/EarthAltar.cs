
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(227)]
    public class EarthAltar : BaseObjectScript
    {
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((triggerer.GetNameId() == 1203) && (!GetGlobalFlag(109))))
            {
                ScriptDaemon.record_time_stamp(512);
                UiSystems.CharSheet.Hide();
                attachee.Destroy();
                SetGlobalFlag(109, true);
                AttachParticles("DesecrateEarth", triggerer);
                foreach (var npc in ObjList.ListVicinity(new locXY(484, 400), ObjectListFilter.OLC_NPC))
                {
                    if ((new[] { 14337, 14381 }).Contains(npc.GetNameId()) && npc.GetLeader() == null) // earth temple guards
                    {
                        npc.Attack(SelectedPartyLeader);
                    }

                }

                foreach (var npc in ObjList.ListVicinity(new locXY(484, 424), ObjectListFilter.OLC_NPC))
                {
                    if ((new[] { 14337, 14381, 14296 }).Contains(npc.GetNameId()) && npc.GetLeader() == null) // earth temple guards
                    {
                        npc.Attack(SelectedPartyLeader);
                    }

                }

            }

            return RunDefault;
        }

    }
}
