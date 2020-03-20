using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Test
{
    public class SpawnSystem : ComponentSystem
    {
        private EntityQuery _updateQuery;
        private EntityArchetype _entityArchetype;
        protected override void OnCreate()
        {
            base.OnCreate();
            _updateQuery = GetEntityQuery(ComponentType.ReadOnly<SpawnComponent>());
            RequireForUpdate(_updateQuery);
            _entityArchetype = EntityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(LocalToWorldComponent),
                typeof(FaceComponent),
                typeof(MoveBySpeedData),
                typeof(WaveData)
            });
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity,ref SpawnComponent spawn) =>
            {
                //int count = spawn.Value;
                //var allEntities = EntityManager.CreateEntity(_entityArchetype, count, Allocator.Temp);

                for (int i = 0; i < spawn.XSize; i++)
                {
                    for (int j = 0; j < spawn.YSize; j++)
                    {
                        var unit = EntityManager.CreateEntity(_entityArchetype);
                        EntityManager.SetComponentData(unit, new PositionComponent
                        {
                            Value = new float3(i * spawn.Spacing, j * spawn.Spacing,0 ),
                        });
                        EntityManager.SetComponentData(unit, new MoveBySpeedData()
                        {
                            Value = 1.0f
                        });
                        EntityManager.SetComponentData(unit, new WaveData()
                        {
                            amplitude = 10,
                            xOffset = 0.25f,
                            yOffset = 0.25f
                        });
                    }
                }

                //var random = new Random((uint)UnityEngine.Time.frameCount * 100);
                //foreach (var unit in allEntities)
                //{
                //    //var posi = random.NextFloat2(-10f,10f);
                //    //EntityManager.SetComponentData<PositionComponent>(unit, new PositionComponent
                //    //{
                //    //    Value = new Vector3(posi.x, 0, posi.y),
                //    //});
                //    EntityManager.SetComponentData(unit, new MoveBySpeedData()
                //    {
                //        Value = 1.0f
                //    });
                //    EntityManager.SetComponentData(unit, new WaveData()
                //    {
                //        amplitude = 10,
                //        xOffset = 0.25f,
                //        yOffset = 0.25f
                //    });
                //}

                EntityManager.RemoveComponent<SpawnComponent>(entity);
            });
        }
    }
}