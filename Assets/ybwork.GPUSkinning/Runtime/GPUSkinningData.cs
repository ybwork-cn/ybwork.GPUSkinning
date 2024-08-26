using System;
using UnityEngine;

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

        //_renderer.SetPropertyBlock(_propBlock);
    }

    public void Update(float deltaTime)
    {
#if UNITY_EDITOR
        if (ForceUpdate)
        {
            _propBlock.SetFloat("_Loop", Loop ? 1 : 0);
            _propBlock.SetFloat("_AnimIndex", AnimIndex);

            _propBlock.SetFloat("_LastAnimLoop", LastAnimLoop ? 1 : 0);
            _propBlock.SetFloat("_LastAnimIndex", LastAnimIndex);
            _propBlock.SetFloat("_LastAnimExitTime", LastAnimExitTime);
        }
#endif

        CurrentTime += deltaTime;
        _propBlock.SetFloat("_CurrentTime", CurrentTime);
        _renderer.SetPropertyBlock(_propBlock);
    }
}
