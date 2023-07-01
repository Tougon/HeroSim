using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Represents the absolute base form of an object. This is used for effects.
/// </summary>
public class EntityBase : MonoBehaviour
{
    // Awake is called before the first frame update
    protected virtual void Awake(){ }

    // Initialization specific to an Entity
    public virtual void Init() { }

    // Initialization involving the values in an EntityController
    public virtual void Init(EntityController ec) { }


    #region Animation Control

    /// <summary>
    /// Sets animation to the given trigger
    /// </summary>
    public virtual void SetAnimation(string val){ }


    /// <summary>
    /// Sets animation to the given trigger with the given bool
    /// </summary>
    public virtual void SetAnimationState(string val, bool b) { }


    /// <summary>
    /// Sets an entity's sprite
    /// </summary>
    public virtual void SetSprite(Sprite s) { }


    /// <summary>
    /// Starts a color tween
    /// </summary>
    public virtual void SetColorTween(Color c, float amt, float duration){ }


    /// <summary>
    /// Starts an overlay tween
    /// </summary>
    public virtual void SetOverlayTween(float amt, Vector2 speed, float duration){ }


    /// <summary>
    /// Sets the overlayed texture to the given texture
    /// </summary>
    public virtual void SetOverlayTexture(Texture t, Vector2 tiling){ }


    /// <summary>
    /// Returns material used by something
    /// </summary>
    public virtual Material GetMaterial(){ return null; }


    /// <summary>
    /// Changes the speed of...something. Could be particle speed, could be animator speed, who knows?
    /// </summary>
    public virtual void FrameSpeedModify(float t){ }
    #endregion
}
