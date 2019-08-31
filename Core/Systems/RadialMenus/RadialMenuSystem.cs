using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
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
        private static readonly ILogger Logger = new ConsoleLogger();

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

        [TempleDllLocation(0x10bd022c)]
        [TempleDllLocation(0x10bd0228)]
        [TempleDllLocation(0x100f0070)]
        [TempleDllLocation(0x100f0090)]
        public Vector2 ActiveMenuWorldPosition { get; set; }

        // Relative to ActiveMenuWorldPosition
        [TempleDllLocation(0x10be67bc)]
        public int RelativeMousePosX { get; set; }

        // Relative to ActiveMenuWorldPosition
        [TempleDllLocation(0x10be6238)]
        public int RelativeMousePosY { get; set; }

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
                if (radMenuEntry.HasArgument)
                {
                    GameSystems.D20.Actions.GlobD20aSetActualArg(radMenuEntry.ArgumentGetter());
                }
                else
                {
                    Logger.Warn("Radial menu entry {0} is of type {1} but has no argument.",
                        radMenuEntry.text, radMenuEntry.type);
                }
            }

            GameSystems.D20.Actions.GlobD20ActnSetD20CAF(radMenuEntry.d20Caf);
            if (radMenuEntry.d20SpellData != null)
            {
                GameSystems.D20.Actions.GlobD20ActnSetSpellData(radMenuEntry.d20SpellData);
            }

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
        public bool RadialMenuCheckboxOrSliderCallback(GameObjectBody obj, ref RadialMenuEntry radEntry)
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
                        // draw min/max arg
                        radMenuEntry.HasMinArg = true;
                        radMenuEntry.HasMaxArg = true;
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

        public bool ActivateEntry(GameObjectBody critter, RadialMenuEntry entry)
        {
            GameSystems.Critter.BuildRadialMenu(critter);

            var radialMenu = GetRadialMenu(critter);
            if (radialMenu == null)
            {
                return false;
            }

            for (var index = 0; index < radialMenu.nodes.Count; index++)
            {
                var node = radialMenu.nodes[index];
                if (node.entry.type != RadialMenuEntryType.Parent && HotkeyCompare(node.entry, entry))
                {
                    return ActivateNode(radialMenu, index);
                }
            }

            return false;
        }

        [TempleDllLocation(0x100f0b80)]
        [TemplePlusLocation("hotkeys.cpp:68")]
        private bool ActivateNode(RadialMenu radialMenu, int nodeIndex)
        {
            Trace.Assert(radialMenu != null);
            Trace.Assert(nodeIndex < radialMenu.nodes.Count);

            // Temporarily make the menu & node active
            activeRadialMenu = radialMenu;
            activeRadialMenuNode = nodeIndex;

            ref var radEntry = ref radialMenu.nodes[nodeIndex].entry;

            if (radEntry.d20ActionType == D20ActionType.CAST_SPELL)
                GameSystems.D20.Actions.ActSeqSpellReset();
            else if (radEntry.d20ActionType == D20ActionType.USE_ITEM && radEntry.d20SpellData.spellEnumOrg != 0)
            {
                GameSystems.D20.Actions.ActSeqSpellReset();
            }

            var nodeType = radEntry.type;
            var result = false;
            if (nodeType == RadialMenuEntryType.Action)
            {
                radEntry.callback?.Invoke(radialMenu.obj, ref radEntry);
                result = true;
            }
            else if (nodeType == RadialMenuEntryType.Slider) // will toggle between min/max values
            {
                // toggle value to min/max
                AutoActivateRadialMenuSlider(ref radEntry);
                // activate / deactivate float line
                ShowMessageAfterSilentToggle(radialMenu.obj, ref radEntry);
                result = true;
            }
            else if (nodeType == RadialMenuEntryType.Toggle)
            {
                radEntry.callback?.Invoke(radialMenu.obj, ref radEntry);
                // activate / deactivate float line
                ShowMessageAfterSilentToggle(radialMenu.obj, ref radEntry);
                result = true;
            }

            activeRadialMenu = null;
            activeRadialMenuNode = -1;
            return result;
        }

        [TempleDllLocation(0x100f05c0)]
        private void AutoActivateRadialMenuSlider(ref RadialMenuEntry radEntry)
        {
            if (radEntry.ArgumentGetter == null || radEntry.ArgumentSetter == null)
            {
                return;
            }

            var currentValue = radEntry.ArgumentGetter();
            if (currentValue == radEntry.minArg)
            {
                radEntry.ArgumentSetter(radEntry.maxArg);
            }
            else
            {
                radEntry.ArgumentSetter(radEntry.minArg);
            }
        }

        [TempleDllLocation(0x100f05f0)]
        private void ShowMessageAfterSilentToggle(GameObjectBody critter, ref RadialMenuEntry radEntry)
        {
            if (radEntry.HasArgument)
            {
                return;
            }

            var prefix = radEntry.text + " ";
            if (radEntry.ArgumentGetter() == radEntry.minArg)
            {
                GameSystems.D20.Combat.FloatCombatLine(critter, D20CombatMessage.deactivated, prefix);
            }
            else
            {
                GameSystems.D20.Combat.FloatCombatLine(critter, D20CombatMessage.activated, prefix);
            }
        }

        [TempleDllLocation(0x100f0380)]
        [TemplePlusLocation("hotkeys.cpp:67")]
        public static bool HotkeyCompare(RadialMenuEntry first, RadialMenuEntry second)
        {
            var actionType = first.d20ActionType;

            if (actionType != second.d20ActionType)
            {
                return false;
            }

            if (actionType == D20ActionType.ACTIVATE_DEVICE_FREE
                || actionType == D20ActionType.ACTIVATE_DEVICE_STANDARD
                || actionType == D20ActionType.ACTIVATE_DEVICE_SPELL)
                return first.text == second.text;

            if (actionType == D20ActionType.USE_ITEM)
            {
                if (first.d20SpellData.spellEnumOrg != second.d20SpellData.spellEnumOrg)
                    return false;

                if (first.d20SpellData.metaMagicData != second.d20SpellData.metaMagicData)
                    return false;
                //return first.textHash == second.textHash;
                return first.d20SpellData.spellSlotLevel == second.d20SpellData.spellSlotLevel;
            }

            if (first.d20ActionData1 != second.d20ActionData1)
                return false;

            if (first.d20SpellData.spellEnumOrg != second.d20SpellData.spellEnumOrg)
                return false;

            if (first.d20SpellData.metaMagicData != second.d20SpellData.metaMagicData)
                return false;

            if (first.d20ActionType == D20ActionType.NONE && first.text != second.text)
                return false;

            if (first.dispKey != second.dispKey)
                return false;

            return true;
        }

        [TempleDllLocation(0x100f07d0)]
        public void BuildRadialMenuAndSetToActive(GameObjectBody obj, Vector2 worldPosition)
        {
            GameSystems.Critter.BuildRadialMenu(obj);
            activeRadialMenu = GetRadialMenu(obj);
            if (activeRadialMenu != null)
            {
                ActiveMenuWorldPosition = worldPosition;
                activeRadialMenuNode = 0;
                ShiftPressed = false;
            }
        }

        // TODO: This is the actual child count (regarding invisible children as well)
        [TempleDllLocation(0x100f0850)]
        [TemplePlusLocation("radialmenu.cpp:168")]
        public int GetRadialActiveMenuNodeChildrenCount(int nodeIdx)
        {
            var actualNodeIdx = nodeIdx;
            if (ShiftPressed && activeRadialMenu.nodes[nodeIdx].morphsTo != -1)
            {
                actualNodeIdx = activeRadialMenu.nodes[nodeIdx].morphsTo;
            }

            return activeRadialMenu.nodes[actualNodeIdx].children.Count;
        }

        [TempleDllLocation(0x100f0890)]
        [TemplePlusLocation("radialmenu.cpp:180")]
        public int RadialMenuGetChild(int nodeId, int childIndex)
        {
            if (ShiftPressed && activeRadialMenu.nodes[nodeId].morphsTo != -1)
            {
                nodeId = activeRadialMenu.nodes[nodeId].morphsTo;
            }

            var childNodeIdx = activeRadialMenu.nodes[nodeId].children[childIndex];
            if (ShiftPressed)
            {
                if (activeRadialMenu.nodes[childNodeIdx].morphsTo != -1)
                {
                    childNodeIdx = activeRadialMenu.nodes[childNodeIdx].morphsTo;
                }
            }

            return childNodeIdx;
        }

        [TempleDllLocation(0x100f0a50)]
        public bool ActiveRadialHasChildrenWithCallback(int nodeIdx)
        {
            return RadialMenuNodeHasChildWithCallback(activeRadialMenu, nodeIdx);
        }

        [TempleDllLocation(0x100f0020)]
        public int RadialMenuGetActualArg(int nodeIdx)
        {
            var node = activeRadialMenu.nodes[nodeIdx];
            var type = node.entry.type;
            if (type != RadialMenuEntryType.Slider
                && type != RadialMenuEntryType.Toggle
                && type != RadialMenuEntryType.Choice)
            {
                return 0;
            }

            return node.entry.ArgumentGetter();

        }
        
        [TempleDllLocation(0x100effc0)]
        public bool RadialMenuSetActiveNodeArg(int value)
        {
            if ( activeRadialMenuNode == -1 )
            {
                return false;
            }
            if ( activeRadialMenu == null )
            {
                return false;
            }

            var node = activeRadialMenu.nodes[activeRadialMenuNode];
            var type = node.entry.type;
            if (type != RadialMenuEntryType.Slider
                && type != RadialMenuEntryType.Toggle
                && type != RadialMenuEntryType.Choice)
            {
                return false;
            }

            var maxArg = node.entry.maxArg;
            if ( value > maxArg )
            {
                node.entry.ArgumentSetter(node.entry.minArg);
                return false;
            }
            if ( value >= node.entry.minArg )
            {
                node.entry.ArgumentSetter(value);
                return true;
            }
            else
            {
                node.entry.ArgumentSetter(node.entry.maxArg);
                return false;
            }
        }

        // TODO: This probably just means "IsNodeVisible"?
        [TempleDllLocation(0x100f0520)]
        private bool RadialMenuNodeHasChildWithCallback(RadialMenu radmenu, int nodeIdx)
        {
            if (nodeIdx == -1)
            {
                return false;
            }

            var node = radmenu.nodes[nodeIdx];
            if (node.entry.callback != null)
            {
                // Previously it was checking against an always-true condition here
                return true;
            }

            // Search for a visible child
            foreach (var childIdx in node.children)
            {
                if (RadialMenuNodeHasChildWithCallback(radmenu, childIdx))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x100f0930)]
        public bool RadialMenuNodeContainsChildQuery(int parentNodeIdx, int childNodeIdx)
        {
            var parentNode = activeRadialMenu.nodes[parentNodeIdx];
            if (parentNode.morphsTo == childNodeIdx)
            {
                return true;
            }

            if (ShiftPressed)
            {
                if (parentNode.morphsTo != -1)
                {
                    parentNodeIdx = parentNode.morphsTo;
                }
            }

            if (childNodeIdx == parentNodeIdx)
            {
                return true;
            }

            if (ShiftPressed)
            {
                if (activeRadialMenu.nodes[childNodeIdx].morphsTo != -1)
                {
                    childNodeIdx = activeRadialMenu.nodes[childNodeIdx].morphsTo;
                }
            }

            foreach (var otherChildIdx in activeRadialMenu.nodes[parentNodeIdx].children)
            {
                if (RadialMenuNodeContainsChildQuery(otherChildIdx, childNodeIdx))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x100f08f0)]
        public RadialMenuNode GetActiveRadMenuNodeRegardMorph(int nodeIdx)
        {
            if (ShiftPressed && activeRadialMenu.nodes[nodeIdx].morphsTo != -1)
            {
                nodeIdx = activeRadialMenu.nodes[nodeIdx].morphsTo;
            }

            return activeRadialMenu.nodes[nodeIdx];
        }

        [TempleDllLocation(0x100f0820)]
        public bool RadialMenuSetActiveNode(int nodeIdx)
        {
            // Only allow visible nodes to be made active
            if (RadialMenuNodeHasChildWithCallback(activeRadialMenu, nodeIdx))
            {
                activeRadialMenuNode = nodeIdx;
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x100eff80)]
        public bool RadialMenuActiveNodeExecuteCallback()
        {
            if (activeRadialMenu != null && activeRadialMenuNode != -1)
            {
                var activeNode = activeRadialMenu.nodes[activeRadialMenuNode];
                var callback = activeNode.entry.callback;
                if (callback == null)
                {
                    return false;
                }

                return callback(
                    activeRadialMenu.obj,
                    ref activeNode.entry
                );
            }

            return true;
        }
    }
}