using Test;
using Unity.Entities;
using UnityEngine;

namespace ECS
{
    public class WDApplication : MonoBehaviour
    {
        public Material MainMaterial;
        public Mesh MainMesh;
        [SerializeField] private int xSize = 10;
        [SerializeField] private int ySize = 10;
        [Range(0.1f, 2f)]
        [SerializeField] private float spacing = 1f;
        private void Start()
        {
            DefaultWorldInitialization.Initialize("MyWorld", false);
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new SpawnComponent
            {
                XSize = xSize,
                YSize = ySize,
                Spacing = spacing
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