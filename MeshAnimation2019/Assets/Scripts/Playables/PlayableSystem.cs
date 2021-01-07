
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[DisableAutoCreation]
public class PlayableSystem : SystemBase
{
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer();
        
        Entities.WithoutBurst()
            .ForEach((Entity entity, in ReferenceGroupCollector rc, in PlayerTag playerTag) =>
        {
            var playerAnim = rc.Get<GameObject>("AnimatorObj").GetComponent<Animator>();
            
            PlayWithPlayable(playerAnim);
            
            commandBuffer.RemoveComponent(entity, typeof(PlayerTag));
                
        }) .Run();
    }

    public void PlayWithPlayable(Animator anim)
    {
        List<AnimationClip> clipsToPlay = new List<AnimationClip>();
        clipsToPlay.Add(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Models/PlayerCq/PlayerCqCreate/Motion/create_appear.anim"));
        PlayableGraph m_Graph = PlayableGraph.Create();
        var custPlayable = ScriptPlayable<CustomPlayable>.Create(m_Graph);

        var playQueue = custPlayable.GetBehaviour();

        playQueue.Initialize(clipsToPlay.ToArray(), custPlayable, m_Graph);

        var playableOutput = AnimationPlayableOutput.Create(m_Graph, "Animation", anim);

        playableOutput.SetSourcePlayable(custPlayable);
        playableOutput.SetSourceInputPort(0);

        m_Graph.Play();
    }

}
