using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Represents a <see cref="Spell"/> that can deal damage.
/// </summary>
[CreateAssetMenu(fileName = "NewSpell", menuName = "Spell/Offensive Spell", order = 2)]
public class OffensiveSpell : Spell
{
    public enum SpellHitType { Physical, Special }

    [Header("Base Params")]

    [Range(0, 250)][GUIColor(0.90f, 0.45f, 0.45f)]
    public float spellPower = 50.0f;
    [GUIColor(0.90f, 0.45f, 0.45f)]
    public SpellHitType spellAttackType;
    [GUIColor(0.90f, 0.45f, 0.45f)]
    public bool varyDefenseType = false;
    [GUIColor(0.90f, 0.45f, 0.45f)]
    [ShowIf("varyDefenseType")]
    public SpellHitType spellDefenseType;

    [Header("Accuracy Params")]
    [GUIColor(0.90f, 0.90f, 0.05f)]
    public bool checkAccuracy = true;
    [Range(0, 100)]
    [ShowIf("checkAccuracy")][GUIColor(0.90f, 0.90f, 0.05f)]
    public float spellAccuracy = 100;
    [Range(0, 100)]
    [ShowIf("checkAccuracy")][GUIColor(0.90f, 0.90f, 0.05f)]
    public float accuracyPerHit = 100;
    [ShowIf("checkAccuracy")][GUIColor(0.90f, 0.90f, 0.05f)]
    public bool checkAccuracyPerHit = true;


    [Header("Multi-Hit Params")]
    [Range(0, 10)]
    [ValidateInput("ValidateMinHitCount")][OnValueChanged("OnMinHitCountChanged")]
    [GUIColor(0.65f, 0.90f, 0.70f)]
    public int minNumberOfHits = 1;

    private bool ValidateMinHitCount(int value) { return value <= maxNumberOfHits; }
    private void OnMinHitCountChanged() { if (!ValidateMinHitCount(minNumberOfHits)) minNumberOfHits = maxNumberOfHits; }


    [Range(0, 10)]
    [ValidateInput("ValidateMaxHitCount")][OnValueChanged("OnMaxHitCountChanged")]
    [GUIColor(0.65f, 0.90f, 0.70f)]
    public int maxNumberOfHits = 1;

    private bool ValidateMaxHitCount(int value) { return value >= minNumberOfHits; }
    private void OnMaxHitCountChanged() { if (!ValidateMaxHitCount(maxNumberOfHits)) maxNumberOfHits = minNumberOfHits; }


    // If checked, hit count will vary between the min and max number of hits.
    [ShowIf("@maxNumberOfHits != minNumberOfHits")]
    [GUIColor(0.65f, 0.90f, 0.70f)]
    public bool varyHitCount = false;
    [ShowIf("@maxNumberOfHits != minNumberOfHits")]
    [GUIColor(0.65f, 0.90f, 0.70f)]
    public AnimationCurve hitCountCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    [Header("Critical Hit Params")]
    [GUIColor(0.90f, 0.45f, 0.05f)]
    public bool canCritical = true;

    [Range(1, 24)]
    [ShowIf("canCritical")]
    [GUIColor(0.90f, 0.45f, 0.05f)]
    public int criticalHitChance = 16;


    /// <summary>
    /// Override for spell hit that factors accuracy
    /// </summary>
    public override bool CheckSpellHit(EntityController user, EntityController target, float amount = -1)
    {
        if (!checkAccuracy)
            return true;

        float accuracy = user.GetAccuracy();
        float evasion = target.GetEvasion();

        float baseCheck = (accuracy / evasion);
        float hit = (amount != -1 ? amount : spellAccuracy) * baseCheck;

        // Get user's accuracy modifiers to decrease hit chance.
        var accuracyModifiers = user.GetAccuracyModifiers();
        
        foreach (float f in accuracyModifiers)
            hit *= f;
        
        bool result = (Random.value * 100) <= hit;

        if (!result)
        {
            if(accuracy / evasion == 1.0f)
                spellFailMessage = Random.value < 0.5f ? "[user]'s attack missed!" : "[target] dodges the attack!";
            else
            {
                if(accuracy > evasion)
                {
                    spellFailMessage = Random.value < (evasion / accuracy) ? "[user]'s attack missed!" : "[target] dodges the attack!";
                }
                else
                {
                    spellFailMessage = Random.value < (accuracy / evasion) ? "[target] dodges the attack!" : "[user]'s attack missed!";
                }
            }
        }

        return result;
    }


