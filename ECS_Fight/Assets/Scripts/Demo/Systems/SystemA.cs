using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Test
{
    public class SystemA : JobComponentSystem
    {
        private EntityQuery _updateQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            _updateQuery = GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<ComponentA>(),
            });
            RequireForUpdate(_updateQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var cmpAaArchetypeChunkComponentType=GetArchetypeChunkComponentType<ComponentA>();
            var jobHandle = new JobA()
            {
                LastSystemVersion= this.LastSystemVersion,
                ComponentAs = cmpAaArchetypeChunkComponentType,
            }.ScheduleParallel(_updateQuery, inputDeps);
            return jobHandle;
        }

        private struct JobA : IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var didChange = chunk.DidChange(ComponentAs, LastSystemVersion);
                if (!didChange)
                    return;
                Debug.LogError("JobA running!");
                var compANative = chunk.GetNativeArray(ComponentAs);
                // for (int i = 0; i < compANative.Length; i++)
                // {
                //     compANative[i] = new ComponentA()
                //     {
                //         Value = i,
                //     };
                // }
            }

            public ArchetypeChunkComponentType<ComponentA> ComponentAs;
            public uint LastSystemVersion;
        }
    }
}