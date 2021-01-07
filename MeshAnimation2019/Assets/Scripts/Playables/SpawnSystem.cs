using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnSystem : SystemBase
{
    protected override void OnCreate()
    {
        GetEntityQuery(ComponentType.ReadOnly<PlayerSpawnTag>());
    }
    
    protected override void OnUpdate()
    {
        Entities.WithStructuralChanges().ForEach((Entity entity, in PlayerSpawnTag spawnerFromEntity, in LocalToWorld location) =>
        {
            var entityManager = AEntityManager.Instance.DefManager;
            var instance = entityManager.Instantiate(spawnerFromEntity.prefab);
            
            // Place the instantiated in a grid with some noise
            var position = math.transform(location.Value,
                new float3(0.0f, 0.0f, 0.0f));
            entityManager.SetComponentData(instance, new Translation {Value = position});
            
            entityManager.DestroyEntity(spawnerFromEntity.prefab);
            entityManager.DestroyEntity(entity);
        }).Run();
    }

}
