// Created by 月北(ybwork) https://github.com/ybwork-cn/

Shader "ybwork/GPUSkinningShader/Cartoon-OutLine"
{
    Properties
    {
        _BoneMap("Bone Map", 2D) = "white" {}
        _BindposMap("Bindpos Map", 2D) = "white" {}
        _AnimInfosMap("Anim Infos Map", 2D) = "white" {}

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
    }
}
