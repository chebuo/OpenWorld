using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.Toon
{
    [ExecuteInEditMode]
    [CreateAssetMenu(menuName="ToonRenderPipelineAsset")]
    public class ToonRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]private float modelRenderResolutionRate=0.7f;
        public float ModelRenderResolutionRate=>modelRenderResolutionRate;

        protected override RenderPipeline CreatePipeline()
        {
            return new ToonRenderPipeline(this);
        }
    }
}