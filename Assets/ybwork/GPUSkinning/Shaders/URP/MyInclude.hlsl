// // Created by 月北(ybwork) https://github.com/ybwork-cn/

// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// float GetLoopTime(float time, float duration)
// {
//     return frac(time / duration);
// }

// float GetClampTime(float time, float duration)
// {
//     return clamp(time / duration, 0, 1);
// }

void GetPositionAndNormal(inout float4 positionOS, inout float3 normalOS, uint4 blendIndices, float4 blendWeights)
{
    // 将顶点从对象空间变换到骨骼空间
    float4x4 matrix0 = mul(_Bones[blendIndices[0]], _Bindposes[blendIndices[0]]);
    float4x4 matrix1 = mul(_Bones[blendIndices[1]], _Bindposes[blendIndices[1]]);
    float4x4 matrix2 = mul(_Bones[blendIndices[2]], _Bindposes[blendIndices[2]]);
    float4x4 matrix3 = mul(_Bones[blendIndices[3]], _Bindposes[blendIndices[3]]);
    float4 p0 = mul(_RootTransform, mul(matrix0, positionOS)) * blendWeights.x;
    float4 p1 = mul(_RootTransform, mul(matrix1, positionOS)) * blendWeights.y;
    float4 p2 = mul(_RootTransform, mul(matrix2, positionOS)) * blendWeights.z;
    float4 p3 = mul(_RootTransform, mul(matrix3, positionOS)) * blendWeights.w;
    positionOS = p0 + p1 + p2 + p3;

    float3 n0 = mul(_RootTransform, mul((float3x3)matrix0, normalOS)) * blendWeights.x;
    float3 n1 = mul(_RootTransform, mul((float3x3)matrix1, normalOS)) * blendWeights.y;
    float3 n2 = mul(_RootTransform, mul((float3x3)matrix2, normalOS)) * blendWeights.z;
    float3 n3 = mul(_RootTransform, mul((float3x3)matrix3, normalOS)) * blendWeights.w;
    normalOS = n0 + n1 + n2 + n3;
}

// float4 GetNormal(uint vid)
// {
//     float t = UNITY_ACCESS_INSTANCED_PROP(Props, _CurrentTime);
//     if(_Loop)
//         t = GetLoopTime(t, _AnimLen);
//     else
//         t = GetClampTime(t, _AnimLen);
//     float animMapNormal_x = (vid + 0.5) * _AnimMapNormal_TexelSize.x;
//     float animMapNormal_y = t;
//     return tex2Dlod(_AnimMapNormal, float4(animMapNormal_x, animMapNormal_y, 0, 0));
// }
