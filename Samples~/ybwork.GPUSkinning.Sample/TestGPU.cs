﻿using UnityEngine;
using ybwork.Async;

public class TestGPU : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    BatchRenderer _batchRenderer;
    void Start()
    {
        _batchRenderer = new BatchRenderer();
        GPUSkinningInfo gpuSkinningInfo = _prefab.GetComponent<GPUSkinningInfo>();
        Material sharedMaterial = _prefab.GetComponent<MeshRenderer>().sharedMaterial;
        Mesh sharedMesh = _prefab.GetComponent<MeshFilter>().sharedMesh;
        _batchRenderer.AddInfo(0, sharedMaterial, sharedMesh);
        for (int i = 0; i < 50; i++)
        {
            for (int j = 0; j < 60; j++)
            {
                RenderObject renderObject = new RenderObject(gpuSkinningInfo);
                renderObject.StateMachine.RegisterLoopState(0);
                renderObject.StateMachine.RegisterOnceState(1, 0);
                renderObject.StateMachine.RegisterLoopState(2);
                renderObject.Matrix = Matrix4x4.TRS(new Vector3(i, 0, j), Quaternion.identity, Vector3.one);
                renderObject.Init(0);
                YueTask.Delay(Random.value * 10).Then(() =>
                {
                    renderObject.SwitchState(Random.Range(0, 4));
                });
                _batchRenderer.AddItem(0, renderObject);
            }
        }
    }

    private void Update()
    {
        _batchRenderer.Update(Time.deltaTime);
        _batchRenderer.Render();
    }
}