// Created by 月北(ybwork) https://github.com/ybwork-cn/

#ifndef UNIVERSAL_LIT_INPUT_INCLUDED
#define UNIVERSAL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

CBUFFER_START(UnityPerMaterial)
sampler2D _BoneMap;
float4 _BoneMap_TexelSize;
sampler2D _BindposMap;
float4 _BindposMap_TexelSize;
sampler2D _AnimInfosMap;
float4 _AnimInfosMap_TexelSize;

float4 _BaseColor;
float4 _BaseMap_ST;
float4 _ShadowColor;
float4 _SpecularMap_ST;
float _EdgeThickness = 1.0;
float _EdgeVertexControlThickness;
float4 _EdgeColor;

float _FalloffScale;
float _RampScale;

float _RimLightScale;
float _NormalScale;
float _SpecularPower;
float _SpecularMultiply;
float _Cutoff;

float4 _MakeupMap_ST;
float _MakeupRangeSize;
float _MakeupRateofChange;

float _DecalScale1;
float4 _DecalColor1;
float _DecalsMakeupScale1;
float4 _DecalsMakeupColor1;
float _MakeupPower1;
float _MakeupLimit1;

float _InterferencePower;
float _InterferenceScale;

float _AdditionlightsMode;

float4 _ChannelMask;
float _Alpha;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(Props)
// put more per - instance properties here
UNITY_DEFINE_INSTANCED_PROP(int, _Loop)
UNITY_DEFINE_INSTANCED_PROP(int, _AnimIndex)
UNITY_DEFINE_INSTANCED_PROP(float, _CurrentTime)
UNITY_DEFINE_INSTANCED_PROP(int, _LastAnimLoop)
UNITY_DEFINE_INSTANCED_PROP(int, _LastAnimIndex)
UNITY_DEFINE_INSTANCED_PROP(float, _LastAnimExitTime)
UNITY_INSTANCING_BUFFER_END(Props)

#endif
