using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BatchRenderer
{
    private readonly Dictionary<int, RenderGroup> _renderGroups = new();

    public RenderGroup AddGroup(int id, float[] animaitonLengths, Material material, Mesh mesh, string[] customPropNames)
    {
        RenderGroup renderGroup = new RenderGroup(animaitonLengths, material, mesh, customPropNames);
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
    public void Render(CommandBuffer cmd, int shaderPass)
    {
        foreach (RenderGroup renderGroup in _renderGroups.Values)
            renderGroup.Draw(cmd, shaderPass);
    }

    /// <summary>
    /// 允许在Update中调用
    /// </summary>
    public void Render(bool isCastShadow, bool receiveShadows)
    {
        foreach (RenderGroup renderGroup in _renderGroups.Values)
            renderGroup.Draw(isCastShadow, receiveShadows);
    }
}
