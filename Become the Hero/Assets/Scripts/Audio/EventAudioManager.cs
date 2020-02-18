using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAudioManager : AudioManager
{
    [SerializeField]
    private string playOnAwake = "random";

    protected override void Awake()
    {
        base.Awake();

        if (playOnAwake.Equals("random"))
            PlayBGM(GetRandomClip(bgmCollection));
        else
            PlayBGM(GetClip(bgmCollection, playOnAwake));
    }

}
