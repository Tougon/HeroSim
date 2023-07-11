using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Hero.SpellEditor;
#endif

/// <summary>
/// Represents any action that can be taken on a turn.
/// </summary>
[CreateAssetMenu(fileName = "NewSpell", menuName = "Spell/Flavor Spell", order = 1)]
public class Spell : ScriptableObject
{
    #region Variables

    public enum SpellType { Other, Attack, Status, Buff, Debuff, Heal }
    public enum SpellTarget { SingleEnemy, RandomEnemy, AllEnemy, Self, SingleParty, AllParty, All }

    public const int DAMAGE_CONSTANT = 50;

    [PropertyOrder(0)] [GUIColor(0.98f, 0.95f, 0.5f)] public string spellName;

    [Header("Standard Spell Params")]
    [SerializeField] [PropertyOrder(1)] private SpellType spellType = SpellType.Other;
    [SerializeField] [PropertyOrder(1)] private SpellTarget spellTarget = SpellTarget.SingleEnemy;
    [PropertyOrder(1)] [Range(0, 250)] [GUIColor(0.05f, 0.80f, 0.85f)] public int spellCost;
    [PropertyOrder(2)] [Range(-6, 6)] [GUIColor(0.90f, 0.90f, 0.05f)] public int spellPriority = 0;
    [PropertyOrder(3)] [GUIColor(0.98f, 0.95f, 0.5f)] public string spellDescription;
    [PropertyOrder(4)] [GUIColor(0.98f, 0.95f, 0.5f)] public string spellCastMessage = "[user] casts [name]!";
    [PropertyOrder(5)] [GUIColor(0.98f, 0.95f, 0.5f)] public string spellFailMessage { get; protected set; }
    [PropertyOrder(6)] [InlineEditor] [GUIColor(0.80f, 0.65f, 0.98f)] public SpellFamily spellFamily;

#if UNITY_EDITOR

    private string spellFamilyButtonName = "Create New Spell Family";

    [PropertyOrder(7)] [Button(ButtonSizes.Small)]
    [GUIColor("CheckFamilyColor")]
    [LabelText("$spellFamilyButtonName")]
    [EnableIf("CheckFamilyName")]
    private void CreateNewSpellFamily()
    {
        if(spellFamily == null || SpellEditorUtilities.CheckIfAssetExists
            (spellFamily.familyName.Replace("Family", "").Replace(" ", "") + "Family", "Assets/Spells/Families/"))
        {
            spellFamily = ScriptableObject.CreateInstance<SpellFamily>();
            spellFamilyButtonName = "Create";
        }
        else
        {
            SpellEditorUtilities.CreateAsset(spellFamily, 
                "Assets/Spells/Families", spellFamily.familyName.Replace("Family", "").Replace(" ", "") + "Family");
            spellFamily = null;
            spellFamilyButtonName = "Create New Spell Family";
        }
    }

    private bool CheckFamilyName() { return spellFamily == null || spellFamily.familyName.Replace("Family", "").Trim() != ""; }
    private Color CheckFamilyColor() { return IsSpellFamilyValid() ? Color.white : Color.green; }

    private bool IsSpellFamilyValid()
    {
        return (spellFamily == null || SpellEditorUtilities.CheckIfAssetExists
            (spellFamily.familyName.Replace("Family", "").Trim() + "Family", "Assets/Spells/Families/"));
    }

#endif

    [PropertySpace(10)]
    [InlineEditor]
    [GUIColor(0.65f, 0.98f, 0.65f)]
    [PropertyOrder(8)] public AnimationSequenceObject spellAnimation;

#if UNITY_EDITOR

    private string spellAnimButtonName = "Create New Animation Sequence";

    [PropertyOrder(8)] [Button(ButtonSizes.Small)]
    [GUIColor("CheckAnimColor")]
    [LabelText("$spellAnimButtonName")]
    [EnableIf("CheckButtonStatus")]
    private void CreateNewAnimationSequence()
    {
        if (IsAnimationValid(false))
        {
            spellAnimation = ScriptableObject.CreateInstance<AnimationSequenceObject>();
            spellAnimation.animationName = this.spellName;
            spellAnimation.disableUI = true;

            spellAnimButtonName = "Create";
        }
        else
        {
            string animPath = SpellEditorUtilities.GetAssetPath(this);

            if(animPath != "")
            {
                animPath = animPath.Replace(this.name + ".asset", "");
                animPath = animPath.Substring(0, animPath.LastIndexOf("/"));
                SpellEditorUtilities.CreateAsset(spellAnimation, animPath, spellAnimation.animationName.Replace(" ", "") + "Anim");
                SpellEditorUtilities.CreateTextFile(animPath + "/" + spellAnimation.animationName.Replace(" ", "") + "AnimScript");
            }
            else
            {
                string slash = SpellEditorUtilities.currentPath == "" ? "" : "/" + SpellEditorUtilities.currentPath;
                string output = "Assets/Spells" + slash;

                SpellEditorUtilities.CreateAsset(spellAnimation, output,
                    spellAnimation.animationName.Replace(" ", "") + "Anim");
                SpellEditorUtilities.CreateTextFile(output + "/" + spellAnimation.animationName.Replace(" ", "") + "AnimScript");
            }

            spellAnimation = null;
            spellAnimButtonName = "Create New Animation Sequence";
        }
    }

