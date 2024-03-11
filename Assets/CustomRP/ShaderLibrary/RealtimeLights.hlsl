#ifndef CUSTOM_REALTIME_LIGHTS_INCLUDED
#define CUSTOM_REALTIME_LIGHTS_INCLUDED
#include "../ShaderLibrary/Input.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
// Structs
struct Light
{
    half3   direction;
    half3   color;
    half    shadowAttenuation;
    //float   distanceAttenuation; // full-float precision required on some platforms
};

Light GetMainLight()
{
    Light light;
    light.direction = half3(_MainLightPosition.xyz);
    //light.distanceAttenuation = unity_LightData.z;
    light.color = _MainLightColor.rgb;
    light.shadowAttenuation = 1.0;
    return light;
}

Light GetMainLight(float4 shadowCoord)
{
    Light light = GetMainLight();
    light.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
    return light;
}

half4 CalculateShadowMask(InputData inputData)
{
    half4 shadowMask = half4(1, 1, 1, 1);

    return shadowMask;
}
#endif