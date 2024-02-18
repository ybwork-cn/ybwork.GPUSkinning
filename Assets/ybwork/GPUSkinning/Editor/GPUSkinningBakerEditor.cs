// Changed by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 保存需要烘焙的动画的相关数据
/// </summary>
public readonly struct AnimData
{
    public readonly int MapWidth;
    public readonly int[] MapWidths;
    public readonly List<AnimationState> AnimationClips;
    public readonly string Name;

    private readonly Animation Animation;
    private readonly SkinnedMeshRenderer[] Skins;

    public AnimData(GPUSkinningBaker baker)
    {
        Animation anim = baker.GetComponent<Animation>();
        Skins = baker.GetComponentsInChildren<SkinnedMeshRenderer>();

        MapWidths = Skins.Select(render => render.bones.Length).ToArray();
        MapWidth = Mathf.NextPowerOfTwo(MapWidths.Sum());
        AnimationClips = new List<AnimationState>(anim.Cast<AnimationState>());
        Animation = anim;
        Name = baker.name;
    }

    #region METHODS

    public readonly void AnimationPlay(string animName)
    {
        Animation.Play(animName);
    }

    public void SampleAnimAndBakeBoneMatrices(ref Matrix4x4[] boneMatrices)
    {
        SampleAnim();
        BakeBoneMatrices(ref boneMatrices);
    }

    private void SampleAnim()
    {
        if (Animation == null)
        {
            Debug.LogError("animation is null!!");
            return;
        }

        Animation.Sample();
    }

    private void BakeBoneMatrices(ref Matrix4x4[] boneMatrices)
    {
        int index = 0;
        foreach (SkinnedMeshRenderer skin in Skins)
        {
            foreach (Transform bone in skin.bones)
            {
                boneMatrices[index] = bone.localToWorldMatrix;
                index++;
            }
        }
    }

    #endregion
}

/// <summary>
/// 烘焙后的数据
/// </summary>
public readonly struct BakedData
{
    public readonly string Name;
    public readonly float AnimLen;
    public readonly Texture2D BoneMap;

    public BakedData(string name, float animLen, Texture2D boneMap)
    {
        Name = name;
        AnimLen = animLen;
        BoneMap = boneMap;
    }
}

public static class GPUSkinningBakerUtils
{
    private static Shader URPShader => Shader.Find("ybwork/URP/GPUSkinningShader");
    private static int BoneMapProp => Shader.PropertyToID("_BoneMap");
    private static int BindposMapProp => Shader.PropertyToID("_BindposMap");
    private static int AnimLenProp => Shader.PropertyToID("_AnimLen");

