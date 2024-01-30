using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class DrawObjectsPass : ScriptableRenderPass
    {
        static string m_ProfilerTag = "DrawObjectsPass";
        RTHandle m_ColorTargetIndentifiers;
        RTHandle m_DepthTargetIndentifiers;
        
        static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");
        //"LightMode" = "CustomLit" 
        ShaderTagId[] shaderTagIds = new ShaderTagId[]
        {
            //new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("CustomLit")
        };
        
        public DrawObjectsPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            ref Camera camera = ref cameraData.camera;
            CommandBuffer cmd = renderingData.commandBuffer;
            cmd.BeginSample(m_ProfilerTag);
            cmd.SetRenderTarget(colorAttachmentHandle, 
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, 
                depthAttachmentHandle, 
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(RTClearFlags.DepthStencil,camera.backgroundColor, 1.0f, 0x00);
            cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix); // 恢复矩阵
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            //.................Setp 1 Set Global parameter..........................
            cmd.SetGlobalVector(s_DrawObjectPassDataPropID, new Vector4(0,0,0,1));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            //.................Setp 2 Draw Renderer..........................
            SortingSettings sortingSettings = new SortingSettings(cameraData.camera);
            DrawingSettings drawingSettings = new DrawingSettings(
                shaderTagIds[0], sortingSettings)
            {
                perObjectData = PerObjectData.LightProbe
            };
            for (int i = 1; i < shaderTagIds.Length; i++)
                drawingSettings.SetShaderPassName(i, shaderTagIds[i]);
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults,
                ref drawingSettings, ref filterSettings);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.EndSample(m_ProfilerTag);
        }
    }
}