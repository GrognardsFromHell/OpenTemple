using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui;

namespace ScriptConversion
{
    internal static class PythonConstants
    {
        public static readonly Dictionary<string, string> Constants = new Dictionary<string, string>
        {
            {"OBJ_HANDLE_NULL", "null"},
            {"anim_obj", "AnimatedObject"},
            {"SKIP_DEFAULT", "SkipDefault"},
            {"RUN_DEFAULT", "RunDefault"},
        };

        public static readonly Dictionary<string, GuessedType> ConstantTypes = new Dictionary<string, GuessedType>
        {
            {"OBJ_HANDLE_NULL", GuessedType.Object},
            {"anim_obj", GuessedType.Object},
            {"SKIP_DEFAULT", GuessedType.Bool},
            {"RUN_DEFAULT", GuessedType.Bool},
        };

        static PythonConstants()
        {
            // These are spell descriptors from the D20 SRD
            AddSpellDescriptors();
            // Cleric Domains (matches with the order of the D20 SRD)
            AddClericDomains();
            AddGenders();
            AddAlignment();
            // Spell Schools (matches D20 SRD)
            AddSpellSchools();
            AddObjectTypes();
            AddFrogAnimCallbacks();
            AddResurrectionTypes();
            AddD20ActionTypes();
            AddD20CAF();
            AddD20DAP();
            AddD20DamageType();
            AddSavingThrowFlags();
            AddSavingThrowTypes();
            AddSaveReduction();
            AddDeities();
            AddFadeResults();
            AddCritterFlags();
            AddContainerFlags();
            AddObjectFlags();
            AddItemFlags();
            AddObjectListCategories();
            AddNpcFlags();
            AddWeaponFlags();

            // Armor type was unused
            // Weapon type was unused

            AddPortalFlags();
            AddQueries();
            AddRadialMenuParam();
            AddSleepStatus();
            AddTargetListOrder();
            AddStandpoints();
            AddSizeCategory();
            AddSignals();
            AddTutorialTopics();
            AddTerrainType();
            AddFeats();
            AddFollowerLootTypes();
            AddEquipSlots();
            AddMaterials();
            AddMonsterSubtypes();
            AddMonsterTypes();
            AddObjectFields();
            AddQuestStates();
            AddRaces();
            AddObjScriptEvents();
            AddSkills();
            AddSpellConditions();
            AddSpells();
            AddStats();
            AddRandomEncounterSetupFlags();
            AddTextFloaterColor();
        }

        private static void AddTextFloaterColor()
        {
            Add("tf_white", TextFloaterColor.White);
            Add("tf_red", TextFloaterColor.Red);
            Add("tf_green", TextFloaterColor.Green);
            Add("tf_blue", TextFloaterColor.Blue);
            Add("tf_yellow", TextFloaterColor.Yellow);
            Add("tf_light_blue", TextFloaterColor.LightBlue);
        }

        private static void AddStats()
        {
            var statEnum = Tig.FS.ReadMesFile("rules/stat_enum.mes");
            foreach (var stat in (Stat[]) Enum.GetValues(typeof(Stat)))
            {
                if (statEnum.ContainsKey((int) stat))
                {
                    var statName = "Stat." + Enum.GetName(typeof(Stat), stat);
                    var enumName = statEnum[(int) stat];
                    Constants[enumName] = statName;
                    ConstantTypes[enumName] = GuessedType.Stat;
                }
            }
        }

        private static void AddSpells()
        {
            var wellKnownSpells = typeof(WellKnownSpells).GetFields()
                .Where(f => f.IsStatic)
                .Where(f => f.FieldType == typeof(int))
                .ToImmutableDictionary(
                    f => (int) f.GetValue(null),
                    f => "WellKnownSpells." + f.Name
                );

            var spellEnumMes = Tig.FS.ReadMesFile("rules/spell_enum.mes");
            foreach (var (spellEnum, spellEnumName) in spellEnumMes)
            {
                if (wellKnownSpells.TryGetValue(spellEnum, out var wellKnownSpell))
                {
                    Constants[spellEnumName] = wellKnownSpell;
                    ConstantTypes[spellEnumName] = GuessedType.Integer;
                }
            }
        }

        /// <summary>
        /// Note: While these may look like spells, they are in fact the *conditions* related to that spell.
        /// </summary>
        private static void AddSpellConditions()
        {
            void Add(string name, Expression<Func<ConditionSpec>> condSpecPath)
            {
                var accessor = condSpecPath.Body.ToString();
                Constants[name] = accessor;
                ConstantTypes[name] = GuessedType.Unknown;
            }

            Add("sp_Aid", () => SpellEffects.SpellAid);
            Add("sp_Animal_Friendship", () => SpellEffects.SpellAnimalFriendship);
            Add("sp_Animal_Growth", () => SpellEffects.SpellAnimalGrowth);
            Add("sp_Animal_Trance", () => SpellEffects.SpellAnimalTrance);
            Add("sp_Animate_Dead", () => SpellEffects.SpellAnimateDead);
            Add("sp_Bane", () => SpellEffects.SpellBane);
            Add("sp_Barkskin", () => SpellEffects.SpellBarkskin);
            Add("sp_Bestow_Curse_Ability", () => SpellEffects.SpellBestowCurseAbility);
            Add("sp_Bestow_Curse_Rolls", () => SpellEffects.SpellBestowCurseRolls);
            Add("sp_Bestow_Curse_Actions", () => SpellEffects.SpellBestowCurseActions);
            Add("sp_Bless", () => SpellEffects.SpellBless);
            Add("sp_Blindness", () => SpellEffects.SpellBlindness);
            Add("sp_Blink", () => SpellEffects.SpellBlink);
            Add("sp_Blur", () => SpellEffects.SpellBlur);
            Add("sp_Break_Enchantment", () => SpellEffects.SpellBreakEnchantment);
            Add("sp_Bulls_Strength", () => SpellEffects.SpellBullsStrength);
            Add("sp_Call_Lightning", () => SpellEffects.SpellCallLightning);
            Add("sp_Call_Lightning_Storm", () => SpellEffects.SpellCallLightningStorm);
            Add("sp_Calm_Animals", () => SpellEffects.SpellCalmAnimals);
            Add("sp_Calm_Emotions", () => SpellEffects.SpellCalmEmotions);
            Add("sp_Cats_Grace", () => SpellEffects.SpellCatsGrace);
            Add("sp_Cause_Fear", () => SpellEffects.SpellCauseFear);
            Add("sp_Chaos_Hammer", () => SpellEffects.SpellChaosHammer);
            Add("sp_Charm_Monster", () => SpellEffects.SpellCharmMonster);
            Add("sp_Charm_Person", () => SpellEffects.SpellCharmPerson);
            Add("sp_Charm_Person_or_Animal", () => SpellEffects.SpellCharmPersonorAnimal);
            Add("sp_Chill_Metal", () => SpellEffects.SpellChillMetal);
            Add("sp_Chill_Touch", () => SpellEffects.SpellChillTouch);
            Add("sp_Clairaudience_Clairvoyance", () => SpellEffects.SpellClairaudienceClairvoyance);
            Add("sp_Cloudkill", () => SpellEffects.SpellCloudkill);
            Add("sp_Cloudkill_Damage", () => SpellEffects.SpellCloudkillDamage);
            Add("sp_Color_Spray_Blind", () => SpellEffects.SpellColorSprayBlind);
            Add("sp_Color_Spray_Stun", () => SpellEffects.SpellColorSprayStun);
            Add("sp_Color_Spray_Unconscious", () => SpellEffects.SpellColorSprayUnconscious);
            Add("sp_Command", () => SpellEffects.SpellCommand);
            Add("sp_Confusion", () => SpellEffects.SpellConfusion);
            Add("sp_Consecrate", () => SpellEffects.SpellConsecrate);
            Add("sp_Consecrate_Hit", () => SpellEffects.SpellConsecrateHit);
            Add("sp_Consecrate_Hit_Undead", () => SpellEffects.SpellConsecrateHitUndead);
            Add("sp_Control_Plants", () => SpellEffects.SpellControlPlants);
            Add("sp_Control_Plants_Tracking", () => SpellEffects.SpellControlPlantsTracking);
            Add("sp_Control_Plants_Charm", () => SpellEffects.SpellControlPlantsCharm);
            Add("sp_Control_Plants_Disentangle", () => SpellEffects.SpellControlPlantsDisentangle);
            Add("sp_Control_Plants_Entangle_Pre", () => SpellEffects.SpellControlPlantsEntanglePre);
            Add("sp_Control_Plants_Entangle", () => SpellEffects.SpellControlPlantsEntangle);
            Add("sp_Darkvision", () => SpellEffects.SpellDarkvision);
            Add("sp_Daze", () => SpellEffects.SpellDaze);
            Add("sp_Death_Ward", () => SpellEffects.SpellDeathWard);
            Add("sp_Death_Knell", () => SpellEffects.SpellDeathKnell);
            Add("sp_Deafness", () => SpellEffects.SpellDeafness);
            Add("sp_Delay_Poison", () => SpellEffects.SpellDelayPoison);
            Add("sp_Desecrate", () => SpellEffects.SpellDesecrate);
            Add("sp_Desecrate_Hit", () => SpellEffects.SpellDesecrateHit);
            Add("sp_Desecrate_Hit_Undead", () => SpellEffects.SpellDesecrateHitUndead);
            Add("sp_Detect_Chaos", () => SpellEffects.SpellDetectChaos);
            Add("sp_Detect_Evil", () => SpellEffects.SpellDetectEvil);
            Add("sp_Detect_Good", () => SpellEffects.SpellDetectGood);
            Add("sp_Detect_Law", () => SpellEffects.SpellDetectLaw);
            Add("sp_Detect_Magic", () => SpellEffects.SpellDetectMagic);
            Add("sp_Detect_Secret_Doors", () => SpellEffects.SpellDetectSecretDoors);
            Add("sp_Detect_Undead", () => SpellEffects.SpellDetectUndead);
            Add("sp_Dimensional_Anchor", () => SpellEffects.SpellDimensionalAnchor);
            Add("sp_Discern_Lies", () => SpellEffects.SpellDiscernLies);
            Add("sp_Dispel_Air", () => SpellEffects.SpellDispelAir);
            Add("sp_Dispel_Earth", () => SpellEffects.SpellDispelEarth);
            Add("sp_Dispel_Fire", () => SpellEffects.SpellDispelFire);
            Add("sp_Dispel_Water", () => SpellEffects.SpellDispelWater);
            Add("sp_Dispel_Chaos", () => SpellEffects.SpellDispelChaos);
            Add("sp_Dispel_Evil", () => SpellEffects.SpellDispelEvil);
            Add("sp_Dispel_Good", () => SpellEffects.SpellDispelGood);
            Add("sp_Dispel_Law", () => SpellEffects.SpellDispelLaw);
            Add("sp_Dispel_Magic", () => SpellEffects.SpellDispelMagic);
            Add("sp_Displacement", () => SpellEffects.SpellDisplacement);
            Add("sp_Divine_Favor", () => SpellEffects.SpellDivineFavor);
            Add("sp_Divine_Power", () => SpellEffects.SpellDivinePower);
            Add("sp_Dominate_Animal", () => SpellEffects.SpellDominateAnimal);
            Add("sp_Dominate_Person", () => SpellEffects.SpellDominatePerson);
            Add("sp_Doom", () => SpellEffects.SpellDoom);
            Add("sp_Eagles_Splendor", () => SpellEffects.SpellEaglesSplendor);
            Add("sp_Emotion_Despair", () => SpellEffects.SpellEmotionDespair);
            Add("sp_Emotion_Fear", () => SpellEffects.SpellEmotionFear);
            Add("sp_Emotion_Friendship", () => SpellEffects.SpellEmotionFriendship);
            Add("sp_Emotion_Hate", () => SpellEffects.SpellEmotionHate);
            Add("sp_Emotion_Hope", () => SpellEffects.SpellEmotionHope);
            Add("sp_Emotion_Rage", () => SpellEffects.SpellEmotionRage);
            Add("sp_Endurance", () => SpellEffects.SpellEndurance);
            Add("sp_Endure_Elements", () => SpellEffects.SpellEndureElements);
            Add("sp_Enlarge", () => SpellEffects.SpellEnlarge);
            Add("sp_Entangle", () => SpellEffects.SpellEntangle);
            Add("sp_Entangle_On", () => SpellEffects.SpellEntangleOn);
            Add("sp_Entangle_Off", () => SpellEffects.SpellEntangleOff);
            Add("sp_Entropic_Shield", () => SpellEffects.SpellEntropicShield);
            Add("sp_Expeditious_Retreat", () => SpellEffects.SpellExpeditiousRetreat);
            Add("sp_Faerie_Fire", () => SpellEffects.SpellFaerieFire);
            Add("sp_False_Life", () => SpellEffects.SpellFalseLife);
            Add("sp_Feeblemind", () => SpellEffects.SpellFeeblemind);
            Add("sp_Fear", () => SpellEffects.SpellFear);
            Add("sp_Find_Traps", () => SpellEffects.SpellFindTraps);
            Add("sp_Fire_Shield", () => SpellEffects.SpellFireShield);
            Add("sp_Flare", () => SpellEffects.SpellFlare);
            Add("sp_Fog_Cloud", () => SpellEffects.SpellFogCloud);
            Add("sp_Fog_Cloud_Hit", () => SpellEffects.SpellFogCloudHit);
            Add("sp_Foxs_Cunning", () => SpellEffects.SpellFoxsCunning);
            Add("sp_Freedom_of_Movement", () => SpellEffects.SpellFreedomofMovement);
            Add("sp_Gaseous_Form", () => SpellEffects.SpellGaseousForm);
            Add("sp_Ghoul_Touch", () => SpellEffects.SpellGhoulTouch);
            Add("sp_Ghoul_Touch_Paralyzed", () => SpellEffects.SpellGhoulTouchParalyzed);
            Add("sp_Ghoul_Touch_Stench", () => SpellEffects.SpellGhoulTouchStench);
            Add("sp_Ghoul_Touch_Stench_Hit", () => SpellEffects.SpellGhoulTouchStenchHit);
            Add("sp_Glibness", () => SpellEffects.SpellGlibness);
            Add("sp_Glitterdust_Blindness", () => SpellEffects.SpellGlitterdustBlindness);
            Add("sp_Glitterdust", () => SpellEffects.SpellGlitterdust);
            Add("sp_Goodberry", () => SpellEffects.SpellGoodberry);
            Add("sp_Goodberry_Tally", () => SpellEffects.SpellGoodberryTally);
            Add("sp_Grease", () => SpellEffects.SpellGrease);
            Add("sp_Grease_Hit", () => SpellEffects.SpellGreaseHit);
            Add("sp_Greater_Heroism", () => SpellEffects.SpellGreaterHeroism);
            Add("sp_Greater_Magic_Fang", () => SpellEffects.SpellGreaterMagicFang);
            Add("sp_Greater_Magic_Weapon", () => SpellEffects.SpellGreaterMagicWeapon);
            Add("sp_Guidance", () => SpellEffects.SpellGuidance);
            Add("sp_Gust_of_Wind", () => SpellEffects.SpellGustofWind);
            Add("sp_Haste", () => SpellEffects.SpellHaste);
            Add("sp_Halt_Undead", () => SpellEffects.SpellHaltUndead);
            Add("sp_Harm", () => SpellEffects.SpellHarm);
            Add("sp_Heal", () => SpellEffects.SpellHeal);
            Add("sp_Heat_Metal", () => SpellEffects.SpellHeatMetal);
            Add("sp_Heroism", () => SpellEffects.SpellHeroism);
            Add("sp_Hold_Animal", () => SpellEffects.SpellHoldAnimal);
            Add("sp_Hold_Monster", () => SpellEffects.SpellHoldMonster);
            Add("sp_Hold_Person", () => SpellEffects.SpellHoldPerson);
            Add("sp_Hold_Portal", () => SpellEffects.SpellHoldPortal);
            Add("sp_Holy_Smite", () => SpellEffects.SpellHolySmite);
            Add("sp_Ice_Storm", () => SpellEffects.SpellIceStorm);
            Add("sp_Ice_Storm_Hit", () => SpellEffects.SpellIceStormHit);
            Add("sp_Invisibility", () => SpellEffects.SpellInvisibility);
            Add("sp_Invisibility_Purge", () => SpellEffects.SpellInvisibilityPurge);
            Add("sp_Invisibility_Purge_Hit", () => SpellEffects.SpellInvisibilityPurgeHit);
            Add("sp_Invisibility_Sphere", () => SpellEffects.SpellInvisibilitySphere);
            Add("sp_Invisibility_Sphere_Hit", () => SpellEffects.SpellInvisibilitySphereHit);
            Add("sp_Invisibility_to_Animals", () => SpellEffects.SpellInvisibilitytoAnimals);
            Add("sp_Invisibility_to_Undead", () => SpellEffects.SpellInvisibilitytoUndead);
            Add("sp_Improved_Invisibility", () => SpellEffects.SpellImprovedInvisibility);
            Add("sp_Keen_Edge", () => SpellEffects.SpellKeenEdge);
            Add("sp_Lesser_Restoration", () => SpellEffects.SpellLesserRestoration);
            Add("sp_Longstrider", () => SpellEffects.SpellLongstrider);
            Add("sp_Mage_Armor", () => SpellEffects.SpellMageArmor);
            Add("sp_Magic_Circle_Inward", () => SpellEffects.SpellMagicCircleInward);
            Add("sp_Magic_Circle_Outward", () => SpellEffects.SpellMagicCircleOutward);
            Add("sp_Magic_Fang", () => SpellEffects.SpellMagicFang);
            Add("sp_Magic_Missile", () => SpellEffects.SpellMagicMissile);
            Add("sp_Magic_Stone", () => SpellEffects.SpellMagicStone);
            Add("sp_Magic_Vestment", () => SpellEffects.SpellMagicVestment);
            Add("sp_Magic_Weapon", () => SpellEffects.SpellMagicWeapon);
            Add("sp_Meld_Into_Stone", () => SpellEffects.SpellMeldIntoStone);
            Add("sp_Melfs_Acid_Arrow", () => SpellEffects.SpellMelfsAcidArrow);
            Add("sp_Minor_Globe_of_Invulnerability", () => SpellEffects.SpellMinorGlobeofInvulnerability);
            Add("sp_Minor_Globe_of_Invulnerability_Hit", () => SpellEffects.SpellMinorGlobeofInvulnerabilityHit);
            Add("sp_Mind_Fog", () => SpellEffects.SpellMindFog);
            Add("sp_Mind_Fog_Hit", () => SpellEffects.SpellMindFogHit);
            Add("sp_Mirror_Image", () => SpellEffects.SpellMirrorImage);
            Add("sp_Mordenkainens_Faithful_Hound", () => SpellEffects.SpellMordenkainensFaithfulHound);
            Add("sp_Negative_Energy_Protection", () => SpellEffects.SpellNegativeEnergyProtection);
            Add("sp_Neutralize_Poison", () => SpellEffects.SpellNeutralizePoison);
            Add("sp_Obscuring_Mist", () => SpellEffects.SpellObscuringMist);
            Add("sp_Obscuring_Mist_Hit", () => SpellEffects.SpellObscuringMistHit);
            Add("sp_Orders_Wrath", () => SpellEffects.SpellOrdersWrath);
            Add("sp_Otilukes_Resilient_Sphere", () => SpellEffects.SpellOtilukesResilientSphere);
            Add("sp_Owls_Wisdom", () => SpellEffects.SpellOwlsWisdom);
            Add("sp_Prayer", () => SpellEffects.SpellPrayer);
            Add("sp_Produce_Flame", () => SpellEffects.SpellProduceFlame);
            Add("sp_Protection_From_Arrows", () => SpellEffects.SpellProtectionFromArrows);
            Add("sp_Protection_From_Alignment", () => SpellEffects.SpellProtectionFromAlignment);
            Add("sp_Protection_From_Elements", () => SpellEffects.SpellProtectionFromElements);
            Add("sp_Rage", () => SpellEffects.SpellRage);
            Add("sp_Raise_Dead", () => SpellEffects.SpellRaiseDead);
            Add("sp_Ray_of_Enfeeblement", () => SpellEffects.SpellRayofEnfeeblement);
            Add("sp_Reduce", () => SpellEffects.SpellReduce);
            Add("sp_Reduce_Animal", () => SpellEffects.SpellReduceAnimal);
            Add("sp_Remove_Blindness", () => SpellEffects.SpellRemoveBlindness);
            Add("sp_Remove_Curse", () => SpellEffects.SpellRemoveCurse);
            Add("sp_Remove_Deafness", () => SpellEffects.SpellRemoveDeafness);
            Add("sp_Remove_Disease", () => SpellEffects.SpellRemoveDisease);
            Add("sp_Remove_Fear", () => SpellEffects.SpellRemoveFear);
            Add("sp_Remove_Paralysis", () => SpellEffects.SpellRemoveParalysis);
            Add("sp_Repel_Vermin", () => SpellEffects.SpellRepelVermin);
            Add("sp_Repel_Vermin_Hit", () => SpellEffects.SpellRepelVerminHit);
            Add("sp_Resistance", () => SpellEffects.SpellResistance);
            Add("sp_Resist_Elements", () => SpellEffects.SpellResistElements);
            Add("sp_Restoration", () => SpellEffects.SpellRestoration);
            Add("sp_Resurrection", () => SpellEffects.SpellResurrection);
            Add("sp_Righteous_Might", () => SpellEffects.SpellRighteousMight);
            Add("sp_Sanctuary", () => SpellEffects.SpellSanctuary);
            Add("sp_Sanctuary_Save_Succeeded", () => SpellEffects.SpellSanctuarySaveSucceeded);
            Add("sp_Sanctuary_Save_Failed", () => SpellEffects.SpellSanctuarySaveFailed);
            Add("sp_See_Invisibility", () => SpellEffects.SpellSeeInvisibility);
            Add("sp_Shield", () => SpellEffects.SpellShield);
            Add("sp_Shield_of_Faith", () => SpellEffects.SpellShieldofFaith);
            Add("sp_Shillelagh", () => SpellEffects.SpellShillelagh);
            Add("sp_Shocking_Grasp", () => SpellEffects.SpellShockingGrasp);
            Add("sp_Shout", () => SpellEffects.SpellShout);
            Add("sp_Silence", () => SpellEffects.SpellSilence);
            Add("sp_Silence_Hit", () => SpellEffects.SpellSilenceHit);
            Add("sp_Sleep", () => SpellEffects.SpellSleep);
            Add("sp_Sleet_Storm", () => SpellEffects.SpellSleetStorm);
            Add("sp_Sleet_Storm_Hit", () => SpellEffects.SpellSleetStormHit);
            Add("sp_Slow", () => SpellEffects.SpellSlow);
            Add("sp_Soften_Earth_and_Stone", () => SpellEffects.SpellSoftenEarthandStone);
            Add("sp_Soften_Earth_and_Stone_Hit", () => SpellEffects.SpellSoftenEarthandStoneHit);
            Add("sp_Soften_Earth_and_Stone_Hit_Save_Failed", () => SpellEffects.SpellSoftenEarthandStoneHitSaveFailed);
            Add("sp_Solid_Fog", () => SpellEffects.SpellSolidFog);
            Add("sp_Solid_Fog_Hit", () => SpellEffects.SpellSolidFogHit);
            Add("sp_Sound_Burst", () => SpellEffects.SpellSoundBurst);
            Add("sp_Spell_Resistance", () => SpellEffects.SpellSpellResistance);
            Add("sp_Spike_Growth", () => SpellEffects.SpellSpikeGrowth);
            Add("sp_Spike_Growth_Hit", () => SpellEffects.SpellSpikeGrowthHit);
            Add("sp_Spike_Growth_Damage", () => SpellEffects.SpellSpikeGrowthDamage);
            Add("sp_Spike_Stones", () => SpellEffects.SpellSpikeStones);
            Add("sp_Spike_Stones_Hit", () => SpellEffects.SpellSpikeStonesHit);
            Add("sp_Spike_Stones_Damage", () => SpellEffects.SpellSpikeStonesDamage);
            Add("sp_Spiritual_Weapon", () => SpellEffects.SpellSpiritualWeapon);
            Add("sp_Stinking_Cloud", () => SpellEffects.SpellStinkingCloud);
            Add("sp_Stinking_Cloud_Hit", () => SpellEffects.SpellStinkingCloudHit);
            Add("sp_Stinking_Cloud_Hit_Pre", () => SpellEffects.SpellStinkingCloudHitPre);
            Add("sp_Stoneskin", () => SpellEffects.SpellStoneskin);
            Add("sp_Suggestion", () => SpellEffects.SpellSuggestion);
            Add("sp_Summon_Swarm", () => SpellEffects.SpellSummonSwarm);
            Add("sp_Tashas_Hideous_Laughter", () => SpellEffects.SpellTashasHideousLaughter);
            Add("sp_Tree_Shape", () => SpellEffects.SpellTreeShape);
            Add("sp_True_Seeing", () => SpellEffects.SpellTrueSeeing);
            Add("sp_True_Strike", () => SpellEffects.SpellTrueStrike);
            Add("sp_Unholy_Blight", () => SpellEffects.SpellUnholyBlight);
            Add("sp_Vampiric_Touch", () => SpellEffects.SpellVampiricTouch);
            Add("sp_Virtue", () => SpellEffects.SpellVirtue);
            Add("sp_Web", () => SpellEffects.SpellWeb);
            Add("sp_Web_On", () => SpellEffects.SpellWebOn);
            Add("sp_Web_Off", () => SpellEffects.SpellWebOff);
            Add("sp_Wind_Wall", () => SpellEffects.SpellWindWall);
            Add("sp_Wind_Wall_Hit", () => SpellEffects.SpellWindWallHit);
            Add("sp_Summoned", () => SpellEffects.SpellSummoned);
            Add("sp_Frog_Tongue", () => SpellEffects.SpellFrogTongue);
            Add("sp_Frog_Tongue_Grappled", () => SpellEffects.SpellFrogTongueGrappled);
            Add("sp_Frog_Tongue_Swallowed", () => SpellEffects.SpellFrogTongueSwallowed);
            Add("sp_Frog_Tongue_Swallowing", () => SpellEffects.SpellFrogTongueSwallowing);
            Add("sp_Vrock_Screech", () => SpellEffects.SpellVrockScreech);
            Add("sp_Vrock_Spores", () => SpellEffects.SpellVrockSpores);
            Add("sp_Ring_of_Freedom_of_Movement", () => SpellEffects.SpellRingofFreedomofMovement);
            Add("sp_Potion_of_Enlarge", () => SpellEffects.SpellPotionofEnlarge);
            Add("sp_Potion_of_Haste", () => SpellEffects.SpellPotionofHaste);
            Add("sp_Dust_of_Disappearance", () => SpellEffects.SpellDustofDisappearance);
            Add("sp_Potion_of_charisma", () => SpellEffects.SpellPotionofcharisma);
            Add("sp_Potion_of_glibness", () => SpellEffects.SpellPotionofglibness);
        }

