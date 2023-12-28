using System.Collections.Generic;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class DrawObjectsPass : ScriptableRenderPass
    {
        ShaderTagId[] shaderTagIds = new ShaderTagId[]
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("CustomLit")
        };
        
        public DrawObjectsPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }
    }
}