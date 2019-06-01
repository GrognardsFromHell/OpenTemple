using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TabFiles;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Feats
{
    public class FeatSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private Dictionary<int, string> featMes;
        private Dictionary<int, string> featEnumsMes;
        private Dictionary<int, string> featMesNew;

        private string[] englishFeatNames = new string[(int) FeatId.NONE];

        [TempleDllLocation(0x1007bfa0)]
        public FeatSystem()
        {
            featMes = Tig.FS.ReadMesFile("mes/feat.mes");
            featEnumsMes = Tig.FS.ReadMesFile("rules/feat_enum.mes");
            if (Tig.FS.FileExists("tpmes/feat.mes"))
            {
                featMesNew = Tig.FS.ReadMesFile("tpmes/feat.mes");
            }
            else
            {
                featMesNew = new Dictionary<int, string>();
            }

            // feat.tab is pretty much only used for english feat names used in protos.tab
            TabFile.ParseFile("rules/feat.tab", record =>
            {
                var featId = (FeatId) record[0].GetInt();
                var featName = record[1].AsString();
                englishFeatNames[(int) featId] = featName;
            });

            /*
             TODO
            MesLine mesLine;
                mesLine.key = 0;
                do
                {
                    var lineFetched = mesFuncs.GetLine(*feats.featMes, &mesLine);
                    feats.featNames[mesLine.key] = (CHAR *)(lineFetched != 0 ? mesLine.value : 0);
                    if (!lineFetched)
                    {
                        lineFetched = mesFuncs.GetLine(feats.featMesNew, &mesLine);
                        feats.featNames[mesLine.key] = (CHAR *)(lineFetched != 0 ? mesLine.value : 0);
                    }
                    mesLine.key++;
                } while (mesLine.key < NUM_FEATS);

                tabSys.tabFileStatusInit(&featPropertiesTabFile, featPropertiesTabLineParser);
                if (tabSys.tabFileStatusBasicFormatter(&featPropertiesTabFile, "tprules//feat_properties.tab"))
                {
                    tabSys.tabFileStatusDealloc(&featPropertiesTabFile);
                }
                else
                {
                    tabSys.tabFileParseLines(&featPropertiesTabFile);
                }

                // New file-based Feats
                _GetNewFeatsFromFile();

                _CompileParents();
*/
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x1007C3D0)]
        public bool HasFeat(GameObjectBody critter, FeatId featId)
        {
            return HasFeatCountByClass(critter, featId) > 0;
        }

        /// <summary>
        /// Counts feats actively taken by the given critter which were
        /// not implied by race or other factors.
        /// </summary>
        public int HasFeatCount(GameObjectBody obj, FeatId featId)
        {
            int featCount = 0;
            var feats = obj.GetInt32Array(obj_f.critter_feat_idx);
            for (var i = 0; i < feats.Count; i++)
            {
                if (feats[i] == (int) featId)
                {
                    featCount++;
                }
            }

            return featCount;
        }

        public int HasFeatCountByClass(GameObjectBody obj, FeatId featEnum, Stat? classLevelBeingRaised = null,
            FeatId? rangerSpecializationFeat = null,
            DomainId newDomain1 = DomainId.None,
            DomainId newDomain2 = DomainId.None,
            int alignmentChoiceNew = 0)
        {
            if (!IsFeatEnabled(featEnum))
                return 0;

            // Race feats
            var objRace = GameSystems.Critter.GetRace(obj, false);
            if (D20RaceSystem.HasFeat(objRace, featEnum))
            {
                return 1;
            }

            // Class automatic feats
            foreach (var classEnum in D20ClassSystem.AllClasses)
            {
                var classLvl = GameSystems.Stat.StatLevelGet(obj, classEnum);
                if (classLevelBeingRaised == classEnum)
                {
                    classLvl++;
                }

                if (classLvl > 0 && D20ClassSystem.HasFeat(featEnum, classEnum, classLvl))
                {
                    return 1;
                }
            }

            foreach (var classId in D20ClassSystem.VanillaClasses)
            {
                if (classId == Stat.level_monk)
                    continue; // so it doesn't add the improved trip feat

                var classLevel = GameSystems.Stat.StatLevelGet(obj, classId);
                if (classLevelBeingRaised == classId)
                {
                    classLevel++;
                }

                if (VanillaClassFeats.HasFeatImplicitly(featEnum, classId, classLevel))
                {
                    return 1;
                }
            }

            var nBarbarianLevel = GameSystems.Stat.StatLevelGet(obj, Stat.level_barbarian);
            var nRogueLevel = GameSystems.Stat.StatLevelGet(obj, Stat.level_rogue);

            if (classLevelBeingRaised == Stat.level_barbarian)
            {
                nBarbarianLevel++;
            }

            if (classLevelBeingRaised == Stat.level_rogue)
            {
                nRogueLevel++;
            }

            // special casing for uncanny dodge for Brb 2 / Rog 4 combo
            if (featEnum == FeatId.IMPROVED_UNCANNY_DODGE)
            {
                if (nBarbarianLevel >= 2 && nRogueLevel >= 4)
                {
                    return 1;
                }
            }

            // ranger styles
            var rangerLvl = GameSystems.Stat.StatLevelGet(obj, Stat.level_ranger);
            if (classLevelBeingRaised == Stat.level_ranger)
            {
                rangerLvl++;
            }

            if (rangerSpecializationFeat.HasValue)
            {
                rangerLvl++;
            }

            if (rangerLvl >= 2)
            {
                if (HasFeatCount(obj, FeatId.RANGER_ARCHERY_STYLE) > 0 ||
                    rangerSpecializationFeat == FeatId.RANGER_ARCHERY_STYLE)
                {
                    if (VanillaClassFeats.HasRangerArcheryStyleFeatImplicitly(featEnum, rangerLvl))
                    {
                        return 1;
                    }
                }
                else if (HasFeatCount(obj, FeatId.RANGER_TWO_WEAPON_STYLE) > 0 ||
                         rangerSpecializationFeat == FeatId.RANGER_TWO_WEAPON_STYLE)
                {
                    if (VanillaClassFeats.HasRangerTwoWeaponStyleFeatImplicitly(featEnum, rangerLvl))
                    {
                        return 1;
                    }
                }
            }

            // war domain
            var objDeity = (DeityId) obj.GetInt32(obj_f.critter_deity);
            var domain1 = (DomainId) obj.GetInt32(obj_f.critter_domain_1);
            var domain2 = (DomainId) obj.GetInt32(obj_f.critter_domain_2);
            if (domain1 == DomainId.None && newDomain1 != DomainId.None)
            {
                domain1 = newDomain1;
            }

            if (domain2 == 0)
            {
                domain2 = newDomain2;
            }

            if (domain1 == DomainId.War || domain2 == DomainId.War)
            {
                switch (objDeity)
                {
                    case DeityId.CORELLON_LARETHIAN:
                        if (featEnum == FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGSWORD ||
                            featEnum == FeatId.WEAPON_FOCUS_LONGSWORD)
                        {
                            return 1;
                        }

                        break;
                    case DeityId.ERYTHNUL:
                        if (featEnum == FeatId.WEAPON_FOCUS_MORNINGSTAR)
                        {
                            return 1;
                        }

                        break;
                    case DeityId.GRUUMSH:
                        if (featEnum == FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGSPEAR ||
                            featEnum == FeatId.WEAPON_FOCUS_LONGSPEAR)
                        {
                            return 1;
                        }

                        break;
                    case DeityId.HEIRONEOUS:
                        if (featEnum == FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGSWORD ||
                            featEnum == FeatId.WEAPON_FOCUS_LONGSWORD)
                        {
                            return 1;
                        }

                        break;
                    case DeityId.HEXTOR:
                        if (featEnum == FeatId.MARTIAL_WEAPON_PROFICIENCY_HEAVY_FLAIL ||
                            featEnum == FeatId.WEAPON_FOCUS_HEAVY_FLAIL)
                        {
                            return 1;
                        }

                        break;
                }
            }

            var currentAlignmentChoice = obj.GetInt32(obj_f.critter_alignment_choice);
            if (currentAlignmentChoice == 0)
            {
                currentAlignmentChoice = alignmentChoiceNew;
            }

            var nPaladinLevel = GameSystems.Stat.StatLevelGet(obj, Stat.level_paladin);
            var nClericLevel = GameSystems.Stat.StatLevelGet(obj, Stat.level_cleric);
            if (classLevelBeingRaised == Stat.level_paladin)
            {
                nPaladinLevel++;
            }
            else if (classLevelBeingRaised == Stat.level_cleric)
            {
                nClericLevel++;
            }

            // simple weapon prof
            if (featEnum == FeatId.SIMPLE_WEAPON_PROFICIENCY)
            {
                if (obj.type == ObjectType.npc)
                {
                    var monCat = GameSystems.Critter.GetCategory(obj);
                    if (monCat == MonsterCategory.outsider || monCat == MonsterCategory.monstrous_humanoid
                                                           || monCat == MonsterCategory.humanoid ||
                                                           monCat == MonsterCategory.giant ||
                                                           monCat == MonsterCategory.fey)
                    {
                        return 1;
                    }
                }
            }
            else if (featEnum == FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL)
            {
                if (GameSystems.Critter.IsCategoryType(obj, MonsterCategory.outsider)
                    && GameSystems.Stat.StatLevelGet(obj, Stat.strength) >= 6)
                {
                    return 1;
                }
            }
            else if (featEnum == FeatId.TURN_UNDEAD && (nClericLevel >= 1 || nPaladinLevel >= 4) &&
                     currentAlignmentChoice == 1)
            {
                return 1;
            }
            else if (featEnum == FeatId.REBUKE_UNDEAD && nClericLevel >= 1 && currentAlignmentChoice == 2)
            {
                return 1;
            }

            var clrLvl = GameSystems.Stat.StatLevelGet(obj, Stat.level_cleric);
            var palLvl = GameSystems.Stat.StatLevelGet(obj, Stat.level_paladin);
            var align = obj.GetInt32(obj_f.critter_alignment_choice);

            return HasFeatCount(obj, featEnum);
        }

        // vanilla was 649 (and Moebius hack increased this to 664 I think)
        public const int NUM_FEATS = 750;

        private readonly Dictionary<FeatId, NewFeatSpec> mNewFeats = new Dictionary<FeatId, NewFeatSpec>();

        private bool IsFeatEnabled(FeatId featId)
        {
            if ((int) featId > NUM_FEATS)
            {
                if (!mNewFeats.TryGetValue(featId, out var newFeat))
                {
                    return false;
                }

                return !newFeat.flags.HasFlag(FeatPropertyFlag.DISABLED);
            }

            // TODO return (m_featPropertiesTable[feat] & FPF_DISABLED) == 0;
            return true;
        }

        public string GetFeatName(FeatId featId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the feat name found in feats.tab
        /// </summary>
        [TempleDllLocation(0x118CD640)]
        public string GetEnglishFeatName(FeatId featId)
        {
            return englishFeatNames[(int) featId];
        }

        [TempleDllLocation(0x1007cf30)]
        public void AddFeat(GameObjectBody obj, FeatId featId)
        {
            if (!ObjCheckFeatPrerequisites(obj, featId, 0, 0, 0, 0))
            {
                Logger.Warn("{0} does not meet the prerequisites for feat {1}. Adding anyway.",
                    obj, featId);
            }

            var array = obj.GetInt32Array(obj_f.critter_feat_idx);
            obj.SetInt32(obj_f.critter_feat_idx, array.Count, (int) featId);
        }

        [TempleDllLocation(0x1007c8f0)]
        private bool ObjCheckFeatPrerequisites(GameObjectBody obj, FeatId featId, int i, int i1, int i2, int i3)
        {
            // TODO
            return true;
        }

        public bool IsProficientWithWeapon(GameObjectBody critter, GameObjectBody weapon)
        {
            return IsProficientWithWeaponType(critter, weapon.GetWeaponType());
        }

        [TempleDllLocation(0x1007c8d0)]
        public bool IsProficientWithWeaponType(GameObjectBody critter, WeaponType weaponType)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x1007c410)]
        public bool IsProficientWithArmor(GameObjectBody critter, GameObjectBody armor)
        {
            switch (armor.GetArmorFlags() & ArmorFlag.TYPE_BITMASK)
            {
                case ArmorFlag.TYPE_LIGHT:
                    if (HasFeatCountByClass(critter, FeatId.ARMOR_PROFICIENCY_LIGHT) != 0)
                        return true;
                    return HasFeatCountByClass(critter, FeatId.ARMOR_PROFICIENCY_DRUID) != 0;
                case ArmorFlag.TYPE_MEDIUM:
                    if (HasFeatCountByClass(critter, FeatId.ARMOR_PROFICIENCY_MEDIUM) != 0)
                        return true;
                    return HasFeatCountByClass(critter, FeatId.ARMOR_PROFICIENCY_DRUID) != 0;
                case ArmorFlag.TYPE_HEAVY:
                    return HasFeatCountByClass(critter, FeatId.ARMOR_PROFICIENCY_HEAVY) != 0;
                case ArmorFlag.TYPE_SHIELD:
                    return HasFeatCountByClass(critter, FeatId.SHIELD_PROFICIENCY) != 0;
                default:
                    return false;
            }
        }
    }
}