#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
#include "../ShaderLibrary/Input.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"

//........................PBR....................................
half3 LightingPhysicallyBased(BRDFData brdfData, Light mainLight,
    half3 normalWS,half3 viewDirectionWS)
{
    half NdotL = saturate(dot(normalWS, mainLight.direction));
    half3 radiance = mainLight.color  * (mainLight.shadowAttenuation * NdotL);
    //Spe
    half spe = DirectBRDFSpecular(brdfData,normalWS,mainLight.direction, viewDirectionWS);
    return spe.rrr;
}

half4 CustomFragmentPBR(InputData inputData, SurfaceData surfaceData)
{
    Light mainlight = GetMainLight(TransformWorldToShadowCoord(inputData.positionWS));
    BRDFData brdfData;
    InitializeBRDFData(surfaceData,brdfData);
    half3 giColor = GlobalIllumination(brdfData,
        inputData.normalWS,inputData.viewDirectionWS,inputData.bakedGI,surfaceData.occlusion);
    half3 lightColor = LightingPhysicallyBased(brdfData,mainlight,
        inputData.normalWS,inputData.viewDirectionWS);
    return half4(lightColor,1);
}
#endif