using UnityEngine;
using ybwork.Async;

public class TestGPU : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    [SerializeField] Material _material;
    BatchRenderer _batchRenderer;
    void Start()
    {
        _batchRenderer = new BatchRenderer();
        GPUSkinningInfo gpuSkinningInfo = _prefab.GetComponent<GPUSkinningInfo>();
        _material = new Material(_prefab.GetComponent<MeshRenderer>().sharedMaterial);
        //_material.EnableKeyword("_EMISSION");
        //_material.SetColor("_EmissionColor", Color.white);
        Mesh sharedMesh = _prefab.GetComponent<MeshFilter>().sharedMesh;
        var renderGroup = _batchRenderer.AddGroup(0, gpuSkinningInfo.AnimaitonLengths, _material, sharedMesh);
        renderGroup.StateMachine.RegisterLoopState(0);
        renderGroup.StateMachine.RegisterOnceState(1, 0);
        renderGroup.StateMachine.RegisterLoopState(2);
        for (int i = 0; i < 50; i++)
        {
            for (int j = 0; j < 60; j++)
            {
                RenderObject renderObject = _batchRenderer.CreateRenderObject(0);
                renderObject.Matrix = Matrix4x4.TRS(new Vector3(i, 0, j), Quaternion.identity, Vector3.one);
                renderObject.Init(0);
                //renderObject.OtherProps["_EmissionForce"] = Random.value;
                YueTask.Delay(Random.value * 10).Then(() =>
                {
                    renderObject.SwitchState(Random.Range(0, 4));
                });
            }
        }
    }

    private void Update()
    {
        _batchRenderer.Update(Time.deltaTime);
        _batchRenderer.Render();
    }
}
