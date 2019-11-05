using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Logbook
{
    public class LogbookUi : IDisposable, IResetAwareSystem, ISaveGameAwareUi
    {
        private const int TAB_QUESTS = 0;
        private const int TAB_REP = 1;
        private const int TAB_EGO = 2;
        private const int TAB_KEYS = 3;
        private const int TAB_RUMORS = 4;
        private const int TAB_QUOTES = 5;

        private static readonly ILogger Logger = new ConsoleLogger();

        public LogbookQuestsUi Quests { get; } = new LogbookQuestsUi();

        public LogbookReputationUi Reputation { get; } = new LogbookReputationUi();

        public LogbookEgoUi Ego { get; } = new LogbookEgoUi();

        public LogbookKeysUi Keys { get; } = new LogbookKeysUi();

        public LogbookRumorsUi Rumors { get; } = new LogbookRumorsUi();

        public LogbookQuotesUi Quotes { get; } = new LogbookQuotesUi();

        [TempleDllLocation(0x101260f0)]
        [TempleDllLocation(0x10be0c58)]
        public bool IsVisible { get; private set; }

        private readonly Dictionary<int, string> _translations;

        private WidgetContainer _window;

        private LogbookTabButton[] _tabButtons;

        private WidgetContainer _tabCoverContainer;

        [TempleDllLocation(0x10BE0C6C)]
        private bool _canShowQuotes;

        [TempleDllLocation(0x10BE0C4C)]
        private int _currentTab = TAB_QUESTS;

        [TempleDllLocation(0x101281a0)]
        public LogbookUi()
        {
            _translations = Tig.FS.ReadMesFile("mes/logbook_ui_text.mes");

            CreateWidgets();
        }

        [TempleDllLocation(0x10125EA0)]
        public void SetCanShowQuotes(bool enable)
        {
            _canShowQuotes = enable;
        }

        [TempleDllLocation(0x10126d90)]
        private void CreateWidgets()
        {
            _window = new WidgetContainer(new Rectangle(27, 40, 750, 445));
            _window.SetKeyStateChangeHandler(OnKeyStateChange);
            _window.ZIndex = 100050;
            _window.Name = "logbook_ui_main_window";
            // Eat window and click messages
            _window.SetMouseMsgHandler(msg => true);
            _window.SetVisible(false);

            var background = new WidgetImage("art/interface/logbook_ui/whole_book.img");
            background.SetY(25);
            background.FixedSize = new Size(750, 420);
            _window.AddContent(background);

            var exitButton = new WidgetButton(new Rectangle(694, 393, 55, 52));
            exitButton.SetStyle(new WidgetButtonStyle
            {
                normalImagePath = "art/interface/logbook_ui/exit_normal.tga",
                hoverImagePath = "art/interface/logbook_ui/exit_hover.tga",
                pressedImagePath = "art/interface/logbook_ui/exit_click.tga"
            }.UseDefaultSounds());
            exitButton.SetClickHandler(Hide);
            exitButton.Name = "logbook_ui_main_exit_butn";
            _window.Add(exitButton);

            _tabButtons = new[]
            {
                new LogbookTabButton(_translations[11]),
                new LogbookTabButton(_translations[12]),
                new LogbookTabButton(_translations[13]),
                new LogbookTabButton(_translations[14]),
                new LogbookTabButton(_translations[15]),
                new LogbookTabButton(_translations[16])
            };

            var buttonX = 40;
            for (var index = 0; index < _tabButtons.Length; index++)
            {
                var tabButton = _tabButtons[index];
                tabButton.SetX(buttonX);
                // Does it reach into the "no tab zone"? Then move it to the right page of the book
                if (tabButton.GetX() + tabButton.GetWidth() >= 302 && buttonX < 389)
                {
                    buttonX = 389;
                    tabButton.SetX(buttonX);
                }

                buttonX += tabButton.GetWidth() - 13; // The -13 is to cause an overlap
                _window.Add(tabButton);

                // Activate the corresponding tab on click
                var tabToActivate = index;
                tabButton.SetClickHandler(() =>
                {
                    _currentTab = tabToActivate;
                    Show(_showingQuotes);
                });
            }

            // This image is used to cover the "lower" part of the tabs to make them appear as bookmarks
            // and must be added after the tabs themselves for z-index reasons
            _tabCoverContainer = new WidgetContainer(new Rectangle(14, 35, 719, 48));
            _tabCoverContainer.AddContent(new WidgetImage("art/interface/logbook_ui/tab_cover.img"));
            _window.Add(_tabCoverContainer);

            // Add the tab containers
            _window.Add(Keys.Container);
        }

        [TempleDllLocation(0x10126ba0)]
        private bool OnKeyStateChange(MessageKeyStateChangeArgs arg)
        {
            if (arg.key == DIK.DIK_ESCAPE && arg.down == false)
            {
                Hide();
                return true;
            }

            return false;
        }


        [TempleDllLocation(0x101ccde0)]
        public void RecordSkillUse(GameObjectBody critter, SkillId skill)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101d4ca0)]
        public void RecordKill(GameObjectBody killer, GameObjectBody killed)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101d0070)]
        public void RecordCriticalHit(GameObjectBody attacker)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101ccd40)]
        public void RecordTrapDisarmed(GameObjectBody critter)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101ccd90)]
        public void RecordTrapSetOff(GameObjectBody critter)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10D249F8)]
        private int _turnCounter = 0;

        [TempleDllLocation(0x101d0040)]
        public void RecordCombatStart()
        {
            _turnCounter = 1;
        }

        [TempleDllLocation(0x101d0050)]
        public void RecordNewTurn()
        {
            if (_turnCounter > 0)
            {
                _turnCounter++;
            }
        }

        [TempleDllLocation(0x101d4590)]
        public void RecordCombatMiss(GameObjectBody performer)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101d4530)]
        public void RecordCombatHit(GameObjectBody member)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101ce5e0)]
        public void RecordCombatDamage(bool weaponDamage, int damageAmount, GameObjectBody attacker,
            GameObjectBody victim)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10128310)]
        public void Show()
        {
            _window.CenterOnScreen();

            if (_canShowQuotes && (Tig.Keyboard.IsPressed(DIK.DIK_LMENU) || Tig.Keyboard.IsPressed(DIK.DIK_RMENU)))
            {
                Show(true);
            }
            else
            {
                if (_currentTab == TAB_QUOTES)
                {
                    _currentTab = TAB_QUESTS;
                }

                Show(false);
            }
        }

        [TempleDllLocation(0x10126030)]
        public void Hide()
        {
            if (IsVisible)
            {
                IsVisible = false;
                _window.SetVisible(false);

                Quests.Hide();
                Ego.Hide();
                Keys.Hide();
                Reputation.Hide();
                Quotes.Hide();
                _showingQuotes = false;
                GameSystems.TimeEvent.PopDisableFidget();
            }
        }

        [TempleDllLocation(0x10be0c78)]
        private bool _showingQuotes;

        [TempleDllLocation(0x10125eb0)]
        public void Show(bool showQuotes)
        {
            UiSystems.HideOpenedWindows(true);
            if (!IsVisible)
            {
                GameSystems.TimeEvent.PushDisableFidget();
            }

            IsVisible = true;

            Quests.Hide();
            Ego.Hide();
            Reputation.Hide();
            Keys.Hide();
            Rumors.Hide();
            Quotes.Hide();

            _window.SetVisible(true);
            _window.BringToFront();

            _showingQuotes = showQuotes;

            for (var i = 0; i < _tabButtons.Length; i++)
            {
                _tabButtons[i].SetActive(i == _currentTab);
            }

            // The active tab button is on front of the page
            _tabCoverContainer.BringToFront();
            _tabButtons[_currentTab].BringToFront();
            _tabButtons[TAB_QUOTES].SetVisible(showQuotes);

            switch (_currentTab)
            {
                case TAB_QUESTS:
                    Quests.Show();
                    break;
                case TAB_REP:
                    Reputation.Show();
                    break;
                case TAB_EGO:
                    Ego.Show();
                    break;
                case TAB_KEYS:
                    Keys.Show();
                    break;
                case TAB_RUMORS:
                    Rumors.Show();
                    break;
                case TAB_QUOTES:
                    Quotes.Show();
                    break;
                default:
                    return;
            }
        }

        public void Dispose()
        {
            Keys.Dispose();
        }

        [TempleDllLocation(0x10125dc0)]
        public void Reset()
        {
            Ego.Reset();
            Keys.Reset();
            Quests.Reset();
            Reputation.Reset();
            Rumors.Reset();
            Quotes.Reset();
        }

        [TempleDllLocation(0x10125de0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10125e40)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }
    }
}