using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public abstract class ScriptableRenderer: IDisposable
    {
        RTHandleRenderTargetIdentifierCompat m_CameraColorTarget;
        RTHandleRenderTargetIdentifierCompat m_CameraDepthTarget;
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
        public void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //.................Setp 1 SetLight..........................
            //.................Setp 2 Set Camera..........................
            //.................Setp 3 Execute Opaque..........................
            //.................Setp 4 Finish Rendering................................
        }
        //手动GC
        public void Dispose()
        {
            
        }
        
        public ScriptableRenderer(ScriptableRenderData data)
        {
            
        }
    }
}