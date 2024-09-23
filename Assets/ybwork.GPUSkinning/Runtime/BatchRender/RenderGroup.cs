using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderGroup
{
    readonly Material _material;
    readonly Mesh _mesh;
    private readonly string[] _customPropNames;
    public readonly GPUSkinningStateMachine StateMachine;
    /// <summary>
    /// TODO:保存实际用于渲染的数据, 用完就扔, 极大的内存浪费
    /// </summary>
    readonly TempRenderObjectGroup _tempRenderGroup;
    readonly List<RenderObject> _renderObjects = new();
    readonly ConcurrentQueue<RenderObject> _tempRenderObjects_Add = new();

    internal int Count;

    internal RenderGroup(float[] animaitonLengths, Material material, Mesh mesh, params string[] customPropNames)
    {
        StateMachine = new GPUSkinningStateMachine(animaitonLengths);
        _material = material;
        _mesh = mesh;
        _customPropNames = customPropNames;
        _tempRenderGroup = new(customPropNames);
    }

    public RenderObject CreateRenderObject()
    {
        RenderObject renderObject = new RenderObject(StateMachine, _customPropNames);
        _tempRenderObjects_Add.Enqueue(renderObject);
        return renderObject;
    }

    public void Update(float deltaTime)
    {
        _renderObjects.RemoveAll(renderObject => renderObject.Destroyed);
        while (_tempRenderObjects_Add.TryDequeue(out RenderObject renderObject))
            _renderObjects.Add(renderObject);

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
        for (int i = 0; i < _renderObjects.Count; i++)
        {
            _tempRenderGroup.AddRange(_renderObjects[i]);
            Count++;
        }
        _tempRenderGroup.DrawMeshInstanced(_material, _mesh, cmd);
    }
}
