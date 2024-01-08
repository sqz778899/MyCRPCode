using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class DrawObjectsPass : ScriptableRenderPass
    {
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
            CommandBuffer cmd = renderingData.commandBuffer;
          
            cmd.SetViewport(new Rect(0, 0, Screen.width, Screen.height)); // 恢复 Viewport
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
                shaderTagIds[0], sortingSettings);
            for (int i = 1; i < shaderTagIds.Length; i++)
                drawingSettings.SetShaderPassName(i, shaderTagIds[i]);
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults,
                ref drawingSettings, ref filterSettings);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}