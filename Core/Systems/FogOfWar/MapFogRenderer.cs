using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems.FogOfWar;

struct FogBlurKernel
{

	private const int PatternDimension = 5;
	private const int PatternStride = 8;

	// 4 kernels (shifted by 0,1,2,3 pixels right), each has
	// 5 rows with 8 columns (for shift-compensation)
	private byte[][] patterns; // [xShift][y][x]

	[Pure]
	public ReadOnlySpan<byte> GetKernel(int x)
	{
		return patterns[x & 3].AsSpan();
	}

	public static FogBlurKernel Create(byte totalPatternValue)
	{

		FogBlurKernel result = default;
		result.patterns = new byte[4][];
		for (int i = 0; i < result.patterns.Length; i++)
		{
			result.patterns[i] = new byte[PatternDimension * PatternStride];
		}

		// Precompute 5x5 inverted blur kernel for distributing the total pattern value
		Span<float> weights = stackalloc float[5 * 5];
		var weightSum = 0.0f;
		var weightOutIdx = 0;
		for (var y = -2; y <= 2; y++) {
			for (var x = -2; x <= 2; x++) {
				var strength = 3.0f - MathF.Sqrt(x * x + y * y);
				weights[weightOutIdx++] = strength;
				weightSum += strength;
			}
		}

		// Now use the computed weights to calculate the actual pattern based on the requested total value
		var patternSum = 0;

		for (var y = 0; y < 5; ++y) {
			for (var x = 0; x < 5; ++x) {
				var pixelValue = (byte)(weights[y * 5 + x] / weightSum * totalPatternValue);
				result.patterns[0][y * PatternStride + x] = pixelValue;
				patternSum += pixelValue;
			}
		}

		// This assigns any remainder of the total pattern value (loss due to rounding down above)
		// to the center pixel of the 5x5 kernel
		result.patterns[0][2 * PatternStride + 2] += (byte) (totalPatternValue - patternSum);

		// Now create 3 versions of the pattern that are shifted by 1-3 pixels to the right
		// This is for optimization reasons since addition is done using 32-bit (4 bytes) addition
		// and the resulting patterns are in reality 8 pixels wide
		for (var xShift = 1; xShift <= 3; ++xShift) {
			for (var y = 0; y < 5; ++y) {
				for (var x = 0; x < 5; ++x) {
					// Output here is shifted by {1,2,3} pixels right
					result.patterns[xShift][y * PatternStride + x + xShift]
						= result.patterns[0][y * PatternStride + x];
				}
			}
		}

		return result;
	}
}

public class FogOfWarRenderer : IDisposable
{

	private readonly MapFoggingSystem _fogSystem;
	    
	private readonly FogBlurKernel sOpaquePattern = FogBlurKernel.Create(0xFF);
	private readonly FogBlurKernel sHalfTransparentPattern = FogBlurKernel.Create(0xA0);

	private MdfMaterialFactory mMdfFactory;
	private readonly RenderingDevice _device;

	private ResourceRef<Material> _material;

	private byte[] _blurredFog;
	private int _blurredFogWidth;
	private int _blurredFogHeight;

	private ResourceRef<DynamicTexture> _blurredFogTexture;

	private ResourceRef<VertexShader> _vertexShader;
	private ResourceRef<VertexBuffer> _vertexBuffer;
	private ResourceRef<IndexBuffer> _indexBuffer;
	private ResourceRef<BufferBinding> _bufferBinding;

	private Vector2 mFogOrigin;

	// Size of the buffers used by the fog system in subtiles
	private Size _originalFogSize;

	public FogOfWarRenderer(MapFoggingSystem fogSystem, RenderingDevice device)
	{
		_device = device;
		_fogSystem = fogSystem;

		_vertexShader = device.GetShaders().LoadVertexShader("fogofwar_vs");
		_bufferBinding = new BufferBinding(device, _vertexShader).Ref();

		UpdateBufferSize(true);

		Span<ushort> indices = stackalloc ushort[]
		{
			0, 2, 1, 2, 0, 3
		};
		_indexBuffer = device.CreateIndexBuffer(indices, debugName:"FogOfWar");
		_vertexBuffer = device.CreateEmptyVertexBuffer(FogOfWarVertex.Size * 4, debugName:"FogOfWar");

		_bufferBinding.Resource
			.AddBuffer<FogOfWarVertex>(_vertexBuffer, 0)
			.AddElement(VertexElementType.Float3, VertexElementSemantic.Position)
			.AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
	}

	private void UpdateBufferSize(bool force = false)
	{
		if (!force
		    && _originalFogSize.Width == _fogSystem._fogScreenBufferWidthSubtiles
		    && _originalFogSize.Height == _fogSystem._fogScreenBufferHeightSubtiles)
		{
			return;
		}

		_originalFogSize = new Size(
			_fogSystem._fogScreenBufferWidthSubtiles,
			_fogSystem._fogScreenBufferHeightSubtiles
		);

		_blurredFogTexture.Dispose();

		_blurredFogWidth = (_originalFogSize.Width / 4) * 4 + 8;
		_blurredFogHeight = (_originalFogSize.Height / 4) * 4 + 8;

		_blurredFogTexture = _device.CreateDynamicTexture(BufferFormat.A8, _blurredFogWidth, _blurredFogHeight);
		_blurredFog = new byte[_blurredFogWidth * _blurredFogHeight];

		CreateMaterial();
	}

