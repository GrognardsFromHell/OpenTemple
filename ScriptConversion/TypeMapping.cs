using System;
using System.Collections.Generic;
using IronPython.Compiler.Ast;
using IronPython.Modules;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Utils;

namespace ScriptConversion
{
    static class TypeMapping
    {
        private static readonly Dictionary<Type, GuessedType> NativeGuessedTypeMapping =
            new Dictionary<Type, GuessedType>
            {
                {typeof(GameObjectBody), GuessedType.Object},
                {typeof(TrapSprungEvent), GuessedType.TrapSprungEvent},
                {typeof(SpellPacketBody), GuessedType.Spell},
                {typeof(List<GameObjectBody>), GuessedType.ObjectList},
                {typeof(SpellTarget[]), GuessedType.SpellTargets},
                {typeof(bool), GuessedType.Bool},
                {typeof(int), GuessedType.Integer},
                {typeof(float), GuessedType.Float},
                {typeof(string), GuessedType.String},
                {typeof(FeatId), GuessedType.FeatId},
                {typeof(QuestState), GuessedType.QuestState},
                {typeof(ObjectType), GuessedType.ObjectType},
                {typeof(locXY), GuessedType.Location},
                {typeof(LocAndOffsets), GuessedType.LocationFull},
                {typeof(TimePoint), GuessedType.Time},
                {typeof(Stat), GuessedType.Stat},
                {typeof(Dice), GuessedType.Dice},
                {typeof(SpellTarget), GuessedType.SpellTarget},
                {typeof(Alignment), GuessedType.Alignment},
                {typeof(Gender), GuessedType.Gender},
                {typeof(SpellDescriptor), GuessedType.SpellDescriptor},
                {typeof(RaceId), GuessedType.Race},
                {typeof(D20CAF), GuessedType.D20CAF},
                {typeof(Material), GuessedType.Material},
                {typeof(LootSharingType), GuessedType.LootSharingType},
                {typeof(MapTerrain), GuessedType.MapTerrain},
                {typeof(TutorialTopic), GuessedType.TutorialTopic},
                {typeof(D20DispatcherKey), GuessedType.DispatcherKey},
                {typeof(CritterFlag), GuessedType.CritterFlag},
                {typeof(SizeCategory), GuessedType.SizeCategory},
                {typeof(DomainId), GuessedType.Domain},
                {typeof(SchoolOfMagic), GuessedType.SchoolOfMagic},
                {typeof(FrogGrapplePhase), GuessedType.FrogGrapplePhase},
                {typeof(ResurrectionType), GuessedType.ResurrectionType},
                {typeof(D20ActionType), GuessedType.D20ActionType},
                {typeof(D20AttackPower), GuessedType.D20AttackPower},
                {typeof(DamageType), GuessedType.DamageType},
                {typeof(D20SavingThrowFlag), GuessedType.D20SavingThrowFlag},
                {typeof(SavingThrowType), GuessedType.SavingThrowType},
                {typeof(D20SavingThrowReduction), GuessedType.D20SavingThrowReduction},
                {typeof(FadeOutResult), GuessedType.FadeOutResult},
                {typeof(ObjectFlag), GuessedType.ObjectFlag},
                {typeof(ObjectListFilter), GuessedType.ObjectListFilter},
                {typeof(WeaponFlag), GuessedType.WeaponFlag},
                {typeof(RadialMenuParam), GuessedType.RadialMenuParam},
                {typeof(SleepStatus), GuessedType.SleepStatus},
                {typeof(TargetListOrder), GuessedType.TargetListOrder},
                {typeof(TargetListOrderDirection), GuessedType.TargetListOrderDirection},
                {typeof(StandPointType), GuessedType.StandPointType},
                {typeof(MonsterCategory), GuessedType.MonsterCategory},
                {typeof(MonsterSubtype), GuessedType.MonsterSubtype},
                {typeof(DeityId), GuessedType.Deity},
                {typeof(SkillId), GuessedType.Skill},
                {typeof(ObjScriptEvent), GuessedType.ObjScriptEvent},
                {typeof(ContainerFlag), GuessedType.ContainerFlag},
                {typeof(ItemFlag), GuessedType.ItemFlag},
                {typeof(PortalFlag), GuessedType.PortalFlag},
                {typeof(obj_f), GuessedType.ObjectField},
                {typeof(NpcFlag), GuessedType.NpcFlag},
                {typeof(EquipSlot), GuessedType.EquipSlot},
                {typeof(RandomEncounter), GuessedType.RandomEncounter},
                {typeof(RandomEncounterQuery), GuessedType.RandomEncounterQuery},
                {typeof(RandomEncounterType), GuessedType.RandomEncounterType},
                {typeof(TextFloaterColor), GuessedType.TextFloaterColor},
                {typeof(Co8SpellFlag), GuessedType.Co8SpellFlag},
                {typeof(void), GuessedType.Void}
            };

