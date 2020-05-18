using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public class CubeMultiple : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject referPrefab;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        conversionSystem.DeclareLinkedEntityGroup(this.gameObject);
        Entity additional1 = conversionSystem.CreateAdditionalEntity(this.gameObject);
        dstManager.SetName(additional1, $"{this.name}_Add1");
        Entity additional2 = conversionSystem.CreateAdditionalEntity(this.gameObject);
        dstManager.SetName(additional2, $"{this.name}_Add2");

        conversionSystem.GetPrimaryEntity(referPrefab);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(referPrefab);
    }
}
