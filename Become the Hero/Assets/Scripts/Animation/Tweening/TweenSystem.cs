using DOTweenConfigs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

public class TweenSystem : SerializedMonoBehaviour
{
    [System.Serializable]
    public class TweenData
    {
        public string animName;
        public string nextAnimName;
        public TweenState tweenState;

        [Space(20)]
        public BetterEvent OnCompleteStatic = new BetterEvent();
    }

    public delegate void OnAnimationCompletedSignature();
    private OnAnimationCompletedSignature OnAnimationCompletedDelegate;

    [SerializeField]
    [InfoBox("Not all Target values need to be assigned.")]
    private TweenActionCache target;

    [Space(20)]
    public List<TweenData> animationList = new List<TweenData>();
    private Dictionary<string, TweenData> animationCache = new Dictionary<string, TweenData>();

    [SerializeField]
    [Header("Initializiation")]
    private bool useStartAnimation;
    [SerializeField] [ShowIf("useStartAnimation")]
    private string startAnimation;

    private TweenData currentTweenData;
    private TweenStateInstance playbackInstance;


    void Awake()
    {
        foreach(var anim in animationList)
        {
            animationCache.Add(anim.animName.ToLower(), anim);
        }

        if (useStartAnimation)
        {
            PlayAnimation(startAnimation);
        }
    }


    /// <summary>
    /// Attempts to play back an animation with the given name
    /// </summary>
    public void PlayAnimation(string animName, OnAnimationCompletedSignature callback = null)
    {
        animName = animName.ToLower();

        if(string.IsNullOrEmpty(animName))
        {
#if UNITY_EDITOR
            Debug.LogWarning("Cannot play animation. Provided key is null or empty.");
#endif
            return;
        }

        if(animationCache.TryGetValue(animName, out TweenData data))
        {
            PlayAnimation_Internal(data, callback);
        }
        else
        {
            callback?.Invoke();
#if UNITY_EDITOR
            Debug.LogWarning($"Cannot play animation. {animName} does not exist in the cache.");
#endif
        }
    }


    public void PlayAnimation(TweenData data)
    {
        PlayAnimation_Internal(data, null);
    }


    private void PlayAnimation_Internal(TweenData data, OnAnimationCompletedSignature callback)
    {
        currentTweenData = null;

        // Stop existing playback if any
        playbackInstance?.StopInstance();

        // Reset the animation delegate
        if (callback != null)
            OnAnimationCompletedDelegate = new OnAnimationCompletedSignature(callback);
        else
            callback = null;

        currentTweenData = data;
        playbackInstance = currentTweenData.tweenState.ExecuteState(target, OnAnimationCompleted);
    }


    public void StopAnimation(bool bComplete = false)
    {
        // Stop existing playback if any
        playbackInstance?.StopInstance(bComplete);
    }


    private void OnAnimationCompleted()
    {
        OnAnimationCompletedDelegate?.Invoke();

        if(currentTweenData != null)
        {
            currentTweenData.OnCompleteStatic.Invoke();

            if(!string.IsNullOrEmpty(currentTweenData.nextAnimName))
                PlayAnimation(currentTweenData.nextAnimName);
        }
    }
}