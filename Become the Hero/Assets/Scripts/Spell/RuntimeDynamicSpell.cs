using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a player's dynamically created spell.
/// This will include the spell itself, overwritten name, and seals (if applicable)
/// </summary>
[System.Serializable]
public class RuntimeDynamicSpell
{
    public Spell spell;
    public string overrideName;
    public Effect seal;
}
