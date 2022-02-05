using System;
using System.Collections.Generic;
using OpenTemple.Core.Config;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;
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
using OpenTemple.Core.Ui.TownMap;
using OpenTemple.Core.Ui.UtilityBar;
using OpenTemple.Core.Ui.WorldMap;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui;

public static class UiSystems
{
    private static readonly List<IDisposable> _disposableSystems = new();

    private static readonly List<ITimeAwareSystem> _timeAwareSystems = new();

    private static readonly List<IResetAwareSystem> _resetAwareSystems = new();

    private static readonly List<ISaveGameAwareUi> _saveSystems = new();

    public static MainMenuUi MainMenu { get; private set; }

    public static GameView GameView { get; private set; }

    // UiMM is unused

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

    public static TextEntryUi TextEntry { get; private set; }

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
        GameView = Startup(new GameView(
            Tig.MainWindow,
            Tig.RenderingDevice,
            Globals.Config.Rendering
        ));
        Tooltip = Startup<TooltipUi>();
        SaveGame = Startup<SaveGameUi>();
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
        TextEntry = Startup<TextEntryUi>();
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
        RandomEncounter = Startup<RandomEncounterUi>();
    }

    private static T Startup<T>() where T : new()
    {
        return Startup(new T());
    }

    private static T Startup<T>(T system)
    {
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
            try
            {
                timeAwareSystem.AdvanceTime(now);
            }
            catch (Exception e)
            {
                if (!ErrorReporting.ReportException(e))
                {
                    throw;
                }
            }
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

    public static void SaveGameState(SavedUiState uiState)
    {
        foreach (var system in _saveSystems)
        {
            system.SaveGame(uiState);
        }
    }

    public static void DisposeAll()
    {
        _disposableSystems.DisposeAndClear();
        _timeAwareSystems.Clear();
        _resetAwareSystems.Clear();
        _saveSystems.Clear();

        GameView = null;
        Tooltip = null;
        SaveGame = null;
        UtilityBar = null;
        MainMenu = null;
        DungeonMaster = null;
        CharSheet = null;
        InGame = null;
        HelpManager = null;
        WorldMapRandomEncounter = null;
        InGameSelect = null;
        ItemCreation = null;
        Party = null;
        TextEntry = null;
        RadialMenu = null;
        Dialog = null;
        KeyManager = null;
        Logbook = null;
        TB = null;
        Combat = null;
        PartyPool = null;
        Popup = null;
        Help = null;
        Alert = null;
        TurnBased = null;
        Anim = null;
        Written = null;
        TownMap = null;
        WorldMap = null;
        PCCreation = null;
        Options = null;
        Camping = null;
        Formation = null;
        RandomEncounter = null;
    }
}

public class CharmapUi
{
}

public class WrittenUi
{
    [TempleDllLocation(0x10160f50)]
    public void Show(GameObject item)
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

    public Action<GameObject> callback { get; set; }

    public string header { get; set; }

    public string icon { get; set; }

    public int transferType { get; set; }

    public GameObject obj { get; set; }

    public GameObject parent { get; set; }

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
    public void Show(GameObject tracker)
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
    private GameObject skillMasteryObj;

    [TempleDllLocation(0x10bf3538)]
    private int skillMasteryIdx;

    [TempleDllLocation(0x10bf34c0)]
    private int[] skillIdx;

    [TempleDllLocation(0x1016a0b0)]
    public void SkillMasteryCallback(GameObject objHnd)
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
    public void CreateItem(GameObject creator, int actionData1)
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
        // I think this is not used
        Stub.TODO();
    }

    [TempleDllLocation(0x10120bf0)]
    public void SaveGame(SavedUiState savedState)
    {
        // I think this is not used
        Stub.TODO();
    }

    [TempleDllLocation(0x1016ceb0)]
    [TempleDllLocation(0x10BF37A4)]
    public bool DontAskToExitMap { get; set; }


    public void AskToExitMap()
    {
        throw new NotImplementedException();
        // TODO v6 = uiRndEncExitWidgets /*0x10bf3550*/.wnd.widgetId;
        // TODO uiRndEncExitWndVisible /*0x10bf3788*/ = 1;
        // TODO WidgetSetHidden /*0x101f9100*/(v6, 0);
    }
}

public class FocusManagerUi
{
}

public class ScrollpaneUi
{
}

public class SlideUi
{
}

public class WorldMapRandomEncounterUi
{
    [TempleDllLocation(0x1016d210)]
    [TempleDllLocation(0x10BF3784)]
    public bool IsActive { get; set; }

    public void StartRandomEncounterTimer()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x1016d0a0)]
    public void Show()
    {
        throw new NotImplementedException();
    }
}

public class AnimUi : IResetAwareSystem, ISaveGameAwareUi
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x101737f0)]
    public void BkgAnimTimeEventSchedule(int eventType, int delayMs)
    {
        TimeEvent evt = new TimeEvent(TimeEventType.BkgAnim);
        evt.arg1.int32 = eventType;
        GameSystems.TimeEvent.Schedule(evt, delayMs, out _);
    }

    [TempleDllLocation(0x10173830)]
    public bool BkgAnimTimeEventExpires(TimeEvent evt)
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