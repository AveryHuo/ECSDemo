using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Jobs;

public class RacingEnemySpawnerSystem : ComponentSystem {

   
    private bool firstUpdate = true;
    private int counter = 0;
    private float spawnYPosition = 6f;
    private float pipeSpawnTimer;
    private Random random;
    private int randomCountMax;
    private float randomEnemySpeedMin;
    private float randomEnemySpeedMax;
    private float randomCarSpeedMin;
    private float randomCarSpeedMax;

    protected override void OnCreate() {
        
        random = new Random(56);

        counter = 0;
        randomCountMax = 4;
        randomEnemySpeedMin = 4.5f;
        randomEnemySpeedMax = 5.5f;
        randomCarSpeedMin = 6f;
        randomCarSpeedMax = 8f;
    }


    protected override void OnUpdate() {
        if (HasSingleton<GameState>()) {
            GameState gameState = GetSingleton<GameState>();

            if (gameState.state == GameState.State.Playing) {
                if (firstUpdate) {
                    firstUpdate = false;
                }

                //Generate once a second
                pipeSpawnTimer -= Time.DeltaTime;
                if (pipeSpawnTimer <= 0f) {
                    float pipeSpawnMax = 1f;
                    pipeSpawnTimer = pipeSpawnMax;
                    //Set Difficulty!
                    counter++;
                    TestEnemyGapDifficulty();

                    //random generate count
                    int spawnCount = random.NextInt(0, randomCountMax);
                    while(spawnCount > 0)
                    {
                        bool isCar = random.NextInt(0, 1000) > 500;
                        float speed = 0;
                        if (isCar)
                            speed = random.NextFloat(randomCarSpeedMin, randomCarSpeedMax);
                        else
                            speed = random.NextFloat(randomEnemySpeedMin, randomEnemySpeedMax);
                        float xPos = random.NextFloat(-2.4f,2.4f);

                        SpwanOther(isCar ? RacingEnemyType.Car : RacingEnemyType.Pipe, xPos, speed);

                        spawnCount--;
                    }
                }
            }
        }
    }

    private void SpwanOther(RacingEnemyType etype, float xPos, float speed)
    {
        Entity enemy;
        if (etype == RacingEnemyType.Car)
        {
            enemy = GetSingleton<PrefabEntityComponent>().pfCar;
        }
        else
        {
            enemy = GetSingleton<PrefabEntityComponent>().pfPipe;
        }
        Entity enemyEntity = EntityManager.Instantiate(enemy);

        EntityManager.SetComponentData(enemyEntity, new Translation
        {
            Value = new float3(xPos, spawnYPosition, 0f)
        });

        EntityManager.SetComponentData(enemyEntity, new MoveSpeed
        {
            moveDirSpeed = new float3(0, -speed, 0)
        });
        EntityManager.SetComponentData(enemyEntity, new RacingEnemy
        {
            enemyType = etype
        });

        //TODO: TO BE DONE LATER
        //float pipeWidth = .8f;
        //EntityManager.SetComponentData(pipeBodyEntity, new NonUniformScale
        //{
        //    Value = new float3(pipeWidth, height, 0f)
        //});

        PhysicsCollider physicsCollider = EntityManager.GetComponentData<PhysicsCollider>(enemyEntity);
        float colliderOffsetWidth;
        float colliderOffsetHeight;
        if (etype == RacingEnemyType.Pipe)
        {
            colliderOffsetWidth = 0.1f;
            colliderOffsetHeight = 0.2f;
        }
        else
        {
            colliderOffsetWidth = 0.1f;
            colliderOffsetHeight = 0.1f;
        }
        BlobAssetReference<Collider> collider = BoxCollider.Create(new BoxGeometry
        {
            Size = new float3(colliderOffsetWidth, colliderOffsetHeight, 1f),
            Center = new float3(0f, 0f, 0f),
            Orientation = quaternion.identity,
            BevelRadius = 0f
        }, physicsCollider.Value.Value.Filter, new Material { Flags = Material.MaterialFlags.IsTrigger }); ;

        EntityManager.SetComponentData(enemyEntity, new PhysicsCollider
        {
            Value = collider,
        });
    }

    private void TestEnemyGapDifficulty() {
        //UnityEngine.Debug.Log(spawnedEnemyCount);

        switch (counter) {
            case 20:
                randomEnemySpeedMin = 4.5f;
                randomEnemySpeedMax = 5.5f;
                randomCarSpeedMin = 6f;
                randomCarSpeedMax = 8f;
                break;
            case 40:
                randomEnemySpeedMin = 5.5f;
                randomEnemySpeedMax = 6.5f;
                randomCarSpeedMin = 7f;
                randomCarSpeedMax = 10f;
                break;
            case 60:
                randomEnemySpeedMin = 6.5f;
                randomEnemySpeedMax = 7.5f;
                randomCarSpeedMin = 9f;
                randomCarSpeedMax = 11f;
                break;
        }
    }

}