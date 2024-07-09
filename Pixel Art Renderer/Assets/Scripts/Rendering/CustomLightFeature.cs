using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class CustomLightFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomLightSettings
    {
        public string lightBufferName = "_CustomLightBuffer";
        public string lightCountName = "_CustomLightCount";
        public Vector3[] lightPositions = new Vector3[1];
    }

    public CustomLightSettings settings = new CustomLightSettings();

    class CustomLightPass : ScriptableRenderPass
    {
        private string profilerTag;
        private CustomLightSettings settings;
        private ComputeBuffer lightBuffer;

        public CustomLightPass(string profilerTag, CustomLightSettings settings)
        {
            this.profilerTag = profilerTag;
            this.settings = settings;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (lightBuffer == null || lightBuffer.count != settings.lightPositions.Length)
            {
                lightBuffer?.Release();
                lightBuffer = new ComputeBuffer(settings.lightPositions.Length, sizeof(float) * 3);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            lightBuffer.SetData(settings.lightPositions);
            cmd.SetGlobalBuffer(settings.lightBufferName, lightBuffer);
            cmd.SetGlobalInt(settings.lightCountName, settings.lightPositions.Length);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // Do not release the buffer here, as it will be reused across frames
        }

        public void Cleanup()
        {
            lightBuffer?.Release();
        }
    }

    CustomLightPass customLightPass;

    public override void Create()
    {
        customLightPass = new CustomLightPass("CustomLightPass", settings)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customLightPass);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        customLightPass?.Cleanup();
    }
}