using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealManager : MonoBehaviour
{
    // TODO: use variable manager, maybe make this not constant?
    private const int COPYRIGHT_TERMS = 3;

    public class SealInstance
    {
        public EntityController sealEntity;
        public Spell sealSpellSource;
        public Effect sealEffect;
        public int sealTurnCount;
        public bool playerSide;
    }

    // List of all currently active seal instances
    private List<SealInstance> sealInstances;

    // List of all spells that have been sealed at all during the course of the battle
    private List<Spell> sealedSpells;


    public void OnBattleStart()
    {
        sealedSpells = new List<Spell>();
        sealInstances = new List<SealInstance>();
    }


    public bool CanSealSpell(Spell spell)
    {
        return !sealedSpells.Contains(spell);
    }


    public void CreateSealInstance(EntityController entity, Spell spellSource, Effect effect, bool playerSide)
    {
        if (effect == null) return;

        var sealInstance = new SealInstance()
        {
            sealEntity = entity,
            sealSpellSource = spellSource,
            sealEffect = effect,
            sealTurnCount = COPYRIGHT_TERMS,
            playerSide = playerSide,
        };

        sealInstances.Add(sealInstance);
    }


    public void OnEntityMove(EntityController entity)
    {
        for (int i = 0; i < sealInstances.Count; i++)
        {
            if (sealInstances[i].sealEntity == entity)
            {
                sealInstances[i].sealTurnCount--;

                if (sealInstances[i].sealTurnCount <= 0)
                {
                    // TODO: sequence effect
                    sealInstances.RemoveAt(i);
                    i--;
                }
            }
        }
    }


    public void OnEntityDefeated(EntityController entity)
    {
        for(int i=0; i<sealInstances.Count; i++)
        {
            if (sealInstances[i].sealEntity == entity)
            {
                // TODO: sequence effect
                sealInstances.RemoveAt(i);
                i--;
            }
        }
    }


    public void CheckForSeal(EntityController entity, bool playerSide)
    {
        var action = entity.action;

        // Realistically should never occur
        if (action == null) return;

        foreach(var seal in sealInstances)
        {
            if (seal.sealEntity == entity || seal.playerSide == playerSide) continue;

            foreach(var flag in action.flags)
            {
                if (seal.sealSpellSource.flags.Contains(flag))
                {
                    Debug.Log("GOTEM!");
                    EffectInstance eff = seal.sealEffect.CreateEffectInstance(
                        seal.sealEntity, entity, null);
                    eff.spellOverride = seal.sealSpellSource;
                    eff.CheckSuccess();
                    eff.OnActivate();
                    // TODO: Do not break, apply for each violation
                    return;
                }
            }
        }
    }
}
