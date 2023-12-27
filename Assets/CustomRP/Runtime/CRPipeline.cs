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
            camera.TryGetCullingParameters(false, out var cullingParameters);
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

            context.Submit();
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
            
        }
    }
}
