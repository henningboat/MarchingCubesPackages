﻿using System;

namespace henningboat.CubeMarching.Runtime.GeometryComponents.Combiners
{
    public static class CombinerOperationExtensions
    {
        public static bool HasBlendFactor(this CombinerOperation combinerOperation)
        {
            switch (combinerOperation)
            {
                case CombinerOperation.Min:
                case CombinerOperation.Max:
                case CombinerOperation.Add:
                case CombinerOperation.Replace:
                case CombinerOperation.ReplaceMaterial:
                    return false;
                case CombinerOperation.SmoothMin:
                case CombinerOperation.SmoothSubtract:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(combinerOperation), combinerOperation, null);
            }
        }
    }
}