using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

internal class RenderGroup
{
    readonly Material _material;
    readonly Mesh _mesh;
    /// <summary>
    /// TODO:保存实际用于渲染的数据, 用完就扔, 极大的内存浪费
    /// </summary>
    readonly TempRenderObjectGroup _tempRenderObjects;
    readonly List<RenderObject> _renderObjects;

    public int Count;

    public RenderGroup(Material material, Mesh mesh)
    {
        _material = material;
        _mesh = mesh;
        _renderObjects = new List<RenderObject>();
        _tempRenderObjects = new TempRenderObjectGroup();
    }

    public void Add(RenderObject renderObject)
    {
        _renderObjects.Add(renderObject);
    }

    public void RemoveItem(RenderObject renderObject)
    {
        _renderObjects.Remove(renderObject);
    }

    public void Update(float deltaTime)
    {
        foreach (RenderObject renderObject in _renderObjects)
        {
            renderObject.Update(deltaTime);
        }
    }

    public void Draw(CommandBuffer cmd)
    {
        if (_material == null || _mesh == null)
            return;

        Count = 0;
        foreach (var renderObject in _renderObjects)
        {
            Count++;
            _tempRenderObjects.Add(renderObject.Matrix, renderObject.RenderObjectData);
        }
        _tempRenderObjects.DrawMeshInstanced(_material, _mesh, cmd);
    }
}
