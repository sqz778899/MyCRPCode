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

    public class ScriptableRenderPass
    {
        public RenderPassEvent renderPassEvent;
    }
}