using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RenderObject
{
    internal bool Destroyed = false;
    public Matrix4x4 Matrix;
    internal readonly RenderObjectData RenderObjectData = new();
    public readonly Dictionary<string, float> OtherProps = new();
    readonly GPUSkinningStateMachine _stateMachine;

    public RenderObject(GPUSkinningStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public void Init(int initState)
    {
        bool stateIsLoop = _stateMachine.GetStateIsLoop(initState);
        RenderObjectData.SwitchState(initState, stateIsLoop);
    }

    public void SwitchState(int state)
    {
        // 尝试重新播放一个循环动作，且当前动作就是目标动作，则跳过
        int currentAnim = RenderObjectData.AnimIndex;
        bool nextStateIsLoop = _stateMachine.GetStateIsLoop(state);
        if (currentAnim == state && nextStateIsLoop)
            return;

        RenderObjectData.SwitchState(state, nextStateIsLoop);
    }

    public void Update(float deltaTime)
    {
        RenderObjectData.Update(deltaTime);
        int animIndex = RenderObjectData.AnimIndex;

        // 非循环动画，超时
        if (!RenderObjectData.Loop && _stateMachine.AnimaitonLengths[animIndex] <= RenderObjectData.CurrentTime)
        {
            if (_stateMachine.TryGetNextState(animIndex, out int nextStateIndex))
            {
                bool nextStateIsLoop = _stateMachine.GetStateIsLoop(nextStateIndex);
                RenderObjectData.SwitchState(nextStateIndex, nextStateIsLoop);
            }
        }
    }

    public void Destroy()
    {
        Destroyed = true;
    }
}
