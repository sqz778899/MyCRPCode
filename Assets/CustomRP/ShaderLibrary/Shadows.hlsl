#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "../ShaderLibrary/Input.hlsl"

///////////////Structs/////////////////////

/////////////////Texture2D/////////////////////
TEXTURE2D(_ScreenSpaceShadowmapTexture);
TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);

///////////////Parameter/////////////////////
float4      _MainLightShadowOffset0; // xy: offset0, zw: offset1
float4      _MainLightShadowOffset1; // xy: offset2, zw: offset3
float4      _MainLightShadowParams;   // (x: shadowStrength, y: >= 1.0 if soft shadows, 0.0 otherwise, z: main light fade scale, w: main light fade bias)
float4      _MainLightShadowmapSize;  // (xy: 1/width and 1/height, zw: width and height)


half MainLightRealtimeShadow(float4 shadowCoord)
{
    half4 shadowParams = _MainLightShadowParams;
    
    real attenuation;
    real shadowStrength = shadowParams.x;
    attenuation = real(SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture,
        sampler_LinearClampCompare, shadowCoord.xyz));
    attenuation = LerpWhiteTo(attenuation, shadowStrength);
    
    return attenuation;
}

#endif