        /// <summary>
        /// For these types typeof(T).Name is not accurate to what we want.
        /// I.e. typeof(int).Name == "Int32"!
        /// </summary>
        private static readonly Dictionary<Type, string> SpecialManagedTypeNames = new Dictionary<Type, string>
        {
            {typeof(List<GameObjectBody>), "List<" + typeof(GameObjectBody).Name + ">"},
            {typeof(void), "void"},
            {typeof(bool), "bool"},
            {typeof(int), "int"},
            {typeof(float), "float"},
            {typeof(string), "string"},
        };

        public static GuessedType GuessTypeFromName(string name, ScriptType scriptType)
        {
            if (name.StartsWith("obj_"))
            {
                return GuessedType.Object;
            }

            switch (name)
            {
                case "x":
                case "y":
                    return GuessedType.Integer;
                case "setup":
                    return GuessedType.RandomEncounterQuery;
                case "encounter":
                    return GuessedType.RandomEncounter;
                case "spell":
                    return GuessedType.Spell;
                case "dc":
                case "radius":
                    return GuessedType.Integer;
                case "loc":
                    return GuessedType.Location;
                case "target":
                    if (scriptType == ScriptType.Spell)
                    {
                        return GuessedType.SpellTarget;
                    }
                    else
                    {
                        return GuessedType.Object;
                    }
                case "speaker":
                case "listener":
                case "attachee":
                case "triggerer":
                case "pc":
                case "critter":
                case "talker":
                case "seeker":
                case "npc":
                case "gremag":
                case "jaroo":
                case "obj":
                case "iuz":
                case "cuthbert":
                case "darley":
                case "zuggtmoy":
                case "sfx":
                    return GuessedType.Object;
                case "line":
                    return GuessedType.Integer;
            }

            if (name.EndsWith("_num") || name.Contains("number") || name.Contains("line"))
            {
                return GuessedType.Integer;
            }

            return GuessedType.Unknown;
        }

        public static GuessedType FromManagedType(Type t)
        {
            if (typeof(IEnumerable<GameObjectBody>).IsAssignableFrom(t))
            {
                return GuessedType.ObjectList;
            }

            if (NativeGuessedTypeMapping.TryGetValue(t, out var guessedType))
            {
                return guessedType;
            }

            Console.WriteLine("Don't know how to map managed type " + t.Name);
            return GuessedType.Unknown;
        }

        public static string GuessManagedType(GuessedType? type)
        {
            if (!type.HasValue)
            {
                return "void";
            }

            foreach (var kvp in NativeGuessedTypeMapping)
            {
                if (kvp.Value == type.Value)
                {
                    if (SpecialManagedTypeNames.TryGetValue(kvp.Key, out var specialName))
                    {
                        return specialName;
                    }

                    return kvp.Key.Name;
                }
            }

            if (type == GuessedType.Unknown)
            {
                return "FIXME";
            }
            if (type == GuessedType.UnknownList)
            {
                return "List<FIXME>";
            }
            if (type == GuessedType.IntList)
            {
                return "IList<int>";
            }

            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}