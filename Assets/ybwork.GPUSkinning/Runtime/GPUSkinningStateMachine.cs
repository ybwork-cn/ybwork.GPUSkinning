﻿using System.Collections.Generic;

public class GPUSkinningStateMachine
{
    private readonly Dictionary<int, bool> _isLoops = new();
    private readonly Dictionary<int, int> _nextStates = new();
    private readonly GPUSkinningInfo _gpuSkinningInfo;

    public GPUSkinningStateMachine(GPUSkinningInfo gpuSkinningInfo)
    {
        _gpuSkinningInfo = gpuSkinningInfo;
    }

    public void RegisterOnceState(int state, int nextState)
    {
        int length = _gpuSkinningInfo.AnimaitonLengths.Length;

        if (state < 0 || state >= length)
            throw new System.IndexOutOfRangeException($"{nameof(state)} {state} not in [{0},{length})");
        if (state < 0 || state >= length)
            throw new System.IndexOutOfRangeException($"{nameof(nextState)} {nextState} not in [{0},{length})");

        _nextStates[state] = nextState;
    }

    public void RegisterLoopState(int state)
    {
        int length = _gpuSkinningInfo.AnimaitonLengths.Length;
        if (state < 0 || state >= length)
            throw new System.IndexOutOfRangeException($"{nameof(state)} {state} not in [{0},{length})");

        _isLoops[state] = true;
    }

    internal bool TryGetNextState(int state, out int nextState)
    {
        return _nextStates.TryGetValue(state, out nextState);
    }

    internal bool GetStateIsLoop(int state)
    {
        if (!_isLoops.TryGetValue(state, out bool isLoop))
            isLoop = false;
        return isLoop;
    }
}
