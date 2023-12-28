using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class CRenderer: ScriptableRenderer
    {
        public string Name = "XXX";
        //.................装配的Pass.................
        DrawObjectsPass m_RenderOpaqueForwardPass;
        
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
            EnqueuePass(m_RenderOpaqueForwardPass);
        }
        

        public CRenderer(CRendererData data): base(data)
        {
            m_RenderOpaqueForwardPass = new DrawObjectsPass(RenderPassEvent.BeforeRenderingOpaques);
        }
    }
}