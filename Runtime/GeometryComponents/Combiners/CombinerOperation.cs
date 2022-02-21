﻿namespace henningboat.CubeMarching.Runtime.GeometryComponents.Combiners
{
    public enum CombinerOperation : byte
    {
        Min = 0,
        Max = 1,
        SmoothMin = 2,
        SmoothSubtract = 3,
        Add = 4,
        Replace = 5,
        ReplaceMaterial = 6,
        Subtract = 7,
    }
}