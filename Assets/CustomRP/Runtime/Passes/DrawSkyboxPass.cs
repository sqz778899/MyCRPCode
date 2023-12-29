using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class DrawSkyboxPass: ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            context.DrawSkybox(renderingData.cameraData.camera);
        }
        
        public DrawSkyboxPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
    }
}