    private bool CheckButtonStatus() { return IsAnimationValid(true); }
    private Color CheckAnimColor() { return IsAnimationValid(false) ? Color.white : Color.green; }


    private bool IsAnimationValid(bool inv)
    {
        if (spellAnimation == null || SpellEditorUtilities.DoesAssetExist(spellAnimation))
            return true;

        string animPath = SpellEditorUtilities.GetAssetPath(this);
        bool result = true;

        if (animPath != "")
        {
            animPath = animPath.Replace(this.name + ".asset", "");
            result = (SpellEditorUtilities.CheckIfAssetExists
                (spellAnimation.animationName.Trim() + "Anim", animPath));

            return inv ? !result : result;
        }

        result =  (SpellEditorUtilities.CheckIfAssetExists
            (spellAnimation.animationName.Trim() + "Anim", "Assets/Spells/" + SpellEditorUtilities.currentPath + "/"));
        return inv ? !result : result;
    }
#endif

    [Header("Effect Params")]
    [GUIColor(0.65f, 0.80f, 0.98f)]
    [PropertyOrder(9)] public List<SpellEffectChance> spellEffects; // Effects that can be invoked by the spell itself

    //[ListDrawerSettings(HideAddButton = , HideRemoveButton = )]
    [InlineEditor]
    [GUIColor(0.65f, 0.80f, 0.98f)]
    [PropertyOrder(10)] public List<Effect> spellProperties = new List<Effect>(); // Used to modify the damage roll

    #endregion



    /// <summary>
    /// Returns an instance of this spell using the spell data to calculate damage and effects
    /// </summary>
    public List<SpellCast> Cast(EntityController user, List<EntityController> targets)
    {
        List<SpellCast> result = new List<SpellCast>();

        foreach(var target in targets)
        {
            // Result of the cast spell
            SpellCast cast = new SpellCast();
            spellFailMessage = "";

            cast.spell = this;
            cast.user = user;
            cast.target = target;

            // Check for MP. If MP is inadequate, don't proceed.
            cast.success = CheckMP(user);

            if (cast.success)
            {
                // Check for additional requirements. If they aren't met, don't proceed.
                cast.success = CheckCanCast(user, target);

                if (cast.success)
                {
                    // Apply properties before dealing damage, as properties may affect damage or accuracy.
                    List<EffectInstance> properties = new List<EffectInstance>();

                    // Activate all properties on this spell
                    foreach (Effect e in spellProperties)
                    {
                        if (e != null && !properties.Exists(f => f.effect == e) || (properties.Exists(f => f.effect == e) && e.IsStackable()))
                        {
                            EffectInstance eff = e.CreateEffectInstance(user, target, cast);
                            eff.CheckSuccess();
                            eff.OnActivate();
                            properties.Add(eff);
                        }
                    }

                    // Get the user's active propeties. This will differ from the above list.
                    List<EffectInstance> userProperties = user.GetProperties();

                    foreach (EffectInstance ef in userProperties)
                    {
                        ef.CheckSuccess();
                        ef.OnActivate();
                        properties.Add(ef);
                    }

                    // Check for spell hit. If spell misses, don't proceed.
                    cast.success = CheckSpellHit(user, target);

                    if (cast.success)
                    {
                        // Calculate damage
                        CalculateDamage(user, target, cast);


                        cast.success = IsFlavorSpell() ? true : cast.HasDoneAnything();

                        if (!cast.success)
                            spellFailMessage = "";
                    }

                    // Deactivate all properties
                    foreach (EffectInstance e in properties)
                        e.OnDeactivate();

                    // Clear properties from player
                    user.ClearProperties();
                }
            }

            result.Add(cast);
        }
        
        // Return spell cast.
        return result;
    }


