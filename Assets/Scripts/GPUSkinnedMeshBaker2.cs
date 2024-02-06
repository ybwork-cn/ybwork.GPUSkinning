using System.Linq;
using UnityEngine;

public class GPUSkinnedMeshBaker2 : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] Material _material;
    [SerializeField] GameObject _prefab;


    private void Awake()
    {
        _material.SetMatrixArray("_Bindposes", _skinnedMeshRenderer.sharedMesh.bindposes);

        Transform _parent = new GameObject().transform;
        for (int i = 0; i < 900; i++)
        {
            Instantiate(_prefab, _parent).transform.position = new Vector3(i % 30, 0, i / 30);
        }
    }

    private void Update()
    {
        var bones = _skinnedMeshRenderer.bones.Select(bone => bone.localToWorldMatrix).ToArray();
        _material.SetMatrixArray("_Bones", bones);
        _material.SetMatrix("_RootTransform", _skinnedMeshRenderer.transform.worldToLocalMatrix);
    }
}
