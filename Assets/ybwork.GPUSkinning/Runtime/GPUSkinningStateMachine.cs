using System.Collections.Generic;

public class GPUSkinningStateMachine
{
    private readonly Dictionary<int, bool> _isLoops = new();
    private readonly Dictionary<int, int> _nextStates = new();

    public void RegisterOnceState(int state, int nextState)
    {
        _nextStates[state] = nextState;
    }

    public void RegisterLoopState(int state)
    {
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