        private static void AddSkills()
        {
            Add("skill_appraise", SkillId.appraise);
            Add("skill_bluff", SkillId.bluff);
            Add("skill_concentration", SkillId.concentration);
            Add("skill_diplomacy", SkillId.diplomacy);
            Add("skill_disable_device", SkillId.disable_device);
            Add("skill_gather_information", SkillId.gather_information);
            Add("skill_heal", SkillId.heal);
            Add("skill_hide", SkillId.hide);
            Add("skill_intimidate", SkillId.intimidate);
            Add("skill_listen", SkillId.listen);
            Add("skill_move_silently", SkillId.move_silently);
            Add("skill_open_lock", SkillId.open_lock);
            Add("skill_pick_pocket", SkillId.pick_pocket);
            Add("skill_search", SkillId.search);
            Add("skill_sense_motive", SkillId.sense_motive);
            Add("skill_spellcraft", SkillId.spellcraft);
            Add("skill_spot", SkillId.spot);
            Add("skill_tumble", SkillId.tumble);
            Add("skill_use_magic_device", SkillId.use_magic_device);
            Add("skill_wilderness_lore", SkillId.wilderness_lore);
            Add("skill_perform", SkillId.perform);
            Add("skill_alchemy", SkillId.alchemy);
            Add("skill_balance", SkillId.balance);
            Add("skill_climb", SkillId.climb);
            Add("skill_craft", SkillId.craft);
            Add("skill_decipher_script", SkillId.decipher_script);
            Add("skill_disguise", SkillId.disguise);
            Add("skill_escape_artist", SkillId.escape_artist);
            Add("skill_forgery", SkillId.forgery);
            Add("skill_handle_animal", SkillId.handle_animal);
            Add("skill_innuendo", SkillId.innuendo);
            Add("skill_intuit_direction", SkillId.intuit_direction);
            Add("skill_jump", SkillId.jump);
            Add("skill_knowledge_arcana", SkillId.knowledge_arcana);
            Add("skill_knowledge_religion", SkillId.knowledge_religion);
            Add("skill_knowledge_nature", SkillId.knowledge_nature);
            Add("skill_knowledge_all", SkillId.knowledge_all);
            Add("skill_profession", SkillId.profession);
            Add("skill_read_lips", SkillId.read_lips);
            Add("skill_ride", SkillId.ride);
            Add("skill_swim", SkillId.swim);
            Add("skill_use_rope", SkillId.use_rope);
        }

        private static void AddObjScriptEvents()
        {
            Add("san_dialog", ObjScriptEvent.Dialog);
            Add("san_first_heartbeat", ObjScriptEvent.FirstHeartbeat);
            Add("san_dying", ObjScriptEvent.Dying);
            Add("san_enter_combat", ObjScriptEvent.EnterCombat);
            Add("san_exit_combat", ObjScriptEvent.ExitCombat);
            Add("san_start_combat", ObjScriptEvent.StartCombat);
            Add("san_end_combat", ObjScriptEvent.EndCombat);
            Add("san_buy_object", ObjScriptEvent.BuyObject);
            Add("san_heartbeat", ObjScriptEvent.Heartbeat);
            Add("san_insert_item", ObjScriptEvent.InsertItem);
            Add("san_will_kos", ObjScriptEvent.WillKos);
        }

        private static void AddRaces()
        {
            Add("race_human", RaceId.human);
            Add("race_deep_dwarf", RaceId.deep_dwarf);
            Add("race_dwarf", RaceId.dwarf);
            Add("race_derro", RaceId.derro);
            Add("race_duergar", RaceId.duergar);
            Add("race_elf", RaceId.elf);
            Add("race_gnome", RaceId.gnome);
            Add("race_mountain_dwarf", RaceId.mountain_dwarf);
            Add("race_halfelf", RaceId.halfelf);
            Add("race_half_elf", RaceId.half_elf);
            Add("race_aquatic_elf", RaceId.aquatic_elf);
            Add("race_halforc", RaceId.half_orc);
            Add("race_half_orc", RaceId.half_orc);
            Add("race_drow", RaceId.drow);
            Add("race_halfling", RaceId.halfling);
            Add("race_gray_elf", RaceId.gray_elf);
            Add("race_wild_elf", RaceId.wild_elf);
            Add("race_wood_elf", RaceId.wood_elf);
            Add("race_svirfneblin", RaceId.svirfneblin);
            Add("race_forest_gnome", RaceId.forest_gnome);
            Add("race_tallfellow", RaceId.tallfellow);
            Add("race_deep_halfling", RaceId.deep_halfling);
            Add("race_hill_giant", RaceId.hill_giant);
            Add("race_troll", RaceId.troll);
        }

        private static void AddQuestStates()
        {
            Add("qs_unknown", QuestState.Unknown);
            Add("qs_mentioned", QuestState.Mentioned);
            Add("qs_accepted", QuestState.Accepted);
            Add("qs_achieved", QuestState.Achieved);
            Add("qs_completed", QuestState.Completed);
            Add("qs_other", QuestState.Other);
            Add("qs_botched", QuestState.Botched);
        }

