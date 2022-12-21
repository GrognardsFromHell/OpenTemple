using System;
using System.Buffers;

namespace OpenTemple.Core.Particles.Instances;

public class ParticleState : IDisposable
{
    private static readonly MemoryPool<float> Pool = MemoryPool<float>.Shared;

    public ParticleState(int particleCount)
    {
        _capacity = particleCount;
        // Round up to 4 since SSE always processes 4 at a time
        if (_capacity % 4 != 0)
        {
            _capacity = (particleCount / 4 + 1) * 4;
        }

        // Store one float per possible state field and particle.
        var dataSize = (int) ParticleStateField.PSF_COUNT * _capacity;
        _data = Pool.Rent(dataSize);
        _memory = _data.Memory;
    }

    public void Dispose()
    {
        if (_data != null)
        {
            _memory = Memory<float>.Empty;
            _data.Dispose();
            _data = null;
        }
    }

    public void SetState(ParticleStateField field, int particleIdx, float value)
    {
        GetStatePtr(field, particleIdx) = value;
    }

    public float GetState(ParticleStateField field, int particleIdx)
    {
        return GetStatePtr(field, particleIdx);
    }

    public ref float GetStatePtr(ParticleStateField field, int particleIdx)
    {
        return ref _memory.Span[_capacity * (int) field + particleIdx];
    }

    private readonly int _capacity;
    private IMemoryOwner<float>? _data;
    private Memory<float> _memory;
}