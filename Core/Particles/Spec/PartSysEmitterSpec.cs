using System;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Spec;

public class PartSysEmitterSpec
{
    public PartSysEmitterSpec(PartSysSpec parent, string name)
    {
        _parent = parent;
        _name = name;
        _params = new IPartSysParam[(int) PartSysParamId.part_attractorBlend + 1];
    }

    public string GetName()
    {
        return _name;
    }

    public PartSysSpec GetParent()
    {
        return _parent;
    }

    public float GetDelay()
    {
        return _delay;
    }

    public void SetDelay(float delay)
    {
        _delay = delay;
    }

    public bool IsPermanent()
    {
        return _permanent;
    }

    public void SetPermanent(bool enable)
    {
        _permanent = enable;
    }

    public float GetLifespan()
    {
        return _lifespan;
    }

    public void SetLifespan(float lifespan)
    {
        _lifespan = lifespan;
    }

    public bool IsPermanentParticles()
    {
        return _permanentParticles;
    }

    public void SetPermanentParticles(bool permanentParticles)
    {
        _permanentParticles = permanentParticles;
    }

    public float GetParticleLifespan()
    {
        return _particleLifespan;
    }

    public void SetParticleLifespan(float particleLifespan)
    {
        _particleLifespan = particleLifespan;
    }

    public int GetMaxParticles()
    {
        return _maxParticles;
    }

    public void SetMaxParticles(int maxParticles)
    {
        _maxParticles = maxParticles;
    }

    public float GetParticleRate()
    {
        return _particleRate;
    }

    public void SetParticleRate(float particleRate)
    {
        _particleRate = particleRate;
    }

    // This is the minimal particle spawn rate when fidelity is 0
    public float GetParticleRateMin()
    {
        return _particleRateMin;
    }

    // This is the minimal particle spawn rate when fidelity is 0
    public void SetParticleRateMin(float particleRateSecondary)
    {
        _particleRateMin = particleRateSecondary;
    }

    public float GetEffectiveParticleRate(float fidelity)
    {
        return _particleRateMin + (_particleRate - _particleRateMin) * fidelity;
    }

    public bool IsInstant()
    {
        return _instant;
    }

    public void SetInstant(bool instant)
    {
        _instant = instant;
    }

    public PartSysEmitterSpace GetSpace()
    {
        return _space;
    }

    public void SetSpace(PartSysEmitterSpace space)
    {
        _space = space;
    }

    public string GetNodeName()
    {
        return _nodeName;
    }

    public void SetNodeName(string nodeName)
    {
        _nodeName = nodeName;
    }

    public PartSysCoordSys GetCoordSys()
    {
        return _coordSys;
    }

    public void SetCoordSys(PartSysCoordSys coordSys)
    {
        _coordSys = coordSys;
    }

    public PartSysCoordSys GetOffsetCoordSys()
    {
        return _offsetCoordSys;
    }

    public void SetOffsetCoordSys(PartSysCoordSys offsetCoordSys)
    {
        _offsetCoordSys = offsetCoordSys;
    }

    public PartSysBlendMode GetBlendMode()
    {
        return _blendMode;
    }

    public void SetBlendMode(PartSysBlendMode blendMode)
    {
        _blendMode = blendMode;
    }

    public string GetTextureName()
    {
        return _textureName;
    }

    public void SetTextureName(string texture)
    {
        _textureName = texture;
    }

    public PartSysCoordSys GetParticlePosCoordSys()
    {
        return _particlePosCoordSys;
    }

    public void SetParticlePosCoordSys(PartSysCoordSys particlePosCoordSys)
    {
        _particlePosCoordSys = particlePosCoordSys;
    }

    public PartSysCoordSys GetParticleVelocityCoordSys()
    {
        return _particleVelocityCoordSys;
    }

    public void SetParticleVelocityCoordSys(PartSysCoordSys particleVelocityCoordSys)
    {
        _particleVelocityCoordSys = particleVelocityCoordSys;
    }

    public PartSysParticleSpace GetParticleSpace()
    {
        return _particleSpace;
    }

    public void SetParticleSpace(PartSysParticleSpace particleSpace)
    {
        _particleSpace = particleSpace;
    }

    public string GetMeshName()
    {
        return _meshName;
    }

    public void SetMeshName(string meshName)
    {
        _meshName = meshName;
    }

    public float GetBoxLeft()
    {
        return _boxLeft;
    }

    public void SetBoxLeft(float boxLeft)
    {
        _boxLeft = boxLeft;
    }

    public float GetBoxTop()
    {
        return _boxTop;
    }

    public void SetBoxTop(float boxTop)
    {
        _boxTop = boxTop;
    }

    public float GetBoxRight()
    {
        return _boxRight;
    }

    public void SetBoxRight(float boxRight)
    {
        _boxRight = boxRight;
    }

    public float GetBoxBottom()
    {
        return _boxBottom;
    }

    public void SetBoxBottom(float boxBottom)
    {
        _boxBottom = boxBottom;
    }

    public IPartSysParam? GetParam(PartSysParamId id)
    {
        if ((int) id < _params.Length)
        {
            return _params[(int) id];
        }
        else
        {
            return null;
        }
    }

    public PartSysParticleType GetParticleType()
    {
        return _particleType;
    }

    public void SetParticleType(PartSysParticleType particleType)
    {
        _particleType = particleType;
    }

    public void SetParam(PartSysParamId id, IPartSysParam param)
    {
        if ((int) id < _params.Length)
        {
            _params[(int) id] = param;
        }
        else
        {
            throw new ArgumentOutOfRangeException("Parameter index out of range: " + id);
        }
    }

    private PartSysSpec _parent;
    private string _name;
    private bool _instant = false;
    private bool _permanent = false;
    private bool _permanentParticles = false;
    private float _lifespan = 1.0f;
    private float _particleLifespan = 1.0f;
    private int _maxParticles = 1;
    private float _particleRate = 1.0f;
    private float _particleRateMin = 0.0f;
    private PartSysEmitterSpace _space = PartSysEmitterSpace.World;
    private string _nodeName;
    private PartSysCoordSys _coordSys = PartSysCoordSys.Cartesian;
    private PartSysCoordSys _offsetCoordSys = PartSysCoordSys.Cartesian;
    private PartSysBlendMode _blendMode = PartSysBlendMode.Add;
    private string _textureName;
    private PartSysCoordSys _particlePosCoordSys = PartSysCoordSys.Cartesian;
    private PartSysCoordSys _particleVelocityCoordSys = PartSysCoordSys.Cartesian;
    private PartSysParticleSpace _particleSpace = PartSysParticleSpace.World;
    private string _meshName;
    private float _boxLeft = -399.0f;
    private float _boxTop = -299.0f;
    private float _boxRight = 399.0f;
    private float _boxBottom = 299.0f;
    private IPartSysParam[] _params;
    private PartSysParticleType _particleType = PartSysParticleType.Point;
    private float _delay = 0.0f;
}