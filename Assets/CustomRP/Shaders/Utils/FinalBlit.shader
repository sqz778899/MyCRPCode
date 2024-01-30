Shader "CRPipline/FinalBlit"
{
    Properties
    {
       _BlitTexture("Texture", 2D) = "white" {}
       _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            ZClip True
            ZTest Always
            ZWrite Off
            Cull Off
            HLSLPROGRAM
                #pragma target 2.0
                #pragma vertex Vert
                #pragma fragment FragColorOnly
                #include "Blit.hlsl"
            ENDHLSL
        }
    }
}
