using Unity.Collections;

namespace henningboat.CubeMarching.Runtime.Triangulation
{
    public struct TriangulationTableData
    {
        [ReadOnly] public NativeArray<int> Edges;
        [ReadOnly] public NativeArray<int> Triangulation;
        [ReadOnly] public NativeArray<int> CornerIndexAFromEdge;
        [ReadOnly] public NativeArray<int> CornerIndexBFromEdge;
        [ReadOnly] public NativeArray<int> VertexCountPerCubeIndex;

        public TriangulationTableData(Allocator allocator)
        {
            Edges = new NativeArray<int>(TriangulationTables.edges.Length, allocator);
            Edges.CopyFrom(TriangulationTables.edges);

            Triangulation = new NativeArray<int>(TriangulationTables.triangulation.Length, allocator);
            //this could be speed up
            for (var i = 0; i < 256; i++)
            for (var j = 0; j < 16; j++)
                Triangulation[i * 16 + j] = TriangulationTables.triangulation[i, j];

            CornerIndexAFromEdge = new NativeArray<int>(TriangulationTables.cornerIndexAFromEdge.Length, allocator);
            CornerIndexAFromEdge.CopyFrom(TriangulationTables.cornerIndexAFromEdge);

            CornerIndexBFromEdge = new NativeArray<int>(TriangulationTables.cornerIndexBFromEdge.Length, allocator);
            CornerIndexBFromEdge.CopyFrom(TriangulationTables.cornerIndexBFromEdge);

            VertexCountPerCubeIndex =
                new NativeArray<int>(TriangulationTables.vertexCountPerCubeIndex.Length, allocator);
            VertexCountPerCubeIndex.CopyFrom(TriangulationTables.vertexCountPerCubeIndex);
        }

        public void Dispose()
        {
            if (Edges.IsCreated) Edges.Dispose();
            if (Triangulation.IsCreated) Triangulation.Dispose();
            if (CornerIndexAFromEdge.IsCreated) CornerIndexAFromEdge.Dispose();
            if (CornerIndexBFromEdge.IsCreated) CornerIndexBFromEdge.Dispose();
            if (VertexCountPerCubeIndex.IsCreated) VertexCountPerCubeIndex.Dispose();
        }
    }
}