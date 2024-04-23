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

public readonly struct AnimInfo
{
    public readonly int StartFrame;
    public readonly int FrameCount;
    public readonly AnimationState AnimationState;

    public AnimInfo(AnimationState animationState, int startFrame)
    {
        StartFrame = startFrame;
        FrameCount = (int)(animationState.clip.frameRate * animationState.length + 1);
        AnimationState = animationState;
    }
}

public static class GPUSkinningBakerUtils
{
    private static Shader URPShader => Shader.Find("ybwork/GPUSkinningShader/URP");
    private static Shader CartoonShader => Shader.Find("ybwork/GPUSkinningShader/Cartoon");
    private static Shader CartoonOutLineShader => Shader.Find("ybwork/GPUSkinningShader/Cartoon-OutLine");
    private static int BoneMapProp => Shader.PropertyToID("_BoneMap");
    private static int BindposMapProp => Shader.PropertyToID("_BindposMap");
    private static int AnimInfosMapProp => Shader.PropertyToID("_AnimInfosMap");
    private static int FullAnimLenProp => Shader.PropertyToID("_FullAnimLen");

    // 在文件夹上添加右键菜单项
    [MenuItem("Assets/GPUSkinningBaker/SaveAll-URP", true, 30)]
    public static bool IsValidFolderSelection_URP()
    {
        // 检查当前所选的是不是文件夹
        return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/GPUSkinningBaker/SaveAll-URP", false, 30)]
    public static void PerformCustomAction_URP()
    {
        PerformCustomAction(URPShader);
    }

    // 在文件夹上添加右键菜单项
    [MenuItem("Assets/GPUSkinningBaker/SaveAll-Cartoon", true, 30)]
    public static bool IsValidFolderSelection_Cartoon()
    {
        // 检查当前所选的是不是文件夹
        return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
    }

    [MenuItem("Assets/GPUSkinningBaker/SaveAll-Cartoon", false, 30)]
    public static void PerformCustomAction_Cartoon()
    {
        PerformCustomAction(CartoonShader, CartoonOutLineShader);
    }

    public static void PerformCustomAction(params Shader[] shaders)
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
            Save(assetPath, baker, shaders);
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

        var mesh = new Mesh();
        mesh.CombineMeshes(combine, mergeSubMeshes: true, useMatrices: false);

        return mesh;
    }

    private static void Save(string assetPath, GPUSkinningBaker baker, Shader[] shaders)
    {
        Texture2D boneMap = BakeBoneMap(baker, out List<AnimInfo> animInfos);
        SaveAsset(CreateFolder(assetPath, "Textures"), "boneMap.asset", boneMap);

        Mesh sharedMesh = CombineMeshes(baker.GetComponentsInChildren<SkinnedMeshRenderer>());
        SaveAsset(CreateFolder(assetPath, "Meshes"), baker.name + ".mesh", sharedMesh);

        Texture2D bindposMap = BakeBindposMap(sharedMesh);
        SaveAsset(CreateFolder(assetPath, "Textures"), "bindposMap.asset", bindposMap);

        Texture2D animInfosMap = BakeAnimInfosMap(animInfos);
        SaveAsset(CreateFolder(assetPath, "Textures"), "animInfosMap.asset", animInfosMap);

        for (int i = 0; i < baker.Materials.Count; i++)
        {
            Material[] materials = CreateMaterial(baker.Materials[i], boneMap.height / 30, boneMap, bindposMap, animInfosMap, shaders);
            for (int j = 0; j < materials.Length; j++)
            {
                Material material = materials[j];
                string name = $"{baker.name}{i + 1}_{j + 1}.mat";
                SaveAsset(CreateFolder(assetPath, "Materials"), name, material);
            }
            SaveAsPrefab(CreateFolder(assetPath), $"{baker.name}_{i}.prefab", sharedMesh, materials);
        }
    }

    private static Texture2D BakeBoneMap(GPUSkinningBaker baker, out List<AnimInfo> animInfos)
    {
        AnimData animData = new(baker);

        // 记录每个动画的信息
        animInfos = new();
        foreach (AnimationState animationState in animData.AnimationClips)
        {
            if (!animationState.clip.legacy)
            {
                Debug.LogError(string.Format($"{animationState.clip.name} is not legacy!!"));
                continue;
            }

            animInfos.Add(new AnimInfo(animationState, animInfos.Sum(item => item.FrameCount)));
        }

        Texture2D boneMap = new(Mathf.NextPowerOfTwo(animData.MapWidth * 4), animInfos.Sum(item => item.FrameCount), TextureFormat.RGBAFloat, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            name = "boneMap"
        };

        foreach (AnimInfo animInfo in animInfos)
        {
            BakePerAnimClip(boneMap, animData, animInfo);
        }

        boneMap.filterMode = FilterMode.Point;
        return boneMap;
    }

