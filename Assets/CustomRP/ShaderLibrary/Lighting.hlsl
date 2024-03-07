#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
#include "../ShaderLibrary/Input.hlsl"
#include "../ShaderLibrary/SurfaceData.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"

//........................PBR....................................
half4 CustomFragmentPBR(InputData inputData, SurfaceData surfaceData)
{
    BRDFData brdfData;
    InitializeBRDFData(surfaceData,brdfData);
    half3 giColor = GlobalIllumination(brdfData,
        inputData.normalWS,inputData.viewDirectionWS,inputData.bakedGI);
    
    return half4(brdfData.perceptualRoughness.rrr,1);
}

#endif