using System;
using SharpDX.DXGI;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.GFX
{
    public class RenderingDevice : IDisposable
    {
	    private ILogger _logger;

        public RenderingDevice(IntPtr windowHandle, int adapterIdx = 0, bool debugDevice = false)
        {
	        mWindowHandle = windowHandle;
	        mShaders = new Shaders(this);
	        mTextures = new Textures(this, 128 * 1024 * 1024);
			this.debugDevice = debugDevice;

			mDxgiFactory = new Factory1();

			  var displayDevices = GetDisplayDevices();

			  // Find the adapter selected by the user, although we might fall back to the
			  // default one
			  // if the user didn't select one or the adapter selection changed
			  mAdapter = GetAdapter(adapterIdx);
			  if (!mAdapter) {
				// Fall back to default
				_logger.Error("Couldn't retrieve adapter #{}. Falling back to default", 0);
				mAdapter = GetAdapter(displayDevices[0].id);
				if (!mAdapter) {
				  throw new GfxException("Couldn't retrieve your configured graphics adapter, but also couldn't fall back to the default adapter.");
				}
			  }

			  // Required for the Direct2D support
			  uint deviceFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
			  if (debugDevice) {
				deviceFlags |=
					D3D11_CREATE_DEVICE_DEBUG | D3D11_CREATE_DEVICE_DISABLE_GPU_TIMEOUT;
			  }

			  // Try creating a D3D11.1 device first (won't work on Vista and Win7 without
			  // SP2)
			  eastl::fixed_vector<D3D_FEATURE_LEVEL, 7> requestedLevels{
				  D3D_FEATURE_LEVEL_11_1, D3D_FEATURE_LEVEL_11_0, D3D_FEATURE_LEVEL_10_1,
				  D3D_FEATURE_LEVEL_10_0, D3D_FEATURE_LEVEL_9_3,  D3D_FEATURE_LEVEL_9_2,
				  D3D_FEATURE_LEVEL_9_1};

			  status = D3D11CreateDevice(mAdapter, D3D_DRIVER_TYPE_UNKNOWN, NULL,
										 deviceFlags, requestedLevels.data(),
										 requestedLevels.size(), D3D11_SDK_VERSION,
										 &mD3d11Device, &mFeatureLevel, &mContext);

			  if (status == DXGI_ERROR_INVALID_CALL) {
				_logger.info("D3D11.1 doesn't seem to be available on this system.");

				status = D3D11CreateDevice(mAdapter, D3D_DRIVER_TYPE_UNKNOWN, NULL,
										   deviceFlags, &requestedLevels[1],
										   requestedLevels.size() - 1, D3D11_SDK_VERSION,
										   &mD3d11Device, &mFeatureLevel, &mContext);
			  }

			  // Handle some special codes
			  if (debugDevice && status == DXGI_ERROR_SDK_COMPONENT_MISSING) {
				throw new GfxException("To use the D3D debugging feature, you need to "
									  "install the corresponding Windows SDK component.");
			  }

			  if (!SUCCEEDED(D3DLOG(status))) {
				throw new GfxException("Unable to create a Direct3D 11 device.");
			  }

			  _logger.info("Created D3D11 device with feature level {}", mFeatureLevel);

			  // Retrieve the interface used to emit event groupings for debugging
			  if (debugDevice) {
				  mContext.QueryInterface(&mImpl->annotation);
			  }

			  // Retrieve DXGI device
			  CComPtr<IDXGIDevice> dxgiDevice;
			  if (!SUCCEEDED(mD3d11Device.QueryInterface(&dxgiDevice))) {
				throw new GfxException("Couldn't retrieve DXGI device from D3D11 device.");
			  }
			  CComPtr<IDXGIAdapter> dxgiAdapter;
			  if (!SUCCEEDED(dxgiDevice->GetParent(__uuidof(IDXGIAdapter),
												   (void **)&dxgiAdapter))) {
				throw new GfxException("Couldn't retrieve DXGI adapter from DXGI device.");
			  }
			  CComPtr<IDXGIFactory> dxgiFactory;
			  if (!SUCCEEDED(dxgiAdapter->GetParent(__uuidof(IDXGIFactory),
													(void **)&dxgiFactory))) {
				throw new GfxException("Couldn't retrieve DXGI factory from DXGI adapter.");
			  }
			  mDxgiFactory = dxgiFactory; // Hang on to the DXGI factory used here

			  // Create 2D rendering
			  mImpl->textEngine = std::make_unique<TextEngine>(mD3d11Device, debugDevice);

			  if (windowHandle) {
				  memset(&mSwapChainDesc, 0, sizeof(mSwapChainDesc));
				  mSwapChainDesc.BufferCount = 2;
				  mSwapChainDesc.BufferDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
				  mSwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
				  mSwapChainDesc.OutputWindow = windowHandle;
				  mSwapChainDesc.SampleDesc.Count = 1;
				  mSwapChainDesc.Windowed =
					  TRUE; // As per the recommendation, we always create windowed

				  if (!SUCCEEDED(D3DLOG(dxgiFactory->CreateSwapChain(
					  mD3d11Device, &mSwapChainDesc, &mSwapChain)))) {
					  throw new GfxException("Unable to create swap chain");
				  }

				  // Get the backbuffer from the swap chain
				  CComPtr<ID3D11Texture2D> backBufferTexture;
				  D3DVERIFY(mSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D),
					  (void **)&backBufferTexture));

				  mBackBufferNew = CreateRenderTargetForNativeSurface(backBufferTexture);
				  var &backBufferSize = mBackBufferNew->GetSize();
				  mBackBufferDepthStencil = CreateRenderTargetDepthStencil(backBufferSize.width, backBufferSize.height);

				  // Push back the initial render target that should never be removed
				  PushBackBufferRenderTarget();
			  }

			  // Create centralized constant buffers for the vertex and pixel shader stages
			  mVsConstantBuffer = CreateConstantBuffer(nullptr, MaxVsConstantBufferSize);
			  mPsConstantBuffer = CreateConstantBuffer(nullptr, MaxPsConstantBufferSize);

			  // TODO: color bullshit is not yet done (tig_d3d_init_handleformat et al)

			  for (var &listener : mResourcesListeners) {
				listener->CreateResources(*this);
			  }
			  mResourcesCreated = true;
        }

        public bool BeginFrame()
        {

	        if (mBeginSceneDepth++ > 0) {
		        return true;
	        }

	        ClearCurrentColorTarget(XMCOLOR(0, 0, 0, 1));
	        ClearCurrentDepthTarget();

	        mLastFrameStart = Clock::now();

	        return true;
        }

        public bool Present()
        {

	        if (--mBeginSceneDepth > 0) {
		        return true;
	        }

	        PresentForce();

	        return true;
        }

        public void PresentForce()
        {
	        mTextures.FreeUnusedTextures();

	        D3DLOG(mSwapChain->Present(0, 0));
        }

        public void Flush()
        {
	        mContext->Flush();
        }

        public void ClearCurrentColorTarget(XMCOLOR color)
        {
	        var &target = GetCurrentRederTargetColorBuffer();

	        // Clear the current render target view
	        XMFLOAT4 clearColorVec;
	        XMStoreFloat4(&clearColorVec, XMLoadColor(&color));

	        mContext->ClearRenderTargetView(target->mRtView, &clearColorVec.x);
        }

        public void ClearCurrentDepthTarget(bool clearDepth = true,
	        bool clearStencil = true,
	        float depthValue = 1.0f,
	        uint8_t stencilValue = 0)
        {
	        if (!clearDepth && !clearStencil) {
		        return;
	        }

	        int flags = 0;
	        if (clearDepth) {
		        flags |= D3D11_CLEAR_DEPTH;
	        }
	        if (clearStencil) {
		        flags |= D3D11_CLEAR_STENCIL;
	        }

	        var &depthStencil = GetCurrentRenderTargetDepthStencilBuffer();

	        if (!depthStencil) {
		        _logger.warn(
			        "Trying to clear current depthstencil view, but none is bound.");
		        return;
	        }

	        mContext->ClearDepthStencilView(depthStencil->mDsView, flags, depthValue,
		        stencilValue);
        }

		using Clock = std::chrono::high_resolution_clock;
		using TimePoint = std::chrono::time_point<Clock>;

		public TimePoint GetLastFrameStart() const {
			return mLastFrameStart;
		}
		public TimePoint GetDeviceCreated() const {
			return mDeviceCreated;
		}

		public const eastl::vector<DisplayDevice> &GetDisplayDevices()
		{

  // Recreate the DXGI factory if we want to enumerate a new list of devices
  if (!mDisplayDevices.empty() && mDxgiFactory->IsCurrent()) {
    return mDisplayDevices;
  }

  // Enumerate devices
  _logger.info("Enumerating DXGI display devices...");

  mDisplayDevices.clear();

  for (uint adapterIdx = 0;; adapterIdx++) {
    CComPtr<IDXGIAdapter1> adapter;
    if (mDxgiFactory->EnumAdapters1(adapterIdx, &adapter) ==
        DXGI_ERROR_NOT_FOUND) {
      break;
    }

    // Get an adapter descriptor
    DXGI_ADAPTER_DESC1 adapterDesc;
    if (!SUCCEEDED(adapter->GetDesc1(&adapterDesc))) {
      _logger.warn("Unable to retrieve DXGI description of adapter #{}",
                   adapterIdx);
      continue;
    }

    DisplayDevice displayDevice;

    displayDevice.name = ucs2_to_local(&adapterDesc.Description[0]);
    _logger.info("Adapter #{} '{}'", adapterIdx, displayDevice.name);

    // Enumerate all outputs of the adapter
    for (uint outputIdx = 0;; outputIdx++) {
      CComPtr<IDXGIOutput> output;
      if (!SUCCEEDED(adapter->EnumOutputs(outputIdx, &output))) {
        break;
      }

      DXGI_OUTPUT_DESC outputDesc;
      if (!SUCCEEDED(D3DLOG(output->GetDesc(&outputDesc)))) {
        continue;
      }

      var deviceName = ucs2_to_local(&outputDesc.DeviceName[0]);

      MONITORINFOEXW monInfoEx;
      monInfoEx.cbSize = sizeof(MONITORINFOEXW);
      if (!GetMonitorInfoW(outputDesc.Monitor, &monInfoEx)) {
        _logger.warn("Could not get monitor info.");
        continue;
      }

      DISPLAY_DEVICEW dispDev;
      dispDev.cb = sizeof(DISPLAY_DEVICEW);
      if (!EnumDisplayDevicesW(monInfoEx.szDevice, 0, &dispDev, 0)) {
        _logger.warn("Could not enumerate display devices for monitor.");
        continue;
      }

      DisplayDeviceOutput displayOutput;
      displayOutput.id = deviceName;
      displayOutput.name = ucs2_to_local(&dispDev.DeviceString[0]);
      _logger.info("  Output #{} Device '{}' Monitor '{}'", outputIdx,
                   deviceName, displayOutput.name);
      displayDevice.outputs.emplace_back(std::move(displayOutput));
    }

    if (!displayDevice.outputs.empty()) {
      mDisplayDevices.emplace_back(std::move(displayDevice));
    } else {
      _logger.info("Skipping device because it has no outputs.");
    }
  }

  return mDisplayDevices;
		}

		// Resize the back buffer
		public void ResizeBuffers(int w, int h)
		{


			if (!mSwapChain) {
				return;
			}

			if (GetCurrentRederTargetColorBuffer() == mBackBufferNew) {
				mImpl->textEngine->SetRenderTarget(nullptr);
			}

			// Somewhat annoyingly, we have to release all existing buffers
			mBackBufferNew->mRtView.Release();
			mBackBufferNew->mTexture.Release();
			mBackBufferDepthStencil->mDsView.Release();
			mBackBufferDepthStencil->mTextureNew.Release();

			D3DVERIFY(mSwapChain->ResizeBuffers(0, 0, 0, DXGI_FORMAT_UNKNOWN, 0));

			// Get the backbuffer from the swap chain
			CComPtr<ID3D11Texture2D> backBufferTexture;
			D3DVERIFY(mSwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D),
				(void **)&backBufferTexture));

			// Create a render target view for rendering to the real backbuffer
			CComPtr<ID3D11RenderTargetView> backBufferView;
			CD3D11_RENDER_TARGET_VIEW_DESC rtvDesc(backBufferTexture.p,
				D3D11_RTV_DIMENSION_TEXTURE2D);
			D3DVERIFY(mD3d11Device->CreateRenderTargetView(backBufferTexture, &rtvDesc,
				&backBufferView));

			D3D11_TEXTURE2D_DESC backBufferDesc;
			backBufferTexture->GetDesc(&backBufferDesc);
			gfx::Size backBufferSize{ (int)backBufferDesc.Width,
				(int)backBufferDesc.Height };

			// Update the back buffer render target
			mBackBufferNew->mTexture = backBufferTexture;
			mBackBufferNew->mSize = backBufferSize;
			mBackBufferNew->mContentRect = { 0, 0, backBufferSize.width, backBufferSize.height };
			mBackBufferNew->mRtView = backBufferView;

			// This works because the actual dx11 surfaces are independently reference counted
			var newDs = CreateRenderTargetDepthStencil(backBufferSize.width, backBufferSize.height);
			mBackBufferDepthStencil->mDsView = newDs->mDsView;
			mBackBufferDepthStencil->mSize = newDs->mSize;
			mBackBufferDepthStencil->mTextureNew = newDs->mTextureNew;

			// Is the back buffer currently the active RT?
			if (GetCurrentRederTargetColorBuffer() == mBackBufferNew) {
				mRenderTargetStack.pop_back();
				PushBackBufferRenderTarget();
			}

			// Notice listeners about changed backbuffer size
			for (var &entry : mResizeListeners) {
				entry.second(w, h);
			}

		}

		public Material CreateMaterial(
			const BlendSpec &blendSpec,
			const DepthStencilSpec &depthStencilSpec,
			const RasterizerSpec &rasterizerSpec,
			const std::vector<MaterialSamplerSpec> &samplerSpecs,
			const VertexShaderPtr &vs,
			const PixelShaderPtr &ps
		)
		{


			var blendState = CreateBlendState(blendSpec);
			var depthStencilState = CreateDepthStencilState(depthStencilSpec);
			var rasterizerState = CreateRasterizerState(rasterizerSpec);

			std::vector<MaterialSamplerBinding> samplerBindings;
			samplerBindings.reserve(samplerSpecs.size());
			for (var &samplerSpec : samplerSpecs) {
				samplerBindings.push_back(
				{samplerSpec.texture, CreateSamplerState(samplerSpec.samplerSpec)});
			}

			return Material(blendState, depthStencilState, rasterizerState,
				samplerBindings, vs, ps);
		}

		private static D3D11_BLEND ConvertBlendOperand(BlendOperand op) {
			switch (op) {
				case BlendOperand::Zero:
					return D3D11_BLEND_ZERO;
				case BlendOperand::One:
					return D3D11_BLEND_ONE;
				case BlendOperand::SrcColor:
					return D3D11_BLEND_SRC_COLOR;
				case BlendOperand::InvSrcColor:
					return D3D11_BLEND_INV_SRC_COLOR;
				case BlendOperand::SrcAlpha:
					return D3D11_BLEND_SRC_ALPHA;
				case BlendOperand::InvSrcAlpha:
					return D3D11_BLEND_INV_SRC_ALPHA;
				case BlendOperand::DestAlpha:
					return D3D11_BLEND_DEST_ALPHA;
				case BlendOperand::InvDestAlpha:
					return D3D11_BLEND_INV_DEST_ALPHA;
				case BlendOperand::DestColor:
					return D3D11_BLEND_DEST_COLOR;
				case BlendOperand::InvDestColor:
					return D3D11_BLEND_INV_DEST_COLOR;
				default:
					throw new GfxException("Unknown blend operand.");
			}
		}

		public BlendStatePtr CreateBlendState(const BlendSpec &spec)
		{
			// Check if we have a matching state already
			var it = mImpl->blendStates.find(spec);

			if (it != mImpl->blendStates.end()) {
				return it->second;
			}

			CD3D11_BLEND_DESC blendDesc{CD3D11_DEFAULT()};

			var &targetDesc = blendDesc.RenderTarget[0];
			targetDesc.BlendEnable = spec.blendEnable ? TRUE : FALSE;
			targetDesc.SrcBlend = ConvertBlendOperand(spec.srcBlend);
			targetDesc.DestBlend = ConvertBlendOperand(spec.destBlend);
			targetDesc.SrcBlendAlpha = ConvertBlendOperand(spec.srcAlphaBlend);
			targetDesc.DestBlendAlpha = ConvertBlendOperand(spec.destAlphaBlend);

			uint8_t writeMask = 0;
			// Never overwrite the alpha channel with random stuff when blending is disabled
			if (spec.writeAlpha && targetDesc.BlendEnable) {
				writeMask |= D3D11_COLOR_WRITE_ENABLE_ALPHA;
			}
			if (spec.writeRed) {
				writeMask |= D3D11_COLOR_WRITE_ENABLE_RED;
			}
			if (spec.writeGreen) {
				writeMask |= D3D11_COLOR_WRITE_ENABLE_GREEN;
			}
			if (spec.writeBlue) {
				writeMask |= D3D11_COLOR_WRITE_ENABLE_BLUE;
			}
			targetDesc.RenderTargetWriteMask = writeMask;

			CComPtr<ID3D11BlendState> gpuState;
			D3DVERIFY(mD3d11Device->CreateBlendState(&blendDesc, &gpuState));

			var state = std::make_shared<BlendState>(spec, gpuState);
			mImpl->blendStates.insert({ spec, state });
			return state;
		}

		private static D3D11_COMPARISON_FUNC ConvertComparisonFunc(ComparisonFunc func) {
			switch (func) {
				case ComparisonFunc::Never:
					return D3D11_COMPARISON_NEVER;
				case ComparisonFunc::Less:
					return D3D11_COMPARISON_LESS;
				case ComparisonFunc::Equal:
					return D3D11_COMPARISON_EQUAL;
				case ComparisonFunc::LessEqual:
					return D3D11_COMPARISON_LESS_EQUAL;
				case ComparisonFunc::Greater:
					return D3D11_COMPARISON_GREATER;
				case ComparisonFunc::NotEqual:
					return D3D11_COMPARISON_NOT_EQUAL;
				case ComparisonFunc::GreaterEqual:
					return D3D11_COMPARISON_GREATER_EQUAL;
				case ComparisonFunc::Always:
					return D3D11_COMPARISON_ALWAYS;
				default:
					throw new GfxException("Unknown comparison func.");
			}
		}

		public DepthStencilStatePtr CreateDepthStencilState(const DepthStencilSpec &spec)
		{
			// Check if we have a matching state already
			var it = mImpl->depthStencilStates.find(spec);

			if (it != mImpl->depthStencilStates.end()) {
				return it->second;
			}

			CD3D11_DEPTH_STENCIL_DESC depthStencilDesc{CD3D11_DEFAULT()};
			depthStencilDesc.DepthEnable = spec.depthEnable ? TRUE : FALSE;
			depthStencilDesc.DepthWriteMask = spec.depthWrite
				? D3D11_DEPTH_WRITE_MASK_ALL
				: D3D11_DEPTH_WRITE_MASK_ZERO;

			depthStencilDesc.DepthFunc = ConvertComparisonFunc(spec.depthFunc);

			CComPtr<ID3D11DepthStencilState> gpuState;

			D3DVERIFY(
				mD3d11Device->CreateDepthStencilState(&depthStencilDesc, &gpuState));

			var state = std::make_shared<DepthStencilState>(spec, gpuState);
			mImpl->depthStencilStates.insert({ spec, state });
			return state;
		}

		public RasterizerStatePtr CreateRasterizerState(const RasterizerSpec &spec)
		{
			// Check if we have a matching state already
			var it = mImpl->rasterizerStates.find(spec);

			if (it != mImpl->rasterizerStates.end()) {
				return it->second;
			}

			CD3D11_RASTERIZER_DESC rasterizerDesc{CD3D11_DEFAULT()};
			if (spec.wireframe) {
				rasterizerDesc.FillMode = D3D11_FILL_WIREFRAME;
			}
			switch (spec.cullMode) {
				case CullMode::Back:
					rasterizerDesc.CullMode = D3D11_CULL_BACK;
					break;
				case CullMode::Front:
					rasterizerDesc.CullMode = D3D11_CULL_FRONT;
					break;
				case CullMode::None:
					rasterizerDesc.CullMode = D3D11_CULL_NONE;
					break;
			}

			rasterizerDesc.ScissorEnable = spec.scissor ? TRUE : FALSE;

			rasterizerDesc.MultisampleEnable = mImpl->antiAliasing ? TRUE : FALSE;

			CComPtr<ID3D11RasterizerState> gpuState;

			D3DVERIFY(mD3d11Device->CreateRasterizerState(&rasterizerDesc, &gpuState));

			var state = std::make_shared<RasterizerState>(spec, gpuState);
			mImpl->rasterizerStates.insert({spec, state});
			return state;
		}

		private static D3D11_TEXTURE_ADDRESS_MODE ConvertTextureAddress(TextureAddress address) {
			switch (address) {
				case TextureAddress::Clamp:
					return D3D11_TEXTURE_ADDRESS_CLAMP;
				case TextureAddress::Wrap:
					return D3D11_TEXTURE_ADDRESS_WRAP;
				default:
					throw new GfxException("Unknown texture address mode.");
			}
		}

		public SamplerStatePtr CreateSamplerState(const SamplerSpec &spec)
		{

			CD3D11_SAMPLER_DESC samplerDesc{CD3D11_DEFAULT()};

			// we only support mapping point + linear
			bool minPoint = (spec.minFilter == TextureFilterType::NearestNeighbor);
			bool magPoint = (spec.magFilter == TextureFilterType::NearestNeighbor);
			bool mipPoint = (spec.mipFilter == TextureFilterType::NearestNeighbor);

			// This is a truth table for all possible values represented above
			if (!minPoint && !magPoint && !mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
			} else if (!minPoint && !magPoint && mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT;
			} else if (!minPoint && magPoint && !mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_LINEAR_MAG_POINT_MIP_LINEAR;
			} else if (!minPoint && magPoint && mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_LINEAR_MAG_MIP_POINT;
			} else if (minPoint && !magPoint && !mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_POINT_MAG_MIP_LINEAR;
			} else if (minPoint && !magPoint && mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_POINT_MAG_LINEAR_MIP_POINT;
			} else if (minPoint && magPoint && !mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_MAG_POINT_MIP_LINEAR;
			} else if (minPoint && magPoint && mipPoint) {
				samplerDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
			}

			samplerDesc.AddressU = ConvertTextureAddress(spec.addressU);
			samplerDesc.AddressV = ConvertTextureAddress(spec.addressV);

			CComPtr<ID3D11SamplerState> gpuState;

			D3DVERIFY(mD3d11Device->CreateSamplerState(&samplerDesc, &gpuState));

			var state = std::make_shared<SamplerState>(spec, gpuState);
			mImpl->samplerStates.insert({ spec, state });
			return state;
		}

		// Changes the current scissor rect to the given rectangle
		public void SetScissorRect(int x, int y, int width, int height)
		{
			D3D11_RECT rect{ x, y, x + width, y + height };
			mContext->RSSetScissorRects(1, &rect);

			mImpl->textEngine->SetScissorRect({ x, y, width, height });
		}

		// Resets the scissor rect to the current render target's size
		public void ResetScissorRect()
		{
			var &size = mRenderTargetStack.back().colorBuffer->GetSize();
			D3D11_RECT rect{ 0, 0, size.width, size.height };
			mContext->RSSetScissorRects(1, &rect);

			mImpl->textEngine->ResetScissorRect();
		}

		public std::shared_ptr<class IndexBuffer> CreateEmptyIndexBuffer(int count)
		{

			CD3D11_BUFFER_DESC bufferDesc(
				count * sizeof(uint16_t),
			D3D11_BIND_INDEX_BUFFER,
			D3D11_USAGE_DYNAMIC,
			D3D11_CPU_ACCESS_WRITE
				);

			CComPtr<ID3D11Buffer> buffer;
			D3DVERIFY(mD3d11Device->CreateBuffer(&bufferDesc, nullptr, &buffer));

			return std::make_shared<IndexBuffer>(buffer, count);
		}
		public std::shared_ptr<class VertexBuffer> CreateEmptyVertexBuffer(int count, bool forPoints = false)
		{

			// Create a dynamic vertex buffer since it'll be updated (probably a lot)
			CD3D11_BUFFER_DESC bufferDesc(
				size,
				D3D11_BIND_VERTEX_BUFFER,
				D3D11_USAGE_DYNAMIC,
				D3D11_CPU_ACCESS_WRITE
			);

			CComPtr<ID3D11Buffer> buffer;
			D3DVERIFY(mD3d11Device->CreateBuffer(&bufferDesc, nullptr, &buffer));

			return std::make_shared<VertexBuffer>(buffer, size);
		}

		private static DXGI_FORMAT ConvertFormat(gfx::BufferFormat format, uint *bytesPerPixel) {
			DXGI_FORMAT formatNew;
			switch (format) {
				case BufferFormat::A8:
					formatNew = DXGI_FORMAT_R8_UNORM;
					*bytesPerPixel = 1;
					break;
				case BufferFormat::A8R8G8B8:
					formatNew = DXGI_FORMAT_B8G8R8A8_UNORM;
					*bytesPerPixel = 4;
					break;
				case BufferFormat::X8R8G8B8:
					formatNew = DXGI_FORMAT_B8G8R8X8_UNORM;
					*bytesPerPixel = 4;
					break;
				default:
					throw new GfxException("Unsupported format: {}", format);
			}
			return formatNew;
		}


		public DynamicTexturePtr CreateDynamicTexture(gfx::BufferFormat format, int width, int height)
		{
			Size size{width, height};

			uint bytesPerPixel;
			var formatNew = ConvertFormat(format, &bytesPerPixel);

			CD3D11_TEXTURE2D_DESC textureDesc(
				formatNew, width, height, 1, 1, D3D11_BIND_SHADER_RESOURCE,
				D3D11_USAGE_DYNAMIC, D3D11_CPU_ACCESS_WRITE);

			CComPtr<ID3D11Texture2D> textureNew;
			D3DVERIFY(mD3d11Device->CreateTexture2D(&textureDesc, nullptr, &textureNew));

			CD3D11_SHADER_RESOURCE_VIEW_DESC resourceViewDesc(
				textureNew, D3D11_SRV_DIMENSION_TEXTURE2D);
			CComPtr<ID3D11ShaderResourceView> resourceView;

			D3DVERIFY(mD3d11Device->CreateShaderResourceView(
				textureNew, &resourceViewDesc, &resourceView));

			return std::make_shared<DynamicTexture>(mContext, textureNew, resourceView,
				size, bytesPerPixel);
		}

		public DynamicTexturePtr CreateDynamicStagingTexture(gfx::BufferFormat format, int width, int height)
		{
			Size size{ width, height };

			uint bytesPerPixel;
			var formatNew = ConvertFormat(format, &bytesPerPixel);

			CD3D11_TEXTURE2D_DESC textureDesc(
				formatNew, width, height, 1, 1, 0,
				D3D11_USAGE_STAGING, D3D11_CPU_ACCESS_READ);

			CComPtr<ID3D11Texture2D> textureNew;
			D3DVERIFY(mD3d11Device->CreateTexture2D(&textureDesc, nullptr, &textureNew));

			return std::make_shared<DynamicTexture>(mContext, textureNew, nullptr,
				size, bytesPerPixel);
		}

		public void CopyRenderTarget(gfx::RenderTargetTexture &renderTarget, gfx::DynamicTexture &stagingTexture)
		{
			mContext->CopyResource(stagingTexture.mTexture, renderTarget.mTexture);
		}

		public RenderTargetTexturePtr CreateRenderTargetTexture(gfx::BufferFormat format, int width, int height,
			bool multiSampled = false)
		{

  Size size{width, height};

  uint bpp;
  var formatDx = ConvertFormat(format, &bpp);

  var bindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET;
  var sampleCount = 1;
  var sampleQuality = 0;

  if (multiSample) {
	  _logger.info("using multisampling");
	  // If this is a multi sample render target, we cannot use it as a texture, or at least, we shouldn't
	  bindFlags = D3D11_BIND_RENDER_TARGET;
	  sampleCount = mImpl->msaaSamples;
	  sampleQuality = mImpl->msaaQuality;
  } else
	  _logger.info("not using multisampling");

  _logger.info("width {} height {}", width, height);
  CD3D11_TEXTURE2D_DESC textureDesc(formatDx, width, height, 1, 1, bindFlags, D3D11_USAGE_DEFAULT, 0, sampleCount, sampleQuality);

  CComPtr<ID3D11Texture2D> texture;
  D3DVERIFY(mD3d11Device->CreateTexture2D(&textureDesc, nullptr, &texture));

  // Create the render target view of the backing buffer
  CComPtr<ID3D11RenderTargetView> rtView;
  CD3D11_RENDER_TARGET_VIEW_DESC rtViewDesc(texture, D3D11_RTV_DIMENSION_TEXTURE2D);

  if (multiSample) {
	  rtViewDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2DMS;
  }

  D3DVERIFY(mD3d11Device->CreateRenderTargetView(texture, &rtViewDesc, &rtView));

  ID3D11Texture2D *srvTexture = texture;
  CComPtr<ID3D11Texture2D> resolvedTexture;
  if (multiSample) {
	  // We have to create another non-multisampled texture and use it for the SRV instead

	  // Adapt the existing texture Desc to be a non-MSAA texture with otherwise identical properties
	  textureDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	  textureDesc.SampleDesc.Count = 1;
	  textureDesc.SampleDesc.Quality = 0;

	  D3DVERIFY(mD3d11Device->CreateTexture2D(&textureDesc, nullptr, &resolvedTexture));
	  srvTexture = resolvedTexture;
  }

  CD3D11_SHADER_RESOURCE_VIEW_DESC resourceViewDesc(texture, D3D11_SRV_DIMENSION_TEXTURE2D);
  CComPtr<ID3D11ShaderResourceView> resourceView;
  D3DVERIFY(mD3d11Device->CreateShaderResourceView(srvTexture, &resourceViewDesc, &resourceView));

  return std::make_shared<RenderTargetTexture>(texture, rtView, resolvedTexture, resourceView, size, multiSample);
		}

		public RenderTargetTexturePtr CreateRenderTargetForNativeSurface(ID3D11Texture2D* surface)
		{
			// Create a render target view for rendering to the real backbuffer
			CComPtr<ID3D11RenderTargetView> backBufferView;
			CD3D11_RENDER_TARGET_VIEW_DESC rtvDesc(surface, D3D11_RTV_DIMENSION_TEXTURE2D);
			D3DVERIFY(mD3d11Device->CreateRenderTargetView(surface, &rtvDesc, &backBufferView));

			D3D11_TEXTURE2D_DESC backBufferDesc;
			surface->GetDesc(&backBufferDesc);

			gfx::Size size{ (int)backBufferDesc.Width, (int)backBufferDesc.Height };
			return std::make_shared<RenderTargetTexture>(surface,
				backBufferView,
				nullptr,
				nullptr,
				size,
				false);
		}

		public RenderTargetTexturePtr CreateRenderTargetForSharedSurface(IUnknown* surface)
		{
			CComPtr<IDXGIResource> dxgiResource;

			if (FAILED(CComPtr<IUnknown>(surface).QueryInterface(&dxgiResource))) {
				return nullptr;
			}

			HANDLE sharedHandle;
			if (FAILED(dxgiResource->GetSharedHandle(&sharedHandle))) {
				return nullptr;
			}

			dxgiResource.Release();

			CComPtr<ID3D11Resource> sharedResource;
			D3DVERIFY(mD3d11Device->OpenSharedResource(sharedHandle, __uuidof(ID3D11Resource), (void**)(&sharedResource)));

			CComPtr<ID3D11Texture2D> sharedTexture;
			D3DVERIFY(sharedResource.QueryInterface(&sharedTexture));

			return CreateRenderTargetForNativeSurface(sharedTexture);
		}

		public RenderTargetDepthStencilPtr CreateRenderTargetDepthStencil(int width, int height,
			bool multiSampled = false)
		{
			CD3D11_TEXTURE2D_DESC descDepth(DXGI_FORMAT_D24_UNORM_S8_UINT, width, height,
				1U,
				1U, // Disable Mip Map generation
				D3D11_BIND_DEPTH_STENCIL);

			// Enable multi sampling
			if (multiSample) {
				descDepth.SampleDesc.Count = mImpl->msaaSamples;
				descDepth.SampleDesc.Quality = mImpl->msaaQuality;
			}

			CComPtr<ID3D11Texture2D> texture;
			D3DVERIFY(mD3d11Device->CreateTexture2D(&descDepth, nullptr, &texture));

			// Create the depth stencil view
			CD3D11_DEPTH_STENCIL_VIEW_DESC descDSV(D3D11_DSV_DIMENSION_TEXTURE2D, descDepth.Format);

			if (multiSample) {
				descDSV.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2DMS;
			}

			CComPtr<ID3D11DepthStencilView> depthStencilView;
			D3DVERIFY(mD3d11Device->CreateDepthStencilView(texture, &descDSV, &depthStencilView));

			Size size{width, height};
			return std::make_shared<RenderTargetDepthStencil>(texture, depthStencilView, size);
		}

		public VertexBufferPtr CreateVertexBuffer<T>(gsl::span<T> data, bool immutable = true)
		{
			return CreateVertexBufferRaw(gsl::span(reinterpret_cast<const uint8_t*>(&data[0]), data.size_bytes()), immutable);
		}
		public VertexBufferPtr CreateVertexBufferRaw(gsl::span<const uint8_t> data, bool immutable = true)
		{

			// Create a dynamic or immutable vertex buffer depending on the immutable flag
			CD3D11_BUFFER_DESC bufferDesc(
				data.size_bytes(),
			D3D11_BIND_VERTEX_BUFFER,
			immutable ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DYNAMIC,
			immutable ? 0 : D3D11_CPU_ACCESS_WRITE
				);

			D3D11_SUBRESOURCE_DATA initData;
			initData.pSysMem = data.data();

			CComPtr<ID3D11Buffer> buffer;
			D3DVERIFY(mD3d11Device->CreateBuffer(&bufferDesc, &initData, &buffer));

			return std::make_shared<VertexBuffer>(buffer, data.size());
		}
		public IndexBufferPtr CreateIndexBuffer(gsl::span<const uint16_t> data, bool immutable = true)
		{
			CD3D11_BUFFER_DESC bufferDesc(
				data.size_bytes(),
			D3D11_BIND_INDEX_BUFFER,
			immutable ? D3D11_USAGE_IMMUTABLE : D3D11_USAGE_DYNAMIC,
			immutable ? 0 : D3D11_CPU_ACCESS_WRITE
				);

			D3D11_SUBRESOURCE_DATA subresourceData;
			subresourceData.pSysMem = data.data();

			CComPtr<ID3D11Buffer> buffer;
			D3DVERIFY(mD3d11Device->CreateBuffer(&bufferDesc, &subresourceData, &buffer));

			return std::make_shared<IndexBuffer>(buffer, data.size());
		}

		public void SetMaterial(const Material &material)
		{
			SetRasterizerState(material.GetRasterizerState());
			SetBlendState(material.GetBlendState());
			SetDepthStencilState(material.GetDepthStencilState());

			for (int i = 0; i < material.GetSamplers().size(); ++i) {
				var &sampler = material.GetSamplers()[i];
				if (sampler.GetTexture()) {
					SetTexture(i, *sampler.GetTexture());
				} else {
					SetTexture(i, *gfx::Texture::GetInvalidTexture());
				}
				SetSamplerState(i, sampler.GetState());
			}

			// Free up the texture bindings of the samplers currently being used
			for (int i = material.GetSamplers().size(); i < mUsedSamplers; ++i) {
				SetTexture(i, *gfx::Texture::GetInvalidTexture());
			}

			mUsedSamplers = material.GetSamplers().size();

			material.GetVertexShader()->Bind();
			material.GetPixelShader()->Bind();
		}

		public void SetVertexShaderConstant(uint startRegister, StandardSlotSemantic semantic)
		{
			switch (semantic) {
				case StandardSlotSemantic::ViewProjMatrix:
					SetVertexShaderConstants(startRegister, mCamera.GetViewProj());
					break;
				case StandardSlotSemantic::UiProjMatrix:
					SetVertexShaderConstants(startRegister, mCamera.GetUiProjection());
				default:
					break;
			}
		}

		public void SetPixelShaderConstant(uint startRegister, StandardSlotSemantic semantic)
		{
			switch (semantic) {
				case StandardSlotSemantic::ViewProjMatrix:
					SetPixelShaderConstants(startRegister, mCamera.GetViewProj());
					break;
				case StandardSlotSemantic::UiProjMatrix:
					SetPixelShaderConstants(startRegister, mCamera.GetUiProjection());
					break;
				default:
					break;
			}
		}

		public void SetRasterizerState(const RasterizerState &state)
		{
			if (mImpl->currentRasterizerState == &state) {
				return; // Already set
			}
			mImpl->currentRasterizerState = &state;
			mContext->RSSetState(state.mGpuState);
		}
		public void SetBlendState(const BlendState &state)
		{
			if (mImpl->currentBlendState == &state) {
				return; // Already set
			}
			mImpl->currentBlendState = &state;
			mContext->OMSetBlendState(state.mGpuState, nullptr, 0xFFFFFFFF);
		}
		public void SetDepthStencilState(const DepthStencilState &state)
		{
			if (mImpl->currentDepthStencilState == &state) {
				return; // Already set
			}
			mImpl->currentDepthStencilState = &state;
			mContext->OMSetDepthStencilState(state.mGpuState, 0);
		}
		public void SetSamplerState(int samplerIdx, const SamplerState &state)
		{
			var &curSampler = mImpl->currentSamplerState[samplerIdx];
			if (curSampler == &state) {
				return; // Already set
			}
			curSampler = &state;

			ID3D11SamplerState *sampler = state.mGpuState;
			mContext->PSSetSamplers(samplerIdx, 1, &sampler);
		}

		public void SetTexture(uint slot, gfx::Texture &texture)
		{
			// If we are binding a multisample render target, we automatically resolve the MSAA to use
			// a non-MSAA texture like a normal texture
			if (texture.GetType() == TextureType::RenderTarget) {
				var &rt = static_cast<RenderTargetTexture&>(texture);

				if (rt.IsMultiSampled()) {
					D3D11_TEXTURE2D_DESC mDesc;
					rt.mTexture->GetDesc(&mDesc);

					mContext->ResolveSubresource(rt.mResolvedTexture,
						0,
						rt.mTexture,
						0,
						mDesc.Format
					);
				}
			}

			// D3D11
			var resourceView = texture.GetResourceView();
			mContext->PSSetShaderResources(slot, 1, &resourceView);
		}

		public void SetIndexBuffer(const gfx::IndexBuffer &indexBuffer)
		{
			mContext->IASetIndexBuffer(indexBuffer.mBuffer, DXGI_FORMAT_R16_UINT, 0);
		}

		public void Draw(PrimitiveType type, uint vertexCount, uint startVertex = 0)
		{
			D3D11_PRIMITIVE_TOPOLOGY primTopology;

			switch (type) {
				case PrimitiveType::TriangleStrip:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP;
					break;
				case PrimitiveType::TriangleList:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
					break;
				case PrimitiveType::LineStrip:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_LINESTRIP;
					break;
				case PrimitiveType::LineList:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_LINELIST;
					break;
				case PrimitiveType::PointList:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_POINTLIST;
					break;
				default:
					throw new GfxException("Unsupported primitive type");
			}

			mContext->IASetPrimitiveTopology(primTopology);
			mContext->Draw(vertexCount, startVertex);
		}

		public void DrawIndexed(PrimitiveType type, uint vertexCount, uint indexCount, uint startVertex = 0,
			uint vertexBase = 0)
		{

			D3D11_PRIMITIVE_TOPOLOGY primTopology;

			switch (type) {
				case PrimitiveType::TriangleStrip:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP;
					break;
				case PrimitiveType::TriangleList:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
					break;
				case PrimitiveType::LineStrip:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_LINESTRIP;
					break;
				case PrimitiveType::LineList:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_LINELIST;
					break;
				case PrimitiveType::PointList:
					primTopology = D3D11_PRIMITIVE_TOPOLOGY_POINTLIST;
					break;
				default:
					throw new GfxException("Unsupported primitive type");
			}

			mContext->IASetPrimitiveTopology(primTopology);
			mContext->DrawIndexed(indexCount, startVertex, vertexBase);
		}

		/*
		  Changes the currently used cursor to the given surface.
		 */
		public void SetCursor(int hotspotX, int hotspotY, const gfx::TextureRef &texture)
		{
			HCURSOR cursor;
			var it = mImpl->cursorCache.find(texture->GetName());
			if (it == mImpl->cursorCache.end()) {
				var textureData = vfs->ReadAsBinary(texture->GetName());
				cursor = gfx::LoadImageToCursor(textureData, hotspotX, hotspotY);
				mImpl->cursorCache[texture->GetName()] = cursor;
			} else {
				cursor = it->second;
			}

			SetClassLong(mWindowHandle, GCL_HCURSOR, (LONG)cursor);
				::SetCursor(cursor);
			mImpl->currentCursor = cursor;
		}

		public void ShowCursor()
		{
			if (mImpl->currentCursor) {
				SetClassLong(mWindowHandle, GCL_HCURSOR, (LONG)mImpl->currentCursor);
					::SetCursor(mImpl->currentCursor);
			}
		}

		public void HideCursor()
		{
			SetClassLong(mWindowHandle, GCL_HCURSOR, (LONG)nullptr);
				::SetCursor(nullptr);
		}

		/*
			Take a screenshot with the given size. The image will be stretched
			to the given size.
		*/
		public void TakeScaledScreenshot(const std::string& filename, int width, int height, int quality = 90)
		{

		  _logger.debug("Creating screenshot with size {}x{} in {}", width, height,
						filename);

		  var &currentTarget = GetCurrentRederTargetColorBuffer();
		  var targetSize = currentTarget->GetSize();

		  // Support taking unscaled screenshots
		  var stretch = true;
		  if (width == 0 || height == 0) {
			width = targetSize.width;
			height = targetSize.height;
			stretch = false;
		  }

		  // Retrieve the backbuffer format...
		  D3D11_TEXTURE2D_DESC currentTargetDesc;
		  currentTarget->mTexture->GetDesc(&currentTargetDesc);

		  // Create a staging surface for copying pixels back from the backbuffer
		  // texture
		  D3D11_TEXTURE2D_DESC stagingDesc = currentTargetDesc;
		  stagingDesc.Width = width;
		  stagingDesc.Height = height;
		  stagingDesc.Usage = D3D11_USAGE_STAGING;
		  stagingDesc.BindFlags = 0; // Not going to bind it at all
		  stagingDesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
		  stagingDesc.MipLevels = 1;
		  stagingDesc.ArraySize = 1;
		  // Never use multi sampling for the screenshot
		  stagingDesc.SampleDesc.Count = 1;
		  stagingDesc.SampleDesc.Quality = 0;

		  CComPtr<ID3D11Texture2D> stagingTex;
		  D3DVERIFY(mD3d11Device->CreateTexture2D(&stagingDesc, nullptr, &stagingTex));

		  if (stretch) {
			  // Create a default texture to copy the current RT to that we can use as a src for the blitting
			  CD3D11_TEXTURE2D_DESC tmpDesc(currentTargetDesc);
			  // Force MSAA off
			  tmpDesc.SampleDesc.Count = 1;
			  tmpDesc.SampleDesc.Quality = 0;
			  // Make it a default texture with binding as Shader Resource
			  tmpDesc.Usage = D3D11_USAGE_DEFAULT;
			  tmpDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;

			  CComPtr<ID3D11Texture2D> tmpTexture;
			  D3DVERIFY(mD3d11Device->CreateTexture2D(&tmpDesc, nullptr, &tmpTexture));

			  // Copy/resolve the current RT into the temp texture
			  if (currentTargetDesc.SampleDesc.Count > 1) {
				  mContext->ResolveSubresource(tmpTexture, 0, currentTarget->mTexture, 0, tmpDesc.Format);
			  } else {
				  mContext->CopyResource(tmpTexture, currentTarget->mTexture);
			  }

			  // Create the Shader Resource View that we can use to use the tmp texture for sampling in a shader
			  CD3D11_SHADER_RESOURCE_VIEW_DESC srvDesc(D3D11_SRV_DIMENSION_TEXTURE2D);
			  CComPtr<ID3D11ShaderResourceView> srv;
			  D3DVERIFY(mD3d11Device->CreateShaderResourceView(tmpTexture, &srvDesc, &srv));

			  // Create our own wrapper so we can use the standard rendering functions
			  Size tmpSize { (int) currentTargetDesc.Width, (int) currentTargetDesc.Height };
			  DynamicTexture tmpTexWrapper{ mContext,
				  tmpTexture,
				  srv,
				  tmpSize,
				  4 };

			  // Create a texture the size of the target and stretch into it via a blt
			  // the target also needs to be a render target for that to work
			  var stretchedRt = CreateRenderTargetTexture(currentTarget->GetFormat(), width, height);

			  PushRenderTarget(stretchedRt, nullptr);
			  ShapeRenderer2d renderer(*this);

			  var w = (float)mCamera.GetScreenWidth();
			  var h = (float)mCamera.GetScreenHeight();
			  renderer.DrawRectangle(0, 0, w, h, tmpTexWrapper);

			  PopRenderTarget();

			  // Copy our stretchted RT to the staging resource
			  mContext->CopyResource(stagingTex, stretchedRt->mTexture);

		  } else {
			  // Resolve multi sampling if necessary
			  if (currentTargetDesc.SampleDesc.Count > 1) {
				  mContext->ResolveSubresource(stagingTex, 0, currentTarget->mTexture, 0, stagingDesc.Format);
			  }
			  else {
				  mContext->CopyResource(stagingTex, currentTarget->mTexture);
			  }
		  }

		  // Lock the resource and retrieve it
		  D3D11_MAPPED_SUBRESOURCE mapped;
		  D3DVERIFY(mContext->Map(stagingTex, 0, D3D11_MAP_READ, 0, &mapped));

		  // Clamp quality to [1, 100]
		  quality = std::min(100, std::max(1, quality));

		  var jpegData(gfx::EncodeJpeg(reinterpret_cast<uint8_t *>(mapped.pData),
										gfx::JpegPixelFormat::BGRX, width, height,
										quality, mapped.RowPitch));

		  mContext->Unmap(stagingTex, 0);

		  // We have to write using tio or else it goes god knows where
		  try {
			vfs->WriteBinaryFile(filename, jpegData);
		  } catch (std::exception &e) {
			_logger.Error("Unable to save screenshot due to an IO error: {}", e.what());
		  }
		}

		// Creates a buffer binding for a MDF material that
		// is preinitialized with the correct shader
		public BufferBinding CreateMdfBufferBinding()
		{
			var &vs = GetShaders().LoadVertexShader(
				"mdf_vs",
			{
				{"TEXTURE_STAGES", "1"} // Necessary so the input struct gets the UVs
			});

			return BufferBinding(vs);
		}

		public Shaders& GetShaders() {
			return mShaders;
		}

		public Textures& GetTextures() {
			return mTextures;
		}

		public WorldCamera& GetCamera() {
			return mCamera;
		}

		public void SetAntiAliasing(bool enable, uint samples, uint quality)
		{

			mImpl->msaaQuality = quality;
			mImpl->msaaSamples = samples;

			if (mImpl->antiAliasing != enable) {
				mImpl->antiAliasing = enable;

				// Recreate all rasterizer states to set the multisampling flag accordingly
				for (var &entry : mImpl->rasterizerStates) {
					var gpuState = entry.second->mGpuState;
					D3D11_RASTERIZER_DESC gpuDesc;
					gpuState->GetDesc(&gpuDesc);

					gpuDesc.MultisampleEnable = enable ? TRUE : FALSE;

					entry.second->mGpuState.Release();
					mD3d11Device->CreateRasterizerState(&gpuDesc, &entry.second->mGpuState);
				}
			}
		}

		public void UpdateBuffer<T>(VertexBuffer &buffer, gsl::span<T> data) {
			UpdateBuffer(buffer, data.data(), data.size_bytes());
		}

		public void UpdateBuffer(VertexBuffer &buffer, const void *data, int size)
		{
			UpdateResource(buffer.mBuffer, data, size);
		}

		public void UpdateBuffer(IndexBuffer &buffer, gsl::span<uint16_t> data)
		{
			UpdateResource(buffer.mBuffer, data.data(), data.size_bytes());
		}

		private static D3D11_MAP ConvertMapMode(gfx::MapMode mapMode) {
			switch (mapMode) {
				case gfx::MapMode::Read:
					return D3D11_MAP_READ;
				case gfx::MapMode::Discard:
					return D3D11_MAP_WRITE_DISCARD;
				case gfx::MapMode::NoOverwrite:
					return D3D11_MAP_WRITE_NO_OVERWRITE;
				default:
					throw new GfxException("Unknown map type");
			}
		}

		public MappedVertexBuffer<TElement> Map<TElement>(VertexBuffer &buffer, gfx::MapMode mode = gfx::MapMode::Discard) {
			var data = MapVertexBufferRaw(buffer, mode);
			var castData = gsl::span<TElement>((TElement*)data.data(), data.size() / sizeof(TElement));
			return MappedVertexBuffer<TElement>(buffer, *this, castData, 0);
		}

		public void Unmap(VertexBuffer &buffer)
		{
			mContext->Unmap(buffer.mBuffer, 0);
		}

		// Index buffer memory mapping techniques
		public MappedIndexBuffer Map(IndexBuffer &buffer, gfx::MapMode mode = gfx::MapMode::Discard)
		{
			var mapMode = ConvertMapMode(mode);

			D3D11_MAPPED_SUBRESOURCE mapped;
			D3DVERIFY(mContext->Map(buffer.mBuffer, 0, mapMode, 0, &mapped));
			var data = gsl::span((uint16_t *)mapped.pData, buffer.mCount);

			return MappedIndexBuffer(buffer, *this, data, 0);
		}

		public void Unmap(IndexBuffer &buffer)
		{
			mContext->Unmap(buffer.mBuffer, 0);
		}

		public MappedTexture Map(DynamicTexture &texture, gfx::MapMode mode = gfx::MapMode::Discard)
		{
			var mapMode = ConvertMapMode(mode);

			D3D11_MAPPED_SUBRESOURCE mapped;
			D3DVERIFY(mContext->Map(texture.mTexture, 0, mapMode, 0, &mapped));
			var size = texture.GetSize().width * texture.GetSize().height *
			            texture.GetBytesPerPixel();
			var data = gsl::span((uint8_t *)mapped.pData, size);
			var rowPitch = mapped.RowPitch;

			return MappedTexture(texture, *this, data, rowPitch);
		}

		public void Unmap(DynamicTexture &texture)
		{
			mContext->Unmap(texture.mTexture, 0);
		}

		public static constexpr uint MaxVsConstantBufferSize = 2048;

		public void SetVertexShaderConstants<T>(uint slot, const T &buffer) {
			static_assert(sizeof(T) <= MaxVsConstantBufferSize, "Constant buffer exceeds maximum size");
			UpdateResource(mVsConstantBuffer, &buffer, sizeof(T));
			VSSetConstantBuffer(slot, mVsConstantBuffer);
		}

		public static constexpr uint MaxPsConstantBufferSize = 512;

		public void SetPixelShaderConstants<T>(uint slot, const T &buffer) {
			static_assert(sizeof(T) <= MaxPsConstantBufferSize, "Constant buffer exceeds maximum size");
			UpdateResource(mPsConstantBuffer, &buffer, sizeof(T));
			PSSetConstantBuffer(slot, mPsConstantBuffer);
		}

		public const gfx::Size &GetBackBufferSize() const
		{
			return mBackBufferNew->GetSize();
		}

		// Pushes the back buffer and it's depth buffer as the current render target
		public void PushBackBufferRenderTarget()
		{
			PushRenderTarget(mBackBufferNew, mBackBufferDepthStencil);
		}
		public void PushRenderTarget(
			const gfx::RenderTargetTexturePtr &colorBuffer,
			const gfx::RenderTargetDepthStencilPtr &depthStencilBuffer
		)
		{
			// If a depth stencil surface is to be used, it HAS to be the same size
			assert(!depthStencilBuffer ||
			       colorBuffer->GetSize() == depthStencilBuffer->GetSize());

			// Set the camera size to the size of the new render target
			var &size = colorBuffer->GetSize();
			mCamera.SetScreenWidth((float)size.width, (float)size.height);

			// Activate the render target on the device
			var rtv = colorBuffer->mRtView;
			ID3D11DepthStencilView *depthStencilView = nullptr; // Optional!
			if (depthStencilBuffer) {
				depthStencilView = depthStencilBuffer->mDsView;
			}

			mContext->OMSetRenderTargets(1, &rtv.p, depthStencilView);
			mImpl->textEngine->SetRenderTarget(colorBuffer->mTexture);

			// Set the viewport accordingly
			CD3D11_VIEWPORT viewport(0.0f, 0.0f, (float)size.width, (float)size.height);
			mContext->RSSetViewports(1, &viewport);

			mRenderTargetStack.push_back({colorBuffer, depthStencilBuffer});

			ResetScissorRect();
		}

		public void PopRenderTarget()
		{
			// The last targt should NOT be popped, if the backbuffer was var-pushed
			if (mBackBufferNew) {
				assert(mRenderTargetStack.size() > 1);
			}

			mRenderTargetStack.pop_back(); // Remove ref to last target

			if (mRenderTargetStack.empty()) {
				mContext->OMSetRenderTargets(0, nullptr, nullptr);
				mImpl->textEngine->SetRenderTarget(nullptr);
				return;
			}

			var &newTarget = mRenderTargetStack.back();

			// Set the camera size to the size of the new render target
			var &size = newTarget.colorBuffer->GetSize();
			mCamera.SetScreenWidth((float)size.width, (float)size.height);

			// Activate the render target on the device
			var rtv = newTarget.colorBuffer->mRtView;
			ID3D11DepthStencilView *depthStencilView = nullptr; // Optional!
			if (newTarget.depthStencilBuffer) {
				depthStencilView = newTarget.depthStencilBuffer->mDsView;
			}

			mContext->OMSetRenderTargets(1, &rtv.p, depthStencilView);
			mImpl->textEngine->SetRenderTarget(newTarget.colorBuffer->mTexture);

			// Set the viewport accordingly
			CD3D11_VIEWPORT viewport(0.0f, 0.0f, (float)size.width, (float)size.height);
			mContext->RSSetViewports(1, &viewport);

			ResetScissorRect();
		}
		public const gfx::RenderTargetTexturePtr &GetCurrentRederTargetColorBuffer() const {
			return mRenderTargetStack.back().colorBuffer;
		}
		public const gfx::RenderTargetDepthStencilPtr &GetCurrentRenderTargetDepthStencilBuffer() const {
			return mRenderTargetStack.back().depthStencilBuffer;
		}

		public ResizeListenerRegistration AddResizeListener(ResizeListener listener)
		{

			var newKey = ++mResizeListenersKey;
			mResizeListeners[newKey] = listener;
			return ResizeListenerRegistration(*this, newKey);
		}

		public bool IsDebugDevice() const
		{
			return mImpl->debugDevice;
		}

		/**
		 * Emits the start of a rendering call group if the debug device is being used.
		 * This information can be used in the graphic debugger.
		 */
		public void BeginPerfGroup(string format, params object[] args) {
			if (IsDebugDevice()) {
				BeginPerfGroupInternal(fmt::format(format, args...).c_str());
			}
		}
		/**
		 * Ends a previously started performance group.
		 */
		public void EndPerfGroup() const
		{
			if (mImpl->debugDevice && mImpl->annotation) {
				mImpl->annotation->EndEvent();
			}
		}

		public TextEngine& GetTextEngine() const
		{
			return *mImpl->textEngine;
		}

        public void Dispose()
        {
	        mDxgiFactory?.Dispose();
	        mDxgiFactory = null;
        }

        private void BeginPerfGroupInternal(string message)
        {
	        if (mImpl->annotation) {
		        mImpl->annotation->BeginEvent(local_to_ucs2(msg).c_str());
	        }
        }

        private void RemoveResizeListener(uint key)
        {
	        mResizeListeners.erase(key);
        }

        private void AddResourceListener(ResourceListener* resourceListener)
        {
	        mResourcesListeners.push_back(listener);
	        if (mResourcesCreated) {
		        listener->CreateResources(*this);
	        }
        }

        private void RemoveResourceListener(ResourceListener* resourceListener)
        {
	        mResourcesListeners.remove(listener);
	        if (mResourcesCreated) {
		        listener->FreeResources(*this);
	        }
        }

		private void UpdateResource(ID3D11Resource *resource, const void *data, int size)
		{

			D3D11_MAPPED_SUBRESOURCE mapped;
			D3DVERIFY(mContext->Map(resource, 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped));

			memcpy(mapped.pData, data, size);

			mContext->Unmap(resource, 0);
		}
		private CComPtr<ID3D11Buffer> CreateConstantBuffer(const void *initialData, int initialDataSize)
		{
			CD3D11_BUFFER_DESC bufferDesc(initialDataSize, D3D11_BIND_CONSTANT_BUFFER,
				D3D11_USAGE_DYNAMIC, D3D11_CPU_ACCESS_WRITE);

			D3D11_SUBRESOURCE_DATA *ptrSubresourceData = nullptr;
			D3D11_SUBRESOURCE_DATA subresourceData;

			if (initialData) {
				ptrSubresourceData = &subresourceData;
				subresourceData.pSysMem = initialData;
			}

			CComPtr<ID3D11Buffer> buffer;
			D3DVERIFY(
				mD3d11Device->CreateBuffer(&bufferDesc, ptrSubresourceData, &buffer));
			return buffer;
		}

		private void VSSetConstantBuffer(uint slot, ID3D11Buffer* buffer)
		{
			mContext->VSSetConstantBuffers(slot, 1, &buffer);
		}

		private void PSSetConstantBuffer(uint slot, ID3D11Buffer* buffer)
		{

			mContext->PSSetConstantBuffers(slot, 1, &buffer);
		}

		private gsl::span<uint8_t> MapVertexBufferRaw(VertexBuffer &buffer, MapMode mode)
		{
			var mapMode = ConvertMapMode(mode);

			D3D11_MAPPED_SUBRESOURCE mapped;
			D3DVERIFY(mContext->Map(buffer.mBuffer, 0, mapMode, 0, &mapped));

			return gsl::span((uint8_t *)mapped.pData, buffer.mSize);
		}

		private CComPtr<IDXGIAdapter1> GetAdapter(int index)
		{
			CComPtr<IDXGIAdapter1> adapter;
			mDxgiFactory->EnumAdapters1(index, &adapter);
			return adapter;
		}

		private int mBeginSceneDepth = 0;

		private HWND mWindowHandle;

		private Factory1 mDxgiFactory;

		// The DXGI adapter we use
		private CComPtr<IDXGIAdapter1> mAdapter;

		// D3D11 device and related
		internal SharpDX.Direct3D11.Device mD3d11Device;
		private SharpDX.Direct3D11.Device1 mD3d11Device1;
		private DXGI_SWAP_CHAIN_DESC mSwapChainDesc;
		private SharpDX.DXGI.SwapChain mSwapChain;
		internal SharpDX.Direct3D11.DeviceContext mContext;
		private gfx::RenderTargetTexturePtr mBackBufferNew;
		private gfx::RenderTargetDepthStencilPtr mBackBufferDepthStencil;

		struct RenderTarget {
			gfx::RenderTargetTexturePtr colorBuffer;
			gfx::RenderTargetDepthStencilPtr depthStencilBuffer;
		};
		private eastl::fixed_vector<RenderTarget, 16> mRenderTargetStack;

		private D3D_FEATURE_LEVEL mFeatureLevel = D3D_FEATURE_LEVEL_9_1;

		private eastl::vector<DisplayDevice> mDisplayDevices;

		private CComPtr<ID3D11Buffer> mVsConstantBuffer;
		private CComPtr<ID3D11Buffer> mPsConstantBuffer;

		private eastl::map<uint, ResizeListener> mResizeListeners;
		private uint mResizeListenersKey = 0;

		private std::list<ResourceListener*> mResourcesListeners;
		private bool mResourcesCreated = false;


		private TimePoint mLastFrameStart = Clock::now();
		private TimePoint mDeviceCreated = Clock::now();

		private int mUsedSamplers = 0;

		private Shaders mShaders;
		private Textures mTextures;
		private WorldCamera mCamera;

		// Anti Aliasing Settings
		private bool antiAliasing = false;
		private uint msaaSamples = 4;
		private uint msaaQuality = 0;

		// Caches for cursors
		private eastl::map<std::string, HCURSOR> cursorCache;
		private HCURSOR currentCursor = nullptr;

		// Caches for created device states
		private std::array<const SamplerState*, 4> currentSamplerState;
		private eastl::map<SamplerSpec, SamplerStatePtr> samplerStates;

		private const DepthStencilState *currentDepthStencilState = nullptr;
		private eastl::map<DepthStencilSpec, DepthStencilStatePtr> depthStencilStates;

		private const BlendState* currentBlendState = nullptr;
		private eastl::map<BlendSpec, BlendStatePtr> blendStates;

		private const RasterizerState *currentRasterizerState = nullptr;
		private eastl::map<RasterizerSpec, RasterizerStatePtr> rasterizerStates;

		// Debugging related
		private bool debugDevice = false;
		private CComPtr<ID3DUserDefinedAnnotation> annotation;

		// Text rendering (Direct2D integration)
		private std::unique_ptr<TextEngine> textEngine;

		internal void FreeResource(GpuResource resource)
		{
			// TODO: Kill it
		}

    }
}