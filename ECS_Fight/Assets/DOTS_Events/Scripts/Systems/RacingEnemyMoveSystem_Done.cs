using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class RacingEnemyMoveSystem_Done : JobComponentSystem {

    public event EventHandler OnRacingEnemyPassed;

    private DOTSEvents_NextFrame<RacingEnemyPassedEvent> dotsEvents;
    public struct RacingEnemyPassedEvent : IComponentData {
        public double Value;
    }

    protected override void OnCreate() {
        dotsEvents = new DOTSEvents_NextFrame<RacingEnemyPassedEvent>(World);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        double elapsedTime = Time.ElapsedTime;

        DOTSEvents_NextFrame<RacingEnemyPassedEvent>.EventTrigger eventTrigger = dotsEvents.GetEventTrigger();

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref Translation translation, ref RacingEnemy pipe, ref MoveSpeed moveSpeed) => {
            float yBefore = translation.Value.y;
            translation.Value += moveSpeed.moveDirSpeed * deltaTime;
            float yAfter = translation.Value.y;

            if (yBefore > -6 && yAfter <= -6) {
                // Passed the Player
                eventTrigger.TriggerEvent(entityInQueryIndex, new RacingEnemyPassedEvent { Value = elapsedTime });
            }
        }).Schedule(inputDeps);

        dotsEvents.CaptureEvents(jobHandle, (RacingEnemyPassedEvent basicEvent) => {
            Debug.Log(basicEvent.Value + " ###### " + elapsedTime);
            OnRacingEnemyPassed?.Invoke(this, EventArgs.Empty);
        });
        return jobHandle;
    }


}