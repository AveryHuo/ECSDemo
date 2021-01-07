using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayableECSManager : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject prefab;
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        
        referencedPrefabs.Add(prefab);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var comp = new PlayerSpawnTag
        {
            prefab = conversionSystem.GetPrimaryEntity(prefab)
        };
        dstManager.AddComponentData(entity, comp);
    }
}

// public class PlayableECSManager : MonoBehaviour
// {
//     public GameObject obj;
//
//     public void Start()
//     {
//         World defaultWorld = World.DefaultGameObjectInjectionWorld;;
//         GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
//         var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(obj, settings);
//         var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//         
//         // Efficiently instantiate a bunch of entities from the already converted entity prefab
//         var instance = entityManager.Instantiate(entityPrefab);
//
//         // Place the instantiated entity in a grid with some noise
//         var position = transform.TransformVector(new float3(1.0f,1.0f,1.0f));
//         entityManager.SetComponentData(instance, new Translation {Value = position});
//     }
// }

// public class PlayableECSManager : MonoBehaviour
// {
//     public GameObject prefab;
//     private PlayableSystem playableSys;
//     public void Start()
//     {
//         var newPrefab = GameObject.Instantiate(prefab);
//         newPrefab.transform.position = Vector3.zero;
//         
//         AEntityManager.Instance.RegisterPrefabToDefaultWorld(newPrefab);
//         playableSys = AEntityManager.Instance.DefWorld.GetOrCreateSystem<PlayableSystem>();
//         AEntityManager.Instance.DefWorld.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(playableSys);
//     }
// }
