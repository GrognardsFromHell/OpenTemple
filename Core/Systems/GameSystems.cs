using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.Config;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TroikaArchives;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Fade;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;

namespace SpicyTemple.Core.Systems
{
    public static class GameSystems
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x103072B8)] private static bool mIronmanFlag;

        [TempleDllLocation(0x10306F44)] private static int mIronmanSaveNumber;

        [TempleDllLocation(0x103072C0)] private static string mIronmanSaveName;

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
        public static StatSystem Stat { get; private set; }
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
        public static ObjSystem Obj { get; private set; }
        public static ProtoSystem Proto { get; private set; }
        public static ObjectSystem Object { get; private set; }
        public static MapSectorSystem MapSector { get; private set; }
        public static SectorVBSystem SectorVB { get; private set; }
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
        public static CombatSystem Combat { get; private set; }
        public static TimeEventSystem TimeEvent { get; private set; }
        public static RumorSystem Rumor { get; private set; }
        public static QuestSystem Quest { get; private set; }
        public static AISystem AI { get; private set; }
        public static AnimSystem Anim { get; private set; }
        public static AnimPrivateSystem AnimPrivate { get; private set; }
        public static ReputationSystem Reputation { get; private set; }
        public static ReactionSystem Reaction { get; private set; }
        public static TileScriptSystem TileScript { get; private set; }
        public static SectorScriptSystem SectorScript { get; private set; }
        public static WPSystem WP { get; private set; }
        public static InvenSourceSystem InvenSource { get; private set; }
        public static TownMapSystem TownMap { get; private set; }
        public static GMovieSystem GMovie { get; private set; }
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
        public static AASSystem AAS { get; private set; }

        private static List<IGameSystem> _initializedSystems = new List<IGameSystem>();

        // All systems that want to listen to map events
        public static IEnumerable<IMapCloseAwareGameSystem> GetMapCloseAwareSystems()
        {
            return _initializedSystems.OfType<IMapCloseAwareGameSystem>();
        }

        public static IEnumerable<ITimeAwareSystem> GetTimeAwareSystems()
        {
            return _initializedSystems.OfType<ITimeAwareSystem>();
        }

        public static IEnumerable<IModuleAwareSystem> GetModuleAwareSystems()
        {
            return _initializedSystems.OfType<IModuleAwareSystem>();
        }

        public static IEnumerable<IResetAwareSystem> GetResetAwareSystems()
        {
            return _initializedSystems.OfType<IResetAwareSystem>();
        }

        public static int Difficulty { get; set; }

        [TempleDllLocation(0x10307054)] public static bool ModuleLoaded { get; private set; }

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

            // TODO gameSystemInitTable.InitPfxLightning();

            mIronmanFlag = false;
            mIronmanSaveName = null;
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
            GMovie?.Dispose();
            GMovie = null;
            TownMap?.Dispose();
            TownMap = null;
            InvenSource?.Dispose();
            InvenSource = null;
            WP?.Dispose();
            WP = null;
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
            SectorVB?.Dispose();
            SectorVB = null;
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
            Obj?.Dispose();
            Obj = null;
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
        }

        private static void DifficultyChanged()
        {
            Difficulty = Globals.Config.GetVanillaInt("difficulty");
        }

// Makes a savegame.
        public static bool SaveGame(string filename, string displayName)
        {
            throw new NotImplementedException(); // TODO
        }

        public static bool SaveGameIronman()
        {
            if (mIronmanFlag && mIronmanSaveName != null)
            {
                var filename = $"iron{mIronmanSaveNumber:D4}";
                return SaveGame(filename, mIronmanSaveName);
            }

            return false;
        }

