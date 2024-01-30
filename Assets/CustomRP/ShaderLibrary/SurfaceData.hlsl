#ifndef CUSTOM_SURFACE_DATA_INCLUDED
#define CUSTOM_SURFACE_DATA_INCLUDED

// Must match Universal ShaderGraph master node
struct SurfaceData
{
    half3 albedo;
    half  alpha;
    half3 normalTS;
    half  smoothness;
    half  metallic;
    half  occlusion;
    half3 emission;
};

#endif