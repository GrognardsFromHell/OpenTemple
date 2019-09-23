
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
    [ObjectScript(135)]
    public class Nalorrem : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.ClearCritterFlag(CritterFlag.MUTE);
            if ((GetGlobalFlag(105)))
            {
                attachee.Attack(triggerer);
            }
            else if ((Utilities.find_npc_near(attachee, 8027) == null))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetQuestState(46) == QuestState.Unknown))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else
            {
                triggerer.BeginDialog(attachee, 40);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var statue in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_SCENERY))
            {
                if ((statue.GetNameId() == 1618))
                {
                    var loc = statue.GetLocation();
                }

            }

            var spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, loc);
            AttachParticles("sp-Fog Cloud", spell_obj);
            var loc = new locXY(543, 539);
            spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, loc);
            AttachParticles("sp-Fog Cloud", spell_obj);
            loc = new locXY(528, 538);
            spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, loc);
            AttachParticles("sp-Fog Cloud", spell_obj);
            DetachScript();
            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(132, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(132, false);
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(105)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }
        public static bool switch_dialog(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8027);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }

    }
}
