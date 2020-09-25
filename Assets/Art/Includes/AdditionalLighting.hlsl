void AdditionalLights_float(float3 WorldPosition, float3 WorldNormal, out float3 CombinedDistAtten)
{
	float3 combinedDistAtten = 0;
	float3 Color = 0;
	float DistanceAtten = 0;

#ifndef SHADERGRAPH_PREVIEW
    WorldNormal = normalize(WorldNormal);
    int pixelLightCount = GetAdditionalLightsCount();

    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
       
	   Color = light.color;
	   DistanceAtten = light.distanceAttenuation ;
	   combinedDistAtten += DistanceAtten * Color;
    }
#endif

	CombinedDistAtten = combinedDistAtten;
}
