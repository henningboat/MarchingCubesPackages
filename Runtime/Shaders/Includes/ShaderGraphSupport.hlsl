#include "Packages/com.henningboat.marchingcubes/Runtime/Shaders/Includes/DistanceFieldTriangulation.hlsl"

StructuredBuffer<PackedTriangle> _TriangleIndeces;

void GetVertexData_float(in int vertexID, out float3 positionWS, out float3 normalWS, out float3 color, out float occlusion)
{
    PackedTriangle currentTriangle = _TriangleIndeces[(vertexID/3)];
    GetVertexDataFromPackedVertex(currentTriangle, 2 - vertexID%3, positionWS, normalWS, color, occlusion);
}