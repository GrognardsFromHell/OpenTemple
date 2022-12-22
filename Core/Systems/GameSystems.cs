using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.TabFiles;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.ActionBar;
using OpenTemple.Core.Systems.AI;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.Clipping;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.FogOfWar;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Movies;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Protos;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Sound;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.Teleport;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.Systems.Vfx;
using OpenTemple.Core.Systems.Waypoints;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;
using HotkeySystem = OpenTemple.Core.Hotkeys.HotkeySystem;

namespace OpenTemple.Core.Systems;

public static class GameSystems
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static bool _resetting = false;

    private static Guid _moduleGuid;
    private static string? _moduleArchivePath;
    private static string? _moduleDirPath;

    public static VagrantSystem Vagrant { get; private set; }
    public static HotkeySystem Hotkeys { get; private set; }
    public static DescriptionSystem Description { get; private set; }
    public static ItemEffectSystem ItemEffect { get; private set; }
    public static TeleportSystem Teleport { get; private set; }
    public static SectorSystem Sector { get; private set; }
    public static RandomSystem Random { get; private set; }
    public static CritterSystem Critter { get; private set; }
    public static ScriptNameSystem ScriptName { get; private set; }
    public static PortraitSystem Portrait { get; private set; }
    public static SkillSystem Skill { get; private set; }
    public static FeatSystem Feat { get; private set; }
    public static D20StatSystem Stat { get; private set; }
    public static ScriptSystem Script { get; private set; }
    public static LevelSystem Level { get; private set; }
    public static D20System D20 { get; private set; }
    public static MapSystem Map { get; private set; }
    public static SpellSystem Spell { get; private set; }
    public static ScrollSystem Scroll { get; private set; }
    public static LocationSystem Location { get; private set; }
    public static LightSystem Light { get; private set; }
    public static TileSystem Tile { get; private set; }
    public static ONameSystem OName { get; private set; }
    public static ObjectNodeSystem ObjectNode { get; private set; }

    public static ObjectSystem Object { get; private set; }
    public static ProtoSystem Proto { get; private set; }

    public static MapObjectSystem MapObject { get; private set; }
    public static RaycastSystem Raycast { get; private set; }
    public static MapSectorSystem MapSector { get; private set; }
    public static SectorVisibilitySystem SectorVisibility { get; private set; }
    public static TextBubbleSystem TextBubble { get; private set; }
    public static TextFloaterSystem TextFloater { get; private set; }
    public static JumpPointSystem JumpPoint { get; private set; }
    public static TerrainSystem Terrain { get; private set; }
    public static ClippingSystem Clipping { get; private set; }
    public static HeightSystem Height { get; private set; }
    public static GMeshSystem GMesh { get; private set; }
    public static PathNodeSystem PathNode { get; private set; }
    public static LightSchemeSystem LightScheme { get; private set; }
    public static PlayerSystem Player { get; private set; }
    public static AreaSystem Area { get; private set; }
    public static DialogSystem Dialog { get; private set; }
    public static SoundMapSystem SoundMap { get; private set; }
    public static SoundGameSystem SoundGame { get; private set; }
    public static ItemSystem Item { get; private set; }
    public static WeaponSystem Weapon { get; private set; }
    public static CombatSystem Combat { get; private set; }
    public static TimeEventSystem TimeEvent { get; private set; }
    public static RumorSystem Rumor { get; private set; }
    public static QuestSystem Quest { get; private set; }
    public static AiSystem AI { get; private set; }
    public static AnimSystem Anim { get; private set; }
    public static ReputationSystem Reputation { get; private set; }
    public static ReactionSystem Reaction { get; private set; }
    public static TileScriptSystem TileScript { get; private set; }
    public static SectorScriptSystem SectorScript { get; private set; }
    public static WaypointSystem Waypoint { get; private set; }
    public static InvenSourceSystem InvenSource { get; private set; }
    public static TownMapSystem TownMap { get; private set; }
    public static MovieSystem Movies { get; private set; }
    public static BrightnessSystem Brightness { get; private set; }
    public static GFadeSystem GFade { get; private set; }
    public static AntiTeleportSystem AntiTeleport { get; private set; }
    public static TrapSystem Trap { get; private set; }
    public static MonsterGenSystem MonsterGen { get; private set; }
    public static PartySystem Party { get; private set; }
    public static D20LoadSaveSystem D20LoadSave { get; private set; }
    public static GameInitSystem GameInit { get; private set; }
    public static ObjFadeSystem ObjFade { get; private set; }
    public static DeitySystem Deity { get; private set; }
    public static UiArtManagerSystem UiArtManager { get; private set; }
    public static ParticleSysSystem ParticleSys { get; private set; }
    public static CheatsSystem Cheats { get; private set; }
    public static SecretdoorSystem Secretdoor { get; private set; }
    public static MapFoggingSystem MapFogging { get; private set; }
    public static RandomEncounterSystem RandomEncounter { get; private set; }
    public static ObjectEventSystem ObjectEvent { get; private set; }
    public static FormationSystem Formation { get; private set; }
    public static ItemHighlightSystem ItemHighlight { get; private set; }
    public static PathXSystem PathX { get; private set; }
    public static PathXRenderSystem PathXRender { get; private set; }
    public static AASSystem AAS { get; private set; }
    public static HelpSystem Help { get; private set; }

    public static VfxSystem Vfx { get; private set; }

    public static RollHistorySystem RollHistory { get; private set; }

    public static PoisonSystem Poison { get; private set; }

    public static DiseaseSystem Disease { get; private set; }

    private static List<(long, string)> _timing = new();

    private static List<IGameSystem> _initializedSystems = new();

    // All systems that want to listen to map events
    public static IEnumerable<IMapCloseAwareGameSystem> MapCloseAwareSystems
        => _initializedSystems.OfType<IMapCloseAwareGameSystem>();

    public static IEnumerable<ITimeAwareSystem> TimeAwareSystems
        => _initializedSystems.OfType<ITimeAwareSystem>();

    public static IEnumerable<IModuleAwareSystem> ModuleAwareSystems
        => _initializedSystems.OfType<IModuleAwareSystem>();

    public static IEnumerable<IResetAwareSystem> ResetAwareSystems
        => _initializedSystems.OfType<IResetAwareSystem>();

    [TempleDllLocation(0x10307054)]
    public static bool ModuleLoaded { get; private set; }

    public static void Init()
    {
        Logger.Info("Loading game systems");

        var config = Globals.Config;

        ModuleLoaded = false;

        InitializeFonts();

        if (!config.SkipLegal)
        {
            PlayLegalMovies();
        }

        using var loadingScreen = new LoadingScreen(Tig.MainWindow, Tig.RenderingDevice, Tig.ShapeRenderer2d);
        InitializeSystems(loadingScreen);
    }

    public static void InitializeFonts()
    {
        var lang = GetLanguage();
        if (lang == "en")
        {
            Logger.Info("Assuming english fonts");
            Tig.Fonts.FontIsEnglish = true;
        }

        foreach (var filename in Tig.FS.ListDirectory("fonts"))
        {
            if (!filename.EndsWith(".otf") && !filename.EndsWith(".ttf"))
            {
                continue;
            }

            var path = $"fonts/{filename}";
            Logger.Info("Adding font '{0}'", path);
            Tig.RenderingDevice.TextEngine.AddFont(path);
        }

        Tig.RenderingDevice.TextEngine.LogLoadedFontFamilies();

        Tig.Fonts.LoadAllFrom("art/interface/fonts");
        Tig.Fonts.PushFont("priory-12", 12);
    }

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public static void Shutdown()
    {
        Logger.Info("Unloading game systems");

        // This really shouldn't be here, but thanks to ToEE's stupid
        // dependency graph, this call criss-crosses across almost all systems
        Map?.CloseMap();

        AAS?.Dispose();

        Vfx?.Dispose();
        Vfx = null!;
        PathXRender?.Dispose();
        PathXRender = null!;
        PathX?.Dispose();
        PathX = null!;
        ItemHighlight?.Dispose();
        ItemHighlight = null!;
        Formation?.Dispose();
        Formation = null!;
        ObjectEvent?.Dispose();
        ObjectEvent = null!;
        RandomEncounter?.Dispose();
        RandomEncounter = null!;
        MapFogging?.Dispose();
        MapFogging = null!;
        Secretdoor?.Dispose();
        Secretdoor = null!;
        Cheats?.Dispose();
        Cheats = null!;
        UiArtManager?.Dispose();
        UiArtManager = null!;
        Deity?.Dispose();
        Deity = null!;
        ObjFade?.Dispose();
        ObjFade = null!;
        GameInit?.Dispose();
        GameInit = null!;
        D20LoadSave?.Dispose();
        D20LoadSave = null!;
        Party?.Dispose();
        Party = null!;
        MonsterGen?.Dispose();
        MonsterGen = null!;
        Trap?.Dispose();
        Trap = null!;
        AntiTeleport?.Dispose();
        AntiTeleport = null!;
        GFade?.Dispose();
        GFade = null!;
        Brightness?.Dispose();
        Brightness = null!;
        Movies?.Dispose();
        Movies = null!;
        TownMap?.Dispose();
        TownMap = null!;
        InvenSource?.Dispose();
        InvenSource = null!;
        Waypoint?.Dispose();
        Waypoint = null!;
        SectorScript?.Dispose();
        SectorScript = null!;
        TileScript?.Dispose();
        TileScript = null!;
        Reaction?.Dispose();
        Reaction = null!;
        Reputation?.Dispose();
        Reputation = null!;
        AI?.Dispose();
        AI = null!;
        Quest?.Dispose();
        Quest = null!;
        Rumor?.Dispose();
        Rumor = null!;
        Combat?.Dispose();
        Combat = null!;
        TimeEvent?.Dispose();
        TimeEvent = null!;
        Anim?.Dispose();
        Anim = null!;
        Item?.Dispose();
        Item = null!;
        Weapon = null!;
        SoundGame?.Dispose();
        SoundGame = null!;
        SoundMap?.Dispose();
        SoundMap = null!;
        Dialog?.Dispose();
        Dialog = null!;
        Area?.Dispose();
        Area = null!;
        Player?.Dispose();
        Player = null!;
        LightScheme?.Dispose();
        LightScheme = null!;
        PathNode?.Dispose();
        PathNode = null!;
        GMesh?.Dispose();
        GMesh = null!;
        Height?.Dispose();
        Height = null!;
        Terrain?.Dispose();
        Terrain = null!;
        Clipping?.Dispose();
        Clipping = null!;
        JumpPoint?.Dispose();
        JumpPoint = null!;
        TextFloater?.Dispose();
        TextFloater = null!;
        TextBubble?.Dispose();
        TextBubble = null!;
        SectorVisibility?.Dispose();
        SectorVisibility = null!;
        MapSector?.Dispose();
        MapSector = null!;
        Object?.Dispose();
        Object = null!;
        ObjectNode?.Dispose();
        ObjectNode = null!;
        OName?.Dispose();
        OName = null!;
        Tile?.Dispose();
        Tile = null!;
        Light?.Dispose();
        Light = null!;
        Location?.Dispose();
        Location = null!;
        Scroll?.Dispose();
        Scroll = null!;
        Map?.Dispose();
        Map = null!;
        ParticleSys?.Dispose();
        ParticleSys = null!;
        D20?.Dispose();
        D20 = null!;
        Raycast?.Dispose();
        Raycast = null!;
        MapObject?.Dispose();
        MapObject = null!;
        Level?.Dispose();
        Level = null!;
        Script?.Dispose();
        Script = null!;
        Stat?.Dispose();
        Stat = null!;
        Spell?.Dispose();
        Spell = null!;
        Feat?.Dispose();
        Feat = null!;
        Skill?.Dispose();
        Skill = null!;
        Portrait?.Dispose();
        Portrait = null!;
        ScriptName?.Dispose();
        ScriptName = null!;
        Critter?.Dispose();
        Critter = null!;
        Random?.Dispose();
        Random = null!;
        Sector?.Dispose();
        Sector = null!;
        Teleport?.Dispose();
        Teleport = null!;
        ItemEffect?.Dispose();
        ItemEffect = null!;
        Description?.Dispose();
        Description = null!;
        Vagrant?.Dispose();
        Vagrant = null!;
        Help?.Dispose();
        Help = null!;
        Proto?.Dispose();
        Proto = null!;
        AAS = null!;
        Hotkeys = null!;

        _resetting = default;
        _moduleGuid = default;
        _moduleArchivePath = default;
        _moduleDirPath = default;
        ModuleLoaded = false;

        _timing.Clear();
        _initializedSystems.Clear();
    }

    /*
Call this before loading a game. Use not yet known.
TODO I do NOT think this is used, should be checked. Seems like leftovers from even before arcanum
*/
    public static void DestroyPlayerObject()
    {
        // TODO gameSystemInitTable.DestroyPlayerObject();
    }

    // Ends the game, resets the game systems and returns to the main menu.
    public static void EndGame()
    {
        // TODO return gameSystemInitTable.EndGame();
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x11E726AC)]
    public static TimePoint LastAdvanceTime { get; private set; }

    public static void AdvanceTime()
    {
        var now = TimePoint.Now;

        // This is used from somewhere in the object system
        LastAdvanceTime = now;

        foreach (var system in TimeAwareSystems)
        {
            try
            {
                system.AdvanceTime(now);
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

    public static void LoadModule(string moduleName, bool editorMode = false)
    {
        if (!editorMode)
        {
            var saveDir = Globals.GameFolders.CurrentSaveFolder;

            if (Directory.Exists(saveDir))
            {
                try
                {
                    Directory.Delete(saveDir, true);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Unable to clean current savegame folder from previous data: {saveDir}", e);
                }
            }
        }

        foreach (var system in ModuleAwareSystems)
        {
            system.LoadModule();
        }
    }

    public static void UnloadModule()
    {
        throw new Exception("Currently unsupported");
    }

    public static void ResetGame()
    {
        Logger.Info("Resetting game systems...");

        _resetting = true;

        var currentSaveFolder = Globals.GameFolders.CurrentSaveFolder;
        if (Directory.Exists(currentSaveFolder))
        {
            Directory.Delete(currentSaveFolder, true);
        }

        Directory.CreateDirectory(currentSaveFolder);

        foreach (var system in _initializedSystems)
        {
            if (system is IResetAwareSystem resetSystem)
            {
                Logger.Debug("Resetting game system {0}", system.GetType().Name);
                resetSystem.Reset();
            }
        }

        _resetting = false;
    }

    public static bool IsResetting()
    {
        return _resetting;
    }

    /// <summary>
    /// Gets the language of the current toee installation (i.e. "en")
    /// </summary>
    private static string GetLanguage()
    {
        if (!Tig.FS.FileExists("mes/language.mes"))
        {
            return "en";
        }

        var content = Tig.FS.ReadMesFile("mes/language.mes");
        if (content[1].Length == 0)
        {
            return "en";
        }

        return content[1];
    }

    private static void PlayLegalMovies()
    {
        MovieSystem.PlayMovie("movies/AtariLogo.bik", null);
        MovieSystem.PlayMovie("movies/TroikaLogo.bik", null);
        MovieSystem.PlayMovie("movies/WotCLogo.bik", null);
    }

    public static void InitializeSystems(ILoadingProgress loadingScreen)
    {
        loadingScreen.Message = "Loading...";

        Vfx = InitializeSystem(loadingScreen, () => new VfxSystem());

        AAS = InitializeSystem(loadingScreen, () => new AASSystem(Tig.FS, Tig.MdfFactory, new AasRenderer(
            Tig.RenderingDevice,
            Tig.ShapeRenderer2d,
            Tig.ShapeRenderer3d
        )));

        // Loading Screen ID: 2
        loadingScreen.Progress = 1 / 79.0f;
        Vagrant = InitializeSystem(loadingScreen, () => new VagrantSystem());
        Hotkeys = InitializeSystem(loadingScreen, () => new HotkeySystem());
        // Loading Screen ID: 2
        loadingScreen.Progress = 2 / 79.0f;
        Description = InitializeSystem(loadingScreen, () => new DescriptionSystem());
        loadingScreen.Progress = 3 / 79.0f;
        ItemEffect = InitializeSystem(loadingScreen, () => new ItemEffectSystem());
        // Loading Screen ID: 3
        loadingScreen.Progress = 4 / 79.0f;
        Teleport = InitializeSystem(loadingScreen, () => new TeleportSystem());
        // Loading Screen ID: 4
        loadingScreen.Progress = 5 / 79.0f;
        Sector = InitializeSystem(loadingScreen, () => new SectorSystem());
        // Loading Screen ID: 5
        loadingScreen.Progress = 6 / 79.0f;
        Random = InitializeSystem(loadingScreen, () => new RandomSystem());
        // Loading Screen ID: 6
        loadingScreen.Progress = 7 / 79.0f;
        Critter = InitializeSystem(loadingScreen, () => new CritterSystem());
        // Loading Screen ID: 7
        loadingScreen.Progress = 8 / 79.0f;
        ScriptName = InitializeSystem(loadingScreen, () => new ScriptNameSystem());
        // Loading Screen ID: 8
        loadingScreen.Progress = 9 / 79.0f;
        Portrait = InitializeSystem(loadingScreen, () => new PortraitSystem());
        // Loading Screen ID: 9
        loadingScreen.Progress = 10 / 79.0f;
        Skill = InitializeSystem(loadingScreen, () => new SkillSystem());
        // Loading Screen ID: 10
        loadingScreen.Progress = 11 / 79.0f;
        Feat = InitializeSystem(loadingScreen, () => new FeatSystem());
        // Loading Screen ID: 11
        loadingScreen.Progress = 12 / 79.0f;
        // Spell system was moved down because it HAS to be loaded after the map system
        loadingScreen.Progress = 13 / 79.0f;
        Stat = InitializeSystem(loadingScreen, () => new D20StatSystem());
        // Loading Screen ID: 12
        loadingScreen.Progress = 14 / 79.0f;
        Script = InitializeSystem(loadingScreen, () => new ScriptSystem());
        loadingScreen.Progress = 15 / 79.0f;
        Level = InitializeSystem(loadingScreen, () => new LevelSystem());
        loadingScreen.Progress = 16 / 79.0f;
        D20 = InitializeSystem(loadingScreen, () => new D20System());
        Help = new HelpSystem();
        // Loading Screen ID: 1
        loadingScreen.Progress = 17 / 79.0f;
        Map = InitializeSystem(loadingScreen, () => new MapSystem(D20));
        // NOTE: we have to move this here because the spell system can ONLY load the spells after the map is loaded!
        Spell = InitializeSystem(loadingScreen, () => new SpellSystem());
        /* START Former Map Subsystems */
        loadingScreen.Progress = 18 / 79.0f;
        Location = InitializeSystem(loadingScreen, () => new LocationSystem());
        loadingScreen.Progress = 19 / 79.0f;
        Scroll = InitializeSystem(loadingScreen, () => new ScrollSystem());
        loadingScreen.Progress = 20 / 79.0f;
        Light = InitializeSystem(loadingScreen, () => new LightSystem());
        loadingScreen.Progress = 21 / 79.0f;
        Tile = InitializeSystem(loadingScreen, () => new TileSystem());
        loadingScreen.Progress = 22 / 79.0f;
        OName = InitializeSystem(loadingScreen, () => new ONameSystem());
        loadingScreen.Progress = 23 / 79.0f;
        ObjectNode = InitializeSystem(loadingScreen, () => new ObjectNodeSystem());
        loadingScreen.Progress = 24 / 79.0f;
        Object = InitializeSystem(loadingScreen, () => new ObjectSystem());
        loadingScreen.Progress = 25 / 79.0f;
        Proto = InitializeSystem(loadingScreen, () => new ProtoSystem(Object));
        loadingScreen.Progress = 26 / 79.0f;
        Raycast = InitializeSystem(loadingScreen, () => new RaycastSystem());
        MapObject = InitializeSystem(loadingScreen, () => new MapObjectSystem());
        loadingScreen.Progress = 27 / 79.0f;
        MapSector = InitializeSystem(loadingScreen, () => new MapSectorSystem());
        loadingScreen.Progress = 28 / 79.0f;
        SectorVisibility = InitializeSystem(loadingScreen, () => new SectorVisibilitySystem());
        loadingScreen.Progress = 29 / 79.0f;
        TextBubble = InitializeSystem(loadingScreen, () => new TextBubbleSystem());
        loadingScreen.Progress = 30 / 79.0f;
        TextFloater = InitializeSystem(loadingScreen, () => new TextFloaterSystem());
        loadingScreen.Progress = 31 / 79.0f;
        JumpPoint = InitializeSystem(loadingScreen, () => new JumpPointSystem());
        loadingScreen.Progress = 32 / 79.0f;
        Clipping = InitializeSystem(loadingScreen, () => new ClippingSystem(Tig.RenderingDevice));
        Terrain = InitializeSystem(loadingScreen, () => new TerrainSystem(Tig.RenderingDevice,
            Tig.ShapeRenderer2d));
        loadingScreen.Progress = 33 / 79.0f;
        Height = InitializeSystem(loadingScreen, () => new HeightSystem());
        loadingScreen.Progress = 34 / 79.0f;
        GMesh = InitializeSystem(loadingScreen, () => new GMeshSystem(AAS));
        loadingScreen.Progress = 35 / 79.0f;
        PathNode = InitializeSystem(loadingScreen, () => new PathNodeSystem());
        /* END Former Map Subsystems */

        loadingScreen.Progress = 36 / 79.0f;
        LightScheme = InitializeSystem(loadingScreen, () => new LightSchemeSystem());
        loadingScreen.Progress = 37 / 79.0f;
        Player = InitializeSystem(loadingScreen, () => new PlayerSystem());
        loadingScreen.Progress = 38 / 79.0f;
        Area = InitializeSystem(loadingScreen, () => new AreaSystem());
        loadingScreen.Progress = 39 / 79.0f;
        Dialog = InitializeSystem(loadingScreen, () => new DialogSystem());
        loadingScreen.Progress = 40 / 79.0f;
        SoundMap = InitializeSystem(loadingScreen, () => new SoundMapSystem());
        loadingScreen.Progress = 41 / 79.0f;
        SoundGame = InitializeSystem(loadingScreen, () => new SoundGameSystem());
        loadingScreen.Progress = 42 / 79.0f;
        Item = InitializeSystem(loadingScreen, () => new ItemSystem());
        Weapon = InitializeSystem(loadingScreen, () => new WeaponSystem());
        loadingScreen.Progress = 43 / 79.0f;
        Combat = InitializeSystem(loadingScreen, () => new CombatSystem());
        loadingScreen.Progress = 44 / 79.0f;
        TimeEvent = InitializeSystem(loadingScreen, () => new TimeEventSystem());
        loadingScreen.Progress = 45 / 79.0f;
        Rumor = InitializeSystem(loadingScreen, () => new RumorSystem());
        loadingScreen.Progress = 46 / 79.0f;
        Quest = InitializeSystem(loadingScreen, () => new QuestSystem());
        loadingScreen.Progress = 47 / 79.0f;
        AI = InitializeSystem(loadingScreen, () => new AiSystem());
        loadingScreen.Progress = 48 / 79.0f;
        Anim = InitializeSystem(loadingScreen, () => new AnimSystem());
        loadingScreen.Progress = 49 / 79.0f;
        loadingScreen.Progress = 50 / 79.0f;
        Reputation = InitializeSystem(loadingScreen, () => new ReputationSystem());
        loadingScreen.Progress = 51 / 79.0f;
        Reaction = InitializeSystem(loadingScreen, () => new ReactionSystem());
        loadingScreen.Progress = 52 / 79.0f;
        TileScript = InitializeSystem(loadingScreen, () => new TileScriptSystem());
        loadingScreen.Progress = 53 / 79.0f;
        SectorScript = InitializeSystem(loadingScreen, () => new SectorScriptSystem());
        loadingScreen.Progress = 54 / 79.0f;

        // NOTE: This system is only used in worlded (rendering related)
        Waypoint = InitializeSystem(loadingScreen, () => new WaypointSystem());
        loadingScreen.Progress = 55 / 79.0f;

        InvenSource = InitializeSystem(loadingScreen, () => new InvenSourceSystem());
        loadingScreen.Progress = 56 / 79.0f;
        TownMap = InitializeSystem(loadingScreen, () => new TownMapSystem());
        loadingScreen.Progress = 57 / 79.0f;
        Movies = InitializeSystem(loadingScreen, () => new MovieSystem());
        loadingScreen.Progress = 58 / 79.0f;
        Brightness = InitializeSystem(loadingScreen, () => new BrightnessSystem());
        loadingScreen.Progress = 59 / 79.0f;
        GFade = InitializeSystem(loadingScreen, () => new GFadeSystem());
        loadingScreen.Progress = 60 / 79.0f;
        AntiTeleport = InitializeSystem(loadingScreen, () => new AntiTeleportSystem());
        loadingScreen.Progress = 61 / 79.0f;
        Trap = InitializeSystem(loadingScreen, () => new TrapSystem());
        loadingScreen.Progress = 62 / 79.0f;
        MonsterGen = InitializeSystem(loadingScreen, () => new MonsterGenSystem());
        loadingScreen.Progress = 63 / 79.0f;
        Party = InitializeSystem(loadingScreen, () => new PartySystem());
        loadingScreen.Progress = 64 / 79.0f;
        D20LoadSave = InitializeSystem(loadingScreen, () => new D20LoadSaveSystem());
        loadingScreen.Progress = 65 / 79.0f;
        GameInit = InitializeSystem(loadingScreen, () => new GameInitSystem());
        loadingScreen.Progress = 66 / 79.0f;
        // NOTE: The "ground" system has been superseded by the terrain system
        loadingScreen.Progress = 67 / 79.0f;
        ObjFade = InitializeSystem(loadingScreen, () => new ObjFadeSystem());
        loadingScreen.Progress = 68 / 79.0f;
        Deity = InitializeSystem(loadingScreen, () => new DeitySystem());
        loadingScreen.Progress = 69 / 79.0f;
        UiArtManager = InitializeSystem(loadingScreen, () => new UiArtManagerSystem());
        loadingScreen.Progress = 70 / 79.0f;
        ParticleSys = InitializeSystem(loadingScreen,
            () => new ParticleSysSystem());
        loadingScreen.Progress = 71 / 79.0f;
        Cheats = InitializeSystem(loadingScreen, () => new CheatsSystem());
        loadingScreen.Progress = 72 / 79.0f;
        loadingScreen.Progress = 73 / 79.0f;
        Secretdoor = InitializeSystem(loadingScreen, () => new SecretdoorSystem());
        loadingScreen.Progress = 74 / 79.0f;
        MapFogging = InitializeSystem(loadingScreen, () => new MapFoggingSystem(Tig.RenderingDevice));
        loadingScreen.Progress = 75 / 79.0f;
        RandomEncounter = InitializeSystem(loadingScreen, () => new RandomEncounterSystem());
        loadingScreen.Progress = 76 / 79.0f;
        ObjectEvent = InitializeSystem(loadingScreen, () => new ObjectEventSystem());
        loadingScreen.Progress = 77 / 79.0f;
        Formation = InitializeSystem(loadingScreen, () => new FormationSystem());
        loadingScreen.Progress = 78 / 79.0f;
        ItemHighlight = InitializeSystem(loadingScreen, () => new ItemHighlightSystem());
        loadingScreen.Progress = 79 / 79.0f;
        PathX = InitializeSystem(loadingScreen, () => new PathXSystem());
        PathXRender = InitializeSystem(loadingScreen, () => new PathXRenderSystem());
        RollHistory = InitializeSystem(loadingScreen, () => new RollHistorySystem());
        Poison = InitializeSystem(loadingScreen, () => new PoisonSystem());
        Disease = InitializeSystem(loadingScreen, () => new DiseaseSystem());

        _timing.Sort((a, b) => b.CompareTo(a));
        foreach (var (time, system) in _timing.Take(10))
        {
            Console.WriteLine(system + ": " + time + "ms");
        }
    }

    private static T InitializeSystem<T>(ILoadingProgress loadingScreen, Func<T> factory)
    {
        var sw = Stopwatch.StartNew();

        Logger.Info($"Loading game system {typeof(T).Name}");
        loadingScreen.Update();

        var system = factory();

        if (system is IGameSystem gameSystem)
        {
            _initializedSystems.Add(gameSystem);
        }

        _timing.Add((sw.ElapsedMilliseconds, typeof(T).Name));

        return system;
    }

    public static void LoadGameState(SavedGameState gameState)
    {
        foreach (var system in _initializedSystems)
        {
            if (system is ISaveGameAwareGameSystem sgSystem)
            {
                sgSystem.LoadGame(gameState);
            }
        }
    }

    public static void SaveGameState(SavedGameState gameState)
    {
        foreach (var system in _initializedSystems)
        {
            if (system is ISaveGameAwareGameSystem sgSystem)
            {
                sgSystem.SaveGame(gameState);
            }
        }
    }
}

public interface IMapCloseAwareGameSystem
{
    void CloseMap();
}

public interface ISaveGameAwareGameSystem
{
    void SaveGame(SavedGameState savedGameState);
    void LoadGame(SavedGameState savedGameState);
}

public interface IResetAwareSystem
{
    void Reset();
}

public interface IModuleAwareSystem
{
    void LoadModule();
    void UnloadModule();
}

// TODO: Can probably be removed
public class ItemEffectSystem : IGameSystem, IModuleAwareSystem
{
    [TempleDllLocation(0x100864d0)]
    public ItemEffectSystem()
    {
        // TODO Could not find a use of this system
    }

    [TempleDllLocation(0x10086550)]
    public void Dispose()
    {
        // TODO Could not find a use of this system
    }

    [TempleDllLocation(0x10086560)]
    public void LoadModule()
    {
        // TODO Could not find a use of this system
    }

    [TempleDllLocation(0x100865c0)]
    public void UnloadModule()
    {
        // TODO Could not find a use of this system
    }
}

public class SectorSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
{
    [TempleDllLocation(0x10AB7470)]
    public int SectorLimitX { get; private set; }

    [TempleDllLocation(0x10AB7448)]
    public int SectorLimitY { get; private set; }

    [TempleDllLocation(0x10081bc0)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x10081bb0)]
    public void Reset()
    {
        // NOTE: Vanilla used to have a sector times object here which we don't use.
    }

    [TempleDllLocation(0x10081be0)]
    public void SaveGame(SavedGameState savedGameState)
    {
        // NOTE: Vanilla used to have a sector times object here which we don't use.
    }

    [TempleDllLocation(0x10081d20)]
    public void LoadGame(SavedGameState savedGameState)
    {
        // NOTE: Vanilla used to have a sector times object here which we don't use.
    }

    [TempleDllLocation(0x10081940)]
    public bool SetLimits(ulong limitX, ulong limitY)
    {
        if (SectorLimitX > 0x4000000 || SectorLimitY > 0x4000000)
        {
            return false;
        }

        SectorLimitX = (int) limitX;
        SectorLimitY = (int) limitY;
        return true;
    }
}

// TODO: This entire system may also be unused because old scripts are not used anymore
public class ScriptNameSystem : IGameSystem, IModuleAwareSystem
{
    private readonly Dictionary<int, string> _scriptIndex = new();

    private readonly Dictionary<int, string> _scriptModuleIndex = new();

    private readonly Dictionary<int, string> _pythonScriptIndex = new();

    private readonly Dictionary<int, string> _dialogIndex = new();

    private static readonly Regex ScriptNamePattern = new(@"^(\d+).*\.scr$");

    private static readonly Regex PythonScriptNamePattern = new(@"^py(\d+).*\.py$");

    private static readonly Regex DialogNamePattern = new(@"^(\d+).*\.dlg$");

    [TempleDllLocation(0x1007e000)]
    public ScriptNameSystem()
    {
        foreach (var (scriptId, filename) in EnumerateScripts())
        {
            if (IsGlobalScriptId(scriptId))
            {
                if (!_scriptIndex.TryAdd(scriptId, filename))
                {
                    throw new Exception($"Duplicate script file number: {scriptId}");
                }
            }
        }
    }

    private static IEnumerable<Tuple<int, string>> EnumerateScripts()
    {
        foreach (var filename in Tig.FS.ListDirectory("scr"))
        {
            var match = ScriptNamePattern.Match(filename);
            if (!match.Success)
            {
                continue;
            }

            var scriptId = int.Parse(match.Groups[1].Value);
            yield return Tuple.Create(scriptId, filename);
        }
    }

    private static IEnumerable<Tuple<int, string>> EnumeratePythonScripts()
    {
        foreach (var filename in Tig.FS.ListDirectory("scr"))
        {
            var match = PythonScriptNamePattern.Match(filename);
            if (!match.Success)
            {
                continue;
            }

            var scriptId = int.Parse(match.Groups[1].Value);
            yield return Tuple.Create(scriptId, filename);
        }
    }

    private static IEnumerable<Tuple<int, string>> EnumerateDialogs()
    {
        foreach (var filename in Tig.FS.ListDirectory("dlg"))
        {
            var match = DialogNamePattern.Match(filename);
            if (!match.Success)
            {
                continue;
            }

            var scriptId = int.Parse(match.Groups[1].Value);
            yield return Tuple.Create(scriptId, "dlg/" + filename);
        }
    }

    [TempleDllLocation(0x1007e0c0)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x1007e0e0)]
    public void LoadModule()
    {
        foreach (var (scriptId, filename) in EnumerateScripts())
        {
            if (IsModuleScriptId(scriptId))
            {
                if (!_scriptModuleIndex.TryAdd(scriptId, filename))
                {
                    throw new Exception($"Duplicate module script file number: {scriptId}");
                }
            }
        }

        foreach (var (scriptId, filename) in EnumeratePythonScripts())
        {
            if (!_pythonScriptIndex.TryAdd(scriptId, filename))
            {
                throw new Exception($"Duplicate Python script file number: {scriptId}");
            }
        }

        foreach (var (scriptId, filename) in EnumerateDialogs())
        {
            if (!_dialogIndex.TryAdd(scriptId, filename))
            {
                throw new Exception($"Duplicate dialog file number: {scriptId}");
            }
        }
    }

    private bool IsModuleScriptId(int scriptId) => scriptId >= 1 && scriptId < 1000;

    private bool IsGlobalScriptId(int scriptId) => scriptId >= 1000;

    [TempleDllLocation(0x1007e1b0)]
    public void UnloadModule()
    {
        _scriptModuleIndex.Clear();
        _pythonScriptIndex.Clear();
        _dialogIndex.Clear();
    }

    /// <summary>
    /// Gets the path for a legacy .scr script file.
    /// </summary>
    [TempleDllLocation(0x1007e1d0)]
    public string? GetScriptPath(int scriptId)
    {
        string filename;
        if (IsModuleScriptId(scriptId))
        {
            filename = _scriptModuleIndex.GetValueOrDefault(scriptId);
        }
        else if (IsGlobalScriptId(scriptId))
        {
            filename = _scriptIndex.GetValueOrDefault(scriptId);
        }
        else
        {
            return null;
        }

        if (filename != null)
        {
            return "scr/" + filename;
        }

        return null;
    }

    [TempleDllLocation(0x1007e270)]
    public bool TryGetDialogScriptPath(int scriptId, [MaybeNullWhen(false)] out string scriptPath)
    {
        return _dialogIndex.TryGetValue(scriptId, out scriptPath);
    }
}

