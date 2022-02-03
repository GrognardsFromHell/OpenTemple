using System;

namespace OpenTemple.Core.Systems.Pathfinding;

[Flags]
public enum PathQueryFlags
{
    /*
        The pathfinder seems to ignore offset x and y of the destination if this flag
        is not set.
    */
    PQF_TO_EXACT = 1,

    /*
        Indicates that the query is on behalf of a critter and the critter is set.
    */
    PQF_HAS_CRITTER = 2,

    /*
        if not set, then sets maxShortPathFindLength to 200 initially
    */
    PQF_MAX_PF_LENGTH_STHG = 4,


    PQF_STRAIGHT_LINE = 8, // appears to indicate a straight line path from->to

    // Either one might mean "ignore blocked target loc???"
    PQF_10 = 0x10,
    PQF_20 = 0x20,
    PQF_40 = 0x40,
    PQF_IGNORE_CRITTERS = 0x80, // path (pass) through critters (flag is set when pathing out of combat)
    PQF_100 = 0x100,
    PQF_STRAIGHT_LINE_ONLY_FOR_SANS_NODE = 0x200,
    PQF_DOORS_ARE_BLOCKING = 0x400, // if set, it will consider doors to block the path
    PQF_800 = 0x800,
    PQF_TARGET_OBJ = 0x1000, // Indicates that the query is to move to a target object.

    /*
    Indicates that the destination should be adjusted for the critter and target
    radius.
    makes PathInit add the radii of the targets to fields tolRadius and distanceToTargetMin
    */
    PQF_ADJUST_RADIUS = 0x2000,

    PQF_DONT_USE_PATHNODES = 0x4000,
    PQF_DONT_USE_STRAIGHT_LINE = 0x8000,
    PQF_FORCED_STRAIGHT_LINE = 0x10000,
    PQF_ADJ_RADIUS_REQUIRE_LOS = 0x20000,

    /*
        if the target destination is not cleared, and PQF_ADJUST_RADIUS is off,
        it will search in a 5x5 tile neighbourgood around the original target tile
        for a clear tile (i.e. one that the critter can fit in without colliding with anything)
    */
    PQF_ALLOW_ALTERNATIVE_TARGET_TILE = 0x40000,

    // Appears to mean that pathfinding should obey the time limit
    PQF_A_STAR_TIME_CAPPED = 0x80000, // it is set when the D20 action has the flag D20CAF_UNNECESSARY

    PQF_IGNORE_CRITTERS_ON_DESTINATION = 0x800000, // NEW! makes it ignored critters on the PathDestIsClear function

    PQF_AVOID_AOOS =
        0x1000000 // NEW! Make the PF attempt avoid Aoos (using the ShouldIgnore function in combat.py to ignore insiginificant threats)
}