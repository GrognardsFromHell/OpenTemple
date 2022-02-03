using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;
using Path = OpenTemple.Core.Systems.Pathfinding.Path;

namespace OpenTemple.Core.Systems.D20
{
    public enum D20CombatMessage
    {
        poison_level = 0,
        hp = 1,
        critical_miss = 2,
        scarred = 3,
        blinded = 4,
        crippled_arm = 5,
        crippled_leg = 6,
        dodge = 11,
        critical_hit = 12,
        stunned = 16,
        unconscious = 17,
        Nonlethal = 25,
        nonlethal = 25,
        stabilizes = 26,
        dying = 27,
        exertion = 28,
        DEAD = 30,
        hit = 31,
        heal_success = 34,
        heal_failure = 35,
        cleave = 36,
        great_cleave = 37,
        invisible = 41,
        reloaded = 42,
        attack_of_opportunity = 43,
        out_of_ammo = 44,
        miss_concealment = 45,
        acid_damage = 46,
        sleeping = 47,
        flatfooted = 48,
        surprised = 49,
        Points_of_Damage_Absorbed = 50,
        afraid = 52,
        spell_disrupted = 54,
        miscast_armor = 57,
        miscast_defensive_casting = 58,
        arcane_spell_failure_due_to_armor = 59,
        touch_attack_hit = 68,
        holding_charge = 70,
        charmed = 73,
        blind = 76,
        _stunned = 89,
        success = 102,
        failure = 103,
        friendly_fire = 107,
        sanctuary = 123,
        tumble_successful = 129,
        tumble_unsuccessful = 130,
        attempt_succeeds = 143,
        attempt_fails = 144,
        gains = 145,
        experience_point = 146,
        award_reduced_due_to_uneven_class_levels = 147,
        gains_a_level = 148,
        feint_successful = 152,
        attached = 154,
        action_readied = 157,
        deflect_attack = 159,
        missile_counterattack = 160,
        grabs_missile = 161,
        uses_Manyshot = 162,
        activated = 165,
        deactivated = 166,
        coup_de_grace_kill = 174,
        duration = 175,
        key_Assign_ = 179,
        keyCurrentlyUnassigned = 182,
        keyCurrentlyAssignedTo = 183,
        key_AssignHotkey = 185,
        fortitude = 500,
        reflex = 501,
        will = 502,
        full_attack = 5001,
        brew_potion = 5066,
        scribe_scroll = 5067,
        craft_wand = 5068,
        craft_rod = 5069,
        craft_wondrous_item = 5070,
        craft_magic_arms_and_armor = 5071,
        identify_potion = 5072,
        use_magic_device_decipher_script = 5073,
        track = 5074,
        set_weapon_charge = 5075,
        wild_shape = 5076,
        ready_vs_spell = 5090,
        ready_vs_approach = 5092,
        flee_combat = 5102,
        craft_staff = 5103, // TODO: This entry is actually MISSING from combat.mes !
        forge_ring = 5104, // TODO: This entry is actually MISSING from combat.mes !
        animal_companion = 6000,
        dismiss_AC = 6001,
        not_during_combat_AC = 6002,
        are_you_sure_you_want_to_dismiss = 6003,
        are_you_sure_you_want_to_dismiss_AC = 6003,
        ok = 6009,
        cancel = 6010,
        name_your_familiar = 6011,
        name_your_animal_companion = 6012,
    }

    public class D20ExperienceSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public const int CRMIN = -2; // equiv to CR 1/4  (next ones are CR 1/3, CR 1/2, CR 1, CR 2, CR 3,...
        public const int CRMAX = 50;
        public const int CRCOUNT = CRMAX - CRMIN + 1;
        public const int XPTABLE_MAXLEVEL = 50;
        public const int XP_REQ_TABLE_SIZE = 100;
        public int [,] XPAwardTable = new int[XPTABLE_MAXLEVEL, CRCOUNT];
        
        public D20ExperienceSystem()
        {
            GenerateXpTable();
        }

        [TempleDllLocation(0x100B5700)]
        private void GenerateXpTable() {
            var table = XPAwardTable;
            var slowerLevelling = Globals.Config.slowerLevelling;
            // First set the table's "spine" - when CR = Level  then   XP = 300*level 
            for (int level = 1; level <= XPTABLE_MAXLEVEL; level++)
            {
                int basicAward = level * 300;
                table[level - 1, level - CRMIN] = basicAward;
                // Slower levelling gameplay option - modifies rewards for level 3 onwards
                if (slowerLevelling && level >= 3)
                {
                    var awardMultiplier =
                        1 - 0.66F * Math.Min(1.0F,
                                     Math.Pow(level - 2.0F, 0.1F) / Math.Pow(16.0F, 0.1F));
                    table[level - 1, level - CRMIN] = (int)(basicAward * awardMultiplier);
                }

            }

            // Fill out the bottom left portion - CRs less than level - from highest to lowest
            for (int level = 1; level <= XPTABLE_MAXLEVEL; level++)
            {
                for (int j = level - CRMIN - 1; j >= 2; j--)
                {
                    int i = level - 1;
                    int cr = j + CRMIN;

                    // 8 CRs below level grant nothing
                    if (cr <= level - 8)
                    {
                        table[i, j] = 0;
                    }
                    else if (cr == 0)
                    {
                        table[i, 2] = table[i, 3] / 2; // CR 1/2
                        table[i, 1] = table[i, 3] / 3; // CR 1/3
                        table[i, 0] = table[i, 3] / 4; // CR 1/4
                    }
                    else if (cr == level - 1)
                    {
                        Debug.Assert(i >= 1);
                        if (slowerLevelling)
                            table[i, j] = Math.Min(table[i - 1, j], (table[i, j + 1] * 6) / 11);
                        else
                            table[i, j] = Math.Min(table[i - 1, j], (table[i, j + 1] * 2) / 3);
                    }
                    else
                    {
                        Debug.Assert(i >= 1);
                        Debug.Assert(j + 2 < CRCOUNT);
                        if (slowerLevelling)
                            table[i, j] = Math.Min(table[i - 1, j], (table[i, j + 2] * 3) / 10);
                        else
                            table[i, j] = Math.Min(table[i - 1, j], table[i, j + 2] / 2);
                    }
                }
            }

            // Fill out the top right portion
            for (int cr_off = 1; cr_off < CRMAX; cr_off++)
            {

                for (int level = 1; level <= XPTABLE_MAXLEVEL && level + cr_off <= CRMAX; level++)
                {
                    int i = level - 1;
                    int j = level - CRMIN + cr_off;

                    Debug.Assert(i >= 0 && i < XPTABLE_MAXLEVEL);
                    Debug.Assert(j >= 0 && j < CRCOUNT);

                    if (cr_off >= 10)
                    {
                        table[i, j] = table[level - 1, j - 1]; // repeat the last value
                    }
                    else if (cr_off == 1)
                    {
                        Debug.Assert(j >= 1);
                        Debug.Assert(i + 1 < XPTABLE_MAXLEVEL);
                        table[i, j] = Math.Max((table[i, j - 1] * 3) / 2, table[i + 1, j]);
                    }
                    else
                    {
                        Debug.Assert(i + 1 < XPTABLE_MAXLEVEL);
                        Debug.Assert(j >= 2);
                        table[i, j] = Math.Max(table[i, j - 2] * 2, table[i + 1, j]);
                    }
                }
            }
        }

        [TempleDllLocation(0x100B5700)]
        public void AwardExperience()
        {
            float fNumLivingPartyMembers = 0.0F;
            foreach (var pc in GameSystems.Party.PlayerCharacters)
            {
                if (GameSystems.Critter.IsDeadNullDestroyed(pc))
                    continue;
                fNumLivingPartyMembers += 1.0F;
            }
            
            foreach (var follower in GameSystems.Party.NPCFollowers)
            {
                if (GameSystems.Critter.IsDeadNullDestroyed(follower))
                    continue;
                if (GameSystems.D20.D20Query(follower, D20DispatcherKey.QUE_ExperienceExempt))
                    continue;
                fNumLivingPartyMembers += 1.0F;
            }
            if (fNumLivingPartyMembers < 0.99)
                return;

            
            bool bShouldUpdatePartyUI = false;
            int xpForxpPile = 0;

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (GameSystems.Critter.IsDeadNullDestroyed(partyMember))
                    continue;
                if (GameSystems.D20.D20Query(partyMember, D20DispatcherKey.QUE_ExperienceExempt))
                    continue;
                if (GameSystems.Party.IsAiFollower(partyMember))
                    continue;

                var level = GameSystems.Critter.GetEffectiveLevel(partyMember);
                if (level <= 0) continue;

                int xpGainRaw = 0; // raw means it's prior to applying multiclass penalties, which  is Someone Else's Problem :P
                var experienceMultiplier = GameSystems.D20.Combat._experienceMultiplier;
                foreach (var entry in GameSystems.D20.Combat._challengeRatingsDefeated)
                {
                    var crValue = entry.Key;
                    var nkill = entry.Value;
                    var crIndex = crValue - CRMIN;
                    if (crValue < CRMIN || crIndex > CRMAX)
                    {
                        Logger.Warn("Cannot award XP for challenge rating {0}", crValue);
                        continue;
                    }
                    float xp = XPAwardTable[level - 1,crIndex];
                    if (nkill > 0){
                        xpGainRaw += (int)(
                            experienceMultiplier * nkill * xp
                            );
                    }
                }
                xpForxpPile += xpGainRaw;
                xpGainRaw = (int) ( xpGainRaw / fNumLivingPartyMembers);
                
                if ( XpGainProcess(partyMember, xpGainRaw)) {
                    
                    if (partyMember.IsPC() || Globals.Config.NPCsLevelLikePCs) {
                        bShouldUpdatePartyUI = true;
                    }
                }

            }

