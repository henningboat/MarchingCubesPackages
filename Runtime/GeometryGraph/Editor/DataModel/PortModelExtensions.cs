﻿using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.MathNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Unity.Collections;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel
{
    public static class PortModelExtensions
    {
        public static void ResolveGeometryInput(this IPortModel port, GeometryGraphResolverContext context, GeometryStackData stack)
        {
            var connectedPort = port.GetConnectedPorts().FirstOrDefault(model => model != null && model.DataTypeHandle == TypeHandle.ExecutionFlow  && model.NodeModel != null);
            if (connectedPort != null && connectedPort.NodeModel is IGeometryNode geometryNode)
            {
                geometryNode.Resolve(context, stack);
            }
        }

        public static GeometryGraphProperty ResolvePropertyInput(this MathNode self, GeometryGraphResolverContext context, GeometryPropertyType geometryPropertyType)
        {
            GeometryGraphProperty resultProperty;

            switch (self)
            {
                case MathOperator mathOperator:
                    var inputs = mathOperator.GetInputProperties(context, geometryPropertyType);
                    if (inputs.Length != 2)
                    {
                        throw new Exception("inputs.Length != 2");
                    }

                    return context.GetOrCreateProperty(self.Guid,
                        new GeometryGraphMathOperatorProperty(context, geometryPropertyType, mathOperator.OperatorType, inputs[0], inputs[1], $"Math Operator {self.Title}"));
                default:
                    throw new ArgumentOutOfRangeException();
                    break;
            }
        }

        public static GeometryGraphProperty ResolvePropertyInput(this IPortModel self, GeometryGraphResolverContext context, GeometryPropertyType geometryPropertyType)
        {
            if (self == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            var connectedNode = self.GetConnectedEdges().FirstOrDefault()?.FromPort.NodeModel;

            switch (connectedNode)
            {
                case MathNode mathNode:
                    return mathNode.ResolvePropertyInput(context, geometryPropertyType);

                case IVariableNodeModel varNode:
                    var objectValue = varNode.VariableDeclarationModel.InitializationModel.ObjectValue;
                   

                    return context.GetOrCreateProperty(varNode.VariableDeclarationModel.Guid,
                        new GeometryGraphExposedVariableNode(varNode.VariableDeclarationModel, objectValue, context, geometryPropertyType,
                            varNode.VariableDeclarationModel.GetVariableName(), "Variable Node " + varNode.Title));

                case IConstantNodeModel constNode:
                    return context.GetOrCreateProperty(constNode.Guid, new GeometryGraphConstantProperty(constNode.ObjectValue, context, geometryPropertyType, ""));

                case IEdgePortalExitModel portalModel:
                    throw new NotImplementedException("Portals are not supported right now");
                // var oppositePortal = portalModel.GraphModel.FindReferencesInGraph<IEdgePortalEntryModel>(portalModel.DeclarationModel).FirstOrDefault();
                // if (oppositePortal != null)
                // {
                //     resultProperty = oppositePortal.InputPort.ResolvePropertyInput(context, geometryPropertyType);
                // }
                // else
                // {
                //     resultProperty = new GeometryGraphConstantProperty(default, context, geometryPropertyType, "Zero Value");
                // }
                //
                // break;
                default:
                    var embeddedValue = self.EmbeddedValue.ObjectValue;
                    if (embeddedValue is Color)
                    {
                        var color = (Color) self.EmbeddedValue.ObjectValue;
                        embeddedValue = new Vector3(color.r, color.g, color.b);
                    }
                    
                    return context.GetOrCreateProperty(self.Guid,
                        new GeometryGraphConstantProperty(embeddedValue, context, geometryPropertyType, $"Embedded Value Node{self.UniqueName} port {self.UniqueName}"));
            }
        }
    }
}