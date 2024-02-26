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
            CommandBuffer cmd = renderingData.commandBuffer;
            SetupMainLightConstants(cmd, ref renderingData.lightData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        void SetupMainLightConstants(CommandBuffer cmd, ref LightData lightData)
        {
            VisibleLight visiblelight = lightData.visibleLights[lightData.mainLightIndex];
            Light light = visiblelight.light;
            Matrix4x4 lightLocalToWorld = visiblelight.localToWorldMatrix;
            Vector4 dir = -lightLocalToWorld.GetColumn(2);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition, dir);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightColor, light.color);
        }
        
        public ForwardLights()
        {
            LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
            LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
        }
    }
}