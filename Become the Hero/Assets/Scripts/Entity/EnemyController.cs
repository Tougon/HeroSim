﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : EntityController
{
    // Start is called before the first frame update
    void Start()
    {
        if(current != null)
            EventManager.Instance.RaiseEntityControllerEvent(EventConstants.ON_ENEMY_INITIALIZE, this);
    }


    protected override void OnDeath()
    {
        base.OnDeath();

        EventManager.Instance.RaiseEntityControllerEvent(EventConstants.ON_ENEMY_DEFEAT, this);
    }

    public override void SelectAction()
    {
        // Until we have "real" AI, an enemy's choice of action should be random, but it should check if the effect will proc.
        action = current.moveList[UnityEngine.Random.Range(0, current.moveList.Count)];

        while (!CheckEffectProc())
            action = current.moveList[UnityEngine.Random.Range(0, current.moveList.Count)];
    }


    private bool CheckEffectProc()
    {
        bool result = true;

        foreach(var ec in action.spellEffects)
        {
            foreach (var e in ec.effects)
            {
                var effInst = e.effect.CreateEffectInstance(this, target, null);
                effInst.CheckSuccess();
                result = effInst.castSuccess;
            }
        }

        foreach(Effect p in action.spellProperties)
        {
            var effInst = p.CreateEffectInstance(this, target, null);
            effInst.CheckSuccess();
            result = effInst.castSuccess;
        }

        return result;
    }
}
