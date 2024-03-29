using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Particles;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Particles.Parser;
using OpenTemple.Core.Particles.Spec;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Particles;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace OpenTemple.Core.Systems;

public class ParticleSysSystem : IGameSystem, ITimeAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly Dictionary<int, PartSysSpec> _specsByNameHash = new();
    private readonly Dictionary<string, PartSysSpec> _specsByName = new();

    private TimePoint _lastSimTime;
    private int _nextId = 1;
    private readonly Dictionary<int, PartSys> _activeSys = new();
    private readonly PartSysExternal _external;

    public float Fidelity => Math.Clamp(Globals.Config.ParticleFidelity / 100.0f, 0, 1);

    public ParticleSysSystem()
    {
        var parser = new PartSysParser();
        parser.ParseFile("rules/partsys0.tab");
        parser.ParseFile("rules/partsys1.tab");
        parser.ParseFile("rules/partsys2.tab");

        foreach (var path in Tig.FS.Search("rules/partsys/*.tab"))
        {
            parser.ParseFile(path);
        }

        foreach (var kvp in parser.Specs)
        {
            _specsByName[kvp.Key] = kvp.Value;
            _specsByNameHash[kvp.Value.GetNameHash()] = kvp.Value;
        }

        _external = new PartSysExternal(this);
    }

    public void Dispose()
    {
    }

    public IEnumerable<PartSys> ActiveSystems => _activeSys.Values;

    [TempleDllLocation(0x101e7e00)]
    public void InvalidateObject(GameObject obj)
    {
        foreach (var sys in _activeSys.Values)
        {
            if (ReferenceEquals(sys.GetAttachedTo(), obj))
            {
                sys.SetAttachedTo(null);
                sys.EndPrematurely();
            }
        }
    }

    public bool DoesNameExist(string name)
    {
        return _specsByName.ContainsKey(name.ToLowerInvariant());
    }

    public bool DoesNameHashExist(int nameHash)
    {
        return _specsByNameHash.ContainsKey(nameHash);
    }

    [TempleDllLocation(0x10049be0)]
    public void Remove(object partSysHandle)
    {
        _activeSys.Remove((int) partSysHandle);
    }

    [TempleDllLocation(0x101e6e30)]
    public int GetNameHash(object partSysHandle)
    {
        return _activeSys[(int) partSysHandle].GetSpec().GetNameHash();
    }

    [TempleDllLocation(0x10049bd0)]
    public object CreateAt(int nameHash, Vector3 pos)
    {
        if (!_specsByNameHash.TryGetValue(nameHash, out var spec))
        {
            Logger.Warn("Unable to spawn unknown particle system: {0}", nameHash);
            return -1;
        }

        var sys = new PartSys(_external, spec);
        sys.SetWorldPos(pos.X, pos.Y, pos.Z);

        var assignedId = _nextId++;

        _activeSys[assignedId] = sys;
        return assignedId;
    }

    [TempleDllLocation(0x10049bd0)]
    public object CreateAt(string name, Vector3 pos)
    {
        if (!_specsByName.TryGetValue(name.ToLowerInvariant(), out var spec))
        {
            Logger.Warn("Unable to spawn unknown particle system: {0}", name);
            return -1;
        }

        var sys = new PartSys(_external, spec);
        sys.SetWorldPos(pos.X, pos.Y, pos.Z);

        var assignedId = _nextId++;

        _activeSys[assignedId] = sys;
        return assignedId;
    }

    [TempleDllLocation(0x10049b70)]
    public object CreateAtObj(string name, GameObject obj)
    {
        if (!_specsByName.TryGetValue(name.ToLowerInvariant(), out var spec))
        {
            Logger.Warn("Unable to spawn unknown particle system: {0}", name);
            return -1;
        }

        var loc = obj.GetLocationFull();
        var absLoc = loc.ToInches3D(obj.OffsetZ);

        var sys = new PartSys(_external, spec);
        sys.SetWorldPos(absLoc.X, absLoc.Y, absLoc.Z);
        sys.SetAttachedTo(obj);

        var assignedId = _nextId++;

        _activeSys[assignedId] = sys;
        return assignedId;
    }

    [TempleDllLocation(0x10049b70)]
    public object CreateAtObj(int nameHash, GameObject obj)
    {
        if (!_specsByNameHash.TryGetValue(nameHash, out var spec))
        {
            Logger.Warn("Unable to spawn unknown particle system: {0}", nameHash);
            return -1;
        }

        var loc = obj.GetLocationFull();
        var absLoc = loc.ToInches3D(obj.OffsetZ);

        var sys = new PartSys(_external, spec);
        sys.SetWorldPos(absLoc.X, absLoc.Y, absLoc.Z);
        sys.SetAttachedTo(obj);

        var assignedId = _nextId++;

        _activeSys[assignedId] = sys;
        return assignedId;
    }

    /// <summary>
    /// Removes all active particle systems i.e. for changing the map.
    /// </summary>
    [TempleDllLocation(0x101e78a0)]
    public void RemoveAll()
    {
        _activeSys.Clear();
    }

    /// <summary>
    /// The given particle system will stop emitting new particles, but
    /// existing particles will continue to be simulated until their lifespan ends.
    /// </summary>
    [TempleDllLocation(0x10049bf0)]
    public void End(object partSysHandle)
    {
        if (_activeSys.TryGetValue((int) partSysHandle, out var partSys))
        {
            partSys.EndPrematurely();
        }
    }

    private static readonly TimeSpan MaxSimTime = TimeSpan.FromSeconds(0.5);

    public void AdvanceTime(TimePoint time)
    {
        // First call
        if (_lastSimTime == default)
        {
            _lastSimTime = time;
            return;
        }

        var sinceLastSim = time - _lastSimTime;
        _lastSimTime = time;

        if (sinceLastSim > MaxSimTime)
        {
            sinceLastSim = MaxSimTime;
        }

        var timeInSecs = (float) sinceLastSim.TotalSeconds * Globals.Config.AnimSpeedFactor;

        var deadSystems = new List<int>();

        foreach (var kvp in _activeSys)
        {
            var sys = kvp.Value;
            sys.Simulate(timeInSecs);

            // Remove dead systems
            if (sys.IsDead())
            {
                deadSystems.Add(kvp.Key);
            }
        }

        foreach (var deadSystem in deadSystems)
        {
            _activeSys.Remove(deadSystem);
        }
    }

    /// <summary>
    /// This method exists primarily for debugging purposes.
    /// </summary>
    public IEnumerable<PartSys> GetAttachedTo(GameObject obj)
    {
        return _activeSys.Values.Where(sys => ReferenceEquals(sys.GetAttachedTo(), obj));
    }
}

