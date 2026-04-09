using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Water_Volume : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private RTHandle _source;
        private readonly Material _material;

        private RTHandle _tempColorTarget;

        public CustomRenderPass(Material mat)
        {
            _material = mat;
        }

        public void Setup(RTHandle source)
        {
            _source = source;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref _tempColorTarget, desc, name: "_TemporaryColourTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Reflection)
                return;

            if (_material == null || _source == null)
                return;

            CommandBuffer commandBuffer = CommandBufferPool.Get();

            Blit(commandBuffer, _source, _tempColorTarget, _material);
            Blit(commandBuffer, _tempColorTarget, _source);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    [System.Serializable]
    public class _Settings
    {
        //[HideInInspector]
        public Material material = null;
        public RenderPassEvent renderPass = RenderPassEvent.AfterRenderingSkybox;
    }

    public _Settings settings = new _Settings();

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        if(settings.material == null)
        {
            settings.material = Resources.Load<Material>("Water_Volume");
        }

        m_ScriptablePass = new CustomRenderPass(settings.material);

        // Configures where the render pass should be injected.
        //m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        m_ScriptablePass.renderPassEvent = settings.renderPass;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {       
        if (settings.material == null)
            return;

        m_ScriptablePass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


