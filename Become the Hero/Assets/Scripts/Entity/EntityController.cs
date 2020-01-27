using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Houses an <see cref="Entity"/> and allows for gameplay operations to be performed on it.
/// </summary>
public class EntityController : EntityBase, IComparable<EntityController>
{
    protected TurnManager turnManger;

    [SerializeField]
    protected Entity current;
    public EntityController target { get; set; }
    public EntityParams param { get; private set; }

    public Spell action { get; protected set; }
    public SpellCast actionResult { get; set; }
    public bool dead { get; protected set; }
    
    public int damageTaken { get; private set; }
    // Used for UI interactions
    public int lastHit { get; private set; }
    public int maxHP { get; private set; }
    public int maxMP { get; private set; }

    // Stat modification
    private int atkStage = 0;
    private int defStage = 0;
    private int spdStage = 0;
    private int evasionStage = 0;
    private int accuracyStage = 0;

    private Dictionary<string, float> offenseModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> defenseModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> speedModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> accuracyModifiers = new Dictionary<string, float>();
    private List<EffectInstance> effects = new List<EffectInstance>();
    private List<EffectInstance> properties = new List<EffectInstance>();

    protected static AnimationSequenceObject spawn;
    protected static AnimationSequenceObject defeat;

    [SerializeField]
    protected EntityControllerUI entityUI;


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        InitEntityController();

        if(spawn == null)
            spawn = (Resources.Load("Animation/Appear", typeof(AnimationSequenceObject))) as AnimationSequenceObject;

