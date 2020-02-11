using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpellList", menuName = "Spell/Utilities/Spell List", order = 9)]
public class SpellList : ScriptableObject
{
    public List<Spell> spells = new List<Spell>();


    public virtual Spell GetRandomSpell()
    {
        return spells[Random.Range(0, spells.Count)];
    }
}
