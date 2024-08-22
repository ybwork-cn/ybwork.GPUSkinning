using System.Collections.Generic;
using UnityEngine;

public class RenderSystem
{
    public List<RenderGroup> RenderGroups = new();
    public List<RenderObject> RenderObjects = new List<RenderObject>();

    public void Init(Material material, Mesh mesh)
    {
        RenderGroup group = new RenderGroup(material, mesh);
        RenderGroups.Add(group);
    }

    public void Render()
    {
        RenderObjects.ForEach(RenderItem);

        foreach (RenderGroup renderGroup in RenderGroups)
        {
            renderGroup.Draw();
        }
    }

    private void RenderItem(RenderObject renderObject)
    {
        RenderGroup group = renderObject.Group;
        group.Add(renderObject);
    }
}
