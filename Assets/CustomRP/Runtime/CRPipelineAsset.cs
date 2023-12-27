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
        public CRenderer renderer;
        public CRendererData  renderData;
        public float renderScale = 1.0f;
        
        protected override RenderPipeline CreatePipeline()
        {
            if (renderData == null || renderer == null)
            {
                renderData = new CRendererData();
                renderer = (CRenderer)renderData.InternalCreateRenderer();
            }
            return new CRPipeline();
        }
    }
}
