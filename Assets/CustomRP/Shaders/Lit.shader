Shader "CRPipline/Lit"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "white" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        _RMOEMap ("RMOE Map", 2D) = "white" {}
        
        [Toggle]_METALLICPARAMETER("MetallicParameter", int) = 0
        _Metallic("Metallic",Range(0,1)) = 0
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

            //Shader_feature
            #pragma shader_feature_local_fragment _METALLICPARAMETER_ON
            
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
