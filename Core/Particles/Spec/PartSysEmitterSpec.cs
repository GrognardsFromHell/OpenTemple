using System;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Spec;

public class PartSysEmitterSpec
{
    public PartSysEmitterSpec(PartSysSpec parent, string name)
    {
        mParent = parent;
        mName = name;
        mParams = new IPartSysParam[(int) PartSysParamId.part_attractorBlend + 1];
    }

    public string GetName()
    {
        return mName;
    }

    public PartSysSpec GetParent()
    {
        return mParent;
    }

    public float GetDelay()
    {
        return mDelay;
    }

    public void SetDelay(float delay)
    {
        mDelay = delay;
    }

    public bool IsPermanent()
    {
        return mPermanent;
    }

    public void SetPermanent(bool enable)
    {
        mPermanent = enable;
    }

    public float GetLifespan()
    {
        return mLifespan;
    }

    public void SetLifespan(float lifespan)
    {
        mLifespan = lifespan;
    }

    public bool IsPermanentParticles()
    {
        return mPermanentParticles;
    }

    public void SetPermanentParticles(bool permanentParticles)
    {
        mPermanentParticles = permanentParticles;
    }

    public float GetParticleLifespan()
    {
        return mParticleLifespan;
    }

    public void SetParticleLifespan(float particleLifespan)
    {
        mParticleLifespan = particleLifespan;
    }

    public int GetMaxParticles()
    {
        return mMaxParticles;
    }

    public void SetMaxParticles(int maxParticles)
    {
        mMaxParticles = maxParticles;
    }

    public float GetParticleRate()
    {
        return mParticleRate;
    }

    public void SetParticleRate(float particleRate)
    {
        mParticleRate = particleRate;
    }

    // This is the minimal particle spawn rate when fidelity is 0
    public float GetParticleRateMin()
    {
        return mParticleRateMin;
    }

    // This is the minimal particle spawn rate when fidelity is 0
    public void SetParticleRateMin(float particleRateSecondary)
    {
        mParticleRateMin = particleRateSecondary;
    }

    public float GetEffectiveParticleRate(float fidelity)
    {
        return mParticleRateMin + (mParticleRate - mParticleRateMin) * fidelity;
    }

    public bool IsInstant()
    {
        return mInstant;
    }

    public void SetInstant(bool instant)
    {
        mInstant = instant;
    }

    public PartSysEmitterSpace GetSpace()
    {
        return mSpace;
    }

    public void SetSpace(PartSysEmitterSpace space)
    {
        mSpace = space;
    }

    public string GetNodeName()
    {
        return mNodeName;
    }

    public void SetNodeName(string nodeName)
    {
        mNodeName = nodeName;
    }

    public PartSysCoordSys GetCoordSys()
    {
        return mCoordSys;
    }

    public void SetCoordSys(PartSysCoordSys coordSys)
    {
        mCoordSys = coordSys;
    }

    public PartSysCoordSys GetOffsetCoordSys()
    {
        return mOffsetCoordSys;
    }

    public void SetOffsetCoordSys(PartSysCoordSys offsetCoordSys)
    {
        mOffsetCoordSys = offsetCoordSys;
    }

    public PartSysBlendMode GetBlendMode()
    {
        return mBlendMode;
    }

    public void SetBlendMode(PartSysBlendMode blendMode)
    {
        mBlendMode = blendMode;
    }

    public string GetTextureName()
    {
        return mTextureName;
    }

    public void SetTextureName(string texture)
    {
        mTextureName = texture;
    }

    public PartSysCoordSys GetParticlePosCoordSys()
    {
        return mParticlePosCoordSys;
    }

    public void SetParticlePosCoordSys(PartSysCoordSys particlePosCoordSys)
    {
        mParticlePosCoordSys = particlePosCoordSys;
    }

    public PartSysCoordSys GetParticleVelocityCoordSys()
    {
        return mParticleVelocityCoordSys;
    }

    public void SetParticleVelocityCoordSys(PartSysCoordSys particleVelocityCoordSys)
    {
        mParticleVelocityCoordSys = particleVelocityCoordSys;
    }

    public PartSysParticleSpace GetParticleSpace()
    {
        return mParticleSpace;
    }

    public void SetParticleSpace(PartSysParticleSpace particleSpace)
    {
        mParticleSpace = particleSpace;
    }

    public string GetMeshName()
    {
        return mMeshName;
    }

    public void SetMeshName(string meshName)
    {
        mMeshName = meshName;
    }

    public float GetBoxLeft()
    {
        return mBoxLeft;
    }

    public void SetBoxLeft(float boxLeft)
    {
        mBoxLeft = boxLeft;
    }

    public float GetBoxTop()
    {
        return mBoxTop;
    }

    public void SetBoxTop(float boxTop)
    {
        mBoxTop = boxTop;
    }

    public float GetBoxRight()
    {
        return mBoxRight;
    }

    public void SetBoxRight(float boxRight)
    {
        mBoxRight = boxRight;
    }

    public float GetBoxBottom()
    {
        return mBoxBottom;
    }

    public void SetBoxBottom(float boxBottom)
    {
        mBoxBottom = boxBottom;
    }

    public IPartSysParam GetParam(PartSysParamId id)
    {
        if ((int) id < mParams.Length)
        {
            return mParams[(int) id];
        }
        else
        {
            return null;
        }
    }

    public PartSysParticleType GetParticleType()
    {
        return mParticleType;
    }

    public void SetParticleType(PartSysParticleType particleType)
    {
        mParticleType = particleType;
    }

    public void SetParam(PartSysParamId id, IPartSysParam param)
    {
        if ((int) id < mParams.Length)
        {
            mParams[(int) id] = param;
        }
        else
        {
            throw new ArgumentOutOfRangeException("Parameter index out of range: " + id);
        }
    }

    private PartSysSpec mParent;
    private string mName;
    private bool mInstant = false;
    private bool mPermanent = false;
    private bool mPermanentParticles = false;
    private float mLifespan = 1.0f;
    private float mParticleLifespan = 1.0f;
    private int mMaxParticles = 1;
    private float mParticleRate = 1.0f;
    private float mParticleRateMin = 0.0f;
    private PartSysEmitterSpace mSpace = PartSysEmitterSpace.World;
    private string mNodeName;
    private PartSysCoordSys mCoordSys = PartSysCoordSys.Cartesian;
    private PartSysCoordSys mOffsetCoordSys = PartSysCoordSys.Cartesian;
    private PartSysBlendMode mBlendMode = PartSysBlendMode.Add;
    private string mTextureName;
    private PartSysCoordSys mParticlePosCoordSys = PartSysCoordSys.Cartesian;
    private PartSysCoordSys mParticleVelocityCoordSys = PartSysCoordSys.Cartesian;
    private PartSysParticleSpace mParticleSpace = PartSysParticleSpace.World;
    private string mMeshName;
    private float mBoxLeft = -399.0f;
    private float mBoxTop = -299.0f;
    private float mBoxRight = 399.0f;
    private float mBoxBottom = 299.0f;
    private IPartSysParam[] mParams;
    private PartSysParticleType mParticleType = PartSysParticleType.Point;
    private float mDelay = 0.0f;
}