#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "../ShaderLibrary/Core.hlsl"
//#include "../ShaderLibrary/ShaderVariablesFunctions.hlsl"
#include "./LitInput.hlsl"

float3 _LightDirection;
float3 _LightPosition;
float4 _ShadowBias;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
     float2 texcoord    : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS               : SV_POSITION;
    float2 uv                       : TEXCOORD0;
};

float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
{
    float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
    float scale = invNdotL * _ShadowBias.y;

    // normal bias is negative since we want to apply an inset normal offset
    positionWS = lightDirection * _ShadowBias.xxx + positionWS;
    positionWS = normalWS * scale.xxx + positionWS;
    return positionWS;
}

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = mul(UNITY_MATRIX_M,float4(input.positionOS.xyz,1.0));
    float3 normalWS = SafeNormalize(mul(input.normalOS, (float3x3)UNITY_MATRIX_I_M));
    
    float3 positionWSBias = ApplyShadowBias(positionWS, normalWS, _LightDirection);
    float4 positionCS = mul(UNITY_MATRIX_VP, float4(positionWSBias, 1.0));

    #if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionCS = GetShadowPositionHClip(input);
    return output;
}


half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    half depth = input.positionCS.z / input.positionCS.w;
    return half4(depth, 1, 1, 1);
}
#endif