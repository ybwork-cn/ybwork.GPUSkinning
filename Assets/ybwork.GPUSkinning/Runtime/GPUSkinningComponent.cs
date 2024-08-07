﻿using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
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
    private readonly UnityEvent<int> _onStateSwitched = new();
    public UnityEvent<int> OnStateSwitched => _onStateSwitched;
    private bool _canCallback = true;

    private void Awake()
    {
        _gpuSkinningInfo = GetComponent<GPUSkinningInfo>();
        _gpuSkinningData = new GPUSkinningData(GetComponent<MeshRenderer>());
        StateMachine = new GPUSkinningStateMachine(_gpuSkinningInfo);
    }

    private void Update()
    {
        _gpuSkinningData.Update(Time.deltaTime * _speed);

        int animIndex = _gpuSkinningData.AnimIndex;

        // 非循环动画，超时
        if (!_gpuSkinningData.Loop && _gpuSkinningInfo.AnimaitonLengths[animIndex] <= _gpuSkinningData.CurrentTime)
        {
            if (_canCallback)
                _onStateSwitched.Invoke(animIndex);
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
        if (_gpuSkinningInfo.AnimaitonLengths.Length <= state)
            throw new IndexOutOfRangeException($"{state} in [0,{_gpuSkinningInfo.AnimaitonLengths.Length}]");

        // 尝试重新播放一个循环动作，且当前动作就是目标动作，则跳过
        int currentAnim = _gpuSkinningData.AnimIndex;
        bool nextStateIsLoop = StateMachine.GetStateIsLoop(state);
        if (currentAnim == state && nextStateIsLoop)
            return;

        _gpuSkinningData.SwitchState(state, nextStateIsLoop);
        _canCallback = true;
    }
}
