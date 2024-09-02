using System;
using UnityEngine;

[Serializable]
internal class GPUSkinningData
{
    public bool ForceUpdate;
    public RenderObjectData RenderObjectData = new();

    private readonly Renderer _renderer;
    private readonly MaterialPropertyBlock _propBlock = new();

    public GPUSkinningData(Renderer renderer)
    {
        _renderer = renderer;
        _renderer.GetPropertyBlock(_propBlock);
    }

    public void SwitchState(int state, bool loop)
    {
        RenderObjectData.LastAnimLoop = RenderObjectData.Loop;
        RenderObjectData.LastAnimIndex = RenderObjectData.AnimIndex;
        RenderObjectData.LastAnimExitTime = RenderObjectData.CurrentTime;
        RenderObjectData.Loop = loop;
        RenderObjectData.AnimIndex = state;
        RenderObjectData.CurrentTime = 0;

        _propBlock.SetFloat("_Loop", RenderObjectData.Loop ? 1 : 0);
        _propBlock.SetFloat("_AnimIndex", RenderObjectData.AnimIndex);
        _propBlock.SetFloat("_CurrentTime", RenderObjectData.CurrentTime);

        _propBlock.SetFloat("_LastAnimLoop", RenderObjectData.LastAnimLoop ? 1 : 0);
        _propBlock.SetFloat("_LastAnimIndex", RenderObjectData.LastAnimIndex);
        _propBlock.SetFloat("_LastAnimExitTime", RenderObjectData.LastAnimExitTime);

        //_renderer.SetPropertyBlock(_propBlock);
    }

    public void Update(float deltaTime)
    {
#if UNITY_EDITOR
        if (ForceUpdate)
        {
            _propBlock.SetFloat("_Loop", RenderObjectData.Loop ? 1 : 0);
            _propBlock.SetFloat("_AnimIndex", RenderObjectData.AnimIndex);

            _propBlock.SetFloat("_LastAnimLoop", RenderObjectData.LastAnimLoop ? 1 : 0);
            _propBlock.SetFloat("_LastAnimIndex", RenderObjectData.LastAnimIndex);
            _propBlock.SetFloat("_LastAnimExitTime", RenderObjectData.LastAnimExitTime);
        }
#endif

        RenderObjectData.CurrentTime += deltaTime;
        _propBlock.SetFloat("_CurrentTime", RenderObjectData.CurrentTime);
        _renderer.SetPropertyBlock(_propBlock);
    }
}
