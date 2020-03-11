using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpellList", menuName = "Spell/Utilities/Player Spell List", order = 10)]
public class PlayerSpellList : ScriptableObject
{
    [SerializeField]
    private List<SpellOdds> spellList = new List<SpellOdds>();
    private List<Spell> doNotSelect = new List<Spell>();

    // Future-proofing: this will be used to prevent spells from appearing under certain conditions.
    private List<Spell> disabledSpells = new List<Spell>();


    /// <summary>
    /// Resets the disabled spells lists. This should be called at the beginning of the game.
    /// </summary>
    public void Initialize()
    {
        doNotSelect.Clear();
        disabledSpells.Clear();
    }


    public Spell[] GetSpellListForTurn()
    {
        Spell[] result = new Spell[4];

        List<Spell> spellPool = new List<Spell>();

        foreach(SpellOdds s in spellList)
        {
            // Skip adding the spell if it's part of the do not select list.
            if (doNotSelect.Contains(s.spell) || disabledSpells.Contains(s.spell))
                continue;

            for (int i = 0; i < s.relativeOdds; i++)
                spellPool.Add(s.spell);
        }

        Spell[] newDoNotSelect = new Spell[4];

        for(int i=0; i<result.Length; i++)
        {
            Spell randSpell = spellPool[Random.Range(0, spellPool.Count)];
            result[i] = randSpell;
            newDoNotSelect[i] = randSpell;

            if (randSpell.spellFamily != null)
                spellPool.RemoveAll(spell => spell.spellFamily == randSpell.spellFamily);
            else
                spellPool.RemoveAll(spell => spell == randSpell);
        }

        doNotSelect.Clear();
        doNotSelect.AddRange(newDoNotSelect);

        return result;
    }

    
    public SpellOdds GetSpellOdds(Spell s)
    {
        foreach (var so in spellList)
        {
            if (so.spell == s)
                return so;
        }

        return null;
    }


    public void AddNewSpell(Spell s, int odds)
    {
        spellList.Add(new SpellOdds(s, odds));
    }


    public void UpdateSpell(Spell s, int newOdds)
    {
        foreach (var odds in spellList)
        {
            if (odds.spell == s)
                odds.relativeOdds = newOdds;
        }
    }
}


[System.Serializable]
public class SpellOdds
{
    public Spell spell;
    [Range(0, 100)]
    public int relativeOdds = 1;

    public SpellOdds(Spell s, int odds)
    {
        spell = s;
        relativeOdds = odds;
    }
}