    // 在文件夹上添加右键菜单项
    [MenuItem("Assets/GPUSkinningBaker/SaveAll", true, 30)]
    public static bool IsValidFolderSelection()
    {
        // 检查当前所选的是不是文件夹
        return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/GPUSkinningBaker/SaveAll", false, 30)]
    public static void PerformCustomAction()
    {
        // 处理右键菜单的逻辑
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        List<(string assetPath, GPUSkinningBaker baker)> bakers = new();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath }); // 查找folderPath中的所有预制体
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (!prefab.TryGetComponent<GPUSkinningBaker>(out _))
                continue;
            GameObject go = Object.Instantiate(prefab);
            go.name = prefab.name;
            GPUSkinningBaker baker = go.GetComponent<GPUSkinningBaker>();
            bakers.Add((assetPath, baker));
        }
        for (int i = 0; i < bakers.Count; i++)
        {
            (string assetPath, GPUSkinningBaker baker) = bakers[i];
            EditorUtility.DisplayProgressBar("Baking", "Baking GPUSkinningMap " + assetPath, (float)i / bakers.Count);
            Save(assetPath, baker);
            Object.DestroyImmediate(baker.gameObject);
            Debug.Log("生成完成:" + assetPath);
        }
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
    }

    private static Mesh CombineMeshes(SkinnedMeshRenderer[] skinnedMeshRenderers)
    {
        CombineInstance[] combine = new CombineInstance[skinnedMeshRenderers.Length];

        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            combine[i].mesh = skinnedMeshRenderers[i].sharedMesh;
            combine[i].transform = skinnedMeshRenderers[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine, mergeSubMeshes: true, useMatrices: false);

        return mesh;
    }

    private static void Save(string assetPath, GPUSkinningBaker baker)
    {
        List<BakedData> datas = Bake(baker);
        var textures = datas.Select(data => SaveAsAsset(assetPath, baker, data)).ToArray();

        string folderPath = CreateFolder(assetPath);
        Mesh sharedMesh = CombineMeshes(baker.GetComponentsInChildren<SkinnedMeshRenderer>());
        //Mesh sharedMesh = baker.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
        AssetDatabase.CreateAsset(sharedMesh, Path.Combine(folderPath, $"{baker.name}.mesh"));

        Texture2D bindposMap = new Texture2D(sharedMesh.bindposeCount, 4, TextureFormat.RGBAHalf, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            name = "bindposMap"
        };
        for (int j = 0; j < sharedMesh.bindposeCount; j++)
        {
            Matrix4x4 bindpos = sharedMesh.bindposes[j];
            Matrix4x4 matrix = sharedMesh.bindposes[j];
            //bindposMap.SetPixel(j, 0, new Color(matrix.m00, matrix.m10, matrix.m20, matrix.m30));
            //bindposMap.SetPixel(j, 1, new Color(matrix.m01, matrix.m11, matrix.m21, matrix.m31));
            //bindposMap.SetPixel(j, 2, new Color(matrix.m02, matrix.m12, matrix.m22, matrix.m32));
            //bindposMap.SetPixel(j, 3, new Color(matrix.m03, matrix.m13, matrix.m23, matrix.m33));
            bindposMap.SetPixel(j, 0, new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03));
            bindposMap.SetPixel(j, 1, new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13));
            bindposMap.SetPixel(j, 2, new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
            bindposMap.SetPixel(j, 3, new Color(matrix.m30, matrix.m31, matrix.m32, matrix.m33));
        }
        bindposMap.Apply();
        string bindposMapPath = Path.Combine(folderPath, $"{bindposMap.name}.asset");
        if (File.Exists(bindposMapPath))
            File.Delete(bindposMapPath);
        AssetDatabase.CreateAsset(bindposMap, bindposMapPath);

        for (int i = 0; i < baker.Materials.Count; i++)
        {
            string name = (i + 1).ToString();
            Material[] materials = Enumerable.Range(0, datas.Count)
                .Select(index => SaveAsMat(assetPath, baker, name, datas[index], baker.Materials[i], textures[index], bindposMap))
                .OrderBy(material => material.name)
                .ToArray();
            string path = Path.Combine(folderPath, $"{baker.name}_{name}.prefab");
            SaveAsPrefab(path, sharedMesh, materials);
        }
    }

    private static List<BakedData> Bake(GPUSkinningBaker baker)
    {
        AnimData animData = new AnimData(baker);

        List<BakedData> bakedDataList = new();

        //每一个动作都生成一个动作图
        foreach (AnimationState animationState in animData.AnimationClips)
        {
            if (!animationState.clip.legacy)
            {
                Debug.LogError(string.Format($"{animationState.clip.name} is not legacy!!"));
                continue;
            }
            bakedDataList.Add(BakePerAnimClip(animData, animationState));
        }
        return bakedDataList;
    }

    private static BakedData BakePerAnimClip(AnimData animData, AnimationState curAnim)
    {
        float sampleTime = 0;
        int curClipFrame = Mathf.ClosestPowerOfTwo((int)(curAnim.clip.frameRate * curAnim.length + 1));
        float perFrameTime = curAnim.length / (curClipFrame - 1);
        Debug.Log(curAnim.clip.name + ":" + curAnim.clip.frameRate + ":" + curAnim.length);

        Texture2D boneMap = new Texture2D(animData.MapWidth * 4, curClipFrame, TextureFormat.RGBAHalf, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            name = curAnim.name
        };
        animData.AnimationPlay(curAnim.name);

        Matrix4x4[] boneMatrices = new Matrix4x4[animData.MapWidth];
        for (var i = 0; i < curClipFrame; i++)
        {
            curAnim.time = sampleTime;

            animData.SampleAnimAndBakeBoneMatrices(ref boneMatrices);

            for (int j = 0; j < boneMatrices.Length; j++)
            {
                Matrix4x4 matrix = boneMatrices[j];
                //boneMap.SetPixel(j * 4 + 0, i, new Color(matrix.m00, matrix.m10, matrix.m20, matrix.m30));
                //boneMap.SetPixel(j * 4 + 1, i, new Color(matrix.m01, matrix.m11, matrix.m21, matrix.m31));
                //boneMap.SetPixel(j * 4 + 2, i, new Color(matrix.m02, matrix.m12, matrix.m22, matrix.m32));
                //boneMap.SetPixel(j * 4 + 3, i, new Color(matrix.m03, matrix.m13, matrix.m23, matrix.m33));
                boneMap.SetPixel(j * 4 + 0, i, new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03));
                boneMap.SetPixel(j * 4 + 1, i, new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13));
                boneMap.SetPixel(j * 4 + 2, i, new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
                boneMap.SetPixel(j * 4 + 3, i, new Color(matrix.m30, matrix.m31, matrix.m32, matrix.m33));
            }

            sampleTime += perFrameTime;
        }
        boneMap.Apply();

        return new BakedData(boneMap.name, curAnim.clip.length, boneMap);
    }

    private static Texture2D SaveAsAsset(string assetPath, GPUSkinningBaker baker, BakedData data)
    {
        string folderPath = CreateFolder(assetPath, "Textures");
        string path = Path.Combine(folderPath, $"{baker.name}_{data.Name}.asset");
        if (File.Exists(path))
            File.Delete(path);
        AssetDatabase.CreateAsset(data.BoneMap, path);
        return data.BoneMap;
    }

    private static Material SaveAsMat(string assetPath, GPUSkinningBaker baker, string name, BakedData data,
        Material sourceMaterial, Texture2D boneMap, Texture2D bindposMap)
    {
        string folderPath = CreateFolder(assetPath, "Matrials");
        string path = Path.Combine(folderPath, $"{baker.name}_{name}_{data.Name}.mat");
        if (File.Exists(path))
            File.Delete(path);

        var material = new Material(URPShader);
        material.CopyMatchingPropertiesFromMaterial(sourceMaterial);
        material.SetTexture(BoneMapProp, boneMap);
        material.SetTexture(BindposMapProp, bindposMap);
        material.SetFloat(AnimLenProp, data.AnimLen);
        material.enableInstancing = true;

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void SaveAsPrefab(string path, Mesh mesh, Material[] materials)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            var go = new GameObject();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().material = materials[0];
            go.AddComponent<MaterialChangerComponent>().materials = materials;
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
        else
        {
            if (!prefab.TryGetComponent(out MeshFilter meshFilter))
                meshFilter = prefab.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            if (!prefab.TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer = prefab.AddComponent<MeshRenderer>();
            meshRenderer.material = materials[0];

            if (!prefab.TryGetComponent(out MaterialChangerComponent materialChangerComponent))
                materialChangerComponent = prefab.AddComponent<MaterialChangerComponent>();
            materialChangerComponent.materials = materials;

            PrefabUtility.SaveAsPrefabAsset(prefab, path);
        }
    }

    private static string CreateFolder(string assetPath, string subPath = null)
    {
        string path = Path.GetDirectoryName(assetPath);
        string folderPath = Path.Combine(path, GPUSkinningBaker.SubPath);
        string result = folderPath;
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(path, GPUSkinningBaker.SubPath);
        }
        if (!string.IsNullOrEmpty(subPath))
        {
            string subFolderPath = Path.Combine(folderPath, subPath);
            result = subFolderPath;
            if (!AssetDatabase.IsValidFolder(subFolderPath))
            {
                AssetDatabase.CreateFolder(folderPath, subPath);
            }
        }
        return result;
    }
}
