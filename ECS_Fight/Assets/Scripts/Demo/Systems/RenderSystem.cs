using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Test
{
    public class RenderSystem : JobComponentSystem
    {
        private EntityQuery _updateQuery;
        private EntityQuery _renderDataQuery;
        static readonly int K_Max_Batch_Count = 1023;
        private List<Matrix4x4[]> _matrix4X4ses = new List<Matrix4x4[]>();
        private readonly List<GCHandle> _handles = new List<GCHandle>();
        protected override void OnCreate()
        {
            base.OnCreate();
            _updateQuery = GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<LocalToWorldComponent>(), 
            });
            _renderDataQuery = GetEntityQuery(ComponentType.ReadOnly<RenderDataComponent>());
            RequireForUpdate(_updateQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var renderData = EntityManager.GetSharedComponentData<RenderDataComponent>(_renderDataQuery.GetSingletonEntity());
            var renderCount = _updateQuery.CalculateEntityCount();
            var remaining = renderCount % K_Max_Batch_Count;
            var batchNum = renderCount / K_Max_Batch_Count + (remaining > 0 ? 1 : 0);
            // 构建
            for (int i = 0; i < batchNum; i++)
            {
                _matrix4X4ses.Add(new Matrix4x4[K_Max_Batch_Count]);
            }
            NativeArray<IntPtr> matrixNativeArray = new NativeArray<IntPtr>(batchNum, Allocator.Temp);
            
            for (int i = 0; i < batchNum; i++)
            {
                var gcHandle = GCHandle.Alloc(_matrix4X4ses[i], GCHandleType.Pinned);
                _handles.Add(gcHandle);
                matrixNativeArray[i] = gcHandle.AddrOfPinnedObject();
            }

            unsafe
            {
                new RenderJob()
                {
                    Matrices = (Matrix4x4**) matrixNativeArray.GetUnsafePtr(),
                }.Schedule(this, inputDeps).Complete();
            }

            //release
            for (int i = 0; i < batchNum; i++)
            {
                _handles[i].Free();
            }
            _handles.Clear();
            
            for (int i = 0; i < batchNum; i++)
            {
                int batchSize = (i == batchNum - 1) ? remaining : K_Max_Batch_Count;
                Graphics.DrawMeshInstanced(renderData.MeshValue, 0, renderData.MaterialValue, _matrix4X4ses[i],
                    batchSize);
            }

            return inputDeps;
        }
        [BurstCompile]
        private unsafe struct RenderJob : IJobForEachWithEntity<LocalToWorldComponent>
        {
            public void Execute(Entity entity, int index, [ReadOnly]ref LocalToWorldComponent local)
            {
                int batch = index / K_Max_Batch_Count;
                int offset = index % K_Max_Batch_Count;
                Matrices[batch][offset] = local.Value;
            }

            [NativeDisableUnsafePtrRestriction]public Matrix4x4** Matrices;
        }
    }
}