using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
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
        public MainMenuUi()
        {
            var mmLocalization = Tig.FS.ReadMesFile("mes/mainmenu.mes");

            var widgetDoc = WidgetDoc.Load("ui/main_menu.json");
            mMainWidget = widgetDoc.TakeRootContainer();

            mViewCinematicsDialog = new ViewCinematicsDialog(this, mmLocalization);
            mSetPiecesDialog = new SetPiecesDialog(this);

            // This eats all mouse messages that reach the full-screen main menu
            mMainWidget.SetMouseMsgHandler(msg => { return true; });
            mMainWidget.SetWidgetMsgHandler(msg => { return true; });

            mMainWidget.SetKeyStateChangeHandler(msg =>
            {
                // Close the menu if it's the ingame menu
                if (msg.key == DIK.DIK_ESCAPE && !msg.down)
                {
                    if (mCurrentPage == MainMenuPage.InGameNormal || mCurrentPage == MainMenuPage.InGameIronman)
                    {
                        Hide();
                    }
                }

                return true;
            });

            mPagesWidget = widgetDoc.GetContainer("pages");

            mPageWidgets[MainMenuPage.MainMenu] = widgetDoc.GetContainer("page-main-menu");
            mPageWidgets[MainMenuPage.Difficulty] = widgetDoc.GetContainer("page-difficulty");
            mPageWidgets[MainMenuPage.InGameNormal] = widgetDoc.GetContainer("page-ingame-normal");
            mPageWidgets[MainMenuPage.InGameIronman] = widgetDoc.GetContainer("page-ingame-ironman");
            mPageWidgets[MainMenuPage.Options] = widgetDoc.GetContainer("page-options");
            //mPageWidgets[MainMenuPage.SetPieces] = widgetDoc.GetWindow("page-set-pieces");

            // Wire up buttons on the main menu
            widgetDoc.GetButton("new-game").SetClickHandler(() => { Show(MainMenuPage.Difficulty); });
            widgetDoc.GetButton("load-game").SetClickHandler(() =>
            {
                Hide();
                UiSystems.SaveGame.ShowLoad(true);
            });
            widgetDoc.GetButton("tutorial").SetClickHandler(() => LaunchTutorial());
            widgetDoc.GetButton("options").SetClickHandler(() => { Show(MainMenuPage.Options); });
            widgetDoc.GetButton("quit-game").SetClickHandler(() =>
            {
                Tig.MessageQueue.Enqueue(new Message(MessageType.EXIT));
            });

            // Wire up buttons on the difficulty selection page
            widgetDoc.GetButton("difficulty-normal").SetClickHandler(() =>
            {
                Globals.GameLib.IsIronmanGame = false;
                Hide();
                UiSystems.PCCreation.Start();
            });
            widgetDoc.GetButton("difficulty-ironman").SetClickHandler(() =>
            {
                Globals.GameLib.IsIronmanGame = true;
                Hide();
                UiSystems.PCCreation.Start();
            });
            widgetDoc.GetButton("difficulty-exit").SetClickHandler(() => { Show(MainMenuPage.MainMenu); });

            // Wire up buttons on the ingame menu (normal difficulty)
            widgetDoc.GetButton("ingame-normal-load").SetClickHandler(() =>
            {
                Hide();
                UiSystems.SaveGame.ShowLoad(false);
            });
            widgetDoc.GetButton("ingame-normal-save").SetClickHandler(() =>
            {
                Hide();
                UiSystems.SaveGame.ShowSave(true);
            });
            widgetDoc.GetButton("ingame-normal-close").SetClickHandler(Hide);
            widgetDoc.GetButton("ingame-normal-quit").SetClickHandler(() =>
            {
                Hide();
                GameSystems.ResetGame();
                UiSystems.Reset();
                Show(MainMenuPage.MainMenu);
            });

            // Wire up buttons on the ingame menu (ironman difficulty)
            widgetDoc.GetButton("ingame-ironman-close").SetClickHandler(Hide);
            widgetDoc.GetButton("ingame-ironman-save-quit").SetClickHandler(() =>
            {
                Globals.GameLib.IronmanSave();
                Globals.GameLib.Reset();
                UiSystems.Reset();
                Show(MainMenuPage.MainMenu);
            });

            // Wire up buttons on the ingame menu (ironman difficulty)
            widgetDoc.GetButton("options-show").SetClickHandler(() =>
            {
                Hide();
                UiSystems.Options.Show(true);
            });
            widgetDoc.GetButton("options-view-cinematics").SetClickHandler(() =>
            {
                Hide();
                UiSystems.UtilityBar.Hide();
                // TODO ui_mm_msg_ui4();
                mViewCinematicsDialog.Show();
            });
            widgetDoc.GetButton("options-credits").SetClickHandler(() =>
            {
                Hide();

                List<int> creditsMovies = new List<int> {100, 110, 111, 112, 113};
                foreach (var movieId in creditsMovies)
                {
                    GameSystems.Movies.MovieQueueAdd(movieId);
                }

                GameSystems.Movies.MovieQueuePlay();

                Show(MainMenuPage.Options);
            });
            widgetDoc.GetButton("options-back").SetClickHandler(() => { Show(MainMenuPage.MainMenu); });

            RepositionWidgets(Globals.UiManager.ScreenSize);
            Globals.UiManager.OnScreenSizeChanged += RepositionWidgets;

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
            // The main menu is defined as visible, if any of the pages is visible
            foreach (var entry in mPageWidgets)
            {
                if (entry.Value.Visible)
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x10116500)]
        public void Show(MainMenuPage page)
        {
            // In case the main menu is shown in-game, we have to take care of some business
            if (!IsVisible())
            {
                if (page == MainMenuPage.InGameNormal || page == MainMenuPage.InGameIronman)
                {
                    GameSystems.TakeSaveScreenshots();
                    GameSystems.TimeEvent.PushDisableFidget();
                }
            }

            // TODO: This seems wrong actually... This should come after hide()
            mCurrentPage = page;
            Hide();

            UiSystems.SaveGame.Hide();
            UiSystems.HideOpenedWindows(false);
            UiSystems.CharSheet.Hide();

            mMainWidget.Show();
            mMainWidget.BringToFront();

            foreach (var entry in mPageWidgets)
            {
                entry.Value.Visible = entry.Key == page;
            }

            if (page != MainMenuPage.InGameNormal)
            {
                UiSystems.UtilityBar.Hide();
            }

            UiSystems.InGame.ResetInput();

            if (!UiSystems.UtilityBar.IsVisible())
                UiSystems.DungeonMaster.Hide();
        }

        [TempleDllLocation(0x10116220)]
        public void Hide()
        {
            if (IsVisible())
            {
                if (mCurrentPage == MainMenuPage.InGameNormal || mCurrentPage == MainMenuPage.InGameIronman)
                {
                    GameSystems.TimeEvent.PopDisableFidget();
                }
            }

            foreach (var entry in mPageWidgets)
            {
                entry.Value.Visible = false;
            }

            mMainWidget.Hide();

            if (mCurrentPage != MainMenuPage.InGameNormal)
            {
                UiSystems.UtilityBar.Show();
            }

            //if (UiSystems.UtilityBar.IsVisible())
            //	UiSystems.DungeonMaster.Show();
        }

        private MainMenuPage mCurrentPage = MainMenuPage.MainMenu;

        private ViewCinematicsDialog mViewCinematicsDialog;
        private SetPiecesDialog mSetPiecesDialog;

        private WidgetContainer mMainWidget;

        private Dictionary<MainMenuPage, WidgetContainer>
            mPageWidgets = new Dictionary<MainMenuPage, WidgetContainer>();

        // The widget that contains all pages
        private WidgetContainer mPagesWidget;

        private void RepositionWidgets(Size size)
        {
            // Attach the pages to the bottom of the screen
            mPagesWidget.Y = size.Height - mPagesWidget.Height;
        }

        [TempleDllLocation(0x10116170)]
        public Task LaunchTutorial()
        {
            InitializePlayerForTutorial();
            var task = SetupTutorialMap();
            UiSystems.Party.UpdateAndShowMaybe();
            Hide();
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
                tpArgs.movieFlags = 0;
                tpArgs.movieId = enterMovie;
            }

            tpArgs.FadeInArgs.flags = FadeFlag.FadeIn;
            tpArgs.FadeInArgs.color = new PackedLinearColorA(0, 0, 0, 255);
            tpArgs.FadeInArgs.fadeSteps = 64;
            tpArgs.FadeInArgs.transitionTime = 3.0f;
            var task = GameSystems.Teleport.FadeAndTeleport(in tpArgs);

            GameSystems.SoundGame.StopAll(false);
            UiSystems.WorldMapRandomEncounter.StartRandomEncounterTimer();
            GameSystems.TimeEvent.PopDisableFidget();

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
                GameSystems.Movies.PlayMovieId(movieId, 0, 0);
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