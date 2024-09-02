using System.Collections.Generic;
using UnityEngine;

public class BatchRenderer
{
    private readonly Dictionary<int, (Material material, Mesh mesh)> _renderInfos = new();
    public Dictionary<int, RenderGroup> RenderGroups = new();

    public void AddInfo(int id, Material material, Mesh mesh)
    {
        _renderInfos.Add(id, (material, mesh));
    }

    public void AddItem(int id, RenderObject renderObject)
    {
        if (!RenderGroups.TryGetValue(id, out RenderGroup group))
        {
            (Material material, Mesh mesh) = _renderInfos[id];
            group = new RenderGroup(material, mesh);
            RenderGroups.Add(id, group);
        }
        group.Add(renderObject);
    }

    public void RemoveItem(int id, RenderObject renderObject)
    {
        RenderGroups[id].RemoveItem(renderObject);
    }

    public void Update(float deltaTime)
    {
        foreach (RenderGroup renderGroup in RenderGroups.Values)
            renderGroup.Update(deltaTime);
    }

    public void Render()
    {
        foreach (RenderGroup renderGroup in RenderGroups.Values)
            renderGroup.Draw();
    }
}
