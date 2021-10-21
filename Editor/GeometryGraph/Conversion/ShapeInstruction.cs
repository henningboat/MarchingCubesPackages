﻿using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using henningboat.CubeMarching.GeometryComponents;
using henningboat.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    internal class ShapeInstruction : GeometryGraphInstruction
    {
        public readonly ShapeType ShapeType;
        public readonly GeometryGraphProperty Transformation;
        public readonly GeometryGraphProperty Color;
        public readonly List<GeometryGraphProperty> ShapeProperties;
        private readonly CombinerInstruction _combiner;

        public ShapeInstruction(ShapeType shapeType, GeometryGraphProperty transformation, GeometryGraphProperty color, List<GeometryGraphProperty> shapeProperties, int currentCombinerDepth,
            CombinerInstruction combiner) : base(currentCombinerDepth)
        {
            ShapeType = shapeType;
            Transformation = transformation;
            Color = color;
            ShapeProperties = shapeProperties;
            _combiner = combiner;
        }

        public override GeometryInstruction GetInstruction()
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.Shape, (int) ShapeType, Depth, _combiner.GetCombinerSetting(), Transformation, ShapeProperties, Color);
        }
    }
}