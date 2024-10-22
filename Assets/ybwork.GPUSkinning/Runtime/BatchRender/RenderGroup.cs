using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

internal class LodRenderGroup
{
    public readonly float MaxDistance;

    public readonly Material Material;
    public readonly Mesh Mesh;

    public readonly List<RenderObject> RenderObjects = new();

    public LodRenderGroup(float maxDistance, Material material, Mesh mesh)
    {
        MaxDistance = maxDistance;
        Material = material;
        Mesh = mesh;
    }
}

public class RenderGroup
{
    private readonly string[] _customPropNames;
    public readonly GPUSkinningStateMachine StateMachine;
    readonly TempRenderObjectGroup _tempRenderGroup;
    readonly List<RenderObject> _renderObjects = new();
    readonly ConcurrentQueue<RenderObject> _tempRenderObjects_Add = new();

    private readonly List<LodRenderGroup> _lodRenderGroups = new();

    internal RenderGroup(float[] animaitonLengths, params string[] customPropNames)
    {
        StateMachine = new GPUSkinningStateMachine(animaitonLengths);
        _customPropNames = customPropNames;
        _tempRenderGroup = new(customPropNames);
    }

    public void RegisterLod(Material material, Mesh mesh, float distance = float.MaxValue)
    {
        _lodRenderGroups.Add(new LodRenderGroup(distance, material, mesh));
        _lodRenderGroups.Sort((g1, g2) =>
        {
            float delta = g1.MaxDistance - g2.MaxDistance;
            return delta switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => 0,
            };
        });
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

    public RenderRusult Draw(CommandBuffer cmd, Camera camera, int shaderPass)
    {
        if (_lodRenderGroups.Count == 0)
            throw new System.IndexOutOfRangeException("未调用RegisterLod()");

        RenderRusult renderRusult = new();
        renderRusult.MeshCount = _lodRenderGroups.Count;
        renderRusult.InstanceCount = _renderObjects.Count;

        Vector3 cameraPos = camera.transform.position;
        foreach (RenderObject renderObject in _renderObjects)
        {
            Vector3 pos = renderObject.Matrix.GetPosition();
            float distance = Vector3.Distance(pos, cameraPos);
            int index = _lodRenderGroups.Count - 1;
            for (int i = _lodRenderGroups.Count - 2; i >= 0; i--)
            {
                if (distance < _lodRenderGroups[i].MaxDistance)
                    index = i;
            }
            _lodRenderGroups[index].RenderObjects.Add(renderObject);
        }

        _lodRenderGroups.ForEach(group =>
        {
            renderRusult.MeshVertexCount += group.Mesh.vertexCount;
            renderRusult.InstanceFaceCount += group.Mesh.triangles.Length / 3 * group.RenderObjects.Count;
            _tempRenderGroup.DrawMeshInstanced(group.RenderObjects, group.Material, group.Mesh, cmd, shaderPass);
            group.RenderObjects.Clear();
        });

        return renderRusult;
    }

    public RenderRusult Draw(Camera camera, bool isCastShadow, bool receiveShadows)
    {
        if (_lodRenderGroups.Count == 0)
            throw new System.IndexOutOfRangeException("未调用RegisterLod()");

        RenderRusult renderRusult = new();
        renderRusult.MeshCount = _lodRenderGroups.Count;
        renderRusult.InstanceCount = _renderObjects.Count;

        Vector3 cameraPos = camera.transform.position;
        foreach (RenderObject renderObject in _renderObjects)
        {
            Vector3 pos = renderObject.Matrix.GetPosition();
            float distance = Vector3.Distance(pos, cameraPos);
            int index = _lodRenderGroups.Count - 1;
            for (int i = _lodRenderGroups.Count - 2; i >= 0; i--)
            {
                if (distance < _lodRenderGroups[i].MaxDistance)
                    index = i;
            }
            _lodRenderGroups[index].RenderObjects.Add(renderObject);
        }

        _lodRenderGroups.ForEach(group =>
        {
            renderRusult.MeshVertexCount += group.Mesh.vertexCount;
            renderRusult.InstanceFaceCount += group.Mesh.triangles.Length / 3 * group.RenderObjects.Count;
            _tempRenderGroup.DrawMeshInstanced(group.RenderObjects, group.Material, group.Mesh, isCastShadow, receiveShadows);
            group.RenderObjects.Clear();
        });

        return renderRusult;
    }
}
