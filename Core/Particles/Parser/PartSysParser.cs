using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.IO.TabFiles;
using OpenTemple.Core.Logging;
using OpenTemple.Core.MaterialDefinitions;
using OpenTemple.Core.Particles.Spec;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Parser;

public class PartSysParser
{
    private const int COL_PARTSYS_NAME = 0;
    private const int COL_EMITTER_NAME = 1;
    private const int COL_DELAY = 2;
    private const int COL_EmitType = 3;
    private const int COL_LIFESPAN = 4;
    private const int COL_PARTICLE_RATE = 5;
    private const int COL_BoundingRadius = 6;
    private const int COL_EMITTER_SPACE = 7;
    private const int COL_EMITTER_NODE_NAME = 8;
    private const int COL_EMITTER_COORD_SYS = 9;
    private const int COL_EMITTER_OFFSET_COORD_SYS = 10;
    private const int COL_PARTICLE_TYPE = 11;
    private const int COL_PARTICLE_SPACE = 12;
    private const int COL_PARTICLE_POS_COORD_SYS = 13;
    private const int COL_PARTICLE_VELOCITY_COORD_SYS = 14;
    private const int COL_MATERIAL = 15;
    private const int COL_PART_LIFESPAN = 16;
    private const int COL_BLEND_MODE = 17;
    private const int COL_Bounce = 18;
    private const int COL_AnimSpeed = 19;
    private const int COL_MODEL = 20;
    private const int COL_Animation = 21;
    private const int COL_BB_LEFT = 67;
    private const int COL_BB_TOP = 68;
    private const int COL_BB_RIGHT = 69;
    private const int COL_BB_BOTTOM = 70;
    private const int COL_PARTICLE_RATE_SECONDARY = 71;

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public PartSysSpec? GetSpec(string name)
    {
        return _specs.GetValueOrDefault(name.ToLowerInvariant());
    }

    public IReadOnlyDictionary<string, PartSysSpec> Specs => _specs;

    private readonly Dictionary<string, PartSysSpec> _specs = new();

    // To speed up loading
    private readonly Dictionary<string, string> _textureNameCache = new();

    private static readonly Dictionary<string, PartSysCoordSys> CoordSysMapping =
        new()
        {
            {"Cartesian", PartSysCoordSys.Cartesian},
            {"Polar", PartSysCoordSys.Polar}
        };

    private static readonly Dictionary<string, PartSysEmitterSpace> EmitterSpaceMapping =
        new()
        {
            {"World", PartSysEmitterSpace.World},
            {"Object Pos", PartSysEmitterSpace.ObjectPos},
            {"Object YPR", PartSysEmitterSpace.ObjectYpr},
            {"Node Pos", PartSysEmitterSpace.NodePos},
            {"Node YPR", PartSysEmitterSpace.NodeYpr},
            {"Bones", PartSysEmitterSpace.Bones}
        };

    private static readonly Dictionary<string, PartSysParticleType> ParticleTypeMapping =
        new()
        {
            {"Point", PartSysParticleType.Point},
            {"Sprite", PartSysParticleType.Sprite},
            {"Disc", PartSysParticleType.Disc},
            {"Billboard", PartSysParticleType.Billboard},
            {"Model", PartSysParticleType.Model}
        };

    private static readonly Dictionary<string, PartSysBlendMode> BlendModeMapping =
        new()
        {
            {"Add", PartSysBlendMode.Add},
            {"Blend", PartSysBlendMode.Blend},
            {"Multiply", PartSysBlendMode.Multiply},
            {"Subtract", PartSysBlendMode.Subtract}
        };

    private static readonly Dictionary<string, PartSysParticleSpace> ParticleSpaceMapping =
        new()
        {
            {"World", PartSysParticleSpace.World},
            {"Emitter YPR", PartSysParticleSpace.EmitterYpr},
            {"Same as Emitter", PartSysParticleSpace.SameAsEmitter}
        };

    private static void ParseOptionalFloat(in TabFileRecord record, int col, string name, Action<float> setter)
    {
        var column = record[col];
        if (column)
        {
            if (column.TryGetFloat(out var value))
            {
                setter(value);
            }
            else
            {
                Logger.Warn("Emitter on line {0} has invalid {1}: '{2}'",
                    record.LineNumber, name, column.AsString());
            }
        }
    }

