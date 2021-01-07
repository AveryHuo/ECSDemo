using System.Runtime.CompilerServices;
using Unity.Entities;

public static class EntityExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetComponentData<T>(this Entity entity, World world = null) where T : struct, IComponentData
    {
        if (world == null)
        {
            world = World.DefaultGameObjectInjectionWorld;
        }

        return world.EntityManager.GetComponentData<T>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddComponent<T>(this Entity entity, World world = null) where T : struct, IComponentData
    {
        if (world == null)
        {
            world = World.DefaultGameObjectInjectionWorld;
        }

        world.EntityManager.AddComponent(entity, ComponentType.ReadWrite<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddComponentData<T>(this Entity entity, T componentData, World world = null) where T : struct, IComponentData
    {
        if (world == null)
        {
            world = World.DefaultGameObjectInjectionWorld;
        }

        world.EntityManager.AddComponentData(entity, componentData);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetComponentData<T>(this Entity entity, T componentData, World world = null) where T : struct, IComponentData
    {
        if (world == null)
        {
            world = World.DefaultGameObjectInjectionWorld;
        }

        world.EntityManager.SetComponentData(entity, componentData);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasComponent<T>(this Entity entity, World world = null) where T : struct, IComponentData
    {
        if (world == null)
        {
            world = World.DefaultGameObjectInjectionWorld;
        }

        return world.EntityManager.HasComponent<T>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveComponent<T>(this Entity entity, World world = null) where T : struct, IComponentData
    {
        if (world == null)
        {
            world = World.DefaultGameObjectInjectionWorld;
        }

        world.EntityManager.RemoveComponent<T>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DestroyEntity(this Entity entity, World world = null)
    {
        if (world == null)
        {
            world = World.DefaultGameObjectInjectionWorld;
        }

        world.EntityManager.DestroyEntity(entity);
    }

    public static void DestroyLater(this Entity entity, EntityCommandBuffer buffer)
    {
        buffer.DestroyEntity(entity);
    }
}