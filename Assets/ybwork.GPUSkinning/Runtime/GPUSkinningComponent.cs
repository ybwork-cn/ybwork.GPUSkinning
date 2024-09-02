using UnityEngine;

public class GPUSkinningComponent : MonoBehaviour
{
    [SerializeField] float _speed = 1;
    [SerializeField] GPUSkinningData _gpuSkinningData;
    public GPUSkinningStateMachine StateMachine { get; private set; }
    public readonly Event<int> OnStateSwitched = new Event<int>();
    private float[] _animaitonLengths;
    private bool _canCallback = true;

    private void Awake()
    {
        _animaitonLengths = GetComponent<GPUSkinningInfo>().AnimaitonLengths;
        _gpuSkinningData = new GPUSkinningData(GetComponent<MeshRenderer>());
        StateMachine = new GPUSkinningStateMachine(_animaitonLengths);
    }

    private void Update()
    {
        _gpuSkinningData.Update(Time.deltaTime * _speed);

        int animIndex = _gpuSkinningData.RenderObjectData.AnimIndex;

        // 非循环动画，超时
        if (!_gpuSkinningData.RenderObjectData.Loop && _animaitonLengths[animIndex] <= _gpuSkinningData.RenderObjectData.CurrentTime)
        {
            if (_canCallback)
                OnStateSwitched.Invoke(animIndex);
            _canCallback = false;

            if (StateMachine.TryGetNextState(animIndex, out int nextStateIndex))
            {
                bool nextStateIsLoop = StateMachine.GetStateIsLoop(nextStateIndex);
                _gpuSkinningData.SwitchState(nextStateIndex, nextStateIsLoop);
                _canCallback = true;
            }
        }
    }

    public void Init(int initState)
    {
        bool stateIsLoop = StateMachine.GetStateIsLoop(initState);
        _gpuSkinningData.SwitchState(initState, stateIsLoop);
        _canCallback = true;
    }

    public void SwitchState(int state)
    {
        // 尝试重新播放一个循环动作，且当前动作就是目标动作，则跳过
        int currentAnim = _gpuSkinningData.RenderObjectData.AnimIndex;
        bool nextStateIsLoop = StateMachine.GetStateIsLoop(state);
        if (currentAnim == state && nextStateIsLoop)
            return;

        _gpuSkinningData.SwitchState(state, nextStateIsLoop);
        _canCallback = true;
    }
}
