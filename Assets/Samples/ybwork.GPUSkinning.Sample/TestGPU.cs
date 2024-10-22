using UnityEngine;
using ybwork.Async;

public class TestGPU : MonoBehaviour
{
    [SerializeField] GameObject _prefab1;
    [SerializeField] GameObject _prefab2;
    BatchRenderer _batchRenderer;
    void Start()
    {
        _batchRenderer = new BatchRenderer();
        GPUSkinningInfo gpuSkinningInfo = _prefab1.GetComponent<GPUSkinningInfo>();
        Material material1 = new Material(_prefab1.GetComponent<MeshRenderer>().sharedMaterial);
        Material material2 = new Material(_prefab2.GetComponent<MeshRenderer>().sharedMaterial);
        //_material.EnableKeyword("_EMISSION");
        //_material.SetColor("_EmissionColor", Color.white);
        Mesh sharedMesh1 = Instantiate(_prefab1.GetComponent<MeshFilter>().sharedMesh);
        Mesh sharedMesh2 = Instantiate(_prefab2.GetComponent<MeshFilter>().sharedMesh);
        string[] customPropNames = new string[] { "_EmissionForce" };
        RenderGroup renderGroup = _batchRenderer.AddGroup(0, gpuSkinningInfo.AnimaitonLengths, customPropNames);
        renderGroup.RegisterLod(material1, sharedMesh1, 5);
        renderGroup.RegisterLod(material2, sharedMesh2);
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
                int emissionForcePropId = renderObject.GetCustomPropId("_EmissionForce");
                //renderObject.OtherProps[emissionForcePropId] = Random.value;
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
        _batchRenderer.Render(Camera.main, true, true);
    }
}
