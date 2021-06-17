using System.Numerics;
using OpenTemple.Core.Ui;
using OpenTemple.Particles;

namespace OpenTemple.Core.Particles
{
/*
Interface for external functionality required by the particle systems.
*/
    public interface IPartSysExternal
    {
        /// <summary>
        /// Get the particle system fidelity setting ranging from 0 to 1.
        /// </summary>
        float GetParticleFidelity();

        /// <summary>
        /// Retrieves the current position for the given object in the world.
        /// Returns true on success.
        /// </summary>
        bool GetObjLocation(object obj, out Vector3 worldPos);

        /// <summary>
        /// Retrieves the current rotation in radians for the given object in the world.
        /// Returns true on success.
        /// </summary>
        bool GetObjRotation(object obj, out float rotation);

        /// <summary>
        /// Returns the radius of the given object.
        /// </summary>
        float GetObjRadius(object obj);

        /// <summary>
        /// Retrieves the world transformation matrix of a given bone for the given
        /// object's
        /// skeleton. This matrix transforms points from the bone space into world space.
        /// </summary>
        bool GetBoneWorldMatrix(object obj, string boneName, out Matrix4x4 boneMatrix);

        /// <summary>
        /// Returns the number of bones that the skeleton for the given object has.
        /// </summary>
        int GetBoneCount(object obj);

        /// Gets the position of a bone and it's parent bone.
        /// Returns the bone idx of the parent or -1 if the bone is not a child bone or is
        /// a special bone.
        ///
        /// The following bones are ignored by this method:
        /// Pony
        /// Footstep
        /// Origin
        /// Casting_ref
        /// EarthElemental_reg
        /// Casting_ref
        /// origin
        /// Bip01
        /// Bip01 Footsteps
        /// FootL_ref
        /// FootR_ref
        /// Head_ref
        /// HandL_ref
        /// HandR_ref
        /// Chest_ref
        /// groundParticleRef
        /// effects_ref
        /// trap_ref
        /// And all bones starting with #, which are cloth simulation related.
        int GetParentChildBonePos(object obj, int boneIdx, out Vector3 parentPos, out Vector3 childPos);

        /// <summary>
        /// Gets the position of the given bone in the skeleton of the object.
        /// true if the bone position was retrieved
        /// </summary>
        bool GetBonePos(object obj, int boneIdx, out Vector3 pos);

        /// <summary>
        /// Checks if the given box (in screen space) is visible or not, given that
        /// the box is centered on the world position.
        /// </summary>
        bool IsBoxVisible(IGameViewport viewport, Vector3 worldPos, Box2d box);
    }
}