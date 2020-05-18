using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

[UpdateAfter(typeof(PlayerInputSystem))]
public class PlayerControlSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        if (HasSingleton<GameState>() && GetSingleton<GameState>().state == GameState.State.Playing) {
            float deltaTime = Time.DeltaTime;
            return Entities.WithAll<Tag_Player>().ForEach((ref MoveSpeed moveSpeed, ref Translation translation, ref Rotation rotation) => {
                float gravity = 0f;
                //moveSpeed.moveDirSpeed.y += gravity * deltaTime;
                translation.Value += moveSpeed.moveDirSpeed * deltaTime;
                moveSpeed.moveDirSpeed.x = 0;
                moveSpeed.moveDirSpeed.y = 0;
                //float rotationDampen = 30f;
                //rotation.Value = quaternion.Euler(0, 0, moveSpeed.moveDirSpeed.y / rotationDampen);
            }).Schedule(inputDeps);
        } else {
            return default;
        }
    }

}