	private void CreateMaterial()
	{
		using var ps = _device.GetShaders().LoadPixelShader("fogofwar_ps");
		var blendState = new BlendSpec();
		blendState.blendEnable = true;
		blendState.srcBlend = BlendOperand.SrcAlpha;
		blendState.destBlend = BlendOperand.InvSrcAlpha;
		DepthStencilSpec depthStencilState = new DepthStencilSpec();
		depthStencilState.depthEnable = false;
		RasterizerSpec rasterizerState = new RasterizerSpec();
		SamplerSpec samplerState = new SamplerSpec();
		samplerState.addressU = TextureAddress.Clamp;
		samplerState.addressV = TextureAddress.Clamp;
		samplerState.magFilter = TextureFilterType.Linear;
		samplerState.minFilter = TextureFilterType.Linear;

		MaterialSamplerSpec[] samplers = {
			new MaterialSamplerSpec(new ResourceRef<ITexture>(_blurredFogTexture.Resource), samplerState)
		};

		_material.Dispose();
		_material = _device.CreateMaterial(blendState, depthStencilState, rasterizerState, samplers, _vertexShader, ps)
			.Ref();
	}

	public void Render(IGameViewport viewport)
	{

		if (!_fogSystem._fogOfWarEnabled) {
			return;
		}

		UpdateBufferSize();

		var subtilesX = _fogSystem._fogScreenBufferWidthSubtiles;
		var subtilesY = _fogSystem._fogScreenBufferHeightSubtiles;

		using var perfGroup = _device.CreatePerfGroup("Fog Of War");

		// Reset the blurred buffer
		Span<byte> blurredFog = _blurredFog.AsSpan();
		blurredFog.Fill(0);

		var fogCheckData = _fogSystem._fogScreenBuffer;

		// Get [0,0] of the subtile data
		var fogDataIdx = 0;

		for (var y = 0; y < subtilesY; y++) {
			for (var x = 0; x < subtilesX; x++) {
				var fogState = fogCheckData[fogDataIdx++];

				// Bit 2 -> Currently in LoS of the party
				if ((fogState & 2) == 0) {
					ReadOnlySpan<byte> patternSrc;
					// Bit 3 -> Explored
					if ((fogState & 4) != 0) {
						patternSrc = sHalfTransparentPattern.GetKernel(x);
					} else
					{
						patternSrc = sOpaquePattern.GetKernel(x);
					}

					// Now we copy 5 rows of 2 dwords each, to apply
					// the filter-kernel to the blurred fog map
					var rowSrc = MemoryMarshal.Cast<byte, ulong>(patternSrc);

					for (var row = 0; row < 5; ++row)
					{
						var src = rowSrc[row];
						var roundedDownX = (x / 4) * 4;
						var destSlice = blurredFog.Slice((y + row) * _blurredFogWidth + roundedDownX);
						var rowDestSlice = MemoryMarshal.Cast<byte, ulong>(destSlice);

						// Due to how the kernel is layed out, the individual bytes in this 8-byte addition will never carry
						// over to the next higher byte, thus this is equivalent to 8 separate 1-byte additions
						rowDestSlice[0] += src;
					}
				}
			}
		}

		var mFogOriginX = _fogSystem._fogScreenBufferOrigin.locx;
		var mFogOriginY = _fogSystem._fogScreenBufferOrigin.locy;

		_blurredFogTexture.Resource.UpdateRaw(_blurredFog, _blurredFogWidth);

		// Use only the relevant subportion of the texture
		var umin = 2.5f / (float)_blurredFogWidth;
		var vmin = 2.5f / (float)_blurredFogHeight;
		var umax = (subtilesX - 0.5f) / (float)_blurredFogWidth;
		var vmax = (subtilesY - 0.5f) / (float)_blurredFogHeight;

		Span<FogOfWarVertex> mVertices = stackalloc FogOfWarVertex[4];
		mVertices[0].pos.X = (mFogOriginX * 3) * locXY.INCH_PER_SUBTILE;
		mVertices[0].pos.Y = 0;
		mVertices[0].pos.Z = (mFogOriginY * 3) * locXY.INCH_PER_SUBTILE;
		mVertices[0].uv = new Vector2(umin, vmin);

		mVertices[1].pos.X = (mFogOriginX * 3 + subtilesX) * locXY.INCH_PER_SUBTILE;
		mVertices[1].pos.Y = 0;
		mVertices[1].pos.Z = (mFogOriginY * 3) * locXY.INCH_PER_SUBTILE;
		mVertices[1].uv = new Vector2(umax, vmin);

		mVertices[2].pos.X = (mFogOriginX * 3 + subtilesX) * locXY.INCH_PER_SUBTILE;
		mVertices[2].pos.Y = 0;
		mVertices[2].pos.Z = (mFogOriginY * 3 + subtilesY) * locXY.INCH_PER_SUBTILE;
		mVertices[2].uv = new Vector2(umax, vmax);

		mVertices[3].pos.X = (mFogOriginX * 3) * locXY.INCH_PER_SUBTILE;
		mVertices[3].pos.Y = 0;
		mVertices[3].pos.Z = (mFogOriginY * 3 + subtilesX) * locXY.INCH_PER_SUBTILE;
		mVertices[3].uv = new Vector2(umin, vmax);

		_vertexBuffer.Resource.Update<FogOfWarVertex>(mVertices);

		_bufferBinding.Resource.Bind();
		_device.SetIndexBuffer(_indexBuffer);

		_device.SetMaterial(_material);
		var viewProj = viewport.Camera.GetViewProj();
		_device.SetVertexShaderConstants(0, ref viewProj);

		_device.DrawIndexed(PrimitiveType.TriangleList, 4, 6);

	}

	public void Dispose()
	{
		_vertexShader.Dispose();
		_material.Dispose();
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct FogOfWarVertex {
		public Vector3 pos;
		public Vector2 uv;

		public static readonly int Size = Marshal.SizeOf<FogOfWarVertex>();
	}

}