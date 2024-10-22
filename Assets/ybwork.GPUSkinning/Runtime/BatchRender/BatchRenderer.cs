using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BatchRenderer
{
    private readonly Dictionary<int, RenderGroup> _renderGroups = new();

    public RenderGroup AddGroup(int id, float[] animaitonLengths, string[] customPropNames)
    {
        RenderGroup renderGroup = new RenderGroup(animaitonLengths, customPropNames);
        _renderGroups.Add(id, renderGroup);
        return renderGroup;
    }

    public RenderObject CreateRenderObject(int id)
    {
        return _renderGroups[id].CreateRenderObject();
    }

    public void Update(float deltaTime)
    {
        foreach (RenderGroup renderGroup in _renderGroups.Values)
            renderGroup.Update(deltaTime);
    }

    /// <summary>
    /// 允许在渲染管线的RendererFeature中调用
    /// </summary>
    public RenderRusult Render(CommandBuffer cmd, Camera camera, int shaderPass)
    {
        RenderRusult renderRusult = new();
        foreach (RenderGroup renderGroup in _renderGroups.Values)
        {
            renderRusult += renderGroup.Draw(cmd, camera, shaderPass);
        }
        return renderRusult;
    }

    /// <summary>
    /// 允许在Update中调用
    /// </summary>
    public RenderRusult Render(Camera camera, bool isCastShadow, bool receiveShadows)
    {
        RenderRusult renderRusult = new();
        foreach (RenderGroup renderGroup in _renderGroups.Values)
        {
            renderRusult += renderGroup.Draw(camera, isCastShadow, receiveShadows);
        }
        return renderRusult;
    }
}

public struct RenderRusult
{
    public int MeshCount;
    public int MeshVertexCount;
    public int InstanceCount;
    public int InstanceFaceCount;

    public static RenderRusult operator +(RenderRusult v1, RenderRusult v2)
    {
        return new RenderRusult
        {
            MeshCount = v1.MeshCount + v2.MeshCount,
            MeshVertexCount = v1.MeshVertexCount + v2.MeshVertexCount,
            InstanceCount = v1.InstanceCount + v2.InstanceCount,
            InstanceFaceCount = v1.InstanceFaceCount + v2.InstanceFaceCount,
        };
    }
}
