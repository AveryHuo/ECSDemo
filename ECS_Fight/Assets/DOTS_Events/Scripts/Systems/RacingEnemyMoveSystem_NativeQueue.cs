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
public class RacingEnemyMoveSystem_NativeQueue : JobComponentSystem {

    public event EventHandler OnRacingEnemyPassed;

    public struct RacingEnemyPassedEvent
    {
    }

    private NativeQueue<RacingEnemyPassedEvent> eventQueue;

    protected override void OnCreate() {
        eventQueue = new NativeQueue<RacingEnemyPassedEvent>(Allocator.Persistent);
    }

    protected override void OnDestroy() {
        eventQueue.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        float3 moveDir = new float3(-1f, 0f, 0f);
        float moveSpeed = 4f;

        NativeQueue<RacingEnemyPassedEvent>.ParallelWriter eventQueueParallel = eventQueue.AsParallelWriter();

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref RacingEnemy pipe) => {
            float xBefore = translation.Value.x;
            translation.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = translation.Value.x;

            if (xBefore > 0 && xAfter <= 0) {
                // Passed the Player
                eventQueueParallel.Enqueue(new RacingEnemyPassedEvent { });
            }
        }).Schedule(inputDeps);

        jobHandle.Complete();

        while (eventQueue.TryDequeue(out RacingEnemyPassedEvent pipePassedEvent)) {
            OnRacingEnemyPassed?.Invoke(this, EventArgs.Empty);
        }

        return jobHandle;
    }

}