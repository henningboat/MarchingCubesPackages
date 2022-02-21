using System;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using SIMDMath;
using Unity.Mathematics;
using static SIMDMath.SimdMath;

namespace henningboat.CubeMarching.Runtime.TerrainChunkSystem
{
    public static class TerrainChunkOperations
    {
        public static PackedDistanceFieldData CombinePackedTerrainData(CombinerOperation combinerOperation, float blendFactor, PackedDistanceFieldData valuesA, PackedDistanceFieldData valuesB)
        {
            PackedDistanceFieldData packedTerrainData;
            switch (combinerOperation)
            {
                case CombinerOperation.Min:
                    packedTerrainData = CombineTerrainMin(valuesA, valuesB);
                    break;
                case CombinerOperation.Max:
                    packedTerrainData = CombineTerrainMax(valuesA, valuesB);
                    break;
                case CombinerOperation.SmoothMin:
                    packedTerrainData = CombineTerrainSmoothMin(valuesA, valuesB, blendFactor);
                    break;
                case CombinerOperation.SmoothSubtract:
                    packedTerrainData = CombineTerrainSmoothSubtract(valuesA, valuesB, blendFactor);
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
                case CombinerOperation.Subtract:
                    packedTerrainData = CombineTerrainSubtract(valuesA, valuesB);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return packedTerrainData;
        }

        private static PackedDistanceFieldData CombineTerrainSubtract(PackedDistanceFieldData valuesA, PackedDistanceFieldData valuesB)
        {
            valuesB.SurfaceDistance = max(-valuesA.SurfaceDistance, valuesB.SurfaceDistance);
            return valuesB;
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
            
            return new PackedDistanceFieldData(new PackedFloat(surfaceDistance), a.TerrainMaterial);
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
            
            var combinedMaterial = PackedTerrainMaterial.Lerp(terrainDataA.TerrainMaterial, terrainDataB.TerrainMaterial, h);

            return new PackedDistanceFieldData(blendedSurfaceDistance, combinedMaterial);
        }

        private static PackedDistanceFieldData CombineTerrainSmoothSubtract(PackedDistanceFieldData terrainDataA, PackedDistanceFieldData terrainDataB, PackedFloat blendFactor)
        {
            var a = terrainDataA.SurfaceDistance;
            var b = terrainDataB.SurfaceDistance;
            var h = clamp(0.5f - 0.5f * (b + a) / blendFactor, 0.0f, 1.0f);
            var blendedSurfaceDistance = lerp(b, -a, h) + blendFactor * h * (1.0f - h);

            var combinedMaterial = terrainDataA.TerrainMaterial;

            return new PackedDistanceFieldData(blendedSurfaceDistance, combinedMaterial);
        }
    }
}