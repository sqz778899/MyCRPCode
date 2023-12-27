using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public abstract class ScriptableRenderer: IDisposable
    {
        RTHandleRenderTargetIdentifierCompat m_CameraColorTarget;
        RTHandleRenderTargetIdentifierCompat m_CameraDepthTarget;
        
        public ScriptableRenderer(ScriptableRenderData data)
        {
            
        }
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
        
        //手动GC
        public void Dispose()
        {
            
        }
    }
}