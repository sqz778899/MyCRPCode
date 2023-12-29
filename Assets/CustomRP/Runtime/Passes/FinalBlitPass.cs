using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class FinalBlitPass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Debug.Log("FinalBlitPass");
            CommandBuffer cmd = renderingData.commandBuffer;
            //RTHandle source,
            //Blitter.BlitTexture(cmd, source, scaleBias, material, passIndex);
        }
        
        public FinalBlitPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
    }
}