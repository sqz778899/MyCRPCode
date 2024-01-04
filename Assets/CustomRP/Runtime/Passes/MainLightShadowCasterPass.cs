using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class MainLightShadowCasterPass: ScriptableRenderPass
    {
        static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        const int k_MaxCascades = 4;
        const int k_ShadowmapBufferBits = 16;
        float m_CascadeBorder;
        float m_MaxShadowDistanceSq;
        
        int m_ShadowCasterCascadesCount;
        int renderTargetWidth;
        int renderTargetHeight;
        int m_MainLightShadowmapID;
        
        internal RTHandle m_MainLightShadowmapTexture;
        
        Matrix4x4[] m_MainLightShadowMatrices;
        Vector4[] m_CascadeSplitDistances;
        ShadowSliceData[] m_CascadeSlices;
        
        bool m_CreateEmptyShadowmap;
        //.........temp todo 
        int shadowLightIndex = 0;

        public void Setup(ref RenderingData renderingData)
        {
            CommandBuffer cmd = renderingData.commandBuffer;
            ref CameraData cameraData = ref renderingData.cameraData;
           
            Debug.Log("Shadow Setup");
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
            cmd.GetTemporaryRT(
                dirShadowAtlasId, 1024, 1024,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
            cmd.SetRenderTarget(
                dirShadowAtlasId,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
            Configure(cmd, cameraData.cameraTargetDescriptor);
            m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
            m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
            m_CreateEmptyShadowmap = false;
            useNativeRenderPass = true;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_MainLightShadowmapTexture);
            ConfigureClear(ClearFlag.All, Color.black);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
          
            CullingResults cullResults = renderingData.cullResults;
            LightData lightData = renderingData.lightData;
            ShadowData shadowData = renderingData.shadowData;
            
            VisibleLight shadowLight = lightData.visibleLight;
            CommandBuffer cmd = renderingData.commandBuffer;
            
            /*
            cmd.GetTemporaryRT(
                dirShadowAtlasId, 1024, 1024,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
            cmd.SetRenderTarget(
                dirShadowAtlasId,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
            );
            cmd.ClearRenderTarget(true, false, Color.clear);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();*/

            ShadowDrawingSettings settings = new ShadowDrawingSettings(cullResults,0,BatchCullingProjectionType.Orthographic);
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, 
                renderingData.cameraData.camera.transform.position);
            
            for (int cascadeIndex = 0; cascadeIndex < m_ShadowCasterCascadesCount; ++cascadeIndex)
            {
                settings.splitData = m_CascadeSlices[cascadeIndex].splitData;
                Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight,
                    shadowLightIndex, ref renderingData.shadowData, 
                    m_CascadeSlices[cascadeIndex].projectionMatrix, m_CascadeSlices[cascadeIndex].resolution);
                cmd.SetGlobalVector("_ShadowBias", shadowBias);
                Vector3 lightDirection = -shadowLight.localToWorldMatrix.GetColumn(2);
                cmd.SetGlobalVector("_LightDirection", new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));
                Vector3 lightPosition = shadowLight.localToWorldMatrix.GetColumn(3);
                cmd.SetGlobalVector("_LightPosition", new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1.0f));
                ShadowUtils.RenderShadowSlice(cmd, ref context, ref m_CascadeSlices[cascadeIndex],
                    ref settings, m_CascadeSlices[cascadeIndex].projectionMatrix, m_CascadeSlices[cascadeIndex].viewMatrix);
                
            }


            cmd.SetGlobalTexture(m_MainLightShadowmapID, m_MainLightShadowmapTexture.nameID);
            Debug.Log("Shadow");
        }
        
        public MainLightShadowCasterPass(RenderPassEvent evt)
        {
            m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
            m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
            m_CascadeSplitDistances = new Vector4[k_MaxCascades];
            renderPassEvent = evt;
            
            m_MainLightShadowmapID = Shader.PropertyToID("_MainLightShadowmapTexture");
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