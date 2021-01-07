using Unity.Entities;
using Unity.Rendering;
 
namespace Game.Systems.Initialization {
    /// <summary>
    /// Suppresses the error: "ArgumentException: A component with type:BoneIndexOffset has not been added to the entity.", until the Unity bug is fixed.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DisableCopySkinnedEntityDataToRenderEntitySystem : ComponentSystem {
        protected override void OnCreate() {
            World.GetOrCreateSystem<CopySkinnedEntityDataToRenderEntity>().Enabled = false;
        }
 
        protected override void OnUpdate() {}
    }
}