public class PortraitSystem : IGameSystem
{
    public void Dispose()
    {
    }
}

public enum SkillMessageId
{
    AlchemySubstanceIdentified = 0,
    AlchemySubstanceAlreadyKnown = 1,
    AlchemySubstanceCannotBeIdentified = 2,
    AlchemyNotEnoughMoney = 3,
    AlchemyCheckFailed = 4,
    UseMagicDeviceActivateBlindlyFailed = 5,
    UseMagicDeviceMishap = 6,
    UseMagicDeviceUseScrollFailed = 7,
    UseMagicDeviceUseWandFailed = 8,
    UseMagicDeviceEmulateAbilityScoreFailed = 9,
    DecipherScriptInvalidItem = 10,
    DecipherScriptAlreadyKnown = 11,
    DecipherScriptAlreadyTriedToday = 12,
    DecipherScriptFailure = 13,
    DecipherScriptSuccess = 14,
    SpellcraftSuccess = 15,
    SpellcraftFailure = 16,
    SpellcraftFailureRaiseRank = 17,
    SpellcraftSchoolProhibited = 18
}

public class LevelupPacket
{
    public int flags;
    public Stat classCode;
    public Stat abilityScoreRaised = (Stat) (-1);
    public List<FeatId> feats = new();

    /// array keeping track of how many skill pts were added to each skill
    public Dictionary<SkillId, int> skillPointsAdded = new();

