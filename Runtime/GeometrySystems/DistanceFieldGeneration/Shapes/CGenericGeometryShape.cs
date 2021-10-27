﻿using System;
using System.Runtime.InteropServices;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace henningboat.CubeMarching.GeometrySystems.DistanceFieldGeneration.Shapes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericGeometryShape
    {
        #region Public Fields

        public int16 Data;
        public ShapeType ShapeType;

        #endregion

        #region Public methods

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref Data);
                switch (ShapeType)
                {
                    case ShapeType.Sphere:
                        return ((CShapeSphere*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.Torus:
                        return ((CShapeTorus*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.Noise: 
                        return ((CShapeNoise*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        //return ((CShapeVoronoi*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                    case ShapeType.Plane:
                        return ((CShapePlane*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break; 
                    case ShapeType.Box:
                        return ((CShapeBox*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        // {
        //     unsafe
        //     {
        //         var ptr = UnsafeUtility.AddressOf(ref Data);
        //         switch (ShapeType)
        //         {
        //             case ShapeType.Sphere:
        //                 return ((CShapeSphere*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             case ShapeType.BoundingBox:
        //                 return ((CShapeBoundingBox*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             case ShapeType.Torus:
        //                 return ((CShapeTorus*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             case ShapeType.Noise:
        //                 return ((CShapeNoise*) ptr)->CalculateBounds(translation, valueBuffer);
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException();
        //         }
        //     }
        //}

        // public unsafe uint CalculateHash()
        // {
        //     var ptr = UnsafeUtility.AddressOf(ref PropertyIndexesA);
        //     switch (ShapeType)
        //     {
        //         case ShapeType.Sphere:
        //             return ((CShapeSphere*) ptr)->CalculateHash();
        //             break;
        //         case ShapeType.BoundingBox:
        //             return ((CShapeBoundingBox*) ptr)->CalculateHash();
        //             break;
        //         case ShapeType.Torus:
        //             return ((CShapeTorus*) ptr)->CalculateHash();
        //             break;
        //         case ShapeType.Noise:
        //             return ((CShapeNoise*) ptr)->CalculateHash();
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }

        #endregion
    }
}