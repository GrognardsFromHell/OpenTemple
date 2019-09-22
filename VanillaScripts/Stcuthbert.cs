
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(173)]
    public class Stcuthbert : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(328, true);
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        DetachScript();

                        obj.BeginDialog(attachee, 1);
                        return RunDefault;
                    }

                }

            }

            return RunDefault;
        }
        public static bool switch_to_iuz(GameObjectBody cuthbert, GameObjectBody pc, int line)
        {
            var iuz = Utilities.find_npc_near(cuthbert, 8042);

            if ((iuz != null))
            {
                pc.BeginDialog(iuz, line);
                iuz.TurnTowards(cuthbert);
                cuthbert.TurnTowards(iuz);
            }
            else
            {
                turn_off_gods(cuthbert, pc);
            }

            return SkipDefault;
        }
        public static bool cuthbert_raise_good(GameObjectBody cuthbert, GameObjectBody pc)
        {
            foreach (var obj in GameSystems.Party.PartyMembers)
            {
                if ((obj.GetStat(Stat.hp_current) <= -10))
                {
                    obj.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");

                    obj.Heal(null, dice);
                    obj.HealSubdual(null, dice);
                }

            }

            return SkipDefault;
        }
        public static bool turn_off_gods(GameObjectBody cuthbert, GameObjectBody pc)
        {
            cuthbert.RemoveFromInitiative();
            cuthbert.SetObjectFlag(ObjectFlag.OFF);
            var iuz = Utilities.find_npc_near(cuthbert, 8042);

            if ((iuz != null))
            {
                iuz.RemoveFromInitiative();
                iuz.SetObjectFlag(ObjectFlag.OFF);
            }

            return SkipDefault;
        }


    }
}
