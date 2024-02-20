// Created by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class GPUSkinningShaderEditor : BaseShaderGUI
{
    private static readonly string[] workflowModeNames = Enum.GetNames(typeof(LitGUI.WorkflowMode));
    private LitGUI.LitProperties litProperties;
    private LitDetailGUI.DetailLitProperties litDetailProperties;

    public static readonly GUIContent boneMapTitle = EditorGUIUtility.TrTextContent("BoneMap");
    public static readonly GUIContent bindposMapTitle = EditorGUIUtility.TrTextContent("BindposMap");
    public static readonly GUIContent animInfosMapTitle = EditorGUIUtility.TrTextContent("AnimInfosMap");
    public static readonly string loopTitle = "Loop";
    public static readonly string animIndexTitle = "Anim Index";
    public static readonly string currentTimeTitle = "Current Time";
    public static readonly string lastAnimLoopTitle = "Last Anim Loop";
    public static readonly string lastAnimIndexTitle = "Last Anim Index";
    public static readonly string lastAnimExitTimeTitle = "Last Anim Exit Time";

    protected MaterialProperty BoneMapProp { get; set; }
    protected MaterialProperty BindposMapProp { get; set; }
    protected MaterialProperty AnimInfosMapProp { get; set; }
    protected MaterialProperty LoopProp { get; set; }
    protected MaterialProperty AnimIndexProp { get; set; }
    protected MaterialProperty CurrentTimeProp { get; set; }
    protected MaterialProperty LastAnimLoopProp { get; set; }
    protected MaterialProperty LastAnimIndexProp { get; set; }
    protected MaterialProperty LastAnimExitTimeProp { get; set; }

    public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
    {
        materialScopesList.RegisterHeaderScope(LitDetailGUI.Styles.detailInputs, Expandable.Details, delegate
        {
            LitDetailGUI.DoDetailArea(litDetailProperties, base.materialEditor);
        });
    }

    public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        litProperties = new LitGUI.LitProperties(properties);
        litDetailProperties = new LitDetailGUI.DetailLitProperties(properties);
        BoneMapProp = FindProperty("_BoneMap", properties, propertyIsMandatory: false);
        BindposMapProp = FindProperty("_BindposMap", properties, propertyIsMandatory: false);
        AnimInfosMapProp = FindProperty("_AnimInfosMap", properties, propertyIsMandatory: false);
        LoopProp = FindProperty("_Loop", properties, propertyIsMandatory: false);
        AnimIndexProp = FindProperty("_AnimIndex", properties, propertyIsMandatory: false);
        CurrentTimeProp = FindProperty("_CurrentTime", properties, propertyIsMandatory: false);
        LastAnimLoopProp = FindProperty("_LastAnimLoop", properties, propertyIsMandatory: false);
        LastAnimIndexProp = FindProperty("_LastAnimIndex", properties, propertyIsMandatory: false);
        LastAnimExitTimeProp = FindProperty("_LastAnimExitTime", properties, propertyIsMandatory: false);
    }

    public override void ValidateMaterial(Material material)
    {
        SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, LitDetailGUI.SetMaterialKeywords);
    }

    public override void DrawSurfaceOptions(Material material)
    {
        EditorGUIUtility.labelWidth = 0f;
        if (litProperties.workflowMode != null)
        {
            DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, workflowModeNames);
        }

        base.DrawSurfaceOptions(material);
    }

    public override void DrawSurfaceInputs(Material material)
    {
        materialEditor.TexturePropertySingleLine(boneMapTitle, BoneMapProp);
        materialEditor.TexturePropertySingleLine(bindposMapTitle, BindposMapProp);
        materialEditor.TexturePropertySingleLine(animInfosMapTitle, AnimInfosMapProp);
        materialEditor.ShaderProperty(LoopProp, new GUIContent(loopTitle));
        materialEditor.IntShaderProperty(AnimIndexProp, new GUIContent(animIndexTitle));
        materialEditor.ShaderProperty(CurrentTimeProp, currentTimeTitle);
        EditorGUILayout.Space();
        materialEditor.ShaderProperty(LastAnimLoopProp, new GUIContent(lastAnimLoopTitle));
        materialEditor.IntShaderProperty(LastAnimIndexProp, new GUIContent(lastAnimIndexTitle));
        materialEditor.ShaderProperty(LastAnimExitTimeProp, lastAnimExitTimeTitle);
        EditorGUILayout.Space();


        base.DrawSurfaceInputs(material);
        LitGUI.Inputs(litProperties, base.materialEditor, material);
        DrawEmissionProperties(material, keyword: true);
        BaseShaderGUI.DrawTileOffset(base.materialEditor, base.baseMapProp);
    }

    public override void DrawAdvancedOptions(Material material)
    {
        if (litProperties.reflections != null && litProperties.highlights != null)
        {
            base.materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
            base.materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
        }

        base.DrawAdvancedOptions(material);
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        if (material == null)
        {
            throw new ArgumentNullException("material");
        }

        if (material.HasProperty("_Emission"))
        {
            material.SetColor("_EmissionColor", material.GetColor("_Emission"));
        }

        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
        {
            BaseShaderGUI.SetupMaterialBlendMode(material);
            return;
        }

        SurfaceType surfaceType = SurfaceType.Opaque;
        BlendMode blendMode = BlendMode.Alpha;
        if (oldShader.name.Contains("/Transparent/Cutout/"))
        {
            surfaceType = SurfaceType.Opaque;
            material.SetFloat("_AlphaClip", 1f);
        }
        else if (oldShader.name.Contains("/Transparent/"))
        {
            surfaceType = SurfaceType.Transparent;
            blendMode = BlendMode.Alpha;
        }

        material.SetFloat("_Blend", (float)blendMode);
        material.SetFloat("_Surface", (float)surfaceType);
        if (surfaceType == SurfaceType.Opaque)
        {
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        else
        {
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        if (oldShader.name.Equals("Standard (Specular setup)"))
        {
            material.SetFloat("_WorkflowMode", 0f);
            Texture texture = material.GetTexture("_SpecGlossMap");
            if (texture != null)
            {
                material.SetTexture("_MetallicSpecGlossMap", texture);
            }
        }
        else
        {
            material.SetFloat("_WorkflowMode", 1f);
            Texture texture2 = material.GetTexture("_MetallicGlossMap");
            if (texture2 != null)
            {
                material.SetTexture("_MetallicSpecGlossMap", texture2);
            }
        }
    }
}
