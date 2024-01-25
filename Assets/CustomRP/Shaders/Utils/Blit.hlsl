#ifndef CUSTOM_BLIT_INCLUDED
#define CUSTOM_BLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

struct Attributes
{
    float4 positionOS       : POSITION;
    float2 uv               : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

TEXTURE2D (_BlitTexture);
TEXTURE2D (_InputDepthTexture);
SamplerState sampler_PointClamp;
SamplerState sampler_LinearClamp;
uniform float4 _BlitScaleBias;
uniform float _BlitMipLevel;

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 texcoord   : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};


Varyings Vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    
    float4 pos = input.positionOS;
    float2 uv  = input.uv;

    output.positionCS = pos;
    output.texcoord   = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
    return output;
}

struct PixelData
{
    float4 color : SV_Target;
    float  depth : SV_Depth;
};

float4 FragColorOnly(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    return half4(1, 1, 1, 1);
    return SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_LinearClamp, input.texcoord.xy, _BlitMipLevel);
}

PixelData FragColorAndDepth(Varyings input)
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    PixelData pd;
    pd.color = SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_LinearClamp, input.texcoord.xy, _BlitMipLevel);
    pd.depth = SAMPLE_TEXTURE2D_LOD(_InputDepthTexture, sampler_PointClamp, input.texcoord.xy, _BlitMipLevel).x;
    return pd;
}

#endif