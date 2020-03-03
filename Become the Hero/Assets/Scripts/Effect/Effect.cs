using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Hero.SpellEditor;
#endif

/// <summary>
/// Represents an effect of an attack.
/// </summary>
[CreateAssetMenu(fileName = "NewEffect", menuName = "Effect/Effect", order = 3)]
[GUIColor(1.0f, 1.0f, 1.0f)]
public class Effect : ScriptableObject
{
    [HideInInspector]
    public delegate void InitialDelegate();

    // Indicates if the effect should be removed on death
    public enum EffectType { Volitile, NonVolitile }

    // Used in functions to indicate if the user or target should recieve the effect
    public enum EffectTarget { user, target }

    [SerializeField]
    [PropertyOrder(0)]
    private bool stackable;

    [PropertyOrder(0)] public bool castSuccess { get; private set; }
    // Indicates if the current event should check if the cast was successful or not
    private bool checkSuccess = true;

    [PropertyOrder(0)] public string effectName;
    // Used to allow for certain effects to be applied independently while using the same name (i.e. stat buffs)
    [SerializeField]
    [PropertyOrder(0)] private bool generic = false;

    // Current instance of effect calculations should be ran on
    public EffectInstance current;

    // Order effects should execute at the end of a turn. Higher priorities execute sooner.
    [SerializeField] [Range(0, 9)]
    private int priority = 3;

    // Turn limit applied to instances of this effect. Generally only used for displays.
    [SerializeField] [Range(0, 20)]
    [PropertyOrder(0)] private int limit = 3;

    [PropertyOrder(0)] public EffectType type = EffectType.Volitile;

    [InlineEditor] [GUIColor(0.98f, 0.85f, 0.55f)]
    [PropertyOrder(0)] public EffectDisplay display;

#if UNITY_EDITOR

    private string effectDisplayButtonName = "Create New Effect Display";
    
    [Button(ButtonSizes.Small)]
    [GUIColor("CheckDisplayColor")]
    [LabelText("$effectDisplayButtonName")]
    [EnableIf("CheckDisplayName")]
    [PropertyOrder(0)]
    private void CreateNewEffectDisplay()
    {
        if (display == null || SpellEditorUtilities.CheckIfAssetExists
            (this.effectName.Replace(" ", "") + "Display", "Assets/UI/EffectDisplay/"))
        {
            display = ScriptableObject.CreateInstance<EffectDisplay>();
            effectDisplayButtonName = "Create";
            display.displayName = effectName;
        }
        else
        {
            SpellEditorUtilities.CreateAsset(display,
                "Assets/UI/EffectDisplay/" + this.effectName.Replace(" ", "") + "Display");
            display = null;
            effectDisplayButtonName = "Create New Effect Display";
        }
    }

    private bool CheckDisplayName() { return display == null || this.effectName.Replace(" ", "") + "Display" != ""; }
    private Color CheckDisplayColor() { return IsEffectDisplayValid() ? Color.white : Color.green; }

    private bool IsEffectDisplayValid()
    {
        return (display == null || SpellEditorUtilities.CheckIfAssetExists
            (this.effectName.Replace(" ", "") + "Display", "Assets/UI/EffectDisplay/"));
    }

#endif

