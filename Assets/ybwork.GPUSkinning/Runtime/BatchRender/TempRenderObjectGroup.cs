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
    private readonly Dictionary<string, List<float[]>> _otherProps = new();

    private readonly List<Matrix4x4[]> _matrices = new();
    private readonly MaterialPropertyBlock _block = new();
    private int _count;

    public void Add(RenderObject renderObject)
    {
        RenderObjectData data = renderObject.RenderObjectData;

        int currentIndex = _count / 1000;
        if (_matrices.Count <= currentIndex)
        {
            _matrices.Add(new Matrix4x4[1000]);
            _loops.Add(new float[1000]);
            _animIndexs.Add(new float[1000]);
            _currentTimes.Add(new float[1000]);
            _lastAnimLoops.Add(new float[1000]);
            _lastAnimIndexs.Add(new float[1000]);
            _lastAnimExitTimes.Add(new float[1000]);

            foreach (var item in renderObject.OtherProps)
            {
                if (!_otherProps.ContainsKey(item.Key))
                    _otherProps[item.Key] = new List<float[]>();
                _otherProps[item.Key].Add(new float[1000]);
            }
        }

        _matrices[currentIndex][_count % 1000] = renderObject.Matrix;

        _loops[currentIndex][_count % 1000] = data.Loop ? 1 : 0;
        _animIndexs[currentIndex][_count % 1000] = data.AnimIndex;
        _currentTimes[currentIndex][_count % 1000] = data.CurrentTime;
        _lastAnimLoops[currentIndex][_count % 1000] = data.LastAnimLoop ? 1 : 0;
        _lastAnimIndexs[currentIndex][_count % 1000] = data.LastAnimIndex;
        _lastAnimExitTimes[currentIndex][_count % 1000] = data.LastAnimExitTime;

        foreach (var item in renderObject.OtherProps)
            _otherProps[item.Key][currentIndex][_count % 1000] = item.Value;

        _count++;
    }


    public void DrawMeshInstanced(Material material, Mesh mesh, CommandBuffer cmd)
    {
        if (_count == 0)
            return;

        _block.Clear();
        for (int i = 0; i < (_count - 1) / 1000 + 1; i++)
        {
            _block.SetFloatArray("_Loop", _loops[i]);
            _block.SetFloatArray("_AnimIndex", _animIndexs[i]);
            _block.SetFloatArray("_CurrentTime", _currentTimes[i]);

            _block.SetFloatArray("_LastAnimLoop", _lastAnimLoops[i]);
            _block.SetFloatArray("_LastAnimIndex", _lastAnimIndexs[i]);
            _block.SetFloatArray("_LastAnimExitTime", _lastAnimExitTimes[i]);

            foreach (var item in _otherProps)
                _block.SetFloatArray(item.Key, item.Value[i]);

            if (cmd != null)
            {
                cmd.DrawMeshInstanced(
                    mesh, 0, material, 0,
                    _matrices[i], Math.Min(_count - i * 1000, 1000),
                    _block);
            }
            else
            {
                Graphics.DrawMeshInstanced(
                    mesh, 0, material,
                    _matrices[i], Math.Min(_count - i * 1000, 1000),
                    _block, ShadowCastingMode.Off, false);
            }
        }
        _count = 0;
    }
}
