using UnityEngine;
using UnityEngine.Rendering;


public class Shadows
{

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    const int maxShadowedDirectionalLightCount = 4;
    const string bufferName = "Shadows";
    private CommandBuffer buffer = new CommandBuffer() { name = bufferName };

    ScriptableRenderContext context;

    CullingResults cullingResults;

    ShadowSettings shadowSettings;

    int shadowedDirectionalLightCount;

    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults,ShadowSettings shadowSettings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;
        shadowedDirectionalLightCount = 0;

    }

    void DrawBoundsWireframe(Bounds bounds, Color color, float duration = 0.0f)
    {
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
        
        // 计算8个顶点
        Vector3[] points = new Vector3[8];
        points[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
        points[1] = center + new Vector3(size.x, -size.y, -size.z) * 0.5f;
        points[2] = center + new Vector3(size.x, size.y, -size.z) * 0.5f;
        points[3] = center + new Vector3(-size.x, size.y, -size.z) * 0.5f;
        points[4] = center + new Vector3(-size.x, -size.y, size.z) * 0.5f;
        points[5] = center + new Vector3(size.x, -size.y, size.z) * 0.5f;
        points[6] = center + new Vector3(size.x, size.y, size.z) * 0.5f;
        points[7] = center + new Vector3(-size.x, size.y, size.z) * 0.5f;
        
        // 绘制12条边
        Debug.DrawLine(points[0], points[1], color, duration);
        Debug.DrawLine(points[1], points[2], color, duration);
        Debug.DrawLine(points[2], points[3], color, duration);
        Debug.DrawLine(points[3], points[0], color, duration);
        
        Debug.DrawLine(points[4], points[5], color, duration);
        Debug.DrawLine(points[5], points[6], color, duration);
        Debug.DrawLine(points[6], points[7], color, duration);
        Debug.DrawLine(points[7], points[4], color, duration);
        
        Debug.DrawLine(points[0], points[4], color, duration);
        Debug.DrawLine(points[1], points[5], color, duration);
        Debug.DrawLine(points[2], points[6], color, duration);
        Debug.DrawLine(points[3], points[7], color, duration);
    }

    
    public void ReserveDirectionalShadows(Light light,int visibleLightIndex)
    {
        if(shadowedDirectionalLightCount < maxShadowedDirectionalLightCount&&
            light.shadows != LightShadows.None&&light.shadowStrength > 0f&&
            cullingResults.GetShadowCasterBounds(visibleLightIndex,out Bounds bounds)
        )
        {
            //DrawBoundsWireframe(bounds, Color.red, 0.1f);
            shadowedDirectionalLights[shadowedDirectionalLightCount++] = new ShadowedDirectionalLight{visibleLightIndex = visibleLightIndex};
            
        }

    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)shadowSettings.directional.atlasSize;
        
        buffer.GetTemporaryRT(dirShadowAtlasId,atlasSize,atlasSize,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true,false,Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i,atlasSize);
        }
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows(int index, int tileSize)
    {
        var light = shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex,BatchCullingProjectionType.Orthographic);
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1, Vector3.zero,
            tileSize, 0f, out Matrix4x4 viewMatrix
            , out Matrix4x4 projMatrix, out ShadowSplitData shadowSplit);
        shadowSettings.splitData = shadowSplit;
        buffer.SetViewProjectionMatrices(viewMatrix,projMatrix);
        //buffer.SetViewport();
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);

    }


    /// <summary>
    /// Releases the temporary render target used for shadow rendering and executes the command buffer to apply changes.
    /// This method should be called when shadow rendering is no longer needed, such as during cleanup or at the end of a frame.
    /// </summary>
    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }
    
    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            buffer.GetTemporaryRT(dirShadowAtlasId,1,1,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        }
    }
    

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}
