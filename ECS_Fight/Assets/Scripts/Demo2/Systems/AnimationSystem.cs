using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

public class AnimationSystem : JobComponentSystem
{
    [BurstCompile]
    private struct MoveJob : IJobChunk
    {
        public double ElapsedTime;

        public ArchetypeChunkComponentType<AnimatorComponent> AnimationType;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkAnimators = chunk.GetNativeArray(AnimationType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var animComp = chunkAnimators[i];
            }
        }
    }
    EntityQuery m_Group;
    protected override void OnCreate()
    {
        m_Group = GetEntityQuery(ComponentType.ReadWrite<AnimatorComponent>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MoveJob()
        {
            AnimationType = GetArchetypeChunkComponentType<AnimatorComponent>(false),
            ElapsedTime = Time.ElapsedTime
        };
        return job.Schedule(m_Group, inputDeps);
    }
}
