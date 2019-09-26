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
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TabFiles;
using SpicyTemple.Core.IO.TroikaArchives;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Particles;
using SpicyTemple.Core.Systems.AI;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.Clipping;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Fade;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.FogOfWar;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Help;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Protos;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Systems.RollHistory;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.Teleport;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.Systems.Waypoints;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public static class GameSystems
    {
        private static readonly ILogger Logger = new ConsoleLogger();

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
        public static SpellSystem Spell { get; private set; }
        public static D20StatSystem Stat { get; private set; }
        public static ScriptSystem Script { get; private set; }
        public static LevelSystem Level { get; private set; }
        public static D20System D20 { get; private set; }
        public static MapSystem Map { get; private set; }
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
        public static AnimPrivateSystem AnimPrivate { get; private set; }
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
        public static D20RollsSystem D20Rolls { get; private set; }
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

        public static int Difficulty { get; set; }

        [TempleDllLocation(0x10307054)]
        public static bool ModuleLoaded { get; private set; }

        public static void Init()
        {
            Logger.Info("Loading game systems");

            var config = Globals.Config;
            config.AddVanillaSetting("difficulty", "1", DifficultyChanged);
            DifficultyChanged(); // Read initial setting
            config.AddVanillaSetting("autosave_between_maps", "1");
            config.AddVanillaSetting("movies_seen", "(304,-1)");
            config.AddVanillaSetting("startup_tip", "0");
            config.AddVanillaSetting("video_adapter", "0");
            config.AddVanillaSetting("video_width", "800");
            config.AddVanillaSetting("video_height", "600");
            config.AddVanillaSetting("video_bpp", "32");
            config.AddVanillaSetting("video_refresh_rate", "60");
            config.AddVanillaSetting("video_antialiasing", "0");
            config.AddVanillaSetting("video_quad_blending", "1");
            config.AddVanillaSetting("particle_fidelity", "100");
            config.AddVanillaSetting("env_mapping", "1");
            config.AddVanillaSetting("cloth_frame_skip", "0");
            config.AddVanillaSetting("concurrent_turnbased", "1");
            config.AddVanillaSetting("end_turn_time", "1");
            config.AddVanillaSetting("end_turn_default", "1");
            config.AddVanillaSetting("draw_hp", "0");

            // Some of these are also registered as value change callbacks and could be replaced by simply calling all
            // value change callbacks here, which makes sense anyway.
            var particleFidelity = config.GetVanillaInt("particle_fidelity") / 100.0f;
            // TODO gameSystemInitTable.SetParticleFidelity(particleFidelity);

            var envMappingEnabled = config.GetVanillaInt("env_mapping");
            // TODO gameSystemInitTable.SetEnvMapping(envMappingEnabled);

            var clothFrameSkip = config.GetVanillaInt("cloth_frame_skip");
            // TODO gameSystemInitTable.SetClothFrameSkip(clothFrameSkip);

            var concurrentTurnbased = config.GetVanillaInt("concurrent_turnbased");
            // TODO gameSystemInitTable.SetConcurrentTurnbased(concurrentTurnbased);

            var endTurnTime = config.GetVanillaInt("end_turn_time");
            // TODO gameSystemInitTable.SetEndTurnTime(endTurnTime);

            var endTurnDefault = config.GetVanillaInt("end_turn_default");
            // TODO gameSystemInitTable.SetEndTurnDefault(endTurnDefault);

            var drawHp = config.GetVanillaInt("draw_hp");
            // TODO gameSystemInitTable.SetDrawHp(drawHp != 0);

            ModuleLoaded = false;

            Tig.Mouse.SetBounds(Tig.RenderingDevice.GetCamera().ScreenSize);

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

            // TODO InitBufferStuff(mConfig);

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
            if (Map != null)
            {
                Map.CloseMap();
            }

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
            D20Rolls?.Dispose();
            D20Rolls = null;
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
            AnimPrivate?.Dispose();
            AnimPrivate = null;
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

        private static void DifficultyChanged()
        {
            Difficulty = Globals.Config.GetVanillaInt("difficulty");
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
                system.AdvanceTime(now);
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

/**
 * Creates the screenshots that will be used in case the game is saved.
 */
        public static void TakeSaveScreenshots()
        {
            var device = Tig.RenderingDevice;
            device.TakeScaledScreenshot("save/temps.jpg", 64, 48);
            device.TakeScaledScreenshot("save/templ.jpg", 256, 192);
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

        // TODO private static void InitBufferStuff(const GameSystemConf& conf);

        private static void ResizeScreen(int w, int h)
        {
            // TODO gameSystemInitTable.gameSystemConf.width = w;
            // TODO gameSystemInitTable.gameSystemConf.height = h;

            UiSystems.ResizeViewport(w, h);
        }

        private static void InitializeSystems(LoadingScreen loadingScreen)
        {
            loadingScreen.SetMessage("Loading...");

            AAS = new AASSystem(Tig.FS, Tig.MdfFactory, new AasRenderer(
                Tig.RenderingDevice,
                Tig.ShapeRenderer2d,
                Tig.ShapeRenderer3d
            ));

            // Loading Screen ID: 2
            loadingScreen.SetProgress(1 / 79.0f);
            Vagrant = InitializeSystem(loadingScreen, () => new VagrantSystem());
            // Loading Screen ID: 2
            loadingScreen.SetProgress(2 / 79.0f);
            Description = InitializeSystem(loadingScreen, () => new DescriptionSystem());
            loadingScreen.SetProgress(3 / 79.0f);
            ItemEffect = InitializeSystem(loadingScreen, () => new ItemEffectSystem());
            // Loading Screen ID: 3
            loadingScreen.SetProgress(4 / 79.0f);
            Teleport = InitializeSystem(loadingScreen, () => new TeleportSystem());
            // Loading Screen ID: 4
            loadingScreen.SetProgress(5 / 79.0f);
            Sector = InitializeSystem(loadingScreen, () => new SectorSystem());
            // Loading Screen ID: 5
            loadingScreen.SetProgress(6 / 79.0f);
            Random = InitializeSystem(loadingScreen, () => new RandomSystem());
            // Loading Screen ID: 6
            loadingScreen.SetProgress(7 / 79.0f);
            Critter = InitializeSystem(loadingScreen, () => new CritterSystem());
            // Loading Screen ID: 7
            loadingScreen.SetProgress(8 / 79.0f);
            ScriptName = InitializeSystem(loadingScreen, () => new ScriptNameSystem());
            // Loading Screen ID: 8
            loadingScreen.SetProgress(9 / 79.0f);
            Portrait = InitializeSystem(loadingScreen, () => new PortraitSystem());
            // Loading Screen ID: 9
            loadingScreen.SetProgress(10 / 79.0f);
            Skill = InitializeSystem(loadingScreen, () => new SkillSystem());
            // Loading Screen ID: 10
            loadingScreen.SetProgress(11 / 79.0f);
            Feat = InitializeSystem(loadingScreen, () => new FeatSystem());
            // Loading Screen ID: 11
            loadingScreen.SetProgress(12 / 79.0f);
            Spell = InitializeSystem(loadingScreen, () => new SpellSystem());
            loadingScreen.SetProgress(13 / 79.0f);
            Stat = InitializeSystem(loadingScreen, () => new D20StatSystem());
            // Loading Screen ID: 12
            loadingScreen.SetProgress(14 / 79.0f);
            Script = InitializeSystem(loadingScreen, () => new ScriptSystem());
            loadingScreen.SetProgress(15 / 79.0f);
            Level = InitializeSystem(loadingScreen, () => new LevelSystem());
            loadingScreen.SetProgress(16 / 79.0f);
            D20 = InitializeSystem(loadingScreen, () => new D20System());
            Help = new HelpSystem();
            // Loading Screen ID: 1
            loadingScreen.SetProgress(17 / 79.0f);
            Map = InitializeSystem(loadingScreen, () => new MapSystem(D20));

            /* START Former Map Subsystems */
            loadingScreen.SetProgress(18 / 79.0f);
            Location = InitializeSystem(loadingScreen, () => new LocationSystem());
            loadingScreen.SetProgress(19 / 79.0f);
            Scroll = InitializeSystem(loadingScreen, () => new ScrollSystem());
            loadingScreen.SetProgress(20 / 79.0f);
            Light = InitializeSystem(loadingScreen, () => new LightSystem());
            loadingScreen.SetProgress(21 / 79.0f);
            Tile = InitializeSystem(loadingScreen, () => new TileSystem());
            loadingScreen.SetProgress(22 / 79.0f);
            OName = InitializeSystem(loadingScreen, () => new ONameSystem());
            loadingScreen.SetProgress(23 / 79.0f);
            ObjectNode = InitializeSystem(loadingScreen, () => new ObjectNodeSystem());
            loadingScreen.SetProgress(24 / 79.0f);
            Object = InitializeSystem(loadingScreen, () => new ObjectSystem());
            loadingScreen.SetProgress(25 / 79.0f);
            Proto = InitializeSystem(loadingScreen, () => new ProtoSystem());
            loadingScreen.SetProgress(26 / 79.0f);
            Raycast = new RaycastSystem();
            MapObject = InitializeSystem(loadingScreen, () => new MapObjectSystem());
            loadingScreen.SetProgress(27 / 79.0f);
            MapSector = InitializeSystem(loadingScreen, () => new MapSectorSystem());
            loadingScreen.SetProgress(28 / 79.0f);
            SectorVisibility = InitializeSystem(loadingScreen, () => new SectorVisibilitySystem());
            loadingScreen.SetProgress(29 / 79.0f);
            TextBubble = InitializeSystem(loadingScreen, () => new TextBubbleSystem());
            loadingScreen.SetProgress(30 / 79.0f);
            TextFloater = InitializeSystem(loadingScreen, () => new TextFloaterSystem());
            loadingScreen.SetProgress(31 / 79.0f);
            JumpPoint = InitializeSystem(loadingScreen, () => new JumpPointSystem());
            loadingScreen.SetProgress(32 / 79.0f);
            Clipping = InitializeSystem(loadingScreen, () => new ClippingSystem(Tig.RenderingDevice));
            Terrain = InitializeSystem(loadingScreen, () => new TerrainSystem(Tig.RenderingDevice,
                Tig.ShapeRenderer2d));
            loadingScreen.SetProgress(33 / 79.0f);
            Height = InitializeSystem(loadingScreen, () => new HeightSystem());
            loadingScreen.SetProgress(34 / 79.0f);
            GMesh = InitializeSystem(loadingScreen, () => new GMeshSystem(AAS));
            loadingScreen.SetProgress(35 / 79.0f);
            PathNode = InitializeSystem(loadingScreen, () => new PathNodeSystem());
            /* END Former Map Subsystems */

            loadingScreen.SetProgress(36 / 79.0f);
            LightScheme = InitializeSystem(loadingScreen, () => new LightSchemeSystem());
            loadingScreen.SetProgress(37 / 79.0f);
            Player = InitializeSystem(loadingScreen, () => new PlayerSystem());
            loadingScreen.SetProgress(38 / 79.0f);
            Area = InitializeSystem(loadingScreen, () => new AreaSystem());
            loadingScreen.SetProgress(39 / 79.0f);
            Dialog = InitializeSystem(loadingScreen, () => new DialogSystem());
            loadingScreen.SetProgress(40 / 79.0f);
            SoundMap = InitializeSystem(loadingScreen, () => new SoundMapSystem());
            loadingScreen.SetProgress(41 / 79.0f);
            SoundGame = InitializeSystem(loadingScreen, () => new SoundGameSystem());
            loadingScreen.SetProgress(42 / 79.0f);
            Item = InitializeSystem(loadingScreen, () => new ItemSystem());
            Weapon = new WeaponSystem();
            loadingScreen.SetProgress(43 / 79.0f);
            Combat = InitializeSystem(loadingScreen, () => new CombatSystem());
            loadingScreen.SetProgress(44 / 79.0f);
            TimeEvent = InitializeSystem(loadingScreen, () => new TimeEventSystem());
            loadingScreen.SetProgress(45 / 79.0f);
            Rumor = InitializeSystem(loadingScreen, () => new RumorSystem());
            loadingScreen.SetProgress(46 / 79.0f);
            Quest = InitializeSystem(loadingScreen, () => new QuestSystem());
            loadingScreen.SetProgress(47 / 79.0f);
            AI = InitializeSystem(loadingScreen, () => new AiSystem());
            loadingScreen.SetProgress(48 / 79.0f);
            Anim = InitializeSystem(loadingScreen, () => new AnimSystem());
            loadingScreen.SetProgress(49 / 79.0f);
            AnimPrivate = InitializeSystem(loadingScreen, () => new AnimPrivateSystem());
            loadingScreen.SetProgress(50 / 79.0f);
            Reputation = InitializeSystem(loadingScreen, () => new ReputationSystem());
            loadingScreen.SetProgress(51 / 79.0f);
            Reaction = InitializeSystem(loadingScreen, () => new ReactionSystem());
            loadingScreen.SetProgress(52 / 79.0f);
            TileScript = InitializeSystem(loadingScreen, () => new TileScriptSystem());
            loadingScreen.SetProgress(53 / 79.0f);
            SectorScript = InitializeSystem(loadingScreen, () => new SectorScriptSystem());
            loadingScreen.SetProgress(54 / 79.0f);

            // NOTE: This system is only used in worlded (rendering related)
            Waypoint = InitializeSystem(loadingScreen, () => new WaypointSystem());
            loadingScreen.SetProgress(55 / 79.0f);

            InvenSource = InitializeSystem(loadingScreen, () => new InvenSourceSystem());
            loadingScreen.SetProgress(56 / 79.0f);
            TownMap = InitializeSystem(loadingScreen, () => new TownMapSystem());
            loadingScreen.SetProgress(57 / 79.0f);
            Movies = InitializeSystem(loadingScreen, () => new MovieSystem());
            loadingScreen.SetProgress(58 / 79.0f);
            Brightness = InitializeSystem(loadingScreen, () => new BrightnessSystem());
            loadingScreen.SetProgress(59 / 79.0f);
            GFade = InitializeSystem(loadingScreen, () => new GFadeSystem());
            loadingScreen.SetProgress(60 / 79.0f);
            AntiTeleport = InitializeSystem(loadingScreen, () => new AntiTeleportSystem());
            loadingScreen.SetProgress(61 / 79.0f);
            Trap = InitializeSystem(loadingScreen, () => new TrapSystem());
            loadingScreen.SetProgress(62 / 79.0f);
            MonsterGen = InitializeSystem(loadingScreen, () => new MonsterGenSystem());
            loadingScreen.SetProgress(63 / 79.0f);
            Party = InitializeSystem(loadingScreen, () => new PartySystem());
            loadingScreen.SetProgress(64 / 79.0f);
            D20LoadSave = InitializeSystem(loadingScreen, () => new D20LoadSaveSystem());
            loadingScreen.SetProgress(65 / 79.0f);
            GameInit = InitializeSystem(loadingScreen, () => new GameInitSystem());
            loadingScreen.SetProgress(66 / 79.0f);
            // NOTE: The "ground" system has been superseded by the terrain system
            loadingScreen.SetProgress(67 / 79.0f);
            ObjFade = InitializeSystem(loadingScreen, () => new ObjFadeSystem());
            loadingScreen.SetProgress(68 / 79.0f);
            Deity = InitializeSystem(loadingScreen, () => new DeitySystem());
            loadingScreen.SetProgress(69 / 79.0f);
            UiArtManager = InitializeSystem(loadingScreen, () => new UiArtManagerSystem());
            loadingScreen.SetProgress(70 / 79.0f);
            ParticleSys = InitializeSystem(loadingScreen,
                () => new ParticleSysSystem(Tig.RenderingDevice.GetCamera()));
            loadingScreen.SetProgress(71 / 79.0f);
            Cheats = InitializeSystem(loadingScreen, () => new CheatsSystem());
            loadingScreen.SetProgress(72 / 79.0f);
            D20Rolls = InitializeSystem(loadingScreen, () => new D20RollsSystem());
            loadingScreen.SetProgress(73 / 79.0f);
            Secretdoor = InitializeSystem(loadingScreen, () => new SecretdoorSystem());
            loadingScreen.SetProgress(74 / 79.0f);
            MapFogging = InitializeSystem(loadingScreen, () => new MapFoggingSystem(Tig.RenderingDevice));
            loadingScreen.SetProgress(75 / 79.0f);
            RandomEncounter = InitializeSystem(loadingScreen, () => new RandomEncounterSystem());
            loadingScreen.SetProgress(76 / 79.0f);
            ObjectEvent = InitializeSystem(loadingScreen, () => new ObjectEventSystem());
            loadingScreen.SetProgress(77 / 79.0f);
            Formation = InitializeSystem(loadingScreen, () => new FormationSystem());
            loadingScreen.SetProgress(78 / 79.0f);
            ItemHighlight = InitializeSystem(loadingScreen, () => new ItemHighlightSystem());
            loadingScreen.SetProgress(79 / 79.0f);
            PathX = InitializeSystem(loadingScreen, () => new PathXSystem());
            PathXRender = new PathXRenderSystem();
            Vfx = new VfxSystem();
            RollHistory = new RollHistorySystem();
            Poison = new PoisonSystem();
            Disease = new DiseaseSystem();
        }

        private static T InitializeSystem<T>(LoadingScreen loadingScreen, Func<T> factory) where T : IGameSystem
        {
            Logger.Info($"Loading game system {typeof(T).Name}");
            Tig.SystemEventPump.PumpSystemEvents();
            loadingScreen.Render();

            var system = factory();

            _initializedSystems.Add(system);

            return system;
        }
    }

    public interface IMapCloseAwareGameSystem
    {
        void CloseMap();
    }

    public interface ISaveGameAwareGameSystem
    {
        bool SaveGame();
        bool LoadGame();
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

        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
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
                yield return Tuple.Create(scriptId, filename);
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
        public string GetDialogScriptPath(int scriptId)
        {
            return _dialogIndex.GetValueOrDefault(scriptId, null);
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

    public class TextBubbleSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x100a3030)]
        public void Remove(GameObjectBody obj)
        {
            // TODO
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

    public class PlayerSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1006ee80)]
        public void Restore()
        {
            Stub.TODO();
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
    }

    public enum PortalSoundEffect
    {
        Open = 0,
        Close,
        Locked = 2
    }

    /**
     * TODO: This might correspond to the scs_ python enum
     * scs_critically_hit = 0
     * scs_dying = 1
     * scs_dying_gruesome = 2
     * scs_fidgeting = 3
     * scs_attacking = 4
     * scs_alerted = 5
     * scs_agitated = 6
     * scs_footsteps = 7
     */
    public enum CritterSoundEffect
    {
        Attack = 0,
        Death = 1,
        Unk5 = 5,
        Footsteps = 7
    }

    public class SoundGameSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem,
        ITimeAwareSystem
    {
        private readonly PositionalAudioConfig _positionalAudioConfig;

        [TempleDllLocation(0x1003d4a0)]
        public SoundGameSystem()
        {
            // TODO SOUND
            var soundParams = Tig.FS.ReadMesFile("sound/soundparams.mes");
            _positionalAudioConfig = new PositionalAudioConfig(soundParams);
        }

        [TempleDllLocation(0x1003bb10)]
        public void Dispose()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003bb80)]
        public void LoadModule()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003bbc0)]
        public void UnloadModule()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003cb30)]
        public void Reset()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1003bbd0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1003cb70)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1003dc50)]
        public void AdvanceTime(TimePoint time)
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003bdb0)]
        public void Sound(int soundId, int loopCount = 1)
        {
            Stub.TODO();
        }

        public int PositionalSound(int soundId, GameObjectBody source)
        {
            return PositionalSound(soundId, 1, source);
        }

        public int PositionalSound(int soundId, locXY location)
        {
            return PositionalSound(soundId, 1, location);
        }

        [TempleDllLocation(0x1003d090)]
        public int PositionalSound(int soundId, int loopCount, GameObjectBody source)
        {
            Stub.TODO();
            return -1;
        }

        [TempleDllLocation(0x1003dcb0)]
        public int PositionalSound(int soundId, int loopCount, locXY location)
        {
            Stub.TODO();
            return -1;
        }

        [TempleDllLocation(0x1003c5b0)]
        public void StopAll(bool b)
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x1003c4d0)]
        public void SetScheme(int s1, int s2)
        {
            // TODO SOUND
        }

        /// <summary>
        /// Sets the tile coordinates the view is currently centered on.
        /// </summary>
        [TempleDllLocation(0x1003D3C0)]
        public void SetViewCenterTile(locXY location)
        {
            // TODO
        }

        [TempleDllLocation(0x1003c770)]
        public void StartCombatMusic(GameObjectBody handle)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1003C8B0)]
        public void StopCombatMusic(GameObjectBody handle)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1003b9e0)]
        public string FindSoundFilename(int soundId)
        {
            throw new NotImplementedException();
        }

        public bool IsValidSoundId(int soundId) => FindSoundFilename(soundId) != null;

        [TempleDllLocation(0x1003bdd0)]
        public SoundSourceSize GetSoundSourceSize(GameObjectBody obj)
        {
            if (obj.type == ObjectType.scenery)
            {
                var sceneryFlags = obj.GetSceneryFlags();
                if (sceneryFlags.HasFlag(SceneryFlag.SOUND_SMALL))
                {
                    return SoundSourceSize.Small;
                }

                if (sceneryFlags.HasFlag(SceneryFlag.SOUND_MEDIUM))
                {
                    return SoundSourceSize.Medium;
                }

                if (sceneryFlags.HasFlag(SceneryFlag.SOUND_EXTRA_LARGE))
                {
                    return SoundSourceSize.ExtraLarge;
                }
            }

            return SoundSourceSize.Large;
        }

        [TempleDllLocation(0x1003BE30)]
        public int GetSoundOutOfRangeRange(GameObjectBody obj)
        {
            var sourceSize = GetSoundSourceSize(obj);
            return _positionalAudioConfig.AttenuationRangeEnd[sourceSize] / 28;
        }
    }

    public enum SoundSourceSize
    {
        Small = 0,
        Medium = 1,
        Large = 2, // This is the default
        ExtraLarge = 3
    }

    public class PositionalAudioConfig
    {
        /// <summary>
        /// Sounds closer than this (in screen coordinates) are unattenuated.
        /// This seems to be relative to the center of the screen.
        /// </summary>
        public Dictionary<SoundSourceSize, int> AttenuationRangeStart { get; } =
            new Dictionary<SoundSourceSize, int>
            {
                {SoundSourceSize.Small, 50},
                {SoundSourceSize.Medium, 50},
                {SoundSourceSize.Large, 150},
                {SoundSourceSize.ExtraLarge, 50}
            };

        /// <summary>
        /// Sound sources further away than this (in screen coordinates) from the
        /// center of the screen play at zero volume.
        /// TODO: This should calculate based on the screen edge.
        /// </summary>
        public Dictionary<SoundSourceSize, int> AttenuationRangeEnd { get; } =
            new Dictionary<SoundSourceSize, int>
            {
                {SoundSourceSize.Small, 150},
                {SoundSourceSize.Medium, 400},
                {SoundSourceSize.Large, 800},
                {SoundSourceSize.ExtraLarge, 1500}
            };

        /// <summary>
        /// The volume for sound sources of a given size at minimum attenuation.
        /// </summary>
        public Dictionary<SoundSourceSize, int> AttenuationMaxVolume { get; } =
            new Dictionary<SoundSourceSize, int>
            {
                {SoundSourceSize.Small, 40},
                {SoundSourceSize.Medium, 70},
                {SoundSourceSize.Large, 100},
                {SoundSourceSize.ExtraLarge, 100}
            };

        /// <summary>
        /// Sounds within this range of the screen center (in screen coordinates) play
        /// dead center.
        /// </summary>
        public int PanningMinRange { get; } = 150;

        /// <summary>
        /// Sounds further away than this range relative to the screen center (in screen coordinates) play
        /// fully on that side.
        /// </summary>
        public int PanningMaxRange { get; } = 400;

        public PositionalAudioConfig()
        {
        }

        public PositionalAudioConfig(Dictionary<int, string> parameters)
        {
            AttenuationRangeStart[SoundSourceSize.Large] = int.Parse(parameters[1]);
            AttenuationRangeEnd[SoundSourceSize.Large] = int.Parse(parameters[2]);
            PanningMinRange = int.Parse(parameters[3]);
            PanningMaxRange = int.Parse(parameters[4]);

            AttenuationRangeStart[SoundSourceSize.Small] = int.Parse(parameters[10]);
            AttenuationRangeEnd[SoundSourceSize.Small] = int.Parse(parameters[11]);
            AttenuationMaxVolume[SoundSourceSize.Small] = int.Parse(parameters[12]);

            AttenuationRangeStart[SoundSourceSize.Medium] = int.Parse(parameters[20]);
            AttenuationRangeEnd[SoundSourceSize.Medium] = int.Parse(parameters[21]);
            AttenuationMaxVolume[SoundSourceSize.Medium] = int.Parse(parameters[22]);

            AttenuationRangeStart[SoundSourceSize.ExtraLarge] = int.Parse(parameters[30]);
            AttenuationRangeEnd[SoundSourceSize.ExtraLarge] = int.Parse(parameters[31]);
            AttenuationMaxVolume[SoundSourceSize.ExtraLarge] = int.Parse(parameters[32]);
        }
    }

    public class RumorSystem : IGameSystem
    {
        [TempleDllLocation(0x1005f960)]
        public RumorSystem()
        {
        }

        [TempleDllLocation(0x1005f9d0)]
        public void Dispose()
        {
        }

        private void AddRumor(int rumorId)
        {
        }

        public void Add(GameObjectBody critter, int rumorId)
        {
            if (critter.IsPC())
            {
                AddRumor(rumorId);
                GameUiBridge.AddRumor(rumorId);
            }
        }
    }

    public class AnimPrivateSystem : IGameSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class ReputationSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054d70)]
        public bool HasFactionFromReputation(GameObjectBody pc, int faction)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054BD0)]
        public int GetReactionModFromReputation(GameObjectBody pc, GameObjectBody npc)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100546e0)]
        public bool HasReputation(GameObjectBody pc, int reputation)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054740)]
        public void AddReputation(GameObjectBody pc, int reputation)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054820)]
        public void RemoveReputation(GameObjectBody pc, int reputation)
        {
            throw new NotImplementedException();
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
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            // TODO Townmap
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10052430)]
        public void sub_10052430(locXY location)
        {
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

    public class MonsterGenSystem : IGameSystem, ISaveGameAwareGameSystem, IBufferResettingSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        public void ResetBuffers()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100508c0)]
        public void CritterKilled(GameObjectBody critter)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10050740)]
        public bool GetNextEventTime(GameObjectBody generator, out TimeSpan delay)
        {
            throw new NotImplementedException();
        }
    }

    public class D20LoadSaveSystem : IGameSystem, ISaveGameAwareGameSystem
    {
        public void Dispose()
        {
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
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

        private readonly Dictionary<DeityId, string> _prayersHeard;

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

            var skillUiMes = Tig.FS.ReadMesFile("mes/skill_ui.mes");
            foreach (var deityId in DeityIds.Deities)
            {
                _prayersHeard[deityId] = skillUiMes[910 + (int) deityId];
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
            return _prayersHeard[deity];
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
            Trace.Assert(portraitId % 10 == 0);
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

    public class D20RollsSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10047160)]
        public void Reset()
        {
            Stub.TODO();
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
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
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void AdvanceTime(TimePoint time)
        {
            // TODO
        }
    }
}