    public List<int> spellEnums = new();
    public int spellEnumToRemove; // spell removed (for Sorcerers)
};

public enum MapType : uint
{
    None,
    StartMap,
    ShoppingMap,
    TutorialMap,
    ArenaMap // new in Temple+
}

public class TileSystem : IGameSystem
{
    public void Dispose()
    {
    }

    [TempleDllLocation(0x100ab8b0)]
    public bool MapTileHasSinksFlag(locXY loc)
    {
        if (GetMapTile(loc, out var tile))
        {
            return tile.flags.HasFlag(TileFlags.TF_Sinks);
        }

        return false;
    }

    [TempleDllLocation(0x100ab810)]
    public bool GetMapTile(locXY loc, out SectorTile tile)
    {
        using var lockedSector = new LockedMapSector(loc);
        var sector = lockedSector.Sector;
        if (sector == null)
        {
            tile = default;
            return false;
        }

        var tileIndex = sector.GetTileOffset(loc);
        tile = sector.tilePkt.tiles[tileIndex];
        return true;
    }

    [TempleDllLocation(0x100ac570)]
    public bool IsBlockingOldVersion(locXY location, bool regardSinks = false)
    {
        if (!GetMapTile(location, out var tile))
        {
            return false;
        }

        if (regardSinks)
        {
            // TODO: This was just borked in vanilla...
            return false;
        }

        var result = tile.flags & (TileFlags.TF_Blocks | TileFlags.TF_CanFlyOver);
        return result != 0;
    }

