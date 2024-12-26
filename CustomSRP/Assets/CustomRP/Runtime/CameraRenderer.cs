
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    const string bufferName = "Render Camera";
    private static readonly ShaderTagId unlitShaderTagId = new("SRPDefaultUnlit"),
        litShaderTagId = new("CustomLit");

    CommandBuffer _buffer = new CommandBuffer {name = bufferName};
    CullingResults _cullingResults;

#if UNITY_EDITOR
    string SampleName { get; set; }
#else
    const string SampleName = bufferName;
#endif

    private Lighting lighting = new Lighting();
    public void Render(ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing,ShadowSettings shadowSettings)
    {
        _context = context;
        _camera = camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance))
        {
            return;
        }
        _buffer.BeginSample(SampleName);
        ExecuteBuffer();
        lighting.Setup(context, _cullingResults, shadowSettings );
        _buffer.EndSample(SampleName);
        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        lighting.Cleanup();
        Submit();
    }
    

    bool Cull(float maxDistance)
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            p.shadowDistance = Mathf.Min(maxDistance, _camera.farClipPlane);
            _cullingResults = _context.Cull(ref p);
            return true;
        }
        return false;
    }
    
    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(_camera);
        var drawSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawSettings.SetShaderPassName(1, litShaderTagId);
        var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
        _context.DrawRenderers(_cullingResults, ref drawSettings, ref filterSettings);
        _context.DrawSkybox(_camera);
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawSettings.sortingSettings = sortingSettings;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingResults, ref drawSettings, ref filterSettings);
    }

    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    partial void PrepareForSceneWindow();
    
    partial void PrepareBuffer();
    void Setup()
    {
        _context.SetupCameraProperties(_camera);
        var flags = _camera.clearFlags;
        _buffer.ClearRenderTarget(flags<=CameraClearFlags.Depth, flags == CameraClearFlags.Color, flags == CameraClearFlags.Color ?
            _camera.backgroundColor.linear : Color.clear);
        _buffer.BeginSample(SampleName);

        ExecuteBuffer();

    }

    void Submit()
    {
        _buffer.EndSample(SampleName);
        ExecuteBuffer();

        _context.Submit();
    }
    
    void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
}
