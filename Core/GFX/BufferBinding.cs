using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace OpenTemple.Core.GFX;

public enum VertexElementType : uint {
	Float1,
	Float2,
	Float3,
	Float4,
	Color, // 4 byte ARGB
	UByte4, // 4 unsigned bytes
	Short2, // 2 signed shorts (expanded to (s1, s2, 0, 1))
	Short4, // 4 signed shorts
	UByte4N, // 4 unsigned bytes, normalized to [0, 1] by dividing by 255
	Short2N, // 2 signed shorts normalized to [0, 1] by dividing by 32767
	Short4N, // 2 signed shorts normalized to [0, 1] by dividing by 32767
	UShort2N, // 2 unsigned shorts normalized to [0, 1] by dividing by 65535
	UShort4N, // 4 unsigned shorts normalized to [0, 1] by dividing by 65535
}

public enum VertexElementSemantic : uint {
	Position,
	BlendWeight,
	BlendIndices,
	Normal,
	PointSize,
	TexCoord,
	Tangent,
	BiNormal,
	TessFactor,
	PositionT,
	Color,
	Fog,
	Depth,
	Sample
}

struct VertexElement {
	public ushort Stream; // Stream index (0 based)
	public ushort Offset; // Offset from the start of the stream in bytes
	public VertexElementType Type; // Data type of the element
	public VertexElementSemantic Semantic; // Semantic for this element
	// If more than slot exists for the semantic, use this to identify which one
	public byte SemanticIndex;
};

public ref struct BufferBindingBuilder {
	private BufferBinding _binding;
	private int _streamIdx;
	private int _curOffset;
	
	public BufferBindingBuilder AddElement(
		VertexElementType type,
		VertexElementSemantic semantic,
		byte semanticIndex = 0
	)
	{

		var elemIdx = _binding._elementCount++;
		ref var elem = ref _binding._elements[elemIdx];
		elem.Stream = (ushort) _streamIdx;
		elem.Offset = (ushort) _curOffset;
		elem.Type = type;
		elem.Semantic = semantic;
		elem.SemanticIndex = semanticIndex;

		_curOffset += BufferBinding.GetElementSize(type);

		return this;
	}

	internal BufferBindingBuilder(BufferBinding binding, int streamIdx)
	{
		_binding = binding;
		_streamIdx = streamIdx;
		_curOffset = 0;
	}
}

public class BufferBinding : GpuResource<BufferBinding>
{
	internal VertexElement[] _elements = new VertexElement[16];
	internal int _elementCount; // Actual number of used elements in _elements
	private readonly OptionalResourceRef<VertexBuffer>[] _streams = new OptionalResourceRef<VertexBuffer>[16];
	private readonly int[] _offsets = new int[16];
	private readonly int[] _strides = new int[16];
	private int _streamCount; // Actual number of used streams
	private InputLayout? _inputLayout;
	private ResourceRef<VertexShader> _shader;
	private readonly RenderingDevice _device;

	public BufferBinding(RenderingDevice device, VertexShader shader)
	{
		_device = device;
		_shader = shader.Ref();
	}

	public BufferBinding SetBuffer(int streamIdx, VertexBuffer buffer) {
		_streams[streamIdx].Dispose();
		_streams[streamIdx] = new ResourceRef<VertexBuffer>(buffer);
		return this;
	}

	public BufferBindingBuilder AddBuffer(VertexBuffer buffer, int offset, int stride) {
		var streamIdx = _streamCount++;
		_streams[streamIdx] = new ResourceRef<VertexBuffer>(buffer);
		_offsets[streamIdx] = offset;
		_strides[streamIdx] = stride;

		return new BufferBindingBuilder(this, streamIdx);
	}

	public BufferBindingBuilder AddBuffer<T>(VertexBuffer? buffer, int offset) where T : unmanaged {
		var streamIdx = _streamCount++;
		_streams[streamIdx] = new OptionalResourceRef<VertexBuffer>(buffer);
		_offsets[streamIdx] = offset;
		unsafe {
			_strides[streamIdx] = sizeof(T);
		}

		return new BufferBindingBuilder(this, streamIdx);
	}

