using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes.henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using SIMDMath;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Shapes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericGeometryShape
    {
        #region Public Fields

        public float32 Data;
        public ShapeType ShapeType;

        #endregion

        #region Public methods

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, AssetDataStorage assetDataStorage, GeometryInstruction geometryInstruction)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (ShapeType)
                {
                    case ShapeType.Sphere:
                        return ((SphereShape*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    case ShapeType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    case ShapeType.Torus:
                        return ((TorusShape*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    case ShapeType.Voronoi:
                        return ((VoronoiShape*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    case ShapeType.Plane:
                        return ((PlaneShape*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    case ShapeType.Box:
                        return ((BoxShape*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    case ShapeType.Ray:
                        return ((RayShape*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    case ShapeType.SDF2D:
                        return ((SDF2DShape*) ptr)->GetSurfaceDistance(positionOS, assetDataStorage, geometryInstruction);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}