    [TempleDllLocation(0x100ab890)]
    public bool MapTileIsSoundProof(locXY location)
    {
        if (!GetMapTile(location, out var tile))
        {
            return false;
        }

        return tile.flags.HasFlag(TileFlags.TF_SoundProof);
    }

    [TempleDllLocation(0x100ab8d0)]
    public bool MapTileIsOutdoors(locXY location)
    {
        if (!GetMapTile(location, out var tile))
        {
            return true;
        }

        return (tile.flags & TileFlags.TF_Indoor) == 0;
    }

    [TempleDllLocation(0x100ab870)]
    public TileMaterial GetMaterial(locXY location)
    {
        if (!GetMapTile(location, out var tile))
        {
            return TileMaterial.Grass;
        }

        return tile.material;
    }

    [TempleDllLocation(0x100ac5c0)]
    public bool IsSubtileBlocked(LocAndOffsets location, bool allowWading)
    {
        if (!GetMapTile(location.location, out var mapTile))
        {
            return false;
        }

        var subtile = new Subtile(location);
        var subtileX = subtile.X % 3;
        var subtileY = subtile.Y % 3;

        var blockingFlag = SectorTile.GetBlockingFlag(subtileX, subtileY);
        var flyoverFlag = SectorTile.GetFlyOverFlag(subtileX, subtileY);

        var tileFlags = mapTile.flags;
        return (tileFlags & blockingFlag) != 0
               || (tileFlags & flyoverFlag) != 0
               || !allowWading && MapTileHasSinksFlag(location.location);
    }
}

