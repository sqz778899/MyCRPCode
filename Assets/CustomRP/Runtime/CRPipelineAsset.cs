using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
    public class CRPipelineAsset : RenderPipelineAsset
    {
        public ScriptableRenderer renderer;
        public float renderScale = 1.0f;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new CRPipeline();
        }
    }
}
