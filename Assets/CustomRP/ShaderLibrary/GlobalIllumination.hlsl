#ifndef CUSTOM_GLOBAL_ILLUMINATION_INCLUDED
#define CUSTOM_GLOBAL_ILLUMINATION_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SphericalHarmonics.hlsl"

half3 SampleSH(half3 normalWS)
{
    // LPPV is not supported in Ligthweight Pipeline
    real4 SHCoefficients[7];
    SHCoefficients[0] = unity_SHAr;
    SHCoefficients[1] = unity_SHAg;
    SHCoefficients[2] = unity_SHAb;
    SHCoefficients[3] = unity_SHBr;
    SHCoefficients[4] = unity_SHBg;
    SHCoefficients[5] = unity_SHBb;
    SHCoefficients[6] = unity_SHC;
    //return half3(unity_SHBg.rrr);
    return max(half3(0, 0, 0), SampleSH9(SHCoefficients, normalWS));
}

#endif