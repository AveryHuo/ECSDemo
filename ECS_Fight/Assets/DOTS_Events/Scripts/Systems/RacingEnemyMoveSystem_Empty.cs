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
public class RacingEnemyMoveSystem_Empty : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;
        float3 moveDir = new float3(-1f, 0f, 0f);
        float moveSpeed = 4f;

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref RacingEnemy pipe) => {
            float xBefore = translation.Value.x;
            translation.Value += moveDir * moveSpeed * deltaTime;
            float xAfter = translation.Value.x;

            if (xBefore > 0 && xAfter <= 0) {
                // Passed the Player
            }
        }).Schedule(inputDeps);

        return jobHandle;
    }

}