public class ONameSystem : IGameSystem
{
    public void Dispose()
    {
    }
}

public class ObjectNodeSystem : IGameSystem
{
    public void Dispose()
    {
    }
}

public class JumpPointSystem : IGameSystem, IModuleAwareSystem
{
    private readonly Dictionary<int, JumpPoint> _jumpPoints = new();

    public void Dispose()
    {
    }

    [TempleDllLocation(0x100bde20)]
    public bool TryGet(int id, [MaybeNullWhen(false)] out string name, out int mapId, out locXY location)
    {
        if (_jumpPoints.TryGetValue(id, out var jumpPoint))
        {
            name = jumpPoint.Name;
            mapId = jumpPoint.MapId;
            location = jumpPoint.Location;
            return true;
        }

        name = null;
        mapId = 0;
        location = locXY.Zero;
        return false;
    }

    public void LoadModule()
    {
        _jumpPoints.Clear();

        TabFile.ParseFile("rules/jumppoint.tab", (in TabFileRecord record) =>
        {
            var id = record[0].GetInt();
            var name = record[1].AsString();
            var mapId = record[2].GetInt();
            var x = record[3].GetInt();
            var y = record[4].GetInt();
            _jumpPoints[id] = new JumpPoint(
                id, name, mapId, new locXY(x, y)
            );
        });
    }

