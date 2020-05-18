using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;

public class GameOverSystem : JobComponentSystem {

    public event System.EventHandler OnGameOver;

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        Entities.WithAll<Tag_GameOver>().WithoutBurst().WithStructuralChanges().ForEach((Entity entity) => {
            EntityManager.RemoveComponent<Tag_GameOver>(entity);
            GameOver();
        }).Run();
        return default;
    }

    private void GameOver() {
        UnityEngine.Debug.Log("Game Over!");
        //GetSingleton<Tag_GameOver>().overCanvas;
        World.GetExistingSystem<RacingEnemyHitSystem>().Enabled = false;
        World.GetExistingSystem<RacingEnemyMoveSystem_Done>().Enabled = false;
        World.GetExistingSystem<RacingEnemyDestroySystem>().Enabled = false;
        World.GetExistingSystem<RacingEnemySpawnerSystem>().Enabled = false;
        World.GetExistingSystem<PlayerControlSystem>().Enabled = false;

        if (HasSingleton<GameState>()) {
            GameState gameState = GetSingleton<GameState>();
            gameState.state = GameState.State.Dead;
            SetSingleton(gameState);
        }

        //OnGameOver?.Invoke(this, System.EventArgs.Empty);
    }

}
