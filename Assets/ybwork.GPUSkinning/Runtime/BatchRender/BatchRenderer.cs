using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BatchRenderer
{
    private readonly Dictionary<int, RenderGroup> _renderGroups = new();

    public void AddGroup(int id, float[] animaitonLengths, Material material, Mesh mesh)
    {
        _renderGroups.Add(id, new RenderGroup(animaitonLengths, material, mesh));
    }

    public RenderObject CreateRenderObject(int id)
    {
        return _renderGroups[id].CreateRenderObject();
    }

    public void RemoveItem(int id, RenderObject renderObject)
    {
        _renderGroups[id].RemoveItem(renderObject);
    }

    public void Update(float deltaTime)
    {
        foreach (RenderGroup renderGroup in _renderGroups.Values)
            renderGroup.Update(deltaTime);
    }

    /// <summary>
    /// 允许在Update中或者渲染管线的RendererFeature中调用
    /// Update中不需要参数
    /// RendererFeature中需要传入CommandBuffer
    /// </summary>
    /// <param name="cmd"></param>
    public void Render(CommandBuffer cmd = null)
    {
        foreach (RenderGroup renderGroup in _renderGroups.Values)
            renderGroup.Draw(cmd);
    }
}
