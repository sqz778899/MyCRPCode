using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public abstract class ScriptableRenderer: IDisposable
    {
        public RTHandle cameraColorTargetHandle
        {
            get { return m_CameraColorTarget.handle; }
        }
        RTHandleRenderTargetIdentifierCompat m_CameraColorTarget;
        RTHandleRenderTargetIdentifierCompat m_CameraDepthTarget;
        bool m_FirstTimeCameraColorTargetIsBound = true;  //记录是不是第一次绑定，是的话要去清理GPU各种状态内存啥的
        bool m_FirstTimeCameraDepthTargetIsBound = true;
        
        List<ScriptableRenderPass> m_ActiveRenderPassQueue = new List<ScriptableRenderPass>(32);
        
        public abstract void Setup(ScriptableRenderContext context, ref RenderingData renderingData);
        
        internal struct RTHandleRenderTargetIdentifierCompat
        {
            public RTHandle handle;
            public RenderTargetIdentifier fallback;
            public bool useRTHandle => handle != null;
            public RenderTargetIdentifier nameID => useRTHandle ? new RenderTargetIdentifier(handle.nameID, 0, CubemapFace.Unknown, -1) : fallback;
        }
        
        internal void Clear()
        {
            m_CameraColorTarget = new RTHandleRenderTargetIdentifierCompat { fallback = BuiltinRenderTextureType.CameraTarget };
            m_CameraDepthTarget = new RTHandleRenderTargetIdentifierCompat { fallback = BuiltinRenderTextureType.CameraTarget };
        }

        public void EnqueuePass(ScriptableRenderPass pass)
        {
            m_ActiveRenderPassQueue.Add(pass);
        }

        public virtual void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData) {}
        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {  
            ref CameraData cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = CommandBufferPool.Get();
            foreach (var pass in m_ActiveRenderPassQueue)
                pass.Configure(cmd, cameraData.cameraTargetDescriptor);
            //.................Setp 1 SetLight..........................
            SetupLights(context, ref renderingData);
            //.................Setp 2 Set Camera..........................
            context.SetupCameraProperties(renderingData.cameraData.camera);
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
            ref CameraData cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = renderingData.commandBuffer;
            //.................Setp 1 Set Camera Target..........................
            SetRenderPassAttachments(cmd, renderPass, ref cameraData);//综合管控各个Pass的RT，不然各个Pass不能相互配合，拿到各种中间信息
            
            //.................Setp 2 Execute Pass..........................
            renderPass.Execute(context, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        void SetRenderPassAttachments(CommandBuffer cmd, ScriptableRenderPass renderPass, ref CameraData cameraData)
        {
            
        }
        
        //手动GC
        public void Dispose()
        {
            
        }
        
        public ScriptableRenderer(ScriptableRenderData data)
        {
            //Clear(CameraRenderType.Base);
        }
    }
}