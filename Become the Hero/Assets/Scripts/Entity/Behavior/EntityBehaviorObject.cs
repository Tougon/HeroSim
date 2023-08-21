using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// Represents an Entity's possible actions and move list.
/// </summary>
[CreateAssetMenu(fileName = "NewEntityBehavior", menuName = "Entity/Behavior/EntityBehavior", order = 0)]
public class EntityBehaviorObject : ScriptableObject
{
    public bool bUseFirstTurn;
    [ShowIf("bUseFirstTurn")]
    public EntityBehavior FirstTurnBehavior;

    [PropertySpace(10)]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<EntityBehavior> TurnBehavior;
}


[System.Serializable]
public class EntityBehavior
{
    public List<EntityBehaviorCheck> Behaviors = new List<EntityBehaviorCheck>();

    public EntityBehaviorCheck.Result GetResult(EntityController user, List<EntityController> targets, 
        List<EntityController> allies)
    {
        foreach(var behavior in Behaviors)
        {
            var result = behavior.DetermineAction(user, targets, allies);

            if (result.ActionSuccess) return result;
        }

        return new EntityBehaviorCheck.Result
        {
            ActionID = -1,
            ActionSuccess = false
        };
    }


    public static bool CheckTargetMatch(Spell spell, EntityBehaviorCheck.Result result)
    {
        switch (spell.GetSpellTarget())
        {
            case Spell.SpellTarget.Self:
                return result.Target == EntityBehaviorCheck.BehaviorCheck.CheckTarget.Self;

            case Spell.SpellTarget.SingleEnemy:
                return result.Target == EntityBehaviorCheck.BehaviorCheck.CheckTarget.Targets;

            case Spell.SpellTarget.SingleParty:
                return result.Target == EntityBehaviorCheck.BehaviorCheck.CheckTarget.Allies;
        }

        return false;
    }
}


[System.Serializable]
public class EntityBehaviorCheck
{
    /// <summary>
    /// Struct representing the result of a behavior check
    /// </summary>
    public struct Result
    {
        public int ActionID { get; set; }
        public bool ActionSuccess { get; set; }
        public EntityController TriggerEntity { get; set; }
        
        public BehaviorCheck.CheckTarget Target { get; set; }
    }


    [System.Serializable]
    public struct BehaviorCheck
    {
        public enum CheckType { HP, HasEffect }
        public enum CheckTarget { Self, Allies, Targets }

        public CheckType checkType;
        [ShowIf("@checkType == CheckType.HP || checkType == CheckType.HasEffect")]
        [HorizontalGroup("Primary", Gap = 3)]
        public CheckTarget target;
        [HorizontalGroup("Primary")]
        public bool negate;
        [ShowIf("@checkType == CheckType.HP")]
        [HorizontalGroup("Primary")]
        [Range(0, 1)]
        public float hpPercent;
        [ShowIf("@checkType == CheckType.HasEffect")]
        [HorizontalGroup("Primary")]
        public Effect effect;

        public bool Check(EntityController user, List<EntityController> targets, List<EntityController> allies, 
            ref Result result)
        {
            result.Target = target;

            switch (checkType)
            {
                case CheckType.HP:
                    return CheckHP(user, targets, allies, ref result);
                case CheckType.HasEffect:
                    return CheckEffect(user, targets, allies,  ref result);
            }

            result.TriggerEntity = null;
            result.Target = CheckTarget.Targets;
            return false;
        }


        private bool CheckHP(EntityController user, List<EntityController> targets, List<EntityController> allies,
            ref Result result)
        {
            if (target == CheckTarget.Self)
            {
                result.TriggerEntity = user;
                bool percentCheck = ((float)user.GetCurrentHP() / (float)user.maxHP) >= hpPercent;

                return negate ? !percentCheck : percentCheck;
            }
            else
            {
                foreach(var target in target == CheckTarget.Allies ? allies : targets)
                {
                    bool percentCheck = ((float)target.GetCurrentHP() / (float)target.maxHP) >= hpPercent;
                    bool val = negate ? !percentCheck : percentCheck;

                    if (val)
                    {
                        result.TriggerEntity = target;
                        return true;
                    }
                }
            }

            return false;
        }


        private bool CheckEffect(EntityController user, List<EntityController> targets, List<EntityController> allies,
            ref Result result)
        {
            if (target == CheckTarget.Self)
            {
                result.TriggerEntity = user;
                return negate ? !user.HasEffect(effect) : user.HasEffect(effect);
            }
            else
            {
                foreach (var target in target == CheckTarget.Allies ? allies : targets)
                {
                    bool val = negate ? !target.HasEffect(effect) : target.HasEffect(effect);

                    if (val)
                    {
                        result.TriggerEntity = target;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public List<BehaviorCheck> behaviorCheck = new List<BehaviorCheck>();
    public List<int> actionIndex = new List<int>();



    /// <summary>
    /// Check if this effect should be applied
    /// </summary>
    public Result DetermineAction(EntityController user, List<EntityController> targets, List<EntityController> allies)
    {
        var result = new Result
        {
            ActionID = -1,
            ActionSuccess = false,
            TriggerEntity = user
        };

        bool success = true;

        for (int i = 0; i < behaviorCheck.Count; i++)
        {
            success = behaviorCheck[i].Check(user, targets, allies, ref result);

            if (!success) break;
        }

        if (success)
        {
            result.ActionSuccess = true;
            result.ActionID = actionIndex[UnityEngine.Random.Range(0, actionIndex.Count)];
        }

        return result;
    }
}