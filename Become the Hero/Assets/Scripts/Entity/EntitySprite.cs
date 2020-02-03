using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Represents the base form of an object with a sprite renderer component
/// </summary>
public class EntitySprite : EntityBase
{
    protected Animator anim;
    protected SpriteRenderer sprite;
    protected Material mat;

    // Awake is called before the first frame update
    protected override void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        // Instances the material
        mat = sprite.material;
    }


    #region Animation Control

    /// <summary>
    /// Sets animation to the given trigger
    /// </summary>
    public override void SetAnimation(string val)
    {
        anim.SetTrigger(val);
    }


    /// <summary>
    /// Sets animation to the given trigger with the given bool
    /// </summary>
    public override void SetAnimationState(string val, bool b)
    {
        if (val.Equals("Visible"))
            sprite.enabled = b;
        else
            anim.SetBool(val, b);
    }


    /// <summary>
    /// Starts a color tween
    /// </summary>
    public override void SetColorTween(Color c, float amt, float duration)
    {
        sprite.DOColor(c, duration);
        mat.DOFloat(amt, "_Amount", duration);
    }


    /// <summary>
    /// Returns sprite renderer component
    /// </summary>
    public SpriteRenderer GetSpriteRenderer()
    {
        return sprite;
    }


    /// <summary>
    /// Returns material used by the sprite renderer
    /// </summary>
    public override Material GetMaterial()
    {
        return mat;
    }


    /// <summary>
    /// Changes the speed of the animator
    /// </summary>
    public override void FrameSpeedModify(float t)
    {
        anim.speed = t;
    }
    #endregion
}
