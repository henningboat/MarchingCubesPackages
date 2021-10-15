using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class GeometryGraphInstance : MonoBehaviour
{
    [SerializeField] private GeometryGraphRuntimeData _geometryGraphRuntimeData;
    [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;

    public GeometryGraphRuntimeData GraphData => _geometryGraphRuntimeData;
    public List<GeometryGraphPropertyOverwrite> Overwrites => _overwrites;


    public List<GeometryGraphPropertyOverwrite> GetOverwrites()
    {
        return Overwrites;
    }

    public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites) 
    {
        _overwrites = newOverwrites;
    }

    private void OnEnable()
    {
        _geometryGraphRuntimeData.AllocateNativeArrays();
    }

    private void OnDestroy()
    {
        
    }
}