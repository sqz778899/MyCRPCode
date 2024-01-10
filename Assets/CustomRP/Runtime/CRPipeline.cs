using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public sealed partial class CRPipeline : RenderPipeline
    {
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            Camera camera = cameras[0];
            RenderCameraStack(context, camera);
        }

        static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera)
        {
            //Step1. SetCamera
            CameraData cameraData = new CameraData();
            cameraData.camera = baseCamera;
            cameraData.maxShadowDistance = Mathf.Min(asset.maxShadowDistance, baseCamera.farClipPlane);
            context.SetupCameraProperties(baseCamera);
            //Step2. Rendering
            RenderSingleCamera(context, ref cameraData);
            //Step3. EndRendering
        }

        static void RenderSingleCamera(ScriptableRenderContext context, ref CameraData cameraData)
        {
            ref CRenderer renderer = ref asset.renderer;
            //Step1.........先搞到 ScriptableCullingParameters。第一个参数是用于VR渲染的，直接false
            cameraData.camera.TryGetCullingParameters(false, out var cullingParams);
            
            CommandBuffer cmd = CommandBufferPool.Get();
            renderer.Clear();
            //Step2.........对ScriptableCullingParameters进行一些初始化设置
            renderer.SetupCullingParameters(ref cullingParams, cameraData.maxShadowDistance);
            
            //Step3.........搞到最重要的CullingResults，得到了裁剪的结果
            var cullResults = context.Cull(ref cullingParams);
            //Step4.........把渲染所需要的所有东西，用RenderingData这个结构体塞进去
            InitializeRenderingData(ref cullResults,ref cameraData,cmd, out var renderingData);
            //Step5.........把各个Pass装配，执行，提交，一条龙绘制出来
            renderer.Setup(context, ref renderingData);
            renderer.Execute(context, ref renderingData);
            context.Submit();
        }

        #region InitRendering
        static void InitializeRenderingData(ref CullingResults cullResults,
            ref CameraData cameraData, CommandBuffer cmd,out RenderingData renderingData)
        {
            renderingData = new RenderingData();
            NativeArray<VisibleLight> visiblelights = cullResults.visibleLights;

            renderingData.commandBuffer = cmd;
            renderingData.cullResults = cullResults;
            renderingData.cameraData = cameraData;
            InitializeLightData(visiblelights,ref renderingData,out bool isShadowOn);
            InitializeShadowData(ref renderingData,isShadowOn);
        }

        static void InitializeLightData(NativeArray<VisibleLight> visibleLights,ref RenderingData renderingData,out bool isShadowOn)
        {
            ref LightData lightData = ref renderingData.lightData;
            lightData.visibleLights = visibleLights;
            for (int i = 0; i < visibleLights.Length; i++)
            {
                VisibleLight currVisibleLight = visibleLights[i];
                Light curLight = currVisibleLight.light;
                if (curLight == RenderSettings.sun &&
                    currVisibleLight.lightType == LightType.Directional)
                {
                    lightData.mainLightIndex = i;
                    isShadowOn = curLight.shadows != LightShadows.None;
                    return;
                }
            }
            isShadowOn = false;
        }

        static void InitializeShadowData(ref RenderingData renderingData,bool isShadowOn)
        {
            ref ShadowData shadowData = ref renderingData.shadowData;
            m_ShadowBiasData.Clear();
            m_ShadowResolutionData.Clear();
            Light mainLight = renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex].light;
            
            m_ShadowBiasData.Add(new Vector4(mainLight.shadowBias, 
                mainLight.shadowNormalBias, 0.0f, 0.0f));
            m_ShadowResolutionData.Add((int)mainLight.shadowResolution);
            //.................Shadow Settings.................
            shadowData.supportsMainLightShadows = isShadowOn;
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
