
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    const string bufferName = "Render Camera";
    private static readonly ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    CommandBuffer _buffer = new CommandBuffer {name = bufferName};
    CullingResults _cullingResults;

#if UNITY_EDITOR
    string SampleName { get; set; }
#else
    const string SampleName = bufferName;
#endif
    
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }
        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }
    

    bool Cull()
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            _cullingResults = _context.Cull(ref p);
            return true;
        }
        return false;
    }
    
    void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(_camera);
        var drawSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
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
