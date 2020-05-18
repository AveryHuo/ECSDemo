using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

[DisableAutoCreation]
public class RacingEnemyMoveSystem : JobComponentSystem {

    public event EventHandler OnRacingEnemyPassed;

    public struct EventComponent : IComponentData {
        public double ElapsedTime;
    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;
        float3 moveDir = new float3(-1f, 0f, 0f);
        float moveSpeed = 4f;

        EntityCommandBuffer entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        EntityCommandBuffer.Concurrent entityCommandBufferConcurrent = entityCommandBuffer.ToConcurrent();
        EntityArchetype eventEntityArchetype = EntityManager.CreateArchetype(typeof(EventComponent));

        double ElapsedTime = Time.ElapsedTime;

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref RacingEnemy pipe) => {
            float xBefore = translation.Value.x;
            translation.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = translation.Value.x;

            if (xBefore > 0 && xAfter <= 0) {
                // Passed the Player
                Entity eventEntity = entityCommandBufferConcurrent.CreateEntity(entityInQueryIndex, eventEntityArchetype);
                entityCommandBufferConcurrent.SetComponent(entityInQueryIndex, eventEntity, new EventComponent {
                    ElapsedTime = ElapsedTime
                });
            }
        }).Schedule(inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        EntityCommandBuffer captureEventsEntityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        Entities.WithoutBurst().ForEach((Entity entity, ref EventComponent eventComponent) => {
            Debug.Log(eventComponent.ElapsedTime + " ### " + ElapsedTime);
            OnRacingEnemyPassed?.Invoke(this, EventArgs.Empty);
            captureEventsEntityCommandBuffer.DestroyEntity(entity);
        }).Run();

        return jobHandle;
    }

}