    public void UnloadModule()
    {
        _jumpPoints.Clear();
    }

    private struct JumpPoint
    {
        public int Id { get; }
        public string Name { get; }
        public int MapId { get; }
        public locXY Location { get; }

        public JumpPoint(int id, string name, int mapId, locXY location)
        {
            Id = id;
            Name = name;
            MapId = mapId;
            Location = location;
        }
    }
}

public class HeightSystem : IGameSystem
{
    public void Dispose()
    {
    }

    public sbyte GetDepth(LocAndOffsets location)
    {
        // TODO: Implement HSD
        return 0;
    }

    [TempleDllLocation(0x100a8970)]
    public void SetDataDirs(string dataDir, string saveDir)
    {
        // TODO
    }

    public void Clear()
    {
        // TODO
    }
}

public class GMeshSystem : IGameSystem
{
    public GMeshSystem(AASSystem aas)
    {
    }

    public void Dispose()
    {
    }

    public void Load(string dataDir)
    {
        // TODO
    }
}

public class PlayerSystem : IGameSystem, IResetAwareSystem
{
    [TempleDllLocation(0x10aa9508)]
    private GameObject? _player;

    [TempleDllLocation(0x10aa94e8)]
    private ObjectId _playerId;

    [TempleDllLocation(0x1006ede0)]
    public PlayerSystem()
    {
        _playerId = ObjectId.CreateNull();
    }

