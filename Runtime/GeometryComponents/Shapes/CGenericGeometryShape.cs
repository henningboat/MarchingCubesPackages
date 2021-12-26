using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.GeometryComponents.Shapes.henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.Utils.Containers;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericGeometryShape
    {
        #region Public Fields

        public float32 Data;
        public ShapeType ShapeType;

        #endregion

        #region Public methods

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (ShapeType)
                {
                    case ShapeType.Sphere:
                        return ((SphereShapeResolver*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    case ShapeType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    case ShapeType.Torus:
                        return ((TorusShapeResolver*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    case ShapeType.Voronoi: 
                        //return ((CShapeNoise*) ptr)->GetSurfaceDistance(positionOS);
                        return ((VoronoiShapeResolver*) ptr)->GetSurfaceDistance(positionOS);
                    case ShapeType.Plane:
                        return ((PlaneShapeResolver*) ptr)->GetSurfaceDistance(positionOS);
                        break; 
                    case ShapeType.Box:
                        return ((BoxShapeResolver*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}