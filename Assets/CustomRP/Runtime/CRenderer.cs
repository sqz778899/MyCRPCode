using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class CRenderer: ScriptableRenderer
    {
        public CRenderer(CRendererData data): base(data)
        {
            //m_BlitMaterial = CoreUtils.CreateEngineMaterial(data.shaders.coreBlitPS);
            // m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
        }
    }
}