using System;
using System.Runtime.InteropServices;
using henningboat.CubeMarching.Runtime.BinaryAssets;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryComponents.Shapes.henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.Runtime.Utils.Containers;
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

        public void WriteShape(in GeometryInstructionIterator iterator, in BinaryDataStorage assetDataStorage,
            in GeometryInstruction geometryInstruction)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (ShapeType)
                {
                    case ShapeType.Sphere:
                        ((SphereShape*) ptr)->WriteShape(iterator, in assetDataStorage, in geometryInstruction);
                        break;
                    case ShapeType.BoundingBox:
                        ((CShapeBoundingBox*) ptr)->WriteShape(iterator, in assetDataStorage, in geometryInstruction);
                        break;
                    case ShapeType.Torus:
                        ((TorusShape*) ptr)->WriteShape(iterator, in assetDataStorage, in geometryInstruction);
                        break;
                    case ShapeType.Voronoi:
                        ((VoronoiShape*) ptr)->WriteShape(iterator, in assetDataStorage,
                            in geometryInstruction);
                        break;
                    case ShapeType.Plane:
                        ((PlaneShape*) ptr)->WriteShape(iterator, in assetDataStorage, in geometryInstruction);
                        break;
                    case ShapeType.Box:
                        ((BoxShape*) ptr)->WriteShape(iterator, in assetDataStorage, in geometryInstruction);
                        break;
                    case ShapeType.Ray:
                        ((RayShape*) ptr)->WriteShape(iterator, in assetDataStorage, in geometryInstruction);
                        break;
                    case ShapeType.SDF2D:
                        ((SDF2DShape*) ptr)->WriteShape(iterator, in assetDataStorage, in geometryInstruction);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}