using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.Config;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Alert;
using OpenTemple.Core.Ui.Camping;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Combat;
using OpenTemple.Core.Ui.Dialog;
using OpenTemple.Core.Ui.InGame;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Ui.Logbook;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.Options;
using OpenTemple.Core.Ui.Party;
using OpenTemple.Core.Ui.PartyCreation;
using OpenTemple.Core.Ui.PartyPool;
using OpenTemple.Core.Ui.RadialMenu;
using OpenTemple.Core.Ui.SaveGame;
using OpenTemple.Core.Ui.UtilityBar;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
    public static class UiSystems
    {
        private static readonly List<IDisposable> _disposableSystems = new List<IDisposable>();

        private static readonly List<ITimeAwareSystem> _timeAwareSystems = new List<ITimeAwareSystem>();

        private static readonly List<IResetAwareSystem> _resetAwareSystems = new List<IResetAwareSystem>();

        private static readonly List<ISaveGameAwareUi> _saveSystems = new List<ISaveGameAwareUi>();

        public static MainMenuUi MainMenu { get; private set; }

        // UiMM is unused

        public static LoadGameUi LoadGame { get; private set; }

        public static SaveGameUi SaveGame { get; private set; }

        public static InGameUi InGame { get; private set; }

        public static RadialMenuUi RadialMenu { get; private set; }

        public static InGameSelectUi InGameSelect { get; private set; }

        public static TurnBasedUi TurnBased { get; private set; }

        public static AnimUi Anim { get; private set; }

        public static TBUi TB { get; private set; }

        public static WorldMapRandomEncounterUi WorldMapRandomEncounter { get; private set; }

        public static CombatUi Combat { get; private set; }

        public static SlideUi Slide { get; private set; }

        public static DialogUi Dialog { get; private set; }

        public static PCCreationUi PCCreation { get; private set; }

        public static CharSheetUi CharSheet { get; private set; }

        public static TooltipUi Tooltip { get; private set; }

        public static LogbookUi Logbook { get; private set; }

        public static ScrollpaneUi Scrollpane { get; private set; }

        public static TownMapUi TownMap { get; private set; }

        public static PopupUi Popup { get; private set; }

        public static TextDialogUi TextDialog { get; private set; }

        public static FocusManagerUi FocusManager { get; private set; }

        public static WorldMapUi WorldMap { get; private set; }

        public static RandomEncounterUi RandomEncounter { get; private set; }

        public static AlertUi Alert { get; private set; }

        public static HelpUi Help { get; private set; }

        public static ItemCreationUi ItemCreation { get; private set; }

        public static SkillMasteryUi SkillMastery { get; private set; }

        public static UtilityBarUi UtilityBar { get; private set; }

        public static DungeonMasterUi DungeonMaster { get; private set; }

        public static TrackUi Track { get; private set; }

        public static PartyPoolUi PartyPool { get; private set; }

        public static PartyUi Party { get; private set; }

        public static FormationUi Formation { get; private set; }

        public static CampingUi Camping { get; private set; }

        public static PartyQuickviewUi PartyQuickview { get; private set; }

        public static OptionsUi Options { get; private set; }

        public static HelpManagerUi HelpManager { get; private set; }

        public static SliderUi Slider { get; private set; }

        public static WrittenUi Written { get; private set; }

        public static CharmapUi Charmap { get; private set; }

        public static KeyManagerUi KeyManager { get; private set; }

        public static void Startup(GameConfig config)
        {
            Tooltip = Startup<TooltipUi>();
            SaveGame = Startup<SaveGameUi>();
            LoadGame = Startup<LoadGameUi>();
            UtilityBar = Startup<UtilityBarUi>();
            MainMenu = Startup<MainMenuUi>();
            DungeonMaster = Startup<DungeonMasterUi>();
            CharSheet = Startup<CharSheetUi>();
            InGame = Startup<InGameUi>();
            HelpManager = Startup<HelpManagerUi>();
            WorldMapRandomEncounter = Startup<WorldMapRandomEncounterUi>();
            InGameSelect = Startup<InGameSelectUi>();
            ItemCreation = Startup<ItemCreationUi>();
            Party = Startup<PartyUi>();
            TextDialog = Startup<TextDialogUi>();
            RadialMenu = Startup<RadialMenuUi>();
            Dialog = Startup<DialogUi>();
            KeyManager = Startup<KeyManagerUi>();
            Logbook = Startup<LogbookUi>();
            TB = Startup<TBUi>();
            Combat = Startup<CombatUi>();
            PartyPool = Startup<PartyPoolUi>();
            Popup = Startup<PopupUi>();
            Help = Startup<HelpUi>();
            Alert = Startup<AlertUi>();
            TurnBased = Startup<TurnBasedUi>();
            Anim = Startup<AnimUi>();
            Written = Startup<WrittenUi>();
            TownMap = Startup<TownMapUi>();
            WorldMap = Startup<WorldMapUi>();
            PCCreation = Startup<PCCreationUi>();
            Options = Startup<OptionsUi>();
            Camping = Startup<CampingUi>();
            Formation = Startup<FormationUi>();
        }

        private static T Startup<T>() where T : new()
        {
            var system = new T();

            if (system is IDisposable disposable)
            {
                _disposableSystems.Add(disposable);
            }

            if (system is ITimeAwareSystem timeAwareSystem)
            {
                _timeAwareSystems.Add(timeAwareSystem);
            }

            if (system is IResetAwareSystem resetAware)
            {
                _resetAwareSystems.Add(resetAware);
            }

            if (system is ISaveGameAwareUi saveGameSystem)
            {
                _saveSystems.Add(saveGameSystem);
            }

            return system;
        }

        public static void AdvanceTime()
        {
            var now = TimePoint.Now;
            foreach (var timeAwareSystem in _timeAwareSystems)
            {
                timeAwareSystem.AdvanceTime(now);
            }
        }

        [TempleDllLocation(0x10115270)]
        public static void Reset()
        {
            foreach (var system in _resetAwareSystems)
            {
                system.Reset();
            }
        }

        [TempleDllLocation(0x101156b0)]
        public static void HideOpenedWindows(bool hideOptions)
        {
            CharSheet.Hide(0);
            Logbook.Hide();
            TownMap.Hide();
            Camping.Hide();
            Help.Hide();
            if (hideOptions)
            {
                Options.Hide();
            }

            Formation.Hide();
        }

        public static void LoadGameState(SavedUiState uiState)
        {
            foreach (var system in _saveSystems)
            {
                system.LoadGame(uiState);
            }
        }

    }

    public class CharmapUi
    {
    }

    public class WrittenUi
    {
        [TempleDllLocation(0x10160f50)]
        public void Show(GameObjectBody item)
        {
            throw new NotImplementedException();
        }
    }

    public struct SliderParams
    {
        public int Amount { get; set; }

        public int MinAmount { get; set; }

        public int field_8 { get; set; }

        public int field_c { get; set; }

        public Action<GameObjectBody> callback { get; set; }

        public string header { get; set; }

        public string icon { get; set; }

        public int transferType { get; set; }

        public GameObjectBody obj { get; set; }

        public GameObjectBody parent { get; set; }

        public int sum { get; set; }

        public int invIdx { get; set; }

        public Action<int> textDrawCallback { get; set; }
    }

    public class SliderUi
    {
        public void Show(ref SliderParams sliderParams)
        {
            throw new NotImplementedException();
        }
    }

    public class PartyQuickviewUi
    {
    }

    public class FormationUi
    {
        [TempleDllLocation(0x10124de0)]
        public bool IsVisible => false; // TODO

        [TempleDllLocation(0x10124d90)]
        public void Show()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10124dc0)]
        public void Hide()
        {
            Stub.TODO();
        }
    }

    public class TrackUi
    {
        [TempleDllLocation(0x10169e50)]
        public void Show(GameObjectBody tracker)
        {
            throw new NotImplementedException();
        }
    }

    public class DungeonMasterUi
    {
        public void Hide()
        {
            // TODO throw new System.NotImplementedException();
        }
    }

    public class SkillMasteryUi
    {
        [TempleDllLocation(0x10bf3548)]
        private GameObjectBody skillMasteryObj;

        [TempleDllLocation(0x10bf3538)]
        private int skillMasteryIdx;

        [TempleDllLocation(0x10bf34c0)]
        private int[] skillIdx;

        [TempleDllLocation(0x1016a0b0)]
        public void SkillMasteryCallback(GameObjectBody objHnd)
        {
            if (objHnd == skillMasteryObj)
            {
                var bitfield = 0;
                for (var i = 0; i < skillMasteryIdx; ++i)
                {
                    bitfield |= 1 << skillIdx[i];
                }

                GameSystems.D20.D20SendSignal(objHnd, D20DispatcherKey.SIG_Rogue_Skill_Mastery_Init, bitfield);
            }
        }
    }

    public class ItemCreationUi : IResetAwareSystem
    {
        [TempleDllLocation(0x10bedf50)]
        private int ItemCreationType_d;

        [TempleDllLocation(0x1014f180)]
        public bool IsVisible { get; set; } // TODO

        [TempleDllLocation(0x10154ba0)]
        public ItemCreationUi()
        {
            Stub.TODO();
            Reset();
        }

        [TempleDllLocation(0x101536c0)]
        public void CreateItem(GameObjectBody creator, int actionData1)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1014f170)]
        public void Reset()
        {
            ItemCreationType_d = 9;
        }
    }

    public class RandomEncounterUi : ISaveGameAwareUi
    {
        [TempleDllLocation(0x10120d40)]
        public void LoadGame(SavedUiState savedState)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10120bf0)]
        public void SaveGame(SavedUiState savedState)
        {
            throw new NotImplementedException();
        }
    }

    public enum WorldMapMode
    {
        Travel = 0,
        Teleport = 1
    }

    public class WorldMapUi : IResetAwareSystem, ISaveGameAwareUi
    {
        [TempleDllLocation(0x10bef7dc)]
        private bool uiWorldmapIsVisible;

        [TempleDllLocation(0x10159b10)]
        public bool IsVisible => uiWorldmapIsVisible;

        [TempleDllLocation(0x1015f140)]
        public void Show(WorldMapMode mode = WorldMapMode.Travel)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1015e210)]
        public void Hide()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101595d0)]
        [TempleDllLocation(0x10bef7fc)]
        public bool IsMakingTrip { get; private set; }

        [TempleDllLocation(0x101595e0)]
        public void AreaDiscovered(int area)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101596a0)]
        public void OnTravelingToMap(int destMapId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101597b0)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101598b0)]
        public void SaveGame(SavedUiState savedState)
        {
            savedState.WorldmapState = new SavedWorldmapUiState();
            Stub.TODO();
        }

        [TempleDllLocation(0x1015e0f0)]
        public void LoadGame(SavedUiState savedState)
        {
            Stub.TODO();
        }
    }

    public class FocusManagerUi
    {
    }

    public class TextDialogUi
    {
        [TempleDllLocation(0x1014e8e0)]
        public bool IsVisible { get; set; }

        [TempleDllLocation(0x10bec3a0)]
        private WidgetContainer uiTextDialogWnd;

        [TempleDllLocation(0x10bec7e8)]
        private WidgetButton ui_popup_text_okbtn;

        [TempleDllLocation(0x10bec8a4)]
        private WidgetButton ui_popup_text_cancelbtn;

        [TempleDllLocation(0x10bec688)]
        private int dword_10BEC688;

        [TempleDllLocation(0x10bec7c0)]
        private int dword_10BEC7C0;

        [TempleDllLocation(0x10BECD4C)]
        private Rectangle[] stru_10BECD4C = new Rectangle[2];

        [TempleDllLocation(0x10bec960)]
        private string ui_popup_text_body;

        [TempleDllLocation(0x10bec358)]
        private string ui_popup_text_title;

        [TempleDllLocation(0x10bec698)]
        private string uiTextDialogOkBtnText;

        [TempleDllLocation(0x10bec6d8)]
        private string uiTextDialogCancelBtnText;

        [TempleDllLocation(0x10bec7c4)]
        private Action<string, bool> uiTextDialogCallback;

        [TempleDllLocation(0x1014e670)]
        public void UiTextDialogInit(UiCreateNamePacket crNamePkt)
        {
            uiTextDialogWnd.SetPos(
                uiTextDialogWnd.X + crNamePkt.wndX - dword_10BEC688,
                uiTextDialogWnd.X + crNamePkt.wndY - dword_10BEC7C0
            );
            stru_10BECD4C[0].X += crNamePkt.wndX - dword_10BEC688;
            stru_10BECD4C[0].Y += crNamePkt.wndY - dword_10BEC7C0;

            ui_popup_text_okbtn.SetPos(
                ui_popup_text_okbtn.X + crNamePkt.wndX - dword_10BEC688,
                ui_popup_text_okbtn.Y + crNamePkt.wndY - dword_10BEC7C0
            );
            stru_10BECD4C[1].X += crNamePkt.wndX - dword_10BEC688;
            stru_10BECD4C[1].Y += crNamePkt.wndY - dword_10BEC7C0;

            dword_10BEC688 = crNamePkt.wndX;
            dword_10BEC7C0 = crNamePkt.wndY;

            ui_popup_text_body = crNamePkt.bodyText ?? "";
            ui_popup_text_title = crNamePkt.title ?? "";
            uiTextDialogOkBtnText = crNamePkt.okBtnText ?? "";
            uiTextDialogCancelBtnText = crNamePkt.cancelBtnText ?? "";
            uiTextDialogCallback = crNamePkt.callback;
        }

        [TempleDllLocation(0x1014e8a0)]
        public void UiTextDialogShow()
        {
            uiTextDialogWnd.Visible = true;
            uiTextDialogWnd.BringToFront();
        }
    }

    public class UiCreateNamePacket
    {
        public int wndX;
        public int wndY;
        public int type_or_flags;
        public string okBtnText;
        public string cancelBtnText;
        public string title;
        public string bodyText;
        public Action<string, bool> callback;
    }

    public class TownMapUi : IResetAwareSystem, ISaveGameAwareUi
    {
        [TempleDllLocation(0x10be1f74)]
        private bool uiTownmapIsAvailable;

        [TempleDllLocation(0x10be1f28)]
        private bool uiTownmapVisible;

        [TempleDllLocation(0x10128b60)]
        public bool IsVisible => uiTownmapVisible;

        [TempleDllLocation(0x1012bcb0)]
        public void Hide()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1012c6a0)]
        public void Show()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10128420)]
        public bool IsTownMapAvailable
        {
            get
            {
                var result = uiTownmapIsAvailable;
                if (!uiTownmapIsAvailable)
                {
                    var curmap = GameSystems.Map.GetCurrentMapId();
                    if (GameSystems.Map.IsVignetteMap(curmap))
                    {
                        result = uiTownmapIsAvailable;
                    }
                    else
                    {
                        result = true;
                        uiTownmapIsAvailable = true;
                    }
                }

                return result;
            }
        }

        [TempleDllLocation(0x1012bb40)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10128650)]
        public void SaveGame(SavedUiState savedState)
        {
            Stub.TODO();
            savedState.TownmapState = new SavedTownmapUiState();
        }

        [TempleDllLocation(0x101288f0)]
        public void LoadGame(SavedUiState savedState)
        {
            var townmapState = savedState.TownmapState;
            Stub.TODO();
        }
    }

    public class ScrollpaneUi
    {
    }

    public class SlideUi
    {
    }

    public class WorldMapRandomEncounterUi
    {
        public void StartRandomEncounterTimer()
        {
            Stub.TODO();
        }
    }

    public class AnimUi : IResetAwareSystem, ISaveGameAwareUi
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x101737f0)]
        public void BkgAnimTimeEventSchedule(int param0, int param1, int delayMs)
        {
            TimeEvent evt = new TimeEvent(TimeEventType.BkgAnim);
            evt.arg1.int32 = param0;
            evt.arg2.int32 = param1;
            GameSystems.TimeEvent.Schedule(evt, delayMs, out _);
        }

        [TempleDllLocation(0x10173830)]
        public bool BkgAnimTimeeventExpires(TimeEvent evt)
        {
            var eventType = evt.arg1.int32;
            if (eventType == 0)
            {
                // Event type 0: Entire party died
                if (!GameSystems.Combat.AllPcsUnconscious())
                {
                    return true;
                }

                var fadeOutArgs = FadeArgs.Default;
                fadeOutArgs.color = PackedLinearColorA.Black;
                fadeOutArgs.transitionTime = 1.0f;
                fadeOutArgs.fadeSteps = 48;
                GameSystems.GFade.PerformFade(ref fadeOutArgs);
                var referenceTime = TimePoint.Now;
                TimeSpan elapsed;
                do
                {
                    Globals.GameLoop.RenderFrame();
                    GameSystems.GFade.AdvanceTime(TimePoint.Now);
                    elapsed = TimePoint.Now - referenceTime;
                } while (elapsed < TimeSpan.FromSeconds(fadeOutArgs.transitionTime));

                GameSystems.GFade.SetGameOpacity(1.0f);
                Globals.GameLib.KillIronmanSave();

                GameSystems.Movies.MovieQueueAdd(2);
                GameSystems.Movies.MovieQueuePlay();
                Logger.Info("DEATH: Resetting game!");
                Globals.GameLib.Reset();
                UiSystems.Reset();
                UiSystems.MainMenu.Show(0);
            }
            else if (eventType == 1)
            {
                // Event type 1: Reset the game.
                Logger.Info("EndGame: Resetting game!");
                Globals.GameLib.Reset();
                UiSystems.Reset();
                UiSystems.MainMenu.Show(0);
            }
            else
            {
                Logger.Info("AnimUI: anim_ui_bkg_process_callback: ERROR: Failed to match event type!");
                return true;
            }

            var fadeInArgs = FadeArgs.Default;
            fadeInArgs.flags = FadeFlag.FadeIn;
            fadeInArgs.transitionTime = 2.0f;
            fadeInArgs.fadeSteps = 48;
            GameSystems.GFade.PerformFade(ref fadeInArgs);
            return true;
        }

        [TempleDllLocation(0x10173780)]
        public void Reset()
        {
            var evt = new TimeEvent(TimeEventType.AmbientLighting);
            GameSystems.TimeEvent.Schedule(evt, 1, out _);
        }

        public void SaveGame(SavedUiState savedState)
        {
        }

        [TempleDllLocation(0x101737b0)]
        public void LoadGame(SavedUiState savedState)
        {
            // TODO: I think this is not used
            GameSystems.TimeEvent.RemoveAll(TimeEventType.AmbientLighting);
            GameSystems.TimeEvent.Schedule(new TimeEvent(TimeEventType.AmbientLighting), TimeSpan.FromHours(1), out _);
        }

        // TODO: I think this might not be needed anymore...
        [TempleDllLocation(0x101739c0)]
        public void ExpireAmbientLighting(TimeEvent evt)
        {
            GameSystems.LightScheme.SetHourOfDay(12);
            GameSystems.TimeEvent.RemoveAll(TimeEventType.AmbientLighting);
        }
    }

    public class SaveGameUi
    {
        [TempleDllLocation(0x10174e60)]
        public bool IsVisible => false; // TODO

        public void Show(bool unk)
        {
            throw new System.NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x10175a40)]
        public void Hide()
        {
            // TODO throw new System.NotImplementedException();
        }
    }
}