	protected override void FreeResource()
	{
		_shader.Dispose();
		_inputLayout?.Dispose();
		foreach (var stream in _streams)
		{
			stream.Dispose();
		}
	}

	public void Bind()
	{

		// D3D11 version
		if (_inputLayout == null)
		{
			var inputDesc = new List<InputElement>(16);

			for (int i = 0; i < _elementCount; ++i) {
				ref var elemIn = ref _elements[i];

				var desc = new InputElement();

				switch (elemIn.Semantic) {
					case VertexElementSemantic.Position:
						desc.SemanticName = "POSITION";
						break;
					case VertexElementSemantic.Normal:
						desc.SemanticName = "NORMAL";
						break;
					case VertexElementSemantic.TexCoord:
						desc.SemanticName = "TEXCOORD";
						break;
					case VertexElementSemantic.Color:
						desc.SemanticName = "COLOR";
						break;
					default:
						throw new GfxException("Unsupported semantic");
				}
				desc.SemanticIndex = elemIn.SemanticIndex;
				desc.AlignedByteOffset = elemIn.Offset;
				desc.InstanceDataStepRate = 0;
				desc.Classification = InputClassification.PerVertexData;
				desc.Slot = elemIn.Stream;

				switch (elemIn.Type) {
					case VertexElementType.Float1:
						desc.Format = Format.R32_Float;
						break;
					case VertexElementType.Float2:
						desc.Format = Format.R32G32_Float;
						break;
					case VertexElementType.Float3:
						desc.Format = Format.R32G32B32_Float;
						break;
					case VertexElementType.Float4:
						desc.Format = Format.R32G32B32A32_Float;
						break;
					case VertexElementType.Color:
						desc.Format = Format.B8G8R8A8_UNorm;
						break;
					default:
						throw new GfxException("Unsupported vertex element type.");
				}

				inputDesc.Add(desc);
			}

			_inputLayout = new InputLayout(
				_device.Device,
				_shader.Resource.CompiledCode,
				inputDesc.ToArray()
			); 
		}

		_device.Context.InputAssembler.InputLayout = _inputLayout;

		// Set the stream sources
		var vertexBuffers = new List<Buffer?>(16);
		for (int i = 0; i < _streamCount; ++i) {
			vertexBuffers.Add(_streams[i].Resource?.Buffer);
		}
		for (var i = _streamCount; i < 16; i++) {
			vertexBuffers.Add(null);
		}
		var offsets = new int[16];

		_device.Context.InputAssembler.SetVertexBuffers(0, vertexBuffers.ToArray(), _strides, offsets);
	}

	internal static int GetElementSize(VertexElementType type)
	{
	        
		switch (type) {
			case VertexElementType.Float1:
				return sizeof(float);
			case VertexElementType.Float2:
				return sizeof(float) * 2;
			case VertexElementType.Float3:
				return sizeof(float) * 3;
			case VertexElementType.Float4:
				return sizeof(float) * 4;
			case VertexElementType.Color:
				return sizeof(byte) * 4;
			case VertexElementType.UByte4:
				return sizeof(byte) * 4;
			case VertexElementType.Short2:
				return sizeof(short) * 2;
			case VertexElementType.Short4:
				return sizeof(short) * 4;
			case VertexElementType.UByte4N:
				return sizeof(byte) * 4;
			case VertexElementType.Short2N:
				return sizeof(short) * 2;
			case VertexElementType.Short4N:
				return sizeof(short) * 4;
			case VertexElementType.UShort2N:
				return sizeof(ushort) * 2;
			case VertexElementType.UShort4N:
				return sizeof(ushort) * 4;
			default:
				throw new ArgumentException("Unknown vertex element type.");
		}
	}

}