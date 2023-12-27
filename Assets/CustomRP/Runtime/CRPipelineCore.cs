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
    
    public sealed partial class CRPipeline
    {
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