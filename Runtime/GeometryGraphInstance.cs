using System.Collections.Generic;
using NonECSImplementation;
using UnityEngine;

[ExecuteInEditMode]
public class GeometryGraphInstance : MonoBehaviour
{
    [SerializeField] private GeometryGraphRuntimeData _geometryGraphRuntimeData;
    [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;
    private GeometryGraphData _graphData;

    internal GeometryGraphData GraphData
    {
        get
        {
            //todo initialize this in a better way
            if (_graphData.ContentHash!=_geometryGraphRuntimeData.ContentHash)
            {
                if (_graphData.GeometryInstructions.IsCreated)
                {
                    _graphData.Dispose();
                }

                _graphData = new GeometryGraphData(_geometryGraphRuntimeData);
            }
            return _graphData;
        }
    }

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
        _graphData = new GeometryGraphData(_geometryGraphRuntimeData);
    }

    private void OnDisable()
    {
        _graphData.Dispose();
    }
}