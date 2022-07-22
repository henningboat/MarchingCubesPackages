using henningboat.CubeMarching.Runtime;
using henningboat.CubeMarching.Runtime.Components;
using henningboat.CubeMarching.Runtime.Systems;
using henningboat.CubeMarching.Runtime.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Overlays;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI
{
    [Overlay(typeof(SceneView), "Geometry", true)]
    public class GeometryDebuggingOverlay : Overlay
    {
        private VisualElement _root;
        private Toggle _mainToggle;
        private VisualElement _controls;
        private SShowDebugVisualizations _system;

        public override VisualElement CreatePanelContent()
        {
            _root = new VisualElement();
            _mainToggle = new Toggle(displayName = "Enable Debug Visuals");
            _root.Add(_mainToggle);
            _controls = new GroupBox();

            var showEnabledChunksToggle = new Toggle("Show chunks with content");
            _mainToggle.RegisterValueChangedCallback<bool>((value) =>
            {
                if (value.newValue)
                {
                    _root.Add(_controls);
                }
                else
                {
                    _root.Remove(_controls);
                }
            });


            var view = base.containerWindow as SceneView;

            _controls.Add(showEnabledChunksToggle);

            return _root;
        }
    }
    
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(SDisplayGeometryLayers))]
    public partial class SShowDebugVisualizations:SystemBase
    {
        private NativeList<float3> _activeChunks;
        
        protected override void OnStopRunning()
        {
            if (_activeChunks.IsCreated)
            {
                _activeChunks.Dispose();
            }
        }

        private void OnDrawGizmos()
        {
            if (_activeChunks.IsCreated)
            {
                foreach (var activeChunk in _activeChunks)
                {
                    Gizmos.DrawWireCube((Vector3) activeChunk + (Constants.chunkLength / 2.0f * Vector3.one),
                        Vector3.one * Constants.chunkLength);
                }
            }
        }

        protected override void OnUpdate()
        {
            if (_activeChunks.IsCreated)
            {
                _activeChunks.Dispose();
            }
            _activeChunks = new NativeList<float3>(10000, Allocator.TempJob);
            var parallelWriter = _activeChunks.AsParallelWriter();
            Dependency = Entities.ForEach((in CGeometryChunk chunk, in CGeometryChunkState state) =>
            {
                if (state.HasContent)
                {
                    parallelWriter.AddNoResize(chunk.PositionWS);
                }
            }).WithBurst().ScheduleParallel(Dependency);
            
            Dependency.Complete();
            GizmosInjector.Instance.RegisterCallback(OnDrawGizmos);
        }
    }
}