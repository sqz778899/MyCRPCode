using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public enum RenderPassEvent
    {
        BeforeRendering = 0,
        BeforeRenderingShadows = 50,
        AfterRenderingShadows = 100,
        BeforeRenderingOpaques = 250,
        AfterRenderingOpaques = 300,
        BeforeRenderingSkybox = 350,
        AfterRenderingSkybox = 400,
        BeforeRenderingTransparents = 450,
        AfterRenderingTransparents = 500,
        BeforeRenderingPostProcessing = 550,
        AfterRenderingPostProcessing = 600,
        AfterRendering = 1000,
    }

    public abstract class ScriptableRenderPass
    {
        public RenderPassEvent renderPassEvent;
        public RTHandle colorAttachmentHandle;
        static public RTHandle k_CameraTarget = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);
        internal bool m_UsesRTHandles;
        RTHandle[] m_ColorAttachments;
        RenderTargetIdentifier[] m_ColorAttachmentIds;
        internal bool overrideCameraTarget { get; set; }
        internal bool useNativeRenderPass { get; set; }
        
        ClearFlag m_ClearFlag = ClearFlag.None;
        Color m_ClearColor = Color.black;
        
        
        public abstract void Execute(ScriptableRenderContext context, ref RenderingData renderingData);
        
        public virtual void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor){}
        
        public void ConfigureTarget(RTHandle colorAttachment)
        {
            m_UsesRTHandles = true;
            overrideCameraTarget = true;

            m_ColorAttachments[0] = colorAttachment;
            m_ColorAttachmentIds[0] = new RenderTargetIdentifier(colorAttachment.nameID, 0, CubemapFace.Unknown, -1);
            for (int i = 1; i < m_ColorAttachments.Length; ++i)
            {
                m_ColorAttachments[i] = null;
                m_ColorAttachmentIds[i] = 0;
            }
        }
        
        public void ConfigureClear(ClearFlag clearFlag, Color clearColor)
        {
            m_ClearFlag = clearFlag;
            m_ClearColor = clearColor;
        }

        public ScriptableRenderPass()
        {
            m_UsesRTHandles = true;
            m_ColorAttachments = new RTHandle[] { k_CameraTarget, null, null, null, null, null, null, null };
            m_ColorAttachmentIds = new RenderTargetIdentifier[] { k_CameraTarget.nameID, 0, 0, 0, 0, 0, 0, 0 };
        }
    }
}