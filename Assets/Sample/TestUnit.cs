using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GPUSkinningComponent))]
public class TestUnit : MonoBehaviour
{
    GPUSkinningComponent _skinningComponent;

    private void Awake()
    {
        _skinningComponent = GetComponent<GPUSkinningComponent>();
    }

    void Update()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0 + i))
                _skinningComponent.SwitchState(i, false);
        }
    }
}
