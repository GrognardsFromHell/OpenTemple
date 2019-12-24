
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [ObjectScript(581)]
    public class ProtectChest : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5066)) // temple level 1 - Earth altar chests
            {
                var attacking_temp = 0;
                foreach (var npc in ObjList.ListVicinity(new locXY(484, 400), ObjectListFilter.OLC_NPC))
                {
                    if ((new[] { 14337, 14381 }).Contains(npc.GetNameId()) && npc.GetLeader() == null) // earth temple guards, elementals
                    {
                        if (ScriptDaemon.can_see2(npc, triggerer))
                        {
                            npc.Attack(SelectedPartyLeader);
                            attacking_temp = 1;
                        }

                    }

                }

                foreach (var npc in ObjList.ListVicinity(new locXY(484, 424), ObjectListFilter.OLC_NPC))
                {
                    if ((new[] { 14337, 14381, 14296 }).Contains(npc.GetNameId()) && npc.GetLeader() == null) // earth temple guards, elementals
                    {
                        if (ScriptDaemon.can_see2(npc, triggerer))
                        {
                            attacking_temp = 1;
                        }

                    }

                }

                if (attacking_temp == 1)
                {
                    UiSystems.CharSheet.Hide();
                    foreach (var npc in ObjList.ListVicinity(new locXY(484, 400), ObjectListFilter.OLC_NPC))
                    {
                        if ((new[] { 14337, 14381 }).Contains(npc.GetNameId()) && npc.GetLeader() == null) // earth temple guards, elementals
                        {
                            npc.Attack(SelectedPartyLeader);
                        }

                    }

                    foreach (var npc in ObjList.ListVicinity(new locXY(484, 424), ObjectListFilter.OLC_NPC))
                    {
                        if ((new[] { 14337, 14381, 14296 }).Contains(npc.GetNameId()) && npc.GetLeader() == null) // earth temple guards, elementals
                        {
                            npc.Attack(SelectedPartyLeader);
                        }

                    }

                    return SkipDefault;
                }
                else
                {
                    return RunDefault;
                }

            }

            if ((attachee.GetMap() == 5115))
            {
                var npc = Utilities.find_npc_near(attachee, 8803);
                if ((npc != null))
                {
                    npc.TurnTowards(triggerer);
                    npc.Attack(triggerer);
                }

            }
            else if ((attachee.GetMap() == 5191))
            {
                var npc = Utilities.find_npc_near(attachee, 14472);
                if ((npc != null))
                {
                    npc.TurnTowards(triggerer);
                    npc.Attack(triggerer);
                }

            }

            DetachScript();
            return SkipDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if (((spell.spellEnum == WellKnownSpells.Knock) || (spell.spellEnum == WellKnownSpells.OpenClose)))
            {
                if ((attachee.GetMap() == 5115))
                {
                    var npc = Utilities.find_npc_near(attachee, 8803);
                    if ((npc != null))
                    {
                        npc.TurnTowards(triggerer);
                        npc.Attack(triggerer);
                    }

                }
                else if ((attachee.GetMap() == 5191))
                {
                    var npc = Utilities.find_npc_near(attachee, 14472);
                    if ((npc != null))
                    {
                        npc.TurnTowards(triggerer);
                        npc.Attack(triggerer);
                    }

                }

            }

            return RunDefault;
        }

    }
}
