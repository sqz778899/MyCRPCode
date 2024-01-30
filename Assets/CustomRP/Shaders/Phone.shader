Shader "CRPipline/Phone"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Gloss("Gloss",Float) = 10
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass
        {
            Name "CustomLit"
            Tags
            {
                "LightMode" = "CustomLit"
            }
            HLSLPROGRAM
            #pragma target 2.0
            #pragma multi_compile_fog

            // GPU Instancing
            #pragma multi_compile_instancing
            
            #include "./LitPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment PhonePassFragment
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            
            ColorMask 0
            
            HLSLPROGRAM
            #pragma target 2.0
            #include "./ShadowCasterPass.hlsl"
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            ENDHLSL
        }
    }
}
