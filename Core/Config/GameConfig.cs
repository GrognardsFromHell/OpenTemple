using System;
using System.Collections.Generic;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.Config;

public enum HpOnLevelUpMode
{
    Max,
    Average,
    Roll
}

public class GameConfig
{

    /// <summary>
    /// The folder where Tempe of Elemental Evil is installed.
    /// </summary>
    public string InstallationFolder { get; set; }

    public RenderingConfig Rendering { get; set; } = new();

    public WindowConfig Window { get; set; } = new();

    public bool EnableDebugUI { get; set; } = true;

    public bool SkipLegal { get; set; } = true;
    public bool laxRules { get; set; }
    public bool allowXpOverflow { get; set; } = false;
    public bool disableMulticlassXpPenalty { get; set; } = false;
        

    public bool disableAlignmentRestrictions { get; set; }

    public bool nonCoreMaterials { get; set; }

    public bool drawObjCylinders { get; set; }

    // 0 = Blob
    // 1 = Geometry
    // 2 = Shadow Map
    public int ShadowType { get; set; } = 2;

    // When ShadowType = 2, Blur
    public bool SoftShadows { get; set; } = true;

    public string fogOfWar { get; set; } = "";

    /// <summary>
    /// Used to speed up all animations.
    /// </summary>
    public float AnimSpeedFactor { get; set; } = 1;

    /// <summary>
    /// This determines for how many party members we update the line of sight per frame.
    /// If a party member doesn't move, they will not be updated regardless of this number.
    /// Keeping this at a lower number will reduce stutter when the entire party is moving.
    /// </summary>
    [TempleDllLocation(0x102ACEFC)]
    public int LineOfSightChecksPerFrame { get; set; } = 1;

    public HpOnLevelUpMode HpOnLevelUpMode { get; set; }

    public bool NPCsLevelLikePCs { get; set; } = true; // Gives the player manual conttrol of NPC levelling

    public bool slowerLevelling { get; set; } = false;

    public bool MaxHpForNpcHitdice { get; set; }

    public bool monstrousRaces { get; set; }

    public bool forgottenRealmsRaces { get; set; }

    public bool newRaces { get; set; }

    public int ScrollSpeed { get; set; } = 3;

    public bool ScrollAcceleration { get; set; } = true;

    public int MaxPCs { set; get; } = 6;

    public bool MaxPCsFlexible { get; set; } = false;

    public bool ShowNpcStats { get; set; }

    public int MaxLevel { get; set; } = 20;

    public bool animCatchup { get; set; }

    public bool AutoAttack { get; set; }

    [TempleDllLocation(0x103074cc)]
    public bool ReduceViolence { get; set; }

    public bool AlwaysRun { get; set; } = true;

    public bool pathfindingDebugMode { get; set; }

    public TimeSpan AStarMaxWindow { get; set; } = TimeSpan.FromSeconds(5);

    public TimeSpan AStarMaxTime { get; set; } = TimeSpan.FromSeconds(4);

    public int ParticleFidelity { get; set; } = 100;

    public bool DebugPartSys { get; set; }

    public bool ConcurrentTurnsEnabled { get; set; } = true;

    public int EndTurnTime { get; set; } = 1;

    public bool EndTurnDefault { get; set; } = true;

    public bool alertAiThroughDoors { get; set; }

    /// <summary>
    /// Maximum time in seconds that AI turns can last. If they exceed it, their turn is ended.
    /// Also known as "greybar reset", due to the grey action bar while the AI takes its turn.
    /// </summary>
    public int AITurnTimeout { get; set; } = 20;

    /// <summary>
    /// Maximum number of actions AI can take during a turn. This is useful to avoid loops where
    /// AI tries to perform an action that runs into the AITurnTimeout, but doesn't use up all
    /// movement, only to repeat the same idea indefinitely. This will stop it after 5 attempts.
    /// </summary>
    public int AITurnMaxActions { get; set; } = 5;

    public TextFloaterCategory ActiveTextFloaters { get; set; } =
        TextFloaterCategory.Damage | TextFloaterCategory.Generic;

    public int TextFloatSpeed { get; set; } = 2;

    [TempleDllLocation(0x10be6d50)]
    public bool RadialMenuClickToActivate { get; set; } = false;

    public bool ShowTargetingCirclesInFogOfWar { get; set; }

    public int TextDuration { get; set; } = 6;

    // All range from 0-100
    public int MasterVolume { get; set; } = 100;
    public int EffectsVolume { get; set; } = 50;
    public int VoiceVolume { get; set; } = 50;
    public int MusicVolume { get; set; } = 50;
    public int ThreeDVolume { get; set; } = 50;

    /// <summary>
    /// This was previously activated via a command line option -dialognumber
    /// </summary>
    [TempleDllLocation(0x108ed0d0)]
    public bool ShowDialogLineIds { get; set; }

    // Templeplus enhancement
    public bool TolerateMonsterPartyMembers { get; set; }

    public bool Co8 { get; set; }

    public string ScriptAssemblyName { get; set; } = "Scripts";

    public bool PartyPoolHidePreGeneratedChars { get; set; }

    public bool PartyPoolHideIncompatibleChars { get; set; }

    /// <summary>
    /// Skips the intro cinematic if set.
    /// </summary>
    public bool SkipIntro { get; set; }

    /// <summary>
    /// Draw numeric party hit points in the party bar.
    /// </summary>
    [TempleDllLocation(0x1002b900)]
    [TempleDllLocation(0x1080AB70)]
    [TempleDllLocation(0x1002b910)]
    public bool ShowPartyHitPoints { get; set; }

    public bool AutoSaveBetweenMaps { get; set; } = true;

    public int StartupTip { get; set; } = 0;

    public List<SeenMovie> SeenMovies { get; set; } = new()
    {
        new SeenMovie(304)
    };


    [TempleDllLocation(0x1080AB9C)]
    [TempleDllLocation(0x1002b8e0)]
    [TempleDllLocation(0x1002b8f0)]
    public bool PartyTextConfirmations { get; set; }

    [TempleDllLocation(0x1080ABA0)]
    [TempleDllLocation(0x1002b8d0)]
    [TempleDllLocation(0x1002b8c0)]
    public bool PartyVoiceConfirmations { get; set; } = true;

    // Amount of point buy points available
    public int PointBuyBudget { get; set; } = 25;

    public string DefaultUiFont { get; set; } = "Arial";

}