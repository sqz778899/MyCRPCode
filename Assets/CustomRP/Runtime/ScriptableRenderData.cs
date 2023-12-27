using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRenderPipeline
{
    public abstract class ScriptableRenderData
    {
        
        internal bool isInvalidated { get; set; }
        protected abstract ScriptableRenderer Create();
        
        internal ScriptableRenderer InternalCreateRenderer()
        {
            isInvalidated = false;
            return Create();
        }
        
    }
}