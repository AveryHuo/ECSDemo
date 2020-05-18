using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpecialCube : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject itsChild;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //conversionSystem.DeclareLinkedEntityGroup(this.gameObject);
        dstManager.AddComponent<LinkedEntityGroup>(entity);
        var leg = dstManager.GetBuffer<LinkedEntityGroup>(entity);
        leg.Add(conversionSystem.GetPrimaryEntity(itsChild)); 
    }
}