namespace CustomRenderPipeline
{
    public class CRendererData: ScriptableRenderData
    {
        protected override ScriptableRenderer Create()
        {
            return new CRenderer(this);
        }
    }
}