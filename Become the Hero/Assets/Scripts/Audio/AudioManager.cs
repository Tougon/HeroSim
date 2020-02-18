using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Uses tweening for fading out audio
using DG.Tweening;

/// <summary>
/// Script that handles audio queueing and playback.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [SerializeField]
    protected int numAudioSources = 5;

    [SerializeField]
    protected AudioCollection sfxCollection;
    [SerializeField]
    protected AudioCollection bgmCollection;
    
    protected List<AudioSource> sfxSources;
    protected AudioSource bgmSource;


    // Start is called before the first frame update
    protected virtual void Awake()
    {
        sfxSources = new List<AudioSource>(numAudioSources);

        GameObject sfxParent = new GameObject("SFX");
        sfxParent.transform.SetParent(transform, false);

        for(int i=0; i<numAudioSources; i++)
        {
            GameObject sfxChild = new GameObject("Sound Source " + (i + 1));
            sfxChild.transform.SetParent(sfxParent.transform, false);
            sfxSources.Add(sfxChild.AddComponent<AudioSource>());
        }

        GameObject bgm = new GameObject("BGM");
        bgm.transform.SetParent(transform, false);
        bgmSource = bgm.AddComponent<AudioSource>();
        bgmSource.loop = true;
    }


    protected AudioClip GetClip(AudioCollection col, string key)
    {
        return col.GetClip(key);
    }


    protected AudioClip GetRandomClip(AudioCollection col)
    {
        return col.GetRandomClip();
    }


    public void PlayBGM(AudioClip newBGM)
    {
        if (newBGM == null)
            return;
        
        bgmSource.clip = newBGM;
        bgmSource.volume = 1.0f;
        bgmSource.Play();
    }
}
