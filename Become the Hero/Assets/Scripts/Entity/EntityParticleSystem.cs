using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Represents the base form of an object with a particle rendererer component
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class EntityParticleSystem : EntityBase
{
    [SerializeField]
    protected bool emitFromEntity = false;

    protected ParticleSystem particle;
    protected ParticleSystem.MainModule particleMain;
    protected ParticleSystem.ShapeModule particleShape;
    protected ParticleSystemRenderer rend;

    // Awake is called before the first frame update
    protected override void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>();
        particleMain = particle.main;
        particleShape = particle.shape;

        // Instances the material
        rend = GetComponentInChildren<ParticleSystemRenderer>();
    }


    public override void Init(EntityController ec)
    {
        if (emitFromEntity)
            particleShape.sprite = ec.GetSpriteRenderer().sprite;
    }


    #region Animation Control

    /// <summary>
    /// Sets animation to the given trigger with the given bool
    /// </summary>
    public override void SetAnimationState(string val, bool b)
    {
        if (val.Equals("Visible"))
        {
            if (b)
                particle.Play();
            else
                particle.Stop();
        }
    }


    /// <summary>
    /// Returns material used by the sprite renderer
    /// </summary>
    public override Material GetMaterial()
    {
        return rend.material;
    }


    /// <summary>
    /// Changes the speed of the animator
    /// </summary>
    public override void FrameSpeedModify(float t)
    {
        particleMain.simulationSpeed = t;
    }
    #endregion
}
