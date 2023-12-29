using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class ForwardLights
    {
        static class LightConstantBuffer
        {
            public static int _MainLightPosition;
            public static int _MainLightColor;
        }

        public void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }
        
        public ForwardLights()
        {
            LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
            LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
        }
    }
}