    [TempleDllLocation(0x1006ee40)]
    public void Dispose()
    {
        _player = null;
        _playerId = ObjectId.CreateNull();
    }

    [TempleDllLocation(0x1006ee00)]
    public void Reset()
    {
        if (_player != null)
        {
            _player = null;
            _playerId = ObjectId.CreateNull();
        }
    }

    [TempleDllLocation(0x1006eef0)]
    public bool PlayerObj_Destroy()
    {
        if (_player != null)
        {
            GameSystems.Object.Destroy(_player);
            _player = null;
            _playerId = ObjectId.CreateNull();
            return true;
        }

        return false;
    }

    [TempleDllLocation(0x1006ee80)]
    public void Restore()
    {
        if (!_playerId.IsNull)
        {
            _player = GameSystems.Object.GetObject(_playerId);
        }
        else
        {
            _player = null;
        }
    }
}

public class RumorSystem : IGameSystem
{
    private readonly Dictionary<int, string> _rumorLinesMaleNpcMalePc;
    private readonly Dictionary<int, string> _rumorLinesMaleNpcFemalePc;
    private readonly Dictionary<int, string> _rumorLinesFemaleNpcMalePc;
    private readonly Dictionary<int, string> _rumorLinesFemaleNpcFemalePc;

    [TempleDllLocation(0x1005f960)]
    public RumorSystem()
    {
        _rumorLinesMaleNpcMalePc = Tig.FS.ReadMesFile("mes/game_rd_npc_m2m.mes");
        _rumorLinesMaleNpcFemalePc = Tig.FS.ReadMesFile("mes/game_rd_npc_m2f.mes");
        _rumorLinesFemaleNpcMalePc = Tig.FS.ReadMesFile("mes/game_rd_npc_f2m.mes");
        _rumorLinesFemaleNpcFemalePc = Tig.FS.ReadMesFile("mes/game_rd_npc_f2f.mes");
    }

    [TempleDllLocation(0x1005f9d0)]
    public void Dispose()
    {
    }

    private void AddRumor(int rumorId)
    {
    }

    [TempleDllLocation(0x1005fc20)]
    public void Add(GameObject critter, int rumorId)
    {
        if (critter.IsPC())
        {
            AddRumor(rumorId);
            GameUiBridge.AddRumor(rumorId);
        }
    }

    [TempleDllLocation(0x1005fb70)]
    [TemplePlusLocation("python_integration_obj.cpp:290")]
    public bool TryFindRumor(GameObject pc, GameObject npc, out int rumorId)
    {
        rumorId = GameSystems.Script.ExecuteScript<int>("rumor_control", "find_rumor", pc, npc);
        if (rumorId == -1)
        {
            return false;
        }

        rumorId /= 10; // No idea why this is hardcoded here
        return true;
    }

    [TempleDllLocation(0x1005fa60)]
    public bool TryGetRumorNpcLine(GameObject pc, GameObject npc, int rumorId, out string lineText)
    {
        Dictionary<int, string> rumorLines;
        if (npc.GetGender() == Gender.Male)
        {
            rumorLines = pc.GetGender() == Gender.Male ? _rumorLinesMaleNpcMalePc : _rumorLinesMaleNpcFemalePc;
        }
        else
        {
            rumorLines = pc.GetGender() == Gender.Male ? _rumorLinesFemaleNpcMalePc : _rumorLinesFemaleNpcFemalePc;
        }

        var key = 10 * rumorId + 1;
        return rumorLines.TryGetValue(key, out lineText);
    }
}

public class SectorScriptSystem : IGameSystem
{
    public void Dispose()
    {
    }
}

public class TownMapSystem : IGameSystem, IModuleAwareSystem, IResetAwareSystem
{
    [TempleDllLocation(0x10AA3340)]
    private byte[] dword_10AA3340;

    [TempleDllLocation(0x10aa3344)]
    private int numTimesToRead;

    [TempleDllLocation(0x10aa32f0)]
    private int dword_10AA32F0;

    [TempleDllLocation(0x10aa32fc)]
    private int dword_10AA32FC;

    public void Dispose()
    {
    }

    [TempleDllLocation(0x10051cd0)]
    public void LoadModule()
    {
        // TODO Townmap
    }

