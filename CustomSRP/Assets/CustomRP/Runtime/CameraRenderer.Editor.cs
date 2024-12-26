
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRenderer
{
#if UNITY_EDITOR
    

    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    
    static Material errorMaterial;

    partial void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var sortingSettings = new SortingSettings(_camera);
        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], sortingSettings);
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        drawingSettings.overrideMaterial = errorMaterial;
        var filterSettings = new FilteringSettings(RenderQueueRange.all);
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filterSettings);
    }

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }
    
    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }
    
    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        _buffer.name =SampleName= _camera.name;
        Profiler.EndSample();
    }
    
#endif
}
