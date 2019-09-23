
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
    [ObjectScript(201)]
    public class Ikian : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        obj.BeginDialog(attachee, 1);
                        DetachScript();
                        return RunDefault;
                    }

                }

            }

            return RunDefault;
        }
        public static bool all_run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetLeader() == null && !((SelectedPartyLeader.GetPartyMembers()).Contains(obj))))
                {
                    obj.RunOff();
                }

            }

            return RunDefault;
        }
        public static bool buff_npc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(761, GetGlobalVar(761) + 1);
            if ((GetGlobalVar(761) == 1))
            {
                attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14333 && obj.GetLeader() == null))
                    {
                        obj.CastSpell(WellKnownSpells.MageArmor, obj);
                    }

                    if ((obj.GetNameId() == 14336 && obj.GetLeader() == null))
                    {
                        obj.CastSpell(WellKnownSpells.ResistElements, obj);
                    }

                }

            }

            if ((GetGlobalVar(761) == 2))
            {
                attachee.CastSpell(WellKnownSpells.OwlsWisdom, attachee);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14333 && obj.GetLeader() == null))
                    {
                        obj.CastSpell(WellKnownSpells.MirrorImage, obj);
                    }

                }

            }

            if ((GetGlobalVar(761) >= 3))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14333 && obj.GetLeader() == null))
                    {
                        obj.CastSpell(WellKnownSpells.Shield, obj);
                    }

                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14334 && obj.GetLeader() == null))
                    {
                        attachee.CastSpell(WellKnownSpells.EndureElements, obj);
                    }

                }

            }

            return RunDefault;
        }

    }
}
