﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderGroup
{
    readonly Material _material;
    readonly Mesh _mesh;
    public readonly GPUSkinningStateMachine StateMachine;
    /// <summary>
    /// TODO:保存实际用于渲染的数据, 用完就扔, 极大的内存浪费
    /// </summary>
    readonly TempRenderObjectGroup _tempRenderGroup = new();
    readonly List<RenderObject> _renderObjects = new();
    readonly ConcurrentQueue<RenderObject> _tempRenderObjects_Add = new();
    readonly ConcurrentQueue<RenderObject> _tempRenderObjects_Remove = new();

    internal int Count;

    public RenderGroup(float[] animaitonLengths, Material material, Mesh mesh)
    {
        StateMachine = new GPUSkinningStateMachine(animaitonLengths);
        _material = material;
        _mesh = mesh;
    }

    public RenderObject CreateRenderObject()
    {
        RenderObject renderObject = new RenderObject(StateMachine);
        _tempRenderObjects_Add.Enqueue(renderObject);
        return renderObject;
    }

    public void RemoveItem(RenderObject renderObject)
    {
        _tempRenderObjects_Remove.Enqueue(renderObject);
    }

    public void Update(float deltaTime)
    {
        while (_tempRenderObjects_Remove.TryDequeue(out RenderObject renderObject))
            _renderObjects.Remove(renderObject);
        while (_tempRenderObjects_Add.TryDequeue(out RenderObject renderObject))
            _renderObjects.Add(renderObject);

        foreach (RenderObject renderObject in _renderObjects)
            renderObject.Update(deltaTime);
    }

    public void Draw(CommandBuffer cmd)
    {
        if (_material == null || _mesh == null)
            return;

        Count = 0;
        for (int i = 0; i < _renderObjects.Count; i++)
        {
            _tempRenderGroup.Add(_renderObjects[i]);
            Count++;
        }
        _tempRenderGroup.DrawMeshInstanced(_material, _mesh, cmd);
    }
}