using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Represents the absolute base form of an object. This is used for effects.
/// </summary>
public class EntityBase : MonoBehaviour
{
    protected Animator anim;
    protected SpriteRenderer sprite;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }


    #region Animation Control

    /// <summary>
    /// Sets animation to the given trigger
    /// </summary>
    public void SetAnimation(string val)
    {
        anim.SetTrigger(val);
    }


    /// <summary>
    /// Sets animation to the given trigger with the given bool
    /// </summary>
    public void SetAnimationState(string val, bool b)
    {
        anim.SetBool(val, b);
    }


    /// <summary>
    /// Starts a color tween
    /// </summary>
    public void SetColorTween(Color c, float duration)
    {
        sprite.DOColor(c, duration);
    }


    /// <summary>
    /// Returns sprite renderer component
    /// </summary>
    public SpriteRenderer GetSpriteRenderer()
    {
        return sprite;
    }


    /// <summary>
    /// Changes the speed of the animator
    /// </summary>
    public void FrameSpeedModify(float t)
    {
        anim.speed = t;
    }
    #endregion
}
