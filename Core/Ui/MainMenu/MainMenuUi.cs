using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Input;
using OpenTemple.Core.Config;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.Location;
using OpenTemple.Core.Scenes;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.Systems.Teleport;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.MainMenu
{
    public enum MainMenuPage
    {
        MainMenu,
        Difficulty,
        InGameNormal,
        InGameIronman,
        Options,
    }

    public class MainMenuUi : IDisposable
    {
        private readonly MainMenuViewModel _model = new();

        private readonly Dictionary<MainMenuPage, List<MainMenuEntry>> _menuPages = new();

        private readonly MainMenu _widget;

        public MainMenuUi()
        {
            _widget = new MainMenu {DataContext = _model, IsVisible = false};
            Tig.MainWindow.AddOverlay(_widget);

            var mmLocalization = Tig.FS.ReadMesFile("mes/mainmenu.mes");

            mViewCinematicsDialog = new ViewCinematicsDialog(this, mmLocalization);
            mSetPiecesDialog = new SetPiecesDialog(this);

            _widget.KeyUp += (_, args) =>
            {
                // Close the menu if it's the ingame menu
                if (args.Key == Key.Escape)
                {
                    if (_currentPage == MainMenuPage.InGameNormal || _currentPage == MainMenuPage.InGameIronman)
                    {
                        Hide();
                    }
                }
            };

            _menuPages[MainMenuPage.MainMenu] = new List<MainMenuEntry>()
            {
                // New Game
                new("#{main_menu:0}", () => { Show(MainMenuPage.Difficulty); }),
                // Load Game
                new("#{main_menu:1}", () =>
                {
                    Hide();
                    UiSystems.SaveGame.ShowLoad(true);
                }),
                // Tutorial
                new("#{main_menu:2}", () => { LaunchTutorial(); }),
                // Options
                new("#{main_menu:3}", () => { Show(MainMenuPage.Options); }),
                // Quit Game
                new("#{main_menu:4}", () => { Tig.MainWindow.Close(); }),
            };

            _menuPages[MainMenuPage.Difficulty] = new List<MainMenuEntry>()
            {
                // Normal
                new("#{main_menu:10}", () =>
                {
                    Globals.GameLib.IsIronmanGame = false;
                    Hide();
                    UiSystems.PCCreation.Start();
                }),
                // Ironman
                new("#{main_menu:11}", () =>
                {
                    Globals.GameLib.IsIronmanGame = true;
                    Hide();
                    UiSystems.PCCreation.Start();
                }),
                // Back
                new("#{main_menu:12}", () => { Show(MainMenuPage.MainMenu); }),
            };

            _menuPages[MainMenuPage.InGameNormal] = new List<MainMenuEntry>()
            {
                // Load Game
                new("#{main_menu:20}", () =>
                {
                    Hide();
                    UiSystems.SaveGame.ShowLoad(false);
                }),
                // Save Game
                new("#{main_menu:21}", () =>
                {
                    Hide();
                    UiSystems.SaveGame.ShowSave(true);
                }),
                // Close Menu
                new("#{main_menu:22}", Hide),
                // Quit to Menu
                new("#{main_menu:23}", () =>
                {
                    Hide();
                    GameSystems.ResetGame();
                    UiSystems.Reset();
                    Show(MainMenuPage.MainMenu);
                }),
            };

            _menuPages[MainMenuPage.InGameIronman] = new List<MainMenuEntry>()
            {
                // Close Menu
                new("#{main_menu:30}", Hide),
                // Quit to Menu
                new("#{main_menu:31}", () =>
                {
                    Globals.GameLib.IronmanSave();
                    Globals.GameLib.Reset();
                    UiSystems.Reset();
                    Show(MainMenuPage.MainMenu);
                }),
            };

            _menuPages[MainMenuPage.Options] = new List<MainMenuEntry>()
            {
                // Show Options
                new("#{main_menu:40}", () =>
                {
                    Hide();
                    UiSystems.Options.Show(true);
                }),
                // View Cinematics
                new("#{main_menu:41}", () =>
                {
                    Hide();
                    UiSystems.UtilityBar.Hide();
                    // TODO ui_mm_msg_ui4();
                    mViewCinematicsDialog.Show();
                }),
                // Credits
                new("#{main_menu:42}", () =>
                {
                    Hide();

                    List<int> creditsMovies = new List<int> {100, 110, 111, 112, 113};
                    foreach (var movieId in creditsMovies)
                    {
                        GameSystems.Movies.MovieQueueAdd(movieId);
                    }

                    GameSystems.Movies.MovieQueuePlay();

                    Show(MainMenuPage.Options);
                }),
                // Back
                new("#{main_menu:43}", () => Show(MainMenuPage.MainMenu)),
            };

            Hide(); // Hide everything by default
        }

        public void Dispose()
        {
            // TODO
        }

        // NOTE: This was part of the old main menu UI, not the new one.
        [TempleDllLocation(0x10112910)]
        public bool LoadGame(SaveGameInfo saveGame)
        {
            while (UiSystems.InGameSelect.IsPicking)
            {
                UiSystems.InGameSelect.FreeCurrentPicker();
            }

            Globals.GameLib.Reset();
            UiSystems.Reset();
            GameSystems.Player.PlayerObj_Destroy();
            if (Globals.GameLib.LoadGame(saveGame))
            {
                Globals.GameLib.ModuleName = saveGame.ModuleName;
                Stub.TODO("Calls to old MM UI were here"); // TODO
                return true;
            }
            else
            {
                while (UiSystems.InGameSelect.IsPicking)
                {
                    UiSystems.InGameSelect.FreeCurrentPicker();
                }

                Globals.GameLib.Reset();
                UiSystems.Reset();
                return false;
            }
        }

        [TempleDllLocation(0x101157f0)]
        public bool IsVisible()
        {
            return _widget.IsVisible;
        }

        [TempleDllLocation(0x10116500)]
        public void Show(MainMenuPage page)
        {
            // In case the main menu is shown in-game, we have to take care of some business
            if (!IsVisible())
            {
                if (page == MainMenuPage.InGameNormal || page == MainMenuPage.InGameIronman)
                {
                    GameSystems.TimeEvent.PauseGameTime();
                }
            }

            Hide();

            UiSystems.SaveGame.Hide();
            UiSystems.HideOpenedWindows(false);
            UiSystems.CharSheet.Hide();

            _widget.ZIndex = 1000;
            _widget.IsVisible = true;

            if (page != MainMenuPage.InGameNormal)
            {
                UiSystems.UtilityBar.Hide();
            }

            UiSystems.InGame.ResetInput();

            if (!UiSystems.UtilityBar.IsVisible())
                UiSystems.DungeonMaster.Hide();

            _model.Entries = _menuPages[page];
        }

        [TempleDllLocation(0x10116220)]
        public void Hide()
        {
            if (IsVisible())
            {
                if (_currentPage == MainMenuPage.InGameNormal || _currentPage == MainMenuPage.InGameIronman)
                {
                    GameSystems.TimeEvent.ResumeGameTime();
                }
            }

            _model.Entries = new List<MainMenuEntry>();
            _widget.IsVisible = false;

            if (_currentPage != MainMenuPage.InGameNormal)
            {
                UiSystems.UtilityBar.Show();
            }

            //if (UiSystems.UtilityBar.IsVisible())
            //	UiSystems.DungeonMaster.Show();
        }

        private MainMenuPage _currentPage = MainMenuPage.MainMenu;

        private ViewCinematicsDialog mViewCinematicsDialog;
        private SetPiecesDialog mSetPiecesDialog;

        [TempleDllLocation(0x10116170)]
        public Task LaunchTutorial()
        {
            InitializePlayerForTutorial();
            var task = SetupTutorialMap();
            UiSystems.Party.UpdateAndShowMaybe();
            Hide();
            Globals.Stage.PushScene(new InGameScene());
            UiSystems.Party.Update();
            return task;
        }

        [TempleDllLocation(0x10111AD0)]
        private Task SetupTutorialMap()
        {
            if (!UiSystems.HelpManager.IsTutorialActive)
            {
                UiSystems.HelpManager.ToggleTutorial();
            }

            var tutorialMap = GameSystems.Map.GetMapIdByType(MapType.TutorialMap);
            return TransitionToMap(tutorialMap);
        }

        [TempleDllLocation(0x10111130)]
        internal Task TransitionToMap(int mapId)
        {
            var fadeArgs = FadeArgs.Default;
            fadeArgs.fadeSteps = 1;
            GameSystems.GFade.PerformFade(ref fadeArgs);
            GameSystems.Anim.StartFidgetTimer();

            var tpArgs = FadeAndTeleportArgs.Default;
            tpArgs.destLoc = GameSystems.Map.GetStartPos(mapId);
            tpArgs.destMap = mapId;
            tpArgs.flags = FadeAndTeleportFlags.FadeIn;
            tpArgs.somehandle = GameSystems.Party.GetLeader();

            var enterMovie = GameSystems.Map.GetEnterMovie(mapId, true);
            if (enterMovie != 0)
            {
                tpArgs.flags |= FadeAndTeleportFlags.play_movie;
                tpArgs.movieId = enterMovie;
            }

            tpArgs.FadeInArgs.flags = FadeFlag.FadeIn;
            tpArgs.FadeInArgs.color = new PackedLinearColorA(0, 0, 0, 255);
            tpArgs.FadeInArgs.fadeSteps = 64;
            tpArgs.FadeInArgs.transitionTime = 3.0f;
            var task = GameSystems.Teleport.FadeAndTeleport(in tpArgs);

            GameSystems.SoundGame.StopAll(false);
            UiSystems.WorldMapRandomEncounter.StartRandomEncounterTimer();
            GameSystems.TimeEvent.ResumeGameTime();

            return task;
        }

        public void InitializePlayerForTutorial()
        {
            var valkor = GameSystems.MapObject.CreateObject(13105, new locXY(480, 40));
            valkor.SetInt32(obj_f.pc_voice_idx, 11);
            GameSystems.Critter.GenerateHp(valkor);
            GameSystems.Party.AddToPCGroup(valkor);

            GameSystems.Item.SpawnTutorialEquipment(valkor);

            // var anim = valkor.GetOrCreateAnimHandle();
            // objects.UpdateRenderHeight(velkor, *anim);
            // objects.UpdateRadius(velkor, *anim);
        }
    }


    class ViewCinematicsDialog
    {
        private readonly MainMenuUi _mainMenu;

        public ViewCinematicsDialog(MainMenuUi mainMenu, IDictionary<int, string> mmMes)
        {
            _mainMenu = mainMenu;

            WidgetDoc doc = WidgetDoc.Load("ui/main_menu_cinematics.json");

            doc.GetButton("view").SetClickHandler(() =>
            {
                if (mSelection < 0 || mSelection >= seenIndices.Count)
                    return;
                var movieIdx = seenIndices[mSelection];
                if (movieIdx < 0 || movieIdx >= movieIds.Count)
                    return;
                var movieId = movieIds[movieIdx];
                GameSystems.Movies.PlayMovieId(movieId, 0);
            });
            doc.GetButton("cancel").SetClickHandler(() =>
            {
                mWidget.Hide();
                _mainMenu.Show(MainMenuPage.Options);
            });

            mListBox = doc.GetScrollView("cinematicsList");

            mWidget = doc.TakeRootContainer();
            mWidget.Hide();

            for (var i = 0; i < 24; i++)
            {
                mMovieNames[i] = mmMes[2000 + i];
            }
        }

        public void Show()
        {
            mListBox.Clear();
            btnIds.Clear();
            seenIndices.Clear();


            for (var i = 0; i < movieIds.Count; i++)
            {
                if (IsMovieSeen(movieIds[i], -1))
                {
                    seenIndices.Add(i);
                }
            }

            int y = 0;
            for (int i = 0; i < seenIndices.Count; i++)
            {
                var movieInd = seenIndices[i];

                var button = new WidgetButton();
                button.SetText(mMovieNames[movieInd]);
                button.SetId(mMovieNames[movieInd]);
                var innerWidth = mListBox.GetInnerWidth();
                button.Width = innerWidth;
                button.SetAutoSizeWidth(false);
                button.SetStyle("mm-cinematics-list-button");
                button.Y = y;
                //var pBtn = button.get();
                btnIds.Add(button);
                var selectIdx = i;
                button.SetClickHandler(() => Select(selectIdx));
                y += button.Height;
                mListBox.Add(button);
            }

            mWidget.Show();
        }

        public void Select(int idx)
        {
            foreach (var pBtn in btnIds)
            {
                pBtn.SetStyle("mm-cinematics-list-button");
            }

            mSelection = idx;
            if (mSelection >= 0 && mSelection < btnIds.Count)
            {
                btnIds[mSelection].SetStyle("mm-cinematics-list-button-selected");
            }
        } // changes scrollbox selection

        public bool IsMovieSeen(int movieId, int soundId)
        {
            return Globals.Config.SeenMovies.Contains(new SeenMovie(movieId, soundId));
        }

        private int mSelection = 0;
        private WidgetContainer mWidget;
        private Dictionary<int, string> mMovieNames = new Dictionary<int, string>();
        private WidgetScrollView mListBox;
        private List<WidgetButton> btnIds = new List<WidgetButton>();
        private List<int> seenIndices = new List<int>(); // indices into movieIds / mMovieNames

        private List<int> movieIds = new List<int>
        {
            1000, 1009, 1007,
            1012, 1002, 1015,
            1005, 1010, 1004,
            1013, 1006, 1016,
            1001, 1011, 1008,
            1014, 1003, 1017,
            304, 300, 303,
            301, 302, 1009
        };
    };

    class SetPiecesDialog
    {
        private readonly MainMenuUi _mainMenuUi;

        public SetPiecesDialog(MainMenuUi mainMenuUi)
        {
            _mainMenuUi = mainMenuUi;

            WidgetDoc doc = WidgetDoc.Load("ui/main_menu_setpieces.json");

            doc.GetButton("go").SetClickHandler(() =>
            {
                mWidget.Hide();
                LaunchScenario();
            });
            doc.GetButton("cancel").SetClickHandler(() =>
            {
                mWidget.Hide();
                _mainMenuUi.Show(MainMenuPage.MainMenu);
            });

            mListBox = doc.GetScrollView("scenariosList");

            mWidget = doc.TakeRootContainer();
            mWidget.Hide();
        }

        public void Select(int i)
        {
            mSelection = i;
        }

        public void Show()
        {
            mListBox.Clear();

            int y = 0;
            const int NUM_SCENARIOS = 0;
            for (int i = 0; i < NUM_SCENARIOS; i++)
            {
                var button = new WidgetButton();
                button.SetText("Arena");
                button.SetId("Arena");
                var innerWidth = mListBox.GetInnerWidth();
                button.Width = innerWidth;
                button.SetAutoSizeWidth(false);
                button.SetStyle("mm-setpieces-list-button");
                button.Y = y;
                btnIds.Add(button);
                var idx = i;
                button.SetClickHandler(() => Select(idx));
                y += button.Height;
                mListBox.Add(button);
            }

            mWidget.Show();
        }

        public void LaunchScenario()
        {
            _mainMenuUi.InitializePlayerForTutorial();

            SetupScenario();
            UiSystems.Party.UpdateAndShowMaybe();
            _mainMenuUi.Hide();
            UiSystems.Party.Update();
        }

        public void SetupScenario()
        {
            var destMap = GameSystems.Map.GetMapIdByType(MapType.ArenaMap);
            _mainMenuUi.TransitionToMap(destMap);

            // TODO var args = PyTuple_New(0);

            // TODO var result = pythonObjIntegration.ExecuteScript("arena_script", "OnStartup", args);
            // TODO Py_DECREF(result);
            // TODO Py_DECREF(args);
        }

        private int mSelection = 0;
        private WidgetContainer mWidget;
        private WidgetScrollView mListBox;
        private List<WidgetButton> btnIds;
    }
}
