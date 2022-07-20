// using System.Linq;
// using henningboat.CubeMarching.Runtime.GeometrySystems;
// using henningboat.CubeMarching.Runtime.GeometrySystems.MeshGenerationSystem;
// using henningboat.CubeMarching.Runtime.Triangulation;
// using henningboat.CubeMarching.Runtime.Utils;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;
// using UnityEngine.Rendering;
//
// namespace henningboat.CubeMarching.Runtime.Output.GeometryFieldMeshCollider
// {
//     public class GeometryFieldMeshCollider : MonoBehaviour, IGeometryFieldReceiver
//     {
//         [SerializeField] private GeometryLayerAsset _geometryLayer;
//         [SerializeField] private Mesh _mesh;
//         [SerializeField] private bool _runInEditMode;
//
//         private MeshCollider _meshCollider;
//         private bool _needsMeshUpdate;
//         private TriangulationTableData _triangulationTableData;
//         private NativeList<float3x3> _vertexBuffer;
//
//         public void Dispose()
//         {
//             if (Application.isPlaying == false && !_runInEditMode)
//             {
//                 return;
//             }
//             
//             if (_mesh != null) _mesh.DeleteSafe();
//             if (_meshCollider != null) _meshCollider.gameObject.DeleteSafe();
//
//             _triangulationTableData.Dispose();
//         }
//
//         public GeometryLayer RequestedLayer()
//         {
//             return _geometryLayer != null ? _geometryLayer.GeometryLayer : GeometryLayer.OutputLayer;
//         }
//
//         public JobHandle ScheduleJobs(JobHandle dependencies, GeometryFieldData requestedField,
//             NativeList<int> chunkIndexesUpdatedThisFrame)
//         {
//             //todo replace
//             dependencies.Complete();
//             
//             if (Application.isPlaying == false && !_runInEditMode)
//             {
//                 return default;
//             }
//             
//             _needsMeshUpdate = chunkIndexesUpdatedThisFrame.Length > 0;
//
//             if (!_needsMeshUpdate) return default;
//
//             _vertexBuffer = new NativeList<float3x3>(Constants.clusterVolume * 5, Allocator.TempJob);
//
//             var generateVertexVertexBufferJob = new JBuildMeshColliderVertexBuffer(_triangulationTableData,
//                 requestedField, _vertexBuffer.AsParallelWriter());
//
//             JobHandle jobHandle;
//             jobHandle = generateVertexVertexBufferJob.Schedule(Constants.chunksPerCluster, 16, dependencies);
//
//             //todo remove this
//             jobHandle.Complete();
//             return jobHandle;
//         }
//
//         public void Initialize(GeometryFieldData geometryFieldData)
//         {
//             if (Application.isPlaying == false && !_runInEditMode)
//             {
//                 return;
//             }
//             
//             var meshColliderGameObject = new GameObject("MeshCollider");
//             _meshCollider = meshColliderGameObject.AddComponent<MeshCollider>();
//             meshColliderGameObject.transform.SetParent(transform);
//
//             meshColliderGameObject.hideFlags = HideFlags.DontSave;
//
//             _triangulationTableData = new TriangulationTableData(Allocator.Persistent);
//         }
//
//         public void OnJobsFinished(GeometryFieldData geometryFieldData)
//         {
//             if (Application.isPlaying == false && !_runInEditMode)
//             {
//                 return;
//             }
//             
//             if (!_needsMeshUpdate) return;
//
//             if (_mesh != null) _mesh.DeleteSafe();
//
//             _mesh = new Mesh();
//             _mesh.indexFormat = IndexFormat.UInt32;
//             _mesh.vertices = _vertexBuffer.AsArray().Reinterpret<float3>(36).Select(f => (Vector3) f).ToArray();
//
//             var indexBuffer = new int[_vertexBuffer.Length * 3];
//             for (var i = 0; i < indexBuffer.Length; i++) indexBuffer[i] = i;
//
//             _mesh.SetTriangles(indexBuffer, 0);
//             _meshCollider.sharedMesh = _mesh;
//
//             _vertexBuffer.Dispose();
//         }
//     }
//
//     [BurstCompile]
//     internal struct JBuildMeshColliderVertexBuffer : IJobParallelFor
//     {
//         [ReadOnly] private readonly TriangulationTableData _triangulationTableData;
//         [ReadOnly] private GeometryFieldData _geometryFieldData;
//         // ReSharper disable once FieldCanBeMadeReadOnly.Local
//         [WriteOnly] private NativeList<float3x3>.ParallelWriter _vertexBuffer;
//
//         public JBuildMeshColliderVertexBuffer(TriangulationTableData triangulationTableData,
//             GeometryFieldData geometryFieldData,
//             NativeList<float3x3>.ParallelWriter vertexBuffer)
//         {
//             _triangulationTableData = triangulationTableData;
//             _geometryFieldData = geometryFieldData;
//             _vertexBuffer = vertexBuffer;
//         }
//
//         public void Execute(int index)
//         {
//             var marcher = new TriangulationUtils.TriangleMarcher(_geometryFieldData);
//
//             var chunkToUpdate = _geometryFieldData.GetChunk(index);
//             
//             if(!chunkToUpdate.Parameters.HasData)
//                 return;
//
//             for (var i = 0; i < Constants.chunkVolume; i++)
//             {
//                 var positionInChunk = DistanceFieldGeneration.Utils.IndexToPositionWS(i, Constants.chunkLength);
//                 marcher.AddTrianglesToList(_geometryFieldData, chunkToUpdate.Parameters.PositionWS + positionInChunk,
//                     _triangulationTableData, _vertexBuffer);
//             }
//         }
//     }
// }