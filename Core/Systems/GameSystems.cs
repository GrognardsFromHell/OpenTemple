using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.Config;
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Protos;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public static class GameSystems
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x103072B8)]
        private static bool mIronmanFlag;

        [TempleDllLocation(0x10306F44)]
        private static int mIronmanSaveNumber;

        [TempleDllLocation(0x103072C0)]
        private static string mIronmanSaveName;

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

    public class CritterSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public void Dispose()
        {
        }

        public void SetStandPoint(in ObjHndl newHandle, StandPointType type, StandPoint standpoint)
        {
            throw new NotImplementedException();
        }

        public void GenerateHp(ObjHndl objHandle) => GenerateHp(GameSystems.Object.GetObject(objHandle));

        [TempleDllLocation(0x1007F720)]
        public void GenerateHp(GameObjectBody obj)
        {
            var hpPts = 0;
            var critterLvlIdx = 0;

            var conMod = 0;
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Has_No_Con_Score) == 0)
            {
                int conScore;

                var dispatcher = obj.GetDispatcher();
                if (dispatcher != null)
                {
                    conScore = GameSystems.Stat.DispatchForCritter(obj, null, DispatcherType.StatBaseGet,
                        D20DispatcherKey.STAT_CONSTITUTION);
                }
                else
                {
                    conScore = GameSystems.Stat.ObjStatBaseGet(obj, Stat.constitution);
                }

                conMod = D20StatSystem.GetModifierForAbilityScore(conScore);
            }

            var numLvls = obj.GetInt32Array(obj_f.critter_level_idx).Count;
            for (var i = 0; i < numLvls; i++)
            {
                var classType = (Stat) obj.GetInt32(obj_f.critter_level_idx, i);
                var classHd = D20ClassSystem.GetClassHitDice(classType);
                if (i == 0)
                {
                    hpPts = classHd; // first class level gets full HP
                }
                else
                {
                    int hdRoll;
                    if (Globals.Config.HpOnLevelUpMode == HpOnLevelUpMode.Max)
                    {
                        hdRoll = classHd;
                    }
                    else if (Globals.Config.HpOnLevelUpMode == HpOnLevelUpMode.Average)
                    {
                        // hit die are always even numbered so randomize the roundoff
                        hdRoll = classHd / 2 + RandomSystem.GetInt(0, 1);
                    }
                    else
                    {
                        hdRoll = Dice.Roll(1, classHd);
                    }

                    if (hdRoll + conMod < 1)
                    {
                        // note: the con mod is applied separately! This just makes sure it doesn't dip to negatives
                        hdRoll = 1 - conMod;
                    }

                    hpPts += hdRoll;
                }
            }

            var racialHd = D20RaceSystem.GetHitDice(GameSystems.Critter.GetRace(obj, false));
            if (racialHd.IsValid)
            {
                hpPts += racialHd.Roll();
            }

            if (obj.IsNPC())
            {
                var numDice = obj.GetInt32(obj_f.npc_hitdice_idx, 0);
                var sides = obj.GetInt32(obj_f.npc_hitdice_idx, 1);
                var modifier = obj.GetInt32(obj_f.npc_hitdice_idx, 2);
                var npcHd = new Dice(numDice, sides, modifier);
                var npcHdVal = npcHd.Roll();
                if (Globals.Config.MaxHpForNpcHitdice)
                {
                    npcHdVal = numDice * npcHd.Sides + npcHd.Modifier;
                }

                if (npcHdVal + conMod * numDice < 1)
                    npcHdVal = numDice * (1 - conMod);
                hpPts += npcHdVal;
            }

            if (hpPts < 1)
            {
                hpPts = 1;
            }

            obj.SetInt32(obj_f.hp_pts, hpPts);
        }

        public bool IsDeadNullDestroyed(ObjHndl handle) => IsDeadNullDestroyed(GameSystems.Object.GetObject(handle));

        public bool IsDeadNullDestroyed(GameObjectBody critter)
        {
            if (critter == null)
            {
                return true;
            }

            var flags = critter.GetFlags();
            if (flags.HasFlag(ObjectFlag.DESTROYED))
            {
                return true;
            }

            return GameSystems.Stat.StatLevelGet(critter, Stat.hp_current) <= -10;
        }

        public bool IsDeadOrUnconscious(ObjHndl handle) => IsDeadOrUnconscious(GameSystems.Object.GetObject(handle));

        public bool IsDeadOrUnconscious(GameObjectBody critter)
        {
            if (IsDeadNullDestroyed(critter))
            {
                return true;
            }

            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Unconscious) != 0;
        }

        public bool IsProne(ObjHndl handle) => IsProne(GameSystems.Object.GetObject(handle));

        public bool IsProne(GameObjectBody critter)
        {
            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Prone) != 0;
        }

        public bool IsMovingSilently(ObjHndl handle) => IsMovingSilently(GameSystems.Object.GetObject(handle));

        public bool IsMovingSilently(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.MOVING_SILENTLY);
        }

        public bool IsCombatModeActive(ObjHndl handle) => IsCombatModeActive(GameSystems.Object.GetObject(handle));

        public bool IsCombatModeActive(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE);
        }

        public bool IsConcealed(ObjHndl handle) => IsConcealed(GameSystems.Object.GetObject(handle));

        public bool IsConcealed(GameObjectBody critter)
        {
            var flags = critter.GetCritterFlags();
            return flags.HasFlag(CritterFlag.IS_CONCEALED);
        }

        public EncodedAnimId GetAnimId(ObjHndl handle, WeaponAnim weaponAnim) =>
            GetAnimId(GameSystems.Object.GetObject(handle), weaponAnim);

        public EncodedAnimId GetAnimId(GameObjectBody critter, WeaponAnim animType)
        {
            var weaponPrim = GetWornItem(critter, EquipSlot.WeaponPrimary);
            var weaponSec = GetWornItem(critter, EquipSlot.WeaponSecondary);
            if (weaponSec == null)
            {
                weaponSec = GetWornItem(critter, EquipSlot.Shield);
            }

            return GetWeaponAnim(critter, weaponPrim, weaponSec, animType);
        }

        [TempleDllLocation(0x10020B60)]
        public EncodedAnimId GetWeaponAnim(GameObjectBody wielder, GameObjectBody primaryWeapon,
            GameObjectBody secondaryWeapon, WeaponAnim animType)
        {
            var mainHandAnim = WeaponAnimType.Unarmed;
            var offHandAnim = WeaponAnimType.Unarmed;
            var ignoreOffHand = false;

            if (primaryWeapon != null)
            {
                if (primaryWeapon.type == ObjectType.weapon)
                {
                    mainHandAnim = GameSystems.Item.GetWeaponAnimType(primaryWeapon, wielder);
                }
                else if (primaryWeapon.type == ObjectType.armor)
                {
                    mainHandAnim = WeaponAnimType.Shield;
                }

                if (GameSystems.Item.GetWieldType(wielder, primaryWeapon) == 2)
                {
                    offHandAnim = mainHandAnim;
                    ignoreOffHand = true;
                }
            }

            if (!ignoreOffHand && secondaryWeapon != null)
            {
                if (secondaryWeapon.type == ObjectType.weapon)
                {
                    offHandAnim = GameSystems.Item.GetWeaponAnimType(secondaryWeapon, wielder);
                }
                else if (secondaryWeapon.type == ObjectType.armor)
                {
                    offHandAnim = WeaponAnimType.Shield;
                }
            }

            // If the user is fully unarmed and has unarmed strike, we'll show the monk stance
            if (mainHandAnim == WeaponAnimType.Unarmed
                && offHandAnim == WeaponAnimType.Unarmed
                && GameSystems.Feat.HasFeat(wielder, FeatId.IMPROVED_UNARMED_STRIKE))
            {
                offHandAnim = WeaponAnimType.Monk;
                mainHandAnim = WeaponAnimType.Monk;
            }

            return new EncodedAnimId(animType, mainHandAnim, offHandAnim);
        }

        public GameObjectBody GetWornItem(GameObjectBody critter, EquipSlot slot)
        {
            return GameSystems.Item.ItemWornAt(critter, slot);
        }

        public int GetCasterLevel(GameObjectBody obj)
        {
            int result = 0;
            foreach (var classEnum in D20ClassSystem.AllClasses)
            {
                if (D20ClassSystem.IsCastingClass(classEnum))
                {
                    var cl = GetCasterLevelForClass(obj, classEnum);
                    if (cl > result)
                        result = cl;
                }
            }

            return result;
        }

        public int GetCasterLevelForClass(GameObjectBody obj, Stat classCode)
        {
            return DispatchGetBaseCasterLevel(obj, classCode);
        }

        public int DispatchGetBaseCasterLevel(GameObjectBody obj, Stat casterClass)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var evtObj = EvtObjSpellCaster.Default;
            evtObj.handle = obj;
            evtObj.arg0 = casterClass;
            dispatcher.Process(DispatcherType.GetBaseCasterLevel, D20DispatcherKey.NONE, evtObj);
            return evtObj.bonlist.OverallBonus;
        }

        public int GetSpellListLevelExtension(GameObjectBody handle, Stat classCode)
        {
            return DispatchSpellListLevelExtension(handle, classCode);
        }

        private int DispatchSpellListLevelExtension(GameObjectBody handle, Stat casterClass)
        {
            var dispatcher = handle.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var evtObj = EvtObjSpellCaster.Default;
            evtObj.handle = handle;
            evtObj.arg0 = casterClass;
            dispatcher.Process(DispatcherType.SpellListExtension, D20DispatcherKey.NONE, evtObj); // TODO REF OUT

            return evtObj.bonlist.OverallBonus;
        }

        public int GetBaseAttackBonus(GameObjectBody obj, Stat classBeingLeveled = default)
        {
            var bab = 0;
            foreach (var it in D20ClassSystem.AllClasses)
            {
                var classLvl = GameSystems.Stat.StatLevelGet(obj, it);
                if (classBeingLeveled == it)
                    classLvl++;
                bab += D20ClassSystem.GetBaseAttackBonus(it, classLvl);
            }

            // get BAB from NPC HD
            if (obj.type == ObjectType.npc)
            {
                var npcHd = obj.GetInt32(obj_f.npc_hitdice_idx, 0);
                var moncat = GameSystems.Critter.GetCategory(obj);
                switch (moncat)
                {
                    case MonsterCategory.aberration:
                    case MonsterCategory.animal:
                    case MonsterCategory.beast:
                    case MonsterCategory.construct:
                    case MonsterCategory.elemental:
                    case MonsterCategory.giant:
                    case MonsterCategory.humanoid:
                    case MonsterCategory.ooze:
                    case MonsterCategory.plant:
                    case MonsterCategory.shapechanger:
                    case MonsterCategory.vermin:
                        return bab + (3 * npcHd / 4);


                    case MonsterCategory.dragon:
                    case MonsterCategory.magical_beast:
                    case MonsterCategory.monstrous_humanoid:
                    case MonsterCategory.outsider:
                        return bab + npcHd;

                    case MonsterCategory.fey:
                    case MonsterCategory.undead:
                        return bab + npcHd / 2;

                    default: break;
                }
            }

            return bab;
        }

        public MonsterCategory GetCategory(GameObjectBody obj)
        {
            if (obj.IsCritter())
            {
                var monCat = obj.GetInt64(obj_f.critter_monster_category);
                return (MonsterCategory) (monCat & 0xFFFFFFFF);
            }

            return MonsterCategory.monstrous_humanoid; // default - so they have at least a weapons proficiency
        }

        public bool IsCategoryType(GameObjectBody obj, MonsterCategory category)
        {
            if (obj != null && obj.IsCritter())
            {
                var monsterCategory = GetCategory(obj);
                return monsterCategory == category;
            }

            return false;
        }

        public RaceId GetRace(GameObjectBody obj, bool baseRace)
        {
            var race = GameSystems.Stat.StatLevelGet(obj, Stat.race);
            if (!baseRace)
            {
                race += GameSystems.Stat.StatLevelGet(obj, Stat.subrace) << 5;
            }

            return (RaceId) race;
        }

        public void UpdateModelEquipment(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1001f3b0)]
        public IEnumerable<GameObjectBody> GetFollowers(GameObjectBody obj)
        {
            var objArray = obj.GetObjectIdArray(obj_f.critter_follower_idx);

            var result = new List<GameObjectBody>(objArray.Count);
            for (var i = 0; i < objArray.Count; i++)
            {
                var follower = GameSystems.Object.GetObject(objArray[i]);
                if (follower != null)
                {
                    result.Add(follower);
                }
            }

            return result;
        }

        [TempleDllLocation(0x10080c20)]
        public void RemoveFollowerFromLeaderCritterFollowers(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        private Dictionary<int, ImmutableList<string>> _addMeshes;

        /// <summary>
        /// This is called initially when the model is loaded for an object and adds NPC specific add meshes.
        /// </summary>
        public void AddNpcAddMeshes(GameObjectBody obj)
        {
            var id = obj.GetInt32(obj_f.npc_add_mesh);
            var model = obj.GetOrCreateAnimHandle();

            foreach (var addMesh in GetAddMeshes(id, 0))
            {
                model.AddAddMesh(addMesh);
            }
        }

        private IImmutableList<string> GetAddMeshes(int matIdx, int raceOffset)
        {
            if (_addMeshes == null)
            {
                var mapping = Tig.FS.ReadMesFile("rules/addmesh.mes");
                _addMeshes = new Dictionary<int, ImmutableList<string>>(mapping.Count);
                foreach (var (key, line) in mapping)
                {
                    _addMeshes[key] = line.Split(";", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToImmutableList();
                }
            }

            if (_addMeshes.TryGetValue(matIdx + raceOffset, out var materials))
            {
                return materials;
            }

            return ImmutableList<string>.Empty;
        }

        [TempleDllLocation(0x101391c0)]
        public bool IsLootableCorpse(GameObjectBody obj)
        {
            if (!obj.IsCritter())
            {
                return false;
            }

            if (!GameSystems.Critter.IsDeadNullDestroyed(obj))
            {
                return false; // It's still alive
            }

            // Find any item in the critters inventory that would be considered lootable
            foreach (var item in obj.EnumerateChildren())
            {
                if (item.GetItemFlags().HasFlag(ItemFlag.NO_LOOT))
                {
                    continue; // Flagged as unlootable
                }

                // ToEE previously excluded worn items here, but we'll consider all items

                return true; // Found an item that is lootable
            }

            return false;
        }

        [TempleDllLocation(0x10080490)]
        public void UpdateNpcHealingTimers()
        {
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                if (obj.IsNPC())
                {
                    UpdateSubdualHealingTimer(obj, true);
                    UpdateNormalHealingTimer(obj, true);
                }
            }
        }

        private bool _isRemovingSubdualHealingTimers;

        [TempleDllLocation(0x1007edc0)]
        private void UpdateSubdualHealingTimer(GameObjectBody obj, bool applyQueuedHealing)
        {
            if (_isRemovingSubdualHealingTimers)
            {
                return; // Could lead to infinite recursion
            }

            GameSystems.TimeEvent.Remove(TimeEventType.SubdualHealing, evt =>
            {
                var timerObj = evt.arg1.handle;
                if (timerObj == obj)
                {
                    if (applyQueuedHealing)
                    {
                        _isRemovingSubdualHealingTimers = true;
                        CritterHealSubdualDamageOverTime(timerObj, evt.arg2.timePoint);
                        _isRemovingSubdualHealingTimers = false;
                    }

                    return true;
                }

                return false;
            });

            var newEvt = new TimeEvent();
            newEvt.system = TimeEventType.SubdualHealing;
            newEvt.arg1.handle = obj;
            newEvt.arg2.timePoint = GameSystems.TimeEvent.GameTime;
            GameSystems.TimeEvent.Schedule(newEvt, TimeSpan.FromHours(1), out _);
        }

        [TempleDllLocation(0x1007EBD0)]
        private void CritterHealSubdualDamageOverTime(GameObjectBody obj, TimePoint lastHealing)
        {
            var flags = obj.GetFlags();

            if (IsDeadNullDestroyed(obj) || flags.HasFlag(ObjectFlag.DONTDRAW) || flags.HasFlag(ObjectFlag.OFF))
            {
                return;
            }

            if (!GameSystems.Party.IsInParty(obj) && obj.GetInt32(obj_f.critter_subdual_damage) > 0)
            {
                // Heal one hit point of subdual damage per level and hour elapsed
                var hoursElapsed = (int) (GameSystems.TimeEvent.GameTime - lastHealing).TotalHours;
                if (hoursElapsed < 1 && !_isRemovingSubdualHealingTimers)
                {
                    hoursElapsed = 1;
                }

                var levels = GameSystems.Stat.StatLevelGet(obj, Stat.level);
                if (levels < 1)
                {
                    levels = 1;
                }

                HealSubdualSub_100B9030(obj, hoursElapsed * levels);
            }
        }

        [TempleDllLocation(0x100B9030)]
        private void HealSubdualSub_100B9030(GameObjectBody obj, int amount)
        {
            throw new NotImplementedException();
        }

        private bool _isRemovingHealingTimers;

        [TempleDllLocation(0x1007f140)]
        private void UpdateNormalHealingTimer(GameObjectBody obj, bool applyQueuedHealing)
        {
            if (_isRemovingHealingTimers)
            {
                return; // Could lead to infinite recursion
            }

            GameSystems.TimeEvent.Remove(TimeEventType.NormalHealing, evt =>
            {
                var timerObj = evt.arg1.handle;
                if (timerObj == obj)
                {
                    if (applyQueuedHealing)
                    {
                        _isRemovingHealingTimers = true;
                        // TODO CritterHealNormalDamageOverTime(timerObj, evt.arg2.timePoint);
                        _isRemovingHealingTimers = false;
                    }

                    return true;
                }

                return false;
            });

            var newEvt = new TimeEvent();
            newEvt.system = TimeEventType.NormalHealing;
            newEvt.arg1.handle = obj;
            newEvt.arg2.timePoint = GameSystems.TimeEvent.GameTime;
            GameSystems.TimeEvent.Schedule(newEvt, TimeSpan.FromHours(8), out _);
        }

        [TempleDllLocation(0x1007e480)]
        public void AddFaction(GameObjectBody obj, int factionId)
        {
            if (!obj.IsNPC())
            {
                return;
            }

            var factionCount = 0;
            while (factionCount < 50 && obj.GetInt32(obj_f.npc_faction, factionCount) != 0)
            {
                factionCount++;
            }

            obj.SetInt32(obj_f.npc_faction, factionCount, factionId);
            obj.SetInt32(obj_f.npc_faction, factionCount + 1, 0);

            if (factionCount == 50)
            {
                Logger.Warn("Critter {0} has too many factions, cannot add more.", obj);
            }
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
            // TODO
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

    public class LocationSystem : IGameSystem, IBufferResettingSystem
    {
        public const bool IsEditor = false;

        private static readonly ILogger Logger = new ConsoleLogger();

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10808D00)]
        public int LocationTranslationX { get; set; }

        [TempleDllLocation(0x10808D48)]
        public int LocationTranslationY { get; set; }

        [TempleDllLocation(0x10808D38)]
        public int LocationLimitX { get; set; } = int.MaxValue;

        [TempleDllLocation(0x10808D20)]
        public int LocationLimitY { get; set; } = int.MaxValue;

        [TempleDllLocation(0x10808D28)]
        public long LocationLimitYTimes14 { get; set; }

        private Size _screenSize;

        public delegate void MapCenterCallback(int centerTileX, int centerTileY);

        [TempleDllLocation(0x10808D5C)]
        [TempleDllLocation(0x100299C0)]
        public event MapCenterCallback OnMapCentered;

        [TempleDllLocation(0x1002A1E0)]
        private void ScreenToTile(int screenX, int screenY, out int tileX, out int tileY)
        {
            var a = (screenX - LocationTranslationX) / 2;
            var b = (int) (((screenY - LocationTranslationY) / 2) * 1.4285715f);
            tileX = (b - a) / 20;
            tileY = (b + a) / 20;
        }

        /// <summary>
        /// Given a rectangle in screen coordinates, calculates the rectangle in
        /// tile-space that is visible.
        /// </summary>
        [TempleDllLocation(0x1002A6B0)]
        public bool GetVisibleTileRect(in Rectangle screenRect, out TileRect tiles)
        {
            // TODO: This way of figuring out the visible tiles has to go,
            // TODO since it does not use the camera transforms, but rather
            // TODO hardcoded assumptions about projection.

            var rect = screenRect;
            ScreenToTile(rect.X, rect.Y, out _, out tiles.y1);
            ScreenToTile(rect.X + rect.Width, rect.Y, out tiles.x1, out _);
            ScreenToTile(rect.X, rect.Y + rect.Height, out tiles.x2, out _);
            ScreenToTile(rect.X + rect.Width, rect.Y + rect.Height, out _, out tiles.y2);
            if (tiles.x1 > tiles.x2 || tiles.y1 > tiles.y2)
                return false;

            // NOTE: A lot of this function dealt with the location limits, which were set
            //       to uint.MaxValue, meaning they never applied.

            return tiles.x1 < tiles.x2 && tiles.y1 < tiles.y2;
        }

        [TempleDllLocation(0x10028e10)]
        private void GetTranslation(int tileX, int tileY, out int translationX, out int translationY)
        {
            translationX = LocationTranslationX + (tileY - tileX - 1) * 20;
            translationY = LocationTranslationY + (tileX + tileY) * 14;
        }

        [TempleDllLocation(0x10029810)]
        private void GetTranslationDelta(int x, int y, out int deltaX, out int deltaY)
        {
            var prevX = LocationTranslationX;
            var prevY = LocationTranslationY;
            LocationTranslationX = 0;
            LocationTranslationY = 0;
            GetTranslation(0, 0, out var originTransX, out var originTransY);
            GetTranslation(x, y, out var tileTransX, out var tileTransY);
            deltaX = originTransX + _screenSize.Width / 2 - tileTransX - prevX;
            deltaY = originTransY + _screenSize.Width / 2 - tileTransY - prevY;
            LocationTranslationX = prevX;
            LocationTranslationY = prevY;
        }

        [TempleDllLocation(0x1002A580)]
        public void CenterOn(int tileX, int tileY)
        {
            GetTranslationDelta(tileX, tileY, out var xa, out var ya);
            AddTranslation(xa, ya);
            OnMapCentered?.Invoke(tileX, tileY);
        }

        [TempleDllLocation(0x1002a3e0)]
        public void AddTranslation(int x, int y)
        {
            if (!IsEditor)
            {
                if (x + LocationTranslationX <= _screenSize.Width / 2)
                {
                    if (y + LocationTranslationY + LocationLimitYTimes14 > _screenSize.Height / 2)
                    {
                        if (!ScreenToLoc(_screenSize.Width - x, -y, out _))
                            return;
                        if (!ScreenToLoc(_screenSize.Width - x, _screenSize.Height - y, out _))
                            return;
                    }
                }
                else if (y + LocationTranslationY + LocationLimitYTimes14 > _screenSize.Height / 2)
                {
                    if (!ScreenToLoc(-x, -y, out _))
                        return;
                    if (!ScreenToLoc(-x, _screenSize.Height - y, out _))
                        return;
                }
            }

            LocationTranslationX += x;
            LocationTranslationY += y;
            UpdateProjectionMatrix();
        }

        private struct CameraParams
        {
            public float xOffset;
            public float yOffset;
            public float scale;
        }

        [TempleDllLocation(0x1002a310)]
        private void UpdateProjectionMatrix()
        {
            if (LocationLimitX >= 0)
            {
                var cameraParams = new CameraParams();

                GetTranslation(0, 0, out var translationX, out var translationY);
                cameraParams.xOffset = translationX + 20.0f;
                cameraParams.yOffset = translationY;
                if (GameSystems.Map.GetCurrentMapId() != 5000 || IsEditor)
                {
                    cameraParams.scale = 1.0f;
                    Update3dProjMatrix(cameraParams);
                }
                else
                {
                    cameraParams.scale = _screenSize.Height / 600.0f;
                    Update3dProjMatrix(cameraParams);
                }
            }
        }

        private void Update3dProjMatrix(CameraParams cameraParams)
        {
            var camera = Tig.RenderingDevice.GetCamera();

            camera.SetTranslation(LocationTranslationX, LocationTranslationY);
            camera.SetScale(cameraParams.scale);
        }

        [TempleDllLocation(0x100290c0)]
        private bool ScreenToLoc(int x, int y, out locXY locOut)
        {
            var v4 = (x - LocationTranslationX) / 2;
            var v5 = (int) (((y - LocationTranslationY) / 2) * 1.4285715);
            if ((x - LocationTranslationX) / 2 >= v5)
            {
                var v7 = (v5 - v4) / 20;
                var tileX = (v5 - v4) / 20;
                if ((v5 - v4) < 0)
                    tileX = --v7;
                if (v7 >= 0 && v7 < LocationLimitX)
                {
                    if (v4 + v5 >= 0)
                    {
                        var tileY = (v5 + v4) / 20;
                        if (v5 + v4 < 0)
                            --tileY;
                        if (tileY >= 0 && tileY < LocationLimitY)
                        {
                            locOut = new locXY(tileX, tileY);
                            return true;
                        }
                    }
                }
            }

            locOut = default;
            return false;
        }

        [TempleDllLocation(0x1002a8f0)]
        public bool SetLimits(ulong limitX, ulong limitY)
        {
            Logger.Debug("location_set_limits( {0}, {1} )", limitX, limitY);
            if (limitX > 0x100000000 || limitY > 0x100000000)
            {
                return false;
            }
            else
            {
                LocationTranslationX = 0;
                LocationTranslationY = 0;
                LocationLimitX = (int) Math.Min(int.MaxValue, limitX);
                LocationLimitY = (int) Math.Min(int.MaxValue, limitY);
                LocationLimitYTimes14 = LocationLimitY * 14;
                return true;
            }
        }

        [TempleDllLocation(0x1002a170)]
        public locXY GetLimitsCenter()
        {
            var limitX = Math.Min(LocationLimitX, 640000);
            var limitY = Math.Min(LocationLimitY, 640000);
            return new locXY(limitX / 2, limitY / 2);
        }

        public void ResetBuffers()
        {
            _screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
        }
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
            throw new NotImplementedException();
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

    public class MapObjectSystem : IGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10021930)]
        public void FreeRenderState(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100219B0)]
        public void RemoveMapObj(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1009f550)]
        public bool ValidateSector(bool requireHandles)
        {
            // Check all objects
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                // Primary keys for objects must be persistable ids
                if (!obj.id.IsPersistable())
                {
                    Logger.Error("Found non persistable object id {0}", obj.id);
                    return false;
                }

                if (obj.IsProto())
                {
                    continue;
                }

                if (GameSystems.Object.GetInventoryFields(obj.type, out var idxField, out var countField))
                {
                    ValidateInventory(obj, idxField, countField, requireHandles);
                }
            }

            return true;
        }

        private bool ValidateInventory(GameObjectBody container, obj_f idxField, obj_f countField, bool requireHandles)
        {
            var actualCount = container.GetObjectIdArray(idxField).Count;

            if (actualCount != container.GetInt32(countField))
            {
                Logger.Error("Count stored in {0} doesn't match actual item count of {1}.",
                    countField, idxField);
                return false;
            }

            for (var i = 0; i < actualCount; ++i)
            {
                var itemId = container.GetObjectId(idxField, i);

                var positional = $"Entry {itemId} in {idxField}@{i} of {container.id}";

                if (itemId.IsNull)
                {
                    Logger.Error("{0} is null", positional);
                    return false;
                }
                else if (!itemId.IsHandle)
                {
                    if (requireHandles)
                    {
                        Logger.Error("{0} is not a handle, but handles are required.", positional);
                        return false;
                    }

                    if (!itemId.IsPersistable())
                    {
                        Logger.Error("{0} is not a valid persistable id.", positional);
                        return false;
                    }
                }

                var itemObj = GameSystems.Object.GetObject(itemId);

                if (itemObj == null)
                {
                    Logger.Error("{0} does not resolve to a loaded object.", positional);
                    return false;
                }

                if (itemObj == container)
                {
                    Logger.Error("{0} is contained inside of itself.", positional);
                    return false;
                }

                // Only items are allowed in containers
                if (!itemObj.IsItem())
                {
                    Logger.Error("{0} is not an item.", positional);
                    return false;
                }
            }

            return true;
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

        [TempleDllLocation(0x1002DC70)]
        public void Render()
        {
            // TODO
        }

        [TempleDllLocation(0x1002d290)]
        public void UpdateDayNight()
        {
            // TODO
        }

        public void Load(int groundId)
        {
            // TODO
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
            throw new NotImplementedException();
        }

        public void StopAll(bool b)
        {
            throw new NotImplementedException();
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

        [TempleDllLocation(0x100628d0)]
        public bool IsCombatActive()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100634e0)]
        public void AdvanceTurn(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100df530)]
        public void RemoveFromInitiative(GameObjectBody obj)
        {
            throw new NotImplementedException();
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

    public class AISystem : IGameSystem, IModuleAwareSystem
    {
        public void Dispose()
        {
        }

        public void LoadModule()
        {
            // TODO AI
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        public void AddAiTimer(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005b5f0)]
        public void FollowerAddWithTimeEvent(GameObjectBody obj, bool forceFollower)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100588d0)]
        public void RemoveAiTimer(GameObjectBody obj)
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

        public bool ProcessAnimEvent(TimeEvent evt)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000c760)]
        public void ClearForObject(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000c890)]
        public void InterruptAll()
        {
            throw new NotImplementedException();
        }

        public void ClearGoalDestinations()
        {
            // TODO
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

        [TempleDllLocation(0x10AA36D0)]
        private GameObjectBody _reactionNpcObject;

        [TempleDllLocation(0x10AA36A8)]
        private GameObjectBody _reactionPlayerObject;

        [TempleDllLocation(0x10053ca0)]
        public GameObjectBody GetLastReactionPlayer(GameObjectBody npc)
        {
            if (_reactionNpcObject == npc)
            {
                return _reactionPlayerObject;
            }

            return null;
        }
    }

    public class TileScriptSystem : IGameSystem
    {
        private const bool IsEditor = false;

        public void Dispose()
        {
        }

        private struct TileScript
        {
            public locXY Location;
            public ObjectScript Script;
        }

        [TempleDllLocation(0x10053b20)]
        public bool TriggerTileScript(locXY tileLoc, GameObjectBody obj)
        {
            if (GetTileScript(tileLoc, out var tileScript))
            {
                var invocation = new ObjScriptInvocation();
                invocation.script = tileScript.Script;
                invocation.triggerer = obj;
                invocation.eventId = ObjScriptEvent.Use;
                GameSystems.Script.Invoke(ref invocation);

                if (invocation.script != tileScript.Script)
                {
                    SetTileScript(in tileScript);
                }

                return true;
            }

            return false;
        }

        private bool GetTileScript(locXY tileLoc, out TileScript tileScript)
        {
            using var lockedSector = new LockedMapSector(new SectorLoc(tileLoc));
            var sector = lockedSector.Sector;
            var tileIndex = sector.GetTileOffset(tileLoc);

            foreach (var scriptInSector in sector.tileScripts)
            {
                if (scriptInSector.tileIndex == tileIndex)
                {
                    tileScript.Location = tileLoc;
                    tileScript.Script.unk1 = scriptInSector.scriptUnk1;
                    tileScript.Script.counters = scriptInSector.scriptCounters;
                    tileScript.Script.scriptId = scriptInSector.scriptId;
                    return true;
                }
                else if (scriptInSector.tileIndex > tileIndex)
                {
                    break; // Tiles are sorted in ascending order
                }
            }

            tileScript = default;
            return false;
        }

        private void SetTileScript(in TileScript tileScript)
        {
            using var lockedSector = new LockedMapSector(new SectorLoc(tileScript.Location));
            var sector = lockedSector.Sector;
            var tileIndex = sector.GetTileOffset(tileScript.Location);

            if (!IsEditor || tileScript.Script.scriptId != 0)
                AddOrUpdateSectorTilescript(sector, tileIndex, tileScript.Script);
            else
                RemoveSectorTilescript(sector, tileIndex);
        }

        [TempleDllLocation(0x10105400)]
        public void AddOrUpdateSectorTilescript(Sector sector, int tileIndex, ObjectScript script)
        {
            // If a tile-script exists for the tile, update it accordingly
            for (var i = 0; i < sector.tileScripts.Length; i++)
            {
                ref var tileScript = ref sector.tileScripts[i];
                if (tileScript.tileIndex == tileIndex)
                {
                    tileScript.field00 |= 1; // Dirty flag most likely
                    tileScript.scriptUnk1 = script.unk1;
                    tileScript.scriptCounters = script.counters;
                    tileScript.scriptId = script.scriptId;
                    sector.tileScriptsDirty = true;
                    return;
                }

                if (tileScript.tileIndex > tileIndex)
                {
                    break; // Entries are sorted in ascending order
                }
            }

            Array.Resize(ref sector.tileScripts, sector.tileScripts.Length + 1);
            sector.tileScripts[^1] = new SectorTileScript
            {
                field00 = 1,
                tileIndex = tileIndex,
                scriptUnk1 = script.unk1,
                scriptCounters = script.counters,
                scriptId = script.scriptId
            };
            // Ensure it is still sorted in ascending order
            Array.Sort(sector.tileScripts, SectorTileScript.TileIndexComparer);
            sector.tileScriptsDirty = true;
        }

        [TempleDllLocation(0x101054b0)]
        private void RemoveSectorTilescript(Sector sector, int tileIndex)
        {
            // Determine how many we need to remove. In normal conditions this should be 0 or 1.
            int removeCount = sector.tileScripts.Count(i => i.tileIndex == tileIndex);
            if (removeCount == 0)
            {
                return;
            }

            // Create a new array without the tile, this will maintain the sort order as well
            var outIdx = 0;
            var newScripts = new SectorTileScript[sector.tileScripts.Length - removeCount];
            foreach (var tileScript in sector.tileScripts)
            {
                if (tileScript.tileIndex != tileIndex)
                {
                    newScripts[outIdx++] = tileScript;
                }
            }

            sector.tileScripts = newScripts;
            sector.tileScriptsDirty = true;
        }

        public void TriggerSectorScript(SectorLoc loc, GameObjectBody obj)
        {
            if (GetSectorScript(loc, out var script))
            {
                // Save for change detection
                var invocation = new ObjScriptInvocation();
                invocation.script = script;
                invocation.eventId = ObjScriptEvent.Use;
                invocation.triggerer = obj;
                GameSystems.Script.Invoke(ref invocation);

                if (invocation.script != script)
                {
                    SetSectorScript(loc, in invocation.script);
                }
            }
        }

        [TempleDllLocation(0x100538e0)]
        public bool GetSectorScript(SectorLoc sectorLoc, out ObjectScript script)
        {
            using var lockedSector = new LockedMapSector(sectorLoc);
            var sectorScript = lockedSector.Sector.sectorScript;
            script.unk1 = sectorScript.data1;
            script.counters = sectorScript.data2;
            script.scriptId = sectorScript.data3;
            return script.scriptId != 0;
        }

        [TempleDllLocation(0x10053930)]
        public void SetSectorScript(SectorLoc sectorLoc, in ObjectScript script)
        {
            using var lockedSector = new LockedMapSector(sectorLoc);
            ref var sectorScript = ref lockedSector.Sector.sectorScript;
            sectorScript.data1 = script.unk1;
            sectorScript.data2 = script.counters;
            sectorScript.data3 = script.scriptId;

            // Dirty flag???
            sectorScript.field0 |= 1;
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
            // TODO movies
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
        [TempleDllLocation(0x102B0868)]
        private static readonly string[] DeityNames =
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

        public void Dispose()
        {
        }

        [TempleDllLocation(0x1004a820)]
        public static bool GetDeityFromEnglishName(string name, out DeityId deityId)
        {
            for (int i = 0; i < DeityNames.Length; i++)
            {
                if (DeityNames[i].Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    deityId = (DeityId) i;
                    return true;
                }
            }

            deityId = default;
            return false;
        }

        public static bool IsDomainSkill(GameObjectBody obj, SkillId skillId)
        {
            throw new NotImplementedException();
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

        [TempleDllLocation(0x101e7e00)]
        public void InvalidateObject(GameObjectBody obj)
        {
            //for (var &sys : particles) {
            //    if (sys.second->GetAttachedTo() == obj.handle) {
            //      sys.second->SetAttachedTo(0);
            //      sys.second->EndPrematurely();
            //  }
            //}
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10049be0)]
        public void Remove(int partSysHandle)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10049bd0)]
        public int CreateAt(in int hashCode, Vector3 centerOfTile)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101e78a0)]
        public void RemoveAll()
        {
            // TODO
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
            throw new NotImplementedException();
        }

        public void SaveEsd()
        {
            throw new NotImplementedException();
        }

        public void SaveExploredTileData(int id)
        {
            throw new NotImplementedException();
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
        public void NotifyMoved(GameObjectBody obj, LocAndOffsets fromLoc, LocAndOffsets toLoc)
        {
            throw new NotImplementedException();
            /*
             *
            static var objevent_notify_moved =
            temple.GetPointer <void (ObjHndl, LocAndOffsets, LocAndOffsets) > (0x10045290);
            objevent_notify_moved(handle, fromLoc, toLoc);
             */
        }

        public void FlushEvents()
        {
            throw new NotImplementedException();
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