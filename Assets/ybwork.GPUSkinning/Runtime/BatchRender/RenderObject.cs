using System;
using UnityEngine;

[Serializable]
public class RenderObject
{
    public Matrix4x4 Matrix;
    internal readonly RenderObjectData RenderObjectData = new();
    public readonly GPUSkinningStateMachine StateMachine;

    public RenderObject(GPUSkinningInfo gpuSkinningInfo)
    {
        StateMachine = new GPUSkinningStateMachine(gpuSkinningInfo);
    }

    public void Init(int initState)
    {
        bool stateIsLoop = StateMachine.GetStateIsLoop(initState);
        RenderObjectData.SwitchState(initState, stateIsLoop);
    }

    public void SwitchState(int state)
    {
        // 尝试重新播放一个循环动作，且当前动作就是目标动作，则跳过
        int currentAnim = RenderObjectData.AnimIndex;
        bool nextStateIsLoop = StateMachine.GetStateIsLoop(state);
        if (currentAnim == state && nextStateIsLoop)
            return;

        RenderObjectData.SwitchState(state, nextStateIsLoop);
    }

    public void Update(float deltaTime)
    {
        RenderObjectData.Update(deltaTime);
        int animIndex = RenderObjectData.AnimIndex;

        // 非循环动画，超时
        if (!RenderObjectData.Loop && StateMachine.AnimaitonLengths[animIndex] <= RenderObjectData.CurrentTime)
        {
            if (StateMachine.TryGetNextState(animIndex, out int nextStateIndex))
            {
                bool nextStateIsLoop = StateMachine.GetStateIsLoop(nextStateIndex);
                RenderObjectData.SwitchState(nextStateIndex, nextStateIsLoop);
            }
        }
    }
}