    // Used for function callbacks
    [PropertySpace(20)]
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToCheckSuccess")][PropertyOrder(1)]
    public List<BetterEventEntry> CheckSuccess = new List<BetterEventEntry>();
    private void AssignThisToCheckSuccess(){ CheckSuccess.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [GUIColor(0.9f, 0.9f, 0.9f)]
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToCheckRemainActive")][PropertyOrder(1)]
    public List<BetterEventEntry> CheckRemainActive = new List<BetterEventEntry>();
    private void AssignThisToCheckRemainActive() { CheckRemainActive.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }
    
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToOnActivate")][PropertyOrder(1)]
    public List<BetterEventEntry> OnActivate = new List<BetterEventEntry>();
    private void AssignThisToOnActivate() { OnActivate.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [GUIColor(0.9f, 0.9f, 0.9f)]
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToActivateFailed")][PropertyOrder(1)]
    public List<BetterEventEntry> OnFailedToActivate = new List<BetterEventEntry>();
    private void AssignThisToActivateFailed() { OnFailedToActivate.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToOnApply")][PropertyOrder(1)]
    public List<BetterEventEntry> OnApply = new List<BetterEventEntry>();
    private void AssignThisToOnApply() { OnApply.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [GUIColor(0.9f, 0.9f, 0.9f)]
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToMoveSelected")][PropertyOrder(1)]
    public List<BetterEventEntry> OnMoveSelected = new List<BetterEventEntry>();
    private void AssignThisToMoveSelected() { OnMoveSelected.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }
    
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToDeactivate")][PropertyOrder(1)]
    public List<BetterEventEntry> OnDeactivate = new List<BetterEventEntry>();
    private void AssignThisToDeactivate() { OnDeactivate.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [GUIColor(0.9f, 0.9f, 0.9f)]
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToTurnStart")][PropertyOrder(1)]
    public List<BetterEventEntry> OnTurnStart = new List<BetterEventEntry>();
    private void AssignThisToTurnStart() { OnTurnStart.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToTurnEnd")][PropertyOrder(1)]
    public List<BetterEventEntry> OnTurnEnd = new List<BetterEventEntry>();
    private void AssignThisToTurnEnd() { OnTurnEnd.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [GUIColor(0.9f, 0.9f, 0.9f)]
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToOnStack")][PropertyOrder(1)]
    public List<BetterEventEntry> OnStack = new List<BetterEventEntry>();
    private void AssignThisToOnStack() { OnStack.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    public bool IsStackable() { return stackable; }
    public void ResetSuccess() { castSuccess = true; checkSuccess = true; }


    /// <summary>
    /// Empty function used for initialization
    /// </summary>
    public void NONE() { }

    /// <summary>
    /// Create an instance of this effect
    /// </summary>
    public EffectInstance CreateEffectInstance(EntityController u, EntityController t, SpellCast s)
    {
        EffectInstance e = new EffectInstance();
        e.user = u;
        e.target = t;
        e.spell = s;
        e.effect = this;
        e.limit = limit;
        return e;
    }


    /// <summary>
    /// Returns the priority of this effect.
    /// </summary>
    public int GetPriority()
    {
        return priority;
    }


    public string GetName()
    {
        if (generic)
            return current.spell.spell.spellName + " " + effectName;
        else
            return effectName;
    }


    #region Check Functions

    /// <summary>
    /// Check if this effect should be applied
    /// </summary>
    public void CheckForCastSuccess()
    {
        castSuccess = true;

        for (int i = 0; i < CheckSuccess.Count; i++)
        {
            CheckSuccess[i].Invoke();
        }
    }


    /// <summary>
    /// Check if this effect should remain active
    /// </summary>
    public void CheckForRemainActive()
    {
        castSuccess = true;

        for (int i = 0; i < CheckRemainActive.Count; i++)
        {
            CheckRemainActive[i].Invoke();
        }
    }


    public void IsActiveForLessThanTurnLimit()
    {
        castSuccess = !castSuccess ? false : current.numTurnsActive < limit;
    }


    public void IsActiveForLessThanXTurns(int duration)
    {
        castSuccess = !castSuccess ? false : current.numTurnsActive < duration;
    }


    public void IsLastMoveSuccessful(EffectTarget target)
    {
        if(target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : current.user.actionResult.success;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.actionResult.success;
    }


    public void IsLastMoveEqualToTarget (EffectTarget target, Spell spell)
    {
        if (target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : spell == current.user.action;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : spell == current.target.action;
    }


    public void IsHealthAbovePercent(EffectTarget target, float percent)
    {
        percent = Mathf.Clamp(percent, 0.0f, 1.0f);

        if (target.Equals(EffectTarget.user))
        {
            float healthPercent = ((float)current.user.GetCurrentHP() / (float)current.user.maxHP);
            castSuccess = !castSuccess ? false : healthPercent > percent;
        }
        else if (target.Equals(EffectTarget.target))
        {
            float healthPercent = ((float)current.target.GetCurrentHP() / (float)current.target.maxHP);
            castSuccess = !castSuccess ? false : healthPercent > percent;
        }
    }

    [TabGroup("First")]
    public void IsEffectMoveEqualToTarget(Spell spell)
    {
        castSuccess = !castSuccess ? false : spell == current.spell.spell;
    }


    public void IsAttackNotMaxed(EffectTarget target)
    {
        if (target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : current.user.GetAttackStage() < EntityController.STAT_STAGE_LIMIT;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.GetAttackStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsAttackNotMin(EffectTarget target)
    {
        if (target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : current.user.GetAttackStage() > -EntityController.STAT_STAGE_LIMIT;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.GetAttackStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsDefenseNotMaxed(EffectTarget target)
    {
        if (target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : current.user.GetDefenseStage() < EntityController.STAT_STAGE_LIMIT;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.GetDefenseStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsDefenseNotMin(EffectTarget target)
    {
        if (target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : current.user.GetDefenseStage() > -EntityController.STAT_STAGE_LIMIT;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.GetDefenseStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsSpeedNotMaxed(EffectTarget target)
    {
        if (target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : current.user.GetSpeedStage() < EntityController.STAT_STAGE_LIMIT;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.GetSpeedStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsSpeedNotMin(EffectTarget target)
    {
        if (target.Equals(EffectTarget.user))
            castSuccess = !castSuccess ? false : current.user.GetSpeedStage() > -EntityController.STAT_STAGE_LIMIT;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.GetSpeedStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsRandomIsLessThanValue(float val)
    {
        castSuccess = !castSuccess ? false : UnityEngine.Random.value <= val;
    }


    public void IsNotAtFullHealth(EffectTarget target)
    {
        if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.user.GetCurrentHP() < current.user.maxHP;
        else if (target.Equals(EffectTarget.target))
            castSuccess = !castSuccess ? false : current.target.GetCurrentHP() < current.target.maxHP;
    }

    #endregion


    #region Action Functions


    public void SetCheckStatus(bool s)
    {
        checkSuccess = s;
    }


    /// <summary>
    /// Sends a dialogue message to the <see cref="DialogueManager"/>
    /// </summary>
    public void SendDialogue(string dialogue)
    {
        if (castSuccess != checkSuccess) return;

        dialogue = DialogueUtilities.ReplacePlaceholderWithEntityName(dialogue, current.user.param, "[user]");
        dialogue = DialogueUtilities.ReplacePlaceholderWithEntityName(dialogue, current.target.param, "[target]");
        dialogue = DialogueUtilities.ReplacePlaceholderWithText(dialogue, current.spell.spell.spellName, "[spell]");
        dialogue = DialogueUtilities.ReplacePlaceholderWithText(dialogue, 
            Mathf.Abs(current.user.lastHit).ToString(), "[udamage]");
        dialogue = DialogueUtilities.ReplacePlaceholderWithText(dialogue, 
            Mathf.Abs(current.target.lastHit).ToString(), "[tdamage]");

        EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, dialogue);
    }


    /// <summary>
    /// Sends an Animation Sequence to the <see cref="Sequencer"/>
    /// </summary>
    public void ApplyAnimation(EffectTarget target, AnimationSequenceObject animation)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
        {
            Hero.Core.Sequence anim = new AnimationSequence(animation, current.user, current.target);
            EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, anim);
        }
        else if (target.Equals(EffectTarget.target))
        {
            Hero.Core.Sequence anim = new AnimationSequence(animation, current.target, current.user);
            EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, anim);
        }
    }


    public void ApplyEffect(EffectTarget target)
    {
        if (castSuccess != checkSuccess) return;

        if(target.Equals(EffectTarget.user))
            current.user.ApplyEffect(current);
        else if (target.Equals(EffectTarget.target))
            current.target.ApplyEffect(current);
    }


    public void RemoveEffect(EffectTarget target)
    {
        if (castSuccess != checkSuccess) return;
        
        if (target.Equals(EffectTarget.user))
            current.user.RemoveEffectNoDeactivate(current);
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveEffectNoDeactivate(current);
    }


    public void RemoveAndDeactivateEffect(EffectTarget target)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.RemoveEffect(current);
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveEffect(current);
    }


    public void RemoveEffect(EffectTarget target, string name)
    {

    }


    public void ApplyProperty(EffectTarget target, Effect property)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
        {
            EffectInstance eff = property.CreateEffectInstance(current.user, current.target, current.spell);
            eff.numTurnsActive = current.numTurnsActive;
            current.user.AddProperty(eff);
        }
        else if (target.Equals(EffectTarget.target))
        {
            EffectInstance eff = property.CreateEffectInstance(current.target, current.user, current.spell);
            eff.numTurnsActive = current.numTurnsActive;
            current.target.AddProperty(eff);
        }

    }


    public void ReplaceAction(EffectTarget target, Spell action)
    {
        if (castSuccess != checkSuccess) return;

        if(target.Equals(EffectTarget.user))
            current.user.SetAction(action);
        else if (target.Equals(EffectTarget.target))
            current.target.SetAction(action);
    }


    /// <summary>
    /// Increases the number of turns this effect has been active
    /// </summary>
    public void IncrementTurnCounter()
    {
        current.numTurnsActive++;
    }


    /// <summary>
    /// Resets the number of turns this effect has been active
    /// </summary>
    public void ResetTurnCounter()
    {
        current.numTurnsActive = 0;
    }


    /// <summary>
    /// Decreases the number of turns this effect has been active
    /// </summary>
    public void DecreaseTurnCounter(int amt)
    {
        current.numTurnsActive -= amt;
    }


    #region MP/HP Manipulation


    public void ModifyHPFromPercentOfHP(EffectTarget target, float percent)
    {
        percent = Mathf.Clamp(percent, 0, 100);
        int amt = 0;

        if (target.Equals(EffectTarget.user))
        {
            amt = Mathf.RoundToInt(((float)current.user.maxHP) * percent);
            current.user.ApplyDamage(-amt, false, false);
        }
        else if (target.Equals(EffectTarget.target))
        {
            amt = Mathf.RoundToInt(((float)current.target.maxHP) * percent);
            current.target.ApplyDamage(-amt, false, false);
        }

    }


    public void ModifyMPFromDamageDealt(EffectTarget target, float modifier, int min, int max)
    {
        if (castSuccess != checkSuccess) return;

        int damage = current.spell.GetDamageApplied();
        damage = (int)(((float)damage) * modifier);
        damage = Mathf.Clamp(damage, min, max);

        if (target.Equals(EffectTarget.user))
            current.user.ModifyMP(damage);
        else if (target.Equals(EffectTarget.target))
            current.target.ModifyMP(damage);
    }


    public void ModifyMPFromDamageTaken(EffectTarget target, float modifier, int min, int max)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
        {
            int damage = current.user.damageTaken;
            damage = (int)(((float)damage) * modifier);
            damage = Mathf.Clamp(damage, min, max);
            current.user.ModifyMP(damage);
        }
        else if (target.Equals(EffectTarget.target))
        {
            int damage = current.target.damageTaken;
            damage = (int)(((float)damage) * modifier);
            damage = Mathf.Clamp(damage, min, max);
            current.target.ModifyMP(damage);
        }
    }


    #endregion

    #region Stat Modifiers

    public void ApplyOffenseModifier(EffectTarget target, float amt)
    {

    }


    public void RemoveOffenseModifier(EffectTarget target)
    {

    }


    public void RemoveOffenseModifier(EffectTarget target, string name)
    {

    }


    public void ApplyDefenseModifier(EffectTarget target, float amt)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.AddDefenseModifier(amt, GetName());
        else if(target.Equals(EffectTarget.target))
            current.target.AddDefenseModifier(amt, GetName());
    }


    public void RemoveDefenseModifier(EffectTarget target)
    {
        if (castSuccess != checkSuccess) return;
        
        if (target.Equals(EffectTarget.user))
            current.user.RemoveDefenseModifier(GetName());
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveDefenseModifier(GetName());
    }


    public void RemoveDefenseModifier(EffectTarget target, string name)
    {
        if (castSuccess != checkSuccess) return;
        
        if (target.Equals(EffectTarget.user))
            current.user.RemoveDefenseModifier(name);
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveDefenseModifier(name);
    }


    public void ApplySpeedModifier(EffectTarget target, float amt)
    {
        if (castSuccess != checkSuccess) return;

        if(target.Equals(EffectTarget.user))
            current.user.AddSpeedModifier(amt, GetName());
        else if(target.Equals(EffectTarget.target))
            current.target.AddSpeedModifier(amt, GetName());
    }


    public void RemoveSpeedModifier(EffectTarget target)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.RemoveSpeedModifier(GetName());
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveSpeedModifier(GetName());
    }


    public void RemoveSpeedModifier(EffectTarget target, string name)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.RemoveSpeedModifier(name);
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveSpeedModifier(name);
    }


    public void ApplyAccuracyModifier(EffectTarget target, float amt)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.AddAccuracyModifier(amt, GetName());
        else if (target.Equals(EffectTarget.target))
            current.target.AddAccuracyModifier(amt, GetName());
    }


    public void ApplyAttackAccuracyModifier(EffectTarget target, float amt)
    {
        if (castSuccess != checkSuccess) return;

        float result = 1;

        for (int i = 0; i < current.numTurnsActive; i++)
            result *= amt;

        if (target.Equals(EffectTarget.user))
            current.user.AddAccuracyModifier(result, GetName());
        else if (target.Equals(EffectTarget.target))
            current.target.AddAccuracyModifier(result, GetName());
    }


    public void RemoveAccuracyModifier(EffectTarget target)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.RemoveAccuracyModifier(GetName());
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveAccuracyModifier(GetName());
        current.user.RemoveAccuracyModifier(GetName());
    }


    public void RemoveAccuracyModifier(EffectTarget target, string name)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.RemoveAccuracyModifier(name);
        else if (target.Equals(EffectTarget.target))
            current.target.RemoveAccuracyModifier(name);
    }


    public void ChangeAttackStage(EffectTarget target, int amt)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.ChangeAttackModifier(amt);
        else if (target.Equals(EffectTarget.target))
            current.target.ChangeAttackModifier(amt);
    }


    public void ChangeDefenseStage(EffectTarget target, int amt)
    {
        if (castSuccess != checkSuccess) return;

        if (target.Equals(EffectTarget.user))
            current.user.ChangeDefenseModifier(amt);
        else if (target.Equals(EffectTarget.target))
            current.target.ChangeDefenseModifier(amt);
    }


    public void ChangeSpeedStage(EffectTarget target, int amt)
    {
        if (castSuccess != checkSuccess) return;
        
        if (target.Equals(EffectTarget.user))
            current.user.ChangeSpeedModifier(amt);
        else if (target.Equals(EffectTarget.target))
            current.target.ChangeSpeedModifier(amt);
    }

    #endregion

    #endregion
}


/// <summary>
/// Represents an instance of an <see cref="Effect"/>
/// </summary>
public class EffectInstance: IComparable<EffectInstance>
{
    public int numTurnsActive;
    public int strength = 1;
    public int limit { get; set; }
    public bool castSuccess { get; private set; }

    // Effect this instance is linked to
    public Effect effect;

    public EntityController user;
    public EntityController target;

    public SpellCast spell;


    #region Checks

    public void CheckSuccess()
    {
        effect.current = this;
        effect.CheckForCastSuccess();
        castSuccess = effect.castSuccess;
    }


    public void CheckRemainActive()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.CheckForRemainActive();
            castSuccess = effect.castSuccess;
        }
    }


    public void OnActivate()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            Invoke(effect.OnActivate);
        }
    }


    public void OnFailedToActivate()
    {
        effect.current = this;
        effect.ResetSuccess();
        Invoke(effect.OnFailedToActivate);
    }


    public void OnApply()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            Invoke(effect.OnApply);
        }
    }


    public void OnDeactivate()
    {
        effect.current = this;
        effect.ResetSuccess();
        Invoke(effect.OnDeactivate);
    }


    public void OnTurnStart()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            Invoke(effect.OnTurnStart);
        }
    }


    public void OnMoveSelected()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            Invoke(effect.OnMoveSelected);
        }
    }


    public void OnTurnEnd()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            Invoke(effect.OnTurnEnd);
        }
    }


    public void OnStack()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            Invoke(effect.OnStack);
        }
    }


    /// <summary>
    /// Custom Invoke function for BetterEvents
    /// </summary>
    private void Invoke(List<BetterEventEntry> Events)
    {
        if (Events == null) return;
        for (int i = 0; i < Events.Count; i++)
        {
            Events[i].Invoke();
        }
    }

    #endregion

    #region Compare

    /// <summary>
    /// Compares the priority of two instances
    /// </summary>
    /// <param name="other">Effect to compare to</param>
    /// <returns>1 if this instance has a greater priority, -1 if this it has lower, 0 otherwise.</returns>
    public int CompareTo(EffectInstance other)
    {
        int pA = effect.GetPriority();
        int pB = other.effect.GetPriority();

        if (pA > pB)
            return -1;
        else if (pB > pA)
            return 1;
        else
            return 0;
    }

    #endregion
}
