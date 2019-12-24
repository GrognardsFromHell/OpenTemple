using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.InGame;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Raycast
{
    public class RaycastSystem : IDisposable
    {
        private readonly List<GoalDestination> _goalDestinations = new List<GoalDestination>();

        public IReadOnlyList<GoalDestination> GoalDestinations => _goalDestinations;

        [TempleDllLocation(0x100bac20)]
        public void GoalDestinationsAdd(GameObjectBody obj, LocAndOffsets loc)
        {
            _goalDestinations.Add(new GoalDestination(obj, loc));
        }

        [TempleDllLocation(0x100bac80)]
        public void GoalDestinationsRemove(GameObjectBody obj)
        {
            _goalDestinations.RemoveAll(gd => gd.obj == obj);
        }

        [TempleDllLocation(0x100bacc0)]
        public void GoalDestinationsClear()
        {
            _goalDestinations.Clear();
        }

        public struct GoalDestination
        {
            public readonly GameObjectBody obj;

            public readonly LocAndOffsets loc;

            public GoalDestination(GameObjectBody obj, LocAndOffsets loc)
            {
                this.obj = obj;
                this.loc = loc;
            }
        }

        public void Dispose()
        {
        }


        [TempleDllLocation(0x100222E0)]
		private static bool IsInSelectionCircle(GameObjectBody handle, Vector3 pos)
		{
			var location = handle.GetLocationFull();
			var center = location.ToInches3D();
			var radiusSquared = handle.GetRadius();
			radiusSquared *= radiusSquared;

			var distFromCenter = (pos - center).LengthSquared();

			return distFromCenter <= radiusSquared;
		}

		[TempleDllLocation(0x10022210)]
		static bool IsPreferredSelectionObject(GameObjectBody selCurrent, GameObjectBody selNew)
		{
			if (selCurrent == null || selNew == null)
			{
				return true;
			}

			var currentRadius = selCurrent.GetRadius();
			var newRadius = selNew.GetRadius();

			// Smaller radius is a higher priority selection
			if (newRadius - 0.1f > currentRadius) {
				return false;
			}

			// If both are critters, prefer the one that is alive.
			if (selCurrent.IsCritter() && selNew.IsCritter()
				&& GameSystems.Critter.IsDeadOrUnconscious(selNew)
				&& !GameSystems.Critter.IsDeadOrUnconscious(selCurrent)) {
				return false;
			}
			return true;
		}

		[TempleDllLocation(0x10022360)]
		public bool PickObjectOnScreen(int x, int y, out GameObjectBody pickedHandle, GameRaycastFlags flags)
		{
			if (flags == 0) {
				flags = GameRaycastFlags.HITTEST_3D;
			}

			var camera = Tig.RenderingDevice.GetCamera();
			var worldCoord = camera.ScreenToWorld(x, y);

			var hitTest3d = (flags & GameRaycastFlags.HITTEST_3D) != 0;
			Ray3d ray = default;

			GameObjectBody selectedByCircle = null;
			GameObjectBody selectedByCylinder = null;
			float closestCylinderHit = float.MaxValue;
			GameObjectBody selectedByMesh = null;
			float closestMeshHit = float.MaxValue;

			if (hitTest3d) {
				ray = Tig.RenderingDevice.GetCamera().GetPickRay(x, y);
			}

			var worldLoc = LocAndOffsets.FromInches(worldCoord);

			// Flags for the ObjectHandles.to be considered
			var objFilter = ObjectListFilter.OLC_ALL;
			if (flags.HasFlag(GameRaycastFlags.ExcludeScenery))
				objFilter &= ~ObjectListFilter.OLC_SCENERY;
			if (flags.HasFlag(GameRaycastFlags.ExcludeItems))
				objFilter &= ~ObjectListFilter.OLC_ITEMS;
			if (flags.HasFlag(GameRaycastFlags.ExcludePortals))
				objFilter &= ~ObjectListFilter.OLC_PORTAL;
			if (flags.HasFlag(GameRaycastFlags.ExcludeContainers))
				objFilter &= ~ObjectListFilter.OLC_CONTAINER;
			if (flags.HasFlag(GameRaycastFlags.ExcludeCritters))
				objFilter &= ~ObjectListFilter.OLC_CRITTERS;

			// Search radius of 1000 is large...
			// Previously it was looking for a tile range which seemed to be calculated incorrectly
			using var objList = ObjList.ListRadius(worldLoc, 1000, objFilter);

			foreach (var obj in objList)
			{
				var location = obj.GetLocationFull();

				if (GameSystems.MapObject.IsUntargetable(obj)) {
					continue;
				}

				var type = obj.type;
				if (!type.IsStatic() && (GameSystems.MapFogging.GetFogStatus(location) & 1) == 0) {
					continue;
				}

				if (type.IsCritter()) {
					if (flags.HasFlag(GameRaycastFlags.ExcludeCritters)) {
						continue;
					}
					if (flags.HasFlag(GameRaycastFlags.ExcludeDead) && GameSystems.Critter.IsDeadNullDestroyed(obj)) {
						continue;
					}
					if (flags.HasFlag(GameRaycastFlags.ExcludeUnconscious) && GameSystems.Critter.IsDeadOrUnconscious(obj)) {
						continue;
					}
				}

				if (hitTest3d) {

					// This seems to be somewhat fishy since the hit-testing doesn't
					// take this into account elsewhere...
					var depth = GameSystems.Height.GetDepth(location);
					var effectiveZ = obj.OffsetZ - depth;

					var cylinder = new Cylinder3d();
					cylinder.baseCenter = location.ToInches3D(effectiveZ);
					cylinder.radius = obj.GetRadius();
					cylinder.height = obj.GetRenderHeight();

					// Interesting. We're doubling the selection circle for certain ObjectHandles.
					// This is probably supposed to make clicking on corpses easier
					bool extendedSelCylinder = type == ObjectType.portal || type.IsCritter() && GameSystems.Critter.IsDeadOrUnconscious(obj);
					if (extendedSelCylinder) {
						cylinder.radius *= 2;
					}

					// Always intersect with the selection cylinder of the mesh, even if the actual selection will still
					// require a mesh hittest
					if (!cylinder.HitTest(ray, out var cylinderHitDist)) {
						continue; // Didn't hit the cylinder
					}

					// The ObjectHandles.that used an extended selection cylinder will only be selected based on a precise mesh hittest
					if ((flags.HasFlag(GameRaycastFlags.HITTEST_CYLINDER)) && !extendedSelCylinder) {
						if (cylinderHitDist < closestCylinderHit) {
							closestCylinderHit = cylinderHitDist;
							selectedByCylinder = obj;
						}
					}

					if (flags.HasFlag(GameRaycastFlags.HITTEST_MESH)) {
						var anim = obj.GetOrCreateAnimHandle();
						if (anim != null) {
							var animParams = obj.GetAnimParams();
							if (anim.HitTestRay(animParams, ray, out var dist) && dist < closestMeshHit) {
								closestMeshHit = dist;
								selectedByMesh = obj;
							}

						}
					}

				} else if (flags.HasFlag(GameRaycastFlags.HITTEST_SEL_CIRCLE)) {

					// Is the mouse pointer within the circle
					if (IsInSelectionCircle(obj, worldCoord)) {

						// If an object is already selected, decide which has higher priority (if circles overlap)
						if (IsPreferredSelectionObject(selectedByCircle, obj)) {
							selectedByCircle = obj;
						}
					}

				}
			}

			// Return what was selected based on the priority mesh>cylinder>circle (which is by precision)
			// but only consider options that were enabled
			if (flags.HasFlag(GameRaycastFlags.HITTEST_MESH) && selectedByMesh != null) {
				pickedHandle = selectedByMesh;
				return true;
			} else if (flags.HasFlag(GameRaycastFlags.HITTEST_CYLINDER) && selectedByCylinder != null) {
				pickedHandle = selectedByCylinder;
				return true;
			} else if (flags.HasFlag(GameRaycastFlags.HITTEST_SEL_CIRCLE) && selectedByCircle != null) {
				pickedHandle = selectedByCircle;
				return true;
			} else {
				pickedHandle = null;
				return false;
			}

		}

    }

    // these flags are generated based on picker specs and used inside the raycast function in 10022360
    [Flags]
    public enum GameRaycastFlags
    {
	    HITTEST_SEL_CIRCLE = 1,
	    HITTEST_CYLINDER = 2, // this is set as a default
	    HITTEST_MESH = 4, // this is set as a default - looks like "Get radius from Aas"
	    HITTEST_3D = 6,
	    ExcludeScenery = 8,
	    ExcludeItems = 16,
	    ExcludePortals = 32,
	    ExcludeContainers = 64,
	    ExcludeCritters = 0x80,
	    ExcludeDead = 0x100,
	    ExcludeUnconscious = 0x200
    }

}