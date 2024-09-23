using System;
using UnityEngine;

[Serializable]
internal class GPUSkinningData
{
    public bool ForceUpdate;
    public RenderObjectData RenderObjectData = new();
    private readonly string[] _customPropNames;
    public readonly float[] CustomPropValues;

    private readonly Renderer _renderer;
    private readonly MaterialPropertyBlock _propBlock = new();

    public GPUSkinningData(Renderer renderer, string[] customPropNames)
    {
        _renderer = renderer;
        _renderer.GetPropertyBlock(_propBlock);
        _customPropNames = customPropNames;
    }

    public void SwitchState(int state, bool loop)
    {
        RenderObjectData.SwitchState(state, loop);

        _propBlock.SetFloat("_Loop", RenderObjectData.Loop ? 1 : 0);
        _propBlock.SetFloat("_AnimIndex", RenderObjectData.AnimIndex);
        _propBlock.SetFloat("_CurrentTime", RenderObjectData.CurrentTime);

        _propBlock.SetFloat("_LastAnimLoop", RenderObjectData.LastAnimLoop ? 1 : 0);
        _propBlock.SetFloat("_LastAnimIndex", RenderObjectData.LastAnimIndex);
        _propBlock.SetFloat("_LastAnimExitTime", RenderObjectData.LastAnimExitTime);

        for (int i = 0; i < _customPropNames.Length; i++)
            _propBlock.SetFloat(_customPropNames[i], CustomPropValues[i]);

        //_renderer.SetPropertyBlock(_propBlock);
    }

    public void Update(float deltaTime)
    {
        RenderObjectData.CurrentTime += deltaTime;

        _propBlock.SetFloat("_CurrentTime", RenderObjectData.CurrentTime);

#if UNITY_EDITOR
        if (ForceUpdate)
        {
            _propBlock.SetFloat("_Loop", RenderObjectData.Loop ? 1 : 0);
            _propBlock.SetFloat("_AnimIndex", RenderObjectData.AnimIndex);

            _propBlock.SetFloat("_LastAnimLoop", RenderObjectData.LastAnimLoop ? 1 : 0);
            _propBlock.SetFloat("_LastAnimIndex", RenderObjectData.LastAnimIndex);
            _propBlock.SetFloat("_LastAnimExitTime", RenderObjectData.LastAnimExitTime);
        }

        for (int i = 0; i < _customPropNames.Length; i++)
            _propBlock.SetFloat(_customPropNames[i], CustomPropValues[i]);
#endif

        _renderer.SetPropertyBlock(_propBlock);
    }
}
