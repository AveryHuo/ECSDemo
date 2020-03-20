using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Test
{
    public class PositionSystem : JobComponentSystem
    {
        private EntityQuery _updateQuery;
        protected override void OnCreate()
        {
            base.OnCreate();
            _updateQuery = GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<PositionComponent>(),
                ComponentType.ReadWrite<LocalToWorldComponent>(), 
            });
            RequireForUpdate(_updateQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var positionArchetypeChunkComponentType = GetArchetypeChunkComponentType<PositionComponent>(true);
            var faceArchetypeChunkComponentType = GetArchetypeChunkComponentType<FaceComponent>(true);
            var localToWorldArchetypeChunkComponentType = GetArchetypeChunkComponentType<LocalToWorldComponent>(false);
            var jobHandle = new PositionJob()
            {
                LastSystemVersion = this.LastSystemVersion,
                PositionArch = positionArchetypeChunkComponentType,
                FaceArch = faceArchetypeChunkComponentType,
                LocalToWorldArch = localToWorldArchetypeChunkComponentType,
            }.Schedule(_updateQuery, inputDeps);
            return jobHandle;
        }
        [BurstCompile]
        private struct PositionJob : IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var isChange = chunk.DidChange(PositionArch, LastSystemVersion);
                if (!isChange)
                    return;
                var positionNativeArray = chunk.GetNativeArray(PositionArch);
                var faceNativeArray = chunk.GetNativeArray(FaceArch);
                var localToWorldNativeArray = chunk.GetNativeArray(LocalToWorldArch);
                var length = positionNativeArray.Length;
                for (int i = 0; i < length; i++)
                {
                    localToWorldNativeArray[i] = new LocalToWorldComponent
                    {
                        Value = float4x4.TRS(positionNativeArray[i].Value, faceNativeArray[i].Value, 1),
                    };
                }
            }

            [ReadOnly]public ArchetypeChunkComponentType<PositionComponent> PositionArch;
            public ArchetypeChunkComponentType<LocalToWorldComponent> LocalToWorldArch;
            [ReadOnly]public uint LastSystemVersion;
            [ReadOnly]public ArchetypeChunkComponentType<FaceComponent> FaceArch;
        }
    }
}