using System;
using System.Buffers;
using System.Collections.Generic;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Particles.Render;

internal abstract class QuadParticleRenderer : ParticleRenderer, IDisposable
{
    private static readonly MemoryPool<SpriteVertex> MemoryPool = MemoryPool<SpriteVertex>.Shared;
    private readonly RenderingDevice _device;

    private ResourceRef<IndexBuffer> _indexBuffer;

    protected QuadParticleRenderer(RenderingDevice device)
    {
        _device = device;

        // Set up an index bufer for the max. quad drawing
        var indices = new List<ushort>(0x8000 / 4 * 6);
        for (ushort i = 0; i < 0x8000; i += 4)
        {
            indices.Add((ushort) (i + 0));
            indices.Add((ushort) (i + 1));
            indices.Add((ushort) (i + 2));
            indices.Add((ushort) (i + 2));
            indices.Add((ushort) (i + 3));
            indices.Add((ushort) (i + 0));
        }

        _indexBuffer = device.CreateIndexBuffer(indices.ToArray());
    }

    public void Dispose()
    {
        _indexBuffer.Dispose();
        _device?.Dispose();
    }

    public override void Render(IGameViewport viewport, PartSysEmitter emitter)
    {
        if (!GetEmitterWorldMatrix(viewport, emitter, out var worldMatrix))
        {
            return;
        }

        _device.SetVertexShaderConstants(0, ref worldMatrix);
        _device.SetIndexBuffer(_indexBuffer);

        RenderParticles(emitter);
    }

    private void RenderParticles(PartSysEmitter emitter)
    {
        var it = emitter.NewIterator();
        var totalCount = emitter.GetActiveCount();

        if (totalCount == 0)
        {
            return;
        }

        // Lazily initialize render state
        if (!emitter.HasRenderState())
        {
            emitter.SetRenderState(
                new QuadEmitterRenderState(_device, emitter));
        }

        var renderState = (QuadEmitterRenderState) emitter.GetRenderState();

        using var spriteMemory = MemoryPool.Rent(4 * totalCount);
        var sUpdateBuffer = spriteMemory.Memory.Span.Slice(0, 4 * totalCount);

        var i = 0;
        while (it.HasNext())
        {
            var particleIdx = it.Next();
            FillVertex(emitter, particleIdx, sUpdateBuffer, i);
            i += 4;
        }

        renderState.vertexBuffer.Resource.Update<SpriteVertex>(sUpdateBuffer);

        _device.SetMaterial(renderState.material);
        renderState.bufferBinding.Resource.Bind();

        // Draw the batch
        _device.DrawIndexed(PrimitiveType.TriangleList, 4 * totalCount, 6 * totalCount);
    }

    protected abstract void FillVertex(PartSysEmitter emitter,
        int particleIdx,
        Span<SpriteVertex> vertices,
        int vertexIdx);
}