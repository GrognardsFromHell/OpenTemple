using System;
using System.Collections.Generic;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Render;

internal class ModelParticleRenderer : ParticleRenderer
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static readonly string[] SearchPath =
    {
        "art/meshes/Particle/",
        "art/meshes/Scenery/Containers/",
        "art/meshes/Scenery/Misc/Main Menu/",
        "art/meshes/Weapons/"
    };

    private readonly IAnimatedModelFactory _modelFactory;
    private readonly IAnimatedModelRenderer _modelRenderer;

    public ModelParticleRenderer(IAnimatedModelFactory aasFactory,
        IAnimatedModelRenderer aasRenderer)
    {
        _modelFactory = aasFactory;
        _modelRenderer = aasRenderer;
    }

    public override void Render(IGameViewport viewport, PartSysEmitter emitter)
    {
        var it = emitter.NewIterator();

        var animParams = AnimatedModelParams.Default;
        animParams.rotation3d = true;

        // Lazily initialize render state
        if (!emitter.HasRenderState())
        {
            // Resolve the mesh filename
            var baseName = ResolveBasename(emitter.GetSpec().GetMeshName());
            var skmName = baseName + ".skm";
            var skaName = baseName + ".ska";

            try
            {
                var animId = new EncodedAnimId(NormalAnimType.ItemIdle); // This seems to be item_idle
                var model = _modelFactory.FromFilenames(skmName, skaName, animId, animParams);

                emitter.SetRenderState(
                    new ModelEmitterRenderState(model)
                );
            }
            catch (Exception e)
            {
                Logger.Error("Unable to load model {0} for particle system {1}: {2}",
                    baseName, emitter.GetSpec().GetParent().GetName(), e);

                emitter.SetRenderState(new ModelEmitterRenderState(null));
            }
        }

        var renderState = (ModelEmitterRenderState) emitter.GetRenderState();

        if (renderState.Model == null)
        {
            return; // The loader above was unable to load the model for this emitter
        }

        var overrides = new MdfRenderOverrides
        {
            ignoreLighting = true,
            overrideDiffuse = true
        };

        var yaw = emitter.GetParamState(PartSysParamId.part_yaw);
        var pitch = emitter.GetParamState(PartSysParamId.part_pitch);
        var roll = emitter.GetParamState(PartSysParamId.part_roll);

        while (it.HasNext())
        {
            var particleIdx = it.Next();
            var age = emitter.GetParticleAge(particleIdx);

            overrides.overrideColor = GeneralEmitterRenderState.GetParticleColor(emitter, particleIdx);

            // Yes, this is *actually* swapped for Y / Z
            var particleState = emitter.GetParticleState();
            animParams.offsetX = particleState.GetState(ParticleStateField.PSF_POS_VAR_X, particleIdx);
            animParams.offsetY = particleState.GetState(ParticleStateField.PSF_POS_VAR_Z, particleIdx);
            animParams.offsetZ = particleState.GetState(ParticleStateField.PSF_POS_VAR_Y, particleIdx);

            if (yaw != null)
            {
                animParams.rotationYaw = Angles.ToRadians(yaw.GetValue(emitter, particleIdx, age));
            }

            if (pitch != null)
            {
                animParams.rotationPitch = Angles.ToRadians(pitch.GetValue(emitter, particleIdx, age));
            }

            if (roll != null)
            {
                animParams.rotationRoll = Angles.ToRadians(roll.GetValue(emitter, particleIdx, age));
            }

            renderState.Model.SetTime(animParams, age);

            _modelRenderer.Render(viewport, renderState.Model, animParams, new List<Light3d>(), overrides);
        }
    }

    private static string ResolveBasename(string modelName)
    {
        // A real, existing basename...
        if (Tig.FS.FileExists(modelName + ".skm") && Tig.FS.FileExists(modelName + ".ska"))
        {
            return modelName;
        }

        foreach (var searchPath in SearchPath)
        {
            var baseName = searchPath + modelName;
            if (Tig.FS.FileExists(baseName + ".skm") && Tig.FS.FileExists(baseName + ".ska"))
            {
                return baseName;
            }
        }

        // Probably invalid . will throw
        return modelName;
    }
}