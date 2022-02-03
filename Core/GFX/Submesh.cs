using System;
using System.Numerics;

namespace OpenTemple.Core.GFX;

public interface ISubmesh
{
    int VertexCount { get; }
    int PrimitiveCount { get; }
    ReadOnlySpan<Vector4> Positions { get; }
    Span<Vector4> Normals { get; }
    ReadOnlySpan<Vector2> UV { get; }
    ReadOnlySpan<ushort> Indices { get; }
};