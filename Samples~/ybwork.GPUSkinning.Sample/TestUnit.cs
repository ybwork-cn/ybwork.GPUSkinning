using UnityEngine;

[RequireComponent(typeof(GPUSkinningComponent))]
public class TestUnit : MonoBehaviour
{
    GPUSkinningComponent _skinningComponent;

    private void Awake()
    {
        _skinningComponent = GetComponent<GPUSkinningComponent>();
        _skinningComponent.StateMachine.RegisterLoopState(0);
        _skinningComponent.StateMachine.RegisterOnceState(1, 0);
        _skinningComponent.StateMachine.RegisterLoopState(2);
        _skinningComponent.OnStateSwitched.AddListener(index =>
        {
            Debug.Log(index);
        });
        _skinningComponent.Init(0);
    }

    void Update()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0 + i))
                _skinningComponent.SwitchState(i);
        }
    }
}
