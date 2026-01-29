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
float2 TexelSize;           // 1.0 / textureSize
float FocusCenter = 0.5;    // Vertical center of focus (0-1, 0.5 = middle)
float FocusBand = 0.25;     // Half-width of sharp focus band (0-0.5)
float BlurAmount = 3.0;     // Maximum blur strength at edges

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Calculate distance from focus center (vertical only)
    float distFromCenter = abs(texCoord.y - FocusCenter);

    // Calculate blur factor (0 in focus band, increases outside)
    float blurFactor = saturate((distFromCenter - FocusBand) / (0.5 - FocusBand));

    // Smooth the transition
    blurFactor = blurFactor * blurFactor; // Quadratic falloff for smoother look

    // If no blur needed, return original
    if (blurFactor < 0.01)
        return tex2D(TextureSampler, texCoord);

    // Apply gaussian blur scaled by blur factor
    float4 color = float4(0, 0, 0, 0);
    float totalWeight = 0.0;

    // Blur radius based on distance from focus
    float radius = blurFactor * BlurAmount;

    // 9-tap vertical blur (tilt-shift is primarily vertical)
    for (int i = -4; i <= 4; i++)
    {
        float weight = exp(-(i * i) / (2.0 * radius * radius + 0.001));
        float2 offset = float2(0, i * TexelSize.y * radius);
        color += tex2D(TextureSampler, texCoord + offset) * weight;
        totalWeight += weight;
    }

    // Add some horizontal blur for more natural bokeh
    for (int j = -2; j <= 2; j++)
    {
        if (j == 0) continue; // Skip center (already sampled)
        float weight = exp(-(j * j) / (2.0 * radius * radius + 0.001)) * 0.5;
        float2 offset = float2(j * TexelSize.x * radius, 0);
        color += tex2D(TextureSampler, texCoord + offset) * weight;
        totalWeight += weight;
    }

    return color / totalWeight;
}

technique TiltShift
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
