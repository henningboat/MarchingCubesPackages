
int GetSubChunkIndexFromPosition(int3 positionWS)
{
    const int3 positionInCluster = positionWS % 64;
    const int chunkIndexInCluster = indexFromCoordAndGridSize(positionInCluster/8,8);
    const int subChunkIndex = indexFromCoordAndGridSize((positionInCluster % 8)/4,2);

    const int subChunkIndexInCluster = chunkIndexInCluster * 8 + subChunkIndex;
    return subChunkIndexInCluster;
}




float GetSurfaceDistanceInterpolated(float3 positionWS)
{
    int3 id = floor(positionWS);


    const float3 positinWithinCube = frac(positionWS);

    const float4 bottonPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y, id.z)), GetPointPosition(int3(id.x + 1, id.y, id.z)), positinWithinCube.x),
                                    lerp(GetPointPosition(int3(id.x, id.y, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y, id.z + 1)), positinWithinCube.x), positinWithinCube.z);

    const float4 topPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y + 1, id.z)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z)), positinWithinCube.x),
                                 lerp(GetPointPosition(int3(id.x, id.y + 1, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z + 1)), positinWithinCube.x), positinWithinCube.z);

    const float4 result = lerp(bottonPlane, topPlane, positinWithinCube.y);
    return result.w;
}

float rand(float3 co)
{
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 22.2830))) * 43758.5453);
}

// float RayMarchThickness(float3 positionWS, float3 viewDir)
// {
//     viewDir += rand(positionWS) * 0.1f;
//     float3 origin = positionWS;
//     positionWS += viewDir;
//
//
//     float maxDistance = 0;
//     float totalThickness = 0;
//
//     //first travel inside the object
//
//     float surfaceDistance = 0;
//     bool reachedOutside = false;
//
//     for (int i = 0; i < 30; i++)
//     {
//         surfaceDistance = GetSurfaceDistanceInterpolated(positionWS);
//         if (surfaceDistance > 0)
//         {
//             viewDir = 0;
//             reachedOutside = true;
//         }
//         else
//         {
//             totalThickness++;
//         }
//         viewDir += rand((positionWS) + i) * 0.01f;
//         positionWS += viewDir * 0.5;
//     }
//
//     if (reachedOutside == false)
//     {
//         return 1;
//     }
//
//     float3 sunDirection = float3(0, 1, 0);
//     positionWS += sunDirection;
//
//     bool hitSomething = false;
//
//     for (int i = 0; i < 30; i++)
//     {
//         surfaceDistance = GetSurfaceDistanceInterpolated(positionWS);
//         if (surfaceDistance < 0)
//         {
//             hitSomething = true;
//         }
//         positionWS += sunDirection;
//     }
//
//     if (hitSomething)
//     {
//         return 1;
//     }
//
//     //from the other side of the object
//     return 1;
//     return totalThickness / 30;
// }
