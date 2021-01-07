using UnityEngine;

using UnityEngine.Playables;

using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;

[RequireComponent(typeof(Animator))]

public class PlayAnimationSample : MonoBehaviour

{

    public AnimationClip clip0;
    public AnimationClip clip1;
    
    PlayableGraph playableGraph;
    private AnimationMixerPlayable m_Mixer;

    public float tranTime = 2;
    private float leftTime = 0;
    
    void Start()

    {

        playableGraph = PlayableGraph.Create("测试");

        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        // Wrap the clip in a playable

        m_Mixer = AnimationMixerPlayable.Create(playableGraph, 2);
        
        AnimationClipPlayable clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);
        AnimationClipPlayable clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);

        playableGraph.Connect(clipPlayable0, 0, m_Mixer, 0);
        playableGraph.Connect(clipPlayable1, 0, m_Mixer, 1);
        
        m_Mixer.SetInputWeight(0,1);
        m_Mixer.SetInputWeight(1,0);
        // Connect the Playable to an output

        playableOutput.SetSourcePlayable(m_Mixer);
        playableOutput.SetSortingOrder(0);
            
        // Plays the Graph.
        leftTime = tranTime;
        playableGraph.Play();

    }

    void Update()
    {
        leftTime = Mathf.Clamp(leftTime - Time.deltaTime, 0, 2);
        float weight = leftTime / tranTime;
        Debug.Log("weight:"+weight);
        m_Mixer.SetInputWeight(0, 1 - weight);
        m_Mixer.SetInputWeight(1, weight);
    }
    
    void OnDestroy()
    {
        playableGraph.Destroy();
    }

}