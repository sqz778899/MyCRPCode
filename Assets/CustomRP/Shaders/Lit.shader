Shader "CRPipline/Lit"
{
    Properties
    {
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
            //#pragma vertex vert
            //#pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            // GPU Instancing
            #pragma multi_compile_instancing
            
            #include "./LitPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            

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
