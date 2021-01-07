using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

    public class AEntityManager
    {
        private static AEntityManager instance;
        public static AEntityManager Instance {
            get
            {
                if(instance == null)
                {
                    instance = new AEntityManager();
                    instance.Init();
                }
                return instance;
            }
        }

        public World DefWorld;
        public EntityManager DefManager;

        public World CacheWorld;
        
        public void Init()
        {
            DefWorld = World.DefaultGameObjectInjectionWorld;
            DefManager = DefWorld.EntityManager;
        }

        public void RegisterPrefabToDefaultWorld(GameObject obj)
        {
            if (DefManager == null || DefWorld == null)
                return;
            
            GameObjectEntity.AddToEntityManager(DefManager, obj);
        }

        #region Cache World
        public Entity CreateEntityInCache()
        {
            if(CacheWorld == null)
                CacheWorld = new World("Cache");
            return CacheWorld.EntityManager.CreateEntity();
        }

        public void AddComponentToCache<T>(Entity entity) where T : struct, IComponentData
        {
            CacheWorld.EntityManager.AddComponent<T>(entity);
        }

        public void AddComponentDataToCache<T>(Entity entity, T t) where T : struct, IComponentData
        {
            CacheWorld.EntityManager.AddComponentData<T>(entity, t);
        }

        #endregion
    }
