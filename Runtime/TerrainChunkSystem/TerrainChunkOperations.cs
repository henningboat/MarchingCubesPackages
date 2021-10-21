using System;
using Code.SIMDMath;
using henningboat.CubeMarching.GeometryComponents;
using Unity.Collections;
using Unity.Mathematics;
using static Code.SIMDMath.SimdMath;

namespace henningboat.CubeMarching.TerrainChunkSystem
{
    public static class TerrainChunkOperations
    {
        public static PackedDistanceFieldData CombinePackedTerrainData(CGeometryCombiner combiner, PackedDistanceFieldData valuesA, PackedDistanceFieldData valuesB, NativeArray<float> propertyBuffer)
        {
            PackedDistanceFieldData packedTerrainData;
            switch (combiner.Operation)
            {
                case CombinerOperation.Min:
                    packedTerrainData = CombineTerrainMin(valuesA, valuesB);
                    break;
                case CombinerOperation.Max:
                    packedTerrainData = CombineTerrainMax(valuesA, valuesB);
                    break;
                case CombinerOperation.SmoothMin:
                    packedTerrainData = CombineTerrainSmoothMin(valuesA, valuesB, combiner.BlendFactor.Resolve(propertyBuffer));
                    break;
                case CombinerOperation.SmoothSubtract:
                    packedTerrainData = CombineTerrainSmoothSubtract(valuesA, valuesB, combiner.BlendFactor.Resolve(propertyBuffer));
                    break;
                case CombinerOperation.Add:
                    packedTerrainData = CombineTerrainAdd(valuesA, valuesB);
                    break;
                //todo this feels like a workaround
                case CombinerOperation.Replace:
                    packedTerrainData = valuesA;
                    break;
                case CombinerOperation.ReplaceMaterial:
                    packedTerrainData = ReplaceTerrainColor(valuesA, valuesB);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return packedTerrainData;
        }

        private static PackedDistanceFieldData ReplaceTerrainColor(PackedDistanceFieldData a, PackedDistanceFieldData b)
        {
            var replaceTerrainMaterial = a.SurfaceDistance.PackedValues > 0;
            return new PackedDistanceFieldData {SurfaceDistance = b.SurfaceDistance, TerrainMaterial = PackedTerrainMaterial.Select(b.TerrainMaterial, a.TerrainMaterial, replaceTerrainMaterial)};
        }

        public static PackedDistanceFieldData CombineTerrainMin(PackedDistanceFieldData a, PackedDistanceFieldData b)
        {
            var bIsSmaller = a.SurfaceDistance.PackedValues > b.SurfaceDistance.PackedValues;
            var surfaceDistance = math.min(a.SurfaceDistance.PackedValues, b.SurfaceDistance.PackedValues);
            var combinedMaterial = PackedTerrainMaterial.Select(a.TerrainMaterial, b.TerrainMaterial, bIsSmaller);

            return new PackedDistanceFieldData(new PackedFloat(surfaceDistance), combinedMaterial);
        }

        public static PackedDistanceFieldData CombineTerrainMax(PackedDistanceFieldData a, PackedDistanceFieldData b)
        {
            var bIsBigger = a.SurfaceDistance.PackedValues < b.SurfaceDistance.PackedValues;
            var surfaceDistance = math.select(a.SurfaceDistance.PackedValues, b.SurfaceDistance.PackedValues, bIsBigger);
            var combinedMaterial = PackedTerrainMaterial.Select(a.TerrainMaterial, b.TerrainMaterial, !bIsBigger);

            return new PackedDistanceFieldData(new PackedFloat(surfaceDistance), combinedMaterial);
        }

        public static PackedDistanceFieldData CombineTerrainAdd(PackedDistanceFieldData a, PackedDistanceFieldData b)
        {
            return new(a.SurfaceDistance + b.SurfaceDistance, a.TerrainMaterial);
        }

        //https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        public static PackedDistanceFieldData CombineTerrainSmoothMin(PackedDistanceFieldData terrainDataA, PackedDistanceFieldData terrainDataB, PackedFloat blendFactor)
        {
            var a = terrainDataA.SurfaceDistance;
            var b = terrainDataB.SurfaceDistance;
            var h = clamp(0.5f + 0.5f * (a - b) / blendFactor, 0.0f, 1.0f);
            var blendedSurfaceDistance = lerp(a, b, h) - blendFactor * h * (1.0f - h);

            var bIsSmaller = terrainDataA.SurfaceDistance.PackedValues > terrainDataB.SurfaceDistance.PackedValues;
            var combinedMaterial = PackedTerrainMaterial.Select(terrainDataA.TerrainMaterial, terrainDataB.TerrainMaterial, bIsSmaller);

            return new PackedDistanceFieldData(blendedSurfaceDistance, combinedMaterial);
        }

        private static PackedDistanceFieldData CombineTerrainSmoothSubtract(PackedDistanceFieldData terrainDataA, PackedDistanceFieldData terrainDataB, PackedFloat blendFactor)
        {
            var a = terrainDataA.SurfaceDistance;
            var b = terrainDataB.SurfaceDistance;
            var h = clamp(0.5f - 0.5f * (b + a) / blendFactor, 0.0f, 1.0f);
            var blendedSurfaceDistance = lerp(b, -a, h) + blendFactor * h * (1.0f - h);

            var bIsSmaller = terrainDataA.SurfaceDistance.PackedValues > terrainDataB.SurfaceDistance.PackedValues;
            var combinedMaterial = PackedTerrainMaterial.Select(terrainDataA.TerrainMaterial, terrainDataB.TerrainMaterial, bIsSmaller);

            return new PackedDistanceFieldData(blendedSurfaceDistance, combinedMaterial);
        }
    }
}