using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct Tag_Tester: IComponentData
{
    public Entity thatPrefabEntity;
}

public class Tester : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject forDeclare;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity declaredEntity = conversionSystem.GetPrimaryEntity(forDeclare);
        dstManager.AddComponentData<Tag_Tester>(entity, new Tag_Tester() { thatPrefabEntity = declaredEntity });
        //var spawnPosition = dstManager.GetComponentData<Translation>(entity);

        //Entity instantiated = dstManager.Instantiate(declaredEntity);
        //dstManager.SetComponentData<Translation>(instantiated, spawnPosition);

    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(forDeclare);
    }
}
