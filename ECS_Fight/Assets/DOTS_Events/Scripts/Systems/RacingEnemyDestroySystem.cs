using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

public class RacingEnemyDestroySystem : JobComponentSystem {

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityCommandBuffer.Concurrent entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        float destroyYPosition = -7f;
        JobHandle jobHandle = Entities.WithAll<RacingEnemy>().ForEach((int entityInQueryIndex, Entity entity, ref Translation translation) => {
            if (translation.Value.y <= destroyYPosition) {
                entityCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
        }).Schedule(inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

}