    /// <summary>
    /// Checks if the user has enough MP to cast this spell.
    /// </summary>
    public virtual bool CheckMP(EntityController user)
    {
        if(user.GetCurrentMP() >= spellCost)
        {
            user.ModifyMP(-spellCost);
            return true;
        }

        spellFailMessage = user.GetEntity().vals.entityName + " does not have enough MP!";
        return false;
    }


    /// <summary>
    /// Skeleton function. This checks additional requirements for the spell's success.
    /// </summary>
    public virtual bool CheckCanCast(EntityController user, EntityController target)
    {
        return true;
    }


    /// <summary>
    /// Skeleton function. This checks if the spell will hit the target.
    /// </summary>
    public virtual bool CheckSpellHit(EntityController user, EntityController target, float amount = -1)
    {
        return true;
    }


    /// <summary>
    /// Skeleton function. Calculates the damage dealt by the spell.
    /// </summary>
    public virtual void CalculateDamage(EntityController user, EntityController target, SpellCast cast)
    {
        int[] result = { 0 };
        bool[] critical = { true };
        cast.SetDamage(result);
        cast.SetCritical(critical);
    }


    /// <summary>
    /// Returns true if this spell is flavor text
    /// </summary>
    public virtual bool IsFlavorSpell()
    {
        return true;
    }


    public virtual int GetPower()
    {
        return -1;
    }


    public virtual int GetAccuracy()
    {
        return -1;
    }


    public SpellType GetSpellType()
    {
        return spellType;
    }
}


/// <summary>
/// Represents a specific instance of a cast spell.
/// </summary>
public class SpellCast
{
    // Was this cast successful?
    public bool success = false;

    /// <summary>
    /// The spell that was cast.
    /// </summary>
    public Spell spell;

    /// <summary>
    /// The user of the spell.
    /// </summary>
    public EntityController user;
    /// <summary>
    /// The targets of the spell.
    /// </summary>
    public EntityController target;

    /// <summary>
    /// Damage dealt by the spell
    /// </summary>
    private int[] damage;
    private int totalDamage = 0;
    /// <summary>
    /// Index of current hit. Used for animation processing.
    /// </summary>
    private int currentHit = 0;
    /// <summary>
    /// Indicates if any hit is critical
    /// </summary>
    public bool critical { get; private set; }
    /// <summary>
    /// Indicates which hit is critical
    /// </summary>
    private bool[] crits;
    /// <summary>
    /// Indicates if the spell hit
    /// </summary>
    private bool[] hits;

    /// <summary>
    /// Spell Effect params
    /// </summary>
    private List<EffectInstance> effects = new List<EffectInstance>();


    public virtual bool HasDoneAnything()
    {
        if (GetDamage() == 0 && !GetEffectProcSuccess() && (hits == null || (hits != null && hits.Length == 0)))
        {
            return false;
        }

        return true;
    }
    

    public virtual int GetDamage()
    {
        return totalDamage;
    }


    public virtual int GetDamage(int index)
    {
        return damage == null || index >= damage.Length || index < 0 ? 0 : damage[index];
    }


    public virtual int GetDamageOfCurrentHit()
    {
        int result = GetDamage(currentHit);

        return result;
    }


    public virtual bool GetHitSuccess(int index)
    {
        if (hits == null)
            return true;

        return index >= hits.Length || index < 0 ? false : hits[index];
    }


    public virtual bool GetCurrentHitSuccess()
    {
        bool result = GetHitSuccess(currentHit);

        return result;
    }


    public virtual bool SpellMissed()
    {
        bool result = false;

        if (hits == null)
            return result;

        foreach(var hit in hits)
        {
            if (hit)
                result = true;
        }

        return result;
    }


    public virtual int GetDamageOfPreviousHit()
    {
        int result = GetDamage(currentHit - 1);

        return result;
    }


    public virtual bool GetIsCurrentHitCritical()
    {
        bool result = false;

        if(crits != null && currentHit < crits.Length)
            result = crits[currentHit];

        return result;
    }


    public virtual void IncrementHit() { currentHit++; }
    // Original function commented out below. I have absolutely no clue why it was looking around like this.
    // public virtual void IncrementHit() { currentHit = damage != null && currentHit < damage.Length ? currentHit + 1 : 0; }


    /// <summary>
    /// Returns the amount of damage dealt by this hit, stopping when the target's HP is depleted.
    /// </summary>
    public virtual int GetDamageApplied()
    {
        int result = 0;

        if (damage == null)
            return result;

        for(int i=0; i<damage.Length; i++)
        {
            result += damage[i];

            if (target.GetCurrentHP() - result <= 0)
                return result;
        }

        return result;
    }
    

