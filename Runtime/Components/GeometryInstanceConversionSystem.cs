using System.Collections.Generic;
using System.Linq;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.DistanceFieldGeneration;
using henningboat.CubeMarching.Runtime.GeometryComponents.Combiners;
using henningboat.CubeMarching.Runtime.GeometryComponents.DistanceModifications;
using henningboat.CubeMarching.Runtime.GeometryGraphSystem;
using henningboat.CubeMarching.Runtime.GeometryListGeneration;
using henningboat.CubeMarching.Runtime.GeometrySystems;
using henningboat.CubeMarching.Runtime.Utils.Containers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace henningboat.CubeMarching.Runtime.Components
{
    [ConverterVersion("Henning", 1)]
    public class GeometryInstanceConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GeometryGraphInstance geometryGraphInstance) => { ConvertGeometryInstance(geometryGraphInstance); });
            Entities.ForEach((PrimitiveInstance primitiveInstance) => { ConvertGeometryInstance(primitiveInstance); });

            Entities.ForEach((GeometryLayerAsset asset) =>
            {
                var layerEntityTest = GetPrimaryEntity(asset);
                DstEntityManager.AddComponent<CGeometryLayerInstance>(layerEntityTest);
                DstEntityManager.AddComponent<Translation>(layerEntityTest);
            });
        }

        private void ConvertGeometryInstance(GeometryInstance instance)
        {
            var entity = GetPrimaryEntity(instance);

            var geometryInstructions = DstEntityManager.AddBuffer<GeometryInstruction>(entity);
            foreach (var geometryInstruction in instance.GeometryInstructionList.geometryInstructions)
            {
                geometryInstructions.Add(geometryInstruction);
            }

            SetTopLevelBlendOperation(geometryInstructions, instance.combinerOperation);

            var valueBuffer = DstEntityManager.AddBuffer<CGeometryGraphValue>(entity).Reinterpret<float>();
            foreach (var f in instance.GeometryInstructionList.valueBuffer)
            {
                valueBuffer.Add(f);
            }

            foreach (var overwrite in instance.GetOverwrites())
            {
                var variable = instance.GeometryInstructionList.GetIndexOfProperty(overwrite.PropertyGUID);
                if (variable != null)
                {
                    for (int i = 0; i < variable.GetSizeInBuffer(); i++)
                    {
                        valueBuffer[variable.Index + i] = overwrite.Value[i];
                    }
                }
            }

            DstEntityManager.AddComponentData(entity,
                new CMainTransformationIndex() {Index = instance.GeometryInstructionList.MainTransformation.Index});

            DstEntityManager.AddComponent<CGeometryInstructionSourceTag>(entity);
            DstEntityManager.AddSharedComponentData(entity,
                new GeometryLayerAssetsReference(instance.geometryLayerAsset));
        }

        private void SetTopLevelBlendOperation(DynamicBuffer<GeometryInstruction> geometryInstructions,
            CombinerOperation combinerOperation)
        {
            for (var i = 0; i < geometryInstructions.Length; i++)
            {
                var geometryInstruction = geometryInstructions[i];
                if (geometryInstruction.CombinerDepth == 0)
                {
                    geometryInstruction.CombinerBlendOperation = combinerOperation;
                    geometryInstructions[i] = geometryInstruction;
                }
            }
        }
    }
}