        private static void AddObjectFields()
        {
            Add("obj_f_location", obj_f.location);
            Add("obj_f_offset_x", obj_f.offset_x);
            Add("obj_f_offset_y", obj_f.offset_y);
            Add("obj_f_blit_flags", obj_f.blit_flags);
            Add("obj_f_blit_color", obj_f.blit_color);
            Add("obj_f_light_flags", obj_f.light_flags);
            Add("obj_f_light_color", obj_f.light_color);
            Add("obj_f_flags", obj_f.flags);
            Add("obj_f_spell_flags", obj_f.spell_flags);
            Add("obj_f_name", obj_f.name);
            Add("obj_f_description", obj_f.description);
            Add("obj_f_size", obj_f.size);
            Add("obj_f_hp_pts", obj_f.hp_pts);
            Add("obj_f_hp_adj", obj_f.hp_adj);
            Add("obj_f_hp_damage", obj_f.hp_damage);
            Add("obj_f_material", obj_f.material);
            Add("obj_f_scripts_idx", obj_f.scripts_idx);
            Add("obj_f_sound_effect", obj_f.sound_effect);
            Add("obj_f_category", obj_f.category);
            Add("obj_f_rotation", obj_f.rotation);
            Add("obj_f_speed_walk", obj_f.speed_walk);
            Add("obj_f_speed_run", obj_f.speed_run);
            Add("obj_f_base_mesh", obj_f.base_mesh);
            Add("obj_f_base_anim", obj_f.base_anim);
            Add("obj_f_radius", obj_f.radius);
            Add("obj_f_3d_render_height", obj_f.render_height);
            Add("obj_f_conditions", obj_f.conditions);
            Add("obj_f_condition_arg0", obj_f.condition_arg0);
            Add("obj_f_permanent_mods", obj_f.permanent_mods);
            Add("obj_f_initiative", obj_f.initiative);
            Add("obj_f_dispatcher", obj_f.dispatcher);
            Add("obj_f_subinitiative", obj_f.subinitiative);
            Add("obj_f_secretdoor_flags", obj_f.secretdoor_flags);
            Add("obj_f_secretdoor_effectname", obj_f.secretdoor_effectname);
            Add("obj_f_secretdoor_dc", obj_f.secretdoor_dc);
            Add("obj_f_pad_i_7", obj_f.pad_i_7);
            Add("obj_f_pad_i_8", obj_f.pad_i_8);
            Add("obj_f_pad_i_9", obj_f.pad_i_9);
            Add("obj_f_pad_i_0", obj_f.pad_i_0);
            Add("obj_f_offset_z", obj_f.offset_z);
            Add("obj_f_rotation_pitch", obj_f.rotation_pitch);
            Add("obj_f_pad_f_3", obj_f.pad_f_3);
            Add("obj_f_pad_f_4", obj_f.pad_f_4);
            Add("obj_f_pad_f_5", obj_f.pad_f_5);
            Add("obj_f_pad_f_6", obj_f.pad_f_6);
            Add("obj_f_pad_f_7", obj_f.pad_f_7);
            Add("obj_f_pad_f_8", obj_f.pad_f_8);
            Add("obj_f_pad_f_9", obj_f.pad_f_9);
            Add("obj_f_pad_f_0", obj_f.pad_f_0);
            Add("obj_f_pad_i64_0", obj_f.pad_i64_0);
            Add("obj_f_pad_i64_1", obj_f.pad_i64_1);
            Add("obj_f_pad_i64_2", obj_f.pad_i64_2);
            Add("obj_f_pad_i64_3", obj_f.pad_i64_3);
            Add("obj_f_pad_i64_4", obj_f.pad_i64_4);
            Add("obj_f_last_hit_by", obj_f.last_hit_by);
            Add("obj_f_pad_obj_1", obj_f.pad_obj_1);
            Add("obj_f_pad_obj_2", obj_f.pad_obj_2);
            Add("obj_f_pad_obj_3", obj_f.pad_obj_3);
            Add("obj_f_pad_obj_4", obj_f.pad_obj_4);
            Add("obj_f_permanent_mod_data", obj_f.permanent_mod_data);
            Add("obj_f_attack_types_idx", obj_f.attack_types_idx);
            Add("obj_f_attack_bonus_idx", obj_f.attack_bonus_idx);
            Add("obj_f_strategy_state", obj_f.strategy_state);
            Add("obj_f_pad_ias_4", obj_f.pad_ias_4);
            Add("obj_f_pad_i64as_0", obj_f.pad_i64as_0);
            Add("obj_f_pad_i64as_1", obj_f.pad_i64as_1);
            Add("obj_f_pad_i64as_2", obj_f.pad_i64as_2);
            Add("obj_f_pad_i64as_3", obj_f.pad_i64as_3);
            Add("obj_f_pad_i64as_4", obj_f.pad_i64as_4);
            Add("obj_f_pad_objas_0", obj_f.pad_objas_0);
            Add("obj_f_pad_objas_1", obj_f.pad_objas_1);
            Add("obj_f_pad_objas_2", obj_f.pad_objas_2);
            Add("obj_f_end", obj_f.end);
            Add("obj_f_portal_begin", obj_f.portal_begin);
            Add("obj_f_portal_flags", obj_f.portal_flags);
            Add("obj_f_portal_lock_dc", obj_f.portal_lock_dc);
            Add("obj_f_portal_key_id", obj_f.portal_key_id);
            Add("obj_f_portal_notify_npc", obj_f.portal_notify_npc);
            Add("obj_f_portal_pad_i_1", obj_f.portal_pad_i_1);
            Add("obj_f_portal_pad_i_2", obj_f.portal_pad_i_2);
            Add("obj_f_portal_pad_i_3", obj_f.portal_pad_i_3);
            Add("obj_f_portal_pad_i_4", obj_f.portal_pad_i_4);
            Add("obj_f_portal_pad_i_5", obj_f.portal_pad_i_5);
            Add("obj_f_portal_pad_obj_1", obj_f.portal_pad_obj_1);
            Add("obj_f_portal_pad_ias_1", obj_f.portal_pad_ias_1);
            Add("obj_f_portal_pad_i64as_1", obj_f.portal_pad_i64as_1);
            Add("obj_f_portal_end", obj_f.portal_end);
            Add("obj_f_container_begin", obj_f.container_begin);
            Add("obj_f_container_flags", obj_f.container_flags);
            Add("obj_f_container_lock_dc", obj_f.container_lock_dc);
            Add("obj_f_container_key_id", obj_f.container_key_id);
            Add("obj_f_container_inventory_num", obj_f.container_inventory_num);
            Add("obj_f_container_inventory_list_idx", obj_f.container_inventory_list_idx);
            Add("obj_f_container_inventory_source", obj_f.container_inventory_source);
            Add("obj_f_container_notify_npc", obj_f.container_notify_npc);
            Add("obj_f_container_pad_i_1", obj_f.container_pad_i_1);
            Add("obj_f_container_pad_i_2", obj_f.container_pad_i_2);
            Add("obj_f_container_pad_i_3", obj_f.container_pad_i_3);
            Add("obj_f_container_pad_i_4", obj_f.container_pad_i_4);
            Add("obj_f_container_pad_i_5", obj_f.container_pad_i_5);
            Add("obj_f_container_pad_obj_1", obj_f.container_pad_obj_1);
            Add("obj_f_container_pad_obj_2", obj_f.container_pad_obj_2);
            Add("obj_f_container_pad_ias_1", obj_f.container_pad_ias_1);
            Add("obj_f_container_pad_i64as_1", obj_f.container_pad_i64as_1);
            Add("obj_f_container_pad_objas_1", obj_f.container_pad_objas_1);
            Add("obj_f_container_end", obj_f.container_end);
            Add("obj_f_scenery_begin", obj_f.scenery_begin);
            Add("obj_f_scenery_flags", obj_f.scenery_flags);
            Add("obj_f_scenery_pad_obj_0", obj_f.scenery_pad_obj_0);
            Add("obj_f_scenery_respawn_delay", obj_f.scenery_respawn_delay);
            Add("obj_f_scenery_pad_i_0", obj_f.scenery_pad_i_0);
            Add("obj_f_scenery_pad_i_1", obj_f.scenery_pad_i_1);
            Add("obj_f_scenery_teleport_to", obj_f.scenery_teleport_to);
            Add("obj_f_scenery_pad_i_4", obj_f.scenery_pad_i_4);
            Add("obj_f_scenery_pad_i_5", obj_f.scenery_pad_i_5);
            Add("obj_f_scenery_pad_obj_1", obj_f.scenery_pad_obj_1);
            Add("obj_f_scenery_pad_ias_1", obj_f.scenery_pad_ias_1);
            Add("obj_f_scenery_pad_i64as_1", obj_f.scenery_pad_i64as_1);
            Add("obj_f_scenery_end", obj_f.scenery_end);
            Add("obj_f_projectile_begin", obj_f.projectile_begin);
            Add("obj_f_projectile_flags_combat", obj_f.projectile_flags_combat);
            Add("obj_f_projectile_flags_combat_damage", obj_f.projectile_flags_combat_damage);
            Add("obj_f_projectile_parent_weapon", obj_f.projectile_parent_weapon);
            Add("obj_f_projectile_parent_ammo", obj_f.projectile_parent_ammo);
            Add("obj_f_projectile_part_sys_id", obj_f.projectile_part_sys_id);
            Add("obj_f_projectile_acceleration_x", obj_f.projectile_acceleration_x);
            Add("obj_f_projectile_acceleration_y", obj_f.projectile_acceleration_y);
            Add("obj_f_projectile_acceleration_z", obj_f.projectile_acceleration_z);
            Add("obj_f_projectile_pad_i_4", obj_f.projectile_pad_i_4);
            Add("obj_f_projectile_pad_obj_1", obj_f.projectile_pad_obj_1);
            Add("obj_f_projectile_pad_obj_2", obj_f.projectile_pad_obj_2);
            Add("obj_f_projectile_pad_obj_3", obj_f.projectile_pad_obj_3);
            Add("obj_f_projectile_pad_ias_1", obj_f.projectile_pad_ias_1);
            Add("obj_f_projectile_pad_i64as_1", obj_f.projectile_pad_i64as_1);
            Add("obj_f_projectile_pad_objas_1", obj_f.projectile_pad_objas_1);
            Add("obj_f_projectile_end", obj_f.projectile_end);
            Add("obj_f_item_begin", obj_f.item_begin);
            Add("obj_f_item_flags", obj_f.item_flags);
            Add("obj_f_item_parent", obj_f.item_parent);
            Add("obj_f_item_weight", obj_f.item_weight);
            Add("obj_f_item_worth", obj_f.item_worth);
            Add("obj_f_item_inv_aid", obj_f.item_inv_aid);
            Add("obj_f_item_inv_location", obj_f.item_inv_location);
            Add("obj_f_item_ground_mesh", obj_f.item_ground_mesh);
            Add("obj_f_item_ground_anim", obj_f.item_ground_anim);
            Add("obj_f_item_description_unknown", obj_f.item_description_unknown);
            Add("obj_f_item_description_effects", obj_f.item_description_effects);
            Add("obj_f_item_spell_idx", obj_f.item_spell_idx);
            Add("obj_f_item_spell_idx_flags", obj_f.item_spell_idx_flags);
            Add("obj_f_item_spell_charges_idx", obj_f.item_spell_charges_idx);
            Add("obj_f_item_ai_action", obj_f.item_ai_action);
            Add("obj_f_item_wear_flags", obj_f.item_wear_flags);
            Add("obj_f_item_material_slot", obj_f.item_material_slot);
            Add("obj_f_item_quantity", obj_f.item_quantity);
            Add("obj_f_item_pad_i_1", obj_f.item_pad_i_1);
            Add("obj_f_item_pad_i_2", obj_f.item_pad_i_2);
            Add("obj_f_item_pad_i_3", obj_f.item_pad_i_3);
            Add("obj_f_item_pad_i_4", obj_f.item_pad_i_4);
            Add("obj_f_item_pad_i_5", obj_f.item_pad_i_5);
            Add("obj_f_item_pad_i_6", obj_f.item_pad_i_6);
            Add("obj_f_item_pad_obj_1", obj_f.item_pad_obj_1);
            Add("obj_f_item_pad_obj_2", obj_f.item_pad_obj_2);
            Add("obj_f_item_pad_obj_3", obj_f.item_pad_obj_3);
            Add("obj_f_item_pad_obj_4", obj_f.item_pad_obj_4);
            Add("obj_f_item_pad_obj_5", obj_f.item_pad_obj_5);
            Add("obj_f_item_pad_wielder_condition_array", obj_f.item_pad_wielder_condition_array);
            Add("obj_f_item_pad_wielder_argument_array", obj_f.item_pad_wielder_argument_array);
            Add("obj_f_item_pad_i64as_1", obj_f.item_pad_i64as_1);
            Add("obj_f_item_pad_i64as_2", obj_f.item_pad_i64as_2);
            Add("obj_f_item_pad_objas_1", obj_f.item_pad_objas_1);
            Add("obj_f_item_pad_objas_2", obj_f.item_pad_objas_2);
            Add("obj_f_item_end", obj_f.item_end);
            Add("obj_f_weapon_begin", obj_f.weapon_begin);
            Add("obj_f_weapon_flags", obj_f.weapon_flags);
            Add("obj_f_weapon_range", obj_f.weapon_range);
            Add("obj_f_weapon_ammo_type", obj_f.weapon_ammo_type);
            Add("obj_f_weapon_ammo_consumption", obj_f.weapon_ammo_consumption);
            Add("obj_f_weapon_missile_aid", obj_f.weapon_missile_aid);
            Add("obj_f_weapon_crit_hit_chart", obj_f.weapon_crit_hit_chart);
            Add("obj_f_weapon_attacktype", obj_f.weapon_attacktype);
            Add("obj_f_weapon_damage_dice", obj_f.weapon_damage_dice);
            Add("obj_f_weapon_animtype", obj_f.weapon_animtype);
            Add("obj_f_weapon_type", obj_f.weapon_type);
            Add("obj_f_weapon_crit_range", obj_f.weapon_crit_range);
            Add("obj_f_weapon_pad_i_1", obj_f.weapon_pad_i_1);
            Add("obj_f_weapon_pad_i_2", obj_f.weapon_pad_i_2);
            Add("obj_f_weapon_pad_obj_1", obj_f.weapon_pad_obj_1);
            Add("obj_f_weapon_pad_obj_2", obj_f.weapon_pad_obj_2);
            Add("obj_f_weapon_pad_obj_3", obj_f.weapon_pad_obj_3);
            Add("obj_f_weapon_pad_obj_4", obj_f.weapon_pad_obj_4);
            Add("obj_f_weapon_pad_obj_5", obj_f.weapon_pad_obj_5);
            Add("obj_f_weapon_pad_ias_1", obj_f.weapon_pad_ias_1);
            Add("obj_f_weapon_pad_i64as_1", obj_f.weapon_pad_i64as_1);
            Add("obj_f_weapon_end", obj_f.weapon_end);
            Add("obj_f_ammo_begin", obj_f.ammo_begin);
            Add("obj_f_ammo_flags", obj_f.ammo_flags);
            Add("obj_f_ammo_quantity", obj_f.ammo_quantity);
            Add("obj_f_ammo_type", obj_f.ammo_type);
            Add("obj_f_ammo_pad_i_1", obj_f.ammo_pad_i_1);
            Add("obj_f_ammo_pad_i_2", obj_f.ammo_pad_i_2);
            Add("obj_f_ammo_pad_obj_1", obj_f.ammo_pad_obj_1);
            Add("obj_f_ammo_pad_ias_1", obj_f.ammo_pad_ias_1);
            Add("obj_f_ammo_pad_i64as_1", obj_f.ammo_pad_i64as_1);
            Add("obj_f_ammo_end", obj_f.ammo_end);
            Add("obj_f_armor_begin", obj_f.armor_begin);
            Add("obj_f_armor_flags", obj_f.armor_flags);
            Add("obj_f_armor_ac_adj", obj_f.armor_ac_adj);
            Add("obj_f_armor_max_dex_bonus", obj_f.armor_max_dex_bonus);
            Add("obj_f_armor_arcane_spell_failure", obj_f.armor_arcane_spell_failure);
            Add("obj_f_armor_armor_check_penalty", obj_f.armor_armor_check_penalty);
            Add("obj_f_armor_pad_i_1", obj_f.armor_pad_i_1);
            Add("obj_f_armor_pad_ias_1", obj_f.armor_pad_ias_1);
            Add("obj_f_armor_pad_i64as_1", obj_f.armor_pad_i64as_1);
            Add("obj_f_armor_end", obj_f.armor_end);
            Add("obj_f_money_begin", obj_f.money_begin);
            Add("obj_f_money_flags", obj_f.money_flags);
            Add("obj_f_money_quantity", obj_f.money_quantity);
            Add("obj_f_money_type", obj_f.money_type);
            Add("obj_f_money_pad_i_1", obj_f.money_pad_i_1);
            Add("obj_f_money_pad_i_2", obj_f.money_pad_i_2);
            Add("obj_f_money_pad_i_3", obj_f.money_pad_i_3);
            Add("obj_f_money_pad_i_4", obj_f.money_pad_i_4);
            Add("obj_f_money_pad_i_5", obj_f.money_pad_i_5);
            Add("obj_f_money_pad_ias_1", obj_f.money_pad_ias_1);
            Add("obj_f_money_pad_i64as_1", obj_f.money_pad_i64as_1);
            Add("obj_f_money_end", obj_f.money_end);
            Add("obj_f_food_begin", obj_f.food_begin);
            Add("obj_f_food_flags", obj_f.food_flags);
            Add("obj_f_food_pad_i_1", obj_f.food_pad_i_1);
            Add("obj_f_food_pad_i_2", obj_f.food_pad_i_2);
            Add("obj_f_food_pad_ias_1", obj_f.food_pad_ias_1);
            Add("obj_f_food_pad_i64as_1", obj_f.food_pad_i64as_1);
            Add("obj_f_food_end", obj_f.food_end);
            Add("obj_f_scroll_begin", obj_f.scroll_begin);
            Add("obj_f_scroll_flags", obj_f.scroll_flags);
            Add("obj_f_scroll_pad_i_1", obj_f.scroll_pad_i_1);
            Add("obj_f_scroll_pad_i_2", obj_f.scroll_pad_i_2);
            Add("obj_f_scroll_pad_ias_1", obj_f.scroll_pad_ias_1);
            Add("obj_f_scroll_pad_i64as_1", obj_f.scroll_pad_i64as_1);
            Add("obj_f_scroll_end", obj_f.scroll_end);
            Add("obj_f_key_begin", obj_f.key_begin);
            Add("obj_f_key_key_id", obj_f.key_key_id);
            Add("obj_f_key_pad_i_1", obj_f.key_pad_i_1);
            Add("obj_f_key_pad_i_2", obj_f.key_pad_i_2);
            Add("obj_f_key_pad_ias_1", obj_f.key_pad_ias_1);
            Add("obj_f_key_pad_i64as_1", obj_f.key_pad_i64as_1);
            Add("obj_f_key_end", obj_f.key_end);
            Add("obj_f_written_begin", obj_f.written_begin);
            Add("obj_f_written_flags", obj_f.written_flags);
            Add("obj_f_written_subtype", obj_f.written_subtype);
            Add("obj_f_written_text_start_line", obj_f.written_text_start_line);
            Add("obj_f_written_text_end_line", obj_f.written_text_end_line);
            Add("obj_f_written_pad_i_1", obj_f.written_pad_i_1);
            Add("obj_f_written_pad_i_2", obj_f.written_pad_i_2);
            Add("obj_f_written_pad_ias_1", obj_f.written_pad_ias_1);
            Add("obj_f_written_pad_i64as_1", obj_f.written_pad_i64as_1);
            Add("obj_f_written_end", obj_f.written_end);
            Add("obj_f_bag_begin", obj_f.bag_begin);
            Add("obj_f_bag_flags", obj_f.bag_flags);
            Add("obj_f_bag_size", obj_f.bag_size);
            Add("obj_f_bag_end", obj_f.bag_end);
            Add("obj_f_generic_begin", obj_f.generic_begin);
            Add("obj_f_generic_flags", obj_f.generic_flags);
            Add("obj_f_generic_usage_bonus", obj_f.generic_usage_bonus);
            Add("obj_f_generic_usage_count_remaining", obj_f.generic_usage_count_remaining);
            Add("obj_f_generic_pad_ias_1", obj_f.generic_pad_ias_1);
            Add("obj_f_generic_pad_i64as_1", obj_f.generic_pad_i64as_1);
            Add("obj_f_generic_end", obj_f.generic_end);
            Add("obj_f_critter_begin", obj_f.critter_begin);
            Add("obj_f_critter_flags", obj_f.critter_flags);
            Add("obj_f_critter_flags2", obj_f.critter_flags2);
            Add("obj_f_critter_abilities_idx", obj_f.critter_abilities_idx);
            Add("obj_f_critter_level_idx", obj_f.critter_level_idx);
            Add("obj_f_critter_race", obj_f.critter_race);
            Add("obj_f_critter_gender", obj_f.critter_gender);
            Add("obj_f_critter_age", obj_f.critter_age);
            Add("obj_f_critter_height", obj_f.critter_height);
            Add("obj_f_critter_weight", obj_f.critter_weight);
            Add("obj_f_critter_experience", obj_f.critter_experience);
            Add("obj_f_critter_pad_i_1", obj_f.critter_pad_i_1);
            Add("obj_f_critter_alignment", obj_f.critter_alignment);
            Add("obj_f_critter_deity", obj_f.critter_deity);
            Add("obj_f_critter_domain_1", obj_f.critter_domain_1);
            Add("obj_f_critter_domain_2", obj_f.critter_domain_2);
            Add("obj_f_critter_alignment_choice", obj_f.critter_alignment_choice);
            Add("obj_f_critter_school_specialization", obj_f.critter_school_specialization);
            Add("obj_f_critter_spells_known_idx", obj_f.critter_spells_known_idx);
            Add("obj_f_critter_spells_memorized_idx", obj_f.critter_spells_memorized_idx);
            Add("obj_f_critter_spells_cast_idx", obj_f.critter_spells_cast_idx);
            Add("obj_f_critter_feat_idx", obj_f.critter_feat_idx);
            Add("obj_f_critter_feat_count_idx", obj_f.critter_feat_count_idx);
            Add("obj_f_critter_fleeing_from", obj_f.critter_fleeing_from);
            Add("obj_f_critter_portrait", obj_f.critter_portrait);
            Add("obj_f_critter_money_idx", obj_f.critter_money_idx);
            Add("obj_f_critter_inventory_num", obj_f.critter_inventory_num);
            Add("obj_f_critter_inventory_list_idx", obj_f.critter_inventory_list_idx);
            Add("obj_f_critter_inventory_source", obj_f.critter_inventory_source);
            Add("obj_f_critter_description_unknown", obj_f.critter_description_unknown);
            Add("obj_f_critter_follower_idx", obj_f.critter_follower_idx);
            Add("obj_f_critter_teleport_dest", obj_f.critter_teleport_dest);
            Add("obj_f_critter_teleport_map", obj_f.critter_teleport_map);
            Add("obj_f_critter_death_time", obj_f.critter_death_time);
            Add("obj_f_critter_skill_idx", obj_f.critter_skill_idx);
            Add("obj_f_critter_reach", obj_f.critter_reach);
            Add("obj_f_critter_subdual_damage", obj_f.critter_subdual_damage);
            Add("obj_f_critter_pad_i_4", obj_f.critter_pad_i_4);
            Add("obj_f_critter_pad_i_5", obj_f.critter_pad_i_5);
            Add("obj_f_critter_sequence", obj_f.critter_sequence);
            Add("obj_f_critter_hair_style", obj_f.critter_hair_style);
            Add("obj_f_critter_strategy", obj_f.critter_strategy);
            Add("obj_f_critter_pad_i_3", obj_f.critter_pad_i_3);
            Add("obj_f_critter_monster_category", obj_f.critter_monster_category);
            Add("obj_f_critter_pad_i64_2", obj_f.critter_pad_i64_2);
            Add("obj_f_critter_pad_i64_3", obj_f.critter_pad_i64_3);
            Add("obj_f_critter_pad_i64_4", obj_f.critter_pad_i64_4);
            Add("obj_f_critter_pad_i64_5", obj_f.critter_pad_i64_5);
            Add("obj_f_critter_damage_idx", obj_f.critter_damage_idx);
            Add("obj_f_critter_attacks_idx", obj_f.critter_attacks_idx);
            Add("obj_f_critter_seen_maplist", obj_f.critter_seen_maplist);
            Add("obj_f_critter_pad_i64as_2", obj_f.critter_pad_i64as_2);
            Add("obj_f_critter_pad_i64as_3", obj_f.critter_pad_i64as_3);
            Add("obj_f_critter_pad_i64as_4", obj_f.critter_pad_i64as_4);
            Add("obj_f_critter_pad_i64as_5", obj_f.critter_pad_i64as_5);
            Add("obj_f_critter_end", obj_f.critter_end);
            Add("obj_f_pc_begin", obj_f.pc_begin);
            Add("obj_f_pc_flags", obj_f.pc_flags);
            Add("obj_f_pc_pad_ias_0", obj_f.pc_pad_ias_0);
            Add("obj_f_pc_pad_i64as_0", obj_f.pc_pad_i64as_0);
            Add("obj_f_pc_player_name", obj_f.pc_player_name);
            Add("obj_f_pc_global_flags", obj_f.pc_global_flags);
            Add("obj_f_pc_global_variables", obj_f.pc_global_variables);
            Add("obj_f_pc_voice_idx", obj_f.pc_voice_idx);
            Add("obj_f_pc_roll_count", obj_f.pc_roll_count);
            Add("obj_f_pc_pad_i_2", obj_f.pc_pad_i_2);
            Add("obj_f_pc_weaponslots_idx", obj_f.pc_weaponslots_idx);
            Add("obj_f_pc_pad_ias_2", obj_f.pc_pad_ias_2);
            Add("obj_f_pc_pad_i64as_1", obj_f.pc_pad_i64as_1);
            Add("obj_f_pc_end", obj_f.pc_end);
            Add("obj_f_npc_begin", obj_f.npc_begin);
            Add("obj_f_npc_flags", obj_f.npc_flags);
            Add("obj_f_npc_leader", obj_f.npc_leader);
            Add("obj_f_npc_ai_data", obj_f.npc_ai_data);
            Add("obj_f_npc_combat_focus", obj_f.npc_combat_focus);
            Add("obj_f_npc_who_hit_me_last", obj_f.npc_who_hit_me_last);
            Add("obj_f_npc_waypoints_idx", obj_f.npc_waypoints_idx);
            Add("obj_f_npc_waypoint_current", obj_f.npc_waypoint_current);
            Add("obj_f_npc_standpoint_day_INTERNAL_DO_NOT_USE", obj_f.npc_standpoint_day_INTERNAL_DO_NOT_USE);
            Add("obj_f_npc_standpoint_night_INTERNAL_DO_NOT_USE", obj_f.npc_standpoint_night_INTERNAL_DO_NOT_USE);
            Add("obj_f_npc_faction", obj_f.npc_faction);
            Add("obj_f_npc_retail_price_multiplier", obj_f.npc_retail_price_multiplier);
            Add("obj_f_npc_substitute_inventory", obj_f.npc_substitute_inventory);
            Add("obj_f_npc_reaction_base", obj_f.npc_reaction_base);
            Add("obj_f_npc_challenge_rating", obj_f.npc_challenge_rating);
            Add("obj_f_npc_reaction_pc_idx", obj_f.npc_reaction_pc_idx);
            Add("obj_f_npc_reaction_level_idx", obj_f.npc_reaction_level_idx);
            Add("obj_f_npc_reaction_time_idx", obj_f.npc_reaction_time_idx);
            Add("obj_f_npc_generator_data", obj_f.npc_generator_data);
            Add("obj_f_npc_ai_list_idx", obj_f.npc_ai_list_idx);
            Add("obj_f_npc_save_reflexes_bonus", obj_f.npc_save_reflexes_bonus);
            Add("obj_f_npc_save_fortitude_bonus", obj_f.npc_save_fortitude_bonus);
            Add("obj_f_npc_save_willpower_bonus", obj_f.npc_save_willpower_bonus);
            Add("obj_f_npc_ac_bonus", obj_f.npc_ac_bonus);
            Add("obj_f_npc_add_mesh", obj_f.npc_add_mesh);
            Add("obj_f_npc_waypoint_anim", obj_f.npc_waypoint_anim);
            Add("obj_f_npc_pad_i_3", obj_f.npc_pad_i_3);
            Add("obj_f_npc_pad_i_4", obj_f.npc_pad_i_4);
            Add("obj_f_npc_pad_i_5", obj_f.npc_pad_i_5);
            Add("obj_f_npc_ai_flags64", obj_f.npc_ai_flags64);
            Add("obj_f_npc_pad_i64_2", obj_f.npc_pad_i64_2);
            Add("obj_f_npc_pad_i64_3", obj_f.npc_pad_i64_3);
            Add("obj_f_npc_pad_i64_4", obj_f.npc_pad_i64_4);
            Add("obj_f_npc_pad_i64_5", obj_f.npc_pad_i64_5);
            Add("obj_f_npc_hitdice_idx", obj_f.npc_hitdice_idx);
            Add("obj_f_npc_ai_list_type_idx", obj_f.npc_ai_list_type_idx);
            Add("obj_f_npc_pad_ias_3", obj_f.npc_pad_ias_3);
            Add("obj_f_npc_pad_ias_4", obj_f.npc_pad_ias_4);
            Add("obj_f_npc_pad_ias_5", obj_f.npc_pad_ias_5);
            Add("obj_f_npc_standpoints", obj_f.npc_standpoints);
            Add("obj_f_npc_pad_i64as_2", obj_f.npc_pad_i64as_2);
            Add("obj_f_npc_pad_i64as_3", obj_f.npc_pad_i64as_3);
            Add("obj_f_npc_pad_i64as_4", obj_f.npc_pad_i64as_4);
            Add("obj_f_npc_pad_i64as_5", obj_f.npc_pad_i64as_5);
            Add("obj_f_npc_end", obj_f.npc_end);
            Add("obj_f_trap_begin", obj_f.trap_begin);
            Add("obj_f_trap_flags", obj_f.trap_flags);
            Add("obj_f_trap_difficulty", obj_f.trap_difficulty);
            Add("obj_f_trap_pad_i_2", obj_f.trap_pad_i_2);
            Add("obj_f_trap_pad_ias_1", obj_f.trap_pad_ias_1);
            Add("obj_f_trap_pad_i64as_1", obj_f.trap_pad_i64as_1);
            Add("obj_f_trap_end", obj_f.trap_end);
            Add("obj_f_total_normal", obj_f.total_normal);
            Add("obj_f_transient_begin", obj_f.transient_begin);
            Add("obj_f_render_color", obj_f.render_color);
            Add("obj_f_render_colors", obj_f.render_colors);
            Add("obj_f_render_palette", obj_f.render_palette);
            Add("obj_f_render_scale", obj_f.render_scale);
            Add("obj_f_render_alpha", obj_f.render_alpha);
            Add("obj_f_render_x", obj_f.render_x);
            Add("obj_f_render_y", obj_f.render_y);
            Add("obj_f_render_width", obj_f.render_width);
            Add("obj_f_render_height", obj_f.render_height);
            Add("obj_f_palette", obj_f.palette);
            Add("obj_f_color", obj_f.color);
            Add("obj_f_colors", obj_f.colors);
            Add("obj_f_render_flags", obj_f.render_flags);
            Add("obj_f_temp_id", obj_f.temp_id);
            Add("obj_f_light_handle", obj_f.light_handle);
            Add("obj_f_overlay_light_handles", obj_f.overlay_light_handles);
            Add("obj_f_internal_flags", obj_f.internal_flags);
            Add("obj_f_find_node", obj_f.find_node);
            Add("obj_f_animation_handle", obj_f.animation_handle);
            Add("obj_f_grapple_state", obj_f.grapple_state);
            Add("obj_f_transient_end", obj_f.transient_end);
            Add("obj_f_type", obj_f.type);
            Add("obj_f_prototype_handle", obj_f.prototype_handle);
        }

