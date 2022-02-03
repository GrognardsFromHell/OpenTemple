using System;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems;

public static class GameObjectRenderExtensions
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public static float GetRadius(this GameObject obj)
    {
        var radiusSet = obj.GetFlags().HasFlag(ObjectFlag.RADIUS_SET);
        float protoRadius;
        if (radiusSet)
        {
            var currentRadius = obj.GetFloat(obj_f.radius);

            if (currentRadius < 2000.0 && currentRadius > 0)
            {
                var protoHandle = obj.GetProtoObj();
                if (protoHandle != null)
                {
                    protoRadius = protoHandle.GetFloat(obj_f.radius);
                    if (protoRadius > 0.0)
                    {
                        currentRadius = protoRadius;
                        obj.SetFloat(obj_f.radius, protoRadius);
                    }
                }
            }

            if (currentRadius < 2000 && currentRadius > 0)
            {
                return currentRadius;
            }

            Logger.Info("Found invalid radius {0} on object, resetting.", currentRadius);
            obj.SetFlag(ObjectFlag.RADIUS_SET, false);
        }

        Logger.Debug("GetRadius: Radius not yet set, now calculating.");

        var model = obj.GetOrCreateAnimHandle();
        if (model == null)
        {
            Logger.Warn("GetRadius: Null AAS handle!");
            var protoHandle = obj.GetProtoObj();
            protoRadius = protoHandle?.GetFloat(obj_f.radius) ?? 0;
            if (protoRadius > 0.0)
            {
                Logger.Debug("Returning radius from Proto: {0}", protoRadius);
                return protoRadius;
            }

            Logger.Debug("Returning default (10.0)");
            return 10.0f;
        }

        var radius = obj.GetFloat(obj_f.radius);
        radiusSet = obj.GetFlags().HasFlag(ObjectFlag.RADIUS_SET); // might be changed I guess
        if (!radiusSet || MathF.Abs(radius) > 2000)
        {
            Logger.Debug("GetRadius: Calculating from AAS model. Initially was {0}", radius);

            UpdateRadius(obj, model);

            radius = obj.GetFloat(obj_f.radius);

            if (radius > 2000.0)
            {
                Logger.Warn("GetRadius: Huge radius calculated from AAS {0}", radius);
                radius = 2000.0f;
            }
            else if (radius <= 0)
            {
                Logger.Warn("GetRadius: Negative radius calculated from AAS: {0}. Changing to default (10.0)",
                    radius);
                radius = 10.0f;
            }

            obj.SetFloat(obj_f.radius, radius);
            obj.SetFlag(ObjectFlag.RADIUS_SET, true);
        }

        return radius;
    }

    public static float GetRenderHeight(this GameObject obj, bool updateIfNeeded = false)
    {
        var renderHeight = obj.GetFloat(obj_f.render_height_3d);

        // Take render height from the animation if necessary
        if (renderHeight < 0 && updateIfNeeded)
        {
            UpdateRenderHeight(obj, GetOrCreateAnimHandle(obj));
            renderHeight = obj.GetRenderHeight();
        }

        return renderHeight;
    }

    [TempleDllLocation(0x10021a40)]
    public static IAnimatedModel GetOrCreateAnimHandle(this GameObject obj)
    {
        // I think this belongs to the map_obj subsystem
        if (obj == null)
        {
            return null;
        }

        // An animation handle was already created
        var animHandle = new AasHandle(obj.GetUInt32(obj_f.animation_handle));
        if (animHandle)
        {
            return GameSystems.AAS.ModelFactory.BorrowByHandle(animHandle.Handle);
        }

        // Which object to retrieve the model properties from (this indirection
        // is used for polymorph)
        var modelSrcObj = obj;

        // If the obj is polymorphed, use the polymorph proto instead
        int polyProto = GameSystems.D20.D20QueryInt(obj, D20DispatcherKey.QUE_Polymorphed);
        if (polyProto != 0)
        {
            modelSrcObj = GameSystems.Proto.GetProtoById((ushort) polyProto);
        }

        var meshId = modelSrcObj.GetInt32(obj_f.base_mesh);
        var skeletonId = modelSrcObj.GetInt32(obj_f.base_anim);
        var idleAnimId = obj.GetIdleAnimId();
        var animParams = obj.GetAnimParams();

        // Create the model from the meshes.mes IDs and store the result in the obj
        var model = GameSystems.AAS.ModelFactory.FromIds(
            meshId,
            skeletonId,
            idleAnimId,
            animParams,
            true // Borrowed since we store the handle in the obj
        );
        model.OnAnimEvent += evt => HandleAnimEvent(obj, evt);
        obj.SetUInt32(obj_f.animation_handle, model.GetHandle());

        if (obj.IsCritter())
        {
            GameSystems.Critter.UpdateModelEquipment(obj);
        }

        if (obj.type == ObjectType.npc)
        {
            GameSystems.Critter.AddNpcAddMeshes(obj);
        }

        var flags = obj.GetFlags();
        if (!flags.HasFlag(ObjectFlag.HEIGHT_SET))
        {
            UpdateRenderHeight(obj, model);
        }

        if (!flags.HasFlag(ObjectFlag.RADIUS_SET))
        {
            UpdateRadius(obj, model);
        }

        return model;
    }

    private static void HandleAnimEvent(GameObject obj, AasEvent evt)
    {
        if (evt is AasCustomEvent customEvent)
        {
            // This is currently used only for balor_death. We should introduce a new SAN for this and use the script classes.
            throw new NotSupportedException();
        }
        else if (evt is AasFadeEvent fadeEvent)
        {
            obj.FadeTo(fadeEvent.TargetOpacity, fadeEvent.TickTimeMs, fadeEvent.ChangePerTick, fadeEvent.Action);
        }
        else if (evt is AasFootstepEvent)
        {
            obj.Footstep();
        }
        else if (evt is AasParticlesEvent particlesEvent)
        {
            GameSystems.ParticleSys.CreateAtObj(particlesEvent.ParticlesId, obj);
            // TODO: Warn if part sys is permanent, because we don't actually track it!
        }
        else if (evt is AasShakeScreenEvent shakeScreenEvent)
        {
            GameSystems.Scroll.ShakeScreen(
                shakeScreenEvent.PeakAmplitude,
                (float) shakeScreenEvent.Duration.TotalMilliseconds
            );
        }
        else if (evt is AasSoundEvent soundEvent)
        {
            GameSystems.SoundGame.PositionalSound(soundEvent.SoundId, obj);
        }
        else
        {
            Logger.Warn("Unknown animation event type: {0}", evt);
        }
    }

    [TempleDllLocation(0x10264510)]
    public static void FreeAnimHandle(this GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        // An animation handle was already created
        var animHandle = obj.GetUInt32(obj_f.animation_handle);
        if (animHandle != 0)
        {
            GameSystems.AAS.ModelFactory.FreeHandle(animHandle);
            obj.SetUInt32(obj_f.animation_handle, 0);
        }
    }

    public static AnimatedModelParams GetAnimParams(this GameObject obj)
    {
        var result = new AnimatedModelParams();
        result.scale = obj.GetScalePercent() / 100.0f;

        // Special case for equippable items
        if (obj.type.IsEquipment())
        {
            var itemFlags = obj.GetItemFlags();
            if (itemFlags.HasFlag(ItemFlag.DRAW_WHEN_PARENTED))
            {
                var parent = GameSystems.Item.GetParent(obj);
                if (parent != null)
                {
                    result.scale *= parent.GetScalePercent() / 100.0f;

                    result.attachedBoneName = GameSystems.Item.GetAttachBone(obj);
                    if (result.attachedBoneName != null)
                    {
                        var parentLoc = parent.GetLocationFull();
                        result.x = parentLoc.location.locx;
                        result.y = parentLoc.location.locy;
                        result.offsetX = parentLoc.off_x;
                        result.offsetY = parentLoc.off_y;

                        var parentDepth = GameSystems.Height.GetDepth(parentLoc);
                        var offsetZ = parent.OffsetZ;
                        result.offsetZ = offsetZ - parentDepth;

                        result.rotation = parent.Rotation;
                        result.rotationPitch = parent.RotationPitch;
                        result.parentAnim = parent.GetOrCreateAnimHandle();

                        return result;
                    }
                }
            }
        }

        var loc = obj.GetLocationFull();
        result.x = loc.location.locx;
        result.y = loc.location.locy;
        result.offsetX = loc.off_x;
        result.offsetY = loc.off_y;

        var flags = obj.GetFlags();
        sbyte depth = 0;
        if (!flags.HasFlag(ObjectFlag.NOHEIGHT))
        {
            depth = GameSystems.Height.GetDepth(loc);
        }

        result.offsetZ = obj.OffsetZ - depth;
        result.rotation = obj.Rotation;
        result.rotationPitch = obj.RotationPitch;

        return result;
    }

    [TempleDllLocation(0x100167f0)]
    public static EncodedAnimId GetIdleAnimId(this GameObject obj)
    {
        var protoId = GameSystems.D20.D20QueryInt(obj, D20DispatcherKey.QUE_Polymorphed);
        if (protoId != 0)
        {
            obj = GameSystems.Proto.GetProtoById((ushort) protoId);
        }

        if (!obj.IsCritter())
        {
            if (obj.type == ObjectType.portal)
            {
                return new EncodedAnimId(obj.IsPortalOpen() ? NormalAnimType.OpenIdle : NormalAnimType.ItemIdle);
            }
            else
            {
                return new EncodedAnimId(NormalAnimType.ItemIdle);
            }
        }
        else if (GameSystems.Critter.IsDeadNullDestroyed(obj))
        {
            return new EncodedAnimId(NormalAnimType.DeadIdle);
        }
        else if (GameSystems.Critter.IsDeadOrUnconscious(obj))
        {
            return new EncodedAnimId(NormalAnimType.DeathProneIdle);
        }
        else if (GameSystems.Critter.IsProne(obj))
        {
            return new EncodedAnimId(NormalAnimType.ProneIdle);
        }
        else if (GameSystems.Critter.IsConcealed(obj))
        {
            return new EncodedAnimId(NormalAnimType.ConcealIdle);
        }
        else if (GameSystems.Critter.IsMovingSilently(obj))
        {
            return new EncodedAnimId(NormalAnimType.SkillHideIdle);
        }
        else if (GameSystems.Critter.IsCombatModeActive(obj))
        {
            // Combat Idle
            return GameSystems.Critter.GetAnimId(obj, WeaponAnim.CombatIdle);
        }
        else
        {
            // Normal Idle
            return GameSystems.Critter.GetAnimId(obj, WeaponAnim.Idle);
        }
    }

    [TempleDllLocation(0x10016910)]
    public static EncodedAnimId GetFidgetAnimId(this GameObject obj)
    {
        if (!obj.IsCritter())
        {
            if (obj.type == ObjectType.portal)
            {
                return new EncodedAnimId(obj.IsPortalOpen() ? NormalAnimType.OpenIdle : NormalAnimType.ItemIdle);
            }
            else
            {
                return new EncodedAnimId(NormalAnimType.ItemFidget);
            }
        }
        else if (GameSystems.Critter.IsDeadNullDestroyed(obj))
        {
            return new EncodedAnimId(NormalAnimType.DeadFidget);
        }
        else if (GameSystems.Critter.IsDeadOrUnconscious(obj))
        {
            return new EncodedAnimId(NormalAnimType.DeathProneFidget);
        }
        else if (GameSystems.Critter.IsProne(obj))
        {
            return new EncodedAnimId(NormalAnimType.ProneFidget);
        }
        else if (GameSystems.Critter.IsConcealed(obj))
        {
            return new EncodedAnimId(NormalAnimType.ConcealIdle);
        }
        else if (GameSystems.Critter.IsMovingSilently(obj))
        {
            return new EncodedAnimId(NormalAnimType.SkillHideFidget);
        }
        else if (GameSystems.Critter.IsCombatModeActive(obj))
        {
            // Combat Fidget
            return GameSystems.Critter.GetAnimId(obj, WeaponAnim.CombatFidget);
        }
        else
        {
            // Normal Fidget
            WeaponAnim fidgetAnim;
            switch (GameSystems.Random.GetInt(0, 2))
            {
                default:
                    fidgetAnim = WeaponAnim.Fidget;
                    break;
                case 1:
                    fidgetAnim = WeaponAnim.Fidget2;
                    break;
                case 2:
                    fidgetAnim = WeaponAnim.Fidget3;
                    break;
            }

            return GameSystems.Critter.GetAnimId(obj, fidgetAnim);
        }
    }

    [TempleDllLocation(0x10021500)]
    public static void UpdateRadius(this GameObject obj, IAnimatedModel model)
    {
        var scale = GetScalePercent(obj);
        var radius = model.GetRadius(scale);

        if (radius > 0)
        {
            obj.SetFloat(obj_f.radius, radius);
            obj.SetFlag(ObjectFlag.RADIUS_SET, true);
        }
    }

    [TempleDllLocation(0x10021360)]
    public static void UpdateRenderHeight(this GameObject obj, IAnimatedModel model)
    {
        var scale = GetScalePercent(obj);
        var height = model.GetHeight(scale);

        obj.SetFloat(obj_f.render_height_3d, height);
        obj.SetFlag(ObjectFlag.HEIGHT_SET, true);
    }

    public static int GetScalePercent(this GameObject obj)
    {
        var modelScale = obj.GetInt32(obj_f.model_scale);

        if (obj.IsCritter())
        {
            DispIoMoveSpeed dispIo = DispIoMoveSpeed.Default;
            dispIo.bonlist.AddBonus(modelScale, 1, 102); // initial value

            var dispatcher = obj.GetDispatcher();
            dispatcher?.Process(DispatcherType.GetModelScale, D20DispatcherKey.NONE, dispIo);

            modelScale = dispIo.bonlist.OverallBonus;
        }

        return modelScale;
    }

    [TempleDllLocation(0x10021d50)]
    public static void SetAnimId(this GameObject obj, EncodedAnimId animId)
    {
        // Propagate animations to main/off hand equipment for critters
        if (obj.IsCritter())
        {
            var mainHand = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
            var offHand = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
            if (offHand == null)
            {
                offHand = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
            }

            // Apparently certain anim IDs cause weapons to disappear,
            // possibly skill use/casting?
            int opacity = 0;
            if (animId.IsSpecialAnim())
            {
                opacity = 255;
            }

            if (mainHand != null)
            {
                GameSystems.ObjFade.FadeTo(mainHand, opacity, 10, 16, 0);
                SetAnimId(mainHand, animId);
            }

            if (offHand != null)
            {
                GameSystems.ObjFade.FadeTo(offHand, opacity, 10, 16, 0);
                SetAnimId(offHand, animId);
            }
        }

        var model = obj.GetOrCreateAnimHandle();
        model.SetAnimId(animId);
    }

    [TempleDllLocation(0x100246f0)]
    public static void AdvanceAnimationTime(this GameObject obj, float deltaTimeInSecs)
    {
        var model = obj.GetOrCreateAnimHandle();
        var animParams = obj.GetAnimParams();
        model.Advance(deltaTimeInSecs, 0.0f, 0.0f, in animParams);
    }

    // sectorChanged was previously an int but only was ever 1 = location changed, 2 = sector changed
    [TempleDllLocation(0x10025050)]
    public static void UpdateRenderingState(this GameObject obj, bool sectorChanged)
    {
        obj.AdvanceAnimationTime(0.0f);
        var flags = obj.GetFlags();
        // Clears flags 0x04000000‬ and 0x02000000‬
        var renderflags = obj.GetUInt32(obj_f.render_flags) & 0xF9FFFFFF;

        var loc = obj.GetLocation();
        if (!flags.HasFlag(ObjectFlag.DISALLOW_WADING))
        {
            if (GameSystems.Tile.MapTileHasSinksFlag(loc))
            {
                if (!flags.HasFlag(ObjectFlag.WADING))
                    obj.SetFlag(ObjectFlag.WADING, true);
            }
            else if (flags.HasFlag(ObjectFlag.WADING))
            {
                obj.SetFlag(ObjectFlag.WADING, false);
            }
        }

        if (obj.IsPC() && GameSystems.Party.IsInParty(obj))
        {
            GameSystems.TownMap.sub_10052430(loc);
            GameSystems.Scroll.SetLocation(loc);
        }

        if (obj.IsCritter())
        {
            GameSystems.TileScript.TriggerTileScript(loc, obj);
        }

        var stashedObj = GameSystems.MapObject.GlobalStashedObject;
        if (stashedObj != null)
        {
            if (stashedObj == obj || GameSystems.Party.IsInParty(obj))
            {
                var firstPartyMember = GameSystems.Party.GetPCGroupMemberN(0);
                // TODO NULLSUB! call_ui_pfunc28(firstPartyMember, stashedObj);
            }
        }

        if (sectorChanged)
        {
            if (obj.IsPC() && GameSystems.Party.IsInParty(obj))
            {
                var sectorLoc = new SectorLoc(loc);
                HandleEnterNewSector(sectorLoc, obj);
            }

            GameSystems.MapSector.MapSectorResetLightHandle(obj);
        }

        obj.SetUInt32(obj_f.render_flags, renderflags);
    }

    private static void HandleEnterNewSector(SectorLoc loc, GameObject obj)
    {
        {
            using var sectorLock = new LockedMapSector(loc);
            var sector = sectorLock.Sector;
            var lightScheme = sector.lightScheme;
            if (lightScheme == 0)
                lightScheme = GameSystems.LightScheme.GetDefaultScheme();
            if (lightScheme != GameSystems.LightScheme.GetCurrentScheme())
            {
                var hourOfDay = GameSystems.TimeEvent.HourOfDay;
                GameSystems.LightScheme.SetCurrentScheme(sector.lightScheme, hourOfDay);
            }

            if (sector.soundList.scheme1 != 0 || sector.soundList.scheme2 != 0)
            {
                GameSystems.SoundGame.SetScheme(sector.soundList.scheme1, sector.soundList.scheme2);
            }
        }

        GameSystems.TileScript.TriggerSectorScript(loc, obj);

        var followers = GameSystems.Critter.EnumerateDirectFollowers(obj);
        foreach (var follower in followers)
        {
            GameSystems.Script.ExecuteObjectScript(obj, follower, 0,
                ObjScriptEvent.NewSector);
        }
    }

    [TempleDllLocation(0x10021290)]
    public static void DestroyRendering(this GameObject obj)
    {
        GameSystems.Light.RemoveAttachedTo(obj);

        var renderPalette = obj.GetInt32(obj_f.render_palette);
        if (renderPalette != 0)
        {
            // I think most of this is 2D rendering which is unused
            throw new NotImplementedException();
            // TODO sub_101EBD40(v1);
            obj.SetInt32(obj_f.render_palette, 0);
        }

        var renderColors = obj.GetUInt32(obj_f.render_colors);
        if (renderColors != 0)
        {
            throw new NotImplementedException();

            // TODO I think most of this is 2D rendering which is unused
            // TODO v3 = *(_DWORD *)(renderColors - 4);
            // TODO *(_DWORD *)(v3 + 4) = dword_107880A0;
            // TODO dword_107880A0 = (void *)v3;

            obj.SetInt32(obj_f.render_colors, 0);
        }

        var renderFlags = obj.GetUInt32(obj_f.render_flags);
        // Clear flags 76000000
        obj.SetUInt32(obj_f.render_flags, renderFlags & 0x89FFFFFF);

        var animHandle = new AasHandle(obj.GetUInt32(obj_f.animation_handle));
        if (animHandle)
        {
            GameSystems.AAS.ModelFactory.FreeHandle(animHandle.Handle);
            obj.SetUInt32(obj_f.animation_handle, 0);
        }

        GameSystems.ParticleSys.InvalidateObject(obj);
    }
}