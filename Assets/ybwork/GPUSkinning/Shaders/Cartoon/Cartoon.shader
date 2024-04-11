// Created by 月北(ybwork) https://github.com/ybwork-cn/

Shader "ybwork/GPUSkinningShader/Cartoon"
{
    Properties
    {
        _BoneMap("Bone Map", 2D) = "white" {}
        _BindposMap("Bindpos Map", 2D) = "white" {}
        _AnimInfosMap("Anim Infos Map", 2D) = "white" {}

        [ToggleUI] _Loop("Loop", Float) = 0
        _AnimIndex("Anim Index", Int) = 0
        _CurrentTime("Current Time", Float) = 0

        [ToggleUI] _LastAnimLoop("Last Anim Loop", Float) = 0
        _LastAnimIndex("Last Anim Index", Int) = 0
        _LastAnimExitTime("Last Anim Exit Time", Float) = 0

        [Space(30)]
        _ChannelMask("Channel Mask(0-255)", vector) = (0, 0, 0, 0)
        [Enum(OFF, 0, FRONT, 1, BACK, 2)] _CullMode("Cull Mode", int) = 0// OFF/FRONT/BACK
        [Enum(NO, 0, OFF, 1)] _Alpha("Use Alpha", int) = 0
        [HideInInspector]_StencilNo("Stencil No", Float) = 1
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_StencilOpPass("Stencil Operation", Float) = 0
        [HideInInspector]_StencilOpFail("Stencil Operation", Float) = 0
        // Base Color
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap("Base Map", 2D) = "white" {}
        // Shadow Color
        _ShadowColor("Shadow Color", Color) = (0.8, 0.8, 0.8, 1)
        // Outline
        _EdgeThickness("Outline Thickness", Float) = 1
        _EdgeVertexControlThickness("Outline Vertex Control Thickness", Range(0, 1)) = 0
        _EdgeColor("Outline Color", Color) = (0, 0, 0, 0)
        // FallOff Light
        _FalloffMap("Falloff Map", 2D) = "black" {}
        _FalloffScale("Falloff Scale", Float) = 1
        // Ramp
        _RampMap("Ramp Map", 2D) = "black" {}
        _RampScale("Ramp Scale", Float) = 1
        // Rim Light
        _RimLightMap("RimLight Map", 2D) = "black" {}
        _RimLightScale("RimeLight Scale", Float) = 0.5
        // Normal
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Float) = 1
        // Specular
        _SpecularMap("Specular Map", 2D) = "black" {}
        _SpecularPower("Specular Power", Float) = 1
        _SpecularMultiply("Specular Multiply", Range(0, 100)) = 1
        // Cutoff
        _Cutoff("Alpha Cutoff", Float) = 0.5
        // Decals Makeup Map
        _MakeupMap("Makeup Map", 2D) = "black" {}
        // Decals
        _DecalMap1("Decal Map 1", 2D) = "black" {}
        _DecalScale1("Decal Alpha 1", Range(0, 1)) = 1
        _DecalColor1("Decal Color 1", Color) = (1, 1, 1, 1)
        // MakeupRange
        _MakeupRangeSize("MakeupRange Size", Range(1, 20)) = 2
        _MakeupRateofChange("Makeup Rate of Change", Range(0, 1)) = 0.5

        // Interference
        _InterferencePower("Interference Power", float) = 3
        _InterferenceScale("Interference Scale", float) = 2

        // AdditionLights
        [Enum(On_Ramp, 0, On_Diff, 1)] _AdditionlightsMode("Addition Lights Mode", Float) = 0

    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        HLSLINCLUDE

        #include "LitInput.hlsl"
        #include "MyInclude.hlsl"

        ENDHLSL

        Pass
        {
            Name "Skin"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            ZWrite On
            Cull[_CullMode]
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref[_StencilNo]
                Comp[_StencilComp]
                Pass[_StencilOpPass]
                Fail[_StencilOpFail]
            }

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 blendWeights : BLENDWEIGHTS;
                uint4 blendIndices : BLENDINDICES;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
                float2 uv2 : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_FalloffMap); SAMPLER(sampler_FalloffMap);
            TEXTURE2D(_RampMap); SAMPLER(sampler_RampMap);
            TEXTURE2D(_RimLightMap); SAMPLER(sampler_RimLightMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_DecalMap1); SAMPLER(sampler_DecalMap1);
            TEXTURE2D(_MakeupMap); SAMPLER(sampler_MakeupMap);
            TEXTURE2D(_SpecularMap); SAMPLER(sampler_SpecularMap);

            #include "CartoonAdditionLight.hlsl"

            // This function was added in URP v9.x.x versions
            // If we want to support URP versions before, we need to handle it instead.
            // Computes the world space view direction (pointing towards the viewer).
            float3 ThisGetWorldSpaceViewDir(float3 positionWS)
            {
                if (unity_OrthoParams.w == 0)
                {
                    // Perspective
                    return normalize(_WorldSpaceCameraPos - positionWS);
                }
                else
                {
                    // Orthographic
                    float4x4 viewMat = GetWorldToViewMatrix();
                    return viewMat[2].xyz;
                }
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                GetPositionAndNormal(IN.positionOS, IN.normalOS, IN.blendIndices, IN.blendWeights);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uv2 = TRANSFORM_TEX(IN.uv2, _BaseMap);
                OUT.color = IN.color;

                OUT.viewDirWS = ThisGetWorldSpaceViewDir(positionInputs.positionWS);

                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz, IN.tangentOS);
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = normalInputs.tangentWS;
                OUT.bitangentWS = normalInputs.bitangentWS;
                return OUT;
            }

            inline float GetColorLumensValue(float3 Color)
            {
                return 0.2126 * Color.r + 0.7152 * Color.g + 0.0722 * Color.b;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                clip(baseMap.a - _Cutoff);
                half4 color = baseMap * _BaseColor;

                // Decal Maps
                half4 decalMap1 = SAMPLE_TEXTURE2D(_DecalMap1, sampler_DecalMap1, IN.uv);
                decalMap1.rgb = decalMap1.rgb * _DecalColor1;
                color.rgb = lerp(color.rgb, decalMap1.rgb, _DecalScale1 * decalMap1.a);

                half3 normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), _NormalScale);
                IN.normalWS = normalize(TransformTangentToWorld(normalTS, half3x3(IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz)));
                // Falloff
                // float3 viewDirWS = GetWorldSpaceViewDir(IN.positionWS);
                float normalDotEye = dot(IN.normalWS, IN.viewDirWS);
                float falloffU = clamp(1 - abs(normalDotEye), 0.02, 0.98);
                half4 falloffSamplerColor = _FalloffScale * SAMPLE_TEXTURE2D(_FalloffMap, sampler_FalloffMap, float2(falloffU, 0.25f));

                // FOR CLOTHES
                half3 shadowColor = color.rgb * color.rgb;
                half3 combinedColor = lerp(color.rgb, shadowColor, falloffSamplerColor.r);
                combinedColor *= (1.0 + falloffSamplerColor.rgb * falloffSamplerColor.a);
                // FOR SKIN

                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS.xyz);
                Light light = GetMainLight(shadowCoord);
                float NdotL = dot(IN.normalWS, light.direction);
                float diff = max(0.01, NdotL);

                // Ramp
                half4 rampSamplerColor = _RampScale * SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(diff, 0.25f));
                // rampSamplerColor.rgb *= _RampScale;
                combinedColor = lerp(combinedColor.rgb, rampSamplerColor.rgb * color.rgb, rampSamplerColor.a);

                // Specular
                // Use the eye vector as the light vector
                half4 reflectionMaskColor = SAMPLE_TEXTURE2D(_SpecularMap, sampler_SpecularMap, IN.uv * _SpecularMap_ST.xy + _SpecularMap_ST.zw);
                float3 halfDir = normalize(IN.viewDirWS + light.direction);
                half3 specularColor = saturate(pow(max(0, dot(halfDir, IN.normalWS)), _SpecularPower)) * reflectionMaskColor.rgb * color;// *fresnel;
                combinedColor += specularColor * _SpecularMultiply * reflectionMaskColor.a;

                // Interference
                float interference = pow(max(0, dot(halfDir, IN.normalWS)), _InterferencePower);

                // Decals Metallic
                half4 MakeupMap = SAMPLE_TEXTURE2D(_MakeupMap, sampler_MakeupMap, IN.uv * _MakeupMap_ST.xy + _MakeupMap_ST.zw);

                half decalsMetallicColor = saturate(pow(max(0, dot(halfDir, IN.normalWS)), 5)) * MakeupMap.r;
                // half3 decalsMetallicColor1 = decalsMetallicColor * _DecalsMakeupScale1 * _DecalsMakeupColor1  * smoothstep(_MakeupRangeI, _MakeupRangeO, 1 - decalMap1.a) * step(_MakeupRangeO + 0.001, _MakeupRangeI) * _MakeupPower1 * step(_MakeupLimit1, MakeupMap.r) * 2;

                // Set a new Alpha of Decal2 and control Makeup Range Size.
                half newDecalAlpha1 = abs(1.0 / _MakeupRangeSize - decalMap1.a) * step(1.0 / _MakeupRangeSize + 0.001, decalMap1.a);
                newDecalAlpha1 = newDecalAlpha1 / ((1 - 1.0 / _MakeupRangeSize) + 0.001);
                newDecalAlpha1 = clamp(newDecalAlpha1 / 0.25, 0, 1);
                half smoothDecalAlpha1 = smoothstep(0, 1, newDecalAlpha1);
                // Set Decal1 Color
                half3 decalsMetallicColor1 = clamp(decalsMetallicColor * _DecalsMakeupColor1, _DecalsMakeupColor1 * _MakeupRateofChange, _DecalsMakeupColor1) * _MakeupPower1 * step(_MakeupLimit1, MakeupMap.r) * 2 * newDecalAlpha1;
                // lerp Base Color and Decal1 Color
                decalsMetallicColor1 = lerp(combinedColor * step(0.02, GetColorLumensValue(decalsMetallicColor1)), decalsMetallicColor1, pow(smoothDecalAlpha1, 4) * step(0.02, GetColorLumensValue(decalsMetallicColor1)) * _DecalsMakeupScale1);
                half3 MakeupColor = clamp(decalsMetallicColor1, 0, 1.6);// (decalsMetallicColor1 + decalsMetallicColor2 + decalsMetallicColor3 + decalsMetallicColor4 + decalsMetallicColor5, 0, 1.6);

                // Shadow
                combinedColor = lerp(_ShadowColor.rgb * combinedColor, combinedColor, light.shadowAttenuation);

                // Rimlight
                float rimlightDot = saturate(0.5 * (NdotL + 1.0));
                falloffU = saturate(rimlightDot * falloffU);
                falloffU = SAMPLE_TEXTURE2D(_RimLightMap, sampler_RimLightMap, float2(falloffU, 0.25f)).r;
                float3 rimColor = color.rgb * _RimLightScale;// * 2.0;
                combinedColor += falloffU * rimColor * light.shadowAttenuation;

                combinedColor = combinedColor.rgb * light.color;
                combinedColor = lerp(combinedColor, MakeupColor * light.color, lerp(0, 1, 0.2126 * MakeupColor.r + 0.7152 * MakeupColor.g + 0.0722 * MakeupColor.b));

                float3 AdditionLights = float3(0, 0, 0);
                AdditionLights = CalculateCartoonAdditionLights(IN.positionWS, IN.normalWS, IN.viewDirWS, color.rgb, _SpecularPower, _SpecularMultiply, reflectionMaskColor, _RampScale, _AdditionlightsMode, _ShadowColor.rgb);

                return half4(combinedColor * light.distanceAttenuation + AdditionLights, _Alpha == 0? color.a : 1);
                // return half4(SpecialValueColor(combinedColor * light.distanceAttenuation + AdditionLights,IN), color.a);//(combinedColor.rgb * light.color + (metallicSpecular + metallicSpecular_H+ specular_Ramp) * step(0.5 , _NewMetallic), color.a);
            }

            ENDHLSL
        }

        Pass
        {
            Name "Outline"

            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil
            {
                Ref[_StencilNo]
                Comp[_StencilComp]
                Pass[_StencilOpPass]
                Fail[_StencilOpFail]
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            // Outline thickness multiplier
            #define INV_EDGE_THICKNESS_DIVISOR 0.01
            // Outline color parameters
            #define SATURATION_FACTOR 0.6
            #define BRIGHTNESS_FACTOR 0.8
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
                float4 blendWeights : BLENDWEIGHTS;
                uint4 blendIndices : BLENDINDICES;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // This function was added in URP v9.x.x versions
            // If we want to support URP versions before, we need to handle it instead.
            // Computes the world space view direction (pointing towards the viewer).
            float3 ThisGetWorldSpaceViewDir(float3 positionWS)
            {
                if (unity_OrthoParams.w == 0)
                {
                    // Perspective
                    return normalize(_WorldSpaceCameraPos - positionWS);
                }
                else
                {
                    // Orthographic
                    float4x4 viewMat = GetWorldToViewMatrix();
                    return viewMat[2].xyz;
                }
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
                float aspect = abs(nearUpperRight.y / nearUpperRight.x);

                GetPositionAndNormal(IN.positionOS, IN.normalOS, IN.blendIndices, IN.blendWeights);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                float4 projSpacePos = positionInputs.positionCS;

                // float3 viewDirWS = ThisGetWorldSpaceViewDir(positionInputs.positionWS);
                // VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
                // float sign = 1;// faceforward(1, viewDirWS, normalInputs.normalWS);

                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, IN.normalOS.xyz);
                float3 clipNormal = mul(viewNormal.xyz, (float3x3)UNITY_MATRIX_I_P);
                float2 projectedNormal = normalize(clipNormal.xy);
                projectedNormal.x *= aspect;


                float2 scaledNormal = max(_EdgeVertexControlThickness, IN.color.r) * _EdgeThickness * INV_EDGE_THICKNESS_DIVISOR * projectedNormal;
                OUT.positionCS = projSpacePos + float4(scaledNormal, -0.00001, 0);

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uv2 = TRANSFORM_TEX(IN.uv2, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                clip(baseMap.a - _Cutoff);

                float maxChan = max(max(baseMap.r, baseMap.g), baseMap.b);
                half4 newMapColor = baseMap;

                maxChan -= (1.0 / 255.0);
                float3 lerpVals = saturate((newMapColor.rgb - float3(maxChan, maxChan, maxChan)) * 255.0);
                newMapColor.rgb = lerp(SATURATION_FACTOR * newMapColor.rgb, newMapColor.rgb, lerpVals);

                half3 col = BRIGHTNESS_FACTOR * newMapColor.rgb * baseMap.rgb * _BaseColor;
                col = lerp(col, _EdgeColor.rgb, _EdgeColor.a);
                return half4(col.rgb, baseMap.a * _BaseColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // --------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 blendWeights : BLENDWEIGHTS;
                uint4 blendIndices : BLENDINDICES;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes IN)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(IN);

                GetPositionAndNormal(IN.positionOS, IN.normalOS, IN.blendIndices, IN.blendWeights);

                output.uv = TRANSFORM_TEX(IN.texcoord, _BaseMap);
                output.positionCS = GetShadowPositionHClip(IN);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }

            ENDHLSL
        }
    }
}
