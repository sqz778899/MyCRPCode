using System;

namespace CustomRenderPipeline
{
    public abstract class ScriptableRenderer: IDisposable
    {
        public ScriptableRenderer(ScriptableRenderData data)
        {
            
        }
        //手动GC
        public void Dispose()
        {
            
        }
    }
}