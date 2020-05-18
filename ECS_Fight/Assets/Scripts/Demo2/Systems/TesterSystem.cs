using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class TesterSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entities.ForEach(
                (Entity e, in Translation t, in Tag_Tester she) =>
                {
                    int count = 15;
                    for (int i = 0; i < count; i++)
                    {
                        Entity instantiated = EntityManager.Instantiate(she.thatPrefabEntity);
                        //Set to the same translation.
                        EntityManager.SetComponentData(instantiated, new Translation()
                        {
                            Value = new float3(10.0f +i, 0.0f+i, 0.0f)
                        });
                        EntityManager.RemoveComponent<Tag_Tester>(e);
                    }
                    
                })
            .WithStructuralChanges().Run();
        return default;
    }
}