using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Particles.Instances;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public static class BardicMusic
    {
        private static readonly int bardFascEnum = 801;

        [TempleDllLocation(0x102f0520)] [AutoRegister]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Bardic Music", 6)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, BardicMusicInitCallback, 0)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, BardicMusicInitCallback, 1)
            .AddHandler(DispatcherType.RadialMenuEntry, BardicMusicRadial)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_COPY_SCROLL,
                CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_COPY_SCROLL, BardicMusicCheck, 0)
            .AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_COPY_SCROLL,
                BardicMusicActionFrame)
            .AddHandler(DispatcherType.BeginRound, BardicMusicBeginRound)
            .AddSignalHandler(D20DispatcherKey.SIG_Sequence, BardicMusicOnSequence)
            .RemoveOnSignal(D20DispatcherKey.SIG_Killed)
            .AddQueryHandler("Bardic Music Type", GetBardicMusicType)
            .AddSignalHandler("Bardic Music End", BardicMusicEnd)
            .AddQueryHandler("Max Bardic Music", QueryMaxBardicMusic)
            .Build();

        [DispTypes(DispatcherType.D20ActionOnActionFrame)]
        [TempleDllLocation(0x100fe570)]
        [TemplePlusLocation("condition.cpp:487")]
        public static void BardicMusicActionFrame(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();

            var action = dispIo.action;
            var performer = action.d20APerformer;
            var bmType = (BardicMusicSongType) (action.data1);

            /*
             decrease usages left, except for Suggestion
            */
            if (bmType != BardicMusicSongType.BM_SUGGESTION)
            {
                evt.SetConditionArg1(evt.GetConditionArg1() - 1);
            }

            /*
                handle already performing music
            */
            if (evt.GetConditionArg2() != 0)
            {
                evt.SetConditionArg2(0);
                var partsys = evt.GetConditionPartSysArg(5);
                GameSystems.ParticleSys.End(partsys);
                var objHnd = evt.GetConditionObjArg(3);
                // make an exception for Suggestion since it shouldn't abort the Fascinate song
                if (bmType != BardicMusicSongType.BM_SUGGESTION)
                {
                    GameSystems.D20.D20SendSignal(objHnd, D20DispatcherKey.SIG_Bardic_Music_Completed);
                }
            }

            object partsysId = null;
            int spellId;

            var curSeq = GameSystems.D20.Actions.CurrentSequence;
            switch (bmType)
            {
                case BardicMusicSongType.BM_INSPIRE_COURAGE:
                    evt.objHndCaller.AddConditionToPartyAround(30.0f, StatusEffects.InspiredCourage, null);
                    partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Inspire Courage", evt.objHndCaller);
                    break;
                case BardicMusicSongType.BM_COUNTER_SONG:
                    evt.objHndCaller.AddConditionToPartyAround(30.0f, StatusEffects.Countersong, null);
                    partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Countersong", evt.objHndCaller);
                    break;
                case BardicMusicSongType.BM_FASCINATE:
                    partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Fascinate", evt.objHndCaller);
                    spellId = GameSystems.Spell.GetNewSpellId();
                    GameSystems.Spell.RegisterSpell(curSeq.spellPktBody, spellId);
                    GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.SpellEffect);
                    break;
                case BardicMusicSongType.BM_INSPIRE_COMPETENCE:
                    curSeq.spellPktBody.Targets[0].Object.AddCondition("Competence", 0, 0);
                    partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Inspire Competence", evt.objHndCaller);
                    break;
                case BardicMusicSongType.BM_SUGGESTION:
                    partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Suggestion", evt.objHndCaller);
                    spellId = GameSystems.Spell.GetNewSpellId();
                    GameSystems.Spell.RegisterSpell(curSeq.spellPktBody, spellId);
                    GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.SpellEffect);

                    break;
                case BardicMusicSongType.BM_INSPIRE_GREATNESS:
                    partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Inspire Greatness", evt.objHndCaller);
                    spellId = GameSystems.Spell.GetNewSpellId();
                    GameSystems.Spell.RegisterSpell(curSeq.spellPktBody, spellId);
                    GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.SpellEffect);
                    break;
                case BardicMusicSongType.BM_SONG_OF_FREEDOM:
                    spellId = GameSystems.Spell.GetNewSpellId();
                    GameSystems.Spell.RegisterSpell(curSeq.spellPktBody, spellId);
                    GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.SpellEffect);
                    break;
                case BardicMusicSongType.BM_INSPIRE_HEROICS:
                    partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Inspire Courage", evt.objHndCaller);
                    spellId = GameSystems.Spell.GetNewSpellId();
                    GameSystems.Spell.RegisterSpell(curSeq.spellPktBody, spellId);
                    GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.SpellEffect);
                    break;
            }

            BardicMusicPlaySound(bmType, performer, 0);

            evt.SetConditionArg2((int) bmType);
            evt.SetConditionArg3(0);
            evt.SetConditionObjArg(3, action.d20ATarget);
            evt.SetConditionPartSysArg(5, (PartSys) partsysId);
            dispIo.returnVal = 0;
        }

        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100fe470)]
        [TemplePlusLocation("condition.cpp:486")]
        public static void BardicMusicCheck(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();

            var action = dispIo.action;
            var perfSkill = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform);
            var bmType = (BardicMusicSongType) action.data1;

            var currentlySinging = (BardicMusicSongType) evt.GetConditionArg2();
            if (!IsPerformanceSkillSufficient(bmType, perfSkill)
                || (currentlySinging == bmType && !CanSingAgain(bmType)))
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                if (currentlySinging == bmType)
                {
                    GameSystems.TextFloater.FloatLine(evt.objHndCaller, TextFloaterCategory.Generic,
                        TextFloaterColor.Red, "Already Singing");
                }

                return;
            }

            if (evt.GetConditionArg1() <= 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100fe820)]
        [TemplePlusLocation("condition.cpp:488")]
        public static void BardicMusicBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();

            var bmType = (BardicMusicSongType) evt.GetConditionArg2();
            if (bmType == 0)
                return;

            var roundsLasted = evt.GetConditionArg3();
            if (dispIo.data1 <= 1)
            {
                evt.SetConditionArg3(roundsLasted + 1);
                var target = evt.GetConditionObjArg(3);
                switch (bmType)
                {
                    case BardicMusicSongType.BM_INSPIRE_GREATNESS:
                        if (target != null)
                        {
                            int bonusRounds =
                                GameSystems.D20.D20QueryPython(evt.objHndCaller, "Bardic Ability Duration Bonus");
                            target.AddCondition("Greatness", bonusRounds + 5, 0, 0, 0);
                        }

                        return;
                    case BardicMusicSongType.BM_INSPIRE_COURAGE:
                        evt.objHndCaller.AddConditionToPartyAround(30.0f, StatusEffects.InspiredCourage, null);
                        return;
                    case BardicMusicSongType.BM_COUNTER_SONG:
                        evt.objHndCaller.AddConditionToPartyAround(30.0f, StatusEffects.Countersong, null);
                        return;
                    case BardicMusicSongType.BM_FASCINATE:
                        target?.AddCondition("Fascinate", -1, 0);
                        return;
                    case BardicMusicSongType.BM_INSPIRE_COMPETENCE:
                        target?.AddCondition("Competence", 0, 0);
                        return;
                    case BardicMusicSongType.BM_SUGGESTION:
                        //args.SetConditionArg2(0);
                        return;
                    case BardicMusicSongType.BM_SONG_OF_FREEDOM: break; // TODO
                    case BardicMusicSongType.BM_INSPIRE_HEROICS:
                        if (target != null)
                        {
                            var bonusRounds =
                                GameSystems.D20.D20QueryPython(evt.objHndCaller, "Bardic Ability Duration Bonus");
                            target.AddCondition("Inspired Heroics", bonusRounds + 5, 0, 0, 0);
                        }

                        break;
                }
            }
            else
            {
                evt.SetConditionArg3(0);
                evt.SetConditionArg2(0);
                var partSys = evt.GetConditionPartSysArg(5);
                if (partSys != null)
                {
                    GameSystems.ParticleSys.End(partSys);
                }

                var target = evt.GetConditionObjArg(3);
                if (target != null)
                {
                    GameSystems.D20.D20SendSignal(target, D20DispatcherKey.SIG_Bardic_Music_Completed);
                }
            }
        }

        private static bool CanSingAgain(BardicMusicSongType bmType)
        {
            return bmType == BardicMusicSongType.BM_SUGGESTION
                   || bmType == BardicMusicSongType.BM_SONG_OF_FREEDOM;
        }

        [TempleDllLocation(0x100fe110)]
        private static bool IsPerformanceSkillSufficient(BardicMusicSongType bmType, int perfSkill)
        {
            switch (bmType)
            {
                case BardicMusicSongType.BM_INSPIRE_COURAGE:
                case BardicMusicSongType.BM_COUNTER_SONG:
                case BardicMusicSongType.BM_FASCINATE:
                    return perfSkill >= 3;
                case BardicMusicSongType.BM_INSPIRE_COMPETENCE:
                    return perfSkill >= 6;
                case BardicMusicSongType.BM_SUGGESTION:
                    return perfSkill >= 9;
                case BardicMusicSongType.BM_INSPIRE_GREATNESS:
                    return perfSkill >= 12;
                case BardicMusicSongType.BM_SONG_OF_FREEDOM:
                    return perfSkill >= 15;
                case BardicMusicSongType.BM_INSPIRE_HEROICS:
                    return perfSkill >= 18;
                default:
                    return false;
            }
        }

        public static void GetBardicMusicType(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg2();
        }

        public static void BardicMusicEnd(in DispatcherCallbackArgs evt)
        {
            evt.EndPartSysInArg(5);
            evt.SetConditionArg2(0); // music type currently playing
            evt.SetConditionArg3(0); // rounds music played
            evt.SetConditionArg4(0); // target handle
            evt.SetConditionArg(4, 0); // target handle upper
        }

        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100fe180)]
        public static void BardicMusicInitCallback(in DispatcherCallbackArgs evt, int data)
        {
            var bardLevels = evt.objHndCaller.DispatchGetLevel((int) Stat.level_bard, BonusList.Create(), null);
            if (bardLevels < 1)
            {
                bardLevels = 1;
            }

            evt.SetConditionArg1(bardLevels);
            if (evt.GetConditionArg2() != 0)
            {
                var partSys = evt.GetConditionPartSysArg(5);
                GameSystems.ParticleSys.Remove(partSys);
            }

            evt.SetConditionArg2(0); // Song being sung
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fe220)]
        [TemplePlusLocation("condition.cpp:485")]
        public static void BardicMusicRadial(in DispatcherCallbackArgs evt)
        {
            var perfSkill = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform);
            var bardLvl = evt.objHndCaller.GetStat(Stat.level_bard);
            if (bardLvl == 0 || perfSkill < 3)
            {
                return;
            }

            //Ask python for the maximum number of uses of bardic music
            int nMaxBardicMusic = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Max Bardic Music");

            var bmusic = RadialMenuEntry.CreateParent(5039);
            bmusic.HasMinArg = true;
            bmusic.HasMaxArg = true;
            bmusic.minArg = evt.GetConditionArg1();
            bmusic.maxArg = nMaxBardicMusic;
            var bmusicId =
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref bmusic,
                    RadialMenuStandardNode.Class);

            var insCourage = RadialMenuEntry.CreateAction(5040, D20ActionType.BARDIC_MUSIC,
                (int) BardicMusicSongType.BM_INSPIRE_COURAGE, "TAG_CLASS_FEATURES_BARD_INSPIRE_COURAGE");
            insCourage.AddAsChild(evt.objHndCaller, bmusicId);

            var counterSong = RadialMenuEntry.CreateAction(5043, D20ActionType.BARDIC_MUSIC,
                (int) BardicMusicSongType.BM_COUNTER_SONG, "TAG_CLASS_FEATURES_BARD_COUNTERSONG");
            counterSong.AddAsChild(evt.objHndCaller, bmusicId);

            {
                // putting this in brackets to prevent copy paste error, grrr
                var fasci = RadialMenuEntry.CreateAction(5042, D20ActionType.BARDIC_MUSIC,
                    (int) BardicMusicSongType.BM_FASCINATE, "TAG_CLASS_FEATURES_BARD_FASCINATE");
                fasci.d20SpellData = new D20SpellData(3003, SpellSystem.GetSpellClass(Stat.level_bard),
                    1); // Spell 3003 - Bardic Fascinate
                fasci.AddAsChild(evt.objHndCaller, bmusicId);
            }

            if (bardLvl >= 3 && perfSkill >= 6)
            {
                var insCompetence = RadialMenuEntry.CreateAction(5041, D20ActionType.BARDIC_MUSIC,
                    (int) BardicMusicSongType.BM_INSPIRE_COMPETENCE, "TAG_CLASS_FEATURES_BARD_INSPIRE_COMPETENCE");
                insCompetence.d20SpellData = new D20SpellData(3004, SpellSystem.GetSpellClass(Stat.level_bard),
                    1); // Spell 3004 - Bardic Inspire Competence
                insCompetence.AddAsChild(evt.objHndCaller, bmusicId);
            }

            if (bardLvl >= 6 && perfSkill >= 9)
            {
                var bardSugg = RadialMenuEntry.CreateAction(5121, D20ActionType.BARDIC_MUSIC,
                    (int) BardicMusicSongType.BM_SUGGESTION, "TAG_CLASS_FEATURES_BARD_SUGGESTION");
                if (bardLvl >= 18 && perfSkill >= 21)
                    bardSugg.d20SpellData = new D20SpellData(3006, SpellSystem.GetSpellClass(Stat.level_bard),
                        1); // Spell 3006 - Bard Suggestion Mass
                else
                    bardSugg.d20SpellData = new D20SpellData(3000, SpellSystem.GetSpellClass(Stat.level_bard),
                        1); // Spell 3000 - Bard Suggestion
                bardSugg.AddAsChild(evt.objHndCaller, bmusicId);
            }

            if (bardLvl >= 9 && perfSkill >= 12)
            {
                var insGreatness = RadialMenuEntry.CreateAction(5044, D20ActionType.BARDIC_MUSIC,
                    (int) BardicMusicSongType.BM_INSPIRE_GREATNESS, "TAG_CLASS_FEATURES_BARD_INSPIRE_GREATNESS");
                insGreatness.d20SpellData = new D20SpellData(3002, SpellSystem.GetSpellClass(Stat.level_bard),
                    1); // Spell 3002 - Bardic Inspire Greatness
                insGreatness.AddAsChild(evt.objHndCaller, bmusicId);
            }


            if (bardLvl >= 12 && perfSkill >= 15)
            {
                var songOfFreedom = RadialMenuEntry.CreateAction(5119, D20ActionType.BARDIC_MUSIC,
                    (int) BardicMusicSongType.BM_SONG_OF_FREEDOM, "TAG_CLASS_FEATURES_BARD_SONG_OF_FREEDOM");
                songOfFreedom.d20SpellData = new D20SpellData(3007, SpellSystem.GetSpellClass(Stat.level_bard),
                    1); // Spell 3007 - Bardic Song of Freedom
                songOfFreedom.AddAsChild(evt.objHndCaller, bmusicId);
            }

            if (bardLvl >= 15 && perfSkill >= 18)
            {
                var insHeroics = RadialMenuEntry.CreateAction(5120, D20ActionType.BARDIC_MUSIC,
                    (int) BardicMusicSongType.BM_INSPIRE_HEROICS, "TAG_CLASS_FEATURES_BARD_INSPIRE_HEROICS");
                insHeroics.d20SpellData = new D20SpellData(3005, SpellSystem.GetSpellClass(Stat.level_bard),
                    1); // Spell 3005 - Bardic Inspire Heroics
                insHeroics.AddAsChild(evt.objHndCaller, bmusicId);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100fe9b0)]
        public static void BardicMusicOnSequence(in DispatcherCallbackArgs evt)
        {
            var currentlySinging = (BardicMusicSongType) evt.GetConditionArg2();
            if (currentlySinging == 0)
            {
                return;
            }

            var dispIo = evt.GetDispIoD20Signal();
            var actSeq = (ActionSequence) dispIo.obj;

            var interruptMusic = false;
            if (currentlySinging == BardicMusicSongType.BM_FASCINATE)
            {
                // Search for an action that might interrupt the bardic song
                foreach (var action in actSeq.d20ActArray)
                {
                    switch (action.d20ActType)
                    {
                        default:
                            interruptMusic = true;
                            break;
                        // Allowed actions
                        case D20ActionType.UNSPECIFIED_MOVE:
                        case D20ActionType.FIVEFOOTSTEP:
                        case D20ActionType.MOVE:
                        case D20ActionType.DOUBLE_MOVE:
                        case D20ActionType.RUN:
                        case D20ActionType.ATTACK_OF_OPPORTUNITY:
                        case D20ActionType.BARDIC_MUSIC:
                            break;
                    }
                }
            }
            else
            {
                foreach (var action in actSeq.d20ActArray)
                {
                    if (action.d20ActType == D20ActionType.CAST_SPELL)
                    {
                        interruptMusic = true;
                        break;
                    }
                }
            }

            if (interruptMusic)
            {
                evt.SetConditionArg2(0);

                var partSys = evt.GetConditionPartSysArg(5);
                GameSystems.ParticleSys.Remove(partSys);

                var target = evt.GetConditionObjArg(3);
                if (target != null)
                {
                    GameSystems.D20.D20SendSignal(target, D20DispatcherKey.SIG_Bardic_Music_Completed);
                }

                BardicMusicPlaySound(currentlySinging, evt.objHndCaller, 1);
            }
        }

        private static readonly Dictionary<BardicMusicSongType, int> BardicSongBaseSoundId =
            new Dictionary<BardicMusicSongType, int>
            {
                {BardicMusicSongType.BM_INSPIRE_COURAGE, 20040},
                {BardicMusicSongType.BM_COUNTER_SONG, 20000},
                {BardicMusicSongType.BM_FASCINATE, 20020},
                {BardicMusicSongType.BM_INSPIRE_COMPETENCE, 20060},
                {BardicMusicSongType.BM_SUGGESTION, 20080},
                {BardicMusicSongType.BM_INSPIRE_GREATNESS, 20060}
            };

        [TempleDllLocation(0x100fe4f0)]
        private static void BardicMusicPlaySound(BardicMusicSongType bardicSongIdx, GameObjectBody performer,
            int evtType)
        {
            if (!BardicSongBaseSoundId.TryGetValue(bardicSongIdx, out var baseSoundId))
            {
                return;
            }

            var instrType = GameSystems.D20.D20QueryInt(performer, D20DispatcherKey.QUE_BardicInstrument);

            var soundId = baseSoundId + instrType + evtType;

            GameSystems.SoundGame.PositionalSound(soundId, 1, performer);
        }

        private static void QueryMaxBardicMusic(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Total uses = bard level
            dispIo.return_val += evt.objHndCaller.GetStat(Stat.level_bard);
        }
    }
}