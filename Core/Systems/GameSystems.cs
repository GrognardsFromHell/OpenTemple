using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.TabFiles;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Particles;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems.AI;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.Clipping;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.FogOfWar;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Protos;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.Teleport;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.Systems.Waypoints;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;
using Path = System.IO.Path;

namespace OpenTemple.Core.Systems
{
    public static class GameSystems
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static bool mResetting = false;

        private static Guid mModuleGuid;
        private static string mModuleArchivePath;
        private static string mModuleDirPath;

        public static VagrantSystem Vagrant { get; private set; }
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

        private static List<IGameSystem> _initializedSystems = new List<IGameSystem>();

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

            var lang = GetLanguage();
            if (lang == "en")
            {
                Logger.Info("Assuming english fonts");
                Tig.Fonts.FontIsEnglish = true;
            }

            if (!config.SkipLegal)
            {
                PlayLegalMovies();
            }

            foreach (var filename in Tig.FS.ListDirectory("fonts/*.ttf"))
            {
                var path = $"fonts/{filename}";
                Logger.Info("Adding TTF font '{0}'", path);
                Tig.RenderingDevice.GetTextEngine().AddFont(path);
            }

            foreach (var filename in Tig.FS.ListDirectory("fonts/*.otf"))
            {
                var path = $"fonts/{filename}";
                Logger.Info("Adding OTF font '{0}'", path);
                Tig.RenderingDevice.GetTextEngine().AddFont(path);
            }

            Tig.Fonts.LoadAllFrom("art/interface/fonts");
            Tig.Fonts.PushFont("priory-12", 12);

