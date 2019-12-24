
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
    [ObjectScript(17)]
    public class ProsperousYoungerD : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 140); // meleny in party
            }
            else if ((GetGlobalVar(904) == 32))
            {
                triggerer.BeginDialog(attachee, 360); // have attacked 3 or more farm animals with meleny in party
            }
            else if ((GetQuestState(7) == QuestState.Completed))
            {
                if ((GetGlobalFlag(46)))
                {
                    triggerer.BeginDialog(attachee, 150); // have completed flirting with disaster quest and married meleny
                }
                else
                {
                    triggerer.BeginDialog(attachee, 120); // have completed flirting with disaster quest
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1); // none of the above
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(196, true);
            if ((!GetGlobalFlag(232)))
            {
                attachee.FloatLine(12014, triggerer);
                SetGlobalVar(23, GetGlobalVar(23) + 1);
                if (GetGlobalVar(23) >= 2)
                {
                    PartyLeader.AddReputation(92);
                }

            }
            else
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
                attachee.FloatLine(12014, triggerer);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.FloatLine(12057, triggerer);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(196, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalVar(904) >= 3))
                {
                    if ((attachee != null))
                    {
                        var leader = attachee.GetLeader();
                        if ((leader != null))
                        {
                            leader.RemoveFollower(attachee);
                            attachee.FloatLine(22000, triggerer);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(232, true);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(232, false);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var randy1 = RandomRange(1, 12);
            if (((attachee.GetMap() == 5035 || attachee.GetMap() == 5046 || attachee.GetMap() == 5018 || attachee.GetMap() == 5086 || attachee.GetMap() == 5055) && randy1 >= 6))
            {
                attachee.FloatLine(12100, triggerer);
            }

            return RunDefault;
        }
        public static bool buttin(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8016);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 210);
            }

            return SkipDefault;
        }
        public static bool buttin2(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8055);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 330);
            }

            return SkipDefault;
        }
        public static bool buttin3(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 14037);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 55);
            }

            return SkipDefault;
        }
        public static bool buttin4(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8020);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 55);
            }

            return SkipDefault;
        }
        public static bool equip_transfer(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemA = attachee.FindItemByName(6142);
            if ((itemA != null))
            {
                itemA.Destroy();
            }

            var itemB = attachee.FindItemByName(6143);
            if ((itemB != null))
            {
                itemB.Destroy();
            }

            var itemC = attachee.FindItemByName(6144);
            if ((itemC != null))
            {
                itemC.Destroy();
            }

            var itemD = attachee.FindItemByName(6145);
            if ((itemD != null))
            {
                itemD.Destroy();
            }

            var itemE = attachee.FindItemByName(6146);
            if ((itemE != null))
            {
                itemE.Destroy();
            }

            var itemF = attachee.FindItemByName(4060);
            if ((itemF != null))
            {
                itemF.Destroy();
            }

            var itemG = attachee.FindItemByName(4061);
            if ((itemG != null))
            {
                itemG.Destroy();
            }

            return RunDefault;
        }
        public static bool switch_to_tarah(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8805);
            var meleny = Utilities.find_npc_near(attachee, 8015);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(meleny);
                meleny.TurnTowards(npc);
            }

            return SkipDefault;
        }

    }
}
