using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BatchRenderer
{
    private readonly Dictionary<int, (Material material, Mesh mesh)> _renderInfos = new();
    private readonly Dictionary<int, RenderGroup> _renderGroups = new();

    public void AddInfo(int id, Material material, Mesh mesh)
    {
        _renderInfos.Add(id, (material, mesh));
    }

    public void AddItem(int id, RenderObject renderObject)
    {
        if (!_renderGroups.TryGetValue(id, out RenderGroup group))
        {
            (Material material, Mesh mesh) = _renderInfos[id];
            group = new RenderGroup(material, mesh);
            _renderGroups.Add(id, group);
        }
        group.Add(renderObject);
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
