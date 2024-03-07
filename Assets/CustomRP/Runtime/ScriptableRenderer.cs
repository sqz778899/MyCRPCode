using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public abstract class ScriptableRenderer: IDisposable
    {
        internal RTHandle m_CameraColorRTH;
        internal RTHandle m_CameraDepthRTH;
        
        List<ScriptableRenderPass> m_ActiveRenderPassQueue = new List<ScriptableRenderPass>(32);
        
        public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);
        
        internal void Clear()
        {
        }

        public void EnqueuePass(ScriptableRenderPass pass)
        {
            m_ActiveRenderPassQueue.Add(pass);
        }
        
        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //.................Setp 1 SetLight..........................
            SetupLights(context, ref renderingData);
            //.................Setp 2 Set Camera Properties..........................
            SetupCameraProperties(renderingData,context);
            //context.SetupCameraProperties(renderingData.cameraData.camera);
            //.................Setp 3 Execute Opaque..........................
            for (int i = 0; i < m_ActiveRenderPassQueue.Count; i++)
                ExecuteRenderPass(context, m_ActiveRenderPassQueue[i],ref renderingData);
            //.................Setp 4 Finish Rendering................................
            //context.ExecuteCommandBuffer(renderingData.commandBuffer);
            renderingData.commandBuffer.Clear();
            m_ActiveRenderPassQueue.Clear();
        }

        void ExecuteRenderPass(ScriptableRenderContext context,ScriptableRenderPass renderPass,ref RenderingData renderingData)
        {
            CommandBuffer cmd = renderingData.commandBuffer;
            //.................Setp 1 Set Camera Target..........................
            //.................Setp 2 Execute Pass..........................
            renderPass.Execute(context, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        #region Execute 流程相关
        public virtual void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData) {}
        
        internal void SetupCameraProperties(RenderingData renderingData,ScriptableRenderContext context)
        {
            ref CommandBuffer cmd = ref renderingData.commandBuffer;
            context.SetupCameraProperties(renderingData.cameraData.camera);

            float scaledCameraWidth = 0;//(float)renderingData.cameraData.cameraTargetDescriptor.width;
            float scaledCameraHeight = 0;//(float)renderingData.cameraData.cameraTargetDescriptor.height;
            cmd.SetGlobalVector(ShaderPropertyId.scaledScreenParams,new Vector4(scaledCameraWidth, scaledCameraHeight, 1.0f + 1.0f / scaledCameraWidth, 1.0f + 1.0f / scaledCameraHeight));
        }
        

        #endregion

        //手动GC
        public void Dispose()
        {
            
        }
        
        public ScriptableRenderer(ScriptableRenderData data)
        {
            
        }
        
        internal void CreateCameraRT(ref RenderingData renderingData)
        {
            ref Camera camera = ref renderingData.cameraData.camera;
            CommandBuffer cmd = renderingData.commandBuffer;
            //Color
            RenderTextureDescriptor colorRTD = new RenderTextureDescriptor(
                camera.pixelWidth, camera.pixelHeight,
                GraphicsFormat.B10G11R11_UFloatPack32, GraphicsFormat.D32_SFloat);
            colorRTD.depthBufferBits = (int)DepthBits.None;
            m_CameraColorRTH?.Release();
            m_CameraColorRTH = RTHandles.Alloc(colorRTD, FilterMode.Bilinear,
                TextureWrapMode.Clamp,name: GlobaName.colorRTName);
            //Depth
            RenderTextureDescriptor depthRTD = new RenderTextureDescriptor(
                camera.pixelWidth, camera.pixelHeight,
                GraphicsFormat.D32_SFloat_S8_UInt, GraphicsFormat.D32_SFloat);
            m_CameraDepthRTH?.Release();
            m_CameraDepthRTH = RTHandles.Alloc(depthRTD, FilterMode.Point,
                TextureWrapMode.Clamp, name: GlobaName.depthRTName);
            //SetRTAndClear
            cmd.SetRenderTarget(m_CameraColorRTH, 
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, 
                m_CameraDepthRTH, 
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(RTClearFlags.DepthStencil,camera.backgroundColor, 1.0f, 0x00);
        }
    }
}