using UnityEngine;

[System.Serializable]
internal class GPUSkinningData
{
    public bool ForceUpdate;
    public bool Loop;
    public int AnimIndex;
    public float CurrentTime;

    public bool LastAnimLoop;
    public int LastAnimIndex;
    public float LastAnimExitTime;

    private readonly Renderer _renderer;
    private readonly MaterialPropertyBlock _propBlock = new();

    public GPUSkinningData(Renderer renderer)
    {
        _renderer = renderer;
        _renderer.GetPropertyBlock(_propBlock);
    }

    public void SwitchState(int state, bool loop)
    {
        LastAnimLoop = Loop;
        LastAnimIndex = AnimIndex;
        LastAnimExitTime = CurrentTime;
        Loop = loop;
        AnimIndex = state;
        CurrentTime = 0;

        _propBlock.SetFloat("_Loop", Loop ? 1 : 0);
        _propBlock.SetFloat("_AnimIndex", AnimIndex);
        _propBlock.SetFloat("_CurrentTime", CurrentTime);

        _propBlock.SetFloat("_LastAnimLoop", LastAnimLoop ? 1 : 0);
        _propBlock.SetFloat("_LastAnimIndex", LastAnimIndex);
        _propBlock.SetFloat("_LastAnimExitTime", LastAnimExitTime);

        _renderer.SetPropertyBlock(_propBlock);
    }

    public void Update(float deltaTime)
    {
        if (Application.isEditor && ForceUpdate)
        {
            _propBlock.SetFloat("_Loop", Loop ? 1 : 0);
            _propBlock.SetFloat("_AnimIndex", AnimIndex);

            _propBlock.SetFloat("_LastAnimLoop", LastAnimLoop ? 1 : 0);
            _propBlock.SetFloat("_LastAnimIndex", LastAnimIndex);
            _propBlock.SetFloat("_LastAnimExitTime", LastAnimExitTime);
        }

        CurrentTime += deltaTime;
        _propBlock.SetFloat("_CurrentTime", CurrentTime);
        _renderer.SetPropertyBlock(_propBlock);
    }
}

public class GPUSkinningComponent : MonoBehaviour
{
    [SerializeField] float _speed = 1;
    [SerializeField] GPUSkinningData _gpuSkinningData;
    public GPUSkinningStateMachine StateMachine { get; private set; }
    private GPUSkinningInfo _gpuSkinningInfo;

    private void Awake()
    {
        StateMachine = new GPUSkinningStateMachine();
        _gpuSkinningInfo = GetComponent<GPUSkinningInfo>();
        _gpuSkinningData = new GPUSkinningData(GetComponent<MeshRenderer>());

        SwitchState(0);
    }

    private void Update()
    {
        _gpuSkinningData.Update(Time.deltaTime * _speed);

        if (!_gpuSkinningData.Loop && _gpuSkinningInfo.AnimaitonLengths[_gpuSkinningData.AnimIndex] <= _gpuSkinningData.CurrentTime)
        {
            if (StateMachine.TryGetNextState(_gpuSkinningData.AnimIndex, out int nextStateIndex))
            {
                bool nextStateIsLoop = StateMachine.GetStateIsLoop(nextStateIndex);
                _gpuSkinningData.SwitchState(nextStateIndex, nextStateIsLoop);
            }
        }
    }

    public void SwitchState(int state)
    {
        // 尝试重新播放一个循环动作，且当前动作就是目标动作，则跳过
        int currentAnim = _gpuSkinningData.AnimIndex;
        bool nextStateIsLoop = StateMachine.GetStateIsLoop(state);
        if (currentAnim == state && nextStateIsLoop)
            return;

        _gpuSkinningData.SwitchState(state, nextStateIsLoop);
    }
}
