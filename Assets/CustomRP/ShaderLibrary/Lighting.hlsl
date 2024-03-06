#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
#include "../ShaderLibrary/Input.hlsl"
#include "../ShaderLibrary/SurfaceData.hlsl"
#include "../ShaderLibrary/BRDFData.hlsl"

//........................PBR....................................
half4 CustomFragmentPBR(InputData inputData, SurfaceData surfaceData)
{
    BRDFData brdfData;
    InitBRDFData(surfaceData,brdfData);
    return half4(0.5,1,1,1);
}

#endif