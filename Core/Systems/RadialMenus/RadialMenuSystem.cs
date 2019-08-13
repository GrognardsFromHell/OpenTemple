using System;
using System.Collections;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Classes;
using static System.String;

namespace SpicyTemple.Core.Systems.RadialMenus
{
    internal struct D20RadialMenuDef
    {
        public RadialMenuStandardNode parent;
        public D20ActionType d20ActionType;
        public int d20ActionData1;
        public int combatMesLineIdx;
        public string helpSystemEntryName;
        public RadialMenuEntryCallback callback;
    }

    public class RadialMenuSystem
    {
        private const int NUM_SPELL_LEVELS = 10; // spells are levels 0-9
        private const int NUM_SPELL_LEVELS_VANILLA = 6; // 0-5

        [TempleDllLocation(0x115B2048)]
        private RadialMenu activeRadialMenu;

        [TempleDllLocation(0x115B204C)]
        private int activeRadialMenuNode;

        [TempleDllLocation(0x11E76CE4)]
        private readonly int[] standardNodeIndices = new int[200]; // was 120 in Co8

        [TempleDllLocation(0x115B2060)]
        private readonly List<RadialMenu> _radialMenus = new List<RadialMenu>();

        [TempleDllLocation(0x10BD0234)]
        [TempleDllLocation(0x100f0100)]
        public bool ShiftPressed { get; set; }

        [TempleDllLocation(0x102E8738)]
        private readonly List<D20RadialMenuDef> _tactialOptions;

        public RadialMenuSystem()
        {
            _tactialOptions = CreateTacticalOptions();
        }

        private List<D20RadialMenuDef> CreateTacticalOptions()
        {
            return new List<D20RadialMenuDef>
            {
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Offense,
                    d20ActionType = D20ActionType.STANDARD_ATTACK,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5000,
                    helpSystemEntryName = "TAG_MELEE_ATTACKS",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Offense,
                    d20ActionType = D20ActionType.FULL_ATTACK,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5001,
                    helpSystemEntryName = "TAG_FULL_ATTACK",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Movement,
                    d20ActionType = D20ActionType.FIVEFOOTSTEP,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5002,
                    helpSystemEntryName = "TAG_5_FOOT_STEP",
                    callback = RadialCallbackMovementMaybe
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Movement,
                    d20ActionType = D20ActionType.MOVE,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5003,
                    helpSystemEntryName = "TAG_ADVENTURING_MOVE",
                    callback = RadialCallbackMovementMaybe
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Movement,
                    d20ActionType = D20ActionType.DOUBLE_MOVE,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5004,
                    helpSystemEntryName = "TAG_ADVENTURING_DOUBLE_MOVE",
                    callback = RadialCallbackMovementMaybe
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Movement,
                    d20ActionType = D20ActionType.RUN,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5005,
                    helpSystemEntryName = "TAG_RUN",
                    callback = RadialCallbackMovementMaybe
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Movement,
                    d20ActionType = D20ActionType.FLEE_COMBAT,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5102,
                    helpSystemEntryName = "TAG_FLEE_COMBAT",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Skills,
                    d20ActionType = D20ActionType.HEAL,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5008,
                    helpSystemEntryName = "TAG_HEAL",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Tactical,
                    d20ActionType = D20ActionType.TOTAL_DEFENSE,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5016,
                    helpSystemEntryName = "TAG_RADIAL_MENU_TOTAL_DEFENSE",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Offense,
                    d20ActionType = D20ActionType.CHARGE,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5017,
                    helpSystemEntryName = "TAG_CHARGE",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Offense,
                    d20ActionType = D20ActionType.TRIP,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5062,
                    helpSystemEntryName = "TAG_TRIP",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Offense,
                    d20ActionType = D20ActionType.COUP_DE_GRACE,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5037,
                    helpSystemEntryName = "TAG_RADIAL_MENU_COUP_DE_GRACE",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Items,
                    d20ActionType = D20ActionType.OPEN_INVENTORY,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5079,
                    helpSystemEntryName = "TAG_HMU_CHAR_INVENTORY_UI",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Class,
                    d20ActionType = D20ActionType.TALK,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5085,
                    helpSystemEntryName = "TAG_RADIAL_MENU_TALK",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Tactical,
                    d20ActionType = D20ActionType.FEINT,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5089,
                    helpSystemEntryName = "TAG_FEINT",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Tactical,
                    d20ActionType = D20ActionType.READY_SPELL,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5090,
                    helpSystemEntryName = "TAG_RADIAL_MENU_READY",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Tactical,
                    d20ActionType = D20ActionType.READY_ENTER,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5092,
                    helpSystemEntryName = "TAG_RADIAL_MENU_READY_APPROACH",
                    callback = RadialMenuCallbackDefault
                },
                new D20RadialMenuDef
                {
                    parent = RadialMenuStandardNode.Tactical,
                    d20ActionType = D20ActionType.READY_EXIT,
                    d20ActionData1 = 0,
                    combatMesLineIdx = 5093,
                    helpSystemEntryName = "TAG_RADIAL_MENU_READY_WITHDRAWL",
                    callback = RadialMenuCallbackDefault
                },
            };
        }

