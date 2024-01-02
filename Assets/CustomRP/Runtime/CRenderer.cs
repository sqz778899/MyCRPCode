using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class CRenderer: ScriptableRenderer
    {
        //.................装配的Light.................
        ForwardLights m_ForwardLights;
        //.................装配的Pass.................
        DrawObjectsPass m_RenderOpaqueForwardPass;
        MainLightShadowCasterPass m_MainLightShadowCasterPass;
        DrawSkyboxPass m_DrawSkyboxPass;
        FinalBlitPass m_FinalBlitPass;
        
        public void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters,
            float shadowDistance)
        {
            cullingParameters.maximumVisibleLights = 1;
            cullingParameters.shadowDistance = shadowDistance;
            cullingParameters.conservativeEnclosingSphere = true;
            cullingParameters.numIterationsEnclosingSphere = 64;
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_MainLightShadowCasterPass.Setup(ref renderingData);
            
            EnqueuePass(m_RenderOpaqueForwardPass);
            EnqueuePass(m_MainLightShadowCasterPass);
            EnqueuePass(m_DrawSkyboxPass);
            EnqueuePass(m_FinalBlitPass);
        }

        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_ForwardLights.Setup(context, ref renderingData);
        }

        public CRenderer(CRendererData data): base(data)
        {
            m_ForwardLights = new ForwardLights();
            m_RenderOpaqueForwardPass = new DrawObjectsPass(RenderPassEvent.BeforeRenderingOpaques);
            m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
            m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
            m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering);
        }
    }
}