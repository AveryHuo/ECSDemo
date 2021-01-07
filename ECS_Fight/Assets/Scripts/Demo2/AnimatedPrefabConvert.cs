using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AnimatedPrefabConvert : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject animObj;
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(animObj);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var animPrefab = conversionSystem.TryGetPrimaryEntity(animObj);
        var animCom = animObj.GetComponent<Animator>();
        dstManager.AddComponentData<AnimatorComponent>(entity, new AnimatorComponent()
        {
        });
    }
}

