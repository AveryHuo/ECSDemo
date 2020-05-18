using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Collections;
using System;
using UnityEngine;

public class RacingEnemyHitSystem : JobComponentSystem {


    private struct EnemyTrigger : ITriggerEventsJob {

        [ReadOnly] public ComponentDataFromEntity<RacingEnemy> tagEnemyComponentDataFromEntity;
        [ReadOnly] public ComponentDataFromEntity<Tag_Wall> tagWallComponentDataFromEntity;
        [ReadOnly] public ComponentDataFromEntity<Tag_Player> tagPlayerComponentDataFromEntity;
        public EntityCommandBuffer entityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent) {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            Entity playerEntity = Entity.Null;
            Entity enemyEntity = Entity.Null;
            Entity wallEntity = Entity.Null;

            if (tagPlayerComponentDataFromEntity.HasComponent(entityA)) playerEntity = entityA;
            if (tagPlayerComponentDataFromEntity.HasComponent(entityB)) playerEntity = entityB;

            if (tagEnemyComponentDataFromEntity.HasComponent(entityA)) enemyEntity = entityA;
            if (tagEnemyComponentDataFromEntity.HasComponent(entityB)) enemyEntity = entityB;

            if (tagWallComponentDataFromEntity.HasComponent(entityA)) wallEntity = entityA;
            if (tagWallComponentDataFromEntity.HasComponent(entityB)) wallEntity = entityB;

            if ((playerEntity != Entity.Null && enemyEntity != Entity.Null) ||
                (playerEntity != Entity.Null && wallEntity != Entity.Null)) {
                // Collision between Player and Enemy or Player and Wall
                entityCommandBuffer.AddComponent(playerEntity, new Tag_GameOver());
            }

            //UnityEngine.Debug.Log(entityA + " " + entityB);
        }

    }

    private struct EnemyCollision : ICollisionEventsJob {

        public void Execute(CollisionEvent collisionEvent) {
            UnityEngine.Debug.Log("Collision: " );
        }
    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityCommandBuffer entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle = new EnemyTrigger {
            tagPlayerComponentDataFromEntity = GetComponentDataFromEntity<Tag_Player>(),
            tagEnemyComponentDataFromEntity = GetComponentDataFromEntity<RacingEnemy>(),
            tagWallComponentDataFromEntity = GetComponentDataFromEntity<Tag_Wall>(),
            entityCommandBuffer = entityCommandBuffer,
        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);


        /*I just noticed I had a system where I was using a command buffer to add a component inside a job and I forgot to call AddJobHandleForProducer(); but everything still worked correctly.
Is that call no longer needed? Or did I just get lucky with the order of the Systems? Does it depend on which buffer system I'm using, in this case EndSimulationEntityCommandBufferSystem?
         * */

        /*
        Entities.WithAll<Tag_GameOver>().ForEach((Entity entity) => {
            UnityEngine.Debug.Log("Game Over!");
        }).Run();
        //*/

        return jobHandle;
    }

}
