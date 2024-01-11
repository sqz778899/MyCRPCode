using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public class MainLightShadowCasterPass: ScriptableRenderPass
    {
        const int k_MaxCascades = 4;
        int m_ShadowCasterCascadesCount;
        
        internal RTHandle m_MainLightShadowmapTexture;
        
        Matrix4x4[] m_MainLightShadowMatrices;
        Vector4[] m_CascadeSplitDistances;
        ShadowSliceData[] m_CascadeSlices;
        
        public void Setup(ref RenderingData renderingData)
        {
            int shadowLightIndex = renderingData.lightData.mainLightIndex;
            Light mainlight = renderingData.lightData.visibleLights[shadowLightIndex].light;
            
            ref CullingResults cullingResults = ref renderingData.cullResults;
            ref ShadowData shadowData = ref renderingData.shadowData;
            ref ShadowSliceData shadowSliceData = ref m_CascadeSlices[0];
            int shadowResolution = shadowData.resolution[0];
            
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                shadowLightIndex, 0,
                shadowData.mainLightShadowCascadesCount, shadowData.mainLightShadowCascadesSplit, 
                shadowResolution, mainlight.shadowNearPlane,
                out shadowSliceData.viewMatrix, out shadowSliceData.projectionMatrix, out shadowSliceData.splitData);
            
            m_CascadeSplitDistances[0] = shadowSliceData.splitData.cullingSphere;
            shadowSliceData.offsetX = 0;
            shadowSliceData.offsetY = 0;
            shadowSliceData.resolution = shadowResolution;
            shadowSliceData.shadowTransform = ShadowUtils.GetShadowTransform(shadowSliceData.projectionMatrix, shadowSliceData.viewMatrix);
            shadowSliceData.splitData.shadowCascadeBlendCullingFactor = 1.0f;

            CreateShadowRT(ref renderingData);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CullingResults cullResults = ref renderingData.cullResults;
            ref LightData lightData = ref renderingData.lightData;
            int mainLightIndex = lightData.mainLightIndex;
            VisibleLight shadowLight = lightData.visibleLights[mainLightIndex];
            ref CommandBuffer cmd = ref renderingData.commandBuffer;

            ShadowDrawingSettings settings = new ShadowDrawingSettings(
                cullResults,mainLightIndex, BatchCullingProjectionType.Orthographic);
            
            Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, lightData.mainLightIndex, 
                ref renderingData.shadowData, m_CascadeSlices[0].projectionMatrix, m_CascadeSlices[0].resolution);
            //Set一些全局变量给Shader用
            cmd.SetGlobalVector(GlobalVector.shadowBias, shadowBias);
            Vector3 lightDirection = -shadowLight.localToWorldMatrix.GetColumn(2);
            cmd.SetGlobalVector(GlobalVector.lightDir, 
                new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, 0.0f));
            Vector3 lightPosition = shadowLight.localToWorldMatrix.GetColumn(3);
            cmd.SetGlobalVector(GlobalVector.lightPos, 
                new Vector4(lightPosition.x, lightPosition.y, lightPosition.z, 1.0f));
            //准备绘制Shadow！！
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
            //绘制Shadow！！
            context.DrawShadows(ref settings);
            //结束绘制Shadow，把一些变量还原
            cmd.DisableScissorRect();//画完阴影之后，记得关闭裁剪，不然后续渲染会被这个裁剪影响。
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.SetGlobalDepthBias(0.0f, 0.0f); // 还原偏移值
            //设置采样阴影贴图的矩阵和贴图，为后续阴影Shader采样提供数据
            cmd.SetGlobalMatrix(ShaderPropertyId.worldToShadowMatrix, m_CascadeSlices[0].shadowTransform);
            cmd.SetGlobalTexture(ShaderPropertyId.shadowmapID, m_MainLightShadowmapTexture.nameID);
        }

        #region MyRegion
        public MainLightShadowCasterPass(RenderPassEvent evt)
        {
            m_MainLightShadowMatrices = new Matrix4x4[k_MaxCascades + 1];
            m_CascadeSlices = new ShadowSliceData[k_MaxCascades];
            m_CascadeSplitDistances = new Vector4[k_MaxCascades];
            renderPassEvent = evt;
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
                isShadowMap: true, name: GlobaName.shadowMapName);
        }
        #endregion
    }
}