internal class PartSysExternal : IPartSysExternal
{
    private readonly ParticleSysSystem _system;

    public PartSysExternal(ParticleSysSystem system)
    {
        _system = system;
    }

    public float GetParticleFidelity()
    {
        return _system.Fidelity;
    }

    public bool GetObjLocation(object obj, out Vector3 worldPos)
    {
        var gameObj = (GameObject) obj;

        var locWithOffsets = gameObj.GetLocationFull();
        var center3d = locWithOffsets.ToInches3D(gameObj.OffsetZ);
        worldPos.X = center3d.X;
        worldPos.Y = center3d.Y;
        worldPos.Z = center3d.Z;
        return true;
    }

    public bool GetObjRotation(object obj, out float rotation)
    {
        var gameObj = (GameObject) obj;
        rotation = gameObj.Rotation + MathF.PI / 4;
        return true;
    }

    public float GetObjRadius(object obj)
    {
        var gameObj = (GameObject) obj;
        return gameObj.GetRadius();
    }

    public bool GetBoneWorldMatrix(object obj, string boneName, out Matrix4x4 boneMatrix)
    {
        var gameObj = (GameObject) obj;
        var model = gameObj.GetOrCreateAnimHandle();
        if (model == null)
        {
            boneMatrix = Matrix4x4.Identity;
            return false;
        }

        var animParams = gameObj.GetAnimParams();

        if (gameObj.type.IsEquipment())
        {
            var parent = GameSystems.Item.GetParent(gameObj);
            if (parent != null)
            {
                var parentModel = parent.GetOrCreateAnimHandle();
                return parentModel.GetBoneWorldMatrixByNameForChild(
                    model, animParams, boneName, out boneMatrix
                );
            }
        }

        return model.GetBoneWorldMatrixByName(animParams, boneName, out boneMatrix);
    }

