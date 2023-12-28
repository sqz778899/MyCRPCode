using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public struct CameraData
    {
        public Camera camera;
        public ScriptableRenderer renderer;
        public RenderTextureDescriptor cameraTargetDescriptor;
    }

    public struct LightData
    {
        public VisibleLight visibleLight;
    }

    public struct ShadowData
    {
        public bool supportsMainLightShadows;
        public int mainLightShadowmapWidth;
        public int mainLightShadowmapHeight;
        public int mainLightShadowCascadesCount;
        public Vector3 mainLightShadowCascadesSplit;
        public float mainLightShadowCascadeBorder;
        public List<Vector4> bias;
        public List<int> resolution;
    }

    public struct RenderingData
    {
        internal CommandBuffer commandBuffer;
        public CullingResults cullResults;
        public CameraData cameraData;
        public LightData lightData;
        public ShadowData shadowData;
    }
    
    public sealed partial class CRPipeline
    {
        static List<Vector4> m_ShadowBiasData = new List<Vector4>();
        static List<int> m_ShadowResolutionData = new List<int>();
        public static CRPipelineAsset asset
        {
            get => GraphicsSettings.currentRenderPipeline as CRPipelineAsset;
        }
        static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, float renderScale)
        {
            int scaledWidth = (int)(camera.pixelWidth * renderScale);
            int scaledHeight = (int)(camera.pixelHeight * renderScale);
            
            RenderTextureDescriptor desc;
            if (camera.targetTexture == null)
                desc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);
            else
                desc = camera.targetTexture.descriptor;
            
            desc.width = scaledWidth;
            desc.height = scaledHeight;
            return desc;
        }
    }
}