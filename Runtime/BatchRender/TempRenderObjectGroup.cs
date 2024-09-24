using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

internal class TempRenderObjectGroup
{
    private readonly List<float[]> _loops = new();
    private readonly List<float[]> _animIndexs = new();
    private readonly List<float[]> _currentTimes = new();
    private readonly List<float[]> _lastAnimLoops = new();
    private readonly List<float[]> _lastAnimIndexs = new();
    private readonly List<float[]> _lastAnimExitTimes = new();
    private readonly List<float[]>[] _customProps;

    private readonly List<Matrix4x4[]> _matrices = new();

    private readonly MaterialPropertyBlock _block = new();
    private readonly string[] _customPropNames;

    public TempRenderObjectGroup(string[] customPropNames)
    {
        _customPropNames = customPropNames;
        _customProps = new List<float[]>[customPropNames.Length];
    }

    private void AddRange(List<RenderObject> renderObjects)
    {
        Matrix4x4[] cur_matrices = null;
        float[] cur_loops = null;
        float[] cur_animIndexs = null;
        float[] cur_currentTimes = null;
        float[] cur_lastAnimLoops = null;
        float[] cur_lastAnimIndexs = null;
        float[] cur_lastAnimExitTimes = null;

        int count = renderObjects.Count;
        for (int i = 0; i < count; i++)
        {
            RenderObject renderObject = renderObjects[i];
            RenderObjectData data = renderObject.RenderObjectData;

            int arrayIndex = i / 1000;
            int index = i % 1000;

            if (_matrices.Count <= arrayIndex)
            {
                _matrices.Add(new Matrix4x4[1000]);
                _loops.Add(new float[1000]);
                _animIndexs.Add(new float[1000]);
                _currentTimes.Add(new float[1000]);
                _lastAnimLoops.Add(new float[1000]);
                _lastAnimIndexs.Add(new float[1000]);
                _lastAnimExitTimes.Add(new float[1000]);

                for (int prpIndex = 0; prpIndex < _customPropNames.Length; prpIndex++)
                {
                    if (_customProps[prpIndex] == null)
                        _customProps[prpIndex] = new List<float[]>();
                    _customProps[prpIndex].Add(new float[1000]);
                }
            }

            if (index == 0)
            {
                cur_matrices = _matrices[arrayIndex];

                cur_loops = _loops[arrayIndex];
                cur_animIndexs = _animIndexs[arrayIndex];
                cur_currentTimes = _currentTimes[arrayIndex];
                cur_lastAnimLoops = _lastAnimLoops[arrayIndex];
                cur_lastAnimIndexs = _lastAnimIndexs[arrayIndex];
                cur_lastAnimExitTimes = _lastAnimExitTimes[arrayIndex];
            }

            cur_matrices[index] = renderObject.Matrix;

            cur_loops[index] = data.Loop ? 1 : 0;
            cur_animIndexs[index] = data.AnimIndex;
            cur_currentTimes[index] = data.CurrentTime;
            cur_lastAnimLoops[index] = data.LastAnimLoop ? 1 : 0;
            cur_lastAnimIndexs[index] = data.LastAnimIndex;
            cur_lastAnimExitTimes[index] = data.LastAnimExitTime;

            for (int propIndex = 0; propIndex < _customPropNames.Length; propIndex++)
                _customProps[propIndex][arrayIndex][index] = renderObject.CustomPropValues[propIndex];
        }
    }

    public void DrawMeshInstanced(List<RenderObject> renderObjects, Material material, Mesh mesh, CommandBuffer cmd, int shaderPass)
    {
        int count = renderObjects.Count;
        if (count == 0)
            return;

        AddRange(renderObjects);

        _block.Clear();
        for (int i = 0; i < (count - 1) / 1000 + 1; i++)
        {
            _block.SetFloatArray("_Loop", _loops[i]);
            _block.SetFloatArray("_AnimIndex", _animIndexs[i]);
            _block.SetFloatArray("_CurrentTime", _currentTimes[i]);

            _block.SetFloatArray("_LastAnimLoop", _lastAnimLoops[i]);
            _block.SetFloatArray("_LastAnimIndex", _lastAnimIndexs[i]);
            _block.SetFloatArray("_LastAnimExitTime", _lastAnimExitTimes[i]);

            for (int propIndex = 0; propIndex < _customPropNames.Length; propIndex++)
                _block.SetFloatArray(_customPropNames[propIndex], _customProps[propIndex][i]);

            cmd.DrawMeshInstanced(
                mesh, 0, material, shaderPass,
                _matrices[i], Math.Min(count - i * 1000, 1000),
                _block);
        }
    }

    public void DrawMeshInstanced(List<RenderObject> renderObjects, Material material, Mesh mesh, bool isCastShadow, bool receiveShadows)
    {
        int count = renderObjects.Count;
        if (count == 0)
            return;

        AddRange(renderObjects);

        _block.Clear();
        for (int i = 0; i < (count - 1) / 1000 + 1; i++)
        {
            _block.SetFloatArray("_Loop", _loops[i]);
            _block.SetFloatArray("_AnimIndex", _animIndexs[i]);
            _block.SetFloatArray("_CurrentTime", _currentTimes[i]);

            _block.SetFloatArray("_LastAnimLoop", _lastAnimLoops[i]);
            _block.SetFloatArray("_LastAnimIndex", _lastAnimIndexs[i]);
            _block.SetFloatArray("_LastAnimExitTime", _lastAnimExitTimes[i]);

            for (int propIndex = 0; propIndex < _customPropNames.Length; propIndex++)
                _block.SetFloatArray(_customPropNames[propIndex], _customProps[propIndex][i]);

            Graphics.DrawMeshInstanced(
                mesh, 0, material,
                _matrices[i], Math.Min(count - i * 1000, 1000),
                _block, isCastShadow ? ShadowCastingMode.On : ShadowCastingMode.Off, receiveShadows);
        }
    }
}
