using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class DrawObjectsPass : ScriptableRenderPass
    {
        public RTHandle m_ColorTargetIndentifiers;
        public RTHandle m_DepthTargetIndentifiers;
        
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
        
        void CreateRT(ref RenderingData renderingData)
        {
            ref Camera camera = ref renderingData.cameraData.camera;
            CommandBuffer cmd = renderingData.commandBuffer;
            //Color
            RenderTextureDescriptor colorRTD = new RenderTextureDescriptor(
                camera.pixelWidth, camera.pixelHeight,
              GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormat.D32_SFloat);
            colorRTD.depthBufferBits = (int)DepthBits.None;
            m_ColorTargetIndentifiers?.Release();
            m_ColorTargetIndentifiers = RTHandles.Alloc(colorRTD, FilterMode.Bilinear,
                TextureWrapMode.Clamp,name: GlobaName.colorRTName);
            //Depth
            RenderTextureDescriptor depthRTD = new RenderTextureDescriptor(
                camera.pixelWidth, camera.pixelHeight,
                GraphicsFormat.D32_SFloat_S8_UInt, GraphicsFormat.D32_SFloat);
            m_DepthTargetIndentifiers?.Release();
            m_DepthTargetIndentifiers = RTHandles.Alloc(depthRTD, FilterMode.Point,
                TextureWrapMode.Clamp, name: GlobaName.depthRTName);
            //SetRTAndClear
            cmd.SetRenderTarget(m_ColorTargetIndentifiers, 
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, 
                m_DepthTargetIndentifiers, 
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(RTClearFlags.DepthStencil,camera.backgroundColor, 1.0f, 0x00);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = renderingData.commandBuffer;
            CreateRT(ref renderingData);
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