        if (defeat == null)
            defeat = (Resources.Load("Animation/Defeat", typeof(AnimationSequenceObject))) as AnimationSequenceObject;
    }


    /// <summary>
    /// Initializes an entity controller
    /// </summary>
    public void InitEntityController()
    {
        if (current != null)
        {
            // Set the params to the entity's params
            param = new EntityParams();
            param.entityHP = ((current.vals.entityHP * 2 * Spell.DAMAGE_CONSTANT) / 100) + Spell.DAMAGE_CONSTANT + 10;
            param.entityMP = current.vals.entityMP;
            param.entityAtk = ((current.vals.entityAtk * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;
            param.entityDef = ((current.vals.entityDef * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;
            param.entityName = current.vals.entityName;
            param.entitySpeed = ((current.vals.entitySpeed * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;

            maxHP = param.entityHP;
            maxMP = param.entityMP;

            // Reset stats and effects
            atkStage = 0;
            defStage = 0;
            spdStage = 0;
            evasionStage = 0;
            accuracyStage = 0;

            effects = new List<EffectInstance>();
            properties = new List<EffectInstance>();
            offenseModifiers = new Dictionary<string, float>();
            defenseModifiers = new Dictionary<string, float>();
            accuracyModifiers = new Dictionary<string, float>();

            if (entityUI != null)
                entityUI.ResetUI(this, maxHP, maxMP);
        }
    }


    /// <summary>
    /// Sets an entity to the given entity
    /// </summary>
    public void SetEntity(Entity e)
    {
        current = e;
        InitEntityController();
        dead = false;

        Hero.Core.Sequence spawnSeq = new AnimationSequence(spawn, this, this);
        EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, spawnSeq);
    }


    /// <summary>
    /// Apply damage to this entity
    /// </summary>
    public void ApplyDamage(int val, bool crit)
    {
        // If dead, do not apply damage
        if (dead) return;

        param.entityHP -= val;
        lastHit = val;

        if (param.entityHP <= 0)
        {
            OnDeath();
            dead = true;
        }

        if(crit || dead)
            sprite.gameObject.transform.DOShakePosition(0.26f, new Vector3(0.59f, 0.0f, 0.0f), 300, 90, false, false);
        else
            sprite.gameObject.transform.DOShakePosition(0.26f, new Vector3(0.34f, 0.0f, 0.0f), 200, 90, false, false);
    }


    // Damage taken is reset at the beginning of every turn
    public void ResetDamageTaken() { damageTaken = 0; }
    public void IncreaseDamageTaken(int val) { damageTaken += val; }


    /// <summary>
    /// Modify MP by the given amount
    /// </summary>
    public void ModifyMP(int amt)
    {
        int deltaMP = param.entityMP;
        param.entityMP += amt;

        param.entityMP = Mathf.Clamp(param.entityMP, 0, maxMP);
        deltaMP -= param.entityMP;

        if (entityUI != null && deltaMP != 0)
            entityUI.ChangeMP(amt);
    }


    /// <summary>
    /// Skeleton function for death handling
    /// </summary>
    protected virtual void OnDeath()
    {
        // Play a death animation that will just be a dissolve or something
        Hero.Core.Sequence defSeq = new AnimationSequence(defeat, this, this);
        EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, defSeq);

        offenseModifiers.Clear();
        defenseModifiers.Clear();
        accuracyModifiers.Clear();

        atkStage = 0;
        defStage = 0;
        spdStage = 0;
        accuracyStage = 0;
        evasionStage = 0;

        // Remove all volitile effects (pretty much all non-revive effects)
        for (int i=0; i<effects.Count; i++)
        {
            if (effects[i].effect.type == Effect.EffectType.Volitile)
            {
                effects.Remove(effects[i]);
                i--;
            }
        }
    }


    #region Getters

    public Entity GetEntity()
    {
        return current;
    }


    public int GetCurrentHP()
    {
        return param.entityHP;
    }


    public int GetCurrentMP()
    {
        return param.entityMP;
    }


    // Accuracy / Evasion calcs go here
    public float GetAccuracy()
    {
        return 1;
    }


    public float GetEvasion()
    {
        return 1;
    }


    /// <summary>
    /// Converts a stat stage to a multiplier
    /// </summary>
    /// <returns>The multipier corresponding to a given stat</returns>
    private float GetStatModifier(int amt)
    {
        return Mathf.Max(2.0f, 2 + (float)amt) / Mathf.Max(2.0f, 2 - (float)amt);
    }
    

    public int GetAttack() { return param.entityAtk; }
    public float GetAttackModifier() { return GetStatModifier(atkStage); }


    public int GetDefense() { return param.entityDef; }
    public float GetDefenseModifier() { return GetStatModifier(defStage); }


    public int GetSpeed() { return param.entitySpeed; }
    public float GetSpeedModifier() { return GetStatModifier(spdStage); }


    public TurnManager GetTurnManager()
    {
        return turnManger;
    }

    #endregion


    public void SetTurnManager(TurnManager tm)
    {
        turnManger = tm;
    }


    #region Action

    public virtual void SelectAction()
    {
        action = current.moveList[UnityEngine.Random.Range(0, current.moveList.Count)];
    }


    public virtual void SelectAction(int index)
    {
        index = Mathf.Clamp(index, 0, current.moveList.Count);
        action = current.moveList[index];
    }


    public void ResetAction()
    {
        action = null;
        actionResult = null;
    }


    public void SetAction(Spell a)
    {
        action = a;
    }

    #endregion

    /// <summary>
    /// Heavy WIP, but this handles effect stuff
    /// </summary>
    #region Effects


    /// <summary>
    /// Execute effects when the turn begins
    /// </summary>
    public void ExecuteTurnStartEffects()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            EffectInstance eff = effects[i];
            eff.OnTurnStart();

            // Prevent skipping over any effects if an effect is removed
            if (!effects.Contains(eff)) i--;
        }
    }


    /// <summary>
    /// Execute effects when the move is selected
    /// </summary>
    public void ExecuteMoveSelectedEffects()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            EffectInstance eff = effects[i];
            eff.OnMoveSelected();

            // Prevent skipping over any effects if an effect is removed
            if (!effects.Contains(eff)) i--;
        }
    }


    /// <summary>
    /// Check if each effect should remain active
    /// </summary>
    public void ExecuteRemainActiveCheck()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            EffectInstance eff = effects[i];
            eff.CheckRemainActive();

            if (!eff.castSuccess)
            {
                RemoveEffect(eff);
                i--;
            }
        }
    }


    /// <summary>
    /// Execute effects when the turn ends
    /// </summary>
    public void ExecuteTurnEndEffects()
    {
        for(int i=0; i<effects.Count; i++)
        {
            EffectInstance eff = effects[i];
            eff.OnTurnEnd();

            // Prevent skipping over any effects if an effect is removed
            if (!effects.Contains(eff)) i--;
        }
    }


    /// <summary>
    /// Applies an effect
    /// </summary>
    public void ApplyEffect(EffectInstance eff)
    {
        if (eff.effect.IsStackable())
        {
            EffectInstance curr = effects.Find(f => f.effect.GetName() == eff.effect.GetName());

            // Handle stacking (reset duration/add buffs)
            if (curr != null)
            {
                // We need an OnStack callback. This would allow stacking to be more customizable
                curr.OnStack();
            }
            else
            {
                // On apply callback
                effects.Add(eff);
                eff.OnApply();
            }
        }
        else if(!effects.Exists(f => f.effect.GetName() == eff.effect.GetName()))
        {
            // On apply callback
            effects.Add(eff);
            eff.OnApply();
        }

        effects.Sort();
    }


    /// <summary>
    /// Removes an effect
    /// </summary>
    public void RemoveEffect(EffectInstance eff)
    {
        if (effects.Contains(eff))
        {
            Debug.Log(eff.effect.GetName());
            eff.OnDeactivate();
            effects.Remove(eff);
        }
    }


    /// <summary>
    /// Removes an effect based on its name
    /// </summary>
    public void RemoveEffect(string s)
    {
        EffectInstance eff = effects.Find(f => f.effect.GetName() == s);

        if (eff != null)
        {
            eff.OnDeactivate();
            effects.Remove(eff);
        }
    }


    /// <summary>
    /// Removes an effect
    /// </summary>
    public void RemoveEffectNoDeactivate(EffectInstance eff)
    {
        if (effects.Contains(eff))
        {
            Debug.Log(eff.effect.GetName());
            effects.Remove(eff);
        }
    }


    /// <summary>
    /// Removes an effect based on its name but does not call deactivation funcitons.
    /// Used to remove effects forcibly, such as status removal.
    /// (Ex. Doomsday Clock's deactivate call kills the user)
    /// </summary>
    public void RemoveEffectNoDeactivate(string s)
    {
        EffectInstance eff = effects.Find(f => f.effect.GetName() == s);

        if (eff != null)
        {
            effects.Remove(eff);
        }
    }

    #endregion


    #region Stat Modification

    /// <summary>
    /// Adds an offensive modifier with a given key
    /// </summary>
    public void AddOffenseModifier(float amt, string key)
    {
        if (offenseModifiers.ContainsKey(key))
            return;

        offenseModifiers.Add(key, amt);
    }

    /// <summary>
    /// Removes an offensive modifier with a given key
    /// </summary>
    public void RemoveOffenseModifier(string key)
    {
        offenseModifiers.Remove(key);
    }

    /// <summary>
    /// Adds a defensive modifier with a given key
    /// </summary>
    public void AddDefenseModifier(float amt, string key)
    {
        if (defenseModifiers.ContainsKey(key))
            return;

        defenseModifiers.Add(key, amt);
    }

    /// <summary>
    /// Removes a defensive modifier with a given key
    /// </summary>
    public void RemoveDefenseModifier(string key)
    {
        defenseModifiers.Remove(key);
    }

    /// <summary>
    /// Adds a speed modifier with a given key
    /// </summary>
    public void AddSpeedModifier(float amt, string key)
    {
        if (speedModifiers.ContainsKey(key))
            return;

        speedModifiers.Add(key, amt);
    }

    /// <summary>
    /// Removes a speed modifier with a given key
    /// </summary>
    public void RemoveSpeedModifier(string key)
    {
        speedModifiers.Remove(key);
    }

    /// <summary>
    /// Adds an accuracy modifier with a given key
    /// </summary>
    public void AddAccuracyModifier(float amt, string key)
    {
        if (accuracyModifiers.ContainsKey(key))
            return;

        accuracyModifiers.Add(key, amt);
    }

    /// <summary>
    /// Removes an accuracy modifier with a given key
    /// </summary>
    public void RemoveAccuracyModifier(string key)
    {
        accuracyModifiers.Remove(key);
    }


    public Dictionary<string, float>.ValueCollection GetOffenseModifiers()
    {
        return offenseModifiers.Values;
    }

    public Dictionary<string, float>.ValueCollection GetDefenseModifiers()
    {
        return defenseModifiers.Values;
    }

    public Dictionary<string, float>.ValueCollection GetSpeedModifiers()
    {
        return speedModifiers.Values;
    }

    public Dictionary<string, float>.ValueCollection GetAccuracyModifiers()
    {
        return accuracyModifiers.Values;
    }

    #endregion


    #region Properties

    public void AddProperty(EffectInstance eff)
    {
        properties.Add(eff);
    }


    public List<EffectInstance> GetProperties()
    {
        return properties;
    }


    public void ClearProperties()
    {
        foreach (EffectInstance eff in properties)
            eff.OnDeactivate();

        properties.Clear();
    }

    #endregion

    #region UI Functions

    public void ShowUI() { if (entityUI != null) entityUI.ShowUI(); }
    public void HideUI() { if (entityUI != null) entityUI.HideUI(); }

    public void UpdateHPUI()
    {
        if (entityUI != null)
            entityUI.ChangeHP(lastHit);
    }

    #endregion

    #region Compare

    /// <summary>
    /// Compares the speed of two entities
    /// </summary>
    /// <param name="obj">EntityController to compare to</param>
    /// <returns>1 if this object is slower, -1 if this it is faster, random otherwise.</returns>
    public int CompareTo(EntityController other)
    {
        int priorityA = action.spellPriority;
        int priorityB = other.action.spellPriority;

        if (priorityA > priorityB)
            return -1;
        else if (priorityB > priorityA)
            return 1;

        int speedA = Mathf.RoundToInt((float)GetSpeed() * GetSpeedModifier());
        int speedB = Mathf.RoundToInt((float)other.GetSpeed() * other.GetSpeedModifier());

        var speedModA = GetSpeedModifiers();
        var speedModB = other.GetSpeedModifiers();

        foreach (float mod in speedModA)
            speedA = Mathf.RoundToInt((float)speedA * mod);

        foreach (float mod in speedModB)
            speedB = Mathf.RoundToInt((float)speedB * mod);

        if (speedA > speedB)
            return -1;
        else if (speedB > speedA)
            return 1;

        float rand = UnityEngine.Random.value;

        if (rand > 0.5f)
            return -1;
        else
            return 1;
    }

    #endregion
}
