using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class DrawSkyboxPass: ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            ref CommandBuffer cmd = ref renderingData.commandBuffer;

            //cmd.SetRenderTarget();
            context.DrawSkybox(renderingData.cameraData.camera);
        }
        
        public DrawSkyboxPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
    }
}