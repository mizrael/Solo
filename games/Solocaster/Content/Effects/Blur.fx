#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D TextureSampler : register(s0);

float2 TexelSize;
float BlurAmount = 1.0;
float DarkenAmount = 0.4;

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = float4(0, 0, 0, 0);

    // 9-tap Gaussian blur
    float weights[9] = { 0.05, 0.09, 0.12, 0.15, 0.18, 0.15, 0.12, 0.09, 0.05 };
    float offsets[9] = { -4, -3, -2, -1, 0, 1, 2, 3, 4 };

    for (int i = 0; i < 9; i++)
    {
        float2 offset = float2(offsets[i] * TexelSize.x * BlurAmount, offsets[i] * TexelSize.y * BlurAmount);
        color += tex2D(TextureSampler, texCoord + offset) * weights[i];
    }

    // Darken
    color.rgb *= DarkenAmount;

    return color;
}

technique Blur
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
