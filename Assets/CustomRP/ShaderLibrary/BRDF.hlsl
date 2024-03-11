#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED
#include "../ShaderLibrary/SurfaceData.hlsl"
#define kDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04)
// standard dielectric reflectivity coef at incident angle (= 4%)

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

half3 EnvironmentBRDFSpecular(BRDFData brdfData, half fresnelTerm)
{
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    return half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm));
}

half3 EnvironmentBRDF(BRDFData brdfData, half3 indirectDiffuse, half3 indirectSpecular, half fresnelTerm)
{
    half3 c = indirectDiffuse * brdfData.diffuse;
    c += indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm);
    return c;
}

void InitializeBRDFData(inout SurfaceData surfaceData, out BRDFData brdfData)
{
    //金属度为0时，反射率为0.04，金属度为1时，反射率为1
    //非金属也有镜面反射，0.04是一个经验值
    half oneMinusReflectivity = kDielectricSpec.a - surfaceData.metallic * kDielectricSpec.a;
    half reflectivity = half(1.0) - oneMinusReflectivity;
    half3 brdfDiffuse = surfaceData.albedo * oneMinusReflectivity;
    //金属高光是有颜色的，非金属高光是白色的
    half3 brdfSpecular = lerp(kDielectricSpec.rgb, surfaceData.albedo, surfaceData.metallic);

    brdfData = (BRDFData)0;
    brdfData.albedo = surfaceData.albedo;
    brdfData.diffuse = brdfDiffuse;
    brdfData.specular = brdfSpecular;
    brdfData.reflectivity = reflectivity;
    brdfData.perceptualRoughness = surfaceData.roughness;
    brdfData.roughness = max(PerceptualRoughnessToRoughness(surfaceData.roughness),HALF_MIN_SQRT);
    brdfData.roughness2 = max(brdfData.roughness * brdfData.roughness,HALF_MIN);
    //描述当光线与表面接触角度接近90度（接近平行于表面，也称为“擦边”或“grazing”）时的光照情况。
    //表面越来越光滑或者反射率越来越高，grazingTerm 越接近1，也就是说在光线与表面接近平行时，接收到的光线越多。
    brdfData.grazingTerm = saturate((1-surfaceData.roughness) + reflectivity);
    //normalizationTerm 在 BRDF 模型中用于确保反射光线强度在所有方向上的总和不会超过入射光线的强度，即“能量守恒”。
    brdfData.normalizationTerm = brdfData.roughness * 4.0 + 2.0;
    //粗糙度的平方通常用于表示反射光分布的宽窄程度。
    brdfData.roughness2MinusOne = brdfData.roughness2 - 1.0;
}

half DirectBRDFSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));
    float NoH = saturate(dot(float3(normalWS), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    half LoH2 = LoH * LoH;
    half specularTerm = brdfData.roughness2 / ((d * d) * max(0.1h, LoH2) * brdfData.normalizationTerm);
    return specularTerm;
}
    
#endif