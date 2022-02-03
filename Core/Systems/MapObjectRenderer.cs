using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems;

public enum ShadowType
{
    ShadowMap,
    Geometry,
    Blob
};

public class MapObjectRenderer : IDisposable
{
    public MapObjectRenderer(
        RenderingDevice device,
        MdfMaterialFactory mdfFactory,
        AasRenderer aasRenderer)
    {
        mDevice = device;
        mAasRenderer = aasRenderer;

        mHighlightMaterial = mdfFactory.LoadMaterial("art/meshes/hilight.mdf");
        mBlobShadowMaterial = mdfFactory.LoadMaterial("art/meshes/shadow.mdf");
        mOccludedMaterial = mdfFactory.LoadMaterial("art/meshes/occlusion.mdf");

        mGrappleController = new FrogGrappleController(device, mdfFactory);

        Globals.ConfigManager.OnConfigChanged += UpdateShadowType;
        UpdateShadowType();

        // Load the weapon glow effect files
        mGlowMaterials[0] = mdfFactory.LoadMaterial("art/meshes/wg_magic.mdf");
        mGlowMaterials[1] = mdfFactory.LoadMaterial("art/meshes/wg_acid.mdf");
        mGlowMaterials[2] = mdfFactory.LoadMaterial("art/meshes/wg_bane.mdf");
        mGlowMaterials[3] = mdfFactory.LoadMaterial("art/meshes/wg_chaotic.mdf");
        mGlowMaterials[4] = mdfFactory.LoadMaterial("art/meshes/wg_cold.mdf");
        mGlowMaterials[5] = mdfFactory.LoadMaterial("art/meshes/wg_fire.mdf");
        mGlowMaterials[6] = mdfFactory.LoadMaterial("art/meshes/wg_holy.mdf");
        mGlowMaterials[7] = mdfFactory.LoadMaterial("art/meshes/wg_law.mdf");
        mGlowMaterials[8] = mdfFactory.LoadMaterial("art/meshes/wg_shock.mdf");
        mGlowMaterials[9] = mdfFactory.LoadMaterial("art/meshes/wg_unholy.mdf");
    }

    private void UpdateShadowType()
    {
        switch (Globals.Config.ShadowType)
        {
            case 0:
                mShadowType = ShadowType.Blob;
                break;
            case 1:
                mShadowType = ShadowType.Geometry;
                break;
            case 2:
                mShadowType = ShadowType.ShadowMap;
                break;
        }
    }

    public void Dispose()
    {
        Globals.ConfigManager.OnConfigChanged -= UpdateShadowType;
    }

    public void RenderMapObjects(IGameViewport viewport, int tileX1, int tileX2, int tileY1, int tileY2)
    {
        using var perfGroup = mDevice.CreatePerfGroup("Map Objects");

        mTotalLastFrame = 0;
        mRenderedLastFrame = 0;

        using var iterator = new SectorIterator(tileX1, tileX2, tileY1, tileY2);
        foreach (var obj in iterator.EnumerateObjects())
        {
            RenderObject(viewport, obj, true);
        }
    }

