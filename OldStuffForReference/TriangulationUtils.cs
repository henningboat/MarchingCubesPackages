// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using Unity.Mathematics;
//
// namespace henningboat.CubeMarching.Runtime.Triangulation
// {
//     public static unsafe class TriangulationUtils
//     {
//         private const int terrainChunkLength = 8;
//         private const int numPointsPerAxis = terrainChunkLength;
//
//         private const float isoLevel = 0;
//         private static readonly int3 _TerrainMapSize = 8;
//
//
//         private static int indexFromCoordAndGridSize(int3 position, int3 gridSize)
//         {
//             return position.z * gridSize.y * gridSize.x + position.y * gridSize.x + position.x;
//         }
//
//         private static int indexFromCoord(int3 position, int sizeOfSubChunk = 8)
//         {
//             return position.z * sizeOfSubChunk * sizeOfSubChunk + position.y * sizeOfSubChunk + position.x;
//         }
//
//         private static int GetPointPositionInIndexMap(int3 position)
//         {
//             int3 positionInIndexMap = 0;
//             positionInIndexMap.x = position.x / 8;
//             positionInIndexMap.y = position.y / 8;
//             positionInIndexMap.z = position.z / 8;
//             var indexInTerrainIndexMap = indexFromCoordAndGridSize(positionInIndexMap, _TerrainMapSize);
//             return indexInTerrainIndexMap;
//         }
//
//         private static float4 GetPointPosition(int3 position, GeometryFieldData terrain)
//         {
//             //todo placeholder
//             position = math.clamp(position, 0, 64);
//             return new float4(position, terrain.GetSingleDistance(position));
//         }
//
//         private static float3 interpolateVerts(float4 v1, float4 v2)
//         {
//             var t = (isoLevel - v1.w) / (v2.w - v1.w);
//             return v1.xyz + t * (v2.xyz - v1.xyz);
//         }
//
//         public struct TriangleMarcher
//         {
//             [ReadOnly] private readonly GeometryFieldData _geometryFieldData;
//             private bool needsNewTriangle;
//
//             public TriangleMarcher(GeometryFieldData geometryFieldData)
//             {
//                 _geometryFieldData = geometryFieldData;
//                 needsNewTriangle = true;
//             }
//
//             public void AddTrianglesToList(GeometryFieldData geometryFieldData, int3 positionWS,
//                 TriangulationTableData triangulationData, NativeList<float3x3>.ParallelWriter triangleVertexBuffer)
//             {
//                 //todo cache this
//                 var _cubeCorners = (float4*) UnsafeUtility.Malloc(8, 4 * 4, Allocator.Temp);
//
//                 _cubeCorners[0] =
//                     GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z), geometryFieldData);
//                 _cubeCorners[1] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z),
//                     geometryFieldData);
//                 _cubeCorners[2] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z + 1),
//                     geometryFieldData);
//                 _cubeCorners[3] = GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z + 1),
//                     geometryFieldData);
//                 _cubeCorners[4] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z),
//                     geometryFieldData);
//                 _cubeCorners[5] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z),
//                     geometryFieldData);
//                 _cubeCorners[6] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1),
//                     geometryFieldData);
//                 _cubeCorners[7] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z + 1),
//                     geometryFieldData);
//
//                 // Calculate unique index for each cube configuration.
//                 // There are 256 possible values
//                 // A value of 0 means cube is entirely inside surface; 255 entirely outside.
//                 // The value is used to look up the edge table, which indicates which edges of the cube are cut by the isosurface.
//                 var cubeIndex = 0;
//                 if (_cubeCorners[0].w > isoLevel) cubeIndex |= 1;
//
//                 if (_cubeCorners[1].w > isoLevel) cubeIndex |= 2;
//
//                 if (_cubeCorners[2].w > isoLevel) cubeIndex |= 4;
//
//                 if (_cubeCorners[3].w > isoLevel) cubeIndex |= 8;
//
//                 if (_cubeCorners[4].w > isoLevel) cubeIndex |= 16;
//
//                 if (_cubeCorners[5].w > isoLevel) cubeIndex |= 32;
//
//                 if (_cubeCorners[6].w > isoLevel) cubeIndex |= 64;
//
//                 if (_cubeCorners[7].w > isoLevel) cubeIndex |= 128;
//
//                 var triangleCountForCube = triangulationData.VertexCountPerCubeIndex[cubeIndex] / 3;
//
//                 if (triangleCountForCube != 0)
//                     for (var triangleIndex = 0; triangleIndex < triangleCountForCube; triangleIndex++)
//                     {
//                         ref var triangulation = ref triangulationData.Triangulation;
//
//                         ref var cornerIndexAFromEdge = ref triangulationData.CornerIndexAFromEdge;
//                         ref var cornerIndexBFromEdge = ref triangulationData.CornerIndexBFromEdge;
//
//                         // Get indices of corner points A and B for each of the three edges
//                         // of the cube that need to be joined to form the triangle.
//                         var a0 = cornerIndexAFromEdge[triangulation[cubeIndex * 16 + triangleIndex * 3]];
//                         var b0 = cornerIndexBFromEdge[triangulation[cubeIndex * 16 + triangleIndex * 3]];
//
//                         var a1 = cornerIndexAFromEdge[triangulation[cubeIndex * 16 + triangleIndex * 3 + 1]];
//                         var b1 = cornerIndexBFromEdge[triangulation[cubeIndex * 16 + triangleIndex * 3 + 1]];
//
//                         var a2 = cornerIndexAFromEdge[triangulation[cubeIndex * 16 + triangleIndex * 3 + 2]];
//                         var b2 = cornerIndexBFromEdge[triangulation[cubeIndex * 16 + triangleIndex * 3 + 2]];
//
//
//                         var triangle = new float3x3(
//                             interpolateVerts(_cubeCorners[a2], _cubeCorners[b2]),
//                             interpolateVerts(_cubeCorners[a1], _cubeCorners[b1]),
//                             interpolateVerts(_cubeCorners[a0], _cubeCorners[b0]));
//                         triangleVertexBuffer.AddNoResize(triangle);
//                     }
//
//                 UnsafeUtility.Free(_cubeCorners, Allocator.Temp);
//             }
//
//             // private static uint PositionAndTriangleIndexToCollisionKey(int3 positionWS, int triangleIndex)
//             // {
//             //     var key = 0;
//             //     key += positionWS.x;
//             //     key += positionWS.y * 64;
//             //     key += positionWS.z * 64 * 64;
//             //     key *= 10;
//             //     key += triangleIndex / 3;
//             //     return (uint) key;
//             // }
//
//             private static void CollisionKeyToPositionAndTriangleIndex(uint key, out int3 positionWS,
//                 out int triangleIndex)
//             {
//                 triangleIndex = (int) key % 10 * 3;
//                 key /= 10;
//                 positionWS = new int3((int) key % 64, (int) (key / 64) % 64, (int) (key / 4096) % 64);
//             }
//
//             //todo unify code with above
//             // public static void GetTriangleFromColliderKey(uint index, BlobAssetReference<TriangulationTables.TriangulationDataBlobAsset> triangulationData, TerrainCollisionDataAccessor terrain,
//             //     out float3 a, out float3 b, out float3 c)
//             // {
//             //     // uint triangleIndex = index % 3 * 3;
//             //     // int positionIndex = (int)index / 3;
//             //     // var positionWS = new int3(positionIndex % 3, (positionIndex / 8) % 8, (positionIndex / 64) % 8);
//             //
//             //     CollisionKeyToPositionAndTriangleIndex(index, out var positionWS, out var triangleIndex);
//             //
//             //     //todo cache this
//             //     var _cubeCorners = (float4*) UnsafeUtility.Malloc(8, 4 * 4, Allocator.Temp);
//             //
//             //     _cubeCorners[0] = GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z), ref terrain);
//             //     _cubeCorners[1] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z), ref terrain);
//             //     _cubeCorners[2] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z + 1), ref terrain);
//             //     _cubeCorners[3] = GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z + 1), ref terrain);
//             //     _cubeCorners[4] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z), ref terrain);
//             //     _cubeCorners[5] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z), ref terrain);
//             //     _cubeCorners[6] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1), ref terrain);
//             //     _cubeCorners[7] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z + 1), ref terrain);
//             //
//             //     // Calculate unique index for each cube configuration.
//             //     // There are 256 possible values
//             //     // A value of 0 means cube is entirely inside surface; 255 entirely outside.
//             //     // The value is used to look up the edge table, which indicates which edges of the cube are cut by the isosurface.
//             //     var _cubeIndex = 0;
//             //     if (_cubeCorners[0].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 1;
//             //     }
//             //
//             //     if (_cubeCorners[1].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 2;
//             //     }
//             //
//             //     if (_cubeCorners[2].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 4;
//             //     }
//             //
//             //     if (_cubeCorners[3].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 8;
//             //     }
//             //
//             //     if (_cubeCorners[4].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 16;
//             //     }
//             //
//             //     if (_cubeCorners[5].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 32;
//             //     }
//             //
//             //     if (_cubeCorners[6].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 64;
//             //     }
//             //
//             //     if (_cubeCorners[7].w > isoLevel)
//             //     {
//             //         _cubeIndex |= 128;
//             //     }
//             //
//             //     ref var triangulation = ref triangulationData.Value.triangulation;
//             //
//             //
//             //     ref var cornerIndexAFromEdge = ref triangulationData.Value.cornerIndexAFromEdge;
//             //     ref var cornerIndexBFromEdge = ref triangulationData.Value.cornerIndexBFromEdge;
//             //
//             //     // Get indices of corner points A and B for each of the three edges
//             //     // of the cube that need to be joined to form the triangle.
//             //     var firstEdge = triangulation[_cubeIndex * 16 + triangleIndex];
//             //     var a0 = cornerIndexAFromEdge[firstEdge];
//             //     var b0 = cornerIndexBFromEdge[firstEdge];
//             //
//             //     var a1 = cornerIndexAFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 1]];
//             //     var b1 = cornerIndexBFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 1]];
//             //
//             //     var a2 = cornerIndexAFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 2]];
//             //     var b2 = cornerIndexBFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 2]];
//             //
//             //
//             //     a = interpolateVerts(_cubeCorners[a0], _cubeCorners[b0]);
//             //     b = interpolateVerts(_cubeCorners[a1], _cubeCorners[b1]);
//             //     c = interpolateVerts(_cubeCorners[a2], _cubeCorners[b2]);
//             //
//             //     UnsafeUtility.Free(_cubeCorners, Allocator.Temp);
//             // }
//         }
//     }
// }