    public int GetBoneCount(object obj)
    {
        var gameObj = (GameObject) obj;
        var model = gameObj.GetOrCreateAnimHandle();
        if (model != null)
        {
            return model.GetBoneCount();
        }
        else
        {
            return 0;
        }
    }

    private static bool IsIgnoredBone(string name)
    {
        if (name[0] == '#')
        {
            return true; // Cloth bone
        }

        if (name.ToLowerInvariant() == "bip01")
        {
            return true;
        }

        return name.Contains("Pony")
               || name.Contains("Footstep")
               || name.Contains("Origin")
               || name.Contains("Casting_ref")
               || name.Contains("EarthElemental_reg")
               || name.Contains("Casting_ref")
               || name.Contains("origin")
               || name.Contains("Bip01 Footsteps")
               || name.Contains("FootL_ref")
               || name.Contains("FootR_ref")
               || name.Contains("Head_ref")
               || name.Contains("HandL_ref")
               || name.Contains("HandR_ref")
               || name.Contains("Chest_ref")
               || name.Contains("groundParticleRef")
               || name.Contains("effects_ref")
               || name.Contains("trap_ref");
    }

    public int GetParentChildBonePos(object obj, int boneIdx, out Vector3 parentPos, out Vector3 childPos)
    {
        var gameObj = (GameObject) obj;
        var model = gameObj.GetOrCreateAnimHandle();
        var aasParams = gameObj.GetAnimParams();

        parentPos = default;
        childPos = default;

        var parentId = model.GetBoneParentId(boneIdx);
        if (parentId < 0)
        {
            return parentId;
        }

        var boneName = model.GetBoneName(boneIdx);
        if (boneName.Length == 0)
        {
            return -1;
        }

        if (IsIgnoredBone(boneName))
        {
            return -1;
        }

        var parentName = model.GetBoneName(parentId);
        if (parentName.Length == 0)
        {
            return -1;
        }

        if (!model.GetBoneWorldMatrixByName(aasParams, parentName, out var worldMatrix))
        {
            return -1;
        }

        parentPos = worldMatrix.Translation;

        if (!model.GetBoneWorldMatrixByName(aasParams, boneName, out worldMatrix))
        {
            return -1;
        }

        childPos = worldMatrix.Translation;
        return parentId;
    }

    public bool GetBonePos(object obj, int boneIdx, out Vector3 pos)
    {
        var gameObj = (GameObject) obj;
        var model = gameObj.GetOrCreateAnimHandle();
        var aasParams = gameObj.GetAnimParams();

        pos = default;

        var boneName = model.GetBoneName(boneIdx);
        if (boneName.Length == 0)
        {
            return false;
        }

        if (!model.GetBoneWorldMatrixByName(aasParams, boneName, out var worldMatrix))
        {
            return false;
        }

        pos = worldMatrix.Translation;
        return true;
    }

    public void WorldToScreen(IGameViewport viewport, Vector3 worldPos, out Vector2 screenPos)
    {
        screenPos = viewport.Camera.WorldToScreen(worldPos);
        var offset2d = viewport.Camera.Get2dTranslation();
        screenPos.X += offset2d.X;
        screenPos.Y += offset2d.Y;
    }

    public bool IsBoxVisible(IGameViewport viewport, Vector3 worldPos, Box2d box)
    {
        WorldToScreen(viewport, worldPos, out var screenPos);

        if (viewport.Camera.IsBoxOnScreen(screenPos,
                box.left, box.top,
                box.right, box.bottom))
        {
            return true;
        }

        return false;
    }
}