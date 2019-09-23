
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
    [ObjectScript(40)]
    public class HedrackAllies : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 8075))
            {
                // hedrack ally mage 1
                if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(747, 0);
                }

            }
            else if ((attachee.GetNameId() == 8076))
            {
                // hedrack ally mage 2
                if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(748, 0);
                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(544)))
            {
                return SkipDefault;
            }
            else if (((attachee.GetNameId() == 8083 || attachee.GetNameId() == 8084) && (!attachee.HasEquippedByName(4099) || !attachee.HasEquippedByName(4100))))
            {
                // ettin two weapon fighting script
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetNameId() == 8083 || attachee.GetNameId() == 8084) && (!attachee.HasEquippedByName(4099) || !attachee.HasEquippedByName(4100))))
            {
                // ettin two weapon fighting script
                attachee.WieldBestInAllSlots();
            }
            else if ((attachee.GetNameId() == 8075))
            {
                // hedrack ally mage 1
                if ((Utilities.obj_percent_hp(attachee) <= 66))
                {
                    if ((GetGlobalVar(745) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 471);
                        SetGlobalVar(745, GetGlobalVar(745) + 1);
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 466);
                    }

                }
                else
                {
                    attachee.SetInt(obj_f.critter_strategy, 466);
                }

            }
            else if ((attachee.GetNameId() == 8076))
            {
                // hedrack ally mage 2
                if ((Utilities.obj_percent_hp(attachee) <= 66))
                {
                    if ((GetGlobalVar(746) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 471);
                        SetGlobalVar(746, GetGlobalVar(746) + 1);
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 467);
                    }

                }
                else
                {
                    attachee.SetInt(obj_f.critter_strategy, 467);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(544))) // Hedrack battle has paused for talking, no one will KOS
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            if ((attachee != null && Utilities.critter_is_unconscious(attachee) != 1 && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                if (((attachee.GetNameId() == 8083 || attachee.GetNameId() == 8084) && (!attachee.HasEquippedByName(4099) || !attachee.HasEquippedByName(4100))))
                {
                    // ettin two weapon fighting script
                    attachee.WieldBestInAllSlots();
                    attachee.WieldBestInAllSlots();
                    DetachScript();
                }
                else if ((attachee.GetNameId() == 8075))
                {
                    // hedrack ally mage 1
                    var closest_jones = Utilities.party_closest(attachee);
                    if ((attachee.DistanceTo(closest_jones) <= 100))
                    {
                        SetGlobalVar(747, GetGlobalVar(747) + 1);
                        if ((attachee.GetLeader() == null))
                        {
                            if ((GetGlobalVar(747) == 4))
                            {
                                attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
                                attachee.PendingSpellsToMemorized();
                            }

                            if ((GetGlobalVar(747) == 8))
                            {
                                attachee.CastSpell(WellKnownSpells.ProtectionFromArrows, attachee);
                                attachee.PendingSpellsToMemorized();
                            }

                        }

                        if ((GetGlobalVar(747) >= 400))
                        {
                            SetGlobalVar(747, 0);
                        }

                    }

                }
                else if ((attachee.GetNameId() == 8076))
                {
                    // hedrack ally mage 2
                    if ((attachee.DistanceTo(PartyLeader) <= 100 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 100 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) <= 100 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) <= 100 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) <= 100 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(5)) <= 100 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(6)) <= 100 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(7)) <= 100))
                    {
                        SetGlobalVar(748, GetGlobalVar(748) + 1);
                        if ((attachee.GetLeader() == null))
                        {
                            if ((GetGlobalVar(748) == 4))
                            {
                                attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
                                attachee.PendingSpellsToMemorized();
                            }

                            if ((GetGlobalVar(748) == 8))
                            {
                                attachee.CastSpell(WellKnownSpells.ProtectionFromArrows, attachee);
                                attachee.PendingSpellsToMemorized();
                            }

                        }

                        if ((GetGlobalVar(748) >= 400))
                        {
                            SetGlobalVar(748, 0);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool will_kos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(544)))
            {
                return SkipDefault;
            }

            return RunDefault;
        }

    }
}