        private static void AddMonsterTypes()
        {
            Add("mc_type_aberration", MonsterCategory.aberration);
            Add("mc_type_animal", MonsterCategory.animal);
            Add("mc_type_beast", MonsterCategory.beast);
            Add("mc_type_construct", MonsterCategory.construct);
            Add("mc_type_dragon", MonsterCategory.dragon);
            Add("mc_type_elemental", MonsterCategory.elemental);
            Add("mc_type_fey", MonsterCategory.fey);
            Add("mc_type_giant", MonsterCategory.giant);
            Add("mc_type_humanoid", MonsterCategory.humanoid);
            Add("mc_type_magical_beast", MonsterCategory.magical_beast);
            Add("mc_type_monstrous_humanoid", MonsterCategory.monstrous_humanoid);
            Add("mc_type_ooze", MonsterCategory.ooze);
            Add("mc_type_outsider", MonsterCategory.outsider);
            Add("mc_type_plant", MonsterCategory.plant);
            Add("mc_type_shapechanger", MonsterCategory.shapechanger);
            Add("mc_type_undead", MonsterCategory.undead);
            Add("mc_type_vermin", MonsterCategory.vermin);
        }

        private static void AddMonsterSubtypes()
        {
            Add("mc_subtype_air", MonsterSubtype.air);
            Add("mc_subtype_aquatic", MonsterSubtype.aquatic);
            Add("mc_subtype_extraplanar", MonsterSubtype.extraplanar);
            Add("mc_subtype_cold", MonsterSubtype.cold);
            Add("mc_subtype_chaotic", MonsterSubtype.chaotic);
            Add("mc_subtype_demon", MonsterSubtype.demon);
            Add("mc_subtype_devil", MonsterSubtype.devil);
            Add("mc_subtype_dwarf", MonsterSubtype.dwarf);
            Add("mc_subtype_earth", MonsterSubtype.earth);
            Add("mc_subtype_electricity", MonsterSubtype.electricity);
            Add("mc_subtype_elf", MonsterSubtype.elf);
            Add("mc_subtype_evil", MonsterSubtype.evil);
            Add("mc_subtype_fire", MonsterSubtype.fire);
            Add("mc_subtype_formian", MonsterSubtype.formian);
            Add("mc_subtype_gnoll", MonsterSubtype.gnoll);
            Add("mc_subtype_gnome", MonsterSubtype.gnome);
            Add("mc_subtype_goblinoid", MonsterSubtype.goblinoid);
            Add("mc_subtype_good", MonsterSubtype.good);
            Add("mc_subtype_guardinal", MonsterSubtype.guardinal);
            Add("mc_subtype_half_orc", MonsterSubtype.half_orc);
            Add("mc_subtype_halfling", MonsterSubtype.halfling);
            Add("mc_subtype_human", MonsterSubtype.human);
            Add("mc_subtype_lawful", MonsterSubtype.lawful);
            Add("mc_subtype_incorporeal", MonsterSubtype.incorporeal);
            Add("mc_subtype_orc", MonsterSubtype.orc);
            Add("mc_subtype_reptilian", MonsterSubtype.reptilian);
            Add("mc_subtype_slaadi", MonsterSubtype.slaadi);
            Add("mc_subtype_water", MonsterSubtype.water);
        }

        private static void AddMaterials()
        {
            Add("mat_stone", Material.stone);
            Add("mat_brick", Material.brick);
            Add("mat_wood", Material.wood);
            Add("mat_plant", Material.plant);
            Add("mat_flesh", Material.flesh);
            Add("mat_metal", Material.metal);
            Add("mat_glass", Material.glass);
            Add("mat_cloth", Material.cloth);
            Add("mat_liquid", Material.liquid);
            Add("mat_paper", Material.paper);
            Add("mat_gas", Material.gas);
            Add("mat_force", Material.force);
            Add("mat_fire", Material.fire);
            Add("mat_powder", Material.powder);
        }

        private static void AddEquipSlots()
        {
            Add("item_wear_helmet", EquipSlot.Helmet);
            Add("item_wear_necklace", EquipSlot.Necklace);
            Add("item_wear_gloves", EquipSlot.Gloves);
            Add("item_wear_weapon_primary", EquipSlot.WeaponPrimary);
            Add("item_wear_weapon_secondary", EquipSlot.WeaponSecondary);
            Add("item_wear_armor", EquipSlot.Armor);
            Add("item_wear_ring_primary", EquipSlot.RingPrimary);
            Add("item_wear_ring_secondary", EquipSlot.RingSecondary);
            Add("item_wear_boots", EquipSlot.Boots);
            Add("item_wear_ammo", EquipSlot.Ammo);
            Add("item_wear_cloak", EquipSlot.Cloak);
            Add("item_wear_shield", EquipSlot.Shield);
            Add("item_wear_robes", EquipSlot.Robes);
            Add("item_wear_bracers", EquipSlot.Bracers);
            Add("item_wear_bardic_item", EquipSlot.BardicItem);
            Add("item_wear_lockpicks", EquipSlot.Lockpicks);
        }

        private static void AddFollowerLootTypes()
        {
            Add("follower_loot_normal", LootSharingType.normal);
            Add("follower_loot_half_share_money_only", LootSharingType.half_share_money_only);
            Add("follower_loot_all_arcane_scrolls_nothing_else", LootSharingType.all_arcane_scrolls_nothing_else);
            Add("follower_loot_one_third_of_all", LootSharingType.one_third_of_all);
            Add("follower_loot_one_fifth_of_all", LootSharingType.one_fifth_of_all);
            Add("follower_loot_nothing", LootSharingType.nothing);
        }

        private static void AddFeats()
        {
            var featEnums = Tig.FS.ReadMesFile("rules/feat_enum.mes");
            foreach (var featId in (FeatId[]) typeof(FeatId).GetEnumValues())
            {
                var mesKey = 20000 + (int) featId;
                if (featEnums.TryGetValue(mesKey, out var featEnumName))
                {
                    Add(featEnumName, featId);
                }
            }
        }

        private static void AddTerrainType()
        {
            Add("TERRAIN_SCRUB", MapTerrain.Scrub);
            Add("TERRAIN_F_ROAD", MapTerrain.RoadFlag);
            Add("TERRAIN_FOREST", MapTerrain.Forest);
            Add("TERRAIN_SWAMP", MapTerrain.Swamp);
            Add("TERRAIN_RIVERSIDE", MapTerrain.Riverside);
        }

