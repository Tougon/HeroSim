using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Represents a <see cref="Spell"/> that deals no damage and applies a status effect.
/// </summary>
[CreateAssetMenu(fileName = "NewSpell", menuName = "Spell/Status Spell", order = 3)]
public class StatusSpell : Spell
{
    [Header("Base Damage Params")]

    public bool checkAccuracy = true;
    [Range(0, 100)]
    [ShowIf("checkAccuracy")]
    public float spellAccuracy = 100;
    
    /// <summary>
    /// Override for spell hit that factors accuracy
    /// </summary>
    public override bool CheckSpellHit(EntityController user, EntityController target)
    {
        if (!checkAccuracy)
            return true;

        float accuracy = user.GetAccuracy();
        float evasion = target.GetEvasion();

        float baseCheck = (accuracy / evasion);
        float hit = spellAccuracy * baseCheck;

        // Get user's accuracy modifiers to decrease hit chance.
        var accuracyModifiers = user.GetAccuracyModifiers();

        foreach (float f in accuracyModifiers)
            hit *= f;

        bool result = (Random.value * 100) <= hit;

        if (!result)
        {
            if (accuracy / evasion == 1.0f)
                spellFailMessage = Random.value < 0.5f ? "[user]'s attack missed!" : "[target] dodges the attack!";
            else
            {
                if (accuracy > evasion)
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

