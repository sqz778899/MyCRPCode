using UnityEngine;
using UnityEngine.Experimental.Rendering;
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

        #region ShaderPropertyID
        int m_MainLightShadowmapID = Shader.PropertyToID("_MainLightShadowmapTexture");
        int m_WorldToShadow = Shader.PropertyToID("_MainLightWorldToShadow");
        #endregion
        
        internal RTHandle m_MainLightShadowmapTexture;
        
        Matrix4x4[] m_MainLightShadowMatrices;
        Vector4[] m_CascadeSplitDistances;
        ShadowSliceData[] m_CascadeSlices;
        
        bool m_CreateEmptyShadowmap;
        

        public void Setup(ref RenderingData renderingData)
        {
            int shadowLightIndex = renderingData.lightData.mainLightIndex;
            Light mainlight = renderingData.lightData.visibleLights[shadowLightIndex].light;

            int shadowResolution = renderingData.shadowData.mainLightShadowmapWidth;
            ref CullingResults cullingResults = ref renderingData.cullResults;
            ref ShadowData shadowData = ref renderingData.shadowData;
            ref ShadowSliceData shadowSliceData = ref m_CascadeSlices[0];
            
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex,
                0, shadowData.mainLightShadowCascadesCount, shadowData.mainLightShadowCascadesSplit, 
                shadowResolution, mainlight.shadowNearPlane, out shadowSliceData.viewMatrix, out shadowSliceData.projectionMatrix,
                out shadowSliceData.splitData);
            
            m_CascadeSplitDistances[0] = shadowSliceData.splitData.cullingSphere;
            shadowSliceData.offsetX = 0;
            shadowSliceData.offsetY = 0;
            shadowSliceData.resolution = shadowResolution;
            shadowSliceData.shadowTransform = ShadowUtils.GetShadowTransform(shadowSliceData.projectionMatrix, shadowSliceData.viewMatrix);
            shadowSliceData.splitData.shadowCascadeBlendCullingFactor = 1.0f;

            CreateShadowRT(ref renderingData);

            m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
            m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
            m_CreateEmptyShadowmap = false;
            useNativeRenderPass = true;
        }

        void CreateShadowRT(ref RenderingData renderingData)
        {
            RenderTextureDescriptor rtd = new RenderTextureDescriptor(
                renderingData.shadowData.mainLightShadowmapWidth, 
                renderingData.shadowData.mainLightShadowmapHeight,
                GraphicsFormat.None, GraphicsFormat.D32_SFloat);
            rtd.shadowSamplingMode = ShadowSamplingMode.CompareDepths;
            
            m_MainLightShadowmapTexture?.Release();
            m_MainLightShadowmapTexture = RTHandles.Alloc(rtd, FilterMode.Point, TextureWrapMode.Clamp,
                isShadowMap: true, name: "_MainLightShadowmapTexture");
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CullingResults cullResults = ref renderingData.cullResults;
            ref LightData lightData = ref renderingData.lightData;
            VisibleLight shadowLight = lightData.visibleLights[lightData.mainLightIndex];
            ref CommandBuffer cmd = ref renderingData.commandBuffer;

            ShadowDrawingSettings settings = new ShadowDrawingSettings(cullResults,0,BatchCullingProjectionType.Orthographic);
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, 
                renderingData.cameraData.camera.transform.position);
            
            settings.splitData = m_CascadeSlices[0].splitData;

            Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, lightData.mainLightIndex, 
                ref renderingData.shadowData, m_CascadeSlices[0].projectionMatrix,
                m_CascadeSlices[0].resolution);
            
            cmd.SetGlobalVector("_ShadowBias", shadowBias);
            Vector3 lightDirection = -shadowLight.localToWorldMatrix.GetColumn(2);
            cmd.SetGlobalVector("_LightDirection", 
                new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));
            Vector3 lightPosition = shadowLight.localToWorldMatrix.GetColumn(3);
            cmd.SetGlobalVector("_LightPosition", 
                new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1.0f));
            
            cmd.SetGlobalDepthBias(1.0f, 2.5f);
            cmd.SetViewport(new Rect(m_CascadeSlices[0].offsetX, m_CascadeSlices[0].offsetY, 
                m_CascadeSlices[0].resolution, m_CascadeSlices[0].resolution));
            cmd.SetViewProjectionMatrices(m_CascadeSlices[0].viewMatrix, m_CascadeSlices[0].projectionMatrix);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            cmd.SetRenderTarget(m_MainLightShadowmapTexture.rt);
            cmd.ClearRenderTarget(true, false, Color.black); // 清空 RenderTexture
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            context.DrawShadows(ref settings);
            cmd.DisableScissorRect();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.SetGlobalDepthBias(0.0f, 0.0f); // Restore previous depth bias values
            
            Matrix4x4 noOpShadowMatrix = Matrix4x4.zero;
            noOpShadowMatrix.m22 = (SystemInfo.usesReversedZBuffer) ? 1.0f : 0.0f;
            
            m_MainLightShadowMatrices[0] = m_CascadeSlices[0].projectionMatrix * m_CascadeSlices[0].viewMatrix;
            cmd.SetGlobalMatrix(m_WorldToShadow, m_CascadeSlices[0].shadowTransform);
            //SetupMainLightShadowReceiverConstants(cmd, ref shadowLight, ref renderingData.shadowData);
            cmd.SetGlobalTexture(m_MainLightShadowmapID, m_MainLightShadowmapTexture.nameID);
            Debug.Log("Shadow");
        }
        
        public MainLightShadowCasterPass(RenderPassEvent evt)
        {
            m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
            m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
            m_CascadeSplitDistances = new Vector4[k_MaxCascades];
            renderPassEvent = evt;
        }
    }
}