        private static void AddTutorialTopics()
        {
            Add("TAG_TUT_REST_CAMP", TutorialTopic.RestCamp);
            Add("TAG_TUT_PORTRAITS", TutorialTopic.Portraits);
            Add("TAG_TUT_OPEN_DOOR", TutorialTopic.OpenDoor);
            Add("TAG_TUT_OPEN_CHEST", TutorialTopic.OpenChest);
            Add("TAG_TUT_INVENTORY", TutorialTopic.Inventory);
            Add("TAG_TUT_DIALOGUE", TutorialTopic.Dialogue);
            Add("TAG_TUT_MOVEMENT", TutorialTopic.Movement);
            Add("TAG_TUT_COMBAT_ACTION_BAR", TutorialTopic.CombatActionBar);
            Add("TAG_TUT_SELECT_CHARACTER", TutorialTopic.SelectCharacter);
            Add("TAG_TUT_CAST_SPELLS", TutorialTopic.CastSpells);
            Add("TAG_TUT_COMBAT_INITIATIVE_BAR", TutorialTopic.CombatInitiativeBar);
            Add("TAG_TUT_COMBAT_ATTACKING", TutorialTopic.CombatAttacking);
            Add("TAG_TUT_KEYS", TutorialTopic.Keys);
            Add("TAG_TUT_TRADE_ITEMS", TutorialTopic.TradeItems);
            Add("TAG_TUT_USE_POTIONS", TutorialTopic.UsePotions);
            Add("TAG_TUT_MEMORIZE_SPELLS", TutorialTopic.MemorizeSpells);
            Add("TAG_TUT_PASSAGE_ICON", TutorialTopic.PassageIcon);
            Add("TAG_TUT_LOOTING", TutorialTopic.Looting);
            Add("TAG_TUT_LOOTING_SWORD", TutorialTopic.LootingSword);
            Add("TAG_TUT_MULTIPLE_CHARACTERS", TutorialTopic.MultipleCharacters);
            Add("TAG_TUT_PICKLOCK", TutorialTopic.Picklock);
            Add("TAG_TUT_ROOM1_OVERVIEW", TutorialTopic.Room1Overview);
            Add("TAG_TUT_ROOM2_OVERVIEW", TutorialTopic.Room2Overview);
            Add("TAG_TUT_ROOM3_OVERVIEW", TutorialTopic.Room3Overview);
            Add("TAG_TUT_LOCKED_DOOR_REMINDER", TutorialTopic.LockedDoorReminder);
            Add("TAG_TUT_ROOM4_OVERVIEW", TutorialTopic.Room4Overview);
            Add("TAG_TUT_ROOM5_OVERVIEW", TutorialTopic.Room5Overview);
            Add("TAG_TUT_ROOM6_OVERVIEW", TutorialTopic.Room6Overview);
            Add("TAG_TUT_ROOM7_OVERVIEW", TutorialTopic.Room7Overview);
            Add("TAG_TUT_CAST_SPELLS_MAGIC_MISSILE", TutorialTopic.CastSpellsMagicMissile);
            Add("TAG_TUT_ROOM8_OVERVIEW", TutorialTopic.Room8Overview);
            Add("TAG_TUT_LOOT_PREFERENCE", TutorialTopic.LootPreference);
            Add("TAG_TUT_LOOT_REMINDER", TutorialTopic.LootReminder);
            Add("TAG_TUT_ROOM9_OVERVIEW", TutorialTopic.Room9Overview);
            Add("TAG_TUT_WAND_USE", TutorialTopic.WandUse);
            Add("TAG_TUT_WAND_FIRE", TutorialTopic.WandFire);
            Add("TAG_TUT_LOOT_PREFERENCE_ARIEL_DEAD", TutorialTopic.LootPreferenceArielDead);
            Add("TAG_TUT_REST_CAMP_ARIEL_DEAD", TutorialTopic.RestCampArielDead);
            Add("TAG_TUT_ARIEL_KILL", TutorialTopic.ArielKill);
        }

        private static void AddSignals()
        {
            Add("S_HP_Changed", D20DispatcherKey.SIG_HP_Changed);
            Add("S_HealSkill", D20DispatcherKey.SIG_HealSkill);
            Add("S_Sequence", D20DispatcherKey.SIG_Sequence);
            Add("S_Pre_Action_Sequence", D20DispatcherKey.SIG_Pre_Action_Sequence);
            Add("S_Action_Recipient", D20DispatcherKey.SIG_Action_Recipient);
            Add("S_BeginTurn", D20DispatcherKey.SIG_BeginTurn);
            Add("S_EndTurn", D20DispatcherKey.SIG_EndTurn);
            Add("S_Dropped_Enemy", D20DispatcherKey.SIG_Dropped_Enemy);
            Add("S_Concentration_Broken", D20DispatcherKey.SIG_Concentration_Broken);
            Add("S_Remove_Concentration", D20DispatcherKey.SIG_Remove_Concentration);
            Add("S_BreakFree", D20DispatcherKey.SIG_BreakFree);
            Add("S_Spell_Cast", D20DispatcherKey.SIG_Spell_Cast);
            Add("S_Spell_End", D20DispatcherKey.SIG_Spell_End);
            Add("S_Spell_Grapple_Removed", D20DispatcherKey.SIG_Spell_Grapple_Removed);
            Add("S_Killed", D20DispatcherKey.SIG_Killed);
            Add("S_AOOPerformed", D20DispatcherKey.SIG_AOOPerformed);
            Add("S_Aid_Another", D20DispatcherKey.SIG_Aid_Another);
            Add("S_TouchAttackAdded", D20DispatcherKey.SIG_TouchAttackAdded);
            Add("S_TouchAttack", D20DispatcherKey.SIG_TouchAttack);
            Add("S_Temporary_Hit_Points_Removed", D20DispatcherKey.SIG_Temporary_Hit_Points_Removed);
            Add("S_Standing_Up", D20DispatcherKey.SIG_Standing_Up);
            Add("S_Bardic_Music_Completed", D20DispatcherKey.SIG_Bardic_Music_Completed);
            Add("S_Combat_End", D20DispatcherKey.SIG_Combat_End);
            Add("S_Initiative_Update", D20DispatcherKey.SIG_Initiative_Update);
            Add("S_RadialMenu_Clear_Checkbox_Group", D20DispatcherKey.SIG_RadialMenu_Clear_Checkbox_Group);
            Add("S_Combat_Critter_Moved", D20DispatcherKey.SIG_Combat_Critter_Moved);
            Add("S_Hide", D20DispatcherKey.SIG_Hide);
            Add("S_Show", D20DispatcherKey.SIG_Show);
            Add("S_Feat_Remove_Slippery_Mind", D20DispatcherKey.SIG_Feat_Remove_Slippery_Mind);
            Add("S_Broadcast_Action", D20DispatcherKey.SIG_Broadcast_Action);
            Add("S_Remove_Disease", D20DispatcherKey.SIG_Remove_Disease);
            Add("S_Rogue_Skill_Mastery_Init", D20DispatcherKey.SIG_Rogue_Skill_Mastery_Init);
            Add("S_Spell_Call_Lightning", D20DispatcherKey.SIG_Spell_Call_Lightning);
            Add("S_Magical_Item_Deactivate", D20DispatcherKey.SIG_Magical_Item_Deactivate);
            Add("S_Spell_Mirror_Image_Struck", D20DispatcherKey.SIG_Spell_Mirror_Image_Struck);
            Add("S_Spell_Sanctuary_Attempt_Save", D20DispatcherKey.SIG_Spell_Sanctuary_Attempt_Save);
            Add("S_Experience_Awarded", D20DispatcherKey.SIG_Experience_Awarded);
            Add("S_Pack", D20DispatcherKey.SIG_Pack);
            Add("S_Unpack", D20DispatcherKey.SIG_Unpack);
            Add("S_Teleport_Prepare", D20DispatcherKey.SIG_Teleport_Prepare);
            Add("S_Teleport_Reconnect", D20DispatcherKey.SIG_Teleport_Reconnect);
            Add("S_Atone_Fallen_Paladin", D20DispatcherKey.SIG_Atone_Fallen_Paladin);
            Add("S_Summon_Creature", D20DispatcherKey.SIG_Summon_Creature);
            Add("S_Attack_Made", D20DispatcherKey.SIG_Attack_Made);
            Add("S_Golden_Skull_Combine", D20DispatcherKey.SIG_Golden_Skull_Combine);
            Add("S_Inventory_Update", D20DispatcherKey.SIG_Inventory_Update);
            Add("S_Critter_Killed", D20DispatcherKey.SIG_Critter_Killed);
            Add("S_SetPowerAttack", D20DispatcherKey.SIG_SetPowerAttack);
            Add("S_SetExpertise", D20DispatcherKey.SIG_SetExpertise);
            Add("S_SetCastDefensively", D20DispatcherKey.SIG_SetCastDefensively);
            Add("S_Resurrection", D20DispatcherKey.SIG_Resurrection);
            Add("S_Dismiss_Spells", D20DispatcherKey.SIG_Dismiss_Spells);
            Add("S_DealNormalDamage", D20DispatcherKey.SIG_DealNormalDamage);
            Add("S_Update_Encumbrance", D20DispatcherKey.SIG_Update_Encumbrance);
            Add("S_Remove_AI_Controlled", D20DispatcherKey.SIG_Remove_AI_Controlled);
            Add("S_Verify_Obj_Conditions", D20DispatcherKey.SIG_Verify_Obj_Conditions);
            Add("S_Web_Burning", D20DispatcherKey.SIG_Web_Burning);
            Add("S_Anim_CastConjureEnd", D20DispatcherKey.SIG_Anim_CastConjureEnd);
            Add("S_Item_Remove_Enhancement", D20DispatcherKey.SIG_Item_Remove_Enhancement);
            Add("S_Disarmed_Weapon_Retrieve", D20DispatcherKey.SIG_Disarmed_Weapon_Retrieve);
            Add("S_Disarm", D20DispatcherKey.SIG_Disarm);
            Add("S_AID_ANOTHER_WAKE_UP", D20DispatcherKey.SIG_AID_ANOTHER_WAKE_UP);
        }

        private static void AddSizeCategory()
        {
            Add("STAT_SIZE_NONE", SizeCategory.None);
            Add("STAT_SIZE_FINE", SizeCategory.Fine);
            Add("STAT_SIZE_DIMINUTIVE", SizeCategory.Diminutive);
            Add("STAT_SIZE_TINY", SizeCategory.Tiny);
            Add("STAT_SIZE_SMALL", SizeCategory.Small);
            Add("STAT_SIZE_MEDIUM", SizeCategory.Medium);
            Add("STAT_SIZE_LARGE", SizeCategory.Large);
            Add("STAT_SIZE_HUGE", SizeCategory.Huge);
            Add("STAT_SIZE_GARGANTUAN", SizeCategory.Gargantuan);
            Add("STAT_SIZE_COLOSSAL", SizeCategory.Colossal);
        }

        private static void AddStandpoints()
        {
            Add("STANDPOINT_DAY", StandPointType.Day);
            Add("STANDPOINT_NIGHT", StandPointType.Night);
            Add("STANDPOINT_SCOUT", StandPointType.Scout);
        }

        private static void AddTargetListOrder()
        {
            Add("SORT_TARGET_LIST_BY_HIT_DICE", TargetListOrder.HitDice);
            Add("SORT_TARGET_LIST_BY_HIT_DICE_THEN_DIST", TargetListOrder.HitDiceThenDist);
            Add("SORT_TARGET_LIST_BY_DIST", TargetListOrder.Dist);
            Add("SORT_TARGET_LIST_BY_DIST_FROM_CASTER", TargetListOrder.DistFromCaster);

            Add("SORT_TARGET_LIST_ORDER_ASCENDING", TargetListOrderDirection.Ascending);
            Add("SORT_TARGET_LIST_ORDER_DESCENDING", TargetListOrderDirection.Descending);
        }

        private static void AddSleepStatus()
        {
            Add("SLEEP_SAFE", SleepStatus.Safe);
            Add("SLEEP_DANGEROUS", SleepStatus.Dangerous);
            Add("SLEEP_IMPOSSIBLE", SleepStatus.Impossible);
            Add("SLEEP_PASS_TIME_ONLY", SleepStatus.PassTimeOnly);
        }

        private static void AddRadialMenuParam()
        {
            Add("RADIAL_MENU_PARAM_MIN_SETTING", RadialMenuParam.MinSetting);
            Add("RADIAL_MENU_PARAM_MAX_SETTING", RadialMenuParam.MaxSetting);
            Add("RADIAL_MENU_PARAM_ACTUAL_SETTING", RadialMenuParam.ActualSetting);
        }

