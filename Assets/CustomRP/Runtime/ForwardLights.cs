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
            Debug.Log("Setup  Lights");
            CommandBuffer cmd = renderingData.commandBuffer;
            SetupMainLightConstants(cmd, ref renderingData.lightData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        void SetupMainLightConstants(CommandBuffer cmd, ref LightData lightData)
        {
            Light light = lightData.visibleLight.light;
            Matrix4x4 lightLocalToWorld = lightData.visibleLight.localToWorldMatrix;
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