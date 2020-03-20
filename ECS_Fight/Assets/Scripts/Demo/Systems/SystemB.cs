using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Test
{
    [UpdateAfter(typeof(SystemA))]
    public class SystemB : JobComponentSystem
    {
        private EntityQuery _updateQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            _updateQuery = GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<ComponentB>(),
            });
            RequireForUpdate(_updateQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var cmpAaArchetypeChunkComponentType=GetArchetypeChunkComponentType<ComponentA>();
            var cmpBaArchetypeChunkComponentType=GetArchetypeChunkComponentType<ComponentB>();
            var jobHandle = new JobB()
            {
                LastSystemVersion= this.LastSystemVersion,
                ComponentAs = cmpAaArchetypeChunkComponentType,
                ComponentBs = cmpBaArchetypeChunkComponentType,
            }.ScheduleParallel(_updateQuery, inputDeps);
            return jobHandle;
        }

        private struct JobB : IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var didChange = chunk.DidChange(ComponentBs, LastSystemVersion);
                if (!didChange)
                    return;
                //Debug.LogError("JobB running!");
                var compANative = chunk.GetNativeArray(ComponentAs);
                var compBNative = chunk.GetNativeArray(ComponentBs);
                // for (int i = 0; i < compANative.Length; i++)
                // {
                //     compANative[i] = new ComponentA()
                //     {
                //         Value = i,
                //     };
                // }
            }

            public ArchetypeChunkComponentType<ComponentB> ComponentBs;
            public ArchetypeChunkComponentType<ComponentA> ComponentAs;
            public uint LastSystemVersion;
        }
    }
}