            foreach (var entry in GameSystems.D20.Combat._challengeRatingsDefeated)
            {
                GameSystems.D20.Combat._challengeRatingsDefeated[entry.Key] = 0;
            }
            GameSystems.D20.Combat._xpTotalFromCombat = xpForxpPile;
            

            if (bShouldUpdatePartyUI){
                GameUiBridge.UpdatePartyUi();
                GameSystems.SoundGame.Sound(100001); // LEVEL_UP.WAV
                GameSystems.SoundGame.Sound(100001); // amp it up a bit
            }
        }

        [TempleDllLocation(0x100B5480)]
        private bool XpGainProcess(GameObject obj, int xpGainRaw)
        {
            if (xpGainRaw <= 0 || obj == null)
                return false;
            var couldAlreadyLevelup = GameSystems.Critter.CanLevelUp(obj);

            int xpReduction = GetMulticlassXpReductionPercent(obj);

            //Check if the multiclass xp penalty should be disabled for this character
            var res = GameSystems.D20.D20QueryPython(obj, "No MultiClass XP Penalty");
            if (res != 0){
                xpReduction = 0;
            }
            int xpGain = (int)((1.0F - xpReduction / 100.0F) * xpGainRaw);



            string text = String.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", obj, GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.gains), xpGain, GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.experience_point) );
            if (xpReduction > 0){
                text += " " + GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.award_reduced_due_to_uneven_class_levels);
            }
            GameSystems.RollHistory.CreateFromFreeText(text + "\n");
            
            var xpNew = obj.GetInt32(obj_f.critter_experience) + xpGain;
            var curLvl = GameSystems.Critter.GetEffectiveLevel(obj);

            var xpCap = GameSystems.Level.GetExperienceForLevel(curLvl + 2) - 1;
            if (curLvl >= Globals.Config.MaxLevel)
                xpCap = GameSystems.Level.GetExperienceForLevel(Globals.Config.MaxLevel);

            if (Globals.Config.allowXpOverflow && xpNew > xpCap)
                xpNew = xpCap;

            GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Experience_Awarded, xpNew);
            obj.SetInt32(obj_f.critter_experience, xpNew);

            if (couldAlreadyLevelup || !GameSystems.Critter.CanLevelUp(obj))
                return false;

            GameSystems.D20.Combat.FloatCombatLine(obj, D20CombatMessage.gains_a_level);
            GameSystems.RollHistory.CreateFromFreeText(String.Format("{0} {1}\n", obj, GameSystems.D20.Combat.GetCombatMesLine(D20CombatMessage.gains_a_level) ) );
            GameSystems.ParticleSys.CreateAtObj("LEVEL UP", obj);

            return true;
        }

        [TempleDllLocation(0x100B53B0)]
        private int GetMulticlassXpReductionPercent(GameObject obj)
        {
            var highestLvl = -1;
            var reductionPct = 0;

            // House rule - disable penalty
            if (Globals.Config.laxRules && Globals.Config.disableMulticlassXpPenalty)
                return reductionPct;
            
            // get highest base class level
            foreach (var classEnum in D20ClassSystem.BaseClasses)
            {
                var classLvl = GameSystems.Stat.StatLevelGet(obj, classEnum);
                if (classLvl > highestLvl && !GameSystems.D20.D20Query( obj, D20DispatcherKey.QUE_FavoredClass, (int)classEnum) ) {
                    highestLvl = classLvl;
                }
            }

            // reduce XP for every lagging class
            foreach (var classEnum in D20ClassSystem.BaseClasses) {
                var classLvl = GameSystems.Stat.StatLevelGet(obj, classEnum);
                if (classLvl > 0 && classLvl < highestLvl - 1 && !GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_FavoredClass, (int)classEnum) ) {
                    reductionPct += 20;
                }
            }

            if (reductionPct >= 80)
                reductionPct = 80;

            return reductionPct;
        }
    }

    public class D20CombatSystem
    {
        public const int MesTumbleSuccessful = 129;
        public const int MesTumbleUnsuccessful = 130;

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10BCA848)]
        private readonly Dictionary<int, string> _messages;

        [TempleDllLocation(0x102CF708)]
        public float _experienceMultiplier;
        [TempleDllLocation(0x10BCA8BC)]
        public int _xpTotalFromCombat { get; set; }

        [TempleDllLocation(0x10BCA8B8)]
        [TempleDllLocation(0x100B6690)]
        public bool SavingThrowsAlwaysFail { get; set; }

        [TempleDllLocation(0x10BCA8B4)]
        [TempleDllLocation(0x100b6670)]
        public bool SavingThrowsAlwaysSucceed { get; set; }

        [TempleDllLocation(0x10BCA8B0)]
        public bool AlwaysCrit { get; set; }
        

        // Used to record how many critters or other challenges have been defeated for a given CR
        // until XP awards can be given out
        [TempleDllLocation(0x10BCA850)]
        public Dictionary<int, int> _challengeRatingsDefeated = new Dictionary<int, int>();

        // Indicates whether last damage was from direct attack (true) or spell (false)
        [TempleDllLocation(0x10BCA8AC)]
        private bool _lastDamageFromAttack;

        [TempleDllLocation(0x100b4770)]
        public D20CombatSystem()
        {
            _messages = Tig.FS.ReadMesFile("mes/combat.mes");
            foreach (var kvp in Tig.FS.ReadMesFile("tpmes/combat.mes"))
            {
                _messages[kvp.Key] = kvp.Value;
            }

            var vars = Tig.FS.ReadMesFile("rules/combat_vars.mes");
            _experienceMultiplier = float.Parse(vars[0], CultureInfo.InvariantCulture);
        }

        [TempleDllLocation(0x100b4b30)]
        public string GetCombatMesLine(int line)
        {
            return _messages[line];
        }

        public string GetCombatMesLine(D20CombatMessage message) => GetCombatMesLine((int) message);

        public void FloatCombatLine(GameObject obj, D20CombatMessage message, string prefix = null,
            string suffix = null)
            => FloatCombatLine(obj, (int) message, prefix, suffix);

        [TempleDllLocation(0x100b4b60)]
        public void FloatCombatLine(GameObject obj, int line, string prefix = null, string suffix = null,
            TextFloaterColor? forcedColor = null)
        {
            TextFloaterColor floatColor;

            if (forcedColor.HasValue)
            {
                floatColor = forcedColor.Value;
            }
            else
            {
                var objType = obj.type;
                if (objType == ObjectType.pc)
                {
                    floatColor = TextFloaterColor.White;
                }
                else if (objType != ObjectType.npc)
                {
                    floatColor = TextFloaterColor.Red;
                }
                else
                {
                    floatColor = TextFloaterColor.Yellow;
                    var npcLeader = GameSystems.Critter.GetLeader(obj);
                    if (!GameSystems.Party.IsInParty(npcLeader))
                    {
                        floatColor = TextFloaterColor.Red;
                    }
                }
            }

            var text = GetCombatMesLine(line);
            if (prefix != null)
            {
                text = prefix + text;
            }

            if (suffix != null)
            {
                text += suffix;
            }

            GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, floatColor, text);
        }

        [TempleDllLocation(0x100b4f20)]
        public bool SavingThrow(GameObject critter, GameObject opponent, int dc,
            SavingThrowType saveType, D20SavingThrowFlag flags = D20SavingThrowFlag.NONE)
        {
            var dispIo = DispIoSavingThrow.Default;
            dispIo.flags = flags;
            dispIo.obj = opponent;

            // Apply static saving throw bonuses for NPCs
            if (critter.IsNPC())
            {
                int npcSaveBonus;
                switch (saveType)
                {
                    case SavingThrowType.Fortitude:
                        npcSaveBonus = critter.GetInt32(obj_f.npc_save_fortitude_bonus);
                        break;
                    case SavingThrowType.Reflex:
                        npcSaveBonus = critter.GetInt32(obj_f.npc_save_reflexes_bonus);
                        break;
                    case SavingThrowType.Will:
                        npcSaveBonus = critter.GetInt32(obj_f.npc_save_willpower_bonus);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(saveType));
                }

                dispIo.bonlist.AddBonus(npcSaveBonus, 0, 139);
            }

            D20StatSystem.Dispatch13SavingThrow(critter, saveType, dispIo);

            var spellResistanceMod = 0;
            dispIo.obj = critter;
            if (opponent != null && !dispIo.flags.HasFlag(D20SavingThrowFlag.CHARM))
            {
                spellResistanceMod = D20StatSystem.Dispatch14SavingThrowResistance(opponent, saveType, dispIo);
            }

            var saveThrowRoll = Dice.D20.Roll();
            if (SavingThrowsAlwaysFail)
            {
                saveThrowRoll = 1;
            }
            else if (SavingThrowsAlwaysSucceed)
            {
                saveThrowRoll = 20;
            }

            dispIo.rollResult = saveThrowRoll;
            if (saveThrowRoll + spellResistanceMod < dc || saveThrowRoll == 1)
            {
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_RerollSavingThrow))
                {
                    GameSystems.RollHistory.AddSavingThrow(critter, dc, saveType, flags, Dice.D20, saveThrowRoll,
                        in dispIo.bonlist);
                    saveThrowRoll = Dice.D20.Roll();
                    flags |= D20SavingThrowFlag.REROLL;
                }
            }

            // Bard's countersong can override the saving throw result
            var v13 = GameSystems.RollHistory.AddSavingThrow(critter, dc, saveType, flags, Dice.D20, saveThrowRoll,
                in dispIo.bonlist);
            GameSystems.RollHistory.CreateRollHistoryString(v13);

            if (saveThrowRoll == 1)
            {
                return false;
            }
            else if (saveThrowRoll == 20)
            {
                return true;
            }

            var countersongResult = D20StatSystem.Dispatch40SavingThrow(critter, saveType, dispIo);
            return countersongResult + saveThrowRoll >= dc;
        }

        public bool SavingThrowSpell(GameObject objHnd, GameObject caster, int DC, SpellSavingThrow spellSaveType,
            D20SavingThrowFlag D20STDFlags, int spellId)
        {
            SavingThrowType saveType;
            switch (spellSaveType)
            {
                case SpellSavingThrow.None:
                    // Spell doesn't offer a save!
                    return false;
                case SpellSavingThrow.Reflex:
                    saveType = SavingThrowType.Reflex;
                    break;
                case SpellSavingThrow.Willpower:
                    saveType = SavingThrowType.Will;
                    break;
                case SpellSavingThrow.Fortitude:
                    saveType = SavingThrowType.Fortitude;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spellSaveType), spellSaveType, null);
            }

            return SavingThrowSpell(objHnd, caster, DC, saveType, D20STDFlags, spellId);
        }

        [TempleDllLocation(0x100b83c0)]
        public bool SavingThrowSpell(GameObject objHnd, GameObject caster, int DC, SavingThrowType saveType,
            D20SavingThrowFlag D20STDFlags, int spellId)
        {
            D20SavingThrowFlag flags;
            SpellEntry spellEntry;
            SpellPacketBody spPkt;

            var result_1 = false;
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out spPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spPkt.spellEnum, out spellEntry);
                flags = D20STDFlags | D20SavingThrowFlag.SPELL_LIKE_EFFECT;
                switch (spellEntry.spellSchoolEnum)
                {
                    case SchoolOfMagic.Abjuration:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_ABJURATION;
                        break;
                    case SchoolOfMagic.Conjuration:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_CONJURATION;
                        break;
                    case SchoolOfMagic.Divination:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_DIVINATION;
                        break;
                    case SchoolOfMagic.Enchantment:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT;
                        break;
                    case SchoolOfMagic.Evocation:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_EVOCATION;
                        break;
                    case SchoolOfMagic.Illusion:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_ILLUSION;
                        break;
                    case SchoolOfMagic.Necromancy:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_NECROMANCY;
                        break;
                    case SchoolOfMagic.Transmutation:
                        flags |= D20SavingThrowFlag.SPELL_SCHOOL_TRANSMUTATION;
                        break;
                    default:
                        break;
                }

                // TODO: Make this nicer. Transfers spell descriptors like ACID, et al over to the saving throw flags
                // TODO: This overflows the available bits (32-bit are not enough)
                for (var i = 0; i < 21; i++)
                {
                    var spellDescriptor = (SpellDescriptor) (1 << i);
                    var savingThrowFlag = (D20SavingThrowFlag) (1 << (i + 13));

                    if (spellEntry.HasDescriptor(spellDescriptor))
                    {
                        flags |= savingThrowFlag;
                    }
                }

                result_1 = SavingThrow(objHnd, caster, DC, saveType, flags);
                spPkt.savingThrowResult = result_1;
                GameSystems.Spell.UpdateSpellPacket(spPkt);
                GameSystems.Script.Spells.UpdateSpell(spPkt.spellId);
            }

            return result_1;
        }

        [TempleDllLocation(0x100b9460)]
        public void KillWithDeathEffect(GameObject critter, GameObject killer)
        {
            if (!critter.AddCondition("Killed By Death Effect", 0, 0))
            {
                Logger.Warn("Failed to add killed by death effect condition.");
            }

            Kill(critter, killer);
        }

        [TempleDllLocation(0x100b8a00)]
        public void Kill(GameObject critter, GameObject killer)
        {
            if (DoOnDeathScripts(critter, killer))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(13, critter, killer);
                FloatCombatLine(critter, 30);
                GameSystems.D20.D20SendSignal(critter, D20DispatcherKey.SIG_Killed, killer);

                critter.AddCondition(BuiltInConditions.Dead);

                if (killer == null)
                {
                    killer = critter.GetObject(obj_f.last_hit_by);
                }

                AwardExperienceForKill(killer, critter);
            }
        }

        [TempleDllLocation(0x100b88e0)]
        private void AwardExperienceForKill(GameObject killer, GameObject killed)
        {
            GameUiBridge.RecordKill(killer, killed);
            if (GameSystems.Party.IsInParty(killer))
            {
                if (!killed.IsPC() && !killed.HasCondition(BuiltInConditions.Summoned))
                {
                    // Prevent a critter from awarding experience multiple times
                    var critterFlags = killed.GetCritterFlags();
                    if (!critterFlags.HasFlag(CritterFlag.EXPERIENCE_AWARDED))
                    {
                        var level = killed.GetStat(Stat.level);
                        var cr = killed.GetInt32(obj_f.npc_challenge_rating);
                        AwardExperienceForChallengeRating(level + cr);
                        killed.SetCritterFlags(critterFlags | CritterFlag.EXPERIENCE_AWARDED);
                    }
                }
            }
        }

        [TempleDllLocation(0x100b8880)]
        public void AwardExperienceForChallengeRating(int challengeRating)
        {
            if (challengeRating > 20)
            {
                challengeRating = 20;
            }
            else if (challengeRating < -2)
            {
                return;
            }

            if (!_challengeRatingsDefeated.TryGetValue(challengeRating, out var count))
            {
                count = 0;
            }

            _challengeRatingsDefeated[challengeRating] = count + 1;

            // Immediately give XP awards if we're not in combat, otherwise they're queued
            if (!GameSystems.Combat.IsCombatActive())
            {
                AwardCombatExperience();
            }
        }

        [TempleDllLocation(0x100b88c0)]
        public void AwardCombatExperience()
        {
            AwardExperience();
            GameUiBridge.LogbookExperience(_xpTotalFromCombat);
        }

        // This is just the non-combat version of the above
        [TempleDllLocation(0x100b88b0)]
        public void AwardExperience()
        {
            GameSystems.D20.Experience.AwardExperience();
        }

        private bool DoOnDeathScripts(GameObject obj, GameObject killer)
        {
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Dead))
            {
                return false;
            }

            var listener = GameSystems.Dialog.GetListeningPartyMember(obj);
            {
                GameSystems.Dialog.GetDyingVoiceLine(obj, listener, out var text, out var soundId);
                GameSystems.Dialog.PlayCritterVoiceLine(obj, listener, text, soundId);
            }

            // Give any NPC follower a chance to complain or lament or rejoyce!
            if (obj.IsPC())
            {
                foreach (var npcFollower in GameSystems.Party.NPCFollowers)
                {
                    if (GameSystems.Critter.GetLeader(npcFollower) == obj)
                    {
                        GameSystems.Dialog.GetLeaderDyingVoiceLine(npcFollower, listener, out var text,
                            out var soundId);
                        GameSystems.Dialog.PlayCritterVoiceLine(npcFollower, listener, text, soundId);
                    }
                }
            }

            // This way of setting death is actually unreliable given certain feats...
            var maxHp = obj.GetStat(Stat.hp_max);
            GameSystems.MapObject.ChangeTotalDamage(obj, maxHp + 10);
            EncodedAnimId animId;
            switch (GameSystems.Random.GetInt(0, 2))
            {
                default:
                    animId = new EncodedAnimId(NormalAnimType.Death);
                    break;
                case 1:
                    animId = new EncodedAnimId(NormalAnimType.Death2);
                    break;
                case 2:
                    animId = new EncodedAnimId(NormalAnimType.Death3);
                    break;
            }

            GameSystems.Critter.HandleDeath(obj, killer, animId);
            return true;
        }


        [TempleDllLocation(0x100B7950)]
        public int DealAttackDamage(GameObject attacker, GameObject target, int d20Data, D20CAF flags,
            D20ActionType actionType)
        {
            GameSystems.AI.ProvokeHostility(attacker, target, 1, 0);

            if (GameSystems.Critter.IsDeadNullDestroyed(target))
            {
                return -1;
            }

            DispIoDamage evtObjDam = new DispIoDamage();
            evtObjDam.attackPacket.d20ActnType = actionType;
            evtObjDam.attackPacket.attacker = attacker;
            evtObjDam.attackPacket.victim = target;
            evtObjDam.attackPacket.dispKey = d20Data;
            evtObjDam.attackPacket.flags = flags;

            ref var weaponUsed = ref evtObjDam.attackPacket.weaponUsed;
            if (flags.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                weaponUsed = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
            }
            else
            {
                weaponUsed = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
            }

            if (weaponUsed != null && weaponUsed.type != ObjectType.weapon)
            {
                weaponUsed = null;
            }

            evtObjDam.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);

            if (flags.HasFlag(D20CAF.CONCEALMENT_MISS))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(11, attacker, target);
                FloatCombatLine(attacker, 45); // Miss (Concealment)!
                var soundId = GameSystems.SoundMap.GetSoundIdForItemEvent(weaponUsed, attacker, target, 6);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);

                GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Attack_Made, evtObjDam);
                return -1;
            }

            if (!flags.HasFlag(D20CAF.HIT))
            {
                FloatCombatLine(attacker, 29);
                GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Attack_Made, evtObjDam);

                var soundId = GameSystems.SoundMap.GetSoundIdForItemEvent(weaponUsed, attacker, target, 6);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);

                if (flags.HasFlag(D20CAF.DEFLECT_ARROWS))
                {
                    FloatCombatLine(target, 5052);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(12, attacker, target);
                }

                // dodge animation
                if (!GameSystems.Critter.IsDeadOrUnconscious(target) && !GameSystems.Critter.IsProne(target))
                {
                    GameSystems.Anim.PushDodge(attacker, target);
                }

                return -1;
            }

            if (target != null
                && attacker != null
                && GameSystems.Critter.NpcAllegianceShared(target, attacker)
                && GameSystems.Combat.AffiliationSame(target, attacker))
            {
                FloatCombatLine(target, 107); // Friendly Fire
            }

            var isUnconsciousAlready = GameSystems.Critter.IsDeadOrUnconscious(target);

            var attackerDispatcher = attacker?.GetDispatcher();
            attackerDispatcher?.Process(DispatcherType.DealingDamage, D20DispatcherKey.NONE, evtObjDam);

            if (evtObjDam.attackPacket.flags.HasFlag(D20CAF.CRITICAL))
            {
                // get extra Hit Dice and apply them
                DispIoAttackBonus evtObjCritDice = DispIoAttackBonus.Default;
                evtObjCritDice.attackPacket.victim = target;
                evtObjCritDice.attackPacket.d20ActnType = evtObjDam.attackPacket.d20ActnType;
                evtObjCritDice.attackPacket.attacker = attacker;
                evtObjCritDice.attackPacket.dispKey = d20Data;
                evtObjCritDice.attackPacket.flags = evtObjDam.attackPacket.flags;
                if (evtObjDam.attackPacket.flags.HasFlag(D20CAF.SECONDARY_WEAPON))
                {
                    evtObjCritDice.attackPacket.weaponUsed =
                        GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                }
                else
                {
                    evtObjCritDice.attackPacket.weaponUsed =
                        GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                }

                if (evtObjCritDice.attackPacket.weaponUsed != null &&
                    evtObjCritDice.attackPacket.weaponUsed.type != ObjectType.weapon)
                {
                    evtObjCritDice.attackPacket.weaponUsed = null;
                }

                evtObjCritDice.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
                var extraHitDice = attacker.DispatchGetCritExtraDice(ref evtObjCritDice);
                evtObjDam.damage.AddCritMultiplier(1 + extraHitDice, 102);

                FloatCombatLine(attacker, 12);

                // play sound
                var soundId = GameSystems.SoundMap.GetCritterSoundEffect(target, CritterSoundEffect.Attack);
                GameSystems.SoundGame.PositionalSound(soundId, target);
                soundId = GameSystems.SoundMap.GetSoundIdForItemEvent(evtObjCritDice.attackPacket.weaponUsed, attacker,
                    target,
                    7);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);

                // increase crit hits in logbook
                GameUiBridge.IncreaseCritHits(attacker);
            }
            else
            {
                var soundId =
                    GameSystems.SoundMap.GetSoundIdForItemEvent(evtObjDam.attackPacket.weaponUsed, attacker, target, 5);
                GameSystems.SoundGame.PositionalSound(soundId, attacker);
            }

            _lastDamageFromAttack = true; // physical damage Flag used for logbook recording
            DamageCritter(attacker, target, evtObjDam);

            // play damage effect particles
            foreach (var damageDice in evtObjDam.damage.dice)
            {
                GameSystems.Anim.PlayDamageEffect(target, damageDice.type, damageDice.rolledDamage);
            }

            GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Attack_Made, evtObjDam);

            // signal events
            if (!isUnconsciousAlready && GameSystems.Critter.IsDeadOrUnconscious(target))
            {
                GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Dropped_Enemy, evtObjDam);
            }

            return evtObjDam.damage.GetOverallDamageByType();
        }

        [TempleDllLocation(0x100b6b30)]
        private void DamageCritter(GameObject attacker, GameObject victim, DispIoDamage dispIoDamage)
        {
            var attackingCritterType = victim.type;
            if (attacker != null)
            {
                attackingCritterType = attacker.type;
            }

            var tgtIsProne = GameSystems.Critter.IsDeadOrUnconscious(victim)
                             || GameSystems.D20.D20Query(victim, D20DispatcherKey.QUE_Prone);

            if ((victim.GetFlags() & ObjectFlag.INVULNERABLE) != 0)
            {
                dispIoDamage.damage.AddModFactor(0.0f, DamageType.Unspecified, 104);
            }

            var damagePacket = dispIoDamage.damage;
            damagePacket.CalcFinalDamage();

            victim.DispatchTakingDamage(dispIoDamage);
            var caf = dispIoDamage.attackPacket.flags;
            if ((caf & D20CAF.TRAP) != 0)
            {
                attacker = null;
            }
            else if (attacker != null)
            {
                attacker.DispatchDealingDamage2(dispIoDamage);
            }

            victim.DispatchTakingDamageFinal(dispIoDamage);

            if (attacker != null)
            {
                victim.SetObject(obj_f.last_hit_by, attacker);
            }

            var realDamage = Math.Max(0, damagePacket.GetOverallLethalDamage());

            var currentDamage = victim.GetInt32(obj_f.hp_damage);
            GameSystems.MapObject.ChangeTotalDamage(victim, currentDamage + realDamage);

            var damageRollHistId = GameSystems.RollHistory.AddDamageRoll(attacker, victim, damagePacket);
            GameSystems.RollHistory.CreateRollHistoryString(damageRollHistId);
            GameSystems.D20.D20SendSignal(victim, D20DispatcherKey.SIG_HP_Changed, -realDamage);

            if (realDamage > 0)
            {
                victim.AddCondition(StatusEffects.Damaged, realDamage);

                if (attacker != null)
                {
                    GameUiBridge.LogbookCombatDamage(GameSystems.D20.Combat._lastDamageFromAttack, realDamage,
                        attacker, victim);

                    if (attacker != victim && GameSystems.Critter.IsFriendly(attacker, victim))
                    {
                        GameSystems.Dialog.GetFriendlyFireVoiceLine(victim, attacker, out var text, out var soundId);
                        GameSystems.Dialog.PlayCritterVoiceLine(victim, attacker, text, soundId);
                    }
                }
            }

            var subdualDamage = damagePacket.GetOverallDamageByType(DamageType.Subdual);
            if (subdualDamage > 0)
            {
                victim.AddCondition(StatusEffects.Damaged, subdualDamage);

                // Increase subdual damage (This was previously always done, even for subdualDamage == 0)
                var currentSubdualDamage = victim.GetInt32(obj_f.critter_subdual_damage);
                GameSystems.MapObject.ChangeSubdualDamage(victim, subdualDamage + currentSubdualDamage);
                GameSystems.D20.D20SendSignal(victim, D20DispatcherKey.SIG_HP_Changed, -subdualDamage);
            }

            var floatColor = TextFloaterColor.Red;
            if (attackingCritterType == ObjectType.pc)
            {
                floatColor = TextFloaterColor.White;
            }
            else if (attackingCritterType == ObjectType.npc &&
                     GameSystems.Party.IsInParty(GameSystems.Critter.GetLeaderRecursive(victim)))
            {
                floatColor = TextFloaterColor.Yellow;
            }

            if (realDamage > 0 || subdualDamage == 0)
            {
                var hpText = GetCombatMesLine(D20CombatMessage.hp);
                var text = $"{realDamage} {hpText}";
                GameSystems.TextFloater.FloatLine(victim, TextFloaterCategory.Damage, floatColor, text);
            }

            if (subdualDamage > 0)
            {
                var hpText = GetCombatMesLine(D20CombatMessage.nonlethal);
                var text = $"{subdualDamage} {hpText}";
                GameSystems.TextFloater.FloatLine(victim, TextFloaterCategory.Damage, floatColor, text);
            }

            if (attacker != null && !tgtIsProne)
            {
                GameSystems.Anim.PushHitByWeapon(victim, attacker);
            }
        }

        [TempleDllLocation(0x100b86c0)]
        public bool TargetWithinReachOfLoc(GameObject attacker, GameObject target, LocAndOffsets loc)
        {
            var reach = attacker.GetReach();
            var radiusFt = attacker.GetRadius() / locXY.INCH_PER_FEET; // Conversion to feet
            var distance = target.DistanceToLocInFeet(loc);
            if (distance < 0.0f)
            {
                distance = 0.0f;
            }

            return distance - radiusFt <= reach;
        }

        // miss chances handling
        private int GetDefenderConcealmentMissChance(GameObject attacker, GameObject victim, D20Action d20a)
        {
            if (attacker.HasCondition(SpellEffects.SpellTrueStrike))
            {
                return 0;
            }

            /* TODO
            if (attacker.HasCondition(BuiltInConditions.WeaponSeeking))
            {
                return 0;
            }
            */

            if (GameSystems.Critter.CanSeeWithBlindsight(attacker, victim))
            {
                return 0;
            }

            var dispIo = DispIoAttackBonus.Default;
            dispIo.attackPacket.flags = d20a.d20Caf;
            dispIo.attackPacket.victim = victim;
            dispIo.attackPacket.attacker = attacker;
            return victim.DispatchDefenderConcealmentMissChance(dispIo);
        }

        private bool IsMiss(int roll, int toHitBon, int tgtAc)
        {
            if (AlwaysCrit)
            {
                return false;
            }

            return roll == 1 || roll != 20 && roll + toHitBon < tgtAc;
        }

        [TempleDllLocation(0x100B7160)]
        public void ToHitProcessing(D20Action action)
        {
            var performer = action.d20APerformer;
            var d20Data = action.data1;
            var tgt = action.d20ATarget;
            if (tgt == null)
                return;

            // mirror image processing
            var numMirrorImages = GameSystems.D20.D20QueryInt(tgt, D20DispatcherKey.QUE_Critter_Has_Mirror_Image);
            if (numMirrorImages > 0)
            {
                int spellId = (int)GameSystems.D20.D20QueryReturnData(tgt, D20DispatcherKey.QUE_Critter_Has_Mirror_Image);
                var spellPkt = GameSystems.Spell.GetActiveSpell(spellId);
                var dice = new Dice(1, 1 + numMirrorImages);
                if (dice.Roll() != 1)
                {
                    // mirror image nominally struck
                    var mirrorImAc = DispIoAttackBonus.Default;
                    mirrorImAc.attackPacket.flags = action.d20Caf | D20CAF.TOUCH_ATTACK;
                    mirrorImAc.attackPacket.d20ActnType = action.d20ActType;
                    mirrorImAc.attackPacket.attacker = performer;
                    mirrorImAc.attackPacket.victim = tgt;
                    var mirrorImageAc = GameSystems.Stat.GetAC(tgt, mirrorImAc);
                    var mirrorImToHit = DispIoAttackBonus.Default;
                    GameSystems.Stat.DispatchAttackBonus(performer, null, ref mirrorImToHit, DispatcherType.ToHitBonus2,
                        D20DispatcherKey.NONE);

                    var spName = spellPkt.GetName();
                    var dispelRes = GameSystems.Spell.DispelRoll(performer, mirrorImToHit.bonlist, 0, mirrorImageAc,
                        spName, out action.rollHistId0);
                    if (dispelRes >= 0)
                    {
                        GameSystems.D20.D20SendSignal(tgt, D20DispatcherKey.SIG_Spell_Mirror_Image_Struck,
                            spellPkt.spellId, 0);
                        GameSystems.D20.Combat.FloatCombatLine(tgt, 109);
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(10, performer, tgt);
                        return;
                    }
                }
            }

            var defenderMissChance = GetDefenderConcealmentMissChance(performer, tgt, action);
            var attackerMissChance = performer.DispatchAttackerConcealmentMissChance();
            if (defenderMissChance > 0 || attackerMissChance > 0)
            {
                if (attackerMissChance > defenderMissChance)
                    defenderMissChance = attackerMissChance;

                // roll miss chance
                var missChanceRoll = Dice.D100.Roll();
                if (missChanceRoll > defenderMissChance)
                {
                    // success
                    action.rollHistId1 = GameSystems.RollHistory.AddPercentageCheck(performer, tgt, defenderMissChance,
                        60, missChanceRoll, 194, 193);
                }
                else
                {
                    // failure
                    action.rollHistId1 = GameSystems.RollHistory.AddPercentageCheck(performer, tgt, defenderMissChance,
                        60, missChanceRoll, 195, 193);

                    // Blind Fight handling (second chance)
                    if (!GameSystems.Feat.HasFeat(performer, FeatId.BLIND_FIGHT))
                        return;
                    missChanceRoll = Dice.D100.Roll();
                    if (missChanceRoll <= defenderMissChance)
                    {
                        GameSystems.RollHistory.AddPercentageCheck(performer, tgt, defenderMissChance, 61,
                            missChanceRoll, 195, 193);
                        return;
                    }

                    action.rollHistId2 = GameSystems.RollHistory.AddPercentageCheck(performer, tgt, defenderMissChance,
                        61, missChanceRoll, 194, 193);
                }
            }

            // get the To Hit bonus
            DispIoAttackBonus dispIoToHitBon = DispIoAttackBonus.Default;
            DispIoAttackBonus dispIoAtkBon = DispIoAttackBonus.Default;
            DispIoAttackBonus dispIoTgtAc = DispIoAttackBonus.Default;
            dispIoToHitBon.attackPacket.flags = action.d20Caf;
            dispIoToHitBon.attackPacket.victim = tgt;
            dispIoToHitBon.attackPacket.d20ActnType = action.d20ActType;
            dispIoToHitBon.attackPacket.attacker = performer;
            dispIoToHitBon.attackPacket.dispKey = d20Data;
            if ((action.d20Caf & D20CAF.TOUCH_ATTACK) != 0)
            {
                dispIoToHitBon.attackPacket.weaponUsed = null;
            }
            else
            {
                if ((action.d20Caf & D20CAF.SECONDARY_WEAPON) != D20CAF.NONE)
                {
                    dispIoToHitBon.attackPacket.weaponUsed =
                        GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);
                }
                else
                {
                    dispIoToHitBon.attackPacket.weaponUsed =
                        GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
                }
            }

            // adds buckler penalty to the bonus list
            GameSystems.Stat.DispatchAttackBonus(performer, null, ref dispIoToHitBon, DispatcherType.BucklerAcPenalty,
                D20DispatcherKey.NONE);

            if (dispIoToHitBon.attackPacket.weaponUsed != null
                && dispIoToHitBon.attackPacket.weaponUsed.type != ObjectType.weapon)
            {
                dispIoToHitBon.attackPacket.weaponUsed = null;
            }

            dispIoToHitBon.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(performer);
            dispIoToHitBon.attackPacket.flags |= D20CAF.FINAL_ATTACK_ROLL;
            // note: the "Global" condition has ToHitBonus2 hook that dispatches the ToHitBonusBase
            GameSystems.Stat.DispatchAttackBonus(performer, null, ref dispIoToHitBon, DispatcherType.ToHitBonus2,
                D20DispatcherKey.NONE);

            var toHitBonFinal = GameSystems.Stat.DispatchAttackBonus(tgt, null, ref dispIoToHitBon,
                DispatcherType.ToHitBonusFromDefenderCondition,
                D20DispatcherKey.NONE);

            dispIoTgtAc.attackPacket = dispIoToHitBon.attackPacket;
            GameSystems.Stat.GetAC(tgt, dispIoTgtAc);

            var tgtAcFinal = GameSystems.Stat.DispatchAttackBonus(performer, null, ref dispIoTgtAc,
                DispatcherType.AcModifyByAttacker,
                D20DispatcherKey.NONE);

            var toHitRoll = Dice.D20.Roll();

            if ((dispIoToHitBon.attackPacket.flags & D20CAF.ALWAYS_HIT) == 0 && !AlwaysCrit)
            {
                // check miss
                if (IsMiss(toHitRoll, toHitBonFinal, tgtAcFinal))
                {
                    if (GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_RerollAttack))
                    {
                        GameSystems.RollHistory.AddAttackRoll(toHitRoll, -1, performer, tgt,
                            dispIoToHitBon.bonlist, dispIoTgtAc.bonlist, dispIoToHitBon.attackPacket.flags);
                        toHitRoll = Dice.Roll(1, 20);
                        dispIoToHitBon.attackPacket.flags |= D20CAF.REROLL;
                    }
                }

                // still a miss
                if (IsMiss(toHitRoll, toHitBonFinal, tgtAcFinal))
                {
                    GameUiBridge.LogbookCombatMiss(performer);
                }
            }

            var critHitRoll = -1;
            if (!IsMiss(toHitRoll, toHitBonFinal, tgtAcFinal) ||
                (dispIoToHitBon.attackPacket.flags & D20CAF.ALWAYS_HIT) != 0)
            {
                // register a hit
                dispIoToHitBon.attackPacket.flags |= D20CAF.HIT;
                GameUiBridge.LogbookCombatHit(performer);

                // do Critical Hit roll
                dispIoAtkBon.attackPacket = dispIoToHitBon.attackPacket;

                var critThreatRange = 21 - GameSystems.Stat.DispatchAttackBonus(performer, null, ref dispIoAtkBon,
                    DispatcherType.GetCriticalHitRange,
                    D20DispatcherKey.NONE);
                if (!GameSystems.D20.D20Query(tgt, D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits))
                {
                    if (toHitRoll >= critThreatRange || AlwaysCrit)
                    {
                        // Add bonuses that only apply to the attack bonus for the confirmation roll (unfortunately, only this
                        // bonus list will be used by the log)
                        var dispatcher = performer.GetDispatcher();
                        dispatcher?.Process(DispatcherType.ConfirmCriticalBonus, D20DispatcherKey.NONE, dispIoToHitBon);
                        toHitBonFinal = dispIoToHitBon.bonlist.OverallBonus;

                        critHitRoll = Dice.D20.Roll(); // Roll to confirm critical hit

                        // RerollCritical handling (e.g. from Luck domain)
                        if (IsMiss(critHitRoll, toHitBonFinal, tgtAcFinal) &&
                            GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_RerollCritical))
                        {
                            GameSystems.RollHistory.AddAttackRoll(toHitRoll, critHitRoll, performer, tgt,
                                dispIoToHitBon.bonlist, dispIoTgtAc.bonlist, dispIoToHitBon.attackPacket.flags);
                            critHitRoll = Dice.Roll(1, 20);
                        }

                        if (!IsMiss(critHitRoll, toHitBonFinal, tgtAcFinal))
                        {
                            dispIoToHitBon.attackPacket.flags |= D20CAF.CRITICAL;
                        }
                    }
                }

                // do Deflect Arrow dispatch
                GameSystems.Stat.DispatchAttackBonus(dispIoToHitBon.attackPacket.victim, null, ref dispIoToHitBon,
                    DispatcherType.DeflectArrows,
                    D20DispatcherKey.NONE);
            }

            // sphagetti handling for Sanctuary spell
            if (action.d20ATarget != null && action.d20ATarget.IsCritter()
                                          && GameSystems.D20.D20QueryWithObject(action.d20ATarget,
                                              D20DispatcherKey.QUE_CanBeAffected_PerformAction, action,
                                              defaultResult: 1) == 0)
            {
                if ((dispIoToHitBon.attackPacket.flags & D20CAF.CRITICAL) != 0)
                {
                    dispIoToHitBon.attackPacket.flags &= ~D20CAF.CRITICAL;
                }

                if ((dispIoToHitBon.attackPacket.flags & D20CAF.HIT) != 0)
                {
                    dispIoToHitBon.attackPacket.flags &= ~D20CAF.HIT;
                    dispIoToHitBon.bonlist.zeroBonusSetMeslineNum(262);
                }
            }

            // all this hard work just to set a couple of flags :D
            action.d20Caf = dispIoToHitBon.attackPacket.flags;
            action.rollHistId0 = GameSystems.RollHistory.AddAttackRoll(toHitRoll, critHitRoll, performer, tgt,
                dispIoToHitBon.bonlist, dispIoTgtAc.bonlist, dispIoToHitBon.attackPacket.flags);

            // there were some additional debug stubs here (nullsubs)
        }


        [TempleDllLocation(0x100b8600)]
        public bool CanMeleeTargetAtLocation(GameObject attacker, GameObject defender, LocAndOffsets loc)
        {
            if (GameSystems.Critter.IsDeadOrUnconscious(attacker))
            {
                return false;
            }

            var weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
            if (CanAttackTargetAtLocRegardItem(attacker, weapon, defender, loc))
            {
                return true;
            }

            weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
            if (CanAttackTargetAtLocRegardItem(attacker, weapon, defender, loc))
            {
                return true;
            }

            if (!attacker.HasNaturalAttacks())
            {
                return false;
            }

            var reachFt = attacker.GetReach();
            var distFt = attacker.DistanceToLocInFeet(loc);
            var targetRadiusFt = defender.GetRadius() / locXY.INCH_PER_FEET;

            return reachFt >= distFt - targetRadiusFt;
        }

        [TempleDllLocation(0x100b6a50)]
        private bool CanAttackTargetAtLocRegardItem(GameObject obj, GameObject weapon, GameObject targetObj,
            LocAndOffsets loc)
        {
            if (weapon != null)
            {
                if (weapon.type != ObjectType.weapon)
                {
                    return false;
                }

                if (weapon.WeaponFlags.HasFlag(WeaponFlag.RANGED_WEAPON))
                {
                    return false;
                }
            }
            else
            {
                // TODO: It seems more sensible to test for natural attacks here...
                var critterFlags = obj.GetCritterFlags();
                if (!critterFlags.HasFlag(CritterFlag.MONSTER) &&
                    !GameSystems.Feat.HasFeat(obj, FeatId.IMPROVED_UNARMED_STRIKE))
                {
                    return false;
                }
            }

            var reachFt = obj.GetReach();
            var distFt = obj.DistanceToLocInFeet(loc);
            var targetRadiusFt = targetObj.GetRadius() / locXY.INCH_PER_FEET;

            return reachFt >= distFt - targetRadiusFt;
        }

        [TempleDllLocation(0x100b5270)]
        public bool IsWithinThreeFeet(GameObject obj, GameObject target, LocAndOffsets loc)
        {
            var radiusFt = target.GetRadius() / locXY.INCH_PER_FEET;
            var distFt = obj.DistanceToLocInFeet(loc) - radiusFt;
            return distFt < 3.0f;
        }

        // The size of increments in which we search for interrupts along the path
        private const float InterruptSearchIncrement = 4.0f;

        [TempleDllLocation(0x100b8b40)]
        public bool FindAttacksOfOpportunity(GameObject mover, Path path, float aooFreeDistFeet,
            out List<AttackOfOpportunity> attacks)
        {
            // aooFreeDistFeet specifies the minimum distance traveled before an AoO is registered (e.g. for Withdrawal it will receive 5 feet)
            attacks = null;

            var aooDistFeet = aooFreeDistFeet;

            var pathLength = path.GetPathResultLength();
            if (aooDistFeet > pathLength)
            {
                return false;
            }

            var enemies = GetHostileCombatantList(mover);

            while (aooDistFeet < pathLength - InterruptSearchIncrement / 2)
            {
                // this is the first possible distance where an Aoo might occur
                GameSystems.PathX.TruncatePathToDistance(path, out var truncatedLoc, aooDistFeet);

                // obj is moving away from truncatedLoc
                // if an enemy can hit you from when you're in the current truncatedLoc
                // it means you incur an AOO

                // loop over enemies to catch interceptions
                foreach (var enemy in enemies)
                {
                    // Check if this enemy already has an attack of opportunity in the list
                    var hasInterrupted = false;
                    if (attacks != null)
                    {
                        foreach (var attack in attacks)
                        {
                            if (enemy == attack.Interrupter)
                            {
                                hasInterrupted = true;
                                break;
                            }
                        }
                    }

                    if (!hasInterrupted)
                    {
                        if (GameSystems.D20.D20QueryWithObject(enemy, D20DispatcherKey.QUE_AOOPossible, mover) != 0)
                        {
                            if (GameSystems.D20.Combat.CanMeleeTargetAtLocation(enemy, mover, truncatedLoc))
                            {
                                if (attacks == null)
                                {
                                    attacks = new List<AttackOfOpportunity>();
                                }

                                attacks.Add(new AttackOfOpportunity(enemy, aooDistFeet, truncatedLoc));
                            }
                        }
                    }
                }

                // advanced the truncatedLoc by 4 feet along the path
                aooDistFeet += InterruptSearchIncrement;
            }

            return attacks != null;
        }

        public List<GameObject> GetHostileCombatantList(GameObject critter)
        {
            var result = new List<GameObject>();
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (critter != combatant && !GameSystems.Critter.IsFriendly(critter, combatant))
                {
                    result.Add(combatant);
                }
            }

            return result;
        }

        public IEnumerable<GameObject> EnumerateEnemiesInRange(GameObject critter, float rangeFeet)
        {
            var perfLoc = critter.GetLocationFull();

            using var enemies =
                ObjList.ListRadius(perfLoc, rangeFeet * locXY.INCH_PER_FEET, ObjectListFilter.OLC_CRITTERS);
            foreach (var enemy in enemies)
            {
                if (GameSystems.Critter.IsDeadNullDestroyed(enemy))
                {
                    continue;
                }

                if (enemy != critter
                    && !GameSystems.Critter.NpcAllegianceShared(enemy, critter)
                    && !GameSystems.Critter.IsFriendly(critter, enemy))
                {
                    yield return enemy;
                }
            }
        }

        [TempleDllLocation(0x100b4830)]
        public float EstimateDistance(GameObject performer, locXY destLoc, int isZero, double d)
        {
            var animPathSpec = new AnimPathData();
            animPathSpec.srcLoc = performer.GetLocation();
            animPathSpec.destLoc = destLoc;
            animPathSpec.size = 200;
            animPathSpec.handle = performer;
            sbyte[] deltas = new sbyte[200];
            animPathSpec.deltas = deltas;
            animPathSpec.flags = AnimPathDataFlags.UNK40 | AnimPathDataFlags.UNK10;

            if (animPathSpec.srcLoc.EstimateDistance(destLoc) * 2.5f > isZero)
            {
                var animPathSearchResult = GameSystems.PathX.AnimPathSearch(ref animPathSpec);
                if (animPathSearchResult != 0)
                {
                    var distance = animPathSearchResult * 2.5f;
                    var radiusFt = performer.GetRadius() / locXY.INCH_PER_FEET;
                    return distance - radiusFt;
                }
                else
                {
                    return -1.0f;
                }
            }
            else
            {
                return 0.0f;
            }
        }

        [TempleDllLocation(0x100b4d00)]
        public GameObject CreateProjectileAndThrow(LocAndOffsets sourceLoc, int protoId, int missX, int missY,
            LocAndOffsets targetLoc, GameObject attacker, GameObject target)
        {
            if (sourceLoc.location == targetLoc.location)
            {
                return null;
            }

            var projectile = GameSystems.MapObject.CreateObject((ushort) protoId, sourceLoc.location);
            if (projectile == null)
            {
                return null;
            }

            projectile.SetInt32(obj_f.projectile_flags_combat_damage, 0);
            projectile.SetInt32(obj_f.projectile_flags_combat, 0);
            projectile.SetObject(obj_f.projectile_parent_weapon, null);
            projectile.SetObject(obj_f.projectile_parent_weapon, null);

            GameSystems.Anim.PushThrowProjectile(attacker, projectile, missX, missY, target, targetLoc, 1);
            return projectile;
        }

        [TempleDllLocation(0x100b4970)]
        public int GetToHitChance(D20Action action)
        {
            var attacker = action.d20ATarget;
            var attackType = action.data1;

            DispIoAttackBonus dispIo = DispIoAttackBonus.Default;
            dispIo.attackPacket.victim = attacker;
            dispIo.attackPacket.flags = action.d20Caf;
            dispIo.attackPacket.attacker = action.d20APerformer;
            dispIo.attackPacket.dispKey = attackType;
            dispIo.attackPacket.d20ActnType = action.d20ActType;
            GameObject weapon;
            if (action.d20Caf.HasFlag(D20CAF.SECONDARY_WEAPON))
            {
                weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponSecondary);
            }
            else
            {
                weapon = GameSystems.Item.ItemWornAt(action.d20APerformer, EquipSlot.WeaponPrimary);
            }

            if (weapon != null && weapon.type == ObjectType.weapon)
            {
                dispIo.attackPacket.weaponUsed = weapon;
            }

            dispIo.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(action.d20APerformer);

            GameSystems.Stat.Dispatch16GetToHitBonus(action.d20APerformer, dispIo);
            var attackBonus = attacker.DispatchGetToHitModifiersFromDefender(dispIo);

            // Reuse the attack packet to query the AC
            var acDispIo = new DispIoAttackBonus();
            acDispIo.attackPacket = dispIo.attackPacket;
            acDispIo.bonlist = BonusList.Default;

            GameSystems.Stat.GetAC(attacker, acDispIo);
            var ac = action.d20APerformer.DispatchGetAcAdjustedByAttacker(acDispIo);

            var result = 5 * (attackBonus - ac + 20);
            return Math.Clamp(result, 5, 95);
        }

        [TempleDllLocation(0x100b9500)]
        public bool ReflexSaveAndDamage(GameObject victim, GameObject attacker, int dc,
            D20SavingThrowReduction reduction, D20SavingThrowFlag savingThrowFlags,
            Dice attackDice, DamageType attackType, D20AttackPower attackPower, D20ActionType actionType, int spellId)
        {
            DispIoReflexThrow savingThrowIo = DispIoReflexThrow.Default;

            savingThrowIo.reduction = reduction;
            savingThrowIo.attackPower = attackPower;
            savingThrowIo.damageMesLine = 105;
            savingThrowIo.attackType = attackType;
            savingThrowIo.flags = savingThrowFlags;
            savingThrowIo.throwResult =
                GameSystems.D20.Combat.SavingThrow(victim, attacker, dc, SavingThrowType.Reflex, savingThrowFlags);

            D20CAF flags = default;
            if (savingThrowIo.throwResult)
            {
                flags = D20CAF.SAVE_SUCCESSFUL;
                switch (reduction)
                {
                    case D20SavingThrowReduction.None:
                        savingThrowIo.effectiveReduction = 0;
                        break;
                    case D20SavingThrowReduction.Quarter:
                        savingThrowIo.effectiveReduction = 25;
                        break;
                    case D20SavingThrowReduction.Half:
                        savingThrowIo.effectiveReduction = 50;
                        break;
                    default:
                        savingThrowIo.effectiveReduction = 100;
                        break;
                }
            }

            victim.GetDispatcher()?.Process(DispatcherType.ReflexThrow, D20DispatcherKey.NONE, savingThrowIo);

            if (savingThrowIo.effectiveReduction == 100)
            {
                if (actionType == D20ActionType.CAST_SPELL)
                    DealSpellDamage(
                        victim,
                        attacker,
                        attackDice,
                        savingThrowIo.attackType,
                        savingThrowIo.attackPower,
                        100,
                        103,
                        actionType,
                        spellId,
                        flags);
                else
                    DoDamage(
                        victim,
                        attacker,
                        attackDice,
                        savingThrowIo.attackType,
                        savingThrowIo.attackPower,
                        100,
                        103,
                        actionType);
            }
            else if (actionType == D20ActionType.CAST_SPELL)
            {
                DealSpellDamage(
                    victim,
                    attacker,
                    attackDice,
                    savingThrowIo.attackType,
                    savingThrowIo.attackPower,
                    savingThrowIo.effectiveReduction,
                    savingThrowIo.damageMesLine,
                    actionType,
                    spellId,
                    flags);
            }
            else
            {
                DoDamage(
                    victim,
                    attacker,
                    attackDice,
                    savingThrowIo.attackType,
                    savingThrowIo.attackPower,
                    savingThrowIo.effectiveReduction,
                    savingThrowIo.damageMesLine,
                    actionType);
            }

            return savingThrowIo.throwResult;
        }

        [TempleDllLocation(0x100b9080)]
        public void SpellDamageFull(GameObject target, GameObject attacker, Dice dicePacked, DamageType damType,
            D20AttackPower attackPower, D20ActionType actionType, int spellId, D20CAF d20caf)
        {
            DealSpellDamage(target, attacker, dicePacked, damType, attackPower, 100, 103, actionType, spellId, d20caf);
        }

        [TempleDllLocation(0x100b7f80)]
        public void DealSpellDamage(GameObject tgt, GameObject attacker, Dice dice, DamageType type,
            D20AttackPower attackPower, int reduction, int damageDescId, D20ActionType actionType, int spellId,
            D20CAF flags)
        {
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spPkt))
            {
                return;
            }

            if (attacker != null && attacker != tgt && GameSystems.Critter.NpcAllegianceShared(tgt, attacker))
            {
                FloatCombatLine(tgt, D20CombatMessage.friendly_fire);
            }

            GameSystems.AI.ProvokeHostility(attacker, tgt, 1, 0);

            if (GameSystems.Critter.IsDeadNullDestroyed(tgt))
            {
                return;
            }

            DispIoDamage evtObjDam = new DispIoDamage();
            evtObjDam.attackPacket.d20ActnType = actionType;
            evtObjDam.attackPacket.attacker = attacker;
            evtObjDam.attackPacket.victim = tgt;
            evtObjDam.attackPacket.dispKey = 1;
            evtObjDam.attackPacket.flags = flags | D20CAF.HIT;

            if (attacker != null && attacker.IsCritter())
            {
                GameObject weapon;
                if (flags.HasFlag(D20CAF.SECONDARY_WEAPON))
                {
                    weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                }
                else
                {
                    weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                }

                if (weapon != null && weapon.type == ObjectType.weapon)
                {
                    evtObjDam.attackPacket.weaponUsed = weapon;
                }

                evtObjDam.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
            }
            else
            {
                evtObjDam.attackPacket.weaponUsed = null;
                evtObjDam.attackPacket.ammoItem = null;
            }

            if (reduction != 100)
            {
                evtObjDam.damage.AddModFactor(reduction * 0.01f, type, damageDescId);
            }

            evtObjDam.damage.AddDamageDice(dice, type, 103);
            evtObjDam.damage.AddAttackPower(attackPower);
            var mmData = spPkt.metaMagicData;
            if (mmData.metaMagicEmpowerSpellCount > 0)
                evtObjDam.damage.flags |= 2; // empowered
            if (mmData.metaMagicFlags.HasFlag(MetaMagicFlags.MetaMagic_Maximize))
                evtObjDam.damage.flags |= 1; // maximized

            attacker.DispatchSpellDamage(evtObjDam.damage, tgt, spPkt);

            _lastDamageFromAttack = false; // is weapon damage (used in logbook for record holding)

            DamageCritter(attacker, tgt, evtObjDam);
        }

        [TempleDllLocation(0x100b94c0)]
        public void DoUnclassifiedDamage(GameObject target, GameObject attacker, Dice dmgDice,
            DamageType damageType,
            D20AttackPower attackPowerType, D20ActionType actionType)
        {
            DoDamage(target, attacker, dmgDice, damageType, attackPowerType, 100, 103, actionType);
        }

        [TempleDllLocation(0x100b8d70)]
        public void DoDamage(GameObject target, GameObject attacker, Dice dmgDice, DamageType damageType,
            D20AttackPower attackPowerType, int reduction, int damageDescMesKey, D20ActionType actionType)
        {
            Trace.Assert(target != null);

            var wasDeadOrUnconscious = GameSystems.Critter.IsDeadOrUnconscious(target);

            if (attacker != null && target != attacker && GameSystems.Combat.AffiliationSame(target, attacker))
            {
                FloatCombatLine(target, D20CombatMessage.friendly_fire);
            }

            if (attacker != null && attacker.IsCritter())
            {
                GameSystems.AI.ProvokeHostility(attacker, target, 1, 0);
            }

            if (!GameSystems.Critter.IsDeadNullDestroyed(target))
            {
                var dispIo = new DispIoDamage();
                dispIo.attackPacket.d20ActnType = actionType;
                dispIo.attackPacket.attacker = attacker;
                dispIo.attackPacket.victim = target;
                dispIo.attackPacket.dispKey = 1;
                dispIo.attackPacket.flags = D20CAF.HIT;
                if (attacker != null)
                {
                    if (IsTrapped(attacker))
                    {
                        dispIo.attackPacket.flags |= D20CAF.TRAP;
                    }

                    if (dispIo.attackPacket.flags.HasFlag(D20CAF.SECONDARY_WEAPON))
                    {
                        dispIo.attackPacket.weaponUsed =
                            GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                    }
                    else
                    {
                        dispIo.attackPacket.weaponUsed = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                    }

                    if (attacker.IsCritter())
                    {
                        dispIo.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
                    }
                    else
                    {
                        dispIo.attackPacket.ammoItem = null;
                    }
                }

                if (dispIo.attackPacket.weaponUsed != null && dispIo.attackPacket.weaponUsed.type != ObjectType.weapon)
                {
                    dispIo.attackPacket.weaponUsed = null;
                }

                if (reduction != 100)
                {
                    var dmgFactor = reduction * 0.01f;
                    dispIo.damage.AddModFactor(dmgFactor, damageType, damageDescMesKey);
                }

                dispIo.damage.AddDamageDice(dmgDice, damageType, damageDescMesKey);
                dispIo.damage.AddAttackPower(attackPowerType);
                _lastDamageFromAttack = true;
                DamageCritter(attacker, target, dispIo);

                if (!wasDeadOrUnconscious && GameSystems.Critter.IsDeadOrUnconscious(target))
                {
                    if (attacker == null || attacker == target || GameSystems.Party.IsInParty(attacker))
                    {
                        if (!GameSystems.Party.IsInParty(target) &&
                            !target.GetCritterFlags().HasFlag(CritterFlag.EXPERIENCE_AWARDED))
                        {
                            GameSystems.D20.Combat.AwardExperienceForKill(attacker, target);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100b6950)]
        public bool IsTrapped(GameObject obj)
        {
            return (obj.type == ObjectType.portal || obj.type == ObjectType.container)
                   && GameSystems.Trap.WillTrigger(obj);
        }

        [TempleDllLocation(0x100b64c0)]
        public bool TryFeint(GameObject attacker, GameObject defender)
        {
            var attackerRoll = Dice.D20.Roll();
            var defenderRoll = Dice.D20.Roll();

            var attackerBonus = BonusList.Default;
            var defenderBonus = BonusList.Default;
            attacker.dispatch1ESkillLevel(SkillId.bluff, ref attackerBonus, defender, 0);
            defender.dispatch1ESkillLevel(SkillId.sense_motive, ref defenderBonus, attacker, 0);

            if (GameSystems.Stat.StatLevelGet(defender, Stat.intelligence) <= 2)
            {
                // A rock is hard to convince that you're attack it
                attackerBonus.AddBonus(-8, 0, 290);
            }
            else if (!GameSystems.Critter.IsCategory(defender, MonsterCategory.humanoid))
            {
                // An ooze might mistake your spasmic movement for a mating ritual
                attackerBonus.AddBonus(-4, 0, 291);
            }

            var defenderBab = defender.DispatchToHitBonusBase();
            defenderBonus.AddBonus(defenderBab, 0, 118);
            var success = attackerRoll + attackerBonus.OverallBonus > defenderRoll + defenderBonus.OverallBonus;
            var mesLineResult = success ? D20CombatMessage.attempt_succeeds : D20CombatMessage.attempt_fails;
            var histId = GameSystems.RollHistory.AddOpposedCheck(
                attacker,
                defender,
                attackerRoll,
                defenderRoll,
                attackerBonus,
                defenderBonus,
                153,
                mesLineResult,
                0);
            GameSystems.RollHistory.CreateRollHistoryString /*0x100dfff0*/(histId);
            return success;
        }

        [TempleDllLocation(0x100b9200)]
        public bool IsFlankedBy(GameObject victim, GameObject attacker)
        {
            if (victim == null)
            {
                return false;
            }

            if (!GameSystems.Combat.CanMeleeTarget(attacker, victim))
            {
                return false;
            }

            if (GameSystems.D20.D20QueryWithObject(victim, D20DispatcherKey.QUE_CanBeFlanked, attacker) == 0)
            {
                return false;
            }

            var attackerPos = attacker.GetLocationFull().ToInches2D();
            var victimPos = victim.GetLocationFull().ToInches2D();
            var victimToAttackerDir = Vector2.Normalize(attackerPos - victimPos);

            var enemies = GameSystems.Combat.GetEnemiesCanMelee(victim);
            var cos120deg = MathF.Cos(2.0943952f); // 120°

            foreach (var enemy in enemies)
            {
                if (enemy != attacker)
                {
                    var enemyPos = enemy.GetLocationFull().ToInches2D();
                    var victimToEnemyDir = Vector2.Normalize(enemyPos - victimPos);

                    // This works out to be a 120° wide angle behind the victim
                    if (Vector2.Dot(victimToAttackerDir, victimToEnemyDir) < cos120deg)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x100b9160)]
        public bool HasThreateningCrittersAtLoc(GameObject obj, LocAndOffsets loc)
        {
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant != obj && !GameSystems.Combat.AffiliationSame(obj, combatant))
                {
                    if (CanMeleeTargetAtLocation(combatant, obj, loc))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x100b4b00)]
        public string GetSaveTypeName(SavingThrowType saveType)
        {
            switch (saveType)
            {
                case SavingThrowType.Fortitude:
                    return GetCombatMesLine(D20CombatMessage.fortitude);
                case SavingThrowType.Reflex:
                    return GetCombatMesLine(D20CombatMessage.reflex);
                case SavingThrowType.Will:
                    return GetCombatMesLine(D20CombatMessage.will);
                default:
                    throw new ArgumentOutOfRangeException(nameof(saveType), saveType, null);
            }
        }


        [TempleDllLocation(0x100b8990)]
        public void KillWithDestroyEffect(GameObject obj, GameObject killer)
        {
            if (DoOnDeathScripts(obj, killer))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(51, obj, killer);
                FloatCombatLine(obj, 187);
                GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Killed, killer);
                obj.AddCondition(StatusEffects.Dead);
                AwardExperienceForKill(killer, obj);
            }
        }

        [TempleDllLocation(0x100b82e0)]
        public GameObject GetCleaveTarget(GameObject objHnd)
        {
            // TODO This actually seems incorrect given that cleave has to use the reach of the weapon that is being cleaved with
            var reach = objHnd.GetReach();
            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (!GameSystems.Combat.AffiliationSame(objHnd, combatant)
                    && !GameSystems.Critter.IsDeadOrUnconscious(combatant)
                    && !GameSystems.D20.Actions.IsCurrentlyActing(combatant))
                {
                    var distance = objHnd.DistanceToObjInFeet(combatant);
                    if (distance < reach)
                    {
                        return combatant;
                    }
                }
            }

            return null;
        }

        public void DealWeaponlikeSpellDamage(GameObject tgt, GameObject attacker, Dice dice, DamageType type,
            D20AttackPower attackPower, int damFactor, int damageDescId, D20ActionType actionType, int spellId,
            D20CAF flags, int projectileIdx = 0)
        {
            var spPkt = GameSystems.Spell.GetActiveSpell(spellId);

            if (attacker != null && attacker != tgt && GameSystems.Critter.NpcAllegianceShared(tgt, attacker))
            {
                GameSystems.D20.Combat.FloatCombatLine(tgt, 107); // friendly fire
            }

            GameSystems.AI.ProvokeHostility(attacker, tgt, 1, 0);

            if (GameSystems.Critter.IsDeadNullDestroyed(tgt))
                return;

            if (IsFlankedBy(tgt, attacker))
            {
                flags |= D20CAF.FLANKED;
            }

            DispIoDamage evtObjDam = DispIoDamage.Create(attacker, tgt);
            evtObjDam.attackPacket.d20ActnType = actionType;
            evtObjDam.attackPacket.dispKey = projectileIdx; // TODO: param should be attack code
            evtObjDam.attackPacket.flags = flags | D20CAF.HIT;

            if (attacker != null && attacker.IsCritter())
            {
                // TODO: Move all of this junk to a utility (setweaponfromequipment?)
                if ((flags & D20CAF.SECONDARY_WEAPON) != D20CAF.NONE)
                    evtObjDam.attackPacket.weaponUsed =
                        GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
                else
                    evtObjDam.attackPacket.weaponUsed = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);

                if (evtObjDam.attackPacket.weaponUsed != null &&
                    evtObjDam.attackPacket.weaponUsed.type != ObjectType.weapon)
                    evtObjDam.attackPacket.weaponUsed = null;

                evtObjDam.attackPacket.ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
            }

            if (damFactor != 100)
            {
                evtObjDam.damage.AddModFactor(damFactor * 0.01f, type, damageDescId);
            }

            if ((flags & D20CAF.CONCEALMENT_MISS) != 0)
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(11, attacker, tgt);
                GameSystems.D20.Combat.FloatCombatLine(attacker, 45); // Miss (Concealment)!
                // GameSystems.D20.D20SendSignal(attacker, D20DispatcherKey.SIG_Attack_Made, (int)&evtObjDam, 0); // casting a spell isn't considered an attack action
                return;
            }

            if ((flags & D20CAF.HIT) == 0)
            {
                GameSystems.D20.Combat.FloatCombatLine(attacker, 29);

                // dodge animation
                if (!GameSystems.Critter.IsDeadOrUnconscious(tgt) && !GameSystems.Critter.IsProne(tgt))
                {
                    GameSystems.Anim.PushDodge(attacker, tgt);
                }

                return;
            }

            _lastDamageFromAttack = false; // is weapon damage

            // get damage dice
            evtObjDam.damage.AddDamageDice(dice, type, 103);
            evtObjDam.damage.AddAttackPower(attackPower);
            var mmData = spPkt.metaMagicData;
            if (mmData.IsEmpowered)
            {
                evtObjDam.damage.flags |= 2; // empowered
            }

            if (mmData.IsMaximize)
            {
                evtObjDam.damage.flags |= 1; // maximized
            }

            if ((evtObjDam.attackPacket.flags & D20CAF.CRITICAL) != 0)
            {
                var extraHitDice = 1;
                evtObjDam.damage.AddCritMultiplier(extraHitDice + 1, 102);
                GameSystems.D20.Combat.FloatCombatLine(attacker, 12);

                // play sound
                var soundId = GameSystems.SoundMap.GetCritterSoundEffect(tgt, CritterSoundEffect.Attack);
                GameSystems.SoundGame.PositionalSound(soundId, tgt);

                // increase crit hits in logbook
                GameUiBridge.IncreaseCritHits(attacker);
            }

            attacker.DispatchDamage(DispatcherType.DealingDamageWeaponlikeSpell, evtObjDam);
            attacker.DispatchSpellDamage(evtObjDam.damage, tgt, spPkt);

            DamageCritter(attacker, tgt, evtObjDam);
        }

        [TempleDllLocation(0x100b4800)]
        public void Load(SavedD20State savedState)
        {
            _challengeRatingsDefeated = new Dictionary<int, int>(savedState.PendingDefeatedEncounters);
        }

        [TempleDllLocation(0x100b47e0)]
        public void Save(SavedD20State savedState)
        {
            savedState.PendingDefeatedEncounters = new Dictionary<int, int>(_challengeRatingsDefeated);
        }
    }
}