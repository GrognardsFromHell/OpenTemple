using System;
using System.Numerics;
using OpenTemple.Core.Particles.Spec;
using OpenTemple.Core.Utils;
using OpenTemple.Particles;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Instances;

internal static class PartSysSimulation
{
    /// Converts from polar coordinates to cartesian coordinates.
    /// See Wikipedia for details:
    /// https://en.wikipedia.org/wiki/Spherical_coordinate_system
    private static Vector3 SphericalDegToCartesian(float azimuth, float inclination, float radius)
    {
        azimuth = Angles.ToRadians(azimuth);
        inclination = Angles.ToRadians(inclination);

        var tmp = MathF.Cos(inclination) * radius;
        return new Vector3(
            tmp * MathF.Sin(azimuth),
            MathF.Sin(inclination) * radius,
            tmp * MathF.Cos(azimuth)
        );
    }

    private static void Rotate2D(float rotation, ref float x, ref float z)
    {
        // Rotate the velocity vector according to the current object rotation in the world
        var newX = -(MathF.Cos(rotation) * x) - MathF.Sin(rotation) * z;
        var newZ = MathF.Sin(rotation) * x - MathF.Cos(rotation) * z;
        x = newX;
        z = newZ;
    }

    private static void RotateAndMove(float dX, float dY, float dZ, float rotation, ref float x, ref float y,
        ref float z)
    {
        if (rotation != 0)
        {
            Rotate2D(rotation, ref x, ref z);
        }

        x += dX;
        y += dY;
        z += dZ;
    }

    private static float GetParticleValue(PartSysEmitter emitter, int particleIdx, PartSysParamId paramId,
        float atLifetime, float defaultValue = 0.0f)
    {
        var state = emitter.GetParamState(paramId);
        return state?.GetValue(emitter, particleIdx, atLifetime) ?? defaultValue;
    }

    private static void SetParticleParam(PartSysEmitter emitter, int particleIdx, PartSysParamId id,
        float atLifetime, ParticleStateField stateField, float defaultValue)
    {
        var value = GetParticleValue(emitter, particleIdx, id, atLifetime, defaultValue);
        emitter.GetParticleState().SetState(stateField, particleIdx, value);
    }

    public static void SimulateParticleAging(PartSysEmitter emitter, float timeToSimulateSec)
    {
        var it = emitter.NewIterator();
        var ages = emitter.GetParticles();

        while (it.HasNext())
        {
            var particleIdx = it.Next();
            ages[particleIdx] += timeToSimulateSec;
        }

        emitter.PruneExpiredParticles();
    }

