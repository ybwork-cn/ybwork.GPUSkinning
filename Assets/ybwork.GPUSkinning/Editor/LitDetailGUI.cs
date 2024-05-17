// Created by 月北(ybwork-cn) https://github.com/ybwork-cn/

using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class LitDetailGUI
{
    public static class Styles
    {
        public static readonly GUIContent detailInputs = EditorGUIUtility.TrTextContent("Detail Inputs", "These settings define the surface details by tiling and overlaying additional maps on the surface.");

        public static readonly GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Mask", "Select a mask for the Detail map. The mask uses the alpha channel of the selected texture. The Tiling and Offset settings have no effect on the mask.");

        public static readonly GUIContent detailAlbedoMapText = EditorGUIUtility.TrTextContent("Base Map", "Select the surface detail texture.The alpha of your texture determines surface hue and intensity.");

        public static readonly GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Designates a Normal Map to create the illusion of bumps and dents in the details of this Material's surface.");

        public static readonly GUIContent detailAlbedoMapScaleInfo = EditorGUIUtility.TrTextContent("Setting the scaling factor to a value other than 1 results in a less performant shader variant.");

        public static readonly GUIContent detailAlbedoMapFormatError = EditorGUIUtility.TrTextContent("This texture is not in linear space.");
    }

    public struct DetailLitProperties
    {
        public MaterialProperty detailMask;

        public MaterialProperty detailAlbedoMapScale;

        public MaterialProperty detailAlbedoMap;

        public MaterialProperty detailNormalMapScale;

        public MaterialProperty detailNormalMap;

        public DetailLitProperties(MaterialProperty[] properties)
        {
            detailMask = BaseShaderGUI.FindProperty("_DetailMask", properties, propertyIsMandatory: false);
            detailAlbedoMapScale = BaseShaderGUI.FindProperty("_DetailAlbedoMapScale", properties, propertyIsMandatory: false);
            detailAlbedoMap = BaseShaderGUI.FindProperty("_DetailAlbedoMap", properties, propertyIsMandatory: false);
            detailNormalMapScale = BaseShaderGUI.FindProperty("_DetailNormalMapScale", properties, propertyIsMandatory: false);
            detailNormalMap = BaseShaderGUI.FindProperty("_DetailNormalMap", properties, propertyIsMandatory: false);
        }
    }

    public static void DoDetailArea(DetailLitProperties properties, MaterialEditor materialEditor)
    {
        materialEditor.TexturePropertySingleLine(Styles.detailMaskText, properties.detailMask);
        materialEditor.TexturePropertySingleLine(Styles.detailAlbedoMapText, properties.detailAlbedoMap, (properties.detailAlbedoMap.textureValue != null) ? properties.detailAlbedoMapScale : null);
        if (properties.detailAlbedoMapScale.floatValue != 1f)
        {
            EditorGUILayout.HelpBox(Styles.detailAlbedoMapScaleInfo.text, MessageType.Info, wide: true);
        }

        Texture2D texture2D = properties.detailAlbedoMap.textureValue as Texture2D;
        if (texture2D != null && GraphicsFormatUtility.IsSRGBFormat(texture2D.graphicsFormat))
        {
            EditorGUILayout.HelpBox(Styles.detailAlbedoMapFormatError.text, MessageType.Warning, wide: true);
        }

        materialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, properties.detailNormalMap, (properties.detailNormalMap.textureValue != null) ? properties.detailNormalMapScale : null);
        materialEditor.TextureScaleOffsetProperty(properties.detailAlbedoMap);
    }

    public static void SetMaterialKeywords(Material material)
    {
        if (material.HasProperty("_DetailAlbedoMap") && material.HasProperty("_DetailNormalMap") && material.HasProperty("_DetailAlbedoMapScale"))
        {
            bool flag = material.GetFloat("_DetailAlbedoMapScale") != 1f;
            bool flag2 = (bool)material.GetTexture("_DetailAlbedoMap") || (bool)material.GetTexture("_DetailNormalMap");
            CoreUtils.SetKeyword(material, "_DETAIL_MULX2", !flag && flag2);
            CoreUtils.SetKeyword(material, "_DETAIL_SCALED", flag && flag2);
        }
    }
}
