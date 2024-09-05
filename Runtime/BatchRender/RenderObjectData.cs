using System;

[Serializable]
internal class RenderObjectData
{
    public bool Loop;
    public int AnimIndex;
    public float CurrentTime;

    public bool LastAnimLoop;
    public int LastAnimIndex;
    public float LastAnimExitTime;

    public void SwitchState(int state, bool loop)
    {
        LastAnimLoop = Loop;
        LastAnimIndex = AnimIndex;
        LastAnimExitTime = CurrentTime;
        Loop = loop;
        AnimIndex = state;
        CurrentTime = 0;
    }

    public void Update(float deltaTime)
    {
        CurrentTime += deltaTime;
    }
}
