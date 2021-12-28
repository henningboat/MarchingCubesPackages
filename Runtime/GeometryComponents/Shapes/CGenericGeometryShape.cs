using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.GeometryComponents.Shapes;
using henningboat.CubeMarching.GeometryComponents.Shapes.henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.Utils.Containers;
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
                        return ((SphereShape*) ptr)->GetSurfaceDistance(positionOS);
                    case ShapeType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->GetSurfaceDistance(positionOS);
                    case ShapeType.Torus:
                        return ((TorusShape*) ptr)->GetSurfaceDistance(positionOS);
                    case ShapeType.Voronoi:
                        return ((VoronoiShape*) ptr)->GetSurfaceDistance(positionOS);
                    case ShapeType.Plane:
                        return ((PlaneShape*) ptr)->GetSurfaceDistance(positionOS);
                    case ShapeType.Box:
                        return ((BoxShape*) ptr)->GetSurfaceDistance(positionOS);
                    case ShapeType.Ray:
                        return ((RayShape*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}