    public static void SimulateParticleSpawn(IPartSysExternal external, PartSysEmitter emitter, int particleIdx,
        float timeToSimulate)
    {
        var spec = emitter.GetSpec();
        var partSpawnTime = emitter.GetParticleSpawnTime(particleIdx);

        var worldPosVar = emitter.GetWorldPosVar();
        var particleX = worldPosVar.X;
        var particleY = worldPosVar.Y;
        var particleZ = worldPosVar.Z;

        var emitOffsetX = GetParticleValue(emitter, particleIdx, PartSysParamId.emit_offset_X, partSpawnTime);
        var emitOffsetY = GetParticleValue(emitter, particleIdx, PartSysParamId.emit_offset_Y, partSpawnTime);
        var emitOffsetZ = GetParticleValue(emitter, particleIdx, PartSysParamId.emit_offset_Z, partSpawnTime);

        if (spec.GetOffsetCoordSys() == PartSysCoordSys.Polar)
        {
            var coords = SphericalDegToCartesian(emitOffsetX, emitOffsetY, emitOffsetZ);
            particleX += coords.X;
            particleY += coords.Y;
            particleZ += coords.Z;
        }
        else
        {
            particleX += emitOffsetX;
            particleY += emitOffsetY;
            particleZ += emitOffsetZ;
        }

        switch (spec.GetSpace())
        {
            case PartSysEmitterSpace.ObjectPos:
            case PartSysEmitterSpace.ObjectYpr:
            {
                if (spec.GetParticleSpace() != PartSysParticleSpace.SameAsEmitter)
                {
                    // TODO: Figure out this formula...
                    var scale = 1.0f - emitter.GetParticleAge(particleIdx) / timeToSimulate;
                    var prevObjPos = emitter.GetPrevObjPos();
                    var objPos = emitter.GetObjPos();
                    var dX = prevObjPos.X + (objPos.X - prevObjPos.X) * scale;
                    var dY = prevObjPos.Y + (objPos.Y - prevObjPos.Y) * scale;
                    var dZ = prevObjPos.Z + (objPos.Z - prevObjPos.Z) * scale;
                    var rotation = 0.0f;
                    if (spec.GetSpace() == PartSysEmitterSpace.ObjectYpr)
                    {
                        rotation = emitter.GetPrevObjRotation() +
                                   (emitter.GetObjRotation() - emitter.GetPrevObjRotation()) * scale;
                    }

                    RotateAndMove(dX, dY, dZ, rotation, ref particleX, ref particleY, ref particleZ);
                }
            }
                break;

            case PartSysEmitterSpace.Bones:
            {
                var scale = 1.0f - emitter.GetParticleAge(particleIdx) / timeToSimulate;
                if (emitter.GetBoneState() == null)
                {
                    break;
                }

                if (emitter.GetBoneState().GetRandomPos(scale, out var bonePos))
                {
                    particleX = bonePos.X;
                    particleY = bonePos.Y;
                    particleZ = bonePos.Z;
                }
            }
                break;

            case PartSysEmitterSpace.NodePos:
            case PartSysEmitterSpace.NodeYpr:
            {
                if (spec.GetParticleSpace() != PartSysParticleSpace.SameAsEmitter)
                {
                    var scale = 1.0f - emitter.GetParticleAge(particleIdx) / timeToSimulate;
                    if (spec.GetSpace() == PartSysEmitterSpace.NodeYpr)
                    {
                        if (!external.GetBoneWorldMatrix(emitter.GetAttachedTo(), spec.GetNodeName(),
                                out var boneM))
                        {
                            boneM = Matrix4x4.Identity;
                        }

                        var ppos = new Vector3(particleX, particleY, particleZ);
                        var newpos = Vector3.Transform(ppos, boneM);
                        particleX = newpos.X;
                        particleY = newpos.Y;
                        particleZ = newpos.Z;
                    }
                    else
                    {
                        var prevObjPos = emitter.GetPrevObjPos();
                        var objPos = emitter.GetObjPos();
                        var dX = prevObjPos.X + (objPos.X - prevObjPos.X) * scale;
                        var dY = prevObjPos.Y + (objPos.Y - prevObjPos.Y) * scale;
                        var dZ = prevObjPos.Z + (objPos.Z - prevObjPos.Z) * scale;
                        RotateAndMove(dX, dY, dZ, 0.0f, ref particleX, ref particleY, ref particleZ);
                    }
                }
            }
                break;

            default:
                break;
        }

        var state = emitter.GetParticleState();
        state.SetState(ParticleStateField.PSF_X, particleIdx, particleX);
        state.SetState(ParticleStateField.PSF_Y, particleIdx, particleY);
        state.SetState(ParticleStateField.PSF_Z, particleIdx, particleZ);

        // Initialize particle color
        SetParticleParam(emitter, particleIdx, PartSysParamId.emit_init_red, partSpawnTime,
            ParticleStateField.PSF_RED, 255.0f);
        SetParticleParam(emitter, particleIdx, PartSysParamId.emit_init_green, partSpawnTime,
            ParticleStateField.PSF_GREEN, 255.0f);
        SetParticleParam(emitter, particleIdx, PartSysParamId.emit_init_blue, partSpawnTime,
            ParticleStateField.PSF_BLUE, 255.0f);
        SetParticleParam(emitter, particleIdx, PartSysParamId.emit_init_alpha, partSpawnTime,
            ParticleStateField.PSF_ALPHA, 255.0f);

        // Initialize particle velocity
        var partVelX = GetParticleValue(emitter, particleIdx, PartSysParamId.emit_init_vel_X, partSpawnTime);
        var partVelY = GetParticleValue(emitter, particleIdx, PartSysParamId.emit_init_vel_Y, partSpawnTime);
        var partVelZ = GetParticleValue(emitter, particleIdx, PartSysParamId.emit_init_vel_Z, partSpawnTime);

        if (spec.GetSpace() == PartSysEmitterSpace.ObjectYpr)
        {
            if (spec.GetParticleSpace() != PartSysParticleSpace.SameAsEmitter)
            {
                var scale = 1.0f - emitter.GetParticleAge(particleIdx) / timeToSimulate;

                var rotation = 0.0f;
                if (spec.GetSpace() == PartSysEmitterSpace.ObjectYpr)
                    rotation = (emitter.GetObjRotation() - emitter.GetPrevObjRotation()) * scale +
                               emitter.GetPrevObjRotation();

                // Rotate the velocity vector according to the current object rotation in the world
                // TODO: Even for rotation == 0, this will flip the velocity vector
                Rotate2D(rotation, ref partVelX, ref partVelZ);
            }
        }
        else if (spec.GetSpace() == PartSysEmitterSpace.NodeYpr)
        {
            if (spec.GetParticleSpace() != PartSysParticleSpace.SameAsEmitter)
            {
                var objId = emitter.GetAttachedTo();

                if (!external.GetBoneWorldMatrix(objId, spec.GetNodeName(), out var boneMatrix))
                {
                    boneMatrix = Matrix4x4.Identity;
                }

                // Construct a directional vector (not a positional one, w=0 here) for the velocity
                var dirVec = new Vector3(partVelX, partVelY, partVelZ);
                dirVec = Vector3.TransformNormal(dirVec, boneMatrix);

                partVelX = dirVec.X;
                partVelY = dirVec.Y;
                partVelZ = dirVec.Z;
            }
        }

        // Are particle coordinates defined as polar coordinates? Convert them to cartesian here
        if (spec.GetParticleVelocityCoordSys() == PartSysCoordSys.Polar)
        {
            var cartesianVel = SphericalDegToCartesian(partVelX, partVelY, partVelZ);
            partVelX = cartesianVel.X;
            partVelY = cartesianVel.Y;
            partVelZ = cartesianVel.Z;
        }

        state.SetState(ParticleStateField.PSF_VEL_X, particleIdx, partVelX);
        state.SetState(ParticleStateField.PSF_VEL_Y, particleIdx, partVelY);
        state.SetState(ParticleStateField.PSF_VEL_Z, particleIdx, partVelZ);

        // I don't know why it's taken at lifetime 0.
        // TODO: Figure out if this actually *never* changes?
        var posVarX = GetParticleValue(emitter, particleIdx, PartSysParamId.part_posVariation_X, 0);
        var posVarY = GetParticleValue(emitter, particleIdx, PartSysParamId.part_posVariation_Y, 0);
        var posVarZ = GetParticleValue(emitter, particleIdx, PartSysParamId.part_posVariation_Z, 0);

        // For a polar system, convert these positions to cartesian to apply them to the
        // rendering position
        if (spec.GetParticlePosCoordSys() == PartSysCoordSys.Polar)
        {
            state.SetState(ParticleStateField.PSF_POS_AZIMUTH, particleIdx, 0);
            state.SetState(ParticleStateField.PSF_POS_INCLINATION, particleIdx, 0);
            state.SetState(ParticleStateField.PSF_POS_RADIUS, particleIdx, 0);

            // Convert to cartesian and add to the actual current particle position
            // As usual, x, y, z here are (azimuth, inclination, radius)
            var cartesianPos = SphericalDegToCartesian(posVarX, posVarY, posVarZ);
            posVarX = cartesianPos.X;
            posVarY = cartesianPos.Y;
            posVarZ = cartesianPos.Z;
        }

        // Apply the position variation to the initial position and store it
        posVarX += particleX;
        posVarY += particleY;
        posVarZ += particleZ;
        state.SetState(ParticleStateField.PSF_POS_VAR_X, particleIdx, posVarX);
        state.SetState(ParticleStateField.PSF_POS_VAR_Y, particleIdx, posVarY);
        state.SetState(ParticleStateField.PSF_POS_VAR_Z, particleIdx, posVarZ);

        /*
        The following code will apply particle movement after
        spawning a particle retroactively. This should only happen
        for high frequency particle systems that spawn multiple particles
        per frame.
        Also note how particle age instead of particle lifetime is used here
        to access parameters of the emitter.
        */
        var partAge = emitter.GetParticleAge(particleIdx);
        if (partAge != 0)
        {
            var partAgeSquared = partAge * partAge;

            particleX += partVelX * partAge;
            particleY += partVelY * partAge;
            particleZ += partVelZ * partAge;

            var param = emitter.GetParamState(PartSysParamId.part_accel_X);
            if (param != null)
            {
                var accelX = param.GetValue(emitter, particleIdx, partAge);
                particleX += accelX * partAgeSquared * 0.5f;
                partVelX += accelX * partAge;
            }

            param = emitter.GetParamState(PartSysParamId.part_accel_Y);
            if (param != null)
            {
                var accelY = param.GetValue(emitter, particleIdx, partAge);
                particleY += accelY * partAgeSquared * 0.5f;
                partVelY += accelY * partAge;
            }

            param = emitter.GetParamState(PartSysParamId.part_accel_Z);
            if (param != null)
            {
                var accelZ = param.GetValue(emitter, particleIdx, partAge);
                particleZ += accelZ * partAgeSquared * 0.5f;
                partVelZ += accelZ * partAge;
            }

            state.SetState(ParticleStateField.PSF_VEL_X, particleIdx, partVelX);
            state.SetState(ParticleStateField.PSF_VEL_Y, particleIdx, partVelY);
            state.SetState(ParticleStateField.PSF_VEL_Z, particleIdx, partVelZ);

            param = emitter.GetParamState(PartSysParamId.part_velVariation_X);
            if (param != null)
            {
                particleX += param.GetValue(emitter, particleIdx, partAge) * partAge;
            }

            param = emitter.GetParamState(PartSysParamId.part_velVariation_Y);
            if (param != null)
            {
                particleY += param.GetValue(emitter, particleIdx, partAge) * partAge;
            }

            param = emitter.GetParamState(PartSysParamId.part_velVariation_Z);
            if (param != null)
            {
                particleZ += param.GetValue(emitter, particleIdx, partAge) * partAge;
            }

            state.SetState(ParticleStateField.PSF_X, particleIdx, particleX);
            state.SetState(ParticleStateField.PSF_Y, particleIdx, particleY);
            state.SetState(ParticleStateField.PSF_Z, particleIdx, particleZ);
        }

        // Simulate rotation for anything other than a point particle
        if (spec.GetParticleType() != PartSysParticleType.Point)
        {
            var emitterAge = emitter.GetParticleSpawnTime(particleIdx);
            var rotation = emitter.GetParamValue(PartSysParamId.emit_yaw, particleIdx, emitterAge, 0.0f);
            emitter.GetParticleState().SetState(ParticleStateField.PSF_ROTATION, particleIdx, rotation);
        }
    }

// Simplifies access to particle parameter state that is based on a particular particles age
    private struct ParticleValueSource
    {
        private readonly PartSysEmitter emitter;
        private readonly int particleIdx;
        private readonly float particleAge;