            using var loadingScreen = new LoadingScreen(Tig.RenderingDevice, Tig.ShapeRenderer2d);
            InitializeSystems(loadingScreen);
        }

        public static void Shutdown()
        {
            Logger.Info("Unloading game systems");

            // This really shouldn't be here, but thanks to ToEE's stupid
            // dependency graph, this call criss-crosses across almost all systems
            Map?.CloseMap();

            Vfx?.Dispose();
            Vfx = null;
            PathXRender?.Dispose();
            PathXRender = null;
            PathX?.Dispose();
            PathX = null;
            ItemHighlight?.Dispose();
            ItemHighlight = null;
            Formation?.Dispose();
            Formation = null;
            ObjectEvent?.Dispose();
            ObjectEvent = null;
            RandomEncounter?.Dispose();
            RandomEncounter = null;
            MapFogging?.Dispose();
            MapFogging = null;
            Secretdoor?.Dispose();
            Secretdoor = null;
            Cheats?.Dispose();
            Cheats = null;
            UiArtManager?.Dispose();
            UiArtManager = null;
            Deity?.Dispose();
            Deity = null;
            ObjFade?.Dispose();
            ObjFade = null;
            GameInit?.Dispose();
            GameInit = null;
            D20LoadSave?.Dispose();
            D20LoadSave = null;
            Party?.Dispose();
            Party = null;
            MonsterGen?.Dispose();
            MonsterGen = null;
            Trap?.Dispose();
            Trap = null;
            AntiTeleport?.Dispose();
            AntiTeleport = null;
            GFade?.Dispose();
            GFade = null;
            Brightness?.Dispose();
            Brightness = null;
            Movies?.Dispose();
            Movies = null;
            TownMap?.Dispose();
            TownMap = null;
            InvenSource?.Dispose();
            InvenSource = null;
            Waypoint?.Dispose();
            Waypoint = null;
            SectorScript?.Dispose();
            SectorScript = null;
            TileScript?.Dispose();
            TileScript = null;
            Reaction?.Dispose();
            Reaction = null;
            Reputation?.Dispose();
            Reputation = null;
            Anim?.Dispose();
            Anim = null;
            AI?.Dispose();
            AI = null;
            Quest?.Dispose();
            Quest = null;
            Rumor?.Dispose();
            Rumor = null;
            TimeEvent?.Dispose();
            TimeEvent = null;
            Combat?.Dispose();
            Combat = null;
            Item?.Dispose();
            Item = null;
            Weapon = null;
            SoundGame?.Dispose();
            SoundGame = null;
            SoundMap?.Dispose();
            SoundMap = null;
            Dialog?.Dispose();
            Dialog = null;
            Area?.Dispose();
            Area = null;
            Player?.Dispose();
            Player = null;
            LightScheme?.Dispose();
            LightScheme = null;
            PathNode?.Dispose();
            PathNode = null;
            GMesh?.Dispose();
            GMesh = null;
            Height?.Dispose();
            Height = null;
            Terrain?.Dispose();
            Terrain = null;
            Clipping?.Dispose();
            Clipping = null;
            JumpPoint?.Dispose();
            JumpPoint = null;
            TextFloater?.Dispose();
            TextFloater = null;
            TextBubble?.Dispose();
            TextBubble = null;
            SectorVisibility?.Dispose();
            SectorVisibility = null;
            MapSector?.Dispose();
            MapSector = null;
            Object?.Dispose();
            Object = null;
            Proto?.Dispose();
            Proto = null;
            ObjectNode?.Dispose();
            ObjectNode = null;
            OName?.Dispose();
            OName = null;
            Tile?.Dispose();
            Tile = null;
            Light?.Dispose();
            Light = null;
            Location?.Dispose();
            Location = null;
            Scroll?.Dispose();
            Scroll = null;
            Map?.Dispose();
            Map = null;
            ParticleSys?.Dispose();
            ParticleSys = null;
            D20?.Dispose();
            D20 = null;
            Raycast?.Dispose();
            Raycast = null;
            MapObject?.Dispose();
            MapObject = null;
            Level?.Dispose();
            Level = null;
            Script?.Dispose();
            Script = null;
            Stat?.Dispose();
            Stat = null;
            Spell?.Dispose();
            Spell = null;
            Feat?.Dispose();
            Feat = null;
            Skill?.Dispose();
            Skill = null;
            Portrait?.Dispose();
            Portrait = null;
            ScriptName?.Dispose();
            ScriptName = null;
            Critter?.Dispose();
            Critter = null;
            Random?.Dispose();
            Random = null;
            Sector?.Dispose();
            Sector = null;
            Teleport?.Dispose();
            Teleport = null;
            ItemEffect?.Dispose();
            ItemEffect = null;
            Description?.Dispose();
            Description = null;
            Vagrant?.Dispose();
            Vagrant = null;
            Help?.Dispose();
            Help = null;
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
                    ErrorReporting.ReportException(e);
                }
            }
        }

        public static void LoadModule(string moduleName)
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

            // TODO Get mModuleGuid
            var preprocessor = new MapMobilePreprocessor(mModuleGuid);

            // Preprocess mob files for each map before we load the first map
            foreach (var entry in Tig.FS.ListDirectory("maps"))
            {
                var path = $"maps/{entry}";
                if (!Tig.FS.DirectoryExists(path))
                {
                    continue;
                }

                preprocessor.Preprocess(path);
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

            mResetting = true;

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

            mResetting = false;
        }

        public static bool IsResetting()
        {
            return mResetting;
        }

        /// <summary>
        /// Gets the language of the current toee installation (i.e. "en")
        /// </summary>
        private static string GetLanguage()
        {
            try
            {
                var content = Tig.FS.ReadMesFile("mes/language.mes");
                if (content[1].Length == 0)
                {
                    return "en";
                }

                return content[1];
            }
            catch
            {
                return "en";
            }
        }

        private static void PlayLegalMovies()
        {
            Movies.PlayMovie("movies/AtariLogo.bik", 0, 0, 0);
            Movies.PlayMovie("movies/TroikaLogo.bik", 0, 0, 0);
            Movies.PlayMovie("movies/WotCLogo.bik", 0, 0, 0);
        }

        public static void InitializeSystems(ILoadingProgress loadingScreen)
        {
            loadingScreen.Message = "Loading...";

            AAS = new AASSystem(Tig.FS, Tig.MdfFactory, new AasRenderer(
                Tig.RenderingDevice,
                Tig.ShapeRenderer2d,
                Tig.ShapeRenderer3d
            ));

            // Loading Screen ID: 2
            loadingScreen.Progress = 1 / 79.0f;
            Vagrant = InitializeSystem(loadingScreen, () => new VagrantSystem());
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
            Proto = InitializeSystem(loadingScreen, () => new ProtoSystem());
            loadingScreen.Progress = 26 / 79.0f;
            Raycast = new RaycastSystem();
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
            Weapon = new WeaponSystem();
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
                () => new ParticleSysSystem(Tig.RenderingDevice.GetCamera()));
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
            PathXRender = new PathXRenderSystem();
            Vfx = new VfxSystem();
            RollHistory = InitializeSystem(loadingScreen, () => new RollHistorySystem());
            Poison = new PoisonSystem();
            Disease = new DiseaseSystem();
        }

        private static T InitializeSystem<T>(ILoadingProgress loadingScreen, Func<T> factory) where T : IGameSystem
        {
            Logger.Info($"Loading game system {typeof(T).Name}");
            Tig.SystemEventPump.PumpSystemEvents();
            loadingScreen.Update();

            var system = factory();

            _initializedSystems.Add(system);

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

    public class ActionBar
    {
        public int advTimeFuncIdx;
        public int flags;
        public float pulseVal;
        public Action resetCallback;
        public float startDist;
        public float endDist = 1.0f;
        public float combatDepletionSpeed = 1.0f;
        public float pulsePhaseRadians;
        public float pulseMean;
        public float pulseAmplitude;
        public float pulseTime;
    }

    public class VagrantSystem : IGameSystem, ITimeAwareSystem
    {
        [TempleDllLocation(0x10AB7588)]
        private readonly List<ActionBar> _activeBars = new List<ActionBar>();

        [TempleDllLocation(0x10ab7580)]
        private TimePoint _timeRef;

        [TempleDllLocation(0x10086ae0)]
        public VagrantSystem()
        {
            _timeRef = TimePoint.Now;
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10086cb0)]
        public void AdvanceTime(TimePoint newsystime)
        {
            var timeElapsedSec = (float) (TimePoint.Now - _timeRef).TotalSeconds;
            _timeRef = TimePoint.Now;

            for (var i = _activeBars.Count - 1; i >= 0; i--)
            {
                var activeBar = _activeBars[i];
                if ((activeBar.flags & 1) != 0)
                {
                    switch (activeBar.advTimeFuncIdx)
                    {
                        case 0:
                            VagrantDebugFunc0(activeBar, timeElapsedSec);
                            break;
                        case 1:
                            VagrantAdvancePulseVal(activeBar, timeElapsedSec);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        [TempleDllLocation(0x10086dd0)]
        private void VagrantDebugFunc0(ActionBar barPkt, float timeElapsedSec)
        {
            var newPulseVal = timeElapsedSec * barPkt.combatDepletionSpeed + barPkt.pulseVal;
            barPkt.pulseVal = newPulseVal;
            if (barPkt.combatDepletionSpeed <= 0.0f)
            {
                if (newPulseVal >= barPkt.endDist)
                {
                    return;
                }

                barPkt.pulseVal = barPkt.endDist;
            }
            else
            {
                if (newPulseVal <= barPkt.endDist)
                {
                    return;
                }

                barPkt.pulseVal = barPkt.endDist;
            }

            barPkt.resetCallback?.Invoke();
            barPkt.flags &= ~1;
        }

        [TempleDllLocation(0x10086c60)]
        private void VagrantAdvancePulseVal(ActionBar barPkt, float timeElapsedSec)
        {
            var newRot = timeElapsedSec / barPkt.pulseTime + barPkt.pulsePhaseRadians;
            barPkt.pulsePhaseRadians = Angles.NormalizeRadians(newRot);
            barPkt.pulseVal = MathF.Cos(barPkt.pulsePhaseRadians) * barPkt.pulseAmplitude + barPkt.pulseMean;
        }

        [TempleDllLocation(0x10086d10)]
        public ActionBar AllocateActionBar()
        {
            var result = new ActionBar();
            _activeBars.Add(result);
            return result;
        }

        [TempleDllLocation(0x10086c50)]
        public void ActionbarUnsetFlag1(ActionBar barPkt)
        {
            barPkt.flags &= ~1;
        }

        [TempleDllLocation(0x10086c40)]
        public bool ActionBarIsFlag1Set(ActionBar barPkt)
        {
            return (barPkt.flags & 1) != 0;
        }

        [TempleDllLocation(0x10086c30)]
        public float ActionBarGetPulseMinVal(ActionBar barPkt)
        {
            return barPkt.pulseVal;
        }

        [TempleDllLocation(0x10086b70)]
        public void ActionBarSetMovementValues(ActionBar bar, float startDist, float endDist,
            float uiCombatDepletionSpeed)
        {
            bar.startDist = startDist;
            bar.pulseVal = startDist;
            bar.advTimeFuncIdx = 0;
            bar.endDist = endDist;
            if (startDist <= endDist)
            {
                bar.combatDepletionSpeed = uiCombatDepletionSpeed;
                bar.flags |= 1;
            }
            else
            {
                bar.flags |= 1;
                bar.combatDepletionSpeed = -uiCombatDepletionSpeed;
            }
        }

        [TempleDllLocation(0x10086bc0)]
        public void ActionBarSetPulseValues(ActionBar pkt, float pulseMinVal, float pulseMaxVal, float pulseTime)
        {
            pkt.pulseVal = pulseMinVal;
            pkt.advTimeFuncIdx = 1;
            pkt.pulseTime = pulseTime;
            pkt.flags |= 1;
            pkt.pulseAmplitude = (pulseMaxVal - pulseMinVal) * 0.5f;
            pkt.pulseMean = pulseMinVal + pkt.pulseAmplitude;
        }
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
        private readonly Dictionary<int, string> _scriptIndex = new Dictionary<int, string>();

        private readonly Dictionary<int, string> _scriptModuleIndex = new Dictionary<int, string>();

        private readonly Dictionary<int, string> _pythonScriptIndex = new Dictionary<int, string>();

        private readonly Dictionary<int, string> _dialogIndex = new Dictionary<int, string>();

        private static readonly Regex ScriptNamePattern = new Regex(@"^(\d+).*\.scr$");

        private static readonly Regex PythonScriptNamePattern = new Regex(@"^py(\d+).*\.py$");

        private static readonly Regex DialogNamePattern = new Regex(@"^(\d+).*\.dlg$");

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
        public string GetScriptPath(int scriptId)
        {
            string filename;
            if (IsModuleScriptId(scriptId))
            {
                filename = _scriptModuleIndex.GetValueOrDefault(scriptId, null);
            }
            else if (IsGlobalScriptId(scriptId))
            {
                filename = _scriptIndex.GetValueOrDefault(scriptId, null);
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
        public bool TryGetDialogScriptPath(int scriptId, out string scriptPath)
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
        public List<FeatId> feats = new List<FeatId>();

        /// array keeping track of how many skill pts were added to each skill
        public Dictionary<SkillId, int> skillPointsAdded = new Dictionary<SkillId, int>();

        public List<int> spellEnums = new List<int>();
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
        private Dictionary<int, JumpPoint> _jumpPoints;

        public void Dispose()
        {
        }

        [TempleDllLocation(0x100bde20)]
        public bool TryGet(int id, out string name, out int mapId, out locXY location)
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
            _jumpPoints = new Dictionary<int, JumpPoint>();

            TabFile.ParseFile("rules/jumppoint.tab", record =>
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
            _jumpPoints = null;
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
        private GameObjectBody _player;

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

    public class SoundMapSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x1006df90)]
        public int GetPortalSoundEffect(GameObjectBody portal, PortalSoundEffect type)
        {
            if (portal == null || portal.type != ObjectType.portal)
            {
                return -1;
            }

            return portal.GetInt32(obj_f.sound_effect) + (int) type;
        }

        [TempleDllLocation(0x1006e0b0)]
        public int CombatFindWeaponSound(GameObjectBody weapon, GameObjectBody attacker, GameObjectBody target,
            int soundType)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x1006dfd0)]
        public int GetAnimateForeverSoundEffect(GameObjectBody obj, int subtype)
        {
            if ((obj.type.IsCritter() || obj.type == ObjectType.container || obj.type == ObjectType.portal ||
                 obj.type.IsEquipment()) && obj.type != ObjectType.weapon)
            {
                return -1;
            }

            var soundId = obj.GetInt32(obj_f.sound_effect);
            if (soundId == 0)
            {
                return -1;
            }

            switch (subtype)
            {
                case 0 when obj.type != ObjectType.weapon:
                    return GameSystems.SoundGame.IsValidSoundId(soundId) ? soundId : -1;
                case 1 when obj.type != ObjectType.weapon:
                    soundId++;
                    return GameSystems.SoundGame.IsValidSoundId(soundId) ? soundId : -1;
                case 2:
                    return soundId;
                default:
                    return -1;
            }
        }

        private static readonly Dictionary<TileMaterial, int> FootstepBaseSound = new Dictionary<TileMaterial, int>
        {
            {TileMaterial.Dirt, 2904},
            {TileMaterial.Grass, 2912},
            {TileMaterial.Water, 2928},
            {TileMaterial.Ice, 2916},
            {TileMaterial.Wood, 2932},
            {TileMaterial.Stone, 2920},
            {TileMaterial.Metal, 2920},
        };

        [TempleDllLocation(0x1006def0)]
        public int GetCritterSoundEffect(GameObjectBody obj, CritterSoundEffect type)
        {
            if (!obj.IsCritter())
            {
                return -1;
            }

            var partyLeader = GameSystems.Party.GetLeader();
            if (partyLeader == null || partyLeader.HasFlag(ObjectFlag.OFF))
            {
                return -1;
            }

            if (type == CritterSoundEffect.Footsteps)
            {
                var critterPos = obj.GetLocation();
                var groundMaterial = GameSystems.Tile.GetMaterial(critterPos);
                if (FootstepBaseSound.TryGetValue(groundMaterial, out var baseId))
                {
                    return baseId + GameSystems.Random.GetInt(0, 3);
                }

                return -1;
            }
            else
            {
                var soundEffect = obj.GetInt32(obj_f.sound_effect);
                return soundEffect + (int) type;
            }
        }

        [TempleDllLocation(0x1006e440)]
        public string GetWeaponHitSoundPath(int soundId)
        {
            throw new NotImplementedException();
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
        public void Add(GameObjectBody critter, int rumorId)
        {
            if (critter.IsPC())
            {
                AddRumor(rumorId);
                GameUiBridge.AddRumor(rumorId);
            }
        }

        [TempleDllLocation(0x1005fb70)]
        [TemplePlusLocation("python_integration_obj.cpp:290")]
        public bool TryFindRumor(GameObjectBody pc, GameObjectBody npc, out int rumorId)
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
        public bool TryGetRumorNpcLine(GameObjectBody pc, GameObjectBody npc, int rumorId, out string lineText)
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

    public class MovieSystem : IGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            // TODO movies
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10034100)]
        public void PlayMovie(string path, int p1, int p2, int p3)
        {
            Stub.TODO();
        }

        /// <summary>
        /// Plays a movie from movies.mes, which could either be a slide or binkw movie.
        /// The soundtrack id is used for BinkW movies with multiple soundtracks.
        /// As far as we know, this is not used at all in ToEE.
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="flags"></param>
        /// <param name="soundtrackId"></param>
        [TempleDllLocation(0x100341f0)]
        public void PlayMovieId(int movieId, int flags, int soundtrackId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10033de0)]
        public void MovieQueueAdd(int movieId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100345a0)]
        public void MovieQueuePlay()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10034670)]
        public void MovieQueuePlayAndEndGame()
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

    public class DeitySystem : IGameSystem
    {
        /// <summary>
        /// These Deity Names are only used for parsing protos.tab.
        /// </summary>
        [TempleDllLocation(0x102B0868)]
        private static readonly string[] EnglishDeityNames =
        {
            "No Deity",
            "Boccob",
            "Corellon Larethian",
            "Ehlonna",
            "Erythnul",
            "Fharlanghn",
            "Garl Glittergold",
            "Gruumsh",
            "Heironeous",
            "Hextor",
            "Kord",
            "Moradin",
            "Nerull",
            "Obad-Hai",
            "Olidammara",
            "Pelor",
            "St. Cuthbert",
            "Vecna",
            "Wee Jas",
            "Yondalla",
            "Old Faith",
            "Zuggtmoy",
            "Iuz",
            "Lolth",
            "Procan",
            "Norebo",
            "Pyremius",
            "Ralishaz",
        };

        private readonly Dictionary<DeityId, string> _deityNames;

        private readonly Dictionary<DeityId, string> _deityPraise;

        [TempleDllLocation(0x1004a760)]
        public DeitySystem()
        {
            var deityMes = Tig.FS.ReadMesFile("mes/deity.mes");

            _deityNames = new Dictionary<DeityId, string>(DeityIds.Deities.Length);
            _deityPraise = new Dictionary<DeityId, string>(DeityIds.Deities.Length);
            foreach (var deityId in DeityIds.Deities)
            {
                _deityNames[deityId] = deityMes[(int) deityId];
                _deityPraise[deityId] = deityMes[3000 + (int) deityId];
            }

            Stub.TODO(); // Missing condition naming
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x1004a820)]
        public static bool GetDeityFromEnglishName(string name, out DeityId deityId)
        {
            for (int i = 0; i < EnglishDeityNames.Length; i++)
            {
                if (EnglishDeityNames[i].Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    deityId = (DeityId) i;
                    return true;
                }
            }

            deityId = default;
            return false;
        }

        [TempleDllLocation(0x1004c0d0)]
        public static bool IsDomainSkill(GameObjectBody obj, SkillId skillId)
        {
            var domain1 = (DomainId) obj.GetStat(Stat.domain_1);
            if (IsDomainSkill(domain1, skillId))
            {
                return true;
            }

            var domain2 = (DomainId) obj.GetStat(Stat.domain_2);
            return IsDomainSkill(domain2, skillId);
        }

        [TempleDllLocation(0x1004aba0)]
        private static bool IsDomainSkill(DomainId domain, SkillId skillId)
        {
            switch (domain)
            {
                case DomainId.Trickery:
                    return skillId == SkillId.bluff || skillId == SkillId.disguise || skillId == SkillId.hide;
                case DomainId.Knowledge:
                    return skillId == SkillId.knowledge_all || skillId == SkillId.knowledge_arcana ||
                           skillId == SkillId.knowledge_religion || skillId == SkillId.knowledge_nature;
                case DomainId.Travel:
                    return skillId == SkillId.wilderness_lore;
                case DomainId.Animal:
                case DomainId.Plant:
                    return skillId == SkillId.knowledge_nature;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x1004a890)]
        public string GetName(DeityId deity)
        {
            return _deityNames[deity];
        }

        public string GetPrayerHeardMessage(DeityId deity)
        {
            var lineId = 910 + (int) deity;
            return GameSystems.Skill.GetSkillUiMessage(lineId);
        }

        [TempleDllLocation(0x1004ab00)]
        public void DeityPraiseFloatMessage(GameObjectBody obj)
        {
            var deity = (DeityId) obj.GetStat(Stat.deity);
            if (_deityPraise.TryGetValue(deity, out var line))
            {
                var color = TextFloaterColor.Red;
                if (obj.IsPC())
                {
                    color = TextFloaterColor.White;
                }
                else if (obj.IsNPC())
                {
                    var leader = GameSystems.Critter.GetLeaderRecursive(obj);
                    if (GameSystems.Party.IsInParty(leader))
                    {
                        color = TextFloaterColor.Yellow;
                    }
                }

                GameSystems.TextFloater.FloatLine(obj, TextFloaterCategory.Generic, color, line);
            }
        }

        // only good deities and St. Cuthbert (any suggestions?)
        private static readonly ISet<DeityId> GoodDeities = new HashSet<DeityId>
        {
            DeityId.CORELLON_LARETHIAN,
            DeityId.EHLONNA,
            DeityId.GARL_GLITTERGOLD,
            DeityId.HEIRONEOUS,
            DeityId.KORD,
            DeityId.MORADIN,
            DeityId.PELOR,
            DeityId.ST_CUTHBERT,
            DeityId.YONDALLA
        };

        public bool AllowsAtonement(DeityId casterDeity)
        {
            return GoodDeities.Contains(casterDeity);
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
        public string GetPortraitPath(int portraitId, PortraitVariant variant)
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
        public string GetInventoryIconPath(int artId)
        {
            if (_inventoryPaths.TryGetValue(artId, out var result))
            {
                return result;
            }

            return null;
        }

        [TempleDllLocation(0x1004a4e0)]
        public string GetGenericTiledImagePath(int artId)
        {
            if (_genericLargePaths.TryGetValue(artId, out var result))
            {
                return result;
            }

            return null;
        }

        [TempleDllLocation(0x1004a360)]
        public string GetGenericPath(int artId)
        {
            if (_genericPaths.TryGetValue(artId, out var result))
            {
                return result;
            }

            return null;
        }
    }

    public class CheatsSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public static class SecretdoorExtensions
    {
        [TempleDllLocation(0x10046470)]
        public static bool IsSecretDoor(this GameObjectBody portal)
        {
            return portal.GetSecretDoorFlags().HasFlag(SecretDoorFlag.SECRET_DOOR);
        }

        public static bool IsUndetectedSecretDoor(this GameObjectBody portal)
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
            Reset();
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
            if (ShowHighlights && !Tig.Keyboard.IsPressed(DIK.DIK_TAB))
            {
                ShowHighlights = false;
            }
            else if (!ShowHighlights && Tig.Keyboard.IsPressed(DIK.DIK_TAB))
            {
                ShowHighlights = true;
            }
        }
    }
}