
#ifdef D3D11
sampler texSampler;
Texture2D texture1 : register(t0);
#else
sampler texSampler : register(s0);
#endif

static const float Pixels[13] =
{
   -6,
   -5,
   -4,
   -3,
   -2,
   -1,
    0,
    1,
    2,
    3,
    4,
    5,
    6,
};

static const float BlurWeights[13] =
{
   0.002216,
   0.008764,
   0.026995,
   0.064759,
   0.120985,
   0.176033,
   0.199471,
   0.176033,
   0.120985,
   0.064759,
   0.026995,
   0.008764,
   0.002216,
};

struct PS_IN {
	float4 pos : SV_POSITION;
	float2 uv1 : TEXCOORD0;
};

float4 main(PS_IN input) : SV_TARGET {
	const float2 uv = input.uv1;

    // Pixel width
    float pixelWidth = 1/256.0f;

    float4 color = {0, 0, 0, 0};

    float2 blur;
#ifdef HOR
    blur.y = uv.y;
#else
	blur.x = uv.x;
#endif

    for (int i = 0; i < 13; i++) 
    {
#ifdef HOR
        blur.x = uv.x + Pixels[i] * pixelWidth;
#else
		blur.y = uv.y + Pixels[i] * pixelWidth;
#endif
#ifdef D3D11
		color.a += texture1.Sample(texSampler, blur.xy).a * BlurWeights[i];
#else
        color.a += tex2D(texSampler, blur.xy).a * BlurWeights[i];
#endif
    }  

    return color;
}
