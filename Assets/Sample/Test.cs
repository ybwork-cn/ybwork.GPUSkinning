using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] GameObject _prefab;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 30; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                GameObject go = Instantiate(_prefab);
                go.transform.position = new Vector3(i, 0, j);
                GPUSkinningComponent gpuSkinningComponent = go.GetComponent<GPUSkinningComponent>();
                gpuSkinningComponent.SwitchState(Random.Range(0, 4), true);
            }
        }
    }
}
