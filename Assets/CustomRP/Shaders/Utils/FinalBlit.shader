Shader "CRPipline/FinalBlit"
{
    Properties
    {
       _BlitTexture("Texture", 2D) = "white" {}
       _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags{"LightMode" = "CustomLit"}

        Pass
        {
            HLSLPROGRAM
                #pragma target 2.0
                #pragma vertex Vert
                #pragma fragment FragColorAndDepth
                #include "Blit.hlsl"
            ENDHLSL
        }
    }
}
