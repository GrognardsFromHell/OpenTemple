
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using OpenTemple.Core.Logging;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    [AutoRegister]
    public static class Assassin
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public const Stat ClassId = Stat.level_assassin;

            public static D20ClassSpec ClassSpec = new D20ClassSpec("assassin")
            {
                classEnum = Stat.level_assassin,
                helpTopic = "TAG_ASSASSINS",
                conditionName = "Assassin",
                flags = ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 4,
                spellListType = SpellListType.Special,
                hasArmoredArcaneCasterFeature = true,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(-1, 0),
                    [2] = ImmutableList.Create(-1, 1),
                    [3] = ImmutableList.Create(-1, 2, 0),
                    [4] = ImmutableList.Create(-1, 3, 1),
                    [5] = ImmutableList.Create(-1, 3, 2, 0),
                    [6] = ImmutableList.Create(-1, 3, 3, 1),
                    [7] = ImmutableList.Create(-1, 3, 3, 2, 0),
                    [8] = ImmutableList.Create(-1, 3, 3, 3, 1),
                    [9] = ImmutableList.Create(-1, 3, 3, 3, 2),
                    [10] = ImmutableList.Create(-1, 3, 3, 3, 3)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.bluff,
                    SkillId.diplomacy,
                    SkillId.disable_device,
                    SkillId.gather_information,
                    SkillId.hide,
                    SkillId.intimidate,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.open_lock,
                    SkillId.pick_pocket,
                    SkillId.search,
                    SkillId.sense_motive,
                    SkillId.spot,
                    SkillId.tumble,
                    SkillId.use_magic_device,
                    SkillId.alchemy,
                    SkillId.balance,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.decipher_script,
                    SkillId.disguise,
                    SkillId.escape_artist,
                    SkillId.forgery,
                    SkillId.jump,
                    SkillId.swim,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_ROGUE, 1},
                    {FeatId.SNEAK_ATTACK, 1},
                    /* Omitted hash level 1 */
                    {FeatId.UNCANNY_DODGE, 2},
                    /* Omitted hash level 2 */
                    {FeatId.IMPROVED_UNCANNY_DODGE, 5},
                    /* Omitted hash level 8 */
                }.ToImmutableDictionary(),
            };

            [AutoRegister] public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
                .AddQueryHandler("Sneak Attack Dice", AssassinSneakAttackDice)
                .AddHandler(DispatcherType.SaveThrowLevel, AssassinPoisonSaveBonus) // Spell casting
                .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
                .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate, OnInitLevelupSpellSelection)
                .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize, OnLevelupSpellsFinalize)
                .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete, OnLevelupSpellsCheckComplete)
                .AddQueryHandler(D20DispatcherKey.QUE_Get_Arcane_Spell_Failure, AssassinSpellFailure)// Hide in Plain Sight    #
                .Build();

        public static void AssassinSneakAttackDice(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var ass_lvl = evt.objHndCaller.GetStat(ClassId);
            dispIo.return_val += 1 + (ass_lvl - 1) / 2;
        }

        public static void AssassinPoisonSaveBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var ass_lvl = evt.objHndCaller.GetStat(ClassId);
            if (ass_lvl < 2)
            {
                return;
            }

            if ((dispIo.flags & D20SavingThrowFlag.POISON) != 0) // D20STD_F_POISON
            {
                var value = ass_lvl / 2;
                dispIo.bonlist.AddBonus(value, 0, 137);
            }
        }

        public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            dispIo.bonlist.AddBonus(classLvl, 0, 137);
        }

        public static void OnInitLevelupSpellSelection(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                throw new NotImplementedException();
                // classSpecModule.InitSpellSelection(evt.objHndCaller);
            }
        }

        public static void OnLevelupSpellsCheckComplete(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                throw new NotImplementedException();
                // if (!classSpecModule.LevelupCheckSpells (evt.objHndCaller))
                // {
                //     dispIo.bonlist.AddBonus(-1, 0, 137); // denotes incomplete spell selection
                // }
            }
        }

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                throw new NotImplementedException();
                // classSpecModule.LevelupSpellsFinalize (evt.objHndCaller);
            }
        }

        public static void AssassinSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.data1 != (int) ClassId)
            {
                return;
            }

            var equip_slot = (EquipSlot) dispIo.data2;
            var item = evt.objHndCaller.ItemWornAt(equip_slot);
            if (item == null)
            {
                return;
            }

            if (equip_slot == EquipSlot.Armor) // armor - bards can cast in light armor with no spell failure
            {
                var armor_flags = item.GetArmorFlags();
                if (armor_flags.IsLightArmorOrLess())
                {
                    return;
                }

                if (evt.objHndCaller.D20Query("Improved Armored Casting") && (armor_flags == ArmorFlag.TYPE_MEDIUM))
                {
                    return;
                }

            }

            dispIo.return_val += item.GetInt(obj_f.armor_arcane_spell_failure);
        }
        // spellCasterSpecObj = PythonModifier(GetSpellCasterConditionName(), 8)
        // spellCasterSpecObj.AddHook(ET_OnGetBaseCasterLevel, EK_NONE, OnGetBaseCasterLevel, ())

        public static void HideInPlainSightQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
        }

        [FeatCondition("Hide in Plain Sight")]
        [AutoRegister] public static readonly ConditionSpec hips_feat = ConditionSpec.Create("Hide In Plain Sight Feat", 2)
            .AddQueryHandler("Can Hide In Plain Sight", HideInPlainSightQuery)
            .Build();

        // Death Attack

        private static readonly D20DispatcherKey deathAttackStudyEnum = (D20DispatcherKey) 2100;
        public static void AssassinDeathAttackRadial(in DispatcherCallbackArgs evt)
        {
            var radial_action = RadialMenuEntry.CreatePythonAction("Study Target",
                D20ActionType.PYTHON_ACTION, deathAttackStudyEnum, 0, "TAG_INTERFACE_HELP");
            var ass_lvl = evt.objHndCaller.GetStat(ClassId);
            // TODO: I think spell level is swapped with caster level here!
            radial_action.d20SpellData = new D20SpellData(3210, SpellSystem.GetSpellClass(ClassId), ass_lvl);
            radial_action.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
            // print "Death attack radial"
            if (evt.objHndCaller.D20Query("Has Studied Target"))
            {
                // print "Has studied target (radial)"
                var radial_parent = RadialMenuEntry.CreateParent("Death Attack");
                // print "Created radial parent"
                var par_node_id = radial_parent.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
                // print "Added radial parent, ID " + str(par_node_id)
                var check_box_paralyze = evt.CreateToggleForArg(1);
                check_box_paralyze.text = "Paralyze";
                check_box_paralyze.helpSystemHashkey = "TAG_INTERFACE_HELP";
                check_box_paralyze.AddAsChild(evt.objHndCaller, par_node_id);
            }
        }

        public static void OnStudyTargetPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // print "Performing Death Attack - Study Target"
            if (dispIo.action.d20ATarget == null)
            {
                Logger.Info("no target! PLS HANDLE ME");
            }

            var old_spell_id = evt.GetConditionArg3();
            if (old_spell_id != 0)
            {
                var spell_packet = Systems.GameSystems.Spell.GetActiveSpell(old_spell_id);
                var prev_tgt = spell_packet.Targets[0].Object;
                if (prev_tgt != null && prev_tgt.D20Query("Is Death Attack Target"))
                {
                    prev_tgt.D20SendSignal("Death Attack Target End", old_spell_id);
                    prev_tgt.FloatLine("Target removed", TextFloaterColor.White);
                }

            }

            // put the new spell_id in arg2
            var new_spell_id = GameSystems.Spell.GetNewSpellId();
            evt.SetConditionArg3(new_spell_id);
            // register the spell in the spells_cast repository so it triggers the spell scripts (Spell3210 - Death Attack.py)
            var cur_seq = GameSystems.D20.Actions.CurrentSequence;
            GameSystems.Spell.RegisterSpell(cur_seq.spellPktBody, new_spell_id);
            GameSystems.Script.Spells.SpellTrigger(new_spell_id, SpellEvent.SpellEffect);
        }

        public static void DeathAtkResetSneak(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            // print "sneak attack status reset"
            evt.SetConditionArg1(0);
        }
        public static void DeathAtkRegisterSneak(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // print "sneak attack registered"
            evt.SetConditionArg1(1);
        }

        private static bool IsActiveCombatant(GameObjectBody critter)
        {
            return GameSystems.Combat.IsCombatActive()
                && GameSystems.D20.Initiative.Contains(critter);
        }

        public static void DeathAtkDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            // print "Death Attack Sneak Attack Not active"
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // print "DeathAtkDamage"
            if ((dispIo.attackPacket.flags & D20CAF.RANGED) != D20CAF.NONE) // Death Attack is for Melee attacks only
            {
                return;
            }

            var tgt = dispIo.attackPacket.victim;
            if (tgt == null)
            {
                return;
            }

            // print "got target"
            var spell_id = evt.GetConditionArg3();
            if (spell_id == 0)
            {
                return;
            }

            // print "got spell id"
            var spell_packet = GameSystems.Spell.GetActiveSpell(spell_id);
            if (!spell_packet.HasTarget(tgt))
            {
                return;
            }

            // print "target is spell target"
            if (!tgt.D20Query("Is Death Attack Ready"))
            {
                return;
            }

            var attackerName = GameSystems.MapObject.GetDisplayNameForParty(evt.objHndCaller);

            // print "DeathAtkDamage target ok"
            // check that target is unaware of assassin (and if it is, override it if it's helpless)
            if (IsActiveCombatant(tgt) && !tgt.D20Query(D20DispatcherKey.QUE_Flatfooted) && tgt.HasLineOfSight(evt.objHndCaller) && !tgt.D20Query(D20DispatcherKey.QUE_Helpless))
            {
                var targetName = GameSystems.MapObject.GetDisplayNameForParty(tgt);
                GameSystems.RollHistory.CreateFromFreeText(targetName + " notices " + attackerName + ", Death Attack unsuccessful...\n\n");
                return;
            }

            // print "Ending Death Attack Target"
            tgt.D20SendSignal("Death Attack Target End", spell_id); // end the target's Death Attack Target status
            evt.SetConditionArg1(0);
            var ass_level = evt.objHndCaller.GetStat(ClassId);
            var int_level = evt.objHndCaller.GetStat(Stat.intelligence);
            var int_mod = (int_level - 10) / 2;

            GameSystems.RollHistory.CreateFromFreeText(attackerName + " attempting Death Attack...\n\n");
            if (tgt.SavingThrowSpell(10 + ass_level + int_mod, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, evt.objHndCaller, spell_id))
            {
                tgt.FloatMesFileLine("mes/spell.mes", 30001);
                GameSystems.RollHistory.CreateFromFreeText("Death Attack failed.\n\n");
            }
            else
            {
                evt.objHndCaller.FloatLine("Death Attack!");
                if (evt.GetConditionArg2() == 0) // death effect
                {
                    tgt.KillWithDeathEffect(evt.objHndCaller);
                    GameSystems.RollHistory.CreateFromFreeText("Killed by Death Attack!\n\n");
                }
                else
                {
                    // else: # paralysis effect
                    tgt.AddCondition("Paralyzed", ass_level + RandomRange(1, 6), 0, 0);
                    GameSystems.RollHistory.CreateFromFreeText("Target paralyzed.\n\n");
                }

            }
        }

        public static void HasStudiedTarget(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // print "Has Studied Target test:"
            var spell_id = evt.GetConditionArg3();
            // print "Spell ID: " + str(spell_id)
            if (spell_id == 0)
            {
                return;
            }

            var spell_packet = GameSystems.Spell.GetActiveSpell(spell_id);
            if (spell_packet.Targets.Length == 0)
            {
                return;
            }

            var tgt = spell_packet.Targets[0].Object;
            if (!tgt.D20Query("Is Death Attack Ready"))
            {
                return;
            }

            // print "   returning ok"
            dispIo.return_val = 1;
        }

        // arg0 - sneak attack registered;  arg1 - paralyze target;  arg2 - spell_id
        [FeatCondition("Death Attack")]
        [AutoRegister] public static readonly ConditionSpec death_attack_feat = ConditionSpec.Create("Death Attack Feat", 3)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, AssassinDeathAttackRadial)
            .AddHandler(DispatcherType.PythonActionPerform, deathAttackStudyEnum, OnStudyTargetPerform)
            .AddHandler(DispatcherType.ToHitBonusFromDefenderCondition, DeathAtkResetSneak)
            .AddHandler(DispatcherType.DealingDamage2, DeathAtkDamage)
            .AddSignalHandler("Sneak Attack Damage Applied", DeathAtkRegisterSneak)
            .AddQueryHandler("Has Studied Target", HasStudiedTarget)
            .Build();

        public static void IsDeathAtkTarget(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
        }

        public static void IsDeathAtkReady(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // print "Is death attack ready query answred by " + attachee.description
            if (evt.GetConditionArg2() >= 3 && evt.GetConditionArg2() <= 6)
            {
                dispIo.return_val = 1;
            }
            else
            {
                dispIo.return_val = 0;
            }
        }

        public static void DeathAtkTargetRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var signal_spell_id = dispIo.data1;
            var spell_id = evt.GetConditionArg1();
            if (spell_id == signal_spell_id)
            {
                evt.SetConditionArg2(evt.GetConditionArg2() + 1);
            }
        }

        public static void DeathAtkTargetRoundsStudied(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var signal_spell_id = dispIo.data1;
            var spell_id = evt.GetConditionArg1();
            if (spell_id == signal_spell_id)
            {
                dispIo.return_val = evt.GetConditionArg2();
            }
        }

        public static void DeathAtkTargetEnd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // print "ending death attack target for " + attachee.description
            var signal_spell_id = dispIo.data1;
            var spell_id = evt.GetConditionArg1();
            // print "signal spell id: " + str(signal_spell_id) + "   spell id: " + str(spell_id)
            if (spell_id == signal_spell_id)
            {
                evt.RemoveThisCondition();
            }
        }

        public static void DeathAtkTargetCountdown(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var numRounds = evt.GetConditionArg2();
            var roundsToReduce = dispIo.data1;
            if (numRounds + roundsToReduce <= 6)
            {
                evt.SetConditionArg2(numRounds + roundsToReduce);
                return;
            }

            evt.RemoveThisCondition();
        }
        // arg0 - spell id, arg1 - number of rounds

        [AutoRegister] public static readonly ConditionSpec deathAtkTgt = ConditionSpec.Create("Death Attack Target", 3)
            .AddQueryHandler("Is Death Attack Target", IsDeathAtkTarget)
            .AddQueryHandler("Is Death Attack Ready", IsDeathAtkReady)
            .AddSignalHandler("Target Study Round", DeathAtkTargetRound)
            .AddQueryHandler("Death Attack Target Rounds Studied", DeathAtkTargetRoundsStudied)
            .AddSignalHandler("Death Attack Target End", DeathAtkTargetEnd)
            .AddHandler(DispatcherType.BeginRound, DeathAtkTargetCountdown)
            .Build();
    }
}
