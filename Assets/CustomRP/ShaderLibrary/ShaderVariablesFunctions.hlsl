#ifndef UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED
#define UNITY_SHADER_VARIABLES_FUNCTIONS_INCLUDED
#include "./Core.hlsl"


// Structs
VertexPositionInputs GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS =  mul(UNITY_MATRIX_M, float4(positionOS, 1.0)).xyz;
    input.positionVS = mul(UNITY_MATRIX_V, float4(input.positionWS, 1.0));
    input.positionCS = mul(UNITY_MATRIX_VP, float4(input.positionWS, 1.0));
    return input;
}


VertexNormalInputs GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;

    // mikkts space compliant. only normalize when extracting normal at frag.
    tbn.normalWS = SafeNormalize(mul(normalOS, (float3x3)UNITY_MATRIX_I_M));
    tbn.tangentWS = float3(mul((float3x3)UNITY_MATRIX_M, tangentOS.xyz));
    tbn.bitangentWS = float3(cross(tbn.normalWS, float3(tbn.tangentWS)));
    return tbn;
}
#endif