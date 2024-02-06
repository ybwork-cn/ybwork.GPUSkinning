using UnityEngine;

public class MeshInfo
{
    public Matrix4x4[] Bindposes;
    public BoneWeight[] BoneWeights;
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public MeshInfo(Mesh mesh)
    {
        Bindposes = mesh.bindposes;
        BoneWeights = mesh.boneWeights;
        Vertices = mesh.vertices;
        Normals = mesh.normals;
    }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GPUSkinnedMeshBaker : MonoBehaviour
{
    [SerializeField] Transform _root;
    [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer;
    MeshInfo _meshInfo;

    Mesh _mesh;
    Vector3[] _vertices;
    Vector3[] _normals;

    private void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        var sharedMesh = _skinnedMeshRenderer.sharedMesh;
        _mesh = Instantiate(sharedMesh);
        meshFilter.mesh = _mesh;
        _vertices = new Vector3[sharedMesh.vertexCount];
        _normals = new Vector3[sharedMesh.vertexCount];
        _meshInfo = new MeshInfo(sharedMesh);
    }

    private void Update()
    {
        Transform[] bones = _skinnedMeshRenderer.bones;
        Matrix4x4 matrix = _root.worldToLocalMatrix;
        Matrix4x4[] bindposes = _meshInfo.Bindposes;
        BoneWeight[] boneWeights = _meshInfo.BoneWeights;

        for (int i = 0; i < _vertices.Length; i++)
        {
            var vertexOriginPosition = _meshInfo.Vertices[i];
            var vertexOriginNormal = _meshInfo.Normals[i];
            var boneIndex0 = boneWeights[i].boneIndex0;
            var boneIndex1 = boneWeights[i].boneIndex1;
            var boneIndex2 = boneWeights[i].boneIndex2;
            var boneIndex3 = boneWeights[i].boneIndex3;

            var weight0 = boneWeights[i].weight0;
            var weight1 = boneWeights[i].weight1;
            var weight2 = boneWeights[i].weight2;
            var weight3 = boneWeights[i].weight3;

            var matrix0 = matrix * bones[boneIndex0].localToWorldMatrix * bindposes[boneIndex0];
            var matrix1 = matrix * bones[boneIndex1].localToWorldMatrix * bindposes[boneIndex1];
            var matrix2 = matrix * bones[boneIndex2].localToWorldMatrix * bindposes[boneIndex2];
            var matrix3 = matrix * bones[boneIndex3].localToWorldMatrix * bindposes[boneIndex3];

            var vertexRuntimePosition0 = matrix0.MultiplyPoint(vertexOriginPosition) * weight0;
            var vertexRuntimePosition1 = matrix1.MultiplyPoint(vertexOriginPosition) * weight1;
            var vertexRuntimePosition2 = matrix2.MultiplyPoint(vertexOriginPosition) * weight2;
            var vertexRuntimePosition3 = matrix3.MultiplyPoint(vertexOriginPosition) * weight3;
            _vertices[i] = vertexRuntimePosition0 + vertexRuntimePosition1 + vertexRuntimePosition2 + vertexRuntimePosition3;

            var vertexRuntimeNormal0 = matrix0.MultiplyVector(vertexOriginNormal) * weight0;
            var vertexRuntimeNormal1 = matrix1.MultiplyVector(vertexOriginNormal) * weight1;
            var vertexRuntimeNormal2 = matrix2.MultiplyVector(vertexOriginNormal) * weight2;
            var vertexRuntimeNormal3 = matrix3.MultiplyVector(vertexOriginNormal) * weight3;
            _normals[i] = vertexRuntimeNormal0 + vertexRuntimeNormal1 + vertexRuntimeNormal2 + vertexRuntimeNormal3;
        }
        _mesh.vertices = _vertices;
        _mesh.normals = _normals;
    }
}
