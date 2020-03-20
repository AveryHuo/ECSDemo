using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveSystem : JobComponentSystem
{
    [BurstCompile]
    private struct MoveJob : IJobChunk
    {
        public double ElapsedTime;

        public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkComponentType<MoveBySpeedData> MoveBySpeedDataType;
        [ReadOnly] public ArchetypeChunkComponentType<WaveData> WaveDataType;
        public uint LastSystemVersion;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {

            var chunkTrans = chunk.GetNativeArray(TranslationType);
            var chunkSpeedDatas = chunk.GetNativeArray(MoveBySpeedDataType);
            var chunkWaveDatas = chunk.GetNativeArray(WaveDataType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var moveSpeed = chunkSpeedDatas[i];
                var waveData = chunkWaveDatas[i];
                var trans = chunkTrans[i];
                float zPos = waveData.amplitude * math.sin((float)ElapsedTime * moveSpeed.Value
               + trans.Value.x * waveData.xOffset + trans.Value.y * waveData.yOffset);

                // Rotate something about its up vector at the speed given by RotationSpeed.
                chunkTrans[i] = new Translation
                {
                    Value = new float3(trans.Value.x, trans.Value.y, zPos)
                };
            }
        }
    }
    EntityQuery m_Group;
    protected override void OnCreate()
    {
        m_Group = GetEntityQuery(ComponentType.ReadWrite<Translation>(),
                        ComponentType.ReadOnly<MoveBySpeedData>(),
                               ComponentType.ReadOnly<WaveData>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MoveJob()
        {
            TranslationType = GetArchetypeChunkComponentType<Translation>(false),
            MoveBySpeedDataType = GetArchetypeChunkComponentType<MoveBySpeedData>(true),
            WaveDataType = GetArchetypeChunkComponentType<WaveData>(true),
            ElapsedTime = Time.ElapsedTime
        };
        return job.Schedule(m_Group, inputDeps);
    }
}