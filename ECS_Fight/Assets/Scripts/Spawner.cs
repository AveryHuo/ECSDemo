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

    bool bSet = false;
    private World defaultWorld;
    private Entity entityPrefab;
    private EntityManager entityManager;
    private EntityArchetype entityArcheType;

    void Start()
    {
        InitEntityManager();
        CreateArcheType();

        var settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);
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
        entityArcheType = entityManager.CreateArchetype( translationType, scaleType, rotationType, renderMeshType,renderBoundsType, localToWorldType);
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

    // create a single Entity using the Conversion Workflow
    private void ConvertToEntity(float3 position)
    {
        if (entityManager == null)
        {
            Debug.LogWarning("ConvertToEntity WARNING: No EntityManager found!");
            return;
        }

        if (gameObjectPrefab == null)
        {
            Debug.LogWarning("ConvertToEntity WARNING: Missing GameObject Prefab");
            return;
        }

        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
        entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);
        
        Entity myEntity = entityManager.Instantiate(entityPrefab);
        entityManager.AddComponentData(myEntity, new BoneIndexOffset());
        entityManager.SetComponentData(myEntity, new Translation
        {
            Value = position
        });
    }


    Entity CreateEntity()
    {
        //GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(defaultWorld, null);
        //entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObjectPrefab, settings);

        //Entity hero = entityManager.Instantiate(entityPrefab);
        Entity hero = entityManager.CreateEntity(entityArcheType);
        //Matrix4x4[] matrixList = new Matrix4x4[1023];
        //Graphics.DrawMeshInstanced(unitMesh, 0, unitMat, matrixList);
        entityManager.SetComponentData(hero, new Scale()
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
}
