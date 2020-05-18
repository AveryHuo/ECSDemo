using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class Spawner : MonoBehaviour
{
    [SerializeField] private Mesh unitMesh;
    [SerializeField] private Material unitMat;
    [SerializeField]  private GameObject gameObjectPrefab;
    [SerializeField] private int xSize = 10;
    [SerializeField] private int ySize = 10;
    [Range(0.1f,2f)]
    [SerializeField] private float spacing = 1f;

    private World defaultWorld;
    private Entity entityPrefab;
    private EntityManager entityManager;
    private EntityArchetype entityArcheType;

    private BlobAssetStore blobAssetStore;
    void Start()
    {
        InitEntityManager();
        CreateArcheType();

        blobAssetStore = new BlobAssetStore();
        InstantiateEntityGrid(xSize, ySize, spacing);
    }

    void InitEntityManager()
    {
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }


    void CreateArcheType()
    {
        var translationType = ComponentType.ReadWrite<Translation>();
        var scaleType = ComponentType.ReadWrite<Scale>();
        var rotationType = ComponentType.ReadWrite<Rotation>();
        var renderMeshType = ComponentType.ReadWrite<RenderMesh>();
        var renderBoundsType = ComponentType.ReadWrite<RenderBounds>();
        var localToWorldType = ComponentType.ReadWrite<LocalToWorld>();
        entityArcheType = entityManager.CreateArchetype( translationType, scaleType, renderMeshType, renderBoundsType,  rotationType,  localToWorldType);
    }


    private void InstantiateEntity(float3 position)
    {
        if (entityManager == null)
        {
            return;
        }

        Entity myEntity = CreateEntity();
        entityManager.AddComponentData(myEntity, new MoveBySpeedData()
        {
            Value = 1.0f
        });
        entityManager.AddComponentData(myEntity, new WaveData()
        {
            amplitude = 10,
            xOffset = 0.25f,
            yOffset = 0.25f
        });
        
        entityManager.SetComponentData(myEntity, new Translation
        {
            Value = position
        });
    }

    private void InstantiateEntityGrid(int dimX, int dimY, float spacing = 1f)
    {
        for (int i = 0; i < dimX; i++)
        {
            for (int j = 0; j < dimY; j++)
            {
                InstantiateEntity(new float3(i * spacing, j * spacing, 0f));
            }
        }
    }



    Entity CreateEntity()
    {
        Entity hero = entityManager.CreateEntity(entityArcheType);
        entityManager.AddComponentData(hero, new Scale()
        {
            Value = 1
        });
        entityManager.AddSharedComponentData(hero, new RenderMesh()
        {
            mesh = unitMesh,
            material = unitMat
        });
        return hero;
    }

    Entity CreateEntity2()
    {
        //BlobAssetStore blobAssetStore = new BlobAssetStore();
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, blobAssetStore);
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);
        //bas.Dispose();

        Entity hero = entityManager.Instantiate(entityPrefab);
        entityManager.AddComponentData(hero, new BoneIndexOffset());
        return hero;
    }

    Entity CreateEntity3()
    {
        //BlobAssetStore blobAssetStore = new BlobAssetStore();
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, blobAssetStore);
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);
        //bas.Dispose();

        Entity hero = entityManager.Instantiate(entityPrefab);
        entityManager.AddComponentData(hero, new BoneIndexOffset());
        return hero;
    }

    private void OnDestroy()
    {
        // Dispose of the BlobAssetStore, else we're get a message:
        // A Native Collection has not been disposed, resulting in a memory leak.
        if (blobAssetStore != null) { blobAssetStore.Dispose(); }
    }
}
