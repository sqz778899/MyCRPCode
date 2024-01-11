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
            ref ShadowSliceData[] shadowSliceData = ref m_CascadeSlices;
            
            //
            int shadowResolution = GetMaxTileResolutionInAtlas(shadowData.mainLightShadowmapWidth,
                shadowData.mainLightShadowmapHeight,
                shadowData.mainLightShadowCascadesCount);
            for (int i = 0; i < shadowData.mainLightShadowCascadesCount; i++)
            {
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    shadowLightIndex, i,
                    shadowData.mainLightShadowCascadesCount, shadowData.mainLightShadowCascadesSplit, 
                    shadowResolution, mainlight.shadowNearPlane,
                    out shadowSliceData[i].viewMatrix, out shadowSliceData[i].projectionMatrix, out shadowSliceData[i].splitData);
                
                m_CascadeSplitDistances[i] = shadowSliceData[i].splitData.cullingSphere;
                shadowSliceData[i].offsetX = (i % 2) * shadowResolution;
                shadowSliceData[i].offsetY = (i / 2) * shadowResolution;
                shadowSliceData[i].resolution = shadowResolution;
                shadowSliceData[i].shadowTransform = ShadowUtils.GetShadowTransform(shadowSliceData[i].projectionMatrix, shadowSliceData[i].viewMatrix);
                shadowSliceData[i].splitData.shadowCascadeBlendCullingFactor = 1.0f;
            }

            CreateShadowRT(ref renderingData);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CullingResults cullResults = ref renderingData.cullResults;
            ref LightData lightData = ref renderingData.lightData;
            int mainLightIndex = lightData.mainLightIndex;
            VisibleLight shadowLight = lightData.visibleLights[mainLightIndex];
            ref CommandBuffer cmd = ref renderingData.commandBuffer;

            if (shadowLight.light.shadows == LightShadows.None)
                return;
            
            ShadowDrawingSettings settings = new ShadowDrawingSettings(
                cullResults,mainLightIndex, BatchCullingProjectionType.Orthographic);
            
            cmd.SetRenderTarget(m_MainLightShadowmapTexture.rt);
            cmd.ClearRenderTarget(true, false, Color.black); // 清空 RenderTexture
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            for (int i = 0; i < renderingData.shadowData.mainLightShadowCascadesCount; i++)
            {
                Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, lightData.mainLightIndex, 
                    ref renderingData.shadowData, m_CascadeSlices[i].projectionMatrix, m_CascadeSlices[i].resolution);
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
                cmd.SetViewport(new Rect(m_CascadeSlices[i].offsetX, m_CascadeSlices[i].offsetY, 
                    m_CascadeSlices[i].resolution, m_CascadeSlices[i].resolution));
                cmd.SetViewProjectionMatrices(m_CascadeSlices[i].viewMatrix, m_CascadeSlices[i].projectionMatrix);
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
                cmd.SetGlobalMatrix(ShaderPropertyId.worldToShadowMatrix, m_CascadeSlices[i].shadowTransform);
            }
            
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

        public int GetMaxTileResolutionInAtlas(int atlasWidth, int atlasHeight, int tileCount)
        {
            int resolution = Mathf.Min(atlasWidth, atlasHeight);
            int currentTileCount = atlasWidth / resolution * atlasHeight / resolution;
            while (currentTileCount < tileCount)
            {
                resolution = resolution >> 1;
                currentTileCount = atlasWidth / resolution * atlasHeight / resolution;
            }
            return resolution;
        }
        #endregion
    }
}