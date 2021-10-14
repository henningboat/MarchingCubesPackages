using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace NonECSImplementation
{
    [ExecuteInEditMode]
    public class GeometryFieldHolder:MonoBehaviour
    {
        public GeometryFieldHolder Instance => FindObjectOfType<GeometryFieldHolder>();

        private GeometryFieldData _data;
        
        public void Awake()
        {
            _data = new GeometryFieldData();
            _data.Initialize(1);
        }

        public void Update()
        {
            //var geometryGraphInstances = FindObjectsOfType<GeometryGraphInstance>();

            _data.AddRandomVoxel();
        }
    }
}