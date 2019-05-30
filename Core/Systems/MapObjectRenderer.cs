using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{

public enum ShadowType {
	ShadowMap,
	Geometry,
	Blob
};

public class MapObjectRenderer : IDisposable {

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

		Globals.Config.AddVanillaSetting("shadow_type", "2", () => {
			int shadowType = Globals.Config.GetVanillaInt("shadow_type");
			switch (shadowType) {
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
			});

		switch (Globals.Config.GetVanillaInt("shadow_type")) {
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
	public void Dispose() {
		Globals.Config.RemoveVanillaCallback("shadow_type");
	}

	public void RenderMapObjects(int tileX1, int tileX2, int tileY1, int tileY2) {

		using var perfGroup = mDevice.CreatePerfGroup("Map Objects");

		mTotalLastFrame = 0;
		mRenderedLastFrame = 0;

		for (var secY = tileY1 / 64; secY <= tileY2 / 64; ++secY) {
			for (var secX = tileX1 / 64; secX <= tileX2 / 64; ++secX) {

				using var sector = new LockedMapSector(secX, secY);
				if (!sector.IsValid)
				{
					continue;
				}

				for (var tx = 0; tx < 64; ++tx) {
					for (var ty = 0; ty < 64; ++ty) {
						foreach (var obj in sector.GetObjectsAt(tx, ty)) {
							RenderObject(obj, true);
						}
					}
				}
			}
		}

	}
	public void RenderObject(GameObjectBody obj, bool showInvisible) {

		mTotalLastFrame++;

		var type = obj.type;
		var flags = obj.GetFlags();

		// Dont render destroyed or disabled objects
		const ObjectFlag dontDrawFlags = ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW;
		if ((flags & dontDrawFlags) != 0) {
			return;
		}

		// Hide invisible objects we're supposed to show them
		if ((flags & ObjectFlag.INVISIBLE) != 0 && !showInvisible) {
			return;
		}

		// Dont draw secret doors that haven't been found yet
		var secretDoorFlags = obj.GetSecretDoorFlags();
		if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR)) {
			var found = secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND);
			if (!found && type != ObjectType.portal)
				return;
		}

		var animatedModel = obj.GetOrCreateAnimHandle();

		var animParams = obj.GetAnimParams();

		locXY worldLoc;

		GameObjectBody parent = null;
		if (type.IsEquipment()) {
			parent = GameSystems.Item.GetParent(obj);
		}

		var alpha = GetAlpha(obj);

		if (parent != null) {
			var parentAlpha = GetAlpha(parent);
			alpha = (alpha + parentAlpha) / 2;

			worldLoc = parent.GetLocation();
		}
		else {
			worldLoc = obj.GetLocation();
		}

		if (alpha == 0) {
			return;
		}

		// Handle fog occlusion of the world position
		if (type != ObjectType.container
			&& (type == ObjectType.projectile
				|| type.IsCritter()
				|| type.IsEquipment())
			&& (GameSystems.MapFogging.GetFogStatus(worldLoc, animParams.offsetX, animParams.offsetY) & 1) == 0) {
			return;
		}

		LocAndOffsets worldPosFull;
		worldPosFull.off_x = animParams.offsetX;
		worldPosFull.off_y = animParams.offsetY;
		worldPosFull.location = worldLoc;

		var radius = obj.GetRadius();
		var renderHeight = obj.GetRenderHeight(true);

		if (!IsObjectOnScreen(worldPosFull, animParams.offsetZ, radius, renderHeight)) {
			return;
		}

		if (Globals.Config.drawObjCylinders) {
			Tig.ShapeRenderer3d.DrawCylinder(
				worldPosFull.ToInches3D(animParams.offsetZ),
				radius,
				renderHeight
			);
		}

		var lightSearchRadius = 0.0f;
		if (!flags.HasFlag(ObjectFlag.DONTLIGHT)) {
			lightSearchRadius = radius;
		}

		LocAndOffsets locAndOffsets;
		locAndOffsets.location = worldLoc;
		locAndOffsets.off_x = animParams.offsetX;
		locAndOffsets.off_y = animParams.offsetY;
		var lights = FindLights(locAndOffsets, lightSearchRadius);

		if (type == ObjectType.weapon) {
			int glowType;
			if (flags.HasFlag(ObjectFlag.INVENTORY) && parent != null) {
				glowType = GameSystems.D20.GetWeaponGlowType(parent, obj);
			} else {
				glowType = GameSystems.D20.GetWeaponGlowType(null, obj);
			}

			if (glowType != 0 && glowType <= mGlowMaterials.Length) {
				var glowMaterial = mGlowMaterials[glowType - 1];
				if (glowMaterial.IsValid) {
					RenderObjectHighlight(obj, glowMaterial);
				}
			}
		}

		if (mShowHighlights
			&& (type.IsEquipment() && !(flags.HasFlag(ObjectFlag.INVENTORY) || flags.HasFlag(ObjectFlag.CLICK_THROUGH))
				|| GameSystems.Critter.IsLootableCorpse(obj)
				|| type == ObjectType.portal))
		{
			RenderObjectHighlight(obj, mHighlightMaterial);

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
		mAasRenderer.Render(animatedModel, animParams, lights, overrides);

		Light3d globalLight = default;
		if (lights.Count > 0) {
			globalLight = lights[0];
		}

		if (type.IsCritter()) {
			if (alpha > 16) {
				if (mShadowType == ShadowType.ShadowMap)
				{
					RenderShadowMapShadow(obj, animParams, animatedModel, globalLight, alpha);
				}
				else if (mShadowType == ShadowType.Geometry) {
					mAasRenderer.RenderGeometryShadow(animatedModel,
						animParams,
						globalLight,
						alpha / 255.0f);
				}
				else if (mShadowType == ShadowType.Blob) {
					RenderBlobShadow(obj, animatedModel, ref animParams, alpha);
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
			if (weaponPrim != null) {
				RenderObject(weaponPrim, showInvisible);
			}
			var weaponSec = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
			if (weaponSec != null) {
				RenderObject(weaponSec, showInvisible);
			}
			var shield = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
			if (shield != null) {
				RenderObject(shield, showInvisible);
			}

		}
		else if (type.IsEquipment() && mShadowType == ShadowType.Geometry)
		{
			mAasRenderer.RenderGeometryShadow(animatedModel,
				animParams,
				globalLight,
				alpha / 255.0f);
		}

		RenderMirrorImages(obj,
			animParams,
			animatedModel,
			lights);

		if (mGrappleController.IsGiantFrog(obj)) {
			mGrappleController.AdvanceAndRender(obj,
				animParams,
				animatedModel,
				lights,
				alpha / 255.0f);
		}

	}
	public void RenderObjectInUi(GameObjectBody obj, int x, int y, float rotation, float scale) {

		var worldPos = mDevice.GetCamera().ScreenToWorld((float)x, (float)y);

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

		mAasRenderer.Render(animatedModel, animParams, lights);

		if (obj.IsCritter()) {
			var weaponPrim = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
			if (weaponPrim != null) {
				RenderObjectInUi(weaponPrim, x, y, rotation, scale);
			}
			var weaponSec = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
			if (weaponSec != null) {
				RenderObjectInUi(weaponSec, x, y, rotation, scale);
			}
			var shield = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
			if (shield != null) {
				RenderObjectInUi(shield, x, y, rotation, scale);
			}
		}

	}
	public void RenderOccludedMapObjects(int tileX1, int tileX2, int tileY1, int tileY2) {

		using var _ = mDevice.CreatePerfGroup("Occluded Map Objects");

		for (var secY = tileY1 / 64; secY <= tileY2 / 64; ++secY) {
			for (var secX = tileX1 / 64; secX <= tileX2 / 64; ++secX) {

				using var sector = new LockedMapSector(secX, secY);

				for (var tx = 0; tx < Sector.SectorSideSize; ++tx) {
					for (var ty = 0; ty < Sector.SectorSideSize; ++ty) {
						foreach (var obj in sector.GetObjectsAt(tx, ty)) {
							RenderOccludedObject(obj);
						}
					}
				}
			}
		}

	}
	public void RenderOccludedObject(GameObjectBody obj) {

		mTotalLastFrame++;

		var type = obj.type;
		var flags = obj.GetFlags();

		// Dont render destroyed or disabled objects
		const ObjectFlag dontDrawFlags = ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW;
		if ((flags & dontDrawFlags) != 0) {
			return;
		}
		if (flags.HasFlag(ObjectFlag.INVISIBLE) || flags.HasFlag(ObjectFlag.INVENTORY)) {
			return;
		}

		switch (type) {
		case ObjectType.scenery:
		case ObjectType.trap:
			return;
		case ObjectType.pc:
		case ObjectType.npc:
			if (GameSystems.Critter.IsConcealed(obj)) {
				return;
			}
			break;
		default:
			break;
		}

		// Dont draw secret doors that haven't been found yet
		var secretDoorFlags = obj.GetSecretDoorFlags();
		if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR)) {
			var found = ((secretDoorFlags & SecretDoorFlag.SECRET_DOOR_FOUND) != 0);
			if (!found && type != ObjectType.portal)
				return;
		}

		var animatedModel = obj.GetOrCreateAnimHandle();

		var animParams = obj.GetAnimParams();

		locXY worldLoc;

		GameObjectBody parent = null;
		if (type.IsEquipment()) {
			parent = GameSystems.Item.GetParent(obj);
		}

		var alpha = GetAlpha(obj);

		if (parent != null) {
			var parentAlpha = GetAlpha(parent);
			alpha = (alpha + parentAlpha) / 2;

			worldLoc = parent.GetLocation();
		} else {
			worldLoc = obj.GetLocation();
		}

		if (alpha == 0) {
			return;
		}

		// Handle fog occlusion of the world position, but handle it differently for portals
		if (type != ObjectType.portal) {
			var fogStatus = GameSystems.MapFogging.GetFogStatus(worldLoc, animParams.offsetX, animParams.offsetY);
			if ((fogStatus & 0xB0) == 0 || (fogStatus & 1) == 0) {
				return;
			}
		}
		else {
			LocAndOffsets loc;
			loc.location = worldLoc;
			loc.off_x = animParams.offsetX - locXY.INCH_PER_SUBTILE;
			loc.off_y = animParams.offsetY - locXY.INCH_PER_SUBTILE;
			loc.Normalize();

			var fogStatus = GameSystems.MapFogging.GetFogStatus(loc.location, loc.off_x, loc.off_y);
			if ((fogStatus & 0xB0) == 0 || (fogStatus & 1) == 0) {
				return;
			}
		}

		LocAndOffsets worldPosFull;
		worldPosFull.off_x = animParams.offsetX;
		worldPosFull.off_y = animParams.offsetY;
		worldPosFull.location = worldLoc;

		var radius = obj.GetRadius();
		var renderHeight = obj.GetRenderHeight(true);

		if (!IsObjectOnScreen(worldPosFull, animParams.offsetZ, radius, renderHeight)) {
			return;
		}

		var lightSearchRadius = 0.0f;
		if (!flags.HasFlag(ObjectFlag.DONTLIGHT)) {
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

		if (type != ObjectType.portal) {
			mOccludedMaterial.Resource.Bind(mDevice, lights, overrides);
			mAasRenderer.RenderWithoutMaterial(animatedModel, animParams);

			if (type.IsCritter()) {
				/*
				This renders the equipment in a critter's hand separately, but
				I am not certain *why* exactly. I thought this would have been
				handled by addmeshes, but it might be that there's a distinct
				difference between addmeshes that are skinned onto the mobile's
				skeleton and equipment that is unskinned and just positioned
				in the player's hands.
				*/
				var weaponPrim = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
				if (weaponPrim != null) {
					RenderOccludedObject(weaponPrim);
				}
				var weaponSec = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
				if (weaponSec != null) {
					RenderOccludedObject(weaponSec);
				}
				var shield = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
				if (shield != null) {
					RenderOccludedObject(shield);
				}

			}
		}
		else {
			if (mShowHighlights) {
				overrides.ignoreLighting = true;
			}
			mAasRenderer.Render(animatedModel, animParams, lights, overrides);
		}

	}

	public void RenderObjectHighlight(GameObjectBody obj, ResourceRef<IMdfRenderMaterial> material)
	{

		mTotalLastFrame++;

		var type = obj.type;
		var flags = obj.GetFlags();

		// Dont render destroyed or disabled objects
		const ObjectFlag dontDrawFlags = ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW;
		if ((flags & dontDrawFlags) != 0) {
			return;
		}

		// Hide invisible objects we're supposed to show them
		if (flags.HasFlag(ObjectFlag.INVISIBLE)) {
			return;
		}

		// Dont draw secret doors that haven't been found yet
		var secretDoorFlags = obj.GetSecretDoorFlags();
		if (secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR)) {
			var found = secretDoorFlags.HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND);
			if (!found && type != ObjectType.portal)
				return;
		}

		var animatedModel = obj.GetOrCreateAnimHandle();
		var animParams = obj.GetAnimParams();

		locXY worldLoc;

		GameObjectBody parent = null;
		if (type.IsEquipment()) {
			parent = GameSystems.Item.GetParent(obj);
		}

		var alpha = GetAlpha(obj);

		if (parent != null) {
			var parentAlpha = GetAlpha(parent);
			alpha = (alpha + parentAlpha) / 2;

			worldLoc = parent.GetLocation();
		}
		else {
			worldLoc = obj.GetLocation();
		}

		if (alpha == 0) {
			return;
		}

		// Handle fog occlusion of the world position
		if (type != ObjectType.container
			&& (type == ObjectType.projectile
				|| type.IsCritter()
				|| type.IsEquipment())
			&& (GameSystems.MapFogging.GetFogStatus(worldLoc, animParams.offsetX, animParams.offsetY) & 1) == 0) {
			return;
		}

		LocAndOffsets worldPosFull;
		worldPosFull.off_x = animParams.offsetX;
		worldPosFull.off_y = animParams.offsetY;
		worldPosFull.location = worldLoc;

		var radius = obj.GetRadius();
		var renderHeight = obj.GetRenderHeight(true);

		if (!IsObjectOnScreen(worldPosFull, animParams.offsetZ, radius, renderHeight)) {
			return;
		}

		var lightSearchRadius = 0.0f;
		if (!flags.HasFlag(ObjectFlag.DONTLIGHT)) {
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
		material.Resource.Bind(mDevice, lights, overrides);
		mAasRenderer.RenderWithoutMaterial(animatedModel, animParams);

	}

	public int GetRenderedLastFrame() {
		return mRenderedLastFrame;
	}
	public int GetTotalLastFrame() {
		return mTotalLastFrame;
	}

	public ShadowType GetShadowType() {
		return mShadowType;
	}
	public void SetShadowType(ShadowType type) {
		mShadowType = type;
	}

	public void SetShowHighlight(bool enable) {
		mShowHighlights = enable;
	}
	public bool GetShowHighlights() {
		return mShowHighlights;
	}

	public List<Light3d> FindLights(LocAndOffsets atLocation, float radius) {

		List<Light3d> lights = new List<Light3d>();

		if (GameSystems.Light.IsGlobalLightEnabled) {
			Light3d light = new Light3d();
			var legacyLight = GameSystems.Light.GlobalLight;
			light.type = (Light3dType)legacyLight.type;
			light.color.R = legacyLight.colorR;
			light.color.G = legacyLight.colorG;
			light.color.B = legacyLight.colorB;
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

		if (radius == 0) {
			return lights;
		}

		// Build a box that has twice the radius convert to tiles as it's width/height
		// For some reason, ToEE will add one more INCH_PER_TILE here, which translates to
		// roughly 28 tiles more search radius than is needed
		var boxDimensions = (int)(radius / locXY.INCH_PER_TILE + locXY.INCH_PER_TILE);
		var tileX1 = atLocation.location.locx - 1 - boxDimensions;
		var tileX2 = atLocation.location.locx + 1 + boxDimensions;
		var tileY1 = atLocation.location.locy - 1 - boxDimensions;
		var tileY2 = atLocation.location.locy + 1 + boxDimensions;

		var sectorIterator = new SectorIterator(tileX1, tileX2, tileY1, tileY2);

		var atPos = atLocation.ToInches2D();

		while (sectorIterator.HasNext) {
			using var sector = sectorIterator.Next();

			foreach (ref var light in sector.Lights) {
				int type;
				LinearColor color;
				Vector3 direction;
				float range, phi;
				var lightPos = light.position.ToInches2D();

				if ((light.flags & 0x40) != 0) {
					if (GameSystems.Light.IsNight) {
						type = light.light2.type;
						color = light.light2.color;
						direction = light.light2.direction;
						range = light.range; // Notice how it's using light 1's range
						phi = light.light2.phi;

						/*
						Kill the daytime particle system if it's night and the
						daytime particle system is still alive.
						*/
						if (light.partSys.handle != null) {
							GameSystems.ParticleSys.Remove(light.partSys.handle);
							light.partSys.handle = null;
						}

						/*
						If the nighttime particle system has not yet been started,
						do it here.
						*/
						ref var nightPartSys = ref light.light2.partSys;
						if (nightPartSys.handle == null && nightPartSys.hashCode != 0) {
							var centerOfTile = light.position.ToInches3D(light.offsetZ);
							nightPartSys.handle = GameSystems.ParticleSys.CreateAt(
								nightPartSys.hashCode, centerOfTile
							);
						}
					}
					else {
						type = light.type;
						color = light.color;
						direction = light.direction;
						range = light.range;
						phi = light.phi;

						// This is just the inverse of what we're doing at night (see above)
						ref var nightPartSys = ref light.light2.partSys;
						if (nightPartSys.handle != null) {
							GameSystems.ParticleSys.Remove(nightPartSys.handle);
							nightPartSys.handle = null;
						}

						ref var dayPartSys = ref light.partSys;
						if (dayPartSys.handle == null && dayPartSys.hashCode != 0) {
							var centerOfTile = light.position.ToInches3D(light.offsetZ);
							dayPartSys.handle = GameSystems.ParticleSys.CreateAt(
								dayPartSys.hashCode, centerOfTile
							);
						}
					}
				}
				else {
					type = light.type;
					color = light.color;
					direction = light.direction;
					range = light.range;
					phi = light.phi;
				}

				if (type == 0) {
					continue;
				}

				// Distance (Squared) between pos and light pos
				var acceptableDistance = (int)(radius + light.range);
				var acceptableDistanceSquared = acceptableDistance * acceptableDistance;
				var diffX = (int)(atPos.X - lightPos.X);
				var diffY = (int)(atPos.Y - lightPos.Y);
				var distanceSquared = diffX * diffX + diffY * diffY;
				if (distanceSquared > acceptableDistanceSquared) {
					continue;
				}

				Light3d light3d = new Light3d();
				if (type == 2) {
					light3d.type = Light3dType.Directional;
					light3d.dir = new Vector4(Vector3.Normalize(direction), 0);
				}
				else if (type == 3) {
					light3d.type = Light3dType.Spot;
					light3d.dir = new Vector4(Vector3.Normalize(direction), 0);
				}
				else if (type == 1) {
					light3d.type = Light3dType.Point;
					light3d.dir.X = direction.X;
					light3d.dir.Y = direction.Y;
					light3d.dir.Z = direction.Z;
				}

				// Some vanilla lights are broken
				if (light3d.dir.X == 0.0f && light3d.dir.Y == 0.0f && light3d.dir.Z == 0.0f) {
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
	private bool mShowHighlights = false;

	private int mRenderedLastFrame = 0;
	private int mTotalLastFrame = 0;

	/*
	Same as sin45 incidentally.
	The idea seems to be that the vertical height of the model is scaled
	according to the camera inclination of 45°.
	*/
	private const float cos45 = 0.70709997f;

	private bool IsObjectOnScreen(in LocAndOffsets location, float offsetZ, float radius, float renderHeight)
	{

		var centerOfTile3d = location.ToInches3D();
		var screenPos = mDevice.GetCamera().WorldToScreenUi(centerOfTile3d);

		// This checks if the object's screen bounding box is off screen
		var bbLeft = screenPos.X - radius;
		var bbRight = screenPos.X + radius;
		var bbTop = screenPos.Y - (offsetZ + renderHeight + radius) * cos45;
		var bbBottom = bbTop + (2 * radius + renderHeight) * cos45;

		var screenWidth = mDevice.GetCamera().GetScreenWidth();
		var screenHeight = mDevice.GetCamera().GetScreenHeight();
		if (bbRight < 0 || bbBottom < 0 || bbLeft > screenWidth || bbTop > screenHeight) {
			return false;
		}

		return true;

	}

	private TimePoint _mirrorImagesLastRenderTime = default;
	private	float _mirrorImagesRotation = 0;

	private void RenderMirrorImages(GameObjectBody obj,
		AnimatedModelParams animParams,
		IAnimatedModel model,
		IList<Light3d> lights)
	{
		var mirrorImages = GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Has_Mirror_Image);

		if (mirrorImages == 0) {
			return;
		}

		// The rotation of the mirror images is animated
		if (_mirrorImagesLastRenderTime != default)
		{
			var elapsedSecs = (float) (TimePoint.Now - _mirrorImagesLastRenderTime).TotalSeconds;
			// One full rotation (2PI) in 16 seconds
			_mirrorImagesRotation += elapsedSecs * 2 * MathF.PI / 16.0f;

			// Wrap the rotation around
			while (_mirrorImagesRotation >= 2 * MathF.PI) {
				_mirrorImagesRotation -= 2 * MathF.PI;
			}
		}
		_mirrorImagesLastRenderTime = TimePoint.Now;

		// The images should partially overlap the actual model
		var radius = obj.GetRadius() * 0.75f;

		for (var i = 0; i < mirrorImages; ++i) {
			// Draw one half on the left and the other on the right,
			// if there are an uneven number, the excess image is drawn on the left
			int pos = i + 1;
			if (pos >(int) mirrorImages / 2) {
				pos = pos - mirrorImages - 1;
			}

			// Generate a world matrix that applies the translation
			MdfRenderOverrides overrides = new MdfRenderOverrides();
			overrides.useWorldMatrix = true;
			var xTrans = MathF.Cos(_mirrorImagesRotation) * pos * radius;
			var yTrans = MathF.Sin(_mirrorImagesRotation) * pos * radius;
			overrides.worldMatrix = Matrix4x4.CreateTranslation(xTrans, 0, yTrans);
			overrides.alpha = 0.31f;

			mAasRenderer.Render(model, animParams, lights, overrides);
		}
	}

	private void RenderShadowMapShadow(GameObjectBody obj,
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
		if (secondaryWeapon != null) {
			var secondaryWeaponModel = secondaryWeapon.GetOrCreateAnimHandle();
			var secondaryWeaponParams = secondaryWeapon.GetAnimParams();

			shadowModels.Add(secondaryWeaponModel);
			shadowParams.Add(secondaryWeaponParams);
		}

		mAasRenderer.RenderShadowMapShadow(
			shadowModels,
			shadowParams,
			worldPos,
			radius,
			height,
			globalLight.dir,
			alpha / 255.0f,
			Globals.Config.softShadows
		);

	}
	private void RenderBlobShadow(GameObjectBody handle,
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
		shapeRenderer3d.DrawQuad(corners, mBlobShadowMaterial.Resource, color);
	}

	private int GetAlpha(GameObjectBody obj) => obj.GetInt32(obj_f.transparency);

}

public static class GameObjectRenderExtensions
{
	
	private static readonly ILogger Logger = new ConsoleLogger();

	public static float GetRadius(this GameObjectBody obj)
	{

		var radiusSet = obj.GetFlags().HasFlag(ObjectFlag.RADIUS_SET);
		GameObjectBody protoHandle;
		float protoRadius;
		if (radiusSet){
			var currentRadius = obj.GetFloat(obj_f.radius);

			if ( currentRadius < 2000.0 && currentRadius > 0)
			{
				protoHandle = obj.GetProtoObj();
				if (protoHandle != null){
					protoRadius = protoHandle.GetFloat( obj_f.radius);
					if (protoRadius > 0.0)
					{
						currentRadius = protoRadius;
						obj.SetFloat(obj_f.radius, protoRadius);
					}
				}

			}
			if (currentRadius <= 600 && currentRadius > 0)
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
			protoHandle = obj.GetProtoObj();
			protoRadius = protoHandle.GetFloat(obj_f.radius);
			if (protoRadius > 0.0) {
				Logger.Debug("Returning radius from Proto: {0}", protoRadius);
				return protoRadius;
			}
			Logger.Debug("Returning default (10.0)");
			return 10.0f;
		}

		var radius = obj.GetFloat(obj_f.radius);
		radiusSet = obj.GetFlags().HasFlag(ObjectFlag.RADIUS_SET); // might be changed I guess
		if (!radiusSet || MathF.Abs(radius) > 2000){
			Logger.Debug("GetRadius: Calculating from AAS model. Initially was {0}", radius);

			UpdateRadius(obj, model);

			radius = obj.GetFloat(obj_f.radius);

			if (radius > 2000.0)
			{
				Logger.Warn("GetRadius: Huge radius calculated from AAS {0}", radius);
				radius = 2000.0f;
			} else if (radius <=0)
			{
				Logger.Warn("GetRadius: Negative radius calculated from AAS: {0}. Changing to default (10.0)", radius);
				radius = 10.0f;
			}

			obj.SetFloat(obj_f.radius, radius);
			obj.SetFlag(ObjectFlag.RADIUS_SET, true);
		}
		return radius;
	}

	public static float GetRenderHeight(this GameObjectBody obj, bool updateIfNeeded = false)
	{
		var renderHeight = obj.GetFloat(obj_f.render_height_3d);
		
		// Take render height from the animation if necessary
		if (renderHeight < 0 && updateIfNeeded) {
			UpdateRenderHeight(obj, GetOrCreateAnimHandle(obj));
			renderHeight = obj.GetRenderHeight();
		}

		return renderHeight;
	}

	[TempleDllLocation(0x10021a40)]
        public static IAnimatedModel GetOrCreateAnimHandle(this GameObjectBody obj)
        {
            // I think this belongs to the map_obj subsystem
            if (obj == null) {
                return null;
            }

            // An animation handle was already created
            var animHandle = new AasHandle(obj.GetUInt32(obj_f.animation_handle));
            if (animHandle) {
                return GameSystems.AAS.ModelFactory.BorrowByHandle(animHandle.Handle);
            }

            // Which object to retrieve the model properties from (this indirection
            // is used for polymorph)
            var modelSrcObj = obj;

            // If the obj is polymorphed, use the polymorph proto instead
            int polyProto = GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Polymorphed);
            if (polyProto != 0) {
                modelSrcObj = GameSystems.Object.GetProto(polyProto);
            }

            var meshId = modelSrcObj.GetInt32(obj_f.base_mesh);
            var skeletonId = modelSrcObj.GetInt32(obj_f.base_anim);
            var idleAnimId = obj.GetIdleAnim();
            var animParams = obj.GetAnimParams();

            // Create the model from the meshes.mes IDs and store the result in the obj
            var model = GameSystems.AAS.ModelFactory.FromIds(
                meshId,
                skeletonId,
                idleAnimId,
                animParams,
                true // Borrowed since we store the handle in the obj
            );
            obj.SetUInt32(obj_f.animation_handle, model.GetHandle());

            if (obj.IsCritter()) {
                GameSystems.Critter.UpdateModelEquipment(obj);
            }

            if (obj.type == ObjectType.npc) {
                GameSystems.Critter.AddNpcAddMeshes(obj);
            }

            var flags = obj.GetFlags();
            if (!flags.HasFlag(ObjectFlag.HEIGHT_SET)) {
                UpdateRenderHeight(obj, model);
            }
            if (!flags.HasFlag(ObjectFlag.RADIUS_SET)) {
                UpdateRadius(obj, model);
            }

            return model;
        }


        public static AnimatedModelParams GetAnimParams(this GameObjectBody obj)
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

        public static EncodedAnimId GetIdleAnim(this GameObjectBody obj)
        {
	        var idleAnimObj = obj;

	        // If polymorphed, compute for the polymorph target
	        var polyProtoNum = GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Polymorphed);
	        if (polyProtoNum != 0) {
		        idleAnimObj = GameSystems.Object.GetProto(polyProtoNum);
	        }

	        if (!idleAnimObj.type.IsCritter()) {
		        if (idleAnimObj.type == ObjectType.portal && ObjectHandles.IsDoorOpen(obj)) {
			        return new EncodedAnimId(78);
		        }
		        return new EncodedAnimId(35);
	        }

	        if (GameSystems.Critter.IsDeadNullDestroyed(obj)) {
		        return new EncodedAnimId(12);
	        }
	        else if (GameSystems.Critter.IsDeadOrUnconscious(obj))
	        {
		        return new EncodedAnimId(14);
	        }
	        else if (GameSystems.Critter.IsProne(obj))
	        {
		        return new EncodedAnimId(1);
	        }
	        else if (GameSystems.Critter.IsConcealed(obj))
	        {
		        return new EncodedAnimId(33);
	        }
	        else if (GameSystems.Critter.IsMovingSilently(obj))
	        {
		        return new EncodedAnimId(44);
	        }
	        else if (GameSystems.Critter.IsCombatModeActive(obj))
	        {
		        return GameSystems.Critter.GetAnimId(idleAnimObj, WeaponAnim.CombatIdle);
	        }
	        else
	        {
		        return GameSystems.Critter.GetAnimId(idleAnimObj, WeaponAnim.Idle);
	        }
        }

        private static void UpdateRadius(GameObjectBody obj, IAnimatedModel model)
        {
	        var scale = GetScalePercent(obj);
	        var radius = model.GetRadius(scale);

	        if (radius > 0) {
		        obj.SetFloat(obj_f.radius, radius);
		        obj.SetFlag( ObjectFlag.RADIUS_SET, true);
	        }
        }

        private static void UpdateRenderHeight(GameObjectBody obj, IAnimatedModel model)
        {
	        var scale = GetScalePercent(obj);
	        var height = model.GetHeight(scale);

	        obj.SetFloat(obj_f.render_height_3d, height);
	        obj.SetFlag( ObjectFlag.HEIGHT_SET, true);
        }

        public static int GetScalePercent(this GameObjectBody obj)
        {
	        var modelScale = obj.GetInt32(obj_f.model_scale);

	        if (obj.IsCritter()) {

		        DispIoMoveSpeed dispIo = DispIoMoveSpeed.Default;
		        dispIo.bonlist.AddBonus(modelScale, 1, 102); // initial value

		        var dispatcher = obj.GetDispatcher();
		        dispatcher?.Process(DispatcherType.GetModelScale, D20DispatcherKey.NONE, dispIo);

		        modelScale = dispIo.bonlist.OverallBonus;
	        }

	        return modelScale;
        }

        public static void SetAnimId(this GameObjectBody obj, EncodedAnimId animId)
        {
	        // Propagate animations to main/off hand equipment for critters
	        if (obj.IsCritter()) {
		        var mainHand = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponPrimary);
		        var offHand = GameSystems.Critter.GetWornItem(obj, EquipSlot.WeaponSecondary);
		        if (offHand == null) {
			        offHand = GameSystems.Critter.GetWornItem(obj, EquipSlot.Shield);
		        }

		        // Apparently certain anim IDs cause weapons to disappear,
		        // possibly skill use/casting?
		        int opacity = 0;
		        if (animId.IsSpecialAnim()) {
			        opacity = 255;
		        }

		        if (mainHand != null) {
			        GameSystems.ObjFade.FadeTo(mainHand, opacity, 10, 16, 0);
			        SetAnimId(mainHand, animId);
		        }
		        if (offHand != null) {
			        GameSystems.ObjFade.FadeTo(offHand, opacity, 10, 16, 0);
			        SetAnimId(offHand, animId);
		        }
	        }

	        var model = obj.GetOrCreateAnimHandle();
	        model.SetAnimId(animId);
        }

        [TempleDllLocation(0x100246f0)]
        public static void AdvanceAnimationTime(this GameObjectBody obj, float deltaTimeInSecs)
        {
	        var model = obj.GetOrCreateAnimHandle();
	        var animParams = obj.GetAnimParams();
	        model.Advance(deltaTimeInSecs, 0.0f, 0.0f, in animParams);
        }

        // sectorChanged was previously an int but only was ever 1 = location changed, 2 = sector changed
        [TempleDllLocation(0x10025050)]
        public static void UpdateRenderingState(this GameObjectBody obj, bool sectorChanged)
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

        private static void HandleEnterNewSector(SectorLoc loc, GameObjectBody obj)
        {
	        {
		        using var sectorLock = new LockedMapSector(loc);
		        var sector = sectorLock.Sector;
		        var lightScheme = sector.lightScheme;
		        if ( lightScheme == 0 )
			        lightScheme = GameSystems.LightScheme.GetDefaultScheme();
		        if ( lightScheme != GameSystems.LightScheme.GetCurrentScheme() )
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

	        var followers = GameSystems.Critter.GetFollowers(obj);
	        foreach (var follower in followers)
	        {
		        GameSystems.Script.ExecuteObjectScript(obj, follower, 0, 0,
			        ObjScriptEvent.NewSector, 0);
	        }
        }

        [TempleDllLocation(0x10021290)]
        public static void DestroyRendering(this GameObjectBody obj)
        {
	        GameSystems.Light.RemoveAttachedTo(obj);

	        var renderPalette = obj.GetInt32(obj_f.render_palette);
	        if ( renderPalette != 0 )
	        {
		        // I think most of this is 2D rendering which is unused
		        throw new NotImplementedException();
		        // TODO sub_101EBD40(v1);
		        obj.SetInt32(obj_f.render_palette, 0);
	        }

	        var renderColors = obj.GetUInt32(obj_f.render_colors);
	        if ( renderColors != 0 )
	        {
		        throw new NotImplementedException();

		        // TODO I think most of this is 2D rendering which is unused
		        // TODO v3 = *(_DWORD *)(renderColors - 4);
		        // TODO *(_DWORD *)(v3 + 4) = dword_107880A0;
		        // TODO dword_107880A0 = (void *)v3;

		        obj.SetInt32( obj_f.render_colors, 0);
	        }

	        var renderFlags = obj.GetUInt32( obj_f.render_flags);
	        // Clear flags 76000000
	        obj.SetUInt32(obj_f.render_flags, renderFlags & 0x89FFFFFF);

	        var animHandle = new AasHandle(obj.GetUInt32(obj_f.animation_handle));
	        if (animHandle) {
		        GameSystems.AAS.ModelFactory.FreeHandle(animHandle.Handle);
		        obj.SetUInt32(obj_f.animation_handle, 0);
	        }

	        GameSystems.ParticleSys.InvalidateObject(obj);
        }

	}

}