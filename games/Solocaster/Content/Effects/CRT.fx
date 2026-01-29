#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D TextureSampler : register(s0);

// Parameters
float2 ScreenSize;              // Screen dimensions in pixels
float Curvature = 0.03;         // Screen curvature amount (0 = flat)
float ChromaticAberration = 0.002; // Color separation at edges
float Vignette = 0.3;           // Edge darkening
float Brightness = 1.1;         // Overall brightness boost to compensate for darkening

// Apply barrel distortion for CRT curvature
float2 CurveUV(float2 uv)
{
    // Center UV around origin
    float2 centered = uv * 2.0 - 1.0;

    // Apply barrel distortion
    float2 offset = centered * (1.0 - Curvature * (centered.x * centered.x + centered.y * centered.y));

    // Convert back to 0-1 range
    return offset * 0.5 + 0.5;
}

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Apply curvature
    float2 curvedUV = CurveUV(texCoord);

    // Check if we're outside the curved screen bounds
    if (curvedUV.x < 0.0 || curvedUV.x > 1.0 || curvedUV.y < 0.0 || curvedUV.y > 1.0)
        return float4(0, 0, 0, 1);

    // Chromatic aberration - separate RGB channels slightly at edges
    float2 centerOffset = curvedUV - 0.5;
    float distFromCenter = length(centerOffset);
    float2 aberrationOffset = centerOffset * ChromaticAberration * distFromCenter;

    float r = tex2D(TextureSampler, curvedUV + aberrationOffset).r;
    float g = tex2D(TextureSampler, curvedUV).g;
    float b = tex2D(TextureSampler, curvedUV - aberrationOffset).b;

    float3 color = float3(r, g, b);

    // Vignette - darken edges
    float vignetteAmount = 1.0 - distFromCenter * Vignette * 2.0;
    vignetteAmount = saturate(vignetteAmount);
    color *= vignetteAmount;

    // Brightness compensation
    color *= Brightness;

    return float4(saturate(color), 1.0);
}

technique CRT
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
