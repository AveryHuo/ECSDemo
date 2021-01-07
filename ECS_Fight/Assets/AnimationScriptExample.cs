using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Animation;
using UnityEngine.Animations;

public class AnimationScriptExample : MonoBehaviour
{
    public GameObject gameObjectPrefab;
    private World defaultWorld;
    private EntityManager entityManager;
    private BlobAssetStore blobAssetStore;
    private Entity entityPrefab;
    public void Start()
    {
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = defaultWorld.EntityManager;

        blobAssetStore = new BlobAssetStore();
        blobAssetStore.ResetCache(true);

        CreateEntity3();
    }
    Entity CreateEntity3()
    {
        
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, blobAssetStore);
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);
        
        Entity hero = entityManager.Instantiate(entityPrefab);
        hero.SetComponentData<Rotation>(new Rotation()
        {
            Value = quaternion.RotateY(280)
        });

        hero.SetComponentData<Translation>(new Translation()
        {
            Value = new float3(0, 0, -7.0f)
        });
        entityPrefab.DestroyEntity();


        return hero;
    }


    public void OnDestroy()
    {
        blobAssetStore.Dispose();
    }
}