    private static Texture2D BakeBindposMap(Mesh sharedMesh)
    {
        Texture2D bindposMap = new(Mathf.NextPowerOfTwo(sharedMesh.bindposeCount), 4, TextureFormat.RGBAFloat, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            name = "bindposMap"
        };
        for (int j = 0; j < sharedMesh.bindposeCount; j++)
        {
            Matrix4x4 matrix = sharedMesh.bindposes[j];
            bindposMap.SetPixel(j, 0, new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03));
            bindposMap.SetPixel(j, 1, new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13));
            bindposMap.SetPixel(j, 2, new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
            bindposMap.SetPixel(j, 3, new Color(matrix.m30, matrix.m31, matrix.m32, matrix.m33));
        }
        bindposMap.Apply();
        bindposMap.filterMode = FilterMode.Point;
        return bindposMap;
    }

    private static Texture2D BakeAnimInfosMap(List<AnimInfo> animInfos)
    {
        Texture2D animInfosMap = new(Mathf.NextPowerOfTwo(animInfos.Count), 2, TextureFormat.RGBAFloat, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            name = "animInfosMap"
        };
        float fullAnimFrameCount = animInfos.Sum(item => item.FrameCount);
        for (int i = 0; i < animInfos.Count; i++)
        {
            float startTime = animInfos[i].StartFrame / 30f;
            animInfosMap.SetPixel(i, 0, new Color(startTime, startTime, startTime, 1));

            float duration = (animInfos[i].FrameCount - 1) / 30f;
            animInfosMap.SetPixel(i, 1, new Color(duration, duration, duration, 1));
        }
        animInfosMap.filterMode = FilterMode.Point;
        animInfosMap.Apply();
        return animInfosMap;
    }

    private static void BakePerAnimClip(Texture2D boneMap, AnimData animData, AnimInfo animInfo)
    {
        float sampleTime = 0;
        float perFrameTime = 1 / animInfo.AnimationState.clip.frameRate;

        animData.AnimationPlay(animInfo.AnimationState.name);

        Matrix4x4[] boneMatrices = new Matrix4x4[animData.MapWidth];
        for (var i = 0; i < animInfo.FrameCount; i++)
        {
            animInfo.AnimationState.time = sampleTime;
            animData.SampleAnimAndBakeBoneMatrices(ref boneMatrices);

            for (int j = 0; j < boneMatrices.Length; j++)
            {
                Matrix4x4 matrix = boneMatrices[j];
                boneMap.SetPixel(j * 4 + 0, i + animInfo.StartFrame, new Color(matrix.m00, matrix.m01, matrix.m02, matrix.m03));
                boneMap.SetPixel(j * 4 + 1, i + animInfo.StartFrame, new Color(matrix.m10, matrix.m11, matrix.m12, matrix.m13));
                boneMap.SetPixel(j * 4 + 2, i + animInfo.StartFrame, new Color(matrix.m20, matrix.m21, matrix.m22, matrix.m23));
                boneMap.SetPixel(j * 4 + 3, i + animInfo.StartFrame, new Color(matrix.m30, matrix.m31, matrix.m32, matrix.m33));
            }

            sampleTime += perFrameTime;
        }
        boneMap.Apply();
    }

    private static void SaveAsset(string folderPath, string filename, Object asset)
    {
        string path = Path.Combine(folderPath, filename);
        if (File.Exists(path))
            File.Delete(path);
        AssetDatabase.CreateAsset(asset, path);
    }

    private static Material[] CreateMaterial(Material sourceMaterial, float fullAnimLen,
        Texture2D boneMap, Texture2D bindposMap, Texture2D animInfosMap, Shader[] shaders)
    {
        return shaders
            .Select(shader =>
            {
                var material = new Material(shader);
                material.CopyMatchingPropertiesFromMaterial(sourceMaterial);
                material.SetTexture(BoneMapProp, boneMap);
                material.SetTexture(BindposMapProp, bindposMap);
                material.SetTexture(AnimInfosMapProp, animInfosMap);
                material.SetFloat(FullAnimLenProp, fullAnimLen);
                material.enableInstancing = true;
                return material;
            })
            .ToArray();
    }

    private static void SaveAsPrefab(string path, string filename, Mesh mesh, Material[] materials)
    {
        filename = Path.Combine(path, filename);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filename);
        if (prefab == null)
        {
            var go = new GameObject();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = materials;
            PrefabUtility.SaveAsPrefabAsset(go, filename);
            Object.DestroyImmediate(go);
        }
        else
        {
            if (!prefab.TryGetComponent(out MeshFilter meshFilter))
                meshFilter = prefab.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            if (!prefab.TryGetComponent(out MeshRenderer meshRenderer))
                meshRenderer = prefab.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;

            PrefabUtility.SaveAsPrefabAsset(prefab, filename);
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
