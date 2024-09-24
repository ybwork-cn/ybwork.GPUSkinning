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
    readonly TempRenderObjectGroup _tempRenderGroup;
    readonly List<RenderObject> _renderObjects = new();
    readonly ConcurrentQueue<RenderObject> _tempRenderObjects_Add = new();

    internal int Count => _renderObjects.Count;

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

    public void Draw(CommandBuffer cmd, int shaderPass)
    {
        if (_material == null || _mesh == null)
            return;

        _tempRenderGroup.DrawMeshInstanced(_renderObjects, _material, _mesh, cmd, shaderPass);
    }

    public void Draw(bool isCastShadow, bool receiveShadows)
    {
        if (_material == null || _mesh == null)
            return;

        _tempRenderGroup.DrawMeshInstanced(_renderObjects, _material, _mesh, isCastShadow, receiveShadows);
    }
}
