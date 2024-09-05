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

    private readonly List<Matrix4x4[]> _matrices = new();
    private readonly MaterialPropertyBlock _block = new();
    private int Count { get; set; }

    public void Add(Matrix4x4 matrixs, RenderObjectData data)
    {
        int currentIndex = Count / 1000;
        if (_matrices.Count <= currentIndex)
        {
            _matrices.Add(new Matrix4x4[1000]);
            _loops.Add(new float[1000]);
            _animIndexs.Add(new float[1000]);
            _currentTimes.Add(new float[1000]);
            _lastAnimLoops.Add(new float[1000]);
            _lastAnimIndexs.Add(new float[1000]);
            _lastAnimExitTimes.Add(new float[1000]);
        }

        _matrices[currentIndex][Count % 1000] = matrixs;

        _loops[currentIndex][Count % 1000] = data.Loop ? 1 : 0;
        _animIndexs[currentIndex][Count % 1000] = data.AnimIndex;
        _currentTimes[currentIndex][Count % 1000] = data.CurrentTime;
        _lastAnimLoops[currentIndex][Count % 1000] = data.LastAnimLoop ? 1 : 0;
        _lastAnimIndexs[currentIndex][Count % 1000] = data.LastAnimIndex;
        _lastAnimExitTimes[currentIndex][Count % 1000] = data.LastAnimExitTime;

        Count++;
    }


    public void DrawMeshInstanced(Material material, Mesh mesh, CommandBuffer cmd)
    {
        if (Count == 0)
            return;

        _block.Clear();
        for (int i = 0; i < (Count - 1) / 1000 + 1; i++)
        {
            _block.SetFloatArray("_Loop", _loops[i]);
            _block.SetFloatArray("_AnimIndex", _animIndexs[i]);
            _block.SetFloatArray("_CurrentTime", _currentTimes[i]);

            _block.SetFloatArray("_LastAnimLoop", _lastAnimLoops[i]);
            _block.SetFloatArray("_LastAnimIndex", _lastAnimIndexs[i]);
            _block.SetFloatArray("_LastAnimExitTime", _lastAnimExitTimes[i]);
            if (cmd != null)
            {
                cmd.DrawMeshInstanced(
                    mesh, 0, material, 0,
                    _matrices[i], Math.Min(Count - i * 1000, 1000),
                    _block);
            }
            else
            {
                Graphics.DrawMeshInstanced(
                    mesh, 0, material,
                    _matrices[i], Math.Min(Count - i * 1000, 1000),
                    _block, ShadowCastingMode.Off, false);
            }
        }
        Count = 0;
    }
}
