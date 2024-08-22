using UnityEngine;

public class RenderObject
{
    public Matrix4x4 Matrix;
    public RenderObjectData Data;

    public readonly RenderGroup Group;

    public RenderObject(RenderGroup group)
    {
        Matrix = Matrix4x4.identity;
        Data = default;
        Group = null;
        Group = group;
    }
}
