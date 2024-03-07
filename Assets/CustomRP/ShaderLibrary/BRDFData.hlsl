#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

//BRDF的经验模型合集
#define kDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04)

half OneMinusReflectivityMetallic(half metallic)
{
    //现实中，即使金属度为0，表面仍然会有反射，根据经验定为0.04。（URP这么干的）
    half oneMinusDielectricSpec = kDielectricSpec.a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

struct BRDFData
{
    half3 albedo;
    half3 diffuse;
    half3 specular;
    half reflectivity;
    half perceptualRoughness;
    half roughness;
    half roughness2;
    half grazingTerm;

    // We save some light invariant BRDF terms so we don't have to recompute
    // them in the light loop. Take a look at DirectBRDF function for detailed explaination.
    half normalizationTerm;     // roughness * 4.0 + 2.0
    half roughness2MinusOne;    // roughness^2 - 1.0
};

void InitBRDFData(SurfaceData surfaceData,out BRDFData brdfData)
{
    brdfData = (BRDFData)0;
    half oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceData.metallic);
    half reflectivity = half(1.0) - oneMinusReflectivity;
    half3 brdfDiffuse = surfaceData.albedo * oneMinusReflectivity;
    half3 brdfSpecular = lerp(kDielectricSpec.rgb, surfaceData.albedo, surfaceData.metallic);

    brdfData.albedo = surfaceData.albedo;
    brdfData.diffuse = brdfDiffuse;
    brdfData.specular = brdfSpecular;
    brdfData.reflectivity = reflectivity;

    brdfData.perceptualRoughness = (1 - surfaceData.smoothness);
    brdfData.roughness           = max(PerceptualRoughnessToRoughness(brdfData.perceptualRoughness), HALF_MIN_SQRT);
    brdfData.roughness2          = max(brdfData.roughness * brdfData.roughness, HALF_MIN);
    brdfData.grazingTerm         = saturate(surfaceData.smoothness + reflectivity);
    brdfData.normalizationTerm   = brdfData.roughness * half(4.0) + half(2.0);
    brdfData.roughness2MinusOne  = brdfData.roughness2 - half(1.0);
}
#endif