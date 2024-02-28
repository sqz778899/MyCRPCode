#ifndef CUSTOM_LIT_INPUT_INCLUDED
#define CUSTOM_LIT_INPUT_INCLUDED
#include "../ShaderLibrary/SurfaceData.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);
SAMPLER(sampler_BumpMap);
TEXTURE2D(_RMOEMap);
SAMPLER(sampler_RMOEMap);

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BumpMap_ST;
float4 _RMOEMap_ST;
half4 _BaseColor;
half _BumpScale;
half _Metallic;

//非常用光照模型：Phone..BlinnPhone..
half _Gloss;
CBUFFER_END


float2 SetUVST(float2 uv, float4 st)
{
    return uv * st.xy + st.zw;
}

void InitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,SetUVST(uv,_BaseMap_ST)) * half4(_BaseColor.rgb, 1.0);
    half alpha = albedo.a + _BaseColor.a;
    
    half4 normalmap = SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap, SetUVST(uv,_BumpMap_ST));
    half3 normalTS = UnpackNormalScale(normalmap, _BumpScale);
    half4 rmoe = SAMPLE_TEXTURE2D(_RMOEMap,sampler_RMOEMap,SetUVST(uv,_RMOEMap_ST));

    outSurfaceData.albedo = albedo.rgb;
    outSurfaceData.alpha = alpha;
    outSurfaceData.normalTS = normalTS;
    outSurfaceData.smoothness = (1 - rmoe.r);
#ifdef _METALLICPARAMETER_ON
    outSurfaceData.metallic = _Metallic;
#else
    outSurfaceData.metallic = rmoe.g;
#endif
    outSurfaceData.occlusion = rmoe.b;
    outSurfaceData.emission = rmoe.a;
}
#endif