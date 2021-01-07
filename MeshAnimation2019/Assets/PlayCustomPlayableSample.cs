using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayCustomPlayableSample : MonoBehaviour
{
    public AnimationClip[] clipsToPlay;

    PlayableGraph m_Graph;

    void Start()
    {
        m_Graph = PlayableGraph.Create();
        var custPlayable = ScriptPlayable<CustomPlayable>.Create(m_Graph);

        var playQueue = custPlayable.GetBehaviour();

        playQueue.Initialize(clipsToPlay, custPlayable, m_Graph);

        var playableOutput = AnimationPlayableOutput.Create(m_Graph, "Animation", GetComponent<Animator>());

        playableOutput.SetSourcePlayable(custPlayable);
        playableOutput.SetSourceInputPort(0);

        m_Graph.Play();
    }

    
    
    void OnDisable()

    {

        // Destroys all Playables and Outputs created by the graph.

        m_Graph.Destroy();

    }
}
