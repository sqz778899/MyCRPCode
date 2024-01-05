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
};

Light GetMainLight()
{
    Light light;
    light.direction = half3(_MainLightPosition.xyz);
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

#endif