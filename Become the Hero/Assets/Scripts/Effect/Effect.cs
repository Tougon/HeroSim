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

    [SerializeField]
    private bool stackable;

    public bool castSuccess { get; private set; }
    // Indicates if the current event should check if the cast was successful or not
    private bool checkSuccess = true;

    public string effectName;
    // Used to allow for certain effects to be applied independently while using the same name (i.e. stat buffs)
    [SerializeField]
    private bool generic = false;

    // Current instance of effect calculations should be ran on
    public EffectInstance current;

    // Order effects should execute at the end of a turn. Higher priorities execute sooner.
    [SerializeField] [Range(0, 9)]
    private int priority = 3;

    // Turn limit applied to instances of this effect. Generally only used for displays.
    [SerializeField] [Range(0, 20)]
    private int limit = 3;

    public EffectType type = EffectType.Volitile;

    [InlineEditor] [GUIColor(0.98f, 0.85f, 0.55f)]
    public EffectDisplay display;

#if UNITY_EDITOR

    private string effectDisplayButtonName = "Create New Effect Display";
    
    [Button(ButtonSizes.Small)]
    [GUIColor("CheckDisplayColor")]
    [LabelText("$effectDisplayButtonName")]
    [EnableIf("CheckDisplayName")]
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
    public UnityEvent CheckSuccess;

    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "AssignThisToCheckSuccess")]
    public List<BetterEventEntry> CheckEffectSuccess = new List<BetterEventEntry>();
    private void AssignThisToCheckSuccess(){ CheckEffectSuccess.Add(new BetterEventEntry(new InitialDelegate(this.NONE))); }

    [GUIColor(0.9f, 0.9f, 0.9f)] public UnityEvent CheckRemainActive;
    public UnityEvent OnActivate;
    [GUIColor(0.9f, 0.9f, 0.9f)] public UnityEvent OnFailedToActivate;
    public UnityEvent OnApply;
    [GUIColor(0.9f, 0.9f, 0.9f)] public UnityEvent OnMoveSelected;
    public UnityEvent OnDeactivate;
    [GUIColor(0.9f, 0.9f, 0.9f)] public UnityEvent OnTurnStart;
    public UnityEvent OnTurnEnd;
    [GUIColor(0.9f, 0.9f, 0.9f)] public UnityEvent OnStack;
    
    public bool IsStackable() { return stackable; }
    public void ResetSuccess() { castSuccess = true; checkSuccess = true; }


    /// <summary>
    /// Empty function used for initialization
    /// </summary>
    public void NONE() { Debug.Log("yesssssss"); }

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
        CheckSuccess.Invoke();
    }


    /// <summary>
    /// Check if this effect should remain active
    /// </summary>
    public void CheckForRemainActive()
    {
        castSuccess = true;
        CheckRemainActive.Invoke();
    }


    public void IsActiveForLessThanTurnLimit()
    {
        castSuccess = !castSuccess ? false : current.numTurnsActive < limit;
    }


    public void IsActiveForLessThanXTurns(int duration)
    {
        castSuccess = !castSuccess ? false : current.numTurnsActive < duration;
    }


    public void IsUserLastMoveSuccessful()
    {
        castSuccess = !castSuccess ? false : current.user.actionResult.success;
    }


    public void IsUserLastMoveEqualToTarget (Spell s)
    {
        castSuccess = !castSuccess ? false : s == current.user.action;
    }


    public void IsUserHealthAbovePercent(float percent)
    {
        percent = Mathf.Clamp(percent, 0.0f, 1.0f);

        float healthPercent = ((float)current.user.GetCurrentHP() / (float)current.user.maxHP);
        castSuccess = !castSuccess ? false : healthPercent > percent;
    }

    [TabGroup("First")]
    public void IsCurrentMoveEqualToTarget(Spell s)
    {
        castSuccess = !castSuccess ? false : s == current.spell.spell;
    }


    public void IsUserAttackNotMaxed()
    {
        castSuccess = !castSuccess ? false : current.user.GetAttackStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsUserAttackNotMin()
    {
        castSuccess = !castSuccess ? false : current.user.GetAttackStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsTargetAttackNotMaxed()
    {
        castSuccess = !castSuccess ? false : current.target.GetAttackStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsTargetAttackNotMin()
    {
        castSuccess = !castSuccess ? false : current.target.GetAttackStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsUserDefenseNotMaxed()
    {
        castSuccess = !castSuccess ? false : current.user.GetDefenseStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsUserDefenseNotMin()
    {
        castSuccess = !castSuccess ? false : current.user.GetDefenseStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsTargetDefenseNotMaxed()
    {
        castSuccess = !castSuccess ? false : current.target.GetDefenseStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsTargetDefenseNotMin()
    {
        castSuccess = !castSuccess ? false : current.target.GetDefenseStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsUserSpeedNotMaxed()
    {
        castSuccess = !castSuccess ? false : current.user.GetSpeedStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsUserSpeedNotMin()
    {
        castSuccess = !castSuccess ? false : current.user.GetSpeedStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsTargetSpeedNotMaxed()
    {
        castSuccess = !castSuccess ? false : current.target.GetSpeedStage() < EntityController.STAT_STAGE_LIMIT;
    }


    public void IsTargetSpeedNotMin()
    {
        castSuccess = !castSuccess ? false : current.target.GetSpeedStage() > -EntityController.STAT_STAGE_LIMIT;
    }


    public void IsRandomIsLessThanValue(float val)
    {
        castSuccess = !castSuccess ? false : UnityEngine.Random.value <= val;
    }


    public void IsUserNotAtFullHealth()
    {
        castSuccess = !castSuccess ? false : current.user.GetCurrentHP() < current.user.maxHP;
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
    public void ApplyAnimationToUser(AnimationSequenceObject aso)
    {
        if (castSuccess != checkSuccess) return;

        Hero.Core.Sequence anim = new AnimationSequence(aso, current.user, current.target);
        EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, anim);
    }


    /// <summary>
    /// Sends an Animation Sequence to the <see cref="Sequencer"/>
    /// </summary>
    public void ApplyAnimationToTarget(AnimationSequenceObject aso)
    {
        if (castSuccess != checkSuccess) return;

        Hero.Core.Sequence anim = new AnimationSequence(aso, current.target, current.user);
        EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, anim);
    }


    public void ApplyEffectToUser()
    {
        if (castSuccess != checkSuccess) return;

        current.user.ApplyEffect(current);
    }


    public void RemoveEffectFromUser()
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveEffectNoDeactivate(current);
    }


    public void RemoveAndDeactivateEffectFromUser()
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveEffect(current);
    }


    public void RemoveEffectFromUser(string name)
    {

    }


    public void ApplyEffectToTarget()
    {
        if (castSuccess != checkSuccess) return;

        current.target.ApplyEffect(current);
    }


    public void RemoveEffectFromTarget()
    {
        if (castSuccess != checkSuccess) return;

        current.target.RemoveEffectNoDeactivate(current);
    }


    public void RemoveAndDeactivateEffectFromTarget()
    {
        if (castSuccess != checkSuccess) return;

        current.target.RemoveEffect(current);
    }



    public void RemoveEffectFromTarget(string name)
    {

    }


    public void ApplyPropertyToUser(Effect property)
    {
        if (castSuccess != checkSuccess) return;

        EffectInstance eff = property.CreateEffectInstance(current.user, current.target, current.spell);
        eff.numTurnsActive = current.numTurnsActive;
        current.user.AddProperty(eff);
    }


    public void ReplaceUserAction(Spell action)
    {
        if (castSuccess != checkSuccess) return;

        current.user.SetAction(action);
    }


    public void ReplaceTargetAction(Spell action)
    {
        if (castSuccess != checkSuccess) return;

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


    public void ModifyUserHPFromPercentOfHP(float percent)
    {
        percent = Mathf.Clamp(percent, 0, 100);

        int amt = Mathf.RoundToInt(((float)current.user.maxHP) * percent);

        current.user.ApplyDamage(-amt, false, false);
    }


    public void ModifyUserMPFromDamageDealt(string s)
    {
        if (castSuccess != checkSuccess) return;

        string[] param = s.Split(',');
        ModifyUserMPFromDamageDealt(float.Parse(param[0]), int.Parse(param[1]), int.Parse(param[2]));
    }


    public void ModifyUserMPFromDamageDealt(float modifier, int min, int max)
    {
        if (castSuccess != checkSuccess) return;

        int damage = current.spell.GetDamageApplied();
        damage = (int)(((float)damage) * modifier);

        damage = Mathf.Clamp(damage, min, max);
        current.user.ModifyMP(damage);
    }


    public void ModifyTargetMPFromDamageDealt(string s)
    {
        if (castSuccess != checkSuccess) return;

        string[] param = s.Split(',');
        ModifyTargetMPFromDamageDealt(float.Parse(param[0]), int.Parse(param[1]), int.Parse(param[2]));
    }


    public void ModifyTargetMPFromDamageDealt(float modifier, int min, int max)
    {
        if (castSuccess != checkSuccess) return;

        int damage = current.spell.GetDamageApplied();
        damage = (int)(((float)damage) * modifier);

        damage = Mathf.Clamp(damage, min, max);
        current.target.ModifyMP(damage);
    }


    public void ModifyUserMPFromDamageTaken(string s)
    {
        if (castSuccess != checkSuccess) return;

        string[] param = s.Split(',');
        ModifyUserMPFromDamageTaken(float.Parse(param[0]), int.Parse(param[1]), int.Parse(param[2]));
    }


    public void ModifyUserMPFromDamageTaken(float modifier, int min, int max)
    {
        if (castSuccess != checkSuccess) return;

        int damage = current.user.damageTaken;
        damage = (int)(((float)damage) * modifier);

        damage = Mathf.Clamp(damage, min, max);
        current.user.ModifyMP(damage);
    }


    public void ModifyTargetMPFromDamageTaken(string s)
    {
        if (castSuccess != checkSuccess) return;

        string[] param = s.Split(',');
        ModifyTargetMPFromDamageTaken(float.Parse(param[0]), int.Parse(param[1]), int.Parse(param[2]));
    }


    public void ModifyTargetMPFromDamageTaken(float modifier, int min, int max)
    {
        if (castSuccess != checkSuccess) return;

        int damage = current.target.damageTaken;
        damage = (int)(((float)damage) * modifier);

        damage = Mathf.Clamp(damage, min, max);
        current.target.ModifyMP(damage);
    }

    #endregion

    #region Stat Modifiers

    public void ApplyOffenseModifierToUser(float amt)
    {

    }


    public void RemoveOffenseModifierFromUser()
    {

    }


    public void RemoveOffenseModifierFromUser(string name)
    {

    }


    public void ApplyOffenseModifierToTarget(float amt)
    {

    }


    public void RemoveOffenseModifierFromTarget()
    {

    }


    public void RemoveOffenseModifierFromTarget(string name)
    {

    }


    public void ApplyDefenseModifierToUser(float amt)
    {
        if (castSuccess != checkSuccess) return;

        current.user.AddDefenseModifier(amt, GetName());
    }


    public void RemoveDefenseModifierFromUser()
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveDefenseModifier(GetName());
    }


    public void RemoveDefenseModifierFromUser(string name)
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveDefenseModifier(name);
    }


    public void ApplyDefenseModifierToTarget(float amt)
    {

    }


    public void RemoveDefenseModifierFromTarget()
    {

    }


    public void RemoveDefenseModifierFromTarget(string name)
    {

    }


    public void ApplySpeedModifierToUser(float amt)
    {
        if (castSuccess != checkSuccess) return;

        current.user.AddSpeedModifier(amt, GetName());
    }


    public void RemoveSpeedModifierFromUser()
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveSpeedModifier(GetName());
    }


    public void RemoveSpeedModifierFromUser(string name)
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveSpeedModifier(name);
    }


    public void ApplySpeedModifierToTarget(float amt)
    {
        if (castSuccess != checkSuccess) return;

        current.target.AddSpeedModifier(amt, GetName());
    }


    public void RemoveSpeedModifierFromTarget()
    {
        if (castSuccess != checkSuccess) return;

        current.target.RemoveSpeedModifier(GetName());
    }


    public void RemoveSpeedModifierFromTarget(string name)
    {
        if (castSuccess != checkSuccess) return;

        current.target.RemoveSpeedModifier(name);
    }


    public void ApplyAccuracyModifierToUser(float amt)
    {
        if (castSuccess != checkSuccess) return;

        current.user.AddAccuracyModifier(amt, GetName());
    }


    public void ApplyAttackAccuracyModifierToUser(float amt)
    {
        if (castSuccess != checkSuccess) return;

        float result = 1;

        for (int i = 0; i < current.numTurnsActive; i++)
            result *= amt;

        current.user.AddAccuracyModifier(result, GetName());
    }


    public void RemoveAccuracyModifierFromUser()
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveAccuracyModifier(GetName());
    }


    public void RemoveAccuracyModifierFromUser(string name)
    {
        if (castSuccess != checkSuccess) return;

        current.user.RemoveAccuracyModifier(name);
    }


    public void ChangeUserAttackStage(int amt)
    {
        if (castSuccess != checkSuccess) return;

        current.user.ChangeAttackModifier(amt);
    }


    public void ChangeTargetAttackStage(int amt)
    {
        if (castSuccess != checkSuccess) return;

        current.target.ChangeAttackModifier(amt);
    }


    public void ChangeUserDefenseStage(int amt)
    {
        if (castSuccess != checkSuccess) return;

        current.user.ChangeDefenseModifier(amt);
    }


    public void ChangeTargetDefenseStage(int amt)
    {
        if (castSuccess != checkSuccess) return;

        current.target.ChangeDefenseModifier(amt);
    }


    public void ChangeUserSpeedStage(int amt)
    {
        if (castSuccess != checkSuccess) return;

        current.user.ChangeSpeedModifier(amt);
    }


    public void ChangeTargetSpeedStage(int amt)
    {
        if (castSuccess != checkSuccess) return;

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
        Invoke(effect.CheckEffectSuccess);
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
            effect.OnActivate.Invoke();
        }
    }


    public void OnFailedToActivate()
    {
        effect.current = this;
        effect.ResetSuccess();
        effect.OnFailedToActivate.Invoke();
    }


    public void OnApply()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            effect.OnApply.Invoke();
        }
    }


    public void OnDeactivate()
    {
        effect.current = this;
        effect.ResetSuccess();
        effect.OnDeactivate.Invoke();
    }


    public void OnTurnStart()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            effect.OnTurnStart.Invoke();
        }
    }


    public void OnMoveSelected()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            effect.OnMoveSelected.Invoke();
        }
    }


    public void OnTurnEnd()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            effect.OnTurnEnd.Invoke();
        }
    }


    public void OnStack()
    {
        if (castSuccess)
        {
            effect.current = this;
            effect.ResetSuccess();
            effect.OnStack.Invoke();
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