    public void RenderObject(IGameViewport viewport, GameObject obj, bool showInvisible)
    {
        mTotalLastFrame++;

        var type = obj.type;
        var flags = obj.GetFlags();

        // Dont render destroyed or disabled objects
        const ObjectFlag dontDrawFlags = ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW;
        if ((flags & dontDrawFlags) != 0)
        {
            return;
        }

        // Hide invisible objects we're supposed to show them
        if ((flags & ObjectFlag.INVISIBLE) != 0 && !showInvisible)
        {
            return;
        }

        // Dont draw secret doors that haven't been found yet
        var secretDoorFlags = obj.GetSecretDoorFlags();
        if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR))
        {
            var found = secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND);
            if (!found && type != ObjectType.portal)
                return;
        }

        var animatedModel = obj.GetOrCreateAnimHandle();

        var animParams = obj.GetAnimParams();

        locXY worldLoc;

        GameObject parent = null;
        if (type.IsEquipment())
        {
            parent = GameSystems.Item.GetParent(obj);
        }

        var alpha = GetAlpha(obj);

        if (parent != null)
        {
            var parentAlpha = GetAlpha(parent);
            alpha = (alpha + parentAlpha) / 2;

            worldLoc = parent.GetLocation();
        }
        else
        {
            worldLoc = obj.GetLocation();
        }

        if (alpha == 0)
        {
            return;
        }

        // Handle fog occlusion of the world position
        if (type != ObjectType.container
            && (type == ObjectType.projectile
                || type.IsCritter()
                || type.IsEquipment())
            && (GameSystems.MapFogging.GetFogStatus(worldLoc, animParams.offsetX, animParams.offsetY) & 1) == 0)
        {
            return;
        }

        LocAndOffsets worldPosFull;
        worldPosFull.off_x = animParams.offsetX;
        worldPosFull.off_y = animParams.offsetY;
        worldPosFull.location = worldLoc;

        var radius = obj.GetRadius();
        var renderHeight = obj.GetRenderHeight(true);

        if (!IsObjectOnScreen(viewport.Camera, worldPosFull, animParams.offsetZ, radius, renderHeight))
        {
            return;
        }

        if (Globals.Config.drawObjCylinders)
        {
            Tig.ShapeRenderer3d.DrawCylinder(
                viewport,
                worldPosFull.ToInches3D(animParams.offsetZ),
                radius,
                renderHeight
            );
        }

        var lightSearchRadius = 0.0f;
        if (!flags.HasFlag(ObjectFlag.DONTLIGHT))
        {
            lightSearchRadius = radius;
        }

        LocAndOffsets locAndOffsets;
        locAndOffsets.location = worldLoc;
        locAndOffsets.off_x = animParams.offsetX;
        locAndOffsets.off_y = animParams.offsetY;
        var lights = FindLights(locAndOffsets, lightSearchRadius);

        if (type == ObjectType.weapon)
        {
            int glowType;
            if (flags.HasFlag(ObjectFlag.INVENTORY) && parent != null)
            {
                glowType = GameSystems.D20.GetWeaponGlowType(parent, obj);
            }
            else
            {
                glowType = GameSystems.D20.GetWeaponGlowType(null, obj);
            }

            if (glowType != 0 && glowType <= mGlowMaterials.Length)
            {
                var glowMaterial = mGlowMaterials[glowType - 1];
                if (glowMaterial.IsValid)
                {
                    RenderObjectHighlight(viewport, obj, glowMaterial);
                }
            }
        }

        if (GameSystems.ItemHighlight.ShowHighlights
            && (type.IsEquipment() &&
                !(flags.HasFlag(ObjectFlag.INVENTORY) || flags.HasFlag(ObjectFlag.CLICK_THROUGH))
                || GameSystems.Critter.IsLootableCorpse(obj)
                || type == ObjectType.portal))
        {
            RenderObjectHighlight(viewport, obj, mHighlightMaterial);

            // Add a single light with full ambient color to make the object appear fully lit
            lights.Clear();
            Light3d fullBrightLight = new Light3d();
            fullBrightLight.ambient = LinearColor.White;
            fullBrightLight.color = new LinearColor(0, 0, 0);
            fullBrightLight.dir = Vector4.UnitZ;
            fullBrightLight.type = Light3dType.Directional;
            lights.Add(fullBrightLight);
        }

        mRenderedLastFrame++;
        MdfRenderOverrides overrides = new MdfRenderOverrides();
        overrides.alpha = alpha / 255.0f;
        mAasRenderer.Render(viewport, animatedModel, animParams, lights, overrides);

        Light3d globalLight = new Light3d();
        if (lights.Count > 0)
        {
            globalLight = lights[0];
        }

        if (type.IsCritter())
        {
            if (alpha > 16)
            {
                if (mShadowType == ShadowType.ShadowMap)
                {
                    RenderShadowMapShadow(viewport, obj, animParams, animatedModel, globalLight, alpha);
                }
                else if (mShadowType == ShadowType.Geometry)
                {
                    mAasRenderer.RenderGeometryShadow(
                        viewport.Camera,
                        animatedModel,
                        animParams,
                        globalLight,
                        alpha / 255.0f);
                }
                else if (mShadowType == ShadowType.Blob)
                {
                    RenderBlobShadow(viewport, obj, animatedModel, ref animParams, alpha);
                }
            }

            /*
            This renders the equipment in a critter's hand separately, but
            I am not certain *why* exactly. I thought this would have been
            handled by addmeshes, but it might be that there's a distinct
            difference between addmeshes that are skinned onto the mobile's
            skeleton and equipment that is unskinned and just positioned
            in the player's hands.
            */
            var weaponPrim = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
            if (weaponPrim != null)
            {
                RenderObject(viewport, weaponPrim, showInvisible);
            }

            var weaponSec = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
            if (weaponSec != null)
            {
                RenderObject(viewport, weaponSec, showInvisible);
            }

            var shield = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
            if (shield != null)
            {
                RenderObject(viewport, shield, showInvisible);
            }
        }
        else if (type.IsEquipment() && mShadowType == ShadowType.Geometry)
        {
            mAasRenderer.RenderGeometryShadow(
                viewport.Camera,
                animatedModel,
                animParams,
                globalLight,
                alpha / 255.0f);
        }

        RenderMirrorImages(
            viewport,
            obj,
            animParams,
            animatedModel,
            lights);

        if (mGrappleController.IsGiantFrog(obj))
        {
            mGrappleController.AdvanceAndRender(
                viewport,
                obj,
                animParams,
                animatedModel,
                lights,
                alpha / 255.0f);
        }
    }

    public void RenderObjectInUi(GameObject obj, int x, int y, float rotation, float scale)
    {
        // TODO: This is a terrible way of doing this
        var viewport = GameViews.Primary;
        var worldPos = viewport.ScreenToWorld(x, y);

        var animatedModel = obj.GetOrCreateAnimHandle();

        var animParams = obj.GetAnimParams();
        animParams.x = 0;
        animParams.y = 0;
        animParams.offsetX = worldPos.X + 865.70398f;
        animParams.offsetZ = 1200.0f;
        animParams.offsetY = worldPos.Z + 865.70398f;
        animParams.scale *= scale;
        animParams.rotation = MathF.PI + rotation;
        animParams.rotationPitch = 0;

        var lights = new List<Light3d>
        {
            new Light3d
            {
                type = Light3dType.Directional,
                color = new LinearColor(1, 1, 1),
                dir = new Vector4(-0.7071200013160706f, -0.7071200013160706f, 0, 0),
            },

            new Light3d
            {
                type = Light3dType.Directional,
                color = new LinearColor(1, 1, 1),
                dir = new Vector4(0, 0.7071200013160706f, -0.7071200013160706f, 0),
            }
        };

        mAasRenderer.Render(viewport, animatedModel, animParams, lights);

        if (obj.IsCritter())
        {
            var weaponPrim = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
            if (weaponPrim != null)
            {
                RenderObjectInUi(weaponPrim, x, y, rotation, scale);
            }

            var weaponSec = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
            if (weaponSec != null)
            {
                RenderObjectInUi(weaponSec, x, y, rotation, scale);
            }

            var shield = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
            if (shield != null)
            {
                RenderObjectInUi(shield, x, y, rotation, scale);
            }
        }
    }

    public void RenderOccludedMapObjects(IGameViewport viewport, int tileX1, int tileX2, int tileY1, int tileY2)
    {
        using var _ = mDevice.CreatePerfGroup("Occluded Map Objects");

        for (var secY = tileY1 / 64; secY <= tileY2 / 64; ++secY)
        {
            for (var secX = tileX1 / 64; secX <= tileX2 / 64; ++secX)
            {
                using var sector = new LockedMapSector(secX, secY);

                foreach (var obj in sector.EnumerateObjects())
                {
                    RenderOccludedObject(viewport, obj);
                }
            }
        }
    }

    public void RenderOccludedObject(IGameViewport viewport, GameObject obj)
    {
        mTotalLastFrame++;

        var type = obj.type;
        var flags = obj.GetFlags();

        // Dont render destroyed or disabled objects
        const ObjectFlag dontDrawFlags = ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW;
        if ((flags & dontDrawFlags) != 0)
        {
            return;
        }

        if (flags.HasFlag(ObjectFlag.INVISIBLE) || flags.HasFlag(ObjectFlag.INVENTORY))
        {
            return;
        }

        switch (type)
        {
            case ObjectType.scenery:
            case ObjectType.trap:
                return;
            case ObjectType.pc:
            case ObjectType.npc:
                if (GameSystems.Critter.IsConcealed(obj))
                {
                    return;
                }

                break;
            default:
                break;
        }

        // Dont draw secret doors that haven't been found yet
        var secretDoorFlags = obj.GetSecretDoorFlags();
        if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR))
        {
            var found = ((secretDoorFlags & SecretDoorFlag.SECRET_DOOR_FOUND) != 0);
            if (!found && type != ObjectType.portal)
                return;
        }

        var animatedModel = obj.GetOrCreateAnimHandle();

        var animParams = obj.GetAnimParams();

        locXY worldLoc;

        GameObject parent = null;
        if (type.IsEquipment())
        {
            parent = GameSystems.Item.GetParent(obj);
        }

        var alpha = GetAlpha(obj);

        if (parent != null)
        {
            var parentAlpha = GetAlpha(parent);
            alpha = (alpha + parentAlpha) / 2;

            worldLoc = parent.GetLocation();
        }
        else
        {
            worldLoc = obj.GetLocation();
        }

        if (alpha == 0)
        {
            return;
        }

        // Handle fog occlusion of the world position, but handle it differently for portals
        if (type != ObjectType.portal)
        {
            var fogStatus = GameSystems.MapFogging.GetFogStatus(worldLoc, animParams.offsetX, animParams.offsetY);
            if ((fogStatus & 0xB0) == 0 || (fogStatus & 1) == 0)
            {
                return;
            }
        }
        else
        {
            LocAndOffsets loc;
            loc.location = worldLoc;
            loc.off_x = animParams.offsetX - locXY.INCH_PER_SUBTILE;
            loc.off_y = animParams.offsetY - locXY.INCH_PER_SUBTILE;
            loc.Normalize();

            var fogStatus = GameSystems.MapFogging.GetFogStatus(loc.location, loc.off_x, loc.off_y);
            if ((fogStatus & 0xB0) == 0 || (fogStatus & 1) == 0)
            {
                return;
            }
        }

        LocAndOffsets worldPosFull;
        worldPosFull.off_x = animParams.offsetX;
        worldPosFull.off_y = animParams.offsetY;
        worldPosFull.location = worldLoc;

        var radius = obj.GetRadius();
        var renderHeight = obj.GetRenderHeight(true);

        if (!IsObjectOnScreen(viewport.Camera, worldPosFull, animParams.offsetZ, radius, renderHeight))
        {
            return;
        }

        var lightSearchRadius = 0.0f;
        if (!flags.HasFlag(ObjectFlag.DONTLIGHT))
        {
            lightSearchRadius = radius;
        }

        LocAndOffsets locAndOffsets;
        locAndOffsets.location = worldLoc;
        locAndOffsets.off_x = animParams.offsetX;
        locAndOffsets.off_y = animParams.offsetY;
        var lights = FindLights(locAndOffsets, lightSearchRadius);

        mRenderedLastFrame++;
        MdfRenderOverrides overrides = new MdfRenderOverrides();
        overrides.alpha = alpha / 255.0f;

        if (type != ObjectType.portal)
        {
            mOccludedMaterial.Resource.Bind(viewport, mDevice, lights, overrides);
            mAasRenderer.RenderWithoutMaterial(animatedModel, animParams);

            if (type.IsCritter())
            {
                /*
                This renders the equipment in a critter's hand separately, but
                I am not certain *why* exactly. I thought this would have been
                handled by addmeshes, but it might be that there's a distinct
                difference between addmeshes that are skinned onto the mobile's
                skeleton and equipment that is unskinned and just positioned
                in the player's hands.
                */
                var weaponPrim = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
                if (weaponPrim != null)
                {
                    RenderOccludedObject(viewport, weaponPrim);
                }

                var weaponSec = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
                if (weaponSec != null)
                {
                    RenderOccludedObject(viewport, weaponSec);
                }

                var shield = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
                if (shield != null)
                {
                    RenderOccludedObject(viewport, shield);
                }
            }
        }
        else
        {
            if (GameSystems.ItemHighlight.ShowHighlights)
            {
                overrides.ignoreLighting = true;
            }

            mAasRenderer.Render(viewport, animatedModel, animParams, lights, overrides);
        }
    }

    public void RenderObjectHighlight(IGameViewport viewport, GameObject obj,
        ResourceRef<IMdfRenderMaterial> material)
    {
        mTotalLastFrame++;

        var type = obj.type;
        var flags = obj.GetFlags();

        // Dont render destroyed or disabled objects
        const ObjectFlag dontDrawFlags = ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW;
        if ((flags & dontDrawFlags) != 0)
        {
            return;
        }

        // Hide invisible objects we're supposed to show them
        if (flags.HasFlag(ObjectFlag.INVISIBLE))
        {
            return;
        }

        // Dont draw secret doors that haven't been found yet
        var secretDoorFlags = obj.GetSecretDoorFlags();
        if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR))
        {
            var found = secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND);
            if (!found && type != ObjectType.portal)
                return;
        }

        var animatedModel = obj.GetOrCreateAnimHandle();
        var animParams = obj.GetAnimParams();

        locXY worldLoc;

        GameObject parent = null;
        if (type.IsEquipment())
        {
            parent = GameSystems.Item.GetParent(obj);
        }

        var alpha = GetAlpha(obj);

        if (parent != null)
        {
            var parentAlpha = GetAlpha(parent);
            alpha = (alpha + parentAlpha) / 2;

            worldLoc = parent.GetLocation();
        }
        else
        {
            worldLoc = obj.GetLocation();
        }

        if (alpha == 0)
        {
            return;
        }

        // Handle fog occlusion of the world position
        if (type != ObjectType.container
            && (type == ObjectType.projectile
                || type.IsCritter()
                || type.IsEquipment())
            && (GameSystems.MapFogging.GetFogStatus(worldLoc, animParams.offsetX, animParams.offsetY) & 1) == 0)
        {
            return;
        }

        LocAndOffsets worldPosFull;
        worldPosFull.off_x = animParams.offsetX;
        worldPosFull.off_y = animParams.offsetY;
        worldPosFull.location = worldLoc;

        var radius = obj.GetRadius();
        var renderHeight = obj.GetRenderHeight(true);

        if (!IsObjectOnScreen(viewport.Camera, worldPosFull, animParams.offsetZ, radius, renderHeight))
        {
            return;
        }

        var lightSearchRadius = 0.0f;
        if (!flags.HasFlag(ObjectFlag.DONTLIGHT))
        {
            lightSearchRadius = radius;
        }

        LocAndOffsets locAndOffsets;
        locAndOffsets.location = worldLoc;
        locAndOffsets.off_x = animParams.offsetX;
        locAndOffsets.off_y = animParams.offsetY;
        var lights = FindLights(locAndOffsets, lightSearchRadius);

        mRenderedLastFrame++;

        MdfRenderOverrides overrides = new MdfRenderOverrides();
        overrides.alpha = alpha / 255.0f;
        material.Resource.Bind(viewport, mDevice, lights, overrides);
        mAasRenderer.RenderWithoutMaterial(animatedModel, animParams);
    }

    public int GetRenderedLastFrame()
    {
        return mRenderedLastFrame;
    }

    public int GetTotalLastFrame()
    {
        return mTotalLastFrame;
    }

    public ShadowType GetShadowType()
    {
        return mShadowType;
    }

    public void SetShadowType(ShadowType type)
    {
        mShadowType = type;
    }

    public List<Light3d> FindLights(LocAndOffsets atLocation, float radius)
    {
        List<Light3d> lights = new List<Light3d>();

        if (GameSystems.Light.IsGlobalLightEnabled)
        {
            Light3d light = new Light3d();
            var legacyLight = GameSystems.Light.GlobalLight;
            light.type = (Light3dType) legacyLight.type;
            light.color = legacyLight.Color;
            light.dir.X = legacyLight.dir.X;
            light.dir.Y = legacyLight.dir.Y;
            light.dir.Z = legacyLight.dir.Z;
            light.pos.X = legacyLight.pos.X;
            light.pos.Y = legacyLight.pos.Y;
            light.pos.Z = legacyLight.pos.Z;
            light.range = legacyLight.range;
            light.phi = legacyLight.phi;
            lights.Add(light);
        }

        if (radius == 0)
        {
            return lights;
        }

        // Build a box that has twice the radius convert to tiles as it's width/height
        // For some reason, ToEE will add one more INCH_PER_TILE here, which translates to
        // roughly 28 tiles more search radius than is needed
        var boxDimensions = (int) (radius / locXY.INCH_PER_TILE + locXY.INCH_PER_TILE);
        var tileX1 = atLocation.location.locx - 1 - boxDimensions;
        var tileX2 = atLocation.location.locx + 1 + boxDimensions;
        var tileY1 = atLocation.location.locy - 1 - boxDimensions;
        var tileY2 = atLocation.location.locy + 1 + boxDimensions;

        using var sectorIterator = new SectorIterator(tileX1, tileX2, tileY1, tileY2);

        var atPos = atLocation.ToInches2D();

        while (sectorIterator.HasNext)
        {
            using var sector = sectorIterator.Next();

            foreach (ref var light in sector.Lights)
            {
                int type;
                LinearColor color;
                Vector3 direction;
                float range, phi;
                var lightPos = light.position.ToInches2D();

                if ((light.flags & 0x40) != 0)
                {
                    if (GameSystems.Light.IsNight)
                    {
                        type = light.light2.type;
                        color = light.light2.color;
                        direction = light.light2.direction;
                        range = light.range; // Notice how it's using light 1's range
                        phi = light.light2.phi;

                        /*
                        Kill the daytime particle system if it's night and the
                        daytime particle system is still alive.
                        */
                        if (light.partSys.handle != null)
                        {
                            GameSystems.ParticleSys.Remove(light.partSys.handle);
                            light.partSys.handle = null;
                        }

                        /*
                        If the nighttime particle system has not yet been started,
                        do it here.
                        */
                        ref var nightPartSys = ref light.light2.partSys;
                        if (nightPartSys.handle == null && nightPartSys.hashCode != 0)
                        {
                            var centerOfTile = light.position.ToInches3D(light.offsetZ);
                            nightPartSys.handle = GameSystems.ParticleSys.CreateAt(
                                nightPartSys.hashCode, centerOfTile
                            );
                        }
                    }
                    else
                    {
                        type = light.type;
                        color = light.color;
                        direction = light.direction;
                        range = light.range;
                        phi = light.phi;

                        // This is just the inverse of what we're doing at night (see above)
                        ref var nightPartSys = ref light.light2.partSys;
                        if (nightPartSys.handle != null)
                        {
                            GameSystems.ParticleSys.Remove(nightPartSys.handle);
                            nightPartSys.handle = null;
                        }

                        ref var dayPartSys = ref light.partSys;
                        if (dayPartSys.handle == null && dayPartSys.hashCode != 0)
                        {
                            var centerOfTile = light.position.ToInches3D(light.offsetZ);
                            dayPartSys.handle = GameSystems.ParticleSys.CreateAt(
                                dayPartSys.hashCode, centerOfTile
                            );
                        }
                    }
                }
                else
                {
                    type = light.type;
                    color = light.color;
                    direction = light.direction;
                    range = light.range;
                    phi = light.phi;
                }

                if (type == 0)
                {
                    continue;
                }

                // Distance (Squared) between pos and light pos
                var acceptableDistance = (int) (radius + light.range);
                var acceptableDistanceSquared = acceptableDistance * acceptableDistance;
                var diffX = (int) (atPos.X - lightPos.X);
                var diffY = (int) (atPos.Y - lightPos.Y);
                var distanceSquared = diffX * diffX + diffY * diffY;
                if (distanceSquared > acceptableDistanceSquared)
                {
                    continue;
                }

                Light3d light3d = new Light3d();
                if (type == 2)
                {
                    light3d.type = Light3dType.Directional;
                    light3d.dir = new Vector4(Vector3.Normalize(direction), 0);
                }
                else if (type == 3)
                {
                    light3d.type = Light3dType.Spot;
                    light3d.dir = new Vector4(Vector3.Normalize(direction), 0);
                }
                else if (type == 1)
                {
                    light3d.type = Light3dType.Point;
                    light3d.dir.X = direction.X;
                    light3d.dir.Y = direction.Y;
                    light3d.dir.Z = direction.Z;
                }

                // Some vanilla lights are broken
                if (light3d.dir.X == 0.0f && light3d.dir.Y == 0.0f && light3d.dir.Z == 0.0f)
                {
                    light3d.dir.X = 0.0f;
                    light3d.dir.Y = 0.0f;
                    light3d.dir.Z = 1.0f;
                }

                light3d.pos.X = lightPos.X;
                light3d.pos.Y = light.offsetZ;
                light3d.pos.Z = lightPos.Y;

                light3d.color = color;

                light3d.range = range;
                light3d.phi = phi;
                lights.Add(light3d);
            }
        }

        return lights;
    }

    private RenderingDevice mDevice;
    private AasRenderer mAasRenderer;
    private ShadowType mShadowType = ShadowType.ShadowMap;
    private ResourceRef<IMdfRenderMaterial> mBlobShadowMaterial;
    private ResourceRef<IMdfRenderMaterial> mHighlightMaterial;
    private ResourceRef<IMdfRenderMaterial> mOccludedMaterial;
    private FrogGrappleController mGrappleController;
    ResourceRef<IMdfRenderMaterial>[] mGlowMaterials = new ResourceRef<IMdfRenderMaterial>[10];

    private int mRenderedLastFrame = 0;
    private int mTotalLastFrame = 0;

    /*
    Same as sin45 incidentally.
    The idea seems to be that the vertical height of the model is scaled
    according to the camera inclination of 45°.
    */
    private const float cos45 = 0.70709997f;

    private bool IsObjectOnScreen(WorldCamera camera, in LocAndOffsets location, float offsetZ, float radius, float renderHeight)
    {
        var centerOfTile3d = location.ToInches3D();
        var screenPos = camera.WorldToScreenUi(centerOfTile3d);

        // This checks if the object's screen bounding box is off screen
        var bbLeft = screenPos.X - radius;
        var bbRight = screenPos.X + radius;
        var bbTop = screenPos.Y - (offsetZ + renderHeight + radius) * cos45;
        var bbBottom = bbTop + (2 * radius + renderHeight) * cos45;

        var screenWidth = camera.GetViewportWidth();
        var screenHeight = camera.GetViewportHeight();
        if (bbRight < 0 || bbBottom < 0 || bbLeft > screenWidth || bbTop > screenHeight)
        {
            return false;
        }

        return true;
    }

    private TimePoint _mirrorImagesLastRenderTime = default;
    private float _mirrorImagesRotation = 0;

    private void RenderMirrorImages(IGameViewport viewport,
        GameObject obj,
        AnimatedModelParams animParams,
        IAnimatedModel model,
        IList<Light3d> lights)
    {
        var mirrorImages = GameSystems.D20.D20QueryInt(obj, D20DispatcherKey.QUE_Critter_Has_Mirror_Image);

        if (mirrorImages == 0)
        {
            return;
        }

        // The rotation of the mirror images is animated
        if (_mirrorImagesLastRenderTime != default)
        {
            var elapsedSecs = (float) (TimePoint.Now - _mirrorImagesLastRenderTime).TotalSeconds;
            // One full rotation (2PI) in 16 seconds
            _mirrorImagesRotation += elapsedSecs * 2 * MathF.PI / 16.0f;

            // Wrap the rotation around
            while (_mirrorImagesRotation >= 2 * MathF.PI)
            {
                _mirrorImagesRotation -= 2 * MathF.PI;
            }
        }

        _mirrorImagesLastRenderTime = TimePoint.Now;

        // The images should partially overlap the actual model
        var radius = obj.GetRadius() * 0.75f;

        for (var i = 0; i < mirrorImages; ++i)
        {
            // Draw one half on the left and the other on the right,
            // if there are an uneven number, the excess image is drawn on the left
            int pos = i + 1;
            if (pos > (int) mirrorImages / 2)
            {
                pos = pos - mirrorImages - 1;
            }

            // Generate a world matrix that applies the translation
            MdfRenderOverrides overrides = new MdfRenderOverrides();
            overrides.useWorldMatrix = true;
            var xTrans = MathF.Cos(_mirrorImagesRotation) * pos * radius;
            var yTrans = MathF.Sin(_mirrorImagesRotation) * pos * radius;
            overrides.worldMatrix = Matrix4x4.CreateTranslation(xTrans, 0, yTrans);
            overrides.alpha = 0.31f;

            mAasRenderer.Render(viewport, model, animParams, lights, overrides);
        }
    }

    private void RenderShadowMapShadow(IGameViewport viewport,
        GameObject obj,
        AnimatedModelParams animParams,
        IAnimatedModel model,
        Light3d globalLight,
        int alpha)
    {
        LocAndOffsets loc = new LocAndOffsets(animParams.x, animParams.y, animParams.offsetX, animParams.offsetY);
        var worldPos = loc.ToInches3D(animParams.offsetZ);

        var radius = obj.GetRadius();
        var height = obj.GetRenderHeight();

        var shadowModels = new List<IAnimatedModel>(3); // TODO POOL
        var shadowParams = new List<AnimatedModelParams>(3); // TODO POOL

        // The critter model always has a shadow
        shadowModels.Add(model);
        shadowParams.Add(animParams);

        var primaryWeapon = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
        if (primaryWeapon != null)
        {
            var primaryWeaponModel = primaryWeapon.GetOrCreateAnimHandle();
            var primaryWeaponParams = primaryWeapon.GetAnimParams();

            shadowModels.Add(primaryWeaponModel);
            shadowParams.Add(primaryWeaponParams);
        }

        var secondaryWeapon = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
        if (secondaryWeapon != null)
        {
            var secondaryWeaponModel = secondaryWeapon.GetOrCreateAnimHandle();
            var secondaryWeaponParams = secondaryWeapon.GetAnimParams();

            shadowModels.Add(secondaryWeaponModel);
            shadowParams.Add(secondaryWeaponParams);
        }

        mAasRenderer.RenderShadowMapShadow(
            viewport,
            shadowModels,
            shadowParams,
            worldPos,
            radius,
            height,
            globalLight.dir,
            alpha / 255.0f,
            Globals.Config.SoftShadows
        );
    }

    private void RenderBlobShadow(
        IGameViewport viewport,
        GameObject handle,
        IAnimatedModel model,
        ref AnimatedModelParams animParams,
        int alpha)
    {
        var shapeRenderer3d = Tig.ShapeRenderer3d;

        Span<ShapeVertex3d> corners = stackalloc ShapeVertex3d[4];

        LocAndOffsets loc = new LocAndOffsets(animParams.x, animParams.y, animParams.offsetX, animParams.offsetY);
        var center = loc.ToInches3D(animParams.offsetZ);

        var radius = handle.GetRadius();

        corners[0].pos.X = center.X - radius;
        corners[0].pos.Y = center.Y;
        corners[0].pos.Z = center.Z - radius;
        corners[0].uv = Vector2.Zero;

        corners[1].pos.X = center.X + radius;
        corners[1].pos.Y = center.Y;
        corners[1].pos.Z = center.Z - radius;
        corners[1].uv = Vector2.UnitX;

        corners[2].pos.X = center.X + radius;
        corners[2].pos.Y = center.Y;
        corners[2].pos.Z = center.Z + radius;
        corners[2].uv = Vector2.One;

        corners[3].pos.X = center.X - radius;
        corners[3].pos.Y = center.Y;
        corners[3].pos.Z = center.Z + radius;
        corners[3].uv = Vector2.UnitY;

        var color = new PackedLinearColorA(mBlobShadowMaterial.Resource.GetSpec().diffuse);
        color.A = (byte) (color.A * alpha / 255);
        shapeRenderer3d.DrawQuad(viewport, corners, mBlobShadowMaterial.Resource, color);
    }

    private int GetAlpha(GameObject obj) => obj.GetInt32(obj_f.transparency);
}