        public ParticleValueSource(PartSysEmitter emitter, int particleIdx, float particleAge)
        {
            this.emitter = emitter;
            this.particleIdx = particleIdx;
            this.particleAge = particleAge;
        }

        public bool GetValue(PartSysParamId paramId, out float value)
        {
            var state = emitter.GetParamState(paramId);
            if (state != null)
            {
                value = state.GetValue(emitter, particleIdx, particleAge);
                return true;
            }

            value = float.NaN;
            return false;
        }
    };

    public static void SimulateParticleMovement(PartSysEmitter emitter, float timeToSimulateSecs)
    {
        var spec = emitter.GetSpec();

        // Used as a factor in integrating the acceleration to retroactively calculate
        // its influence on the particle position
        var accelIntegrationFactor = timeToSimulateSecs * timeToSimulateSecs * 0.5f;

        var state = emitter.GetParticleState();
        var it = emitter.NewIterator();
        while (it.HasNext())
        {
            var particleIdx = it.Next();
            var particleAge = emitter.GetParticleAge(particleIdx);

            var valueSource = new ParticleValueSource(emitter, particleIdx, particleAge);

            var x = state.GetState(ParticleStateField.PSF_X, particleIdx);
            var y = state.GetState(ParticleStateField.PSF_Y, particleIdx);
            var z = state.GetState(ParticleStateField.PSF_Z, particleIdx);

            var velX = state.GetState(ParticleStateField.PSF_VEL_X, particleIdx);
            var velY = state.GetState(ParticleStateField.PSF_VEL_Y, particleIdx);
            var velZ = state.GetState(ParticleStateField.PSF_VEL_Z, particleIdx);

            // Calculate new position of particle based on velocity
            x += timeToSimulateSecs * velX;
            y += timeToSimulateSecs * velY;
            z += timeToSimulateSecs * velZ;

            // Apply acceleration to velocity (retroactively to position as well)
            float value;
            if (valueSource.GetValue(PartSysParamId.part_accel_X, out value))
            {
                x += accelIntegrationFactor * value;
                velX += timeToSimulateSecs * value;
            }

            if (valueSource.GetValue(PartSysParamId.part_accel_Y, out value))
            {
                y += accelIntegrationFactor * value;
                velY += timeToSimulateSecs * value;
            }

            if (valueSource.GetValue(PartSysParamId.part_accel_Z, out value))
            {
                z += accelIntegrationFactor * value;
                velZ += timeToSimulateSecs * value;
            }

            /*
                Apply Velocity Var
            */
            if (spec.GetParticleVelocityCoordSys() == PartSysCoordSys.Polar)
            {
                if (spec.GetParticlePosCoordSys() != PartSysCoordSys.Polar)
                {
                    // Velocity is polar, positions are not . convert velocity
                    var azimuth = emitter.GetParamValue(PartSysParamId.part_velVariation_X, particleIdx,
                        particleAge);
                    var inclination = emitter.GetParamValue(PartSysParamId.part_velVariation_Y, particleIdx,
                        particleAge);
                    var radius = emitter.GetParamValue(PartSysParamId.part_velVariation_Z, particleIdx,
                        particleAge);

                    var cartesianVel = SphericalDegToCartesian(azimuth, inclination, radius);
                    x += cartesianVel.X * timeToSimulateSecs;
                    y += cartesianVel.Y * timeToSimulateSecs;
                    z += cartesianVel.Z * timeToSimulateSecs;
                }
                else
                {
                    // Modify the spherical coordinates of the particle directly
                    if (valueSource.GetValue(PartSysParamId.part_velVariation_X, out value))
                    {
                        ref var azimuth = ref state.GetStatePtr(ParticleStateField.PSF_POS_AZIMUTH, particleIdx);
                        azimuth += value * timeToSimulateSecs;
                    }

                    if (valueSource.GetValue(PartSysParamId.part_velVariation_Y, out value))
                    {
                        ref var inclination =
                            ref state.GetStatePtr(ParticleStateField.PSF_POS_INCLINATION, particleIdx);
                        inclination += value * timeToSimulateSecs;
                    }

                    if (valueSource.GetValue(PartSysParamId.part_velVariation_Z, out value))
                    {
                        ref var radius = ref state.GetStatePtr(ParticleStateField.PSF_POS_RADIUS, particleIdx);
                        radius += value * timeToSimulateSecs;
                    }
                }
            }
            else
            {
                // Cartesian velocity seems pretty simple here
                if (valueSource.GetValue(PartSysParamId.part_velVariation_X, out value))
                {
                    x += value * timeToSimulateSecs;
                }

                if (valueSource.GetValue(PartSysParamId.part_velVariation_Y, out value))
                {
                    y += value * timeToSimulateSecs;
                }

                if (valueSource.GetValue(PartSysParamId.part_velVariation_Z, out value))
                {
                    z += value * timeToSimulateSecs;
                }
            }

            /*
                Apply Pos Var
            */
            float xPosVar, yPosVar, zPosVar;

            if (spec.GetParticlePosCoordSys() == PartSysCoordSys.Polar)
            {
                // Get current particle spherical coordinates
                // Azimuth / Incliation are stored in radians alraedy, while the pos_var is in degrees
                var azimuth = Angles.ToDegrees(state.GetState(ParticleStateField.PSF_POS_AZIMUTH, particleIdx));
                var inclination =
                    Angles.ToDegrees(state.GetState(ParticleStateField.PSF_POS_INCLINATION, particleIdx));
                var radius = state.GetState(ParticleStateField.PSF_POS_RADIUS, particleIdx);

                // Modify them according to position variation parameters
                if (valueSource.GetValue(PartSysParamId.part_posVariation_X, out value))
                {
                    azimuth += value;
                }

                if (valueSource.GetValue(PartSysParamId.part_posVariation_Y, out value))
                {
                    inclination += value;
                }

                if (valueSource.GetValue(PartSysParamId.part_posVariation_Z, out value))
                {
                    radius += value;
                }

                // Convert the position that has been modified this way to cartesian
                var cartesianPosVar = SphericalDegToCartesian(azimuth, inclination, radius);

                // Add the current unmodified particle pos to get to the final position
                xPosVar = cartesianPosVar.X;
                yPosVar = cartesianPosVar.Y;
                zPosVar = cartesianPosVar.Z;
            }
            else
            {
                xPosVar = emitter.GetParamValue(PartSysParamId.part_posVariation_X, particleIdx, particleAge);
                yPosVar = emitter.GetParamValue(PartSysParamId.part_posVariation_Y, particleIdx, particleAge);
                zPosVar = emitter.GetParamValue(PartSysParamId.part_posVariation_Z, particleIdx, particleAge);
            }

            // Save new particle state
            state.SetState(ParticleStateField.PSF_X, particleIdx, x);
            state.SetState(ParticleStateField.PSF_Y, particleIdx, y);
            state.SetState(ParticleStateField.PSF_Z, particleIdx, z);
            state.SetState(ParticleStateField.PSF_VEL_X, particleIdx, velX);
            state.SetState(ParticleStateField.PSF_VEL_Y, particleIdx, velY);
            state.SetState(ParticleStateField.PSF_VEL_Z, particleIdx, velZ);
            state.SetState(ParticleStateField.PSF_POS_VAR_X, particleIdx, x + xPosVar);
            state.SetState(ParticleStateField.PSF_POS_VAR_Y, particleIdx, y + yPosVar);
            state.SetState(ParticleStateField.PSF_POS_VAR_Z, particleIdx, z + zPosVar);
        }
    }
}