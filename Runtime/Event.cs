using System;

public class Event<T>
{
    private Action<T> _action;

    public void AddListener(Action<T> action)
    {
        _action += action;
    }

    public void RemoveAllListener()
    {
        _action = null;
    }

    public void Invoke(T t)
    {
        _action?.Invoke(t);
    }
}
