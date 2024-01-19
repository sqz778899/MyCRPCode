using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{

    public struct CameraData
    {
        public Camera camera;
        public float maxShadowDistance;
        public ScriptableRenderer renderer;
        public RenderTextureDescriptor cameraTargetDescriptor;
    }

    public struct LightData
    {
        public int mainLightIndex;
        public NativeArray<VisibleLight> visibleLights;
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
    internal static class GlobaName
    {
        public static readonly string colorRTName = "_MainColorRT";
        public static readonly string depthRTName = "_CameraDepthAttachment";
        public static readonly string shadowMapName = "_MainLightShadowmapTexture";
    }

    internal static class ShaderPropertyId
    {
        public static readonly int worldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
        //...............Shadow About.................
        public static readonly int worldToShadowMatrix = Shader.PropertyToID("_MainLightWorldToShadow");
        public static readonly int shadowmapID = Shader.PropertyToID(GlobaName.shadowMapName);
        public static readonly int cascadeShadowSplitSpheres0 = Shader.PropertyToID("_CascadeShadowSplitSpheres0");
        public static readonly int cascadeShadowSplitSpheres1 = Shader.PropertyToID("_CascadeShadowSplitSpheres1");
        public static readonly int cascadeShadowSplitSpheres2 = Shader.PropertyToID("_CascadeShadowSplitSpheres2");
        public static readonly int cascadeShadowSplitSpheres3 = Shader.PropertyToID("_CascadeShadowSplitSpheres3");
        public static readonly int cascadeShadowSplitSphereRadii = Shader.PropertyToID("_CascadeShadowSplitSphereRadii");
    }

    internal static class GlobalVector
    {
        public static readonly string shadowBias = "_ShadowBias";
        public static readonly string lightDir = "_LightDirection";
        public static readonly string lightPos = "_LightPosition";
    }
    public sealed partial class CRPipeline
    {
        static List<Vector4> m_ShadowBiasData = new List<Vector4>();
        static List<int> m_ShadowResolutionData = new List<int>();
        public static CRPipelineAsset asset
        {
            get => GraphicsSettings.currentRenderPipeline as CRPipelineAsset;
        }
    }
}