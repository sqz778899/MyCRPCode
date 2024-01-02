#ifndef CUSTOM_PIPELINE_CORE_INCLUDED
#define CUSTOM_PIPELINE_CORE_INCLUDED
#include "../ShaderLibrary/Input.hlsl"
// Structs
struct VertexPositionInputs
{
    float3 positionWS; // World space position
    float3 positionVS; // View space position
    float4 positionCS; // Homogeneous clip space position
    //float4 positionNDC;// Homogeneous normalized device coordinates
};

struct VertexNormalInputs
{
    float3 tangentWS;
    float3 bitangentWS;
    float3 normalWS;
};
#endif