using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public sealed partial class CRPipeline : RenderPipeline
    {
        //private readonly CRPipelineAsset pipelineAsset;
        //public const string k_ShaderTagName = "UniversalPipeline";
        
        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            
            Camera camera = cameras[0];
            RenderCameraStack(context, camera);
            /*camera.TryGetCullingParameters(false, out var cullingParameters);
            CullingResults cullingResults = context.Cull(ref cullingParameters);


            context.SetupCameraProperties(camera);
            context.DrawSkybox(camera);

            SortingSettings sortingSettings = new SortingSettings(camera);
            DrawingSettings drawingSettings = new DrawingSettings(
                unlitShaderTagId, sortingSettings);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            context.DrawRenderers(
                cullingResults, ref drawingSettings, ref filteringSettings
            );

            context.Submit();*/
        }

        static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera)
        {
            //Step1. SetCamera
            CameraData CameraData = new CameraData();
            CameraData.camera = baseCamera;
            CameraData.cameraTargetDescriptor = CreateRenderTextureDescriptor(baseCamera,asset.renderScale);
            //Step2. Rendering
            RenderSingleCamera(context, ref CameraData);
            //Step3. EndRendering
        }

        static void RenderSingleCamera(ScriptableRenderContext context, ref CameraData cameraData)
        {
            ref CRenderer renderer = ref asset.renderer;
            cameraData.camera.TryGetCullingParameters(false, out var cullingParams);
            
            CommandBuffer cmd = CommandBufferPool.Get();
            renderer.Clear();
            renderer.SetupCullingParameters(ref cullingParams, asset.maxShadowDistance);
            var cullResults = context.Cull(ref cullingParams);
            InitializeRenderingData(ref cullResults,ref cameraData,cmd, out var renderingData);
            
            renderer.Setup(context, ref renderingData);
            renderer.Execute(context, ref renderingData);
            context.Submit();
        }

        #region InitRendering
        static void InitializeRenderingData(ref CullingResults cullResults,
            ref CameraData cameraData, CommandBuffer cmd,out RenderingData renderingData)
        {
            renderingData = new RenderingData();
            VisibleLight visiblelight = cullResults.visibleLights[0];
            bool mainLightCastShadows = visiblelight.light.shadows != LightShadows.None;
            
            renderingData.commandBuffer = cmd;
            renderingData.cullResults = cullResults;
            renderingData.cameraData = cameraData;
            InitializeLightData(visiblelight,ref renderingData);
            InitializeShadowData(visiblelight,ref renderingData,mainLightCastShadows);
        }

        static void InitializeLightData(VisibleLight visiblelight,ref RenderingData renderingData)
        {
            ref LightData lightData = ref renderingData.lightData;
            lightData.visibleLight = visiblelight;
        }

        static void InitializeShadowData(VisibleLight visiblelight,ref RenderingData renderingData,bool mainLightCastShadows)
        {
            ref ShadowData shadowData = ref renderingData.shadowData;
            m_ShadowBiasData.Clear();
            m_ShadowResolutionData.Clear();
            m_ShadowBiasData.Add(new Vector4(visiblelight.light.shadowBias, 
                visiblelight.light.shadowNormalBias, 0.0f, 0.0f));
            m_ShadowResolutionData.Add((int)visiblelight.light.shadowResolution);
            //.................Shadow Settings.................
            shadowData.supportsMainLightShadows = mainLightCastShadows;
            shadowData.bias = m_ShadowBiasData;
            shadowData.resolution = m_ShadowResolutionData;
            shadowData.mainLightShadowCascadesCount = asset.shadowCascadeCount;
            shadowData.mainLightShadowCascadeBorder = asset.cascadeBorder;
            shadowData.mainLightShadowmapWidth = asset.mainLightShadowmapResolution;
            shadowData.mainLightShadowmapHeight = asset.mainLightShadowmapResolution;
        }
        #endregion
    }
}
