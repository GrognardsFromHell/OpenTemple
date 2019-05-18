using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SharpDX.Direct3D11;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TroikaArchives;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Fade;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.Protos;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.Teleport;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;

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
        public static AiSystem AI { get; private set; }
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

        public static RollHistorySystem RollHistory { get; private set; }

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

            // TODO gameSystemInitTable.InitPfxLightning();
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
            Stat = InitializeSystem(loadingScreen, () => new D20StatSystem());
            // Loading Screen ID: 12
            loadingScreen.SetProgress(14 / 79.0f);
            Script = InitializeSystem(loadingScreen, () => new ScriptSystem());
            loadingScreen.SetProgress(15 / 79.0f);
            Level = InitializeSystem(loadingScreen, () => new LevelSystem());
            loadingScreen.SetProgress(16 / 79.0f);
            D20 = InitializeSystem(loadingScreen, () => new D20System());
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
            MapObject = InitializeSystem(loadingScreen, () => new MapObjectSystem());
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

            RollHistory = new RollHistorySystem();
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
            // TODO
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

    public class RandomSystem : IGameSystem
    {
        private static readonly Random _random = new Random();

        public void Dispose()
        {
        }

        public static int GetInt(int fromInclusive, int toInclusive)
        {
            return _random.Next(fromInclusive, toInclusive + 1);
        }
    }

    // TODO: This entire system may also be unused because old scripts are not used anymore
    public class ScriptNameSystem : IGameSystem, IModuleAwareSystem
    {
        private readonly Dictionary<int, string> _scriptIndex = new Dictionary<int, string>();

        private readonly Dictionary<int, string> _scriptModuleIndex = new Dictionary<int, string>();

        private static readonly Regex ScriptNamePattern = new Regex(@"^(\d+).*\.scr$");

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
        }

        private bool IsModuleScriptId(int scriptId) => scriptId >= 1 && scriptId < 1000;

        private bool IsGlobalScriptId(int scriptId) => scriptId >= 1000;

        [TempleDllLocation(0x1007e1b0)]
        public void UnloadModule()
        {
            _scriptModuleIndex.Clear();
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

    public class SkillSystem : IGameSystem, ISaveGameAwareGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly Dictionary<SkillId, string> _skillNames = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillId, string> _skillNamesEnglish = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillId, string> _skillShortDescriptions = new Dictionary<SkillId, string>();

        private readonly Dictionary<string, SkillId> _skillByEnumNames = new Dictionary<string, SkillId>();
        private readonly Dictionary<SkillId, string> _skillEnumNames = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillId, string> _skillHelpTopics = new Dictionary<SkillId, string>();

        private readonly Dictionary<SkillMessageId, string> _skillMessages = new Dictionary<SkillMessageId, string>();

        [TempleDllLocation(0x1007cfa0)]
        public SkillSystem()
        {
            Globals.Config.AddVanillaSetting("follower skills", "1");

            var localization = Tig.FS.ReadMesFile("mes/skill.mes");
            var skillRules = Tig.FS.ReadMesFile("rules/skill.mes");

            for (int i = 0; i < 42; i++)
            {
                // These two are localized
                _skillNames[(SkillId) i] = localization[i];
                _skillShortDescriptions[(SkillId) i] = localization[5000 + i];

                // This is the original english name
                _skillNamesEnglish[(SkillId) i] = skillRules[i];

                // Maps names such as skill_appraise to the actual enum
                _skillByEnumNames[skillRules[200 + i]] = (SkillId) i;
                _skillEnumNames[(SkillId) i] = skillRules[200 + i];

                // Help topics are optional
                var helpTopic = skillRules[10200 + i];
                if (!string.IsNullOrWhiteSpace(helpTopic))
                {
                    _skillHelpTopics[(SkillId) i] = helpTopic;
                }
            }

            foreach (var msgType in Enum.GetValues(typeof(SkillMessageId)))
            {
                _skillMessages[(SkillMessageId) msgType] = localization[1000 + (int) msgType];
            }
        }

        public string GetSkillEnumName(SkillId skill) => _skillEnumNames[skill];

        [TempleDllLocation(0x1007d2b0)]
        public string GetSkillEnglishName(SkillId skill) => _skillNamesEnglish[skill];

        [TempleDllLocation(0x1007d210)]
        public void ShowSkillMessage(GameObjectBody obj, SkillMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public string GetSkillMessage(SkillMessageId messageId) => _skillMessages[messageId];

        [TempleDllLocation(0x1007d2f0)]
        public bool GetSkillIdFromEnglishName(string enumName, out SkillId skillId)
        {
            foreach (var entry in _skillNamesEnglish)
            {
                if (entry.Value.Equals(enumName, StringComparison.InvariantCultureIgnoreCase))
                {
                    skillId = entry.Key;
                    return true;
                }
            }

            skillId = default;
            return false;
        }

        [TempleDllLocation(0x1007d2c0)]
        public string GetHelpTopic(SkillId skillId)
        {
            return _skillHelpTopics.GetValueOrDefault(skillId, null);
        }

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

        [TempleDllLocation(0x1007daa0)]
        public int AddSkillRanks(GameObjectBody obj, SkillId skillId, int ranksToAdd)
        {
            // Get the last class the character leveled up as
            var levClass = Stat.level_fighter; // default
            var numClasses = obj.GetInt32Array(obj_f.critter_level_idx).Count;
            if (numClasses > 0)
            {
                levClass = (Stat) obj.GetInt32(obj_f.critter_level_idx, numClasses - 1);
            }

            if (D20ClassSystem.IsClassSkill(skillId, levClass) ||
                (levClass == Stat.level_cleric && DeitySystem.IsDomainSkill(obj, skillId))
                || GameSystems.D20.D20QueryPython(obj, "Is Class Skill", skillId) != 0)
            {
                ranksToAdd *= 2;
            }

            var skillPtNew = ranksToAdd + obj.GetInt32(obj_f.critter_skill_idx, (int) skillId);
            if (obj.IsPC())
            {
                var expectedMax = 2 * GameSystems.Stat.StatLevelGet(obj, Stat.level) + 6;
                if (skillPtNew > expectedMax)
                    Logger.Warn("PC {0} has more skill points than they should (has: {1} , expected: {2}",
                        obj, skillPtNew, expectedMax);
            }

            obj.SetInt32(obj_f.critter_skill_idx, (int) skillId, skillPtNew);
            return skillPtNew;
        }
    }

    public struct LevelupPacket
    {
        public int flags;
        public int classCode;
        public Stat abilityScoreRaised;
        public Dictionary<FeatId, int> feats;

        /// array keeping track of how many skill pts were added to each skill
        public Dictionary<SkillId, int> skillPointsAdded;

        public int spellEnumCount; // how many spells were learned (added to spells_known)
        public Dictionary<int, int> spellEnums;
        public int spellEnumToRemove; // spell removed (for Sorcerers)
    };

    public class LevelSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x100731e0)]
        public void LevelUpApply(GameObjectBody obj, in LevelupPacket levelUpPacket)
        {
            var array = obj.GetInt32Array(obj_f.critter_level_idx);
            obj.SetInt32(obj_f.critter_level_idx, array.Count, levelUpPacket.classCode);

            Stub.TODO();
        }
    }

    public enum MapType : uint
    {
        None,
        StartMap,
        ShoppingMap,
        TutorialMap,
        ArenaMap // new in Temple+
    }

    /// <summary>
    /// Formerly known as "map daylight system"
    /// </summary>
    public class LightSystem : IGameSystem, IBufferResettingSystem
    {
        [TempleDllLocation(0x11869200)]
        public LegacyLight GlobalLight { get; private set; }

        [TempleDllLocation(0x118691E0)]
        public bool IsGlobalLightEnabled { get; private set; }

        [TempleDllLocation(0x10B5DC80)]
        public bool IsNight { get; private set; }

        [TempleDllLocation(0x100a7d40)]
        public LightSystem()
        {
            // TODO
        }

        [TempleDllLocation(0x100a5b30)]
        public void ResetBuffers()
        {
            // TODO
        }

        [TempleDllLocation(0x100a7860)]
        public void Load(string dataDir)
        {
            // TODO
        }

        // Sets the info from daylight.mes based on map id
        [TempleDllLocation(0x100a7040)]
        public void SetMapId(int mapId)
        {
            // TODO
        }

        [TempleDllLocation(0x100a7f80)]
        public void Dispose()
        {
            // TODO
        }

        [TempleDllLocation(0x100A85F0)]
        public void RemoveAttachedTo(GameObjectBody obj)
        {
            var renderFlags = obj.GetUInt32(obj_f.render_flags);
            if ((renderFlags & 0x80000000) != 0)
            {
                var lightHandle = obj.GetInt32(obj_f.light_handle);
                if (lightHandle != 0)
                {
                    // TODO: Free sector light
                    throw new NotImplementedException();
                }

                obj.SetUInt32(obj_f.render_flags, renderFlags & ~0x80000000);
            }
        }

        [TempleDllLocation(0x100a88c0)]
        public void SetColors(PackedLinearColorA indoorColor, PackedLinearColorA outdoorColor)
        {
            // TODO
        }

        [TempleDllLocation(0x100a75e0)]
        public void UpdateDaylight()
        {
            // TODO light
        }
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

    public class SectorVBSystem : IGameSystem
    {
        [TempleDllLocation(0x11868FA0)]
        private string _dataDir;

        [TempleDllLocation(0x118690C0)]
        private string _saveDir;

        public void Dispose()
        {
        }

        [TempleDllLocation(0x100aa430)]
        public void SetDirectories(string dataDir, string saveDir)
        {
            _dataDir = dataDir;
            _saveDir = saveDir;
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

    public class JumpPointSystem : IGameSystem
    {
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

        [TempleDllLocation(0x100A4FB0)]
        public void Render()
        {
            // TODO
        }

        public void Load(string dataDir)
        {
            // TODO
        }
    }

    public class HeightSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        public sbyte GetDepth(LocAndOffsets parentLoc)
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

    public class PathNodeSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x100a9720)]
        public void SetDataDirs(string dataDir, string saveDir)
        {
            // TODO
        }

        public void Load(string dataDir, string saveDir)
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
    }

    public class SoundGameSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem,
        ITimeAwareSystem
    {
        [TempleDllLocation(0x1003d4a0)]
        public SoundGameSystem()
        {
            // TODO SOUND
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
        public void Sound(int soundId, int a2)
        {
            Stub.TODO();
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

        [TempleDllLocation(0x1003C8B0)]
        public void StopCombatMusic(GameObjectBody handle)
        {
            Stub.TODO();
        }
    }

    public class RumorSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem
    {
        [TempleDllLocation(0x1005f960)]
        public RumorSystem()
        {
        }

        [TempleDllLocation(0x1005f9d0)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x101f5850)]
        public void LoadModule()
        {
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101f5850)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101f5850)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }
    }

    public class QuestSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        public QuestSystem()
        {
            // TODO quests
        }

        public void Dispose()
        {
            // TODO quests
        }

        [TempleDllLocation(0x1005f310)]
        public void LoadModule()
        {
            Reset();
        }

        public void UnloadModule()
        {
            // TODO quests
        }

        [TempleDllLocation(0x1005f2a0)]
        public void Reset()
        {
            // TODO quests
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

        public void ResetBuffers()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100521b0)]
        public void Flush()
        {
            Stub.TODO();
        }
    }

    public class GMovieSystem : IGameSystem, IModuleAwareSystem
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

        public void MovieQueueAdd(int movieId)
        {
            Stub.TODO();
        }

        public void MovieQueuePlay()
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

        public void ResetBuffers()
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

        private Dictionary<DeityId, string> _deityNames;

        [TempleDllLocation(0x1004a760)]
        public DeitySystem()
        {
            var deityMes = Tig.FS.ReadMesFile("mes/deity.mes");

            _deityNames = new Dictionary<DeityId, string>(DeityIds.Deities.Length);
            foreach (var deityId in DeityIds.Deities)
            {
                _deityNames[deityId] = deityMes[(int) deityId];
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
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x1004a890)]
        public string GetName(DeityId deity)
        {
            return _deityNames[deity];
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
    }

    public class ParticleSysSystem : IGameSystem
    {
        public ParticleSysSystem(WorldCamera camera)
        {
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x101e7e00)]
        public void InvalidateObject(GameObjectBody obj)
        {
            //for (var &sys : particles) {
            //    if (sys.second->GetAttachedTo() == obj.handle) {
            //      sys.second->SetAttachedTo(0);
            //      sys.second->EndPrematurely();
            //  }
            //}
            Stub.TODO();
        }

        [TempleDllLocation(0x10049be0)]
        public void Remove(object partSysHandle)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10049bd0)]
        public object CreateAt(in int hashCode, Vector3 centerOfTile)
        {
            // TODO
            return null;
        }

        [TempleDllLocation(0x101e78a0)]
        public void RemoveAll()
        {
            Stub.TODO();
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

        [TempleDllLocation(0x10046b70)]
        public void AfterTeleportStuff(locXY loc)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10046ea0)]
        public void QueueSearchTimer(GameObjectBody obj)
        {
            GameSystems.TimeEvent.Remove(TimeEventType.Search, evt => evt.arg1.handle == obj);

            var newEvt = new TimeEvent(TimeEventType.Search);
            newEvt.arg1.handle = obj;
            newEvt.arg2.int32 = 1;
            GameSystems.TimeEvent.ScheduleNow(newEvt);
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

        [TempleDllLocation(0x100336B0)]
        public void PerformFogChecks()
        {
            // TODO
        }

        public void ResetBuffers()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002ECB0)]
        public byte GetFogStatus(locXY loc, float offsetX, float offsetY)
        {
            return 0xFF;
        }

        public void SaveEsd()
        {
            Stub.TODO();
        }

        public void SaveExploredTileData(int id)
        {
            Stub.TODO();
        }

        public void LoadFogColor(string dataDir)
        {
            // TODO
        }

        public void Disable()
        {
            // TODO
        }

        public void Enable()
        {
            // TODO
        }

        public void LoadExploredTileData(in int mapEntryId)
        {
            // TODO
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

        [TempleDllLocation(0x10045850)]
        public void UpdateSleepStatus()
        {
            Stub.TODO();
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
            // TODO
        }

        [TempleDllLocation(0x10045290)]
        public void NotifyMoved(GameObjectBody obj, LocAndOffsets fromLoc, LocAndOffsets toLoc)
        {
            Stub.TODO();
            /*
             *
            static var objevent_notify_moved =
            temple.GetPointer <void (ObjHndl, LocAndOffsets, LocAndOffsets) > (0x10045290);
            objevent_notify_moved(handle, fromLoc, toLoc);
             */
        }

        public void FlushEvents()
        {
            Stub.TODO();
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
            // TODO
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