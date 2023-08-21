using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Houses an <see cref="Entity"/> and allows for gameplay operations to be performed on it.
/// </summary>
public class EntityController : EntitySprite, IComparable<EntityController>
{
    protected TurnManager turnManager;

    [SerializeField]
    protected Entity current;
    public List<EntityController> allies { get; set; }
    public List<EntityController> enemies { get; set; }
    public List<EntityController> target { get; set; }
    public EntityParams param { get; private set; }

    public Spell action { get; protected set; }
    public List<SpellCast> actionResult { get; set; }
    public bool ready { get; set; }
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
    private int matkStage = 0;
    private int mdefStage = 0;
    private int evasionStage = 0;
    private int accuracyStage = 0;

    public const int STAT_STAGE_LIMIT = 6;

    private Dictionary<string, float> attackModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> defenseModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> speedModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> magicAttackModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> magicDefenseModifiers = new Dictionary<string, float>();
    private Dictionary<string, float> accuracyModifiers = new Dictionary<string, float>();
    private List<EffectInstance> effects = new List<EffectInstance>();
    private List<EffectInstance> properties = new List<EffectInstance>();

    protected static AnimationSequenceObject spawn;
    protected static AnimationSequenceObject defeat;

    [SerializeField]
    protected EntityControllerUI entityUI;

    protected bool isIdentified = false;

    public bool acceptTouch { get; set; }
    private bool touched = false;
    private IEnumerator touchTimer;


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        Init();
        acceptTouch = false;
        target = new List<EntityController>();

        if(spawn == null)
            spawn = (Resources.Load("Animation/Appear", typeof(AnimationSequenceObject))) as AnimationSequenceObject;

