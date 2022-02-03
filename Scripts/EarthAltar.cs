
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(227)]
    public class EarthAltar : BaseObjectScript
    {
        public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
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
