using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;
using System.Dynamic;

public class PlayerInputSystem : JobComponentSystem {

    private enum InputType
    {
        JUMP,
        UP,
        DOWN,
        LEFT,
        RIGHT,
        NONE
    }
    private bool isUping = false;
    private bool isDowning = false;
    private bool isLefting = false;
    private bool isRighting = false;

    private float playerSpeed = 2.0f;
    private InputType GetInputType()
    {
        InputType retType = InputType.NONE;
        bool moveUpInputUp = Input.GetKeyUp(KeyCode.UpArrow);
        bool moveDownInputUp = Input.GetKeyUp(KeyCode.DownArrow);
        bool moveUpInputDown = Input.GetKeyDown(KeyCode.UpArrow);
        bool moveDownInputDown = Input.GetKeyDown(KeyCode.DownArrow);
        bool moveLeftInputUp = Input.GetKeyUp(KeyCode.LeftArrow);
        bool moveRightInputUp = Input.GetKeyUp(KeyCode.RightArrow);
        bool moveLeftInputDown = Input.GetKeyDown(KeyCode.LeftArrow);
        bool moveRightInputDown = Input.GetKeyDown(KeyCode.RightArrow);

        bool jumpInputDown = Input.GetKeyDown(KeyCode.Space);

        if (jumpInputDown)
        {
            retType = InputType.JUMP;
            return retType;
        }

        if (moveUpInputUp)
            isUping = false;
        if(moveUpInputDown)
            isUping = true;
        if (moveDownInputUp)
            isDowning = false;
        if (moveDownInputDown)
            isDowning = true;
        if (moveLeftInputUp)
            isLefting = false;
        if (moveLeftInputDown)
            isLefting = true;
        if (moveRightInputUp)
            isRighting = false;
        if (moveRightInputDown)
            isRighting = true;

        if (isUping)
            retType = InputType.UP;
        if (isDowning)
            retType = InputType.DOWN;
        if (isLefting)
            retType = InputType.LEFT;
        if (isRighting)
            retType = InputType.RIGHT;

        return retType;

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        InputType curInputType = GetInputType();
        bool isUp = curInputType == InputType.UP;
        bool isDown = curInputType == InputType.DOWN;
        bool isLeft = curInputType == InputType.LEFT;
        bool isRight = curInputType == InputType.RIGHT;
        float speed = playerSpeed;
        if (curInputType == InputType.JUMP) {
            if (HasSingleton<GameState>()) {
                GameState gameState = GetSingleton<GameState>();
                if (gameState.state == GameState.State.WaitingToStart) {
                    gameState.state = GameState.State.Playing;
                    SetSingleton(gameState);

                    World.GetExistingSystem<RacingEnemyHitSystem>().Enabled = true;
                    World.GetExistingSystem<RacingEnemyMoveSystem_Done>().Enabled = true;
                    World.GetExistingSystem<RacingEnemyDestroySystem>().Enabled = true;
                    World.GetExistingSystem<RacingEnemySpawnerSystem>().Enabled = true;
                    World.GetExistingSystem<PlayerControlSystem>().Enabled = true;
                }
            }
        }

        return Entities.WithoutBurst().WithAll<Tag_Player>().ForEach((ref MoveSpeed moveSpeed) => {
            if (isUp)
            {
                moveSpeed.moveDirSpeed.y = speed;
            }
            if (isDown)
            {
                moveSpeed.moveDirSpeed.y = -speed;
            }
            if (isLeft)
            {
                moveSpeed.moveDirSpeed.x = -speed;
            }
            if (isRight)
            {
                moveSpeed.moveDirSpeed.x = speed;
            }
        }).Schedule(inputDeps);
    }
}