    /// <summary>
    /// Sets the damage of this SpellCast.
    /// For each hit of damage, roll for effect success.
    /// </summary>
    public virtual void SetDamage(int[] d)
    {
        damage = d;

        for (int i = 0; i < damage.Length; i++)
        {
            totalDamage += damage[i];

            for(int n = 0; n < spell.spellEffects.Count; n++)
            {
                Effect e = spell.spellEffects[n].GetEffect();

                float proc = Random.value;

                if (proc < spell.spellEffects[n].chance &&
                    (e != null && !effects.Exists(f => f.effect == e) || (effects.Exists(f => f.effect == e) && e.IsStackable())))
                {
                    EffectInstance eff = e.CreateEffectInstance(user, target, this);
                    eff.CheckSuccess();
                    effects.Add(eff);
                }
            }
        }
    }


    public virtual int GetNumHits()
    {
        return damage == null ? 0 : damage.Length;
    }


    /// <summary>
    /// Sets critical hit status.
    /// </summary>
    public virtual void SetCritical(bool[] c)
    {
        crits = c;

        for(int i=0; i<crits.Length; i++)
        {
            if (crits[i])
            {
                critical = true;
                return;
            }
        }
    }


    /// <summary>
    /// Sets hit status.
    /// </summary>
    public virtual void SetHits(bool[] h)
    {
        hits = h;
    }


    /// <summary>
    /// Returns true if any effect is invoked by this cast.
    /// </summary>
    public virtual bool GetEffectProcSuccess()
    {
        bool result = false;

        for(int i=0; i<effects.Count; i++)
        {
            if (effects[i].castSuccess)
                result = true;
        }

        return result;
    }


    public virtual List<EffectInstance> GetEffects()
    {
        return effects;
    }


    public string GetCastMessage()
    {
        string result = spell.spellCastMessage;
        var userVals = user.GetEntity().vals;
        var targetVals = target.GetEntity().vals;

        result = DialogueUtilities.ReplacePlaceholderWithEntityName(result, userVals, "[user]");
        result = DialogueUtilities.ReplacePlaceholderWithEntityName(result, targetVals, "[target]");
        result = DialogueUtilities.ReplacePlaceholderWithText(result, spell.spellName, "[name]");

        return result;
    }


    public string GetFailMessage()
    {
        string result = spell.spellFailMessage;
        var userVals = user.GetEntity().vals;
        var targetVals = target.GetEntity().vals;

        result = DialogueUtilities.ReplacePlaceholderWithEntityName(result, userVals, "[user]");
        result = DialogueUtilities.ReplacePlaceholderWithEntityName(result, targetVals, "[target]");
        result = DialogueUtilities.ReplacePlaceholderWithText(result, spell.spellName, "[name]");

        return result;
    }
}

/// <summary>
/// Master class that handles effect distribution for spells.
/// This is slightly confusing so here's an example:
/// Say we have a spell that has a 80% chance to invoke an effect.
/// Half of the time, that effect should do one thing, the other half of the time, another.
/// The SpellEffectChance chance would be 0.8, while the effect chance is 0.5 for each.
/// Keep in mind this will NOT automatically split so be careful.
/// </summary>
[System.Serializable]
public class SpellEffectChance
{
    [System.Serializable]
    [GUIColor(1.0f, 1.0f, 1.0f)]
    public class EffectChance
    {
        [Range(0, 1)]
        public float chance;
        
        [InlineEditor]
        public Effect effect;
    }

    // Odds of this effect set being invoked.
    [Range(0, 1)]
    [GUIColor(1.0f, 1.0f, 1.0f)] public float chance;

    [InfoBox("The sum of all effect chances is greater than 1. Some effects may not activate with the correct frequency.", 
        InfoMessageType.Warning, "CheckIfTotalChanceIsGreaterThanOne")]
    public EffectChance[] effects;

    private bool CheckIfTotalChanceIsGreaterThanOne()
    {
        float result = 0;

        for(int i=0; i<effects.Length; i++)
        {
            result += effects[i].chance;

            if (result > 1.0f)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a random effect from effects.
    /// </summary>
    public Effect GetEffect()
    {
        float rand = Random.value;
        float cur = 0;

        for(int i=0; i<effects.Length; i++)
        {
            cur += effects[i].chance;

            if (rand <= cur)
                return effects[i].effect;
        }

        return null;
    }
}