    [TempleDllLocation(0x10052130)]
    public void UnloadModule()
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x10052100)]
    public void Reset()
    {
        dword_10AA3340 = null;
        numTimesToRead = 0;
        dword_10AA32F0 = 0;
        dword_10AA32FC = 0;
    }

    [TempleDllLocation(0x10052430)]
    public void sub_10052430(locXY location)
    {
        // TODO This entire function / system is unused and an Arkanum leftover I believe
        using var lockedSector = new LockedMapSector(location);
        if (!lockedSector.IsValid)
        {
            return;
        }

        var townmapInfo = lockedSector.Sector.townmapInfo;
        if (townmapInfo == 0)
        {
            return;
        }

        Stub.TODO();
    }

    [TempleDllLocation(0x100521b0)]
    public void Flush()
    {
        Stub.TODO();
    }
}

public class BrightnessSystem : IGameSystem
{
    public void Dispose()
    {
    }
}

// TODO This system is not used and should be removed
public class AntiTeleportSystem : IGameSystem, IModuleAwareSystem
{
    public void Dispose()
    {
    }

    public void LoadModule()
    {
    }

    public void UnloadModule()
    {
    }
}

public class D20LoadSaveSystem : IGameSystem, ISaveGameAwareGameSystem
{
    public void Dispose()
    {
    }

    [TempleDllLocation(0x1004fbd0)]
    public void LoadGame(SavedGameState savedGameState)
    {
        var d20State = savedGameState.D20State;
        GameSystems.D20.Initiative.Load(d20State);
        GameSystems.D20.Actions.Load(d20State);
        GameSystems.D20.Hotkeys.LoadHotkeys(d20State.Hotkeys);
        GameSystems.D20.Combat.Load(d20State);
        GameSystems.Combat.LoadBrawlState(d20State.BrawlState);
    }

    [TempleDllLocation(0x1004fb70)]
    public void SaveGame(SavedGameState savedGameState)
    {
        var d20State = new SavedD20State();
        GameSystems.D20.Initiative.Save(d20State);
        GameSystems.D20.Actions.Save(d20State);
        d20State.Hotkeys = GameSystems.D20.Hotkeys.SaveHotkeys();
        GameSystems.D20.Combat.Save(d20State);
        d20State.BrawlState = GameSystems.Combat.SaveBrawlState();
        savedGameState.D20State = d20State;
    }
}

public enum PortraitVariant
{
    Big = 0,
    Medium,
    Small,
    SmallGrey,
    MediumGrey
}

public class UiArtManagerSystem : IGameSystem, IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly Dictionary<int, string> _portraitPaths;
    private readonly Dictionary<int, string> _inventoryPaths;
    private readonly Dictionary<int, string> _genericPaths;
    private readonly Dictionary<int, string> _genericLargePaths;

    [TempleDllLocation(0x1004a610)]
    public UiArtManagerSystem()
    {
        var portraitsMes = Tig.FS.ReadMesFile("art/interface/portraits/portraits.mes");
        _portraitPaths = portraitsMes.ToDictionary(
            kp => kp.Key,
            kp => "art/interface/portraits/" + kp.Value
        );

        var inventoryMes = Tig.FS.ReadMesFile("art/interface/inventory/inventory.mes");
        _inventoryPaths = inventoryMes.ToDictionary(
            kp => kp.Key,
            kp => "art/interface/inventory/" + kp.Value
        );

        var genericMes = Tig.FS.ReadMesFile("art/interface/generic/generic.mes");
        _genericPaths = genericMes.ToDictionary(
            kp => kp.Key,
            kp => "art/interface/generic/" + kp.Value
        );

        var genericLargeMes = Tig.FS.ReadMesFile("art/interface/generic/generic_large.mes");
        _genericLargePaths = genericLargeMes.ToDictionary(
            kp => kp.Key,
            kp => "art/interface/generic/" + kp.Value
        );
    }

    [TempleDllLocation(0x1004a250)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x1004a360)]
    public string? GetPortraitPath(int portraitId, PortraitVariant variant)
    {
        if (portraitId % 10 != 0)
        {
            Logger.Warn("Trying to get invalid portrait id: {0}", portraitId);
            return GetPortraitPath(0, variant);
        }

        int key = portraitId + (int) variant;
        if (_portraitPaths.TryGetValue(key, out var result))
        {
            return result;
        }

        return null;
    }

    [TempleDllLocation(0x1004a360)]
    public string? GetInventoryIconPath(int artId)
    {
        return _inventoryPaths.GetValueOrDefault(artId);
    }

    [TempleDllLocation(0x1004a4e0)]
    public string? GetGenericTiledImagePath(int artId)
    {
        return _genericLargePaths.GetValueOrDefault(artId);
    }

    [TempleDllLocation(0x1004a360)]
    public string? GetGenericPath(int artId)
    {
        return _genericPaths.GetValueOrDefault(artId);
    }
}

public class CheatsSystem : IGameSystem
{
    public void Dispose()
    {
    }

    void LevelupCritter(GameObject critter)
    {
        if (GameSystems.Critter.CanLevelUp(critter))
            return;
        var curLevel = GameSystems.Critter.GetEffectiveLevel(critter);
        var xpReq = GameSystems.Level.GetExperienceForLevel(curLevel + 1);

        var curXp = critter.GetInt32(obj_f.critter_experience);
        if (xpReq > curXp)
            GameSystems.Critter.AwardXp(critter, xpReq - curXp);
    }

    [TempleDllLocation(0x100495B0)]
    public void LevelupParty()
    {
        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            if (GameSystems.Party.IsAiFollower(partyMember))
                continue;

            LevelupCritter(partyMember);
        }
    }
}

public static class SecretdoorExtensions
{
    [TempleDllLocation(0x10046470)]
    public static bool IsSecretDoor(this GameObject portal)
    {
        return portal.GetSecretDoorFlags().HasFlag(SecretDoorFlag.SECRET_DOOR);
    }

    public static bool IsUndetectedSecretDoor(this GameObject portal)
    {
        var flags = portal.GetSecretDoorFlags();
        return flags.HasFlag(SecretDoorFlag.SECRET_DOOR) && !flags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND);
    }
}

[Flags]
public enum MapTerrain
{
    Scrub = 0,
    RoadFlag = 1,
    Forest = 2,
    Swamp = 4,
    Riverside = 6
}

public enum SleepStatus
{
    Safe = 0,
    Dangerous = 1,
    Impossible = 2,
    PassTimeOnly = 3
}

public class ItemHighlightSystem : IGameSystem, IResetAwareSystem, ITimeAwareSystem
{
    [TempleDllLocation(0x10788cec)]
    [TempleDllLocation(0x1001d7d0)]
    public bool ShowHighlights { get; private set; }

    public ItemHighlightSystem()
    {
    }

    public void Dispose()
    {
    }

    [TempleDllLocation(0x100431d0)]
    public void Reset()
    {
        ShowHighlights = false;
    }

    [TempleDllLocation(0x100431f0)]
    public void AdvanceTime(TimePoint time)
    {
        ShowHighlights = GameSystems.Hotkeys.IsHeld(InGameHotKey.ObjectHighlight);
    }
}
