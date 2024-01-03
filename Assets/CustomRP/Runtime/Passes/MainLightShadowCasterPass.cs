using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class MainLightShadowCasterPass: ScriptableRenderPass
    {
        const int k_MaxCascades = 4;
        const int k_ShadowmapBufferBits = 16;
        float m_CascadeBorder;
        float m_MaxShadowDistanceSq;
        
        int m_ShadowCasterCascadesCount;
        int renderTargetWidth;
        int renderTargetHeight;
        
        internal RTHandle m_MainLightShadowmapTexture;
        
        Matrix4x4[] m_MainLightShadowMatrices;
        Vector4[] m_CascadeSplitDistances;
        ShadowSliceData[] m_CascadeSlices;
        
        bool m_CreateEmptyShadowmap;
        //.........temp todo 
        int shadowLightIndex = 0;

        public void Setup(ref RenderingData renderingData)
        {
            Debug.Log("Shadow Setup");
            VisibleLight visibleLight = renderingData.lightData.visibleLight;
            Light light = renderingData.lightData.visibleLight.light;
            if (!renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out Bounds bounds))
            {
                Debug.LogError("Cant Find Shadow");
                return;
            }
            renderTargetWidth = renderingData.shadowData.mainLightShadowmapWidth;
            renderTargetHeight = (m_ShadowCasterCascadesCount == 2) ?
                renderingData.shadowData.mainLightShadowmapHeight >> 1 :
                renderingData.shadowData.mainLightShadowmapHeight;
            
            m_ShadowCasterCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;
            int shadowResolution = ShadowUtils.GetMaxTileResolutionInAtlas(renderingData.shadowData.mainLightShadowmapWidth,
                renderingData.shadowData.mainLightShadowmapHeight, m_ShadowCasterCascadesCount);
            
            for (int cascadeIndex = 0; cascadeIndex < m_ShadowCasterCascadesCount; ++cascadeIndex)
            {
                bool success = ShadowUtils.ExtractDirectionalLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData,
                    shadowLightIndex, cascadeIndex, renderTargetWidth, renderTargetHeight, shadowResolution, light.shadowNearPlane,
                    out m_CascadeSplitDistances[cascadeIndex], out m_CascadeSlices[cascadeIndex]);

                if (!success)
                    return;
            }
            
            ShadowUtils.ShadowRTNeedsReAlloc(
                ref m_MainLightShadowmapTexture, renderTargetWidth, renderTargetHeight,
                k_ShadowmapBufferBits, name: "_MainLightShadowmapTexture");
            
            m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
            m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
            m_CreateEmptyShadowmap = false;
            useNativeRenderPass = true;
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CullingResults cullResults = renderingData.cullResults;
            LightData lightData = renderingData.lightData;
            ShadowData shadowData = renderingData.shadowData;
            VisibleLight shadowLight = lightData.visibleLight;
            CommandBuffer cmd = renderingData.commandBuffer;
            
            Debug.Log("Shadow");
        }
        
        public MainLightShadowCasterPass(RenderPassEvent evt)
        {
            m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
            m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
            m_CascadeSplitDistances = new Vector4[k_MaxCascades];
            renderPassEvent = evt;
        }
        
        bool SetupForEmptyRendering(ref RenderingData renderingData)
        {
            /*if (!renderingData.cameraData.renderer.stripShadowsOffVariants)
                return false;

            m_CreateEmptyShadowmap = true;
            useNativeRenderPass = false;
            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyLightShadowmapTexture, 1, 1, k_ShadowmapBufferBits, name: "_EmptyLightShadowmapTexture");*/

            return true;
        }
    }
}