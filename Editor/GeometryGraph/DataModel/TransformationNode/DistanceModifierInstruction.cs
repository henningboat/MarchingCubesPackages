﻿using System.Linq;
using Editor.GeometryGraph.Conversion;
using henningboat.CubeMarching.PrimitiveBehaviours;
using henningboat.CubeMarching.TerrainChunkEntitySystem;

namespace Editor.GeometryGraph.DataModel.TransformationNode
{
    public abstract class DistanceModifierInstruction : GeometryGraphInstruction
    {
        private GeometryGraphProperty[] _properties;
        private GeometryGraphProperty _transformation;
        private CombinerState _combiner;
        protected abstract DistanceModificationType Type { get; }

        protected DistanceModifierInstruction(int depth, CombinerState combiner, GeometryGraphProperty transformation,
            params GeometryGraphProperty[] properties) : base(depth)
        {
            _combiner = combiner;
            _transformation = transformation;
            _properties = properties;
        }


        public override GeometryInstruction GetInstruction()
        {
            return GeometryInstructionUtility.CreateInstruction(GeometryInstructionType.DistanceModification,
                (int) Type, default, _properties.ToList());
        }
    }
}