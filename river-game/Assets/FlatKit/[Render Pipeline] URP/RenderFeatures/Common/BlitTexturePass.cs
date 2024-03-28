using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FlatKit {
internal class BlitTexturePass : ScriptableRenderPass {
    public static readonly string CopyEffectShaderName = "Hidden/FlatKit/CopyTexture";

    private readonly ProfilingSampler _profilingSampler;
    private readonly Material _effectMaterial;
    private readonly ScriptableRenderPassInput _passInput;
    private readonly Material _copyMaterial;

#if UNITY_2022_1_OR_NEWER
    private RTHandle _temporaryColorTexture;
#else
    private RenderTargetHandle _temporaryColorTexture;
#endif

    public BlitTexturePass(Material effectMaterial, bool useDepth, bool useNormals, bool useColor) {
        _effectMaterial = effectMaterial;
        var name = effectMaterial.name.Substring(effectMaterial.name.LastIndexOf('/') + 1);
        _profilingSampler = new ProfilingSampler($"Blit {name}");
        _passInput = (useColor ? ScriptableRenderPassInput.Color : ScriptableRenderPassInput.None) |
                     (useDepth ? ScriptableRenderPassInput.Depth : ScriptableRenderPassInput.None) |
                     (useNormals ? ScriptableRenderPassInput.Normal : ScriptableRenderPassInput.None);

#if !UNITY_2022_1_OR_NEWER
        _copyMaterial = CoreUtils.CreateEngineMaterial(CopyEffectShaderName);
#endif
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
        ConfigureInput(_passInput);
        base.Configure(cmd, cameraTextureDescriptor);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
#if !UNITY_2022_1_OR_NEWER
        ConfigureTarget(new RenderTargetIdentifier(renderingData.cameraData.renderer.cameraColorTarget, 0,
            CubemapFace.Unknown, -1));
        _temporaryColorTexture.Init("_EffectTexture");
#endif
    }

    public void Setup(RenderingData renderingData) {
#if UNITY_2022_1_OR_NEWER
        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref _temporaryColorTexture, descriptor, wrapMode: TextureWrapMode.Clamp,
            name: "_EffectTexture");
#endif
    }

    public void Dispose() {
#if UNITY_2022_1_OR_NEWER
        _temporaryColorTexture?.Release();
#endif
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        if (_effectMaterial == null) return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, _profilingSampler)) {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            SetSourceSize(cmd, descriptor);

#if UNITY_2022_1_OR_NEWER
            var cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            // cmd.GetTemporaryRT(Shader.PropertyToID(_temporaryColorTexture.name), descriptor);
#else
            var cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(_temporaryColorTexture.id, descriptor);
#endif

            // Also seen as `renderingData.cameraData.xr.enabled` and `#if ENABLE_VR && ENABLE_XR_MODULE`.
            if (renderingData.cameraData.xrRendering) {
                _effectMaterial.EnableKeyword("_USE_DRAW_PROCEDURAL"); // `UniversalRenderPipelineCore.cs`.
#if UNITY_2022_1_OR_NEWER
#pragma warning disable CS0618
                cmd.SetRenderTarget(_temporaryColorTexture);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _effectMaterial, 0, 0);
                cmd.SetGlobalTexture("_EffectTexture", _temporaryColorTexture);
                cmd.SetRenderTarget(new RenderTargetIdentifier(cameraTargetHandle, 0, CubemapFace.Unknown, -1));
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _copyMaterial, 0, 0);
#else
                cmd.SetRenderTarget(_temporaryColorTexture.Identifier());
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _effectMaterial, 0, 0);
                cmd.SetGlobalTexture("_EffectTexture", _temporaryColorTexture.Identifier());
                cmd.SetRenderTarget(new RenderTargetIdentifier(cameraTargetHandle, 0, CubemapFace.Unknown, -1));
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _copyMaterial, 0, 0);
#endif
            } else {
                _effectMaterial.DisableKeyword("_USE_DRAW_PROCEDURAL");
                // Note: `FinalBlitPass` has `cmd.SetRenderTarget` at this point, but it's unclear what that does.
#if UNITY_2022_1_OR_NEWER
                cmd.Blit(cameraTargetHandle, _temporaryColorTexture, _effectMaterial, 0);
                cmd.Blit(_temporaryColorTexture, cameraTargetHandle);
#else
                cmd.Blit(cameraTargetHandle, _temporaryColorTexture.Identifier(), _effectMaterial, 0);
                cmd.Blit(_temporaryColorTexture.Identifier(), cameraTargetHandle);
#endif
            }
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    // Copied from `PostProcessUtils.cs`.
    private static void SetSourceSize(CommandBuffer cmd, RenderTextureDescriptor desc) {
        float width = desc.width;
        float height = desc.height;
        if (desc.useDynamicScale) {
            width *= ScalableBufferManager.widthScaleFactor;
            height *= ScalableBufferManager.heightScaleFactor;
        }

        cmd.SetGlobalVector("_SourceSize", new Vector4(width, height, 1.0f / width, 1.0f / height));
    }
}
}