        private static void AddQueries()
        {
            Add("Q_Helpless", D20DispatcherKey.QUE_Helpless);
            Add("Q_SneakAttack", D20DispatcherKey.QUE_SneakAttack);
            Add("Q_OpponentSneakAttack", D20DispatcherKey.QUE_OpponentSneakAttack);
            Add("Q_CoupDeGrace", D20DispatcherKey.QUE_CoupDeGrace);
            Add("Q_Mute", D20DispatcherKey.QUE_Mute);
            Add("Q_CannotCast", D20DispatcherKey.QUE_CannotCast);
            Add("Q_CannotUseIntSkill", D20DispatcherKey.QUE_CannotUseIntSkill);
            Add("Q_CannotUseChaSkill", D20DispatcherKey.QUE_CannotUseChaSkill);
            Add("Q_RapidShot", D20DispatcherKey.QUE_RapidShot);
            Add("Q_Critter_Is_Concentrating", D20DispatcherKey.QUE_Critter_Is_Concentrating);
            Add("Q_Critter_Is_On_Consecrate_Ground", D20DispatcherKey.QUE_Critter_Is_On_Consecrate_Ground);
            Add("Q_Critter_Is_On_Desecrate_Ground", D20DispatcherKey.QUE_Critter_Is_On_Desecrate_Ground);
            Add("Q_Critter_Is_Held", D20DispatcherKey.QUE_Critter_Is_Held);
            Add("Q_Critter_Is_Invisible", D20DispatcherKey.QUE_Critter_Is_Invisible);
            Add("Q_Critter_Is_Afraid", D20DispatcherKey.QUE_Critter_Is_Afraid);
            Add("Q_Critter_Is_Blinded", D20DispatcherKey.QUE_Critter_Is_Blinded);
            Add("Q_Critter_Is_Charmed", D20DispatcherKey.QUE_Critter_Is_Charmed);
            Add("Q_Critter_Is_Confused", D20DispatcherKey.QUE_Critter_Is_Confused);
            Add("Q_Critter_Is_AIControlled", D20DispatcherKey.QUE_Critter_Is_AIControlled);
            Add("Q_Critter_Is_Cursed", D20DispatcherKey.QUE_Critter_Is_Cursed);
            Add("Q_Critter_Is_Deafened", D20DispatcherKey.QUE_Critter_Is_Deafened);
            Add("Q_Critter_Is_Diseased", D20DispatcherKey.QUE_Critter_Is_Diseased);
            Add("Q_Critter_Is_Poisoned", D20DispatcherKey.QUE_Critter_Is_Poisoned);
            Add("Q_Critter_Is_Stunned", D20DispatcherKey.QUE_Critter_Is_Stunned);
            Add("Q_Critter_Is_Immune_Critical_Hits", D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits);
            Add("Q_Critter_Is_Immune_Poison", D20DispatcherKey.QUE_Critter_Is_Immune_Poison);
            Add("Q_Critter_Has_Spell_Resistance", D20DispatcherKey.QUE_Critter_Has_Spell_Resistance);
            Add("Q_Critter_Has_Condition", D20DispatcherKey.QUE_Critter_Has_Condition);
            Add("Q_Critter_Has_Freedom_of_Movement", D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement);
            Add("Q_Critter_Has_Endure_Elements", D20DispatcherKey.QUE_Critter_Has_Endure_Elements);
            Add("Q_Critter_Has_Protection_From_Elements", D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements);
            Add("Q_Critter_Has_Resist_Elements", D20DispatcherKey.QUE_Critter_Has_Resist_Elements);
            Add("Q_Critter_Has_True_Seeing", D20DispatcherKey.QUE_Critter_Has_True_Seeing);
            Add("Q_Critter_Has_Spell_Active", D20DispatcherKey.QUE_Critter_Has_Spell_Active);
            Add("Q_Critter_Can_Call_Lightning", D20DispatcherKey.QUE_Critter_Can_Call_Lightning);
            Add("Q_Critter_Can_See_Invisible", D20DispatcherKey.QUE_Critter_Can_See_Invisible);
            Add("Q_Critter_Can_See_Darkvision", D20DispatcherKey.QUE_Critter_Can_See_Darkvision);
            Add("Q_Critter_Can_See_Ethereal", D20DispatcherKey.QUE_Critter_Can_See_Ethereal);
            Add("Q_Critter_Can_Discern_Lies", D20DispatcherKey.QUE_Critter_Can_Discern_Lies);
            Add("Q_Critter_Can_Detect_Chaos", D20DispatcherKey.QUE_Critter_Can_Detect_Chaos);
            Add("Q_Critter_Can_Detect_Evil", D20DispatcherKey.QUE_Critter_Can_Detect_Evil);
            Add("Q_Critter_Can_Detect_Good", D20DispatcherKey.QUE_Critter_Can_Detect_Good);
            Add("Q_Critter_Can_Detect_Law", D20DispatcherKey.QUE_Critter_Can_Detect_Law);
            Add("Q_Critter_Can_Detect_Magic", D20DispatcherKey.QUE_Critter_Can_Detect_Magic);
            Add("Q_Critter_Can_Detect_Undead", D20DispatcherKey.QUE_Critter_Can_Detect_Undead);
            Add("Q_Critter_Can_Find_Traps", D20DispatcherKey.QUE_Critter_Can_Find_Traps);
            Add("Q_Critter_Can_Dismiss_Spells", D20DispatcherKey.QUE_Critter_Can_Dismiss_Spells);
            Add("Q_Obj_Is_Blessed", D20DispatcherKey.QUE_Obj_Is_Blessed);
            Add("Q_Unconscious", D20DispatcherKey.QUE_Unconscious);
            Add("Q_Dying", D20DispatcherKey.QUE_Dying);
            Add("Q_Dead", D20DispatcherKey.QUE_Dead);
            Add("Q_AOOPossible", D20DispatcherKey.QUE_AOOPossible);
            Add("Q_AOOIncurs", D20DispatcherKey.QUE_AOOIncurs);
            Add("Q_HoldingCharge", D20DispatcherKey.QUE_HoldingCharge);
            Add("Q_Has_Temporary_Hit_Points", D20DispatcherKey.QUE_Has_Temporary_Hit_Points);
            Add("Q_SpellInterrupted", D20DispatcherKey.QUE_SpellInterrupted);
            Add("Q_ActionTriggersAOO", D20DispatcherKey.QUE_ActionTriggersAOO);
            Add("Q_ActionAllowed", D20DispatcherKey.QUE_ActionAllowed);
            Add("Q_Prone", D20DispatcherKey.QUE_Prone);
            Add("Q_RerollSavingThrow", D20DispatcherKey.QUE_RerollSavingThrow);
            Add("Q_RerollAttack", D20DispatcherKey.QUE_RerollAttack);
            Add("Q_RerollCritical", D20DispatcherKey.QUE_RerollCritical);
            Add("Q_Commanded", D20DispatcherKey.QUE_Commanded);
            Add("Q_Turned", D20DispatcherKey.QUE_Turned);
            Add("Q_Rebuked", D20DispatcherKey.QUE_Rebuked);
            Add("Q_CanBeFlanked", D20DispatcherKey.QUE_CanBeFlanked);
            Add("Q_Critter_Is_Grappling", D20DispatcherKey.QUE_Critter_Is_Grappling);
            Add("Q_Barbarian_Raged", D20DispatcherKey.QUE_Barbarian_Raged);
            Add("Q_Barbarian_Fatigued", D20DispatcherKey.QUE_Barbarian_Fatigued);
            Add("Q_NewRound_This_Turn", D20DispatcherKey.QUE_NewRound_This_Turn);
            Add("Q_Flatfooted", D20DispatcherKey.QUE_Flatfooted);
            Add("Q_Masterwork", D20DispatcherKey.QUE_Masterwork);
            Add("Q_FailedDecipherToday", D20DispatcherKey.QUE_FailedDecipherToday);
            Add("Q_Polymorphed", D20DispatcherKey.QUE_Polymorphed);
            Add("Q_IsActionInvalid_CheckAction", D20DispatcherKey.QUE_IsActionInvalid_CheckAction);
            Add("Q_CanBeAffected_PerformAction", D20DispatcherKey.QUE_CanBeAffected_PerformAction);
            Add("Q_CanBeAffected_ActionFrame", D20DispatcherKey.QUE_CanBeAffected_ActionFrame);
            Add("Q_AOOWillTake", D20DispatcherKey.QUE_AOOWillTake);
            Add("Q_Weapon_Is_Mighty_Cleaving", D20DispatcherKey.QUE_Weapon_Is_Mighty_Cleaving);
            Add("Q_Autoend_Turn", D20DispatcherKey.QUE_Autoend_Turn);
            Add("Q_ExperienceExempt", D20DispatcherKey.QUE_ExperienceExempt);
            Add("Q_FavoredClass", D20DispatcherKey.QUE_FavoredClass);
            Add("Q_IsFallenPaladin", D20DispatcherKey.QUE_IsFallenPaladin);
            Add("Q_WieldedTwoHanded", D20DispatcherKey.QUE_WieldedTwoHanded);
            Add("Q_Critter_Is_Immune_Energy_Drain", D20DispatcherKey.QUE_Critter_Is_Immune_Energy_Drain);
            Add("Q_Critter_Is_Immune_Death_Touch", D20DispatcherKey.QUE_Critter_Is_Immune_Death_Touch);
            Add("Q_Failed_Copy_Scroll", D20DispatcherKey.QUE_Failed_Copy_Scroll);
            Add("Q_Armor_Get_AC_Bonus", D20DispatcherKey.QUE_Armor_Get_AC_Bonus);
            Add("Q_Armor_Get_Max_DEX_Bonus", D20DispatcherKey.QUE_Armor_Get_Max_DEX_Bonus);
            Add("Q_Armor_Get_Max_Speed", D20DispatcherKey.QUE_Armor_Get_Max_Speed);
            Add("Q_FightingDefensively", D20DispatcherKey.QUE_FightingDefensively);
            Add("Q_Elemental_Gem_State", D20DispatcherKey.QUE_Elemental_Gem_State);
            Add("Q_Untripable", D20DispatcherKey.QUE_Untripable);
            Add("Q_Has_Thieves_Tools", D20DispatcherKey.QUE_Has_Thieves_Tools);
            Add("Q_Critter_Is_Encumbered_Light", D20DispatcherKey.QUE_Critter_Is_Encumbered_Light);
            Add("Q_Critter_Is_Encumbered_Medium", D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium);
            Add("Q_Critter_Is_Encumbered_Heavy", D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy);
            Add("Q_Critter_Is_Encumbered_Overburdened", D20DispatcherKey.QUE_Critter_Is_Encumbered_Overburdened);
            Add("Q_Has_Aura_Of_Courage", D20DispatcherKey.QUE_Has_Aura_Of_Courage);
            Add("Q_BardicInstrument", D20DispatcherKey.QUE_BardicInstrument);
            Add("Q_EnterCombat", D20DispatcherKey.QUE_EnterCombat);
            Add("Q_AI_Fireball_OK", D20DispatcherKey.QUE_AI_Fireball_OK);
            Add("Q_Critter_Cannot_Loot", D20DispatcherKey.QUE_Critter_Cannot_Loot);
            Add("Q_Critter_Cannot_Wield_Items", D20DispatcherKey.QUE_Critter_Cannot_Wield_Items);
            Add("Q_Critter_Is_Spell_An_Ability", D20DispatcherKey.QUE_Critter_Is_Spell_An_Ability);
            Add("Q_Play_Critical_Hit_Anim", D20DispatcherKey.QUE_Play_Critical_Hit_Anim);
            Add("Q_Is_BreakFree_Possible", D20DispatcherKey.QUE_Is_BreakFree_Possible);
            Add("Q_Critter_Has_Mirror_Image", D20DispatcherKey.QUE_Critter_Has_Mirror_Image);
            Add("Q_Wearing_Ring_of_Change", D20DispatcherKey.QUE_Wearing_Ring_of_Change);
            Add("Q_Critter_Has_No_Con_Score", D20DispatcherKey.QUE_Critter_Has_No_Con_Score);
            Add("Q_Item_Has_Enhancement_Bonus", D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus);
            Add("Q_Item_Has_Keen_Bonus", D20DispatcherKey.QUE_Item_Has_Keen_Bonus);
            Add("Q_AI_Has_Spell_Override", D20DispatcherKey.QUE_AI_Has_Spell_Override);
            Add("Q_Weapon_Get_Keen_Bonus", D20DispatcherKey.QUE_Weapon_Get_Keen_Bonus);
            Add("Q_Disarmed", D20DispatcherKey.QUE_Disarmed);
            Add("Q_Can_Perform_Disarm", D20DispatcherKey.QUE_Can_Perform_Disarm);
            Add("Q_Craft_Wand_Spell_Level", D20DispatcherKey.QUE_Craft_Wand_Spell_Level);
            Add("Q_Is_Ethereal", D20DispatcherKey.QUE_Is_Ethereal);
            Add("Q_Empty_Body_Num_Rounds", D20DispatcherKey.QUE_Empty_Body_Num_Rounds);
        }

        private static void AddPortalFlags()
        {
            Add("OPF_LOCKED", PortalFlag.LOCKED);
            Add("OPF_JAMMED", PortalFlag.JAMMED);
            Add("OPF_MAGICALLY_HELD", PortalFlag.MAGICALLY_HELD);
            Add("OPF_NEVER_LOCKED", PortalFlag.NEVER_LOCKED);
            Add("OPF_ALWAYS_LOCKED", PortalFlag.ALWAYS_LOCKED);
            Add("OPF_LOCKED_DAY", PortalFlag.LOCKED_DAY);
            Add("OPF_LOCKED_NIGHT", PortalFlag.LOCKED_NIGHT);
            Add("OPF_BUSTED", PortalFlag.BUSTED);
            Add("OPF_NOT_STICKY", PortalFlag.NOT_STICKY);
            Add("OPF_OPEN", PortalFlag.OPEN);
        }

        private static void AddWeaponFlags()
        {
            Add("OWF_LOUD", WeaponFlag.LOUD);
            Add("OWF_SILENT", WeaponFlag.SILENT);
            Add("OWF_UNUSED_1", WeaponFlag.UNUSED_1);
            Add("OWF_UNUSED_2", WeaponFlag.UNUSED_2);
            Add("OWF_THROWABLE", WeaponFlag.THROWABLE);
            Add("OWF_TRANS_PROJECTILE", WeaponFlag.TRANS_PROJECTILE);
            Add("OWF_BOOMERANGS", WeaponFlag.BOOMERANGS);
            Add("OWF_IGNORE_RESISTANCE", WeaponFlag.IGNORE_RESISTANCE);
            Add("OWF_DAMAGE_ARMOR", WeaponFlag.DAMAGE_ARMOR);
            Add("OWF_DEFAULT_THROWS", WeaponFlag.DEFAULT_THROWS);
            Add("OWF_RANGED_WEAPON", WeaponFlag.RANGED_WEAPON);
            Add("OWF_WEAPON_LOADED", WeaponFlag.WEAPON_LOADED);
            Add("OWF_MAGIC_STAFF", WeaponFlag.MAGIC_STAFF);
        }

        private static void AddNpcFlags()
        {
            Add("ONF_EXTRAPLANAR", NpcFlag.EXTRAPLANAR);
            Add("ONF_EX_FOLLOWER", NpcFlag.EX_FOLLOWER);
            Add("ONF_WAYPOINTS_DAY", NpcFlag.WAYPOINTS_DAY);
            Add("ONF_WAYPOINTS_NIGHT", NpcFlag.WAYPOINTS_NIGHT);
            Add("ONF_AI_WAIT_HERE", NpcFlag.AI_WAIT_HERE);
            Add("ONF_AI_SPREAD_OUT", NpcFlag.AI_SPREAD_OUT);
            Add("ONF_JILTED", NpcFlag.JILTED);
            Add("ONF_LOGBOOK_IGNORES", NpcFlag.LOGBOOK_IGNORES);
            Add("ONF_UNUSED_00000080", NpcFlag.UNUSED_00000080);
            Add("ONF_KOS", NpcFlag.KOS);
            Add("ONF_USE_ALERTPOINTS", NpcFlag.USE_ALERTPOINTS);
            Add("ONF_FORCED_FOLLOWER", NpcFlag.FORCED_FOLLOWER);
            Add("ONF_KOS_OVERRIDE", NpcFlag.KOS_OVERRIDE);
            Add("ONF_WANDERS", NpcFlag.WANDERS);
            Add("ONF_WANDERS_IN_DARK", NpcFlag.WANDERS_IN_DARK);
            Add("ONF_FENCE", NpcFlag.FENCE);
            Add("ONF_FAMILIAR", NpcFlag.FAMILIAR);
            Add("ONF_CHECK_LEADER", NpcFlag.CHECK_LEADER);
            Add("ONF_NO_EQUIP", NpcFlag.NO_EQUIP);
            Add("ONF_CAST_HIGHEST", NpcFlag.CAST_HIGHEST);
            Add("ONF_GENERATOR", NpcFlag.GENERATOR);
            Add("ONF_GENERATED", NpcFlag.GENERATED);
            Add("ONF_GENERATOR_RATE1", NpcFlag.GENERATOR_RATE1);
            Add("ONF_GENERATOR_RATE2", NpcFlag.GENERATOR_RATE2);
            Add("ONF_GENERATOR_RATE3", NpcFlag.GENERATOR_RATE3);
            Add("ONF_DEMAINTAIN_SPELLS", NpcFlag.DEMAINTAIN_SPELLS);
            Add("ONF_UNUSED_02000000", NpcFlag.UNUSED_02000000);
            Add("ONF_UNUSED_04000000", NpcFlag.UNUSED_04000000);
            Add("ONF_UNUSED_08000000", NpcFlag.UNUSED_08000000);
            Add("ONF_BACKING_OFF", NpcFlag.BACKING_OFF);
            Add("ONF_NO_ATTACK", NpcFlag.NO_ATTACK);
            Add("ONF_BOSS_MONSTER", NpcFlag.BOSS_MONSTER);
        }

        private static void AddObjectListCategories()
        {
            Add("OLC_NONE", ObjectListFilter.OLC_NONE);
            Add("OLC_PORTAL", ObjectListFilter.OLC_PORTAL);
            Add("OLC_CONTAINER", ObjectListFilter.OLC_CONTAINER);
            Add("OLC_SCENERY", ObjectListFilter.OLC_SCENERY);
            Add("OLC_PROJECTILE", ObjectListFilter.OLC_PROJECTILE);
            Add("OLC_WEAPON", ObjectListFilter.OLC_WEAPON);
            Add("OLC_AMMO", ObjectListFilter.OLC_AMMO);
            Add("OLC_ARMOR", ObjectListFilter.OLC_ARMOR);
            Add("OLC_MONEY", ObjectListFilter.OLC_MONEY);
            Add("OLC_FOOD", ObjectListFilter.OLC_FOOD);
            Add("OLC_SCROLL", ObjectListFilter.OLC_SCROLL);
            Add("OLC_KEY", ObjectListFilter.OLC_KEY);
            Add("OLC_BAG", ObjectListFilter.OLC_BAG);
            Add("OLC_WRITTEN", ObjectListFilter.OLC_WRITTEN);
            Add("OLC_GENERIC", ObjectListFilter.OLC_GENERIC);
            Add("OLC_ITEMS", ObjectListFilter.OLC_ITEMS);
            Add("OLC_PC", ObjectListFilter.OLC_PC);
            Add("OLC_NPC", ObjectListFilter.OLC_NPC);
            Add("OLC_CRITTERS", ObjectListFilter.OLC_CRITTERS);
            Add("OLC_MOBILE", ObjectListFilter.OLC_MOBILE);
            Add("OLC_TRAP", ObjectListFilter.OLC_TRAP);
            Add("OLC_ALL", ObjectListFilter.OLC_ALL);
        }

        private static void AddItemFlags()
        {
            Add("OIF_IDENTIFIED", ItemFlag.IDENTIFIED);
            Add("OIF_WONT_SELL", ItemFlag.WONT_SELL);
            Add("OIF_IS_MAGICAL", ItemFlag.IS_MAGICAL);
            Add("OIF_NO_PICKPOCKET", ItemFlag.NO_PICKPOCKET);
            Add("OIF_NO_DISPLAY", ItemFlag.NO_DISPLAY);
            Add("OIF_NO_DROP", ItemFlag.NO_DROP);
            Add("OIF_NEEDS_SPELL", ItemFlag.NEEDS_SPELL);
            Add("OIF_CAN_USE_BOX", ItemFlag.CAN_USE_BOX);
            Add("OIF_NEEDS_TARGET", ItemFlag.NEEDS_TARGET);
            Add("OIF_LIGHT_SMALL", ItemFlag.LIGHT_SMALL);
            Add("OIF_LIGHT_MEDIUM", ItemFlag.LIGHT_MEDIUM);
            Add("OIF_LIGHT_LARGE", ItemFlag.LIGHT_LARGE);
            Add("OIF_LIGHT_XLARGE", ItemFlag.LIGHT_XLARGE);
            Add("OIF_PERSISTENT", ItemFlag.PERSISTENT);
            Add("OIF_MT_TRIGGERED", ItemFlag.MT_TRIGGERED);
            Add("OIF_STOLEN", ItemFlag.STOLEN);
            Add("OIF_USE_IS_THROW", ItemFlag.USE_IS_THROW);
            Add("OIF_NO_DECAY", ItemFlag.NO_DECAY);
            Add("OIF_UBER", ItemFlag.UBER);
            Add("OIF_NO_NPC_PICKUP", ItemFlag.NO_NPC_PICKUP);
            Add("OIF_NO_RANGED_USE", ItemFlag.NO_RANGED_USE);
            Add("OIF_VALID_AI_ACTION", ItemFlag.VALID_AI_ACTION);
            Add("OIF_DRAW_WHEN_PARENTED", ItemFlag.DRAW_WHEN_PARENTED);
            Add("OIF_EXPIRES_AFTER_USE", ItemFlag.EXPIRES_AFTER_USE);
            Add("OIF_NO_LOOT", ItemFlag.NO_LOOT);
            Add("OIF_USES_WAND_ANIM", ItemFlag.USES_WAND_ANIM);
            Add("OIF_NO_TRANSFER", ItemFlag.NO_TRANSFER);
        }

