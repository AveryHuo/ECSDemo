using Test;
using Unity.Entities;
using UnityEngine;

namespace ECS
{
    public class WDApplication : MonoBehaviour
    {
        public Material MainMaterial;
        public Mesh MainMesh;
        private void Start()
        {
            DefaultWorldInitialization.Initialize("MyWorld", false);
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new SpawnComponent
            {
                XSize = 80,
                YSize = 80,
                Spacing = 1
            });
            

            var entityData = entityManager.CreateEntity();
            entityManager.AddSharedComponentData(entityData, new RenderDataComponent
            {
                MeshValue = MainMesh,
                MaterialValue = MainMaterial,
            });
        }
    }
}