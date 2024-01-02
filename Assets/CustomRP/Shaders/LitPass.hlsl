#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "../ShaderLibrary/Core.hlsl"
//#include "../ShaderLibrary/ShaderVariablesFunctions.hlsl"
#include "./LitInput.hlsl"


struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS               : SV_POSITION;
    float2 uv                       : TEXCOORD0;
    float3 positionWS               : TEXCOORD1;
    float3 normalWS                 : TEXCOORD2;
    half4 tangentWS                 : TEXCOORD3;
    //float4 shadowCoord              : TEXCOORD4;
    //half3 viewDirTS                 : TEXCOORD5;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    //VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS);
    //VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.normalWS = SafeNormalize(mul(input.normalOS, (float3x3)UNITY_MATRIX_I_M));
    output.tangentWS = half4(SafeNormalize(float3(mul((float3x3)UNITY_MATRIX_M, input.tangentOS.xyz))),1);
    output.positionCS = mul(UNITY_MATRIX_VP, input.positionOS);
    return output;
}


half4 LitPassFragment(Varyings input): SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    half NOL = saturate(dot(input.normalWS, _MainLightPosition.xyz)) * _MainLightColor;
    half3 Color = _MainLightColor.rgb * NOL;
    return half4(Color, 1);
}
#endif