using UnityEngine;
using ybwork.Async;

public class Test : MonoBehaviour
{
    [SerializeField] GameObject _prefab;

    void Start()
    {
        Transform parent = new GameObject("Roles").transform;
        for (int i = 0; i < 50; i++)
        {
            for (int j = 0; j < 60; j++)
            {
                GameObject go = Instantiate(_prefab, parent);
                go.transform.position = new Vector3(i, 0, j);
                GPUSkinningComponent gpuSkinningComponent = go.AddComponent<GPUSkinningComponent>();
                YueTask.Delay(Random.value * 100).Then(() =>
                {
                    gpuSkinningComponent.SwitchState(Random.Range(0, 4));
                });
            }
        }
    }
}