        private static void AddObjectFlags()
        {
            Add("OF_RADIUS_SET", ObjectFlag.RADIUS_SET);
            Add("OF_DESTROYED", ObjectFlag.DESTROYED);
            Add("OF_OFF", ObjectFlag.OFF);
            Add("OF_FLAT", ObjectFlag.FLAT);
            Add("OF_TEXT", ObjectFlag.TEXT);
            Add("OF_SEE_THROUGH", ObjectFlag.SEE_THROUGH);
            Add("OF_SHOOT_THROUGH", ObjectFlag.SHOOT_THROUGH);
            Add("OF_TRANSLUCENT", ObjectFlag.TRANSLUCENT);
            Add("OF_SHRUNK", ObjectFlag.SHRUNK);
            Add("OF_DONTDRAW", ObjectFlag.DONTDRAW);
            Add("OF_INVISIBLE", ObjectFlag.INVISIBLE);
            Add("OF_NO_BLOCK", ObjectFlag.NO_BLOCK);
            Add("OF_CLICK_THROUGH", ObjectFlag.CLICK_THROUGH);
            Add("OF_INVENTORY", ObjectFlag.INVENTORY);
            Add("OF_DYNAMIC", ObjectFlag.DYNAMIC);
            Add("OF_PROVIDES_COVER", ObjectFlag.PROVIDES_COVER);
            Add("OF_RANDOM_SIZE", ObjectFlag.RANDOM_SIZE);
            Add("OF_NOHEIGHT", ObjectFlag.NOHEIGHT);
            Add("OF_WADING", ObjectFlag.WADING);
            Add("OF_UNUSED_40000", ObjectFlag.UNUSED_40000);
            Add("OF_STONED", ObjectFlag.STONED);
            Add("OF_DONTLIGHT", ObjectFlag.DONTLIGHT);
            Add("OF_TEXT_FLOATER", ObjectFlag.TEXT_FLOATER);
            Add("OF_INVULNERABLE", ObjectFlag.INVULNERABLE);
            Add("OF_EXTINCT", ObjectFlag.EXTINCT);
            Add("OF_TRAP_PC", ObjectFlag.TRAP_PC);
            Add("OF_TRAP_SPOTTED", ObjectFlag.TRAP_SPOTTED);
            Add("OF_DISALLOW_WADING", ObjectFlag.DISALLOW_WADING);
            Add("OF_UNUSED_08000000", ObjectFlag.UNUSED_08000000);
            Add("OF_HEIGHT_SET", ObjectFlag.HEIGHT_SET);
            Add("OF_ANIMATED_DEAD", ObjectFlag.ANIMATED_DEAD);
            Add("OF_TELEPORTED", ObjectFlag.TELEPORTED);
        }

        private static void AddContainerFlags()
        {
            Add("OCOF_LOCKED", ContainerFlag.LOCKED);
            Add("OCOF_JAMMED", ContainerFlag.JAMMED);
            Add("OCOF_MAGICALLY_HELD", ContainerFlag.MAGICALLY_HELD);
            Add("OCOF_NEVER_LOCKED", ContainerFlag.NEVER_LOCKED);
            Add("OCOF_ALWAYS_LOCKED", ContainerFlag.ALWAYS_LOCKED);
            Add("OCOF_LOCKED_DAY", ContainerFlag.LOCKED_DAY);
            Add("OCOF_LOCKED_NIGHT", ContainerFlag.LOCKED_NIGHT);
            Add("OCOF_BUSTED", ContainerFlag.BUSTED);
            Add("OCOF_NOT_STICKY", ContainerFlag.NOT_STICKY);
            Add("OCOF_INVEN_SPAWN_ONCE", ContainerFlag.INVEN_SPAWN_ONCE);
            Add("OCOF_INVEN_SPAWN_INDEPENDENT", ContainerFlag.INVEN_SPAWN_INDEPENDENT);
            Add("OCOF_OPEN", ContainerFlag.OPEN);
            Add("OCOF_HAS_BEEN_OPENED", ContainerFlag.HAS_BEEN_OPENED);
        }

        private static void AddCritterFlags()
        {
            Add("OCF_FATIGUE_LIMITING", CritterFlag.FATIGUE_LIMITING);
            Add("OCF_IS_CONCEALED", CritterFlag.IS_CONCEALED);
            Add("OCF_MOVING_SILENTLY", CritterFlag.MOVING_SILENTLY);
            Add("OCF_EXPERIENCE_AWARDED", CritterFlag.EXPERIENCE_AWARDED);
            Add("OCF_UNUSED_00000008", CritterFlag.UNUSED_00000008);
            Add("OCF_FLEEING", CritterFlag.FLEEING);
            Add("OCF_STUNNED", CritterFlag.STUNNED);
            Add("OCF_PARALYZED", CritterFlag.PARALYZED);
            Add("OCF_BLINDED", CritterFlag.BLINDED);
            Add("OCF_HAS_ARCANE_ABILITY", CritterFlag.HAS_ARCANE_ABILITY);
            Add("OCF_UNUSED_00000200", CritterFlag.UNUSED_00000200);
            Add("OCF_UNUSED_00000400", CritterFlag.UNUSED_00000400);
            Add("OCF_UNUSED_00000800", CritterFlag.UNUSED_00000800);
            Add("OCF_SLEEPING", CritterFlag.SLEEPING);
            Add("OCF_MUTE", CritterFlag.MUTE);
            Add("OCF_SURRENDERED", CritterFlag.SURRENDERED);
            Add("OCF_MONSTER", CritterFlag.MONSTER);
            Add("OCF_SPELL_FLEE", CritterFlag.SPELL_FLEE);
            Add("OCF_ENCOUNTER", CritterFlag.ENCOUNTER);
            Add("OCF_COMBAT_MODE_ACTIVE", CritterFlag.COMBAT_MODE_ACTIVE);
            Add("OCF_LIGHT_SMALL", CritterFlag.LIGHT_SMALL);
            Add("OCF_LIGHT_MEDIUM", CritterFlag.LIGHT_MEDIUM);
            Add("OCF_LIGHT_LARGE", CritterFlag.LIGHT_LARGE);
            Add("OCF_LIGHT_XLARGE", CritterFlag.LIGHT_XLARGE);
            Add("OCF_UNREVIVIFIABLE", CritterFlag.UNREVIVIFIABLE);
            Add("OCF_UNRESSURECTABLE", CritterFlag.UNRESURRECTABLE);
            Add("OCF_UNUSED_02000000", CritterFlag.UNUSED_02000000);
            Add("OCF_UNUSED_04000000", CritterFlag.UNUSED_04000000);
            Add("OCF_NO_FLEE", CritterFlag.NO_FLEE);
            Add("OCF_NON_LETHAL_COMBAT", CritterFlag.NON_LETHAL_COMBAT);
            Add("OCF_MECHANICAL", CritterFlag.MECHANICAL);
            Add("OCF_UNUSED_40000000", CritterFlag.UNUSED_40000000);
        }

        private static void AddFadeResults()
        {
            Add("OBJFADE_C_NONE", FadeOutResult.None);
            Add("OBJFADE_C_OBJ_DESTROY", FadeOutResult.Destroy);
            Add("OBJFADE_C_RUNOFF", FadeOutResult.RunOff);
            Add("OBJFADE_C_POOP_OFF", FadeOutResult.DropItemsAndDestroy);
        }

        private static void AddDeities()
        {
            Add("DEITY_NONE", DeityId.NONE);
            Add("DEITY_BOCCOB", DeityId.BOCCOB);
            Add("DEITY_CORELLON_LARETHIAN", DeityId.CORELLON_LARETHIAN);
            Add("DEITY_EHLONNA", DeityId.EHLONNA);
            Add("DEITY_ERYTHNUL", DeityId.ERYTHNUL);
            Add("DEITY_FHARLANGHN", DeityId.FHARLANGHN);
            Add("DEITY_GARL_GLITTERGOLD", DeityId.GARL_GLITTERGOLD);
            Add("DEITY_GRUUMSH", DeityId.GRUUMSH);
            Add("DEITY_HEIRONEOUS", DeityId.HEIRONEOUS);
            Add("DEITY_HEXTOR", DeityId.HEXTOR);
            Add("DEITY_KORD", DeityId.KORD);
            Add("DEITY_MORADIN", DeityId.MORADIN);
            Add("DEITY_NERULL", DeityId.NERULL);
            Add("DEITY_OBAD_HAI", DeityId.OBAD_HAI);
            Add("DEITY_OLIDAMMARA", DeityId.OLIDAMMARA);
            Add("DEITY_PELOR", DeityId.PELOR);
            Add("DEITY_ST_CUTHBERT", DeityId.ST_CUTHBERT);
            Add("DEITY_VECNA", DeityId.VECNA);
            Add("DEITY_WEE_JAS", DeityId.WEE_JAS);
            Add("DEITY_YONDALLA", DeityId.YONDALLA);
            Add("DEITY_OLD_FAITH", DeityId.OLD_FAITH);
            Add("DEITY_ZUGGTMOY", DeityId.ZUGGTMOY);
            Add("DEITY_IUZ", DeityId.IUZ);
            Add("DEITY_LOLTH", DeityId.LOLTH);
            Add("DEITY_PROCAN", DeityId.PROCAN);
            Add("DEITY_NOREBO", DeityId.NOREBO);
            Add("DEITY_PYREMIUS", DeityId.PYREMIUS);
            Add("DEITY_RALISHAZ", DeityId.RALISHAZ);
        }

        private static void AddSaveReduction()
        {
            Add("D20_Save_Reduction_None", D20SavingThrowReduction.None);
            Add("D20_Save_Reduction_Half", D20SavingThrowReduction.Half);
            Add("D20_Save_Reduction_Quarter", D20SavingThrowReduction.Quarter);
        }

        private static void AddSavingThrowTypes()
        {
            Add("D20_Save_Fortitude", SavingThrowType.Fortitude);
            Add("D20_Save_Reflex", SavingThrowType.Reflex);
            Add("D20_Save_Will", SavingThrowType.Will);
        }

        private static void AddSavingThrowFlags()
        {
            Add("D20STD_F_MAX", D20SavingThrowFlag.MAX);
            Add("D20STD_F_NONE", D20SavingThrowFlag.NONE);
            Add("D20STD_F_REROLL", D20SavingThrowFlag.REROLL);
            Add("D20STD_F_CHARM", D20SavingThrowFlag.CHARM);
            Add("D20STD_F_TRAP", D20SavingThrowFlag.TRAP);
            Add("D20STD_F_POISON", D20SavingThrowFlag.POISON);
            Add("D20STD_F_SPELL_LIKE_EFFECT", D20SavingThrowFlag.SPELL_LIKE_EFFECT);
            Add("D20STD_F_SPELL_SCHOOL_ABJURATION", D20SavingThrowFlag.SPELL_SCHOOL_ABJURATION);
            Add("D20STD_F_SPELL_SCHOOL_CONJURATION", D20SavingThrowFlag.SPELL_SCHOOL_CONJURATION);
            Add("D20STD_F_SPELL_SCHOOL_DIVINATION", D20SavingThrowFlag.SPELL_SCHOOL_DIVINATION);
            Add("D20STD_F_SPELL_SCHOOL_ENCHANTMENT", D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT);
            Add("D20STD_F_SPELL_SCHOOL_EVOCATION", D20SavingThrowFlag.SPELL_SCHOOL_EVOCATION);
            Add("D20STD_F_SPELL_SCHOOL_ILLUSION", D20SavingThrowFlag.SPELL_SCHOOL_ILLUSION);
            Add("D20STD_F_SPELL_SCHOOL_NECROMANCY", D20SavingThrowFlag.SPELL_SCHOOL_NECROMANCY);
            Add("D20STD_F_SPELL_SCHOOL_TRANSMUTATION", D20SavingThrowFlag.SPELL_SCHOOL_TRANSMUTATION);
            Add("D20STD_F_SPELL_DESCRIPTOR_ACID", D20SavingThrowFlag.SPELL_DESCRIPTOR_ACID);
            Add("D20STD_F_SPELL_DESCRIPTOR_CHAOTIC", D20SavingThrowFlag.SPELL_DESCRIPTOR_CHAOTIC);
            Add("D20STD_F_SPELL_DESCRIPTOR_COLD", D20SavingThrowFlag.SPELL_DESCRIPTOR_COLD);
            Add("D20STD_F_SPELL_DESCRIPTOR_DARKNESS", D20SavingThrowFlag.SPELL_DESCRIPTOR_DARKNESS);
            Add("D20STD_F_SPELL_DESCRIPTOR_DEATH", D20SavingThrowFlag.SPELL_DESCRIPTOR_DEATH);
            Add("D20STD_F_SPELL_DESCRIPTOR_ELECTRICITY", D20SavingThrowFlag.SPELL_DESCRIPTOR_ELECTRICITY);
            Add("D20STD_F_SPELL_DESCRIPTOR_EVIL", D20SavingThrowFlag.SPELL_DESCRIPTOR_EVIL);
            Add("D20STD_F_SPELL_DESCRIPTOR_FEAR", D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR);
            Add("D20STD_F_SPELL_DESCRIPTOR_FIRE", D20SavingThrowFlag.SPELL_DESCRIPTOR_FIRE);
            Add("D20STD_F_SPELL_DESCRIPTOR_FORCE", D20SavingThrowFlag.SPELL_DESCRIPTOR_FORCE);
            Add("D20STD_F_SPELL_DESCRIPTOR_GOOD", D20SavingThrowFlag.SPELL_DESCRIPTOR_GOOD);
            Add("D20STD_F_SPELL_DESCRIPTOR_LANGUAGE_DEPENDENT", D20SavingThrowFlag.SPELL_DESCRIPTOR_LANGUAGE_DEPENDENT);
            Add("D20STD_F_SPELL_DESCRIPTOR_LAWFUL", D20SavingThrowFlag.SPELL_DESCRIPTOR_LAWFUL);
            Add("D20STD_F_SPELL_DESCRIPTOR_LIGHT", D20SavingThrowFlag.SPELL_DESCRIPTOR_LIGHT);
            Add("D20STD_F_SPELL_DESCRIPTOR_MIND_AFFECTING", D20SavingThrowFlag.SPELL_DESCRIPTOR_MIND_AFFECTING);
            Add("D20STD_F_SPELL_DESCRIPTOR_SONIC", D20SavingThrowFlag.SPELL_DESCRIPTOR_SONIC);
            Add("D20STD_F_SPELL_DESCRIPTOR_TELEPORTATION", D20SavingThrowFlag.SPELL_DESCRIPTOR_TELEPORTATION);
            Add("D20STD_F_SPELL_DESCRIPTOR_AIR", D20SavingThrowFlag.SPELL_DESCRIPTOR_AIR);
            Add("D20STD_F_SPELL_DESCRIPTOR_EARTH", D20SavingThrowFlag.SPELL_DESCRIPTOR_EARTH);
            Add("D20STD_F_SPELL_DESCRIPTOR_WATER", D20SavingThrowFlag.SPELL_DESCRIPTOR_WATER);
            Add("D20STD_F_DISABLE_SLIPPERY_MIND", D20SavingThrowFlag.DISABLE_SLIPPERY_MIND);
        }

        private static void AddD20DamageType()
        {
            Add("D20DT_UNSPECIFIED", DamageType.Unspecified);
            Add("D20DT_BLUDGEONING", DamageType.Bludgeoning);
            Add("D20DT_PIERCING", DamageType.Piercing);
            Add("D20DT_SLASHING", DamageType.Slashing);
            Add("D20DT_BLUDGEONING_AND_PIERCING", DamageType.BludgeoningAndPiercing);
            Add("D20DT_PIERCING_AND_SLASHING", DamageType.PiercingAndSlashing);
            Add("D20DT_SLASHING_AND_BLUDGEONING", DamageType.SlashingAndBludgeoning);
            Add("D20DT_SLASHING_AND_BLUDGEONING_AND_PIERCING", DamageType.SlashingAndBludgeoningAndPiercing);
            Add("D20DT_ACID", DamageType.Acid);
            Add("D20DT_COLD", DamageType.Cold);
            Add("D20DT_ELECTRICITY", DamageType.Electricity);
            Add("D20DT_FIRE", DamageType.Fire);
            Add("D20DT_SONIC", DamageType.Sonic);
            Add("D20DT_NEGATIVE_ENERGY", DamageType.NegativeEnergy);
            Add("D20DT_SUBDUAL", DamageType.Subdual);
            Add("D20DT_POISON", DamageType.Poison);
            Add("D20DT_POSITIVE_ENERGY", DamageType.PositiveEnergy);
            Add("D20DT_FORCE", DamageType.Force);
            Add("D20DT_BLOOD_LOSS", DamageType.BloodLoss);
            Add("D20DT_MAGIC", DamageType.Magic);
        }

        private static void AddD20DAP()
        {
            Add("D20DAP_NORMAL", D20AttackPower.NORMAL);
            Add("D20DAP_UNSPECIFIED", D20AttackPower.UNSPECIFIED);
            Add("D20DAP_SILVER", D20AttackPower.SILVER);
            Add("D20DAP_MAGIC", D20AttackPower.MAGIC);
            Add("D20DAP_HOLY", D20AttackPower.HOLY);
            Add("D20DAP_UNHOLY", D20AttackPower.UNHOLY);
            Add("D20DAP_CHAOS", D20AttackPower.CHAOS);
            Add("D20DAP_LAW", D20AttackPower.LAW);
            Add("D20DAP_ADAMANTIUM", D20AttackPower.ADAMANTIUM);
            Add("D20DAP_BLUDGEONING", D20AttackPower.BLUDGEONING);
            Add("D20DAP_PIERCING", D20AttackPower.PIERCING);
            Add("D20DAP_SLASHING", D20AttackPower.SLASHING);
            Add("D20DAP_MITHRIL", D20AttackPower.MITHRIL);
            Add("D20DAP_COLD", D20AttackPower.COLD);
        }

