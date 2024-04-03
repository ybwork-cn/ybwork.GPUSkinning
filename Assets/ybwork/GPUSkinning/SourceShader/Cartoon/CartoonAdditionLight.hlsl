TEXTURE2D(_GlobalAdditionLightsRampMap); SAMPLER(sampler_GlobalAdditionLightsRampMap);

half CartoonAdditionalLightRealtimeShadow(int lightIndex, float3 positionWS, half3 lightDirection)
{
    return 0;
}

float3 CatroonAdditionLighting(Light light, float3 normalWS, float3 viewDir, float3 albedo, float gloss, float highLightMul, float4 specularMask, float RampScale, float AdditionlightsMode,float3 shadowColor)
{
    float3 AdditionLightingColor = float3(0, 0, 0);
    //diffuse
    float NdotL = dot(normalWS, light.direction);
    float diff = max(0.001, NdotL)* light.shadowAttenuation;
    float4 rampSamplerColor = RampScale * SAMPLE_TEXTURE2D(_GlobalAdditionLightsRampMap, sampler_GlobalAdditionLightsRampMap, float2(diff, 0.25f));
    AdditionLightingColor = lerp(lerp(albedo, rampSamplerColor.rgb * albedo, rampSamplerColor.a), albedo * diff, AdditionlightsMode);

    //specular
    float3 halfDir = normalize(viewDir + light.direction);
    float3 specularColor = saturate(pow(max(0, dot(halfDir, normalWS)), gloss)) * specularMask.rgb * albedo;
    AdditionLightingColor += specularColor * highLightMul * specularMask.a;

    AdditionLightingColor = lerp(shadowColor * AdditionLightingColor, AdditionLightingColor, light.shadowAttenuation) *step(AdditionlightsMode, 0.5) + AdditionLightingColor * step(0.5, AdditionlightsMode);

    return  AdditionLightingColor* light.color* light.distanceAttenuation;
}

float3 CalculateCartoonAdditionLights(float3 positionWS, float3 normalWS, float3 viewDir, float3 albedo, float gloss, float highLightMul, float4 specularMask, float RampScale, float AdditionlightsMode, float3 shadowColor)
{
    float3 finalColor = float3(0, 0, 0);
    // uint meshRenderingLayers = GetMeshRenderingLightLayer();

    for (uint lightIndex = 0; lightIndex < min(GetAdditionalLightsCount(), MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);

        // if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            //light.shadowAttenuation = CartoonAdditionalLightRealtimeShadow(lightIndex, positionWS, light.direction);
            finalColor += CatroonAdditionLighting(light, normalWS, viewDir, albedo, gloss, highLightMul, specularMask, RampScale, AdditionlightsMode, shadowColor);
        }
    }
    return finalColor;
}
