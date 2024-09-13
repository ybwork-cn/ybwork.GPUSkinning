// Created by 月北(ybwork-cn) https://github.com/ybwork-cn/

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class LitGUI
{
    public enum WorkflowMode
    {
        Specular,
        Metallic
    }

    public enum SmoothnessMapChannel
    {
        SpecularMetallicAlpha,
        AlbedoAlpha
    }

    public static class Styles
    {
        public static GUIContent workflowModeText = EditorGUIUtility.TrTextContent("Workflow Mode", "Select a workflow that fits your textures. Choose between Metallic or Specular.");

        public static GUIContent specularMapText = EditorGUIUtility.TrTextContent("Specular Map", "Designates a Specular Map and specular color determining the apperance of reflections on this Material's surface.");

        public static GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic Map", "Sets and configures the map for the Metallic workflow.");

        public static GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Controls the spread of highlights and reflections on the surface.");

        public static GUIContent smoothnessMapText = EditorGUIUtility.TrTextContent("Smoothness Map", "Sets and configures the map for the Metallic workflow.");

        public static GUIContent smoothnessMapChannelText = EditorGUIUtility.TrTextContent("Source", "Specifies where to sample a smoothness map from. By default, uses the alpha channel for your map.");

        public static GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "When enabled, the Material reflects the shine from direct lighting.");

        public static GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Environment Reflections", "When enabled, the Material samples reflections from the nearest Reflection Probes or Lighting Probe.");

        public static GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map", "Defines a Height Map that will drive a parallax effect in the shader making the surface seem displaced.");

        public static GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion Map", "Sets an occlusion map to simulate shadowing from ambient lighting.");

        public static readonly string[] metallicSmoothnessChannelNames = new string[2]
        {
                "Metallic Alpha",
                "Albedo Alpha"
        };

        public static readonly string[] specularSmoothnessChannelNames = new string[2]
        {
                "Specular Alpha",
                "Albedo Alpha"
        };

        public static GUIContent clearCoatText = EditorGUIUtility.TrTextContent("Clear Coat", "A multi-layer material feature which simulates a thin layer of coating on top of the surface material.\nPerformance cost is considerable as the specular component is evaluated twice, once per layer.");

        public static GUIContent clearCoatMaskText = EditorGUIUtility.TrTextContent("Mask", "Specifies the amount of the coat blending.\nActs as a multiplier of the clear coat map mask value or as a direct mask value if no map is specified.\nThe map specifies clear coat mask in the red channel and clear coat smoothness in the green channel.");

        public static GUIContent clearCoatSmoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Specifies the smoothness of the coating.\nActs as a multiplier of the clear coat map smoothness value or as a direct smoothness value if no map is specified.");
    }

    public struct LitProperties
    {
        public MaterialProperty workflowMode;

        public MaterialProperty metallic;

        public MaterialProperty specColor;

        public MaterialProperty metallicGlossMap;

        public MaterialProperty specGlossMap;

        public MaterialProperty smoothness;

        public MaterialProperty smoothnessGlossMap;

        public MaterialProperty smoothnessMapChannel;

        public MaterialProperty bumpMapProp;

        public MaterialProperty bumpScaleProp;

        public MaterialProperty parallaxMapProp;

        public MaterialProperty parallaxScaleProp;

        public MaterialProperty occlusionStrength;

        public MaterialProperty occlusionMap;

        public MaterialProperty highlights;

        public MaterialProperty reflections;

        public MaterialProperty clearCoat;

        public MaterialProperty clearCoatMap;

        public MaterialProperty clearCoatMask;

        public MaterialProperty clearCoatSmoothness;

        public LitProperties(MaterialProperty[] properties)
        {
            workflowMode = BaseShaderGUI.FindProperty("_WorkflowMode", properties, propertyIsMandatory: false);
            metallic = BaseShaderGUI.FindProperty("_Metallic", properties);
            specColor = BaseShaderGUI.FindProperty("_SpecColor", properties, propertyIsMandatory: false);
            metallicGlossMap = BaseShaderGUI.FindProperty("_MetallicGlossMap", properties);
            specGlossMap = BaseShaderGUI.FindProperty("_SpecGlossMap", properties, propertyIsMandatory: false);
            smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties, propertyIsMandatory: false);
            smoothnessGlossMap = BaseShaderGUI.FindProperty("_SmoothnessGlossMap", properties, propertyIsMandatory: false);
            smoothnessMapChannel = BaseShaderGUI.FindProperty("_SmoothnessTextureChannel", properties, propertyIsMandatory: false);
            bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties, propertyIsMandatory: false);
            bumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties, propertyIsMandatory: false);
            parallaxMapProp = BaseShaderGUI.FindProperty("_ParallaxMap", properties, propertyIsMandatory: false);
            parallaxScaleProp = BaseShaderGUI.FindProperty("_Parallax", properties, propertyIsMandatory: false);
            occlusionStrength = BaseShaderGUI.FindProperty("_OcclusionStrength", properties, propertyIsMandatory: false);
            occlusionMap = BaseShaderGUI.FindProperty("_OcclusionMap", properties, propertyIsMandatory: false);
            highlights = BaseShaderGUI.FindProperty("_SpecularHighlights", properties, propertyIsMandatory: false);
            reflections = BaseShaderGUI.FindProperty("_EnvironmentReflections", properties, propertyIsMandatory: false);
            clearCoat = BaseShaderGUI.FindProperty("_ClearCoat", properties, propertyIsMandatory: false);
            clearCoatMap = BaseShaderGUI.FindProperty("_ClearCoatMap", properties, propertyIsMandatory: false);
            clearCoatMask = BaseShaderGUI.FindProperty("_ClearCoatMask", properties, propertyIsMandatory: false);
            clearCoatSmoothness = BaseShaderGUI.FindProperty("_ClearCoatSmoothness", properties, propertyIsMandatory: false);
        }
    }

    public static void Inputs(LitProperties properties, MaterialEditor materialEditor, Material material)
    {
        DoMetallicSpecularArea(properties, materialEditor, material);
        BaseShaderGUI.DrawNormalArea(materialEditor, properties.bumpMapProp, properties.bumpScaleProp);
        if (HeightmapAvailable(material))
        {
            DoHeightmapArea(properties, materialEditor);
        }

        if (properties.occlusionMap != null)
        {
            materialEditor.TexturePropertySingleLine(Styles.occlusionText, properties.occlusionMap, (properties.occlusionMap.textureValue != null) ? properties.occlusionStrength : null);
        }

        if (ClearCoatAvailable(material))
        {
            DoClearCoat(properties, materialEditor, material);
        }
    }

    private static bool ClearCoatAvailable(Material material)
    {
        return material.HasProperty("_ClearCoat") && material.HasProperty("_ClearCoatMap") && material.HasProperty("_ClearCoatMask") && material.HasProperty("_ClearCoatSmoothness");
    }

    private static bool HeightmapAvailable(Material material)
    {
        return material.HasProperty("_Parallax") && material.HasProperty("_ParallaxMap");
    }

    private static void DoHeightmapArea(LitProperties properties, MaterialEditor materialEditor)
    {
        materialEditor.TexturePropertySingleLine(Styles.heightMapText, properties.parallaxMapProp, (properties.parallaxMapProp.textureValue != null) ? properties.parallaxScaleProp : null);
    }

    private static bool ClearCoatEnabled(Material material)
    {
        return material.HasProperty("_ClearCoat") && (double)material.GetFloat("_ClearCoat") > 0.0;
    }

    public static void DoClearCoat(LitProperties properties, MaterialEditor materialEditor, Material material)
    {
        materialEditor.ShaderProperty(properties.clearCoat, Styles.clearCoatText);
        bool flag = (double)material.GetFloat("_ClearCoat") > 0.0;
        EditorGUI.BeginDisabledGroup(!flag);
        EditorGUI.indentLevel += 2;
        materialEditor.TexturePropertySingleLine(Styles.clearCoatMaskText, properties.clearCoatMap, properties.clearCoatMask);
        materialEditor.ShaderProperty(properties.clearCoatSmoothness, Styles.clearCoatSmoothnessText);
        EditorGUI.indentLevel -= 2;
        EditorGUI.EndDisabledGroup();
    }

    public static void DoMetallicSpecularArea(LitProperties properties, MaterialEditor materialEditor, Material material)
    {
        string[] smoothnessChannelNames;
        if (properties.workflowMode == null || (int)properties.workflowMode.floatValue == 1)
        {
            bool flagSpecularMap = properties.metallicGlossMap.textureValue != null;
            bool flagSmoothnessMap = properties.smoothnessGlossMap.textureValue != null;
            smoothnessChannelNames = Styles.metallicSmoothnessChannelNames;
            materialEditor.TexturePropertySingleLine(Styles.metallicMapText, properties.metallicGlossMap, flagSpecularMap ? null : properties.metallic);
            materialEditor.TexturePropertySingleLine(Styles.smoothnessMapText, properties.smoothnessGlossMap, flagSmoothnessMap ? null : properties.smoothness);
        }
        else
        {
            bool flagSpecularMap = properties.specGlossMap.textureValue != null;
            bool flagSmoothnessMap = properties.smoothnessGlossMap.textureValue != null;
            smoothnessChannelNames = Styles.specularSmoothnessChannelNames;
            BaseShaderGUI.TextureColorProps(materialEditor, Styles.specularMapText, properties.specGlossMap, flagSpecularMap ? null : properties.specColor);
            BaseShaderGUI.TextureColorProps(materialEditor, Styles.smoothnessMapText, properties.smoothnessGlossMap, flagSmoothnessMap ? null : properties.smoothness);
        }

        DoSmoothnessMapChannel(material, properties.smoothnessMapChannel, smoothnessChannelNames);
    }

    internal static bool IsOpaque(Material material)
    {
        bool result = true;
        if (material.HasProperty(Property.SurfaceType))
        {
            result = ((int)material.GetFloat(Property.SurfaceType) == 0);
        }

        return result;
    }

    public static void DoSmoothnessMapChannel(Material material, MaterialProperty smoothnessMapChannel, string[] smoothnessChannelNames)
    {
        EditorGUI.indentLevel += 2;
        if (smoothnessMapChannel != null)
        {
            bool flag = IsOpaque(material);
            EditorGUI.indentLevel++;
            EditorGUI.showMixedValue = smoothnessMapChannel.hasMixedValue;
            if (flag)
            {
                MaterialEditor.BeginProperty(smoothnessMapChannel);
                EditorGUI.BeginChangeCheck();
                int selectedIndex = (int)smoothnessMapChannel.floatValue;
                selectedIndex = EditorGUILayout.Popup(Styles.smoothnessMapChannelText, selectedIndex, smoothnessChannelNames);
                if (EditorGUI.EndChangeCheck())
                {
                    smoothnessMapChannel.floatValue = selectedIndex;
                }

                MaterialEditor.EndProperty();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(disabled: true);
                EditorGUILayout.Popup(Styles.smoothnessMapChannelText, 0, smoothnessChannelNames);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel -= 2;
    }

    public static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
    {
        int num = (int)material.GetFloat("_SmoothnessTextureChannel");
        if (num == 1)
        {
            return SmoothnessMapChannel.AlbedoAlpha;
        }

        return SmoothnessMapChannel.SpecularMetallicAlpha;
    }

    internal static void SetupSpecularWorkflowKeyword(Material material, out bool isSpecularWorkflow)
    {
        isSpecularWorkflow = false;
        if (material.HasProperty(Property.SpecularWorkflowMode))
        {
            isSpecularWorkflow = ((int)material.GetFloat(Property.SpecularWorkflowMode) == 0);
        }

        CoreUtils.SetKeyword(material, "_SPECULAR_SETUP", isSpecularWorkflow);
    }

    public static void SetMaterialKeywords(Material material)
    {
        SetupSpecularWorkflowKeyword(material, out bool isSpecularWorkflow);
        string name = isSpecularWorkflow ? "_SpecGlossMap" : "_MetallicGlossMap";
        bool state = material.GetTexture(name) != null;
        CoreUtils.SetKeyword(material, "_METALLICSPECGLOSSMAP", state);
        if (material.HasProperty("_SpecularHighlights"))
        {
            CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF", material.GetFloat("_SpecularHighlights") == 0f);
        }

        if (material.HasProperty("_EnvironmentReflections"))
        {
            CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF", material.GetFloat("_EnvironmentReflections") == 0f);
        }

        if (material.HasProperty("_OcclusionMap"))
        {
            CoreUtils.SetKeyword(material, "_OCCLUSIONMAP", material.GetTexture("_OcclusionMap"));
        }

        if (material.HasProperty("_ParallaxMap"))
        {
            CoreUtils.SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));
        }

        if (material.HasProperty("_SmoothnessTextureChannel"))
        {
            bool flag = IsOpaque(material);
            CoreUtils.SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha && flag);
        }

        if (ClearCoatEnabled(material))
        {
            if (material.HasProperty("_ClearCoatMap") && material.GetTexture("_ClearCoatMap") != null)
            {
                CoreUtils.SetKeyword(material, "_CLEARCOAT", state: false);
                CoreUtils.SetKeyword(material, "_CLEARCOATMAP", state: true);
            }
            else
            {
                CoreUtils.SetKeyword(material, "_CLEARCOAT", state: true);
                CoreUtils.SetKeyword(material, "_CLEARCOATMAP", state: false);
            }
        }
        else
        {
            CoreUtils.SetKeyword(material, "_CLEARCOAT", state: false);
            CoreUtils.SetKeyword(material, "_CLEARCOATMAP", state: false);
        }

        CoreUtils.SetKeyword(material, "_SMOOTHNESSGLOSSMAP", state);
    }
}