        private static void AddD20CAF()
        {
            Add("D20CAF_UNNECESSARY", D20CAF.UNNECESSARY);
            Add("D20CAF_NONE", D20CAF.NONE); // New
            Add("D20CAF_HIT", D20CAF.HIT);
            Add("D20CAF_CRITICAL", D20CAF.CRITICAL);
            Add("D20CAF_RANGED", D20CAF.RANGED);
            Add("D20CAF_ACTIONFRAME_PROCESSED", D20CAF.ACTIONFRAME_PROCESSED);
            Add("D20CAF_NEED_PROJECTILE_HIT", D20CAF.NEED_PROJECTILE_HIT);
            Add("D20CAF_NEED_ANIM_COMPLETED", D20CAF.NEED_ANIM_COMPLETED);
            Add("D20CAF_ATTACK_OF_OPPORTUNITY", D20CAF.ATTACK_OF_OPPORTUNITY);
            Add("D20CAF_CONCEALMENT_MISS", D20CAF.CONCEALMENT_MISS);
            Add("D20CAF_TOUCH_ATTACK", D20CAF.TOUCH_ATTACK);
            Add("D20CAF_FREE_ACTION", D20CAF.FREE_ACTION);
            Add("D20CAF_CHARGE", D20CAF.CHARGE);
            Add("D20CAF_REROLL", D20CAF.REROLL);
            Add("D20CAF_REROLL_CRITICAL", D20CAF.REROLL_CRITICAL);
            Add("D20CAF_TRAP", D20CAF.TRAP);
            Add("D20CAF_ALTERNATE", D20CAF.ALTERNATE);
            Add("D20CAF_NO_PRECISION_DAMAGE", D20CAF.NO_PRECISION_DAMAGE);
            Add("D20CAF_FLANKED", D20CAF.FLANKED);
            Add("D20CAF_DEFLECT_ARROWS", D20CAF.DEFLECT_ARROWS);
            Add("D20CAF_FULL_ATTACK", D20CAF.FULL_ATTACK);
            Add("D20CAF_AOO_MOVEMENT", D20CAF.AOO_MOVEMENT);
            Add("D20CAF_BONUS_ATTACK", D20CAF.BONUS_ATTACK);
            Add("D20CAF_THROWN", D20CAF.THROWN);
            Add("D20CAF_SAVE_SUCCESSFUL", D20CAF.SAVE_SUCCESSFUL);
            Add("D20CAF_SECONDARY_WEAPON", D20CAF.SECONDARY_WEAPON);
            Add("D20CAF_MANYSHOT", D20CAF.MANYSHOT);
            Add("D20CAF_ALWAYS_HIT", D20CAF.ALWAYS_HIT);
            Add("D20CAF_COVER", D20CAF.COVER);
            Add("D20CAF_COUNTERSPELLED", D20CAF.COUNTERSPELLED);
            Add("D20CAF_THROWN_GRENADE", D20CAF.THROWN_GRENADE);
            Add("D20CAF_FINAL_ATTACK_ROLL", D20CAF.FINAL_ATTACK_ROLL);
            Add("D20CAF_TRUNCATED", D20CAF.TRUNCATED);
        }

        private static void AddD20ActionTypes()
        {
            Add("D20A_NONE", D20ActionType.NONE);
            Add("D20A_UNSPECIFIED_MOVE", D20ActionType.UNSPECIFIED_MOVE);
            Add("D20A_UNSPECIFIED_ATTACK", D20ActionType.UNSPECIFIED_ATTACK);
            Add("D20A_STANDARD_ATTACK", D20ActionType.STANDARD_ATTACK);
            Add("D20A_FULL_ATTACK", D20ActionType.FULL_ATTACK);
            Add("D20A_STANDARD_RANGED_ATTACK", D20ActionType.STANDARD_RANGED_ATTACK);
            Add("D20A_RELOAD", D20ActionType.RELOAD);
            Add("D20A_5FOOTSTEP", D20ActionType.FIVEFOOTSTEP);
            Add("D20A_MOVE", D20ActionType.MOVE);
            Add("D20A_DOUBLE_MOVE", D20ActionType.DOUBLE_MOVE);
            Add("D20A_RUN", D20ActionType.RUN);
            Add("D20A_CAST_SPELL", D20ActionType.CAST_SPELL);
            Add("D20A_HEAL", D20ActionType.HEAL);
            Add("D20A_CLEAVE", D20ActionType.CLEAVE);
            Add("D20A_ATTACK_OF_OPPORTUNITY", D20ActionType.ATTACK_OF_OPPORTUNITY);
            Add("D20A_WHIRLWIND_ATTACK", D20ActionType.WHIRLWIND_ATTACK);
            Add("D20A_TOUCH_ATTACK", D20ActionType.TOUCH_ATTACK);
            Add("D20A_TOTAL_DEFENSE", D20ActionType.TOTAL_DEFENSE);
            Add("D20A_CHARGE", D20ActionType.CHARGE);
            Add("D20A_FALL_TO_PRONE", D20ActionType.FALL_TO_PRONE);
            Add("D20A_STAND_UP", D20ActionType.STAND_UP);
            Add("D20A_TURN_UNDEAD", D20ActionType.TURN_UNDEAD);
            Add("D20A_DEATH_TOUCH", D20ActionType.DEATH_TOUCH);
            Add("D20A_PROTECTIVE_WARD", D20ActionType.PROTECTIVE_WARD);
            Add("D20A_FEAT_OF_STRENGTH", D20ActionType.FEAT_OF_STRENGTH);
            Add("D20A_BARDIC_MUSIC", D20ActionType.BARDIC_MUSIC);
            Add("D20A_PICKUP_OBJECT", D20ActionType.PICKUP_OBJECT);
            Add("D20A_COUP_DE_GRACE", D20ActionType.COUP_DE_GRACE);
            Add("D20A_USE_ITEM", D20ActionType.USE_ITEM);
            Add("D20A_BARBARIAN_RAGE", D20ActionType.BARBARIAN_RAGE);
            Add("D20A_STUNNING_FIST", D20ActionType.STUNNING_FIST);
            Add("D20A_SMITE_EVIL", D20ActionType.SMITE_EVIL);
            Add("D20A_LAY_ON_HANDS_SET", D20ActionType.LAY_ON_HANDS_SET);
            Add("D20A_DETECT_EVIL", D20ActionType.DETECT_EVIL);
            Add("D20A_STOP_CONCENTRATION", D20ActionType.STOP_CONCENTRATION);
            Add("D20A_BREAK_FREE", D20ActionType.BREAK_FREE);
            Add("D20A_TRIP", D20ActionType.TRIP);
            Add("D20A_REMOVE_DISEASE", D20ActionType.REMOVE_DISEASE);
            Add("D20A_ITEM_CREATION", D20ActionType.ITEM_CREATION);
            Add("D20A_WHOLENESS_OF_BODY_SET", D20ActionType.WHOLENESS_OF_BODY_SET);
            Add("D20A_USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL", D20ActionType.USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL);
            Add("D20A_TRACK", D20ActionType.TRACK);
            Add("D20A_ACTIVATE_DEVICE_STANDARD", D20ActionType.ACTIVATE_DEVICE_STANDARD);
            Add("D20A_SPELL_CALL_LIGHTNING", D20ActionType.SPELL_CALL_LIGHTNING);
            Add("D20A_AOO_MOVEMENT", D20ActionType.AOO_MOVEMENT);
            Add("D20A_CLASS_ABILITY_SA", D20ActionType.CLASS_ABILITY_SA);
            Add("D20A_ACTIVATE_DEVICE_FREE", D20ActionType.ACTIVATE_DEVICE_FREE);
            Add("D20A_OPEN_INVENTORY", D20ActionType.OPEN_INVENTORY);
            Add("D20A_ACTIVATE_DEVICE_SPELL", D20ActionType.ACTIVATE_DEVICE_SPELL);
            Add("D20A_DISABLE_DEVICE", D20ActionType.DISABLE_DEVICE);
            Add("D20A_SEARCH", D20ActionType.SEARCH);
            Add("D20A_SNEAK", D20ActionType.SNEAK);
            Add("D20A_TALK", D20ActionType.TALK);
            Add("D20A_OPEN_LOCK", D20ActionType.OPEN_LOCK);
            Add("D20A_SLEIGHT_OF_HAND", D20ActionType.SLEIGHT_OF_HAND);
            Add("D20A_OPEN_CONTAINER", D20ActionType.OPEN_CONTAINER);
            Add("D20A_THROW", D20ActionType.THROW);
            Add("D20A_THROW_GRENADE", D20ActionType.THROW_GRENADE);
            Add("D20A_FEINT",
                D20ActionType.FEINT); // note: this was missing in the ToEE dll strings hence the offset from here on
            Add("D20A_READY_SPELL", D20ActionType.READY_SPELL);
            Add("D20A_READY_COUNTERSPELL", D20ActionType.READY_COUNTERSPELL);
            Add("D20A_READY_ENTER", D20ActionType.READY_ENTER);
            Add("D20A_READY_EXIT", D20ActionType.READY_EXIT);
            Add("D20A_COPY_SCROLL", D20ActionType.COPY_SCROLL);
            Add("D20A_READIED_INTERRUPT", D20ActionType.READIED_INTERRUPT);
            Add("D20A_LAY_ON_HANDS_USE", D20ActionType.LAY_ON_HANDS_USE);
            Add("D20A_WHOLENESS_OF_BODY_USE", D20ActionType.WHOLENESS_OF_BODY_USE);
            Add("D20A_DISMISS_SPELLS", D20ActionType.DISMISS_SPELLS);
            Add("D20A_FLEE_COMBAT", D20ActionType.FLEE_COMBAT);
            Add("D20A_USE_POTION", D20ActionType.USE_POTION);
            Add("D20A_DIVINE_MIGHT", D20ActionType.DIVINE_MIGHT); // new for Temple+ from here on
            Add("D20A_DISARM", D20ActionType.DISARM);
            Add("D20A_SUNDER", D20ActionType.SUNDER);
            Add("D20A_BULLRUSH", D20ActionType.BULLRUSH);
            Add("D20A_TRAMPLE", D20ActionType.TRAMPLE);
            Add("D20A_GRAPPLE", D20ActionType.GRAPPLE);
            Add("D20A_PIN", D20ActionType.PIN);
            Add("D20A_OVERRUN", D20ActionType.OVERRUN);
            Add("D20A_SHIELD_BASH", D20ActionType.SHIELD_BASH);
            Add("D20A_DISARMED_WEAPON_RETRIEVE", D20ActionType.DISARMED_WEAPON_RETRIEVE);
            Add("D20A_AID_ANOTHER_WAKE_UP", D20ActionType.AID_ANOTHER_WAKE_UP);
            Add("D20A_EMPTY_BODY", D20ActionType.EMPTY_BODY);
            Add("D20A_QUIVERING_PALM", D20ActionType.QUIVERING_PALM);
            Add("D20A_PYTHON_ACTION", D20ActionType.PYTHON_ACTION); // will fetch the action from the python API
        }

        private static void AddResurrectionTypes()
        {
            Add("CRITTER_R_RAISE_DEAD", ResurrectionType.RaiseDead);
            Add("CRITTER_R_RESURRECT", ResurrectionType.Resurrection);
            Add("CRITTER_R_RESURRECT_TRUE", ResurrectionType.TrueResurrection);
            Add("CRITTER_R_CUTHBERT_RESURRECT", ResurrectionType.CuthbertResurrection);
        }

        private static void AddFrogAnimCallbacks()
        {
            Add("ANIM_CALLBACK_FROG_FAILED_LATCH", FrogGrapplePhase.FailedLatch);
            Add("ANIM_CALLBACK_FROG_LATCH", FrogGrapplePhase.Latch);
            Add("ANIM_CALLBACK_FROG_PULL", FrogGrapplePhase.Pull);
            Add("ANIM_CALLBACK_FROG_SWALLOW", FrogGrapplePhase.Swallow);
        }

        private static void AddObjectTypes()
        {
            Add("obj_t_ammo", ObjectType.ammo);
            Add("obj_t_armor", ObjectType.armor);
            Add("obj_t_bag", ObjectType.bag);
            Add("obj_t_container", ObjectType.container);
            Add("obj_t_food", ObjectType.food);
            Add("obj_t_generic", ObjectType.generic);
            Add("obj_t_key", ObjectType.key);
            Add("obj_t_money", ObjectType.money);
            Add("obj_t_npc", ObjectType.npc);
            Add("obj_t_pc", ObjectType.pc);
            Add("obj_t_portal", ObjectType.portal);
            Add("obj_t_projectile", ObjectType.projectile);
            Add("obj_t_scenery", ObjectType.scenery);
            Add("obj_t_scroll", ObjectType.scroll);
            Add("obj_t_trap", ObjectType.trap);
            Add("obj_t_weapon", ObjectType.weapon);
            Add("obj_t_written", ObjectType.written);
        }

        private static void AddSpellSchools()
        {
            Add("Abjuration", SchoolOfMagic.Abjuration);
            Add("Conjuration", SchoolOfMagic.Conjuration);
            Add("Divination", SchoolOfMagic.Divination);
            Add("Enchantment", SchoolOfMagic.Enchantment);
            Add("Evocation", SchoolOfMagic.Evocation);
            Add("Illusion", SchoolOfMagic.Illusion);
            Add("Necromancy", SchoolOfMagic.Necromancy);
            Add("Transmutation", SchoolOfMagic.Transmutation);
        }

        private static void AddAlignment()
        {
            Add("TRUE_NEUTRAL", Alignment.TRUE_NEUTRAL);
            Add("LAWFUL_NEUTRAL", Alignment.LAWFUL_NEUTRAL);
            Add("CHAOTIC_NEUTRAL", Alignment.CHAOTIC_NEUTRAL);
            Add("NEUTRAL_GOOD", Alignment.NEUTRAL_GOOD);
            Add("LAWFUL_GOOD", Alignment.LAWFUL_GOOD);
            Add("CHAOTIC_GOOD", Alignment.CHAOTIC_GOOD);
            Add("NEUTRAL_EVIL", Alignment.NEUTRAL_EVIL);
            Add("LAWFUL_EVIL", Alignment.LAWFUL_EVIL);
            Add("CHAOTIC_EVIL", Alignment.CHAOTIC_EVIL);

            Add("ALIGNMENT_NEUTRAL", Alignment.NEUTRAL);
            Add("ALIGNMENT_TRUE_NEUTRAL", Alignment.TRUE_NEUTRAL);
            Add("ALIGNMENT_LAWFUL_NEUTRAL", Alignment.LAWFUL_NEUTRAL);
            Add("ALIGNMENT_LAWFUL", Alignment.LAWFUL);
            Add("ALIGNMENT_CHAOTIC_NEUTRAL", Alignment.CHAOTIC_NEUTRAL);
            Add("ALIGNMENT_CHAOTIC", Alignment.CHAOTIC);
            Add("ALIGNMENT_NEUTRAL_GOOD", Alignment.NEUTRAL_GOOD);
            Add("ALIGNMENT_GOOD", Alignment.GOOD);
            Add("ALIGNMENT_LAWFUL_GOOD", Alignment.LAWFUL_GOOD);
            Add("ALIGNMENT_CHAOTIC_GOOD", Alignment.CHAOTIC_GOOD);
            Add("ALIGNMENT_NEUTRAL_EVIL", Alignment.NEUTRAL_EVIL);
            Add("ALIGNMENT_EVIL", Alignment.EVIL);
            Add("ALIGNMENT_LAWFUL_EVIL", Alignment.LAWFUL_EVIL);
            Add("ALIGNMENT_CHAOTIC_EVIL", Alignment.CHAOTIC_EVIL);
        }

        private static void AddGenders()
        {
            Add("gender_female", Gender.Female);
            Add("gender_male", Gender.Male);
        }

        private static void AddSpellDescriptors()
        {
            Add("CHAOTIC", SpellDescriptor.CHAOTIC);
            Add("ACID", SpellDescriptor.ACID);
            Add("COLD", SpellDescriptor.COLD);
            Add("DARKNESS", SpellDescriptor.DARKNESS);
            Add("DEATH", SpellDescriptor.DEATH);
            Add("ELECTRICITY", SpellDescriptor.ELECTRICITY);
            Add("EVIL", SpellDescriptor.EVIL);
            Add("FEAR", SpellDescriptor.FEAR);
            Add("FIRE", SpellDescriptor.FIRE);
            Add("FORCE", SpellDescriptor.FORCE);
            Add("GOOD", SpellDescriptor.GOOD);
            Add("LANGUAGE", SpellDescriptor.LANGUAGE_DEPENDENT);
            Add("LAWFUL", SpellDescriptor.LAWFUL);
            Add("LIGHT", SpellDescriptor.LIGHT);
            Add("MIND", SpellDescriptor.MIND_AFFECTING);
            Add("SONIC", SpellDescriptor.SONIC);
            Add("TELEPORTATION", SpellDescriptor.TELEPORTATION);
            Add("AIR", SpellDescriptor.AIR);
            Add("EARTH", SpellDescriptor.EARTH);
            Add("WATER", SpellDescriptor.WATER);
        }

        private static void AddClericDomains()
        {
            Add("none", DomainId.None);
            Add("air", DomainId.Air);
            Add("animal", DomainId.Animal);
            Add("chaos", DomainId.Chaos);
            Add("death", DomainId.Death);
            Add("destruction", DomainId.Destruction);
            Add("earth", DomainId.Earth);
            Add("evil", DomainId.Evil);
            Add("fire", DomainId.Fire);
            Add("good", DomainId.Good);
            Add("healing", DomainId.Healing);
            Add("knowledge", DomainId.Knowledge);
            Add("law", DomainId.Law);
            Add("luck", DomainId.Luck);
            Add("magic", DomainId.Magic);
            Add("plant", DomainId.Plant);
            Add("protection", DomainId.Protection);
            Add("strength", DomainId.Strength);
            Add("sun", DomainId.Sun);
            Add("travel", DomainId.Travel);
            Add("trickery", DomainId.Trickery);
            Add("war", DomainId.War);
            Add("water", DomainId.Water);
            Add("special", DomainId.Special);
            Add("domain_special", DomainId.Special);
        }

        private static void Add<T>(string name, T value)
        {
            var text = typeof(T).Name + '.' + typeof(T).GetEnumName(value);
            Constants.Add(name, text);
            ConstantTypes.Add(name, TypeMapping.FromManagedType(typeof(T)));
        }

        private static void AddRandomEncounterSetupFlags()
        {
            Add("ES_F_SLEEP_ENCOUNTER", RandomEncounterType.Resting);
        }
    }
}