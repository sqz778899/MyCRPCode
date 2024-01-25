using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class FinalBlitPass : ScriptableRenderPass
    {
        RTHandle m_source;
        Material m_BlitMaterial;
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_source = renderingData.cameraData.renderer.m_CameraColorRTH;
            Debug.Log("FinalBlitPass");
            ref CameraData cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = renderingData.commandBuffer;
            //准备好要画的RT
            var rtd = new RenderTargetIdentifier(cameraData.camera.targetTexture);
            cmd.SetRenderTarget(colorAttachmentHandle, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            //用这个HDR空间的RT作为源图，往屏幕上画
            //RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            //
            Blitter.BlitTexture(cmd,m_source,new Vector4(1,-1,0,1), m_BlitMaterial, 0);
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