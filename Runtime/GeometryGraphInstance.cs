using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GeometryGraphInstance : MonoBehaviour
{
    //[SerializeField] private GeometryGraphAsset _geometryGraph;
    [SerializeField] private List<GeometryGraphPropertyOverwrite> _overwrites;

    //public GeometryGraphAsset Graph => _geometryGraph;
    public List<GeometryGraphPropertyOverwrite> Overwrites => _overwrites;


    public List<GeometryGraphPropertyOverwrite> GetOverwrites()
    {
        return Overwrites;
    }

    public void SetOverwrites(List<GeometryGraphPropertyOverwrite> newOverwrites)
    {
        _overwrites = newOverwrites;
    }
}