    private static void ParseOptionalEnum<T>(in TabFileRecord record,
        int col,
        string name,
        Dictionary<string, T> mapping,
        Action<T> setter) where T : Enum
    {
        var column = record[col];
        if (column)
        {
            if (column.TryGetEnum(mapping, out var value))
            {
                setter(value);
            }
            else
            {
                Logger.Warn("Emitter on line {0} has invalid {1}: '{2}'",
                    record.LineNumber, name, column.AsString());
            }
        }
    }

    void ParseLifespan(in TabFileRecord record, PartSysEmitterSpec emitter)
    {
        var colLifespan = record[COL_LIFESPAN];
        if (!colLifespan || colLifespan.EqualsIgnoreCase("perm"))
        {
            emitter.SetPermanent(true);
        }
        else if (colLifespan.TryGetFloat(out var lifespan))
        {
            if (lifespan == 0)
            {
                emitter.SetInstant(true);
            }

            emitter.SetLifespan(lifespan / 30.0f);
        }
        else
        {
            Logger.Warn("Emitter on line {0} has invalid lifespan: '{1}'",
                record.LineNumber, colLifespan);
        }
    }

    void ParseParticleLifespan(in TabFileRecord record, PartSysEmitterSpec emitter)
    {
        var colLifespan = record[COL_PART_LIFESPAN];
        if (!colLifespan || colLifespan.EqualsIgnoreCase("perm"))
        {
            emitter.SetPermanentParticles(true);
            emitter.SetParticleLifespan(1.0f);
        }
        else if (colLifespan.TryGetFloat(out var lifespan))
        {
            emitter.SetParticleLifespan(lifespan / 30.0f);
        }
        else
        {
            Logger.Warn("Emitter on line {0} has invalid particle lifespan: '{1}'",
                record.LineNumber, colLifespan);
        }
    }

    void ParseParticleRate(in TabFileRecord record, PartSysEmitterSpec emitter)
    {
        var maxParticles = emitter.GetMaxParticles();
        if (!record[COL_PARTICLE_RATE].TryGetFloat(out var rate) || rate == 0)
        {
            Logger.Warn("Emitter on line {0} has invalid particle rate: '{1}'",
                record.LineNumber, record[COL_PARTICLE_RATE]);
        }
        else if (emitter.IsPermanent())
        {
            // For a permanent emitter, the max. number of particles is how many spawn per time unit multiplied
            // by how many time units they will exist
            maxParticles = (int) (emitter.GetParticleLifespan() * rate) + 1;
        }
        else if (emitter.IsInstant() || emitter.IsPermanentParticles())
        {
            maxParticles = (int) (rate) + 1;
        }
        else
        {
            // If the emitter lifespan limits the number of particles more, use that to calculate the max. particles
            var lifespan = MathF.Min(emitter.GetLifespan(), emitter.GetParticleLifespan() * 30.0f);
            maxParticles = (int) (rate * lifespan) + 1;
        }

        emitter.SetMaxParticles(maxParticles);
        emitter.SetParticleRate(rate);
        emitter.SetParticleRateMin(rate);

        // This secondary particle rate is the minimum that is used for scaling
        // down particle systems using the particle fidelity slider in the game
        // settings
        var minRateCol = record[COL_PARTICLE_RATE_SECONDARY];
        if (!minRateCol.IsEmpty)
        {
            if (minRateCol.TryGetFloat(out var rateSecondary))
            {
                emitter.SetParticleRateMin(rateSecondary);
            }
            else
            {
                Logger.Warn("Emitter on line {0} has invalid secondary particle rate: '{1}'",
                    record.LineNumber, minRateCol);
            }
        }
    }

    void ParseEmitterNodeName(in TabFileRecord record, PartSysEmitterSpec emitter)
    {
        var col = record[COL_EMITTER_NODE_NAME];
        if (col)
        {
            emitter.SetNodeName(col.AsString());
        }
    }

    void ParseMaterial(in TabFileRecord record, PartSysEmitterSpec emitter)
    {
        var colMaterial = record[COL_MATERIAL];
        if (!colMaterial)
        {
            return;
        }

        var materialName = colMaterial.AsString();

        if (_textureNameCache.TryGetValue(materialName, out var knownTextureName))
        {
            emitter.SetTextureName(knownTextureName);
            return;
        }

        /*
            Since the renderer will only display a texture and ignore all other MDF properties,
            we parse the texture from the MDF file here to speed this process up.
            This also removes a dependency on the actual MDF material factory.
        */
        var fullName = $"art/meshes/particle/{materialName}.mdf";
        try
        {
            var mdfContent = Tig.FS.ReadTextFile(fullName);
            MdfParser mdfParser = new MdfParser(fullName, mdfContent);
            var mdfMaterial = mdfParser.Parse();
            if (mdfMaterial.Samplers.Count == 0)
            {
                Logger.Warn("Emitter on line {0} has material: '{1}' with no associated textures.",
                    record.LineNumber, fullName);
                return;
            }

            var textureName = mdfMaterial.Samplers[0].Filename;
            emitter.SetTextureName(textureName);
            _textureNameCache[materialName] = textureName;
        }
        catch (Exception e)
        {
            Logger.Warn("Emitter on line {0} has unknown material: '{1}': {2}",
                record.LineNumber, fullName, e.Message);
        }
    }

