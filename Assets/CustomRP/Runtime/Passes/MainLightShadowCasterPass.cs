﻿using UnityEngine;
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
            Debug.Log("Shadow Setup");
            
            Light light = renderingData.lightData.visibleLight.light;
            renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out Bounds bounds);

            int shadowResolution = renderingData.shadowData.mainLightShadowmapWidth;
            renderTargetWidth = renderingData.shadowData.mainLightShadowmapWidth;
            renderTargetHeight = renderingData.shadowData.mainLightShadowmapHeight;
            
            ref CullingResults cullingResults = ref renderingData.cullResults;
            ref ShadowData shadowData = ref renderingData.shadowData;
            ref ShadowSliceData shadowSliceData = ref m_CascadeSlices[0];
            
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex,
                0, shadowData.mainLightShadowCascadesCount, shadowData.mainLightShadowCascadesSplit, 
                shadowResolution, light.shadowNearPlane, out shadowSliceData.viewMatrix, out shadowSliceData.projectionMatrix,
                out shadowSliceData.splitData);
            
            m_CascadeSplitDistances[0] = shadowSliceData.splitData.cullingSphere;
            shadowSliceData.offsetX = 0;
            shadowSliceData.offsetY = 0;
            shadowSliceData.resolution = shadowResolution;
            shadowSliceData.shadowTransform = ShadowUtils.GetShadowTransform(shadowSliceData.projectionMatrix, shadowSliceData.viewMatrix);
            shadowSliceData.splitData.shadowCascadeBlendCullingFactor = 1.0f;
            
            RenderTextureDescriptor rtd = new RenderTextureDescriptor(renderTargetWidth, renderTargetHeight,
                GraphicsFormat.None, GraphicsFormat.D16_UNorm);
            rtd.shadowSamplingMode = ShadowSamplingMode.CompareDepths;
            
            m_MainLightShadowmapTexture?.Release();
            m_MainLightShadowmapTexture = RTHandles.Alloc(rtd, FilterMode.Point, TextureWrapMode.Clamp,
                isShadowMap: true, name: "_MainLightShadowmapTexture");

            m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
            m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
            m_CreateEmptyShadowmap = false;
            useNativeRenderPass = true;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CullingResults cullResults = ref renderingData.cullResults;
            ref LightData lightData = ref renderingData.lightData;
            ref VisibleLight shadowLight = ref lightData.visibleLight;
            ref CommandBuffer cmd = ref renderingData.commandBuffer;

            ShadowDrawingSettings settings = new ShadowDrawingSettings(cullResults,0,BatchCullingProjectionType.Orthographic);
            cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, 
                renderingData.cameraData.camera.transform.position);
            
            settings.splitData = m_CascadeSlices[0].splitData;

            Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, shadowLightIndex, 
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
            cmd.ClearRenderTarget(true, true, Color.black); // 清空 RenderTexture
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            context.DrawShadows(ref settings);
            cmd.DisableScissorRect();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.SetGlobalDepthBias(0.0f, 0.0f); // Restore previous depth bias values

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