#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Core.hlsl"
#include "../ShaderLibrary/RealtimeLights.hlsl"
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
//...........................
void InitInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    
    inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    inputData.viewDirectionWS = normalize(_WorldSpaceCameraPos - input.positionWS);

    //................构建TBN矩阵.....................
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
    inputData.tangentToWorld = tangentToWorld;
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
    
    //................Shadow.....................
    inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

    //....................SH.....................
    inputData.bakedGI = SampleSH(input.normalWS);
    //SAMPLE_GI(input.positionWS, inputData.vertexSH);
}
//..........................Vertex Shader.............................................
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
    half4 worldPos = mul(unity_ObjectToWorld, float4(input.positionOS.xyz, 1.0));
    output.positionWS = worldPos.xyz;
    output.positionCS = mul(UNITY_MATRIX_VP, worldPos);
    return output;
}

//..........................Fragment Shader.............................................
half4 PhonePassFragment(Varyings input): SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
    half NOL = saturate(dot(input.normalWS, _MainLightPosition.xyz)) * _MainLightColor;
    float3 reflectDir = normalize(reflect(_MainLightPosition.xyz * -1, input.normalWS));
    float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
    float3 specular =  pow(max(0, dot(reflectDir, viewDir)), _Gloss);
   
    half3 Color = _MainLightColor.rgb * NOL * light.shadowAttenuation + specular;
    
    return half4(Color, 1);
}

half4 BlinnPhonePassFragment(Varyings input): SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
    half NOL = saturate(dot(input.normalWS, _MainLightPosition.xyz)) * _MainLightColor;
    float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
    float3 halfDir = normalize(viewDir + _MainLightPosition.xyz);
    float3 specular = pow(max(0,dot(input.normalWS, halfDir)), _Gloss);
   
    half3 Color = _MainLightColor.rgb * NOL * light.shadowAttenuation + specular;
    
    return half4(Color, 1);
}

half4 LitPassFragment(Varyings input): SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    SurfaceData surfaceData;
    InitSurfaceData(input.uv,surfaceData);

    InputData inputData;
    InitInputData(input, surfaceData.normalTS, inputData);
    
    Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
    half NOL = saturate(dot(input.normalWS, _MainLightPosition.xyz)) * _MainLightColor;
    float3 R = reflect(_MainLightPosition.xyz * -1, input.normalWS);
    float3 V = normalize(_WorldSpaceCameraPos - input.positionWS);
    float3 specular =  pow(max(0, dot(R, V)), 50);
   
    half3 Color = _MainLightColor.rgb * NOL * light.shadowAttenuation + specular;
   
    return half4(Color * inputData.bakedGI, 1);
}
#endif