        [TempleDllLocation(0x100f0110)]
        private bool RadialMenuCallbackDefault(GameObjectBody obj, ref RadialMenuEntry radMenuEntry)
        {
            GameSystems.D20.Actions.TurnBasedStatusInit(obj);
            GameSystems.D20.Actions.SequenceSwitch(obj);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(radMenuEntry.d20ActionType, radMenuEntry.d20ActionData1);

            var entryType = radMenuEntry.type;
            if (entryType == RadialMenuEntryType.Slider
                || entryType == RadialMenuEntryType.Toggle
                || entryType == RadialMenuEntryType.Choice)
            {
                GameSystems.D20.Actions.GlobD20aSetActualArg(radMenuEntry.actualArg);
            }

            GameSystems.D20.Actions.GlobD20ActnSetD20CAF(radMenuEntry.d20Caf);
            GameSystems.D20.Actions.GlobD20ActnSetSpellData(radMenuEntry.d20SpellData);
            ClearActiveRadialMenu();
            return true;
        }

        [TempleDllLocation(0x100f01b0)]
        private bool RadialCallbackMovementMaybe(GameObjectBody critter, ref RadialMenuEntry entry)
        {
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(entry.d20ActionType, entry.d20ActionData1);
            GameSystems.D20.Actions.RadialMenuSetSeqPicker(entry.d20ActionType, entry.d20ActionData1,
                D20TargetClassification.Movement);
            GameSystems.D20.Actions.GlobD20ActnSetD20CAF(entry.d20Caf);
            ClearActiveRadialMenu();
            return false;
        }

        [TempleDllLocation(0x100f0a70)]
        public void SortRadialMenu(GameObjectBody obj)
        {
            GetRadialMenu(obj).Sort();
        }

        [TempleDllLocation(0x100f0050)]
        public int GetCurrentNode()
        {
            if (activeRadialMenu != null)
            {
                return activeRadialMenuNode;
            }

            return -1;
        }

        [TempleDllLocation(0x100f12b0)]
        public int GetStandardNode(RadialMenuStandardNode standardNode)
        {
            return standardNodeIndices[(int) standardNode];
        }

        [TempleDllLocation(0x100f0670)]
        public int AddChildNode(GameObjectBody critter, ref RadialMenuEntry entry, int parentIdx)
        {
            var radMenu = GetRadialMenu(critter);

            var node = new RadialMenuNode();
            radMenu.nodes.Add(node);
            var index = radMenu.nodes.Count - 1;
            node.entry = entry;
            node.parent = parentIdx;
            node.morphsTo = -1;

            var parentNode = radMenu.nodes[parentIdx];
            // TODO: Remove node.entry.textHash = conds.hashmethods.StringHash(radialMenuEntry.text);
            parentNode.children.Add(index);
            return index;
        }

        [TempleDllLocation(0x100f0d10)]
        public int AddParentChildNode(GameObjectBody critter, ref RadialMenuEntry entry, int parentIdx)
        {
            var radMenu = GetRadialMenu(critter);
            var node = new RadialMenuNode();
            radMenu.nodes.Add(node);
            var index = radMenu.nodes.Count - 1;
            node.parent = parentIdx;
            node.morphsTo = -1;

            node.entry = entry;
            node.entry.d20ActionType = D20ActionType.NONE;
            node.entry.type = RadialMenuEntryType.Parent;

            // TODO  (remove) node.entry.textHash = ElfHash.Hash(radialMenuEntry.text);
            if (parentIdx != -1)
            {
                var parentNode = radMenu.nodes[parentIdx];
                parentNode.children.Add(index);
            }

            return index;
        }

