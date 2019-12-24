using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells
{
    public class CharSheetSpellsUi : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private const string TabLabelStyle = "char-spell-class-tab-label";

        public WidgetContainer Container { get; }

        // Contains the tabs for switching between classes
        private readonly WidgetContainer _classTabBar;

        private readonly List<ClassSpellListData> _spellLists = new List<ClassSpellListData>();

        private readonly WidgetText _spellsKnownHeader;
        private readonly WidgetContainer _knownSpellsContainer;
        private readonly WidgetText _memorizedSpellsHeader;
        private readonly WidgetContainer _memorizedSpellsContainer;
        private MemorizedSpellsList _memorizedSpellsList;

        [TempleDllLocation(0x101bbbc0)]
        public CharSheetSpellsUi()
        {
            var doc = WidgetDoc.Load("ui/char_spells.json");

            Container = doc.TakeRootContainer();
            Container.ZIndex = 100050;
            Container.Name = "char_spells_ui_main_window";

            _classTabBar = doc.GetContainer("char_spells_ui_nav_class_tab_bar");

            _spellsKnownHeader = doc.GetTextContent("known-spells-header");
            _knownSpellsContainer = doc.GetContainer("known-spells-container");
            _memorizedSpellsHeader = doc.GetTextContent("memorized-spells-header");
            _memorizedSpellsContainer = doc.GetContainer("memorized-spells-container");

//            // Created @ 0x101bb77c
//            var char_spells_ui_nav_class_tab_button1 = new WidgetButton(new Rectangle(0, 0, 0, 19));
//            // char_spells_ui_nav_class_tab_button1.OnHandleMessage += 0x101b8b60;
//            // char_spells_ui_nav_class_tab_button1.OnBeforeRender += 0x101b6bf0;
//            // char_spells_ui_nav_class_tab_button1.OnRenderTooltip += 0x101b8320;
//            char_spells_ui_nav_class_tab_button1.Name = "char_spells_ui_nav_class_tab_button";
//            char_spells_ui_nav_class_tab_window1.Add(char_spells_ui_nav_class_tab_button1);

            Stub.TODO();
        }

        [TempleDllLocation(0x101b5d20)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101b5d80)]
        public void Show(GameObjectBody critter)
        {
            _spellLists.Clear();
            BuildCharSpellLists(critter, _spellLists);
            BuildTabBar();

            // Make the first tab active
            if (_spellLists.Count > 0)
            {
                ActivateTab(_spellLists[0]);
            }
            else
            {
                _spellsKnownHeader.Visible = false;
                _knownSpellsContainer.Visible = false;
                _memorizedSpellsHeader.Visible = false;
                _memorizedSpellsContainer.Visible = false;
            }

            Container.Visible = true;
            Stub.TODO();
        }

        private void BuildCharSpellLists(GameObjectBody critter, List<ClassSpellListData> spellLists)
        {
            spellLists.Clear();

            var spellsPerDayLists = GameSystems.Spell.GetSpellsPerDay(critter);
            foreach (var spellsPerDay in spellsPerDayLists)
            {
                var spellListTab = new ClassSpellListData(critter);
                spellListTab.SpellsPerDay = spellsPerDay;
                spellLists.Add(spellListTab);
            }
        }

        private void BuildTabBar()
        {
            _classTabBar.Clear();

            var currentX = 0;
            foreach (var spellList in _spellLists)
            {
                var button = new ClassTabButton(spellList.SpellsPerDay.Name, TabLabelStyle);
                button.Y = 4;
                button.X = currentX;
                button.SetClickHandler(() => ActivateTab(spellList));
                currentX += button.Width;
                _classTabBar.Add(button);

                spellList.Button = button;
            }
        }

        private void ActivateTab(ClassSpellListData spellList)
        {
            if (spellList.Active)
            {
                return;
            }

            _memorizedSpellsContainer.Clear();
            _memorizedSpellsContainer.Visible = false;
            _knownSpellsContainer.Clear();
            _knownSpellsContainer.Visible = true;
            _memorizedSpellsHeader.Visible = false;
            _memorizedSpellsList?.Dispose();
            _memorizedSpellsList = null;

            foreach (var otherSpellList in _spellLists)
            {
                otherSpellList.Active = otherSpellList == spellList;
                otherSpellList.Button.IsActive = otherSpellList == spellList;
            }

            var spellsPerDay = spellList.SpellsPerDay;
            var classCode = spellsPerDay.ClassCode;
            if (GameSystems.Spell.IsDomainSpell(classCode)
                || GameSystems.Spell.GetCastingClass(classCode) != Stat.level_wizard)
            {
                _spellsKnownHeader.SetText("#{char_ui_spells:4}"); // Known spells
            }
            else
            {
                _spellsKnownHeader.SetText("#{char_ui_spells:0}"); // Spellbook
            }

            var knownSpellsList = new KnownSpellsList(
                new Rectangle(5, 3, 189, 222),
                spellList.Caster,
                classCode
            );
            _knownSpellsContainer.Add(knownSpellsList);
            _knownSpellsContainer.Visible = true;

            if (spellsPerDay.Type == SpellsPerDayType.Vancian)
            {
                knownSpellsList.OnMemorizeSpell += (spell, desiredSlotButton) =>
                    MemorizeSpell(spellList, spell, desiredSlotButton);

                _memorizedSpellsHeader.Visible = true;
                _memorizedSpellsList = new MemorizedSpellsList(
                    new Rectangle(5, 3, 189, 222),
                    spellList.Caster,
                    spellsPerDay
                );
                _memorizedSpellsList.OnUnmemorizeSpell +=
                    (level, slotIndex) => UnmemorizeSpell(spellList, level, slotIndex);
                _memorizedSpellsContainer.Add(_memorizedSpellsList);
                _memorizedSpellsContainer.Visible = true;
            }
        }

        private void UnmemorizeSpell(ClassSpellListData spellList, int level, int slotIndex)
        {
            var spellsPerDay = spellList.SpellsPerDay;
            var caster = spellList.Caster;

            if (spellsPerDay.Type != SpellsPerDayType.Vancian)
            {
                return; // Only makes sense to unmemorize spells when the caster needs to memorize in the first place
            }

            if (level >= spellsPerDay.Levels.Length || slotIndex >= spellsPerDay.Levels[level].Slots.Length)
            {
                return; // Button is out of range -> shouldn't happen
            }

            var slot = spellsPerDay.Levels[level].Slots[slotIndex];
            if (!slot.HasSpell)
            {
                return;
            }

            GameSystems.Spell.SpellRemoveFromStorage(caster, obj_f.critter_spells_memorized_idx, slot.MemorizedSpell,
                0);

            UpdateMemorizedSpells(spellList);
        }

        [TempleDllLocation(0x101b8f10)]
        private void MemorizeSpell(ClassSpellListData spellList, SpellStoreData knownSpell,
            MemorizedSpellButton desiredSlotButton)
        {
            var critter = spellList.Caster;
            var spellsPerDay = spellList.SpellsPerDay;


            // If the player chose a specific slot, we'll overwrite whatever is in it and if necessary also make it used
            if (desiredSlotButton != null)
            {
                var desiredSlot = desiredSlotButton.Slot;
                var desiredLevel = desiredSlotButton.Level;
                var desiredSlotIndex = desiredSlotButton.SlotIndex;
                if (CanMemorizeInSlot(critter, knownSpell, desiredLevel, desiredSlot))
                {
                    // Remove the existing spell in the slot first
                    if (desiredSlot.HasSpell)
                    {
                        UnmemorizeSpell(spellList, desiredLevel, desiredSlotIndex);
                    }

                    MemorizeInSlot(critter, knownSpell, desiredLevel, desiredSlotIndex);
                    UpdateMemorizedSpells(spellList);
                }

                return;
            }

            // Otherwise find a free slot of appropriate level
            if (knownSpell.spellLevel >= spellsPerDay.Levels.Length)
            {
                Logger.Debug("Cannot memorize spell of level {0}, because {1} has no slots for that level.",
                    knownSpell.spellLevel, critter);
                return;
            }

            var level = spellsPerDay.Levels[knownSpell.spellLevel];
            for (var slotIndex = 0; slotIndex < level.Slots.Length; slotIndex++)
            {
                var slot = level.Slots[slotIndex];
                if (!slot.HasSpell && CanMemorizeInSlot(critter, knownSpell, level.Level, slot))
                {
                    MemorizeInSlot(critter, knownSpell, level.Level, slotIndex);
                    UpdateMemorizedSpells(spellList);
                    return;
                }
            }

            Logger.Debug("Cannot memorize spell of level {0}, because no suitable slots exist.", knownSpell.spellLevel);
        }

        private void UpdateMemorizedSpells(ClassSpellListData spellList)
        {
            GameSystems.Spell.UpdateMemorizedSpells(spellList.Caster, spellList.SpellsPerDay);
            _memorizedSpellsList?.UpdateSpells();
        }

        [TempleDllLocation(0x101b5bc0)]
        private void MemorizeInSlot(GameObjectBody caster, SpellStoreData knownSpell, int slotLevel, int slotIndex)
        {
            var spellStoreState = new SpellStoreState();
            spellStoreState.usedUp = true;
            spellStoreState.spellStoreType = SpellStoreType.spellStoreMemorized;

            // TODO: set 0x80000000 in metamagic if used slot is a specialization slot

            GameSystems.Spell.SpellMemorizedAdd(
                caster,
                knownSpell.spellEnum,
                knownSpell.classCode,
                knownSpell.spellLevel,
                spellStoreState,
                knownSpell.metaMagicData
            );
        }

        private bool CanMemorizeInSlot(GameObjectBody caster, SpellStoreData knownSpell, int slotLevel, SpellSlot slot)
        {
            // Slot must be of appropriate level
            if (slotLevel != knownSpell.spellLevel)
            {
                return false;
            }

            // Only appropriate spells can go into specialization slots
            if (slot.Source == SpellSlotSource.WizardSpecialization)
            {
                GameSystems.Spell.GetSchoolSpecialization(caster, out var specializedSchool, out _, out _);
                if (specializedSchool != GameSystems.Spell.GetSpellSchoolEnum(knownSpell.spellEnum))
                {
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x101b6a10)]
        public void Hide()
        {
            Container.Visible = false;
            Stub.TODO();
        }

        [TempleDllLocation(0x101b1860)]
        public void Reset()
        {
            Stub.TODO();
        }
    }
}