        if (defeat == null)
            defeat = (Resources.Load("Animation/Defeat", typeof(AnimationSequenceObject))) as AnimationSequenceObject;
    }


    /// <summary>
    /// Initializes an entity controller
    /// </summary>
    public override void Init()
    {
        if (current != null)
        {
            // Set the params to the entity's params
            param = new EntityParams();

            // Get name info
            param.entityName = current.vals.entityName;
            param.useArticle = current.vals.useArticle;
            param.article = current.vals.article;

            // Get stats
            param.entityHP = ((current.vals.entityHP * 2 * Spell.DAMAGE_CONSTANT) / 100) + Spell.DAMAGE_CONSTANT + 10;
            param.entityMP = ((current.vals.entityMP * 2 * Spell.DAMAGE_CONSTANT) / 100) + Spell.DAMAGE_CONSTANT + 10;
            param.entityAtk = ((current.vals.entityAtk * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;
            param.entityDef = ((current.vals.entityDef * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;
            param.entityMgAtk = ((current.vals.entityMgAtk * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;
            param.entityMgDef = ((current.vals.entityMgDef * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;
            param.entitySpeed = ((current.vals.entitySpeed * 2 * Spell.DAMAGE_CONSTANT) / 100) + 5;
            param.entityCritModifier = current.vals.entityCritModifier;
            param.entityDodgeModifier = current.vals.entityDodgeModifier;

            maxHP = param.entityHP;
            maxMP = param.entityMP;

            // Reset stats and effects
            atkStage = 0;
            defStage = 0;
            spdStage = 0;
            matkStage = 0;
            mdefStage = 0;
            evasionStage = 0;
            accuracyStage = 0;

            // Reset ID. If we add save data, we'll need a check here
            isIdentified = false;

            // Set the sprite
            param.entitySprite = current.vals.entitySprite;
            sprite.sprite = param.entitySprite;

            effects = new List<EffectInstance>();
            properties = new List<EffectInstance>();
            attackModifiers = new Dictionary<string, float>();
            defenseModifiers = new Dictionary<string, float>();
            speedModifiers = new Dictionary<string, float>();
            magicAttackModifiers = new Dictionary<string, float>();
            magicDefenseModifiers = new Dictionary<string, float>();
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
        Init();
        dead = false;

        Hero.Core.Sequence spawnSeq = new AnimationSequence(spawn, this, this);
        EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, spawnSeq);
    }


    /// <summary>
    /// Apply damage to this entity
    /// </summary>
    public void ApplyDamage(int val, bool crit, bool vibrate, bool hit = true)
    {
        // If dead, do not apply damage
        if (dead) return;

        if(val == 0)
        {
            // TODO: Dodge animation
            if (vibrate)
            {
                sprite.transform.DOComplete();

                if (hit)
                {
                    sprite.gameObject.transform.DOShakePosition(0.2f, new Vector3(0.12f, 0.0f, 0.0f), 150);
                }
                else
                {
                    sprite.transform.DOLocalMove(new Vector3(-0.24f, 0, 0), 0.1f).OnComplete(() => 
                        sprite.transform.DOLocalMove(new Vector3(0, 0, 0), 0.1f).SetDelay(0.1f));
                }
            }
        }
        else
        {
            param.entityHP -= val;
            lastHit = val;

            param.entityHP = Mathf.Clamp(param.entityHP, 0, maxHP);

            if (param.entityHP <= 0)
            {
                OnDeath();
                dead = true;
            }

            if (vibrate)
            {
                if (hit)
                {
                    sprite.transform.DOComplete();

                    if (crit || dead)
                        sprite.gameObject.transform.DOShakePosition(0.26f, new Vector3(0.32f, 0.0f, 0.0f), 300, 90, false, false);
                    else
                        sprite.gameObject.transform.DOShakePosition(0.26f, new Vector3(0.24f, 0.0f, 0.0f), 200, 90, false, false);
                }
            }
        }
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

        attackModifiers.Clear();
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


    public List<EffectInstance> GetEffects()
    {
        return effects;
    }


    public int GetCurrentHP()
    {
        return param.entityHP;
    }


    public int GetCurrentMP()
    {
        return param.entityMP;
    }

    
    public float GetAccuracy()
    {
        return 1;
    }


    public float GetEvasion()
    {
        return param.entityDodgeModifier;
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
    public void ChangeAttackModifier(int amt) { atkStage = Mathf.Clamp(atkStage + amt, -STAT_STAGE_LIMIT, STAT_STAGE_LIMIT); }
    public int GetAttackStage() { return atkStage; }


    public int GetDefense() { return param.entityDef; }
    public float GetDefenseModifier() { return GetStatModifier(defStage); }
    public void ChangeDefenseModifier(int amt) { defStage = Mathf.Clamp(defStage + amt, -STAT_STAGE_LIMIT, STAT_STAGE_LIMIT); }
    public int GetDefenseStage() { return defStage; }


    public int GetSpeed() { return param.entitySpeed; }
    public float GetSpeedModifier() { return GetStatModifier(spdStage); }
    public void ChangeSpeedModifier(int amt) { spdStage = Mathf.Clamp(spdStage + amt, -STAT_STAGE_LIMIT, STAT_STAGE_LIMIT); }
    public int GetSpeedStage() { return spdStage; }


    public int GetMagicAttack() { return param.entityMgAtk; }
    public float GetMagicAttackModifier() { return GetStatModifier(matkStage); }
    public void ChangeMagicAttackModifier(int amt) { matkStage = Mathf.Clamp(matkStage + amt, -STAT_STAGE_LIMIT, STAT_STAGE_LIMIT); }
    public int GetMagicAttackStage() { return matkStage; }


    public int GetMagicDefense() { return param.entityMgDef; }
    public float GetMagicDefenseModifier() { return GetStatModifier(mdefStage); }
    public void ChangeMagicDefenseModifier(int amt) { mdefStage = Mathf.Clamp(mdefStage + amt, -STAT_STAGE_LIMIT, STAT_STAGE_LIMIT); }
    public int GetMagicDefenseStage() { return mdefStage; }


    public TurnManager GetTurnManager()
    {
        return turnManager;
    }


    public bool IsIdentified() { return isIdentified; }

    #endregion


    public void SetTurnManager(TurnManager tm)
    {
        turnManager = tm;
    }


    #region Action

    public virtual void SelectAction()
    {
        // Check if there is a behavior assigned to this entity and the behavior is valid
        if (current.behavior && current.behavior.TurnBehavior.Count > 0)
        {
            EntityBehavior behavior = null;

            // If it is the first turn, use the special first turn behavior instead of the loop
            if(turnManager.TurnNumber == 1 && current.behavior.bUseFirstTurn)
            {
                behavior = current.behavior.FirstTurnBehavior;
            }
            else
            {
                int turnIndex = (turnManager.TurnNumber - 1) % current.behavior.TurnBehavior.Count;
                behavior = current.behavior.TurnBehavior[turnIndex];
            }

            var result = behavior.GetResult(this, enemies, allies);

            if (result.ActionSuccess)
            {
                // NOTE: If no check occurs, result.Target can mismatch with the action
                // This value defaults to "Self" and in my example, 0 is an attack
                // Evan: This value should ONLY be used if it matches the action
                action = current.moveList[Mathf.Clamp(result.ActionID, 0, current.moveList.Count)];

                if (EntityBehavior.CheckTargetMatch(action, result)) 
                    SetTarget(result.TriggerEntity);
                else SetTarget();
            }
            else
            {
                // If the check fails, randomize the action from all available actions.
                // This should never occur, but if it does, it's no big deal
#if UNITY_EDITOR
                Debug.LogWarning($"WARNING: Action for {current.name} on turn {turnManager.TurnNumber} " +
                    $"has failed and will be randomized.");
#endif
                SelectRandomAction();
            }
        }
        else
        {
            SelectRandomAction();
        }

    }

    protected void SelectRandomAction()
    {
        action = current.moveList[UnityEngine.Random.Range(0, current.moveList.Count)];
        SetTarget();
    }


    public virtual void SelectAction(int index)
    {
        index = Mathf.Clamp(index, 0, current.moveList.Count);
        action = current.moveList[index];
    }


    public virtual void SelectAction(string name)
    {
        foreach(var move in current.moveList)
        {
            if (move.name == name)
            {
                action = move;
                break;
            }
        }
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


    #region Targetting

    public virtual void SetTarget(EntityController trigger = null)
    {
        // TODO: implement
        var available = GetPossibleTargets();
        int index = UnityEngine.Random.Range(0, available.Count);

        switch (action.GetSpellTarget())
        {
            case Spell.SpellTarget.SingleEnemy:

                if(trigger != null) target = new List<EntityController>() { trigger };
                else target = new List<EntityController>() { available[index] };

                break;

            case Spell.SpellTarget.SingleParty:

                if (trigger != null) target = new List<EntityController>() { trigger };
                else target = new List<EntityController>() { available[index] };

                break;

            case Spell.SpellTarget.RandomEnemy:
                target = new List<EntityController>() { available[index] };
                break;

            default:
                target = available;
                break;
        }
    }

    public virtual List<EntityController> GetPossibleTargets()
    {
        List<EntityController> result = new List<EntityController>();

        switch (action.GetSpellTarget())
        {
            case Spell.SpellTarget.All:

                foreach (var item in enemies) result.Add(item);
                foreach (var item in allies) result.Add(item);

                break;

            case Spell.SpellTarget.AllParty:
                result = allies;
                break;

            case Spell.SpellTarget.AllEnemy:
                result = enemies;
                break;

            case Spell.SpellTarget.SingleParty:
                result = allies;
                break;

            case Spell.SpellTarget.SingleEnemy:
                result = enemies;
                break;

            case Spell.SpellTarget.RandomEnemy:
                result = enemies;
                break;

            case Spell.SpellTarget.Self:
                result.Add(this);
                break;
        }

        return result;
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
        if (dead) return;

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
    /// Returns true if an instance of the given Effect exists on this EntityController.
    /// </summary>
    public bool HasEffect(Effect eff)
    {
        return effects.Exists(f => f.effect.GetName() == eff.GetName());
    }


    /// <summary>
    /// Removes an effect
    /// </summary>
    public void RemoveEffect(EffectInstance eff)
    {
        if (effects.Contains(eff))
        {
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
        if (attackModifiers.ContainsKey(key))
            return;

        attackModifiers.Add(key, amt);
    }

    /// <summary>
    /// Removes an offensive modifier with a given key
    /// </summary>
    public void RemoveOffenseModifier(string key)
    {
        attackModifiers.Remove(key);
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


    public Dictionary<string, float>.ValueCollection GetAttackModifiers()
    {
        return attackModifiers.Values;
    }

    public Dictionary<string, float>.ValueCollection GetDefenseModifiers()
    {
        return defenseModifiers.Values;
    }

    public Dictionary<string, float>.ValueCollection GetSpeedModifiers()
    {
        return speedModifiers.Values;
    }

    public Dictionary<string, float>.ValueCollection GetMagicAttackModifiers()
    {
        return magicAttackModifiers.Values;
    }

    public Dictionary<string, float>.ValueCollection GetMagicDefenseModifiers()
    {
        return magicDefenseModifiers.Values;
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

    #region Pointer/Menu Functions

    /// <summary>
    /// On Pointer Down callback.
    /// </summary>
    public void OnPointerDown()
    {
        if (acceptTouch)
        {
            touched = true;

            touchTimer = TouchTimer();
            StartCoroutine(touchTimer);
        }
    }

    /// <summary>
    /// Runs while the EntityController is touched.
    /// </summary>
    private IEnumerator TouchTimer()
    {
        yield return new WaitForSeconds(1.0f);

        // Invoke the display event
        EventManager.Instance.RaiseEntityControllerEvent(EventConstants.OPEN_STATUS_SCREEN, this);
        OnPointerUp();
    }

    /// <summary>
    /// On Pointer Up callback.
    /// </summary>
    public void OnPointerUp()
    {
        // Only call if touched, because the OnExit event uses the same behavior.
        if (touched && acceptTouch)
        {
            if (touchTimer != null)
                StopCoroutine(touchTimer);

            touched = false;
        }
    }

    #endregion

    #endregion


    #region Compare

    /// <summary>
    /// Compares the speed of two entities
    /// </summary>
    /// <param name="obj">EntityController to compare to</param>
    /// <returns>1 if this object is slower, -1 if this it is faster, random otherwise.</returns>
    public int CompareTo(EntityController other)
    {
        int priorityA = action ? action.spellPriority : -10;
        int priorityB = other.action ? other.action.spellPriority : -10;

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
