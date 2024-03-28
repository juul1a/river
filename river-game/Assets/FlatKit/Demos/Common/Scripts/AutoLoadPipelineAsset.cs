using UnityEngine;
using UnityEngine.Rendering;

namespace FlatKit {
[ExecuteAlways]
public class AutoLoadPipelineAsset : MonoBehaviour {
    [SerializeField]
    private RenderPipelineAsset pipelineAsset;
    private RenderPipelineAsset _previousPipelineAsset;
    private bool _overrodeQualitySettings;

    void OnEnable() {
        UpdatePipeline();
    }

    void OnDisable() {
        ResetPipeline();
    }

    private void OnValidate() {
        UpdatePipeline();
    }

    private void UpdatePipeline() {
        if (pipelineAsset) {
            if (QualitySettings.renderPipeline != null && QualitySettings.renderPipeline != pipelineAsset) {
                _previousPipelineAsset = QualitySettings.renderPipeline;
                QualitySettings.renderPipeline = pipelineAsset;
                _overrodeQualitySettings = true;
            } else if (GraphicsSettings.renderPipelineAsset != pipelineAsset) {
                _previousPipelineAsset = GraphicsSettings.renderPipelineAsset;
                GraphicsSettings.renderPipelineAsset = pipelineAsset;
                _overrodeQualitySettings = false;
            }
        }
    }

    private void ResetPipeline() {
        if (_previousPipelineAsset) {
            if (_overrodeQualitySettings) {
                QualitySettings.renderPipeline = _previousPipelineAsset;
            } else {
                GraphicsSettings.renderPipelineAsset = _previousPipelineAsset;
            }
        }
    }
}
}