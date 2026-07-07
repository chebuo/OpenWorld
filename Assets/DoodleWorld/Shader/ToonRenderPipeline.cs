using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Rendering.Toon
{
    public class ToonRenderPipeline : RenderPipeline
    {
        ToonRenderPipelineAsset asset;
        CommandBuffer[] commandBuffers;
        public ToonRenderPipeline(ToonRenderPipelineAsset asset)
        {
            this.asset = asset;
            commandBuffers=new CommandBuffer[1];
            // CommandBufferの事前生成
            for (int i = 0; i < commandBuffers.Length; i++)
            {
                commandBuffers[i] = new CommandBuffer();
                commandBuffers[i].name = "ToonRP";
            }
        }
        protected override void Render(ScriptableRenderContext context,List<Camera> cameras)
        {
            foreach(var camera in cameras)
            {
                context.SetupCameraProperties(camera);

                if(!camera.TryGetCullingParameters(out var cullingParams))continue;

                var cullingResults=context.Cull(ref cullingParams);

                var cmd = commandBuffers[0];
                cmd.Clear();
                cmd.ClearRenderTarget(true,true,Color.black);

                var shaderTagId=new ShaderTagId("SRPDefaultUnlit");
                var rendererListDesc=new RendererListDesc(
                    shaderTagId,
                    cullingResults,
                    camera)
                {
                    renderQueueRange=RenderQueueRange.all,
                    sortingCriteria=SortingCriteria.CommonOpaque
                };
                var rendererList=context.CreateRendererList(rendererListDesc);

                cmd.DrawRendererList(rendererList);

                context.ExecuteCommandBuffer(cmd);
                context.Submit();
            }
        }
    }
}
