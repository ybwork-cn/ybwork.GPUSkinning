using System.Collections.Generic;
using UnityEngine;

public class RenderSystem
{
    private Dictionary<int, (Material material, Mesh mesh)> _renderInfos = new();
    public Dictionary<int, RenderGroup> RenderGroups = new();

    public void Init()
    {
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

    public void Render()
    {
        foreach (RenderGroup renderGroup in RenderGroups.Values)
            renderGroup.Draw();
    }
}