    void ParseMesh(in TabFileRecord record, PartSysEmitterSpec emitter)
    {
        // This only applies to emitters that emit 3D particles
        if (emitter.GetParticleType() != PartSysParticleType.Model)
        {
            return;
        }

        // The model filename is usually just the filename without path + extension
        emitter.SetMeshName(record[COL_MODEL].AsString());
    }

    public void ParseEmitter(in TabFileRecord record)
    {
        var systemName = record[COL_PARTSYS_NAME].AsString();

        var systemNameLower = systemName.ToLowerInvariant();
        if (!_specs.TryGetValue(systemNameLower, out var system))
        {
            // Create it on demand
            system = new PartSysSpec(systemName);
            _specs[systemNameLower] = system;
        }

        // Add the emitter
        var emitter = system.CreateEmitter(record[COL_EMITTER_NAME].AsString());

        ParseOptionalFloat(record, COL_DELAY, "Delay", value => emitter.SetDelay(value / 30.0f));

        ParseLifespan(record, emitter);

        ParseParticleLifespan(record, emitter);

        ParseParticleRate(record, emitter);

        ParseOptionalEnum(record, COL_EMITTER_SPACE, "emitter space", EmitterSpaceMapping,
            emitter.SetSpace);

        ParseEmitterNodeName(record, emitter);

        ParseOptionalEnum(record, COL_EMITTER_COORD_SYS, "emitter coord sys", CoordSysMapping,
            emitter.SetCoordSys);

        ParseOptionalEnum(record, COL_EMITTER_OFFSET_COORD_SYS, "emitter offset coord sys",
            CoordSysMapping, emitter.SetOffsetCoordSys);

        ParseOptionalEnum(record, COL_PARTICLE_TYPE, "particle type", ParticleTypeMapping,
            emitter.SetParticleType);

        ParseOptionalEnum(record, COL_BLEND_MODE, "blend mode", BlendModeMapping,
            emitter.SetBlendMode);

        ParseMaterial(record, emitter);

        ParseOptionalEnum(record, COL_PARTICLE_POS_COORD_SYS, "particle pos coord sys",
            CoordSysMapping, emitter.SetParticlePosCoordSys);

        ParseOptionalEnum(record, COL_PARTICLE_VELOCITY_COORD_SYS, "particle velocity coord sys",
            CoordSysMapping, emitter.SetParticleVelocityCoordSys);

        ParseOptionalEnum(record, COL_PARTICLE_SPACE, "particle space", ParticleSpaceMapping,
            emitter.SetParticleSpace);

        ParseMesh(record, emitter);

        // Parse the bounding box
        ParseOptionalFloat(record, COL_BB_LEFT, "bb left", emitter.SetBoxLeft);
        ParseOptionalFloat(record, COL_BB_TOP, "bb top", emitter.SetBoxTop);
        ParseOptionalFloat(record, COL_BB_RIGHT, "bb right", emitter.SetBoxRight);
        ParseOptionalFloat(record, COL_BB_BOTTOM, "bb bottom", emitter.SetBoxBottom);

        for (int paramId = 0; paramId <= (int) PartSysParamId.part_attractorBlend; paramId++)
        {
            int colIdx = 22 + paramId;
            var col = record[colIdx];
            if (col)
            {
                var param = ParserParams.Parse((PartSysParamId) paramId,
                    col,
                    emitter.GetLifespan(),
                    emitter.GetParticleLifespan(),
                    out var success);
                if (success)
                {
                    emitter.SetParam((PartSysParamId) paramId, param);
                }
                else
                {
                    Logger.Warn(
                        "Unable to parse particle system param {0} for particle system {1} and emitter {2} with value '{3}'",
                        paramId, systemName, emitter.GetName(), col.AsString());
                }
            }
        }
    }

    public void ParseFile(string filename)
    {
        TabFile.ParseFile(filename, ParseEmitter);
    }

    public void ParseString(ReadOnlySpan<byte> spec)
    {
        TabFile.ParseSpan(spec, ParseEmitter);
    }
}