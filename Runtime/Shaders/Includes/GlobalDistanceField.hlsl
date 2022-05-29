//
//
// float GetSurfaceDistanceInterpolated(float3 positionWS)
// {
//     int3 id = floor(positionWS);
//
//
//     const float3 positinWithinCube = frac(positionWS);
//
//     const float4 bottonPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y, id.z)), GetPointPosition(int3(id.x + 1, id.y, id.z)), positinWithinCube.x),
//                                     lerp(GetPointPosition(int3(id.x, id.y, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y, id.z + 1)), positinWithinCube.x), positinWithinCube.z);
//
//     const float4 topPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y + 1, id.z)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z)), positinWithinCube.x),
//                                  lerp(GetPointPosition(int3(id.x, id.y + 1, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z + 1)), positinWithinCube.x), positinWithinCube.z);
//
//     const float4 result = lerp(bottonPlane, topPlane, positinWithinCube.y);
//     return result.w;
// }
//
// float rand(float3 co)
// {
//     return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 22.2830))) * 43758.5453);
// }