        public int AddToStandardNode(GameObjectBody handle, ref RadialMenuEntry entry,
            RadialMenuStandardNode standardNode)
        {
            var node = GetStandardNode(RadialMenuStandardNode.Class);
            return AddParentChildNode(handle, ref entry, node);
        }

        [TempleDllLocation(0x100eff60)]
        public void ClearActiveRadialMenu()
        {
            activeRadialMenu = null;
            activeRadialMenuNode = -1;
        }

        [TempleDllLocation(0x100f0200)]
        public int RadialMenuCheckboxOrSliderCallback(GameObjectBody obj, ref RadialMenuEntry radEntry)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100f2650)]
        [TemplePlusLocation("radialmenu.cpp:110")]
        public void BuildStandardRadialMenu(GameObjectBody critter)
        {
            AssignMenu(critter);
            standardNodeIndices[0] = 0; // root
            // set the main 6 nodes: Spells, Skills, Feats, Class, Combat, Items
            for (var i = RadialMenuStandardNode.Spells; i <= RadialMenuStandardNode.Items; i++)
            {
                SetStandardNode(critter, i, RadialMenuStandardNode.Root);
            }

            // Set the combat nodes
            for (var i = RadialMenuStandardNode.Movement; i <= RadialMenuStandardNode.Options; i++)
            {
                SetStandardNode(critter, i, RadialMenuStandardNode.Combat);
            }

            // Set the magic item nodes: potions, wands, scrolls
            for (var i = RadialMenuStandardNode.Potions; i <= RadialMenuStandardNode.Scrolls; i++)
            {
                SetStandardNode(critter, i, RadialMenuStandardNode.Items);
            }


            // Set spell nodes (classes & spell numbers)
            SetStandardNode(critter, RadialMenuStandardNode.CopyScroll, RadialMenuStandardNode.Class);

            for (var i = RadialMenuStandardNode.SpellsWizard; i <= RadialMenuStandardNode.SpellsDomain; i++)
            {
                SetStandardNode(critter, i, RadialMenuStandardNode.Spells);
            }

            for (var i = RadialMenuStandardNode.SpellsWizard; i <= RadialMenuStandardNode.SpellsDomain; i++)
            {
                for (var j = 24 + (i - RadialMenuStandardNode.SpellsWizard) * 10;
                    j < 34 + (i - RadialMenuStandardNode.SpellsWizard) * 10;
                    j++)
                {
                    SetStandardNode(critter, (RadialMenuStandardNode) j, i);
                }
            }

            // add the tactical options
            foreach (var def in _tactialOptions)
            {
                var radEntry = RadialMenuEntry.CreateAction(def.combatMesLineIdx, def.d20ActionType, def.d20ActionData1,
                    def.helpSystemEntryName);
                radEntry.callback = def.callback;
                radEntry.AddAsChild(critter, GetStandardNode(def.parent));
            }

            // Decipher Script
            var umdSkill = GameSystems.Skill.GetSkillRanks(critter, SkillId.use_magic_device);
            if (umdSkill != 0)
            {
                var radEntry = RadialMenuEntry.CreateAction(5073, D20ActionType.USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL,
                    0, "TAG_UMD");
                radEntry.AddAsChild(critter, GetStandardNode(RadialMenuStandardNode.Skills));
            }

            // Disable Device
            if (GameSystems.Skill.GetSkillRanks(critter, SkillId.disable_device) != 0)
            {
                var radEntry =
                    RadialMenuEntry.CreateAction(5080, D20ActionType.DISABLE_DEVICE, 0, "TAG_DISABLE_DEVICE");
                radEntry.AddAsChild(critter, GetStandardNode(RadialMenuStandardNode.Skills));
            }

            // Open Lock
            if (GameSystems.Skill.GetSkillRanks(critter, SkillId.open_lock) != 0)
            {
                var radEntry = RadialMenuEntry.CreateAction(5086, D20ActionType.OPEN_LOCK, 0, "TAG_OPEN_LOCK");
                radEntry.AddAsChild(critter, GetStandardNode(RadialMenuStandardNode.Skills));
            }

            // Open Lock
            if (GameSystems.Skill.GetSkillRanks(critter, SkillId.pick_pocket) != 0)
            {
                var radEntry =
                    RadialMenuEntry.CreateAction(5087, D20ActionType.SLEIGHT_OF_HAND, 0, "TAG_SLEIGHT_OF_HAND");
                radEntry.AddAsChild(critter, GetStandardNode(RadialMenuStandardNode.Skills));
            }

            // Sneak
            var sneakMesLine = GameSystems.Critter.IsMovingSilently(critter) ? 5083 : 5082;
            var sneakRadEntry =
                RadialMenuEntry.CreateAction(sneakMesLine, D20ActionType.SNEAK, 0, "TAG_RADIAL_MENU_SNEAK");
            sneakRadEntry.AddAsChild(critter, GetStandardNode(RadialMenuStandardNode.Skills));

            // Search
            if (!GameSystems.Combat.IsCombatActive())
            {
                var radEntry = RadialMenuEntry.CreateAction(5081, D20ActionType.SEARCH, 0, "TAG_SEARCH");
                radEntry.AddAsChild(critter, GetStandardNode(RadialMenuStandardNode.Skills));
            }

            // Spells
            var obj = critter;
            var numKnown = obj.GetArrayLength(obj_f.critter_spells_known_idx);
            var numMem = obj.GetArrayLength(obj_f.critter_spells_memorized_idx);
            if (numKnown > 0 || numMem > 0)
            {
                var radEntry = RadialMenuEntry.CreateAction(5091, D20ActionType.READY_COUNTERSPELL, 0,
                    "TAG_RADIAL_MENU_READY_COUNTERSPELL");
                radEntry.AddAsChild(critter, GetStandardNode(RadialMenuStandardNode.Tactical));
            }

            for (var i = 0; i < numMem; i++)
            {
                var spData = obj.GetSpell(obj_f.critter_spells_memorized_idx, i);
                if (GameSystems.Spell.IsDomainSpell(spData.classCode)
                    || D20ClassSystem.IsVancianCastingClass(GameSystems.Spell.GetCastingClass(spData.classCode)))
                {
                    AddSpell(critter, spData, out _, out _);
                }
            }

            // Spells Known
            for (var i = 0; i < numKnown; i++)
            {
                var spData = obj.GetSpell(obj_f.critter_spells_known_idx, i);
                if (!GameSystems.Spell.IsDomainSpell(spData.classCode)
                    && D20ClassSystem.IsNaturalCastingClass(GameSystems.Spell.GetCastingClass(spData.classCode)))
                {
                    AddSpell(critter, spData, out _, out _);
                }
            }

            // dismiss spells
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Can_Dismiss_Spells))
            {
                var dism = RadialMenuEntry.CreateParent(5101);
                SetStandardNode(critter, RadialMenuStandardNode.SpellsDismiss, RadialMenuStandardNode.Spells);
            }
        }

        [TempleDllLocation(0x100f0ee0)]
        private void AssignMenu(GameObjectBody critter)
        {
            var radialMenu = GetRadialMenu(critter);
            if (radialMenu != null)
            {
                radialMenu.nodes.Clear();
            }
            else
            {
                radialMenu = new RadialMenu(critter);
                _radialMenus.Add(radialMenu);
            }

            var rootEntry = new RadialMenuEntry {text = "ROOT"};
            AddParentChildNode(critter, ref rootEntry, -1);
        }

        private void SetStandardNode(GameObjectBody handle, RadialMenuStandardNode stdNode,
            RadialMenuStandardNode specialParent)
        {
            var meskey = 1000 + (int) stdNode;
            var isSpellNode = false;
            var isVanillaNode = false;

            if (stdNode == RadialMenuStandardNode.SpellsDismiss)
            {
                meskey = 5101;
            }
            else if (stdNode > RadialMenuStandardNode.SpellsDomain && stdNode <= (RadialMenuStandardNode) 104)
            {
                isSpellNode = true;

                if (specialParent == RadialMenuStandardNode.SpellsSorcerer &&
                    stdNode < (RadialMenuStandardNode) 34
                    || specialParent == RadialMenuStandardNode.SpellsBard &&
                    stdNode < (RadialMenuStandardNode) 44)
                    isVanillaNode = true;

                if (isVanillaNode)
                    meskey = (int) (stdNode - 24) % NUM_SPELL_LEVELS_VANILLA + 1024;
                else
                    meskey = (int) (stdNode - 24) % NUM_SPELL_LEVELS + 1024;
            }

            var radMenuEntry = RadialMenuEntry.CreateParent(meskey);

            // change name
            if (stdNode >= RadialMenuStandardNode.SpellsWizard && stdNode < RadialMenuStandardNode.SpellsDomain)
            {
                var spellClass = GetSpellClassFromSpecialNode(handle, stdNode);

                if (!GameSystems.Spell.IsDomainSpell(spellClass))
                {
                    var classEnum = GameSystems.Spell.GetCastingClass(spellClass);
                    radMenuEntry.text = GameSystems.Stat.GetStatShortName(classEnum);
                }
                else
                {
                    radMenuEntry.text =
                        GameSystems.D20.Combat.GetCombatMesLine((int) (1000 + RadialMenuStandardNode.SpellsDomain));
                }
            }
            // Set min/max for Natural Casting
            else if (isSpellNode)
            {
                var nodeSpellClass = GetSpellClassFromSpecialNode(handle, specialParent);
                if (!GameSystems.Spell.IsDomainSpell(nodeSpellClass))
                {
                    var classCode = GameSystems.Spell.GetCastingClass(nodeSpellClass);
                    var isNaturalCasting = D20ClassSystem.IsNaturalCastingClass(classCode);

                    if (isNaturalCasting)
                    {
                        radMenuEntry.flags |= 6; // draw min/max arg
                        var spLvl = (int) (stdNode - 24) % NUM_SPELL_LEVELS;

                        var spellClass = GameSystems.Spell.GetSpellClass(classCode);
                        var numSpellsPerDay = GameSystems.Spell.GetNumSpellsPerDay(handle, classCode, spLvl);
                        if (numSpellsPerDay < 0)
                            numSpellsPerDay = 0;

                        radMenuEntry.maxArg = numSpellsPerDay;

                        var numSpellsCast = GameSystems.Spell.NumSpellsInLevel(handle, obj_f.critter_spells_cast_idx,
                            spellClass, spLvl);
                        if (numSpellsCast < numSpellsPerDay)
                            radMenuEntry.minArg = numSpellsPerDay - numSpellsCast;
                        else
                            radMenuEntry.minArg = 0;
                    }
                }
            }


            standardNodeIndices[(int) stdNode] =
                AddParentChildNode(handle, ref radMenuEntry, standardNodeIndices[(int) specialParent]);
        }

        private int GetSpellClassFromSpecialNode(GameObjectBody handle, RadialMenuStandardNode specialParent)
        {
            var spellClasses = new List<int>();
            foreach (var classEnum in D20ClassSystem.ClassesWithSpellLists)
            {
                if (handle.GetStat(classEnum) <= 0)
                    continue;

                spellClasses.Add(GameSystems.Spell.GetSpellClass(classEnum));
            }

            var idx = specialParent - RadialMenuStandardNode.SpellsWizard;
            if (idx < spellClasses.Count)
            {
                return spellClasses[specialParent - RadialMenuStandardNode.SpellsWizard];
            }

            return 0; // will register as domain spell
        }

        [TempleDllLocation(0x100f1470)]
        private void AddSpell(GameObjectBody objHnd, SpellStoreData spellData, out int specialNode_1,
            out RadialMenuEntry entry)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100f04d0)]
        public RadialMenu GetRadialMenu(GameObjectBody obj)
        {
            foreach (var radialMenu in _radialMenus)
            {
                if (radialMenu.obj == obj)
                {
                    return radialMenu;
                }
            }

            return null;
        }

        [TempleDllLocation(0x100f12c0)]
        public string GetAbilityReducedName(int statIdx)
        {
            throw new NotImplementedException();
        }
    }
}