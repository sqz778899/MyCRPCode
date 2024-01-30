using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class FinalBlitPass : ScriptableRenderPass
    {
        static string m_ProfilerTag = "FinalBlitPass";
        RTHandle m_source;
        Material m_BlitMaterial;
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = renderingData.commandBuffer;
            cmd.BeginSample(m_ProfilerTag);
            //准备好要画的RT
            Vector4 rtHandleScale;
            if (cameraData.isSceneViewCamera)
                rtHandleScale = new Vector4(1,1, 0, 0);
            else
                rtHandleScale = new Vector4(1,-1, 0, 1);
            
            var rtd = new RenderTargetIdentifier(cameraData.camera.targetTexture);
            cmd.SetRenderTarget(rtd, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            //用这个HDR空间的RT作为源图，往屏幕上画
            Blitter.BlitTexture(cmd,m_source,rtHandleScale, m_BlitMaterial, 0);
            cmd.EndSample(m_ProfilerTag);
        }

        internal void Setup(RTHandle CameraRT)
        {
            m_source = CameraRT;
        }
        
        public FinalBlitPass(RenderPassEvent evt,Material blitMaterial)
        {
            renderPassEvent = evt;
            m_BlitMaterial = blitMaterial;
        }
    }
    
}