    /// <summary>
    /// Override for damage calculation that factors accuracy
    /// </summary>
    public override void CalculateDamage(EntityController user, EntityController target, SpellCast cast)
    {
        // Use the maximum number of htis
        int numHits = maxNumberOfHits;

        // If hit count is to be varied, randomize the number of hits here.
        if (varyHitCount)
        {
            if (minNumberOfHits > maxNumberOfHits)
                minNumberOfHits = maxNumberOfHits;

            float time = hitCountCurve.Evaluate(Random.value);
            numHits = Mathf.RoundToInt(Mathf.Lerp(minNumberOfHits, maxNumberOfHits, time));
        }

        SpellHitType attackType = this.spellAttackType;
        SpellHitType defenseType = this.varyDefenseType ? this.spellDefenseType : this.spellAttackType;

        int[] result = new int[numHits];
        bool[] crits = new bool[numHits];
        bool[] hit = new bool[numHits];

        // Run damage calculation for each hit
        for(int i=0; i<result.Length; i++)
        {
            if(checkAccuracyPerHit && !CheckSpellHit(user, target, accuracyPerHit))
            {
                // TODO: Accuracy check
                continue;
            }

            hit[i] = true;
            float critChance = user.param.entityCritModifier == 0 ? 0 :
                (float)criticalHitChance / user.param.entityCritModifier;

            // Indicate if this hit is critical
            bool critical = false;

            if(critChance > 0)
                critical = canCritical && (Random.value < (1.0f / critChance));
            crits[i] = critical;

            // Get attack and defense modifications
            float atkMod = attackType == SpellHitType.Physical ? 
                user.GetAttackModifier() : user.GetMagicAttackModifier();
            float defMod = defenseType == SpellHitType.Physical ? 
                target.GetDefenseModifier() : target.GetMagicDefenseModifier();

            float attack = attackType == SpellHitType.Physical ?
                user.GetAttack() : user.GetMagicAttack();
            float defense = defenseType == SpellHitType.Physical ?
                target.GetDefense() : target.GetMagicDefense();

            // Negate negative attack and positive defense mods if the hit is critical
            atkMod = critical && atkMod < 1.0f ? 1.0f : atkMod;
            defMod = critical && defMod > 1.0f ? 1.0f : defMod;

            // Calculate damage
            float damage = ((((2 * DAMAGE_CONSTANT) / 5 + 2) * spellPower *
                ((attack * atkMod) / (defense * defMod))) / 50.0f);

            // Other modifier applied at the end. Includes critical hit and move specific modifiers
            var offenseModifiers = attackType == SpellHitType.Physical ? 
                user.GetAttackModifiers() : user.GetMagicAttackModifiers();
            var defenseModifiers = defenseType == SpellHitType.Physical ?
                target.GetDefenseModifiers() : target.GetMagicDefenseModifiers();

            // Modify the damage based on the active offensive and defensive modifiers
            foreach (float f in offenseModifiers)
                damage *= f;

            foreach (float d in defenseModifiers)
                damage *= d;

            damage *= Random.Range(0.85f, 1.0f);
            damage = critical ? damage * 1.5f : damage;

            result[i] = Mathf.RoundToInt(damage);
        }

        // Set the damage and critical
        cast.SetDamage(result);
        cast.SetCritical(crits);
        cast.SetHits(hit);
    }


    public override int GetPower()
    {
        return (int)spellPower;
    }


    public override int GetAccuracy()
    {
        if (!checkAccuracy)
            return -1;

        return (int)spellAccuracy;
    }

    public override bool IsFlavorSpell()
    {
        return false;
    }
}
