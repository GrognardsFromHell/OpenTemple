using System;

namespace OpenTemple.Core.Systems.Raycast
{
    [Flags]
    public enum RaycastFlag : uint
    {
        HasToBeCleared = 1,
        HasRadius = 2,
        HasSourceObj = 4,
        ExcludeItemObjects = 8, // excludes obj_t_weapon through obj_t_gneric and obj_t_bag
        StopAfterFirstBlockerFound = 0x10, // probably a performance optimization; also probably a source of bugs!
        StopAfterFirstFlyoverFound = 0x20,
        RequireDistToSourceLessThanTargetDist = 0x40, // I think this flag has another more general role, not quite sure yet
        HasRangeLimit = 0x80,
        HasTargetObj = 0x100,
        GetObjIntersection =0x200, // will return the first point along the ray where it intersects with the found object/tile
        IgnoreFlyover = 0x400, // probably used for LOS or archery queries
        ExcludePortals = 0x800, // doors etc.
        FoundCoverProvider = 0x80000000, // subtile marked as Blocker OR (FlyOver AND Cover)
    }
}