// Loads a game.
        public static bool LoadGame(string filename)
        {
            throw new NotImplementedException(); // TODO
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

        [TempleDllLocation(0x11E726AC)] public static TimePoint LastAdvanceTime { get; private set; }

        public static void AdvanceTime()
        {
            var now = TimePoint.Now;

            // This is used from somewhere in the object system
            LastAdvanceTime = now;

            foreach (var system in GetTimeAwareSystems())
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

            var preprocessor = new MapMobilePreprocessor(mModuleGuid);

            // Preprocess mob files for each map before we load the first map
            foreach (var entry in Tig.FS.ListDirectory("maps/*.*"))
            {
                var path = $"maps/{entry}";
                if (Tig.FS.DirectoryExists(path))
                {
                    continue;
                }

                preprocessor.Preprocess(path);
            }

            foreach (var system in GetModuleAwareSystems())
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

            mIronmanFlag = false;
            mIronmanSaveName = null;

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

        public static bool IsIronman()
        {
            return mIronmanFlag;
        }

        public static void SetIronman(bool enable)
        {
            mIronmanFlag = enable;
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
            GMovie.PlayMovie("movies/AtariLogo.bik", 0, 0, 0);
            GMovie.PlayMovie("movies/TroikaLogo.bik", 0, 0, 0);
            GMovie.PlayMovie("movies/WotCLogo.bik", 0, 0, 0);
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
            Stat = InitializeSystem(loadingScreen, () => new StatSystem());
            // Loading Screen ID: 12
            loadingScreen.SetProgress(14 / 79.0f);
            Script = InitializeSystem(loadingScreen, () => new ScriptSystem());
            loadingScreen.SetProgress(15 / 79.0f);
            Level = InitializeSystem(loadingScreen, () => new LevelSystem());
            loadingScreen.SetProgress(16 / 79.0f);
            D20 = InitializeSystem(loadingScreen, () => new D20System());
            // Loading Screen ID: 1
            loadingScreen.SetProgress(17 / 79.0f);
            Map = InitializeSystem(loadingScreen, () => new MapSystem(D20, Party));

            /* START Former Map Subsystems */
            loadingScreen.SetProgress(18 / 79.0f);
            Scroll = InitializeSystem(loadingScreen, () => new ScrollSystem());
            loadingScreen.SetProgress(19 / 79.0f);
            Location = InitializeSystem(loadingScreen, () => new LocationSystem());
            loadingScreen.SetProgress(20 / 79.0f);
            Light = InitializeSystem(loadingScreen, () => new LightSystem());
            loadingScreen.SetProgress(21 / 79.0f);
            Tile = InitializeSystem(loadingScreen, () => new TileSystem());
            loadingScreen.SetProgress(22 / 79.0f);
            OName = InitializeSystem(loadingScreen, () => new ONameSystem());
            loadingScreen.SetProgress(23 / 79.0f);
            ObjectNode = InitializeSystem(loadingScreen, () => new ObjectNodeSystem());
            loadingScreen.SetProgress(24 / 79.0f);
            Obj = InitializeSystem(loadingScreen, () => new ObjSystem());
            loadingScreen.SetProgress(25 / 79.0f);
            Proto = InitializeSystem(loadingScreen, () => new ProtoSystem());
            loadingScreen.SetProgress(26 / 79.0f);
            Object = InitializeSystem(loadingScreen, () => new ObjectSystem());
            loadingScreen.SetProgress(27 / 79.0f);
            MapSector = InitializeSystem(loadingScreen, () => new MapSectorSystem());
            loadingScreen.SetProgress(28 / 79.0f);
            SectorVB = InitializeSystem(loadingScreen, () => new SectorVBSystem());
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
            loadingScreen.SetProgress(43 / 79.0f);
            Combat = InitializeSystem(loadingScreen, () => new CombatSystem());
            loadingScreen.SetProgress(44 / 79.0f);
            TimeEvent = InitializeSystem(loadingScreen, () => new TimeEventSystem());
            loadingScreen.SetProgress(45 / 79.0f);
            Rumor = InitializeSystem(loadingScreen, () => new RumorSystem());
            loadingScreen.SetProgress(46 / 79.0f);
            Quest = InitializeSystem(loadingScreen, () => new QuestSystem());
            loadingScreen.SetProgress(47 / 79.0f);
            AI = InitializeSystem(loadingScreen, () => new AISystem());
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
            WP = InitializeSystem(loadingScreen, () => new WPSystem());
            loadingScreen.SetProgress(55 / 79.0f);

            InvenSource = InitializeSystem(loadingScreen, () => new InvenSourceSystem());
            loadingScreen.SetProgress(56 / 79.0f);
            TownMap = InitializeSystem(loadingScreen, () => new TownMapSystem());
            loadingScreen.SetProgress(57 / 79.0f);
            GMovie = InitializeSystem(loadingScreen, () => new GMovieSystem());
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

    public class VagrantSystem : IGameSystem, ITimeAwareSystem
    {
        public void Dispose()
        {
        }

        public void AdvanceTime(TimePoint time)
        {
            throw new NotImplementedException();
        }
    }

    public class DescriptionSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
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
    }

    public class ItemEffectSystem : IGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }
    }

    public class TeleportSystem : IGameSystem, IResetAwareSystem, ITimeAwareSystem
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
            throw new NotImplementedException();
        }
    }

    public class SectorSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
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
    }

    public class RandomSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class CritterSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        public void SetStandPoint(in ObjHndl newHandle, StandPointType type, StandPoint standpoint)
        {
            throw new NotImplementedException();
        }

        public void GenerateHp(in ObjHndl objHandle)
        {
            throw new NotImplementedException();
        }
    }

    public class ScriptNameSystem : IGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }
    }

    public class PortraitSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class SkillSystem : IGameSystem, ISaveGameAwareGameSystem
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

    public class FeatSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class SpellSystem : IGameSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public string GetSpellName(in uint sp1SpellEnum)
        {
            throw new NotImplementedException();
        }
    }

    public class StatSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class ScriptSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
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
    }

    public class LevelSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class D20System : IGameSystem, IResetAwareSystem, ITimeAwareSystem
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
            throw new NotImplementedException();
        }
    }

    public enum MapType : uint {
        None,
        StartMap,
        ShoppingMap,
        TutorialMap,
        ArenaMap // new in Temple+
    }

    public class MapSystem : IGameSystem
    {
        public MapSystem(D20System d20, PartySystem party)
        {
        }

        public MapObjectSystem MapObject { get; }

        // TODO: Might now be the normal ObjectSystem or ObjSystem
        public class MapObjectSystem
        {
            public void Move(in ObjHndl objHandle, LocAndOffsets newLocation)
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
        }

        public void CloseMap()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentMapId()
        {
            throw new NotImplementedException();
        }

        public locXY GetStartPos(in int mapId)
        {
            throw new NotImplementedException();
        }

        public int GetMapIdByType(MapType mapType)
        {
            throw new NotImplementedException();
        }

        public int GetEnterMovie(in int mapId, bool ignoreVisited)
        {
            throw new NotImplementedException();
        }
    }

    public class ScrollSystem : IGameSystem
    {

        [TempleDllLocation(0x10005E70)]
        public ScrollSystem()
        {
            // TODO
        }

        [TempleDllLocation(0x10006480)]
        public void SetScrollDirection(int scrollDir)
        {
        }

        [TempleDllLocation(0x102AC238)]
        public int ScrollButter { get; private set; }

        public void Dispose()
        {
        }
    }

    public class LocationSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class LightSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class TileSystem : IGameSystem
    {
        public void Dispose()
        {
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

    public class ObjSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class ProtoSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class MapSectorSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        public void RemoveSectorLight(in ObjHndl handle)
        {
            throw new NotImplementedException();
        }
    }

    public class SectorVBSystem : IGameSystem
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
    }

    public class TextFloaterSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class JumpPointSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class TerrainSystem : IGameSystem
    {
        public TerrainSystem(RenderingDevice device, ShapeRenderer2d shapeRenderer2d)
        {
        }

        public void Dispose()
        {
        }
    }

    public class ClippingSystem : IGameSystem
    {
        public ClippingSystem(RenderingDevice device)
        {
        }

        public void Dispose()
        {
        }
    }

    public class HeightSystem : IGameSystem
    {
        public void Dispose()
        {
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
    }

    public class PathNodeSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class LightSchemeSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
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
    }

    public class AreaSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
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
    }

    public class DialogSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class SoundMapSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class SoundGameSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem,
        ITimeAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
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

        public void AdvanceTime(TimePoint time)
        {
            throw new NotImplementedException();
        }

        public void StopAll(bool b)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemSystem : IGameSystem, IBufferResettingSystem
    {
        public void Dispose()
        {
        }
    }

    public class CombatSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
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

        public void AdvanceTime(TimePoint time)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeEventSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class RumorSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
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
    }

    public class QuestSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
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
    }

    public class AISystem : IGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        public void AddAiTimer(in ObjHndl handle)
        {
            throw new NotImplementedException();
        }
    }

    public class AnimSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        public void PushDisableFidget()
        {
            throw new NotImplementedException();
        }

        public void PopDisableFidget()
        {
            throw new NotImplementedException();
        }

        public void StartFidgetTimer()
        {
            throw new NotImplementedException();
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
    }

    public class ReactionSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class TileScriptSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class SectorScriptSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class WPSystem : IGameSystem, IBufferResettingSystem
    {
        public void Dispose()
        {
        }
    }

    public class InvenSourceSystem : IGameSystem
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
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class GMovieSystem : IGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10034100)]
        public void PlayMovie(string path, int p1, int p2, int p3)
        {
            throw new NotImplementedException(); // TODO
        }

        /// <summary>
        /// Plays a movie from movies.mes, which could either be a slide or binkw movie.
        /// The soundtrack id is used for BinkW movies with multiple soundtracks.
        /// As far as we know, this is not used at all in ToEE.
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="flags"></param>
        /// <param name="soundtrackId"></param>
        public void PlayMovieId(int movieId, int flags, int soundtrackId)
        {
            throw new NotImplementedException(); // TODO
        }

        public void MovieQueueAdd(int movieId)
        {
            throw new NotImplementedException();
        }

        public void MovieQueuePlay()
        {
            throw new NotImplementedException();
        }
    }

    public class BrightnessSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class AntiTeleportSystem : IGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }
    }

    public class TrapSystem : IGameSystem
    {
        public void Dispose()
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
    }

    public class PartySystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
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

        public void AddToPCGroup(in ObjHndl objHndl)
        {
            throw new NotImplementedException();
        }

        public ObjHndl GetLeader()
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

    public class GameInitSystem : IGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class ObjFadeSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class DeitySystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class UiArtManagerSystem : IGameSystem
    {
        public void Dispose()
        {
        }
    }

    public class ParticleSysSystem : IGameSystem
    {
        public ParticleSysSystem(WorldCamera camera)
        {
        }

        public void Dispose()
        {
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
    }

    public class SecretdoorSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
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
    }

    public class MapFoggingSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem
    {
        public MapFoggingSystem(RenderingDevice renderingDevice)
        {
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    public class RandomEncounterSystem : IGameSystem, ISaveGameAwareGameSystem
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

    public class ObjectEventSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
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

        public void AdvanceTime(TimePoint time)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10045290)]
        public void NotifyMoved(in ObjHndl handle, LocAndOffsets fromLoc, LocAndOffsets toLoc)
        {
            throw new NotImplementedException();
            /*
             *
            static var objevent_notify_moved =
            temple.GetPointer <void (ObjHndl, LocAndOffsets, LocAndOffsets) > (0x10045290);
            objevent_notify_moved(handle, fromLoc, toLoc);
             */
        }
    }

    public class FormationSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
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
            throw new NotImplementedException();
        }
    }

    public class PathXSystem : IGameSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}