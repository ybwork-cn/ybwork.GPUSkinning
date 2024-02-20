using System;
using UnityEngine;

[Serializable]
public class GPUSkinningData
{
    public bool Loop;
    public int AnimIndex;
    public float CurrentTime;
    public int LastAnimIndex;
    public float LastAnimExitTime;

    public void SwitchState(Material material, int state, bool loop)
    {
        Loop = loop;
        LastAnimIndex = AnimIndex;
        LastAnimExitTime = CurrentTime;
        AnimIndex = state;
        CurrentTime = 0;

        material.SetFloat("_Loop", Loop ? 1 : 0);
        material.SetFloat("_AnimIndex", AnimIndex);
        material.SetFloat("_CurrentTime", CurrentTime);
        material.SetFloat("_LastAnimIndex", LastAnimIndex);
        material.SetFloat("_LastAnimExitTime", LastAnimExitTime);
    }

    public void Update(Material material, float deltaTime)
    {
        CurrentTime += deltaTime / 3;
        material.SetFloat("_CurrentTime", CurrentTime);
    }
}

public class GPUSkinningComponent : MonoBehaviour
{
    Material _material;
    [SerializeField] GPUSkinningData _gpuSkinningData;

    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            _gpuSkinningData.SwitchState(_material, 0, true);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            _gpuSkinningData.SwitchState(_material, 1, true);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            _gpuSkinningData.SwitchState(_material, 2, false);

        _gpuSkinningData.Update(_material, Time.deltaTime);
    }
}
