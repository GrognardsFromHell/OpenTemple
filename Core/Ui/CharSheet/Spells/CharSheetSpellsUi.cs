using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Spells
{
    public class CharSheetSpellsUi : IDisposable
    {
        private const string TabLabelStyle = "char-spell-class-tab-label";

        public WidgetContainer Container { get; }

        // Contains the tabs for switching between classes
        private readonly WidgetContainer _classTabBar;

        private readonly List<ClassSpellListData> _spellLists = new List<ClassSpellListData>();

        private readonly WidgetText _spellsKnownHeader;
        private readonly WidgetContainer _spellsKnownContainer;
        private readonly WidgetText _memorizedSpellsHeader;
        private readonly WidgetContainer _memorizedSpellsContainer;

        [TempleDllLocation(0x101bbbc0)]
        public CharSheetSpellsUi()
        {
            var doc = WidgetDoc.Load("ui/char_spells.json");

            Container = doc.TakeRootContainer();
            Container.ZIndex = 100050;
            Container.Name = "char_spells_ui_main_window";

            _classTabBar = doc.GetWindow("char_spells_ui_nav_class_tab_bar");

            _spellsKnownHeader = doc.GetTextContent("known-spells-header");
            _spellsKnownContainer = doc.GetWindow("known-spells-container");
            _memorizedSpellsHeader = doc.GetTextContent("memorized-spells-header");
            _memorizedSpellsContainer = doc.GetWindow("memorized-spells-container");

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

            Container.SetVisible(true);
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
                button.SetY(4);
                button.SetX(currentX);
                button.SetClickHandler(() => ActivateTab(spellList));
                currentX += button.GetWidth();
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
            _spellsKnownContainer.Clear();

            foreach (var otherSpellList in _spellLists)
            {
                otherSpellList.Active = otherSpellList == spellList;
                otherSpellList.Button.SetActive(otherSpellList == spellList);
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

            _spellsKnownContainer.Add(new SpellsKnownList(
                new Rectangle(5, 3, 189, 222),
                spellList.Caster,
                classCode
            ));

            if (spellsPerDay.Type == SpellsPerDayType.Vancian)
            {
            }

        }

        [TempleDllLocation(0x101b6a10)]
        public void Hide()
        {
            Container.SetVisible(false);
            Stub.TODO();
        }

        [TempleDllLocation(0x101b1860)]
        public void Reset()
        {
            Stub.TODO();
        }
    }
}