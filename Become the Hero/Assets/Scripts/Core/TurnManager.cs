﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hero.Core;

public class TurnManager : MonoBehaviour
{
    private List<EntityController> entities = new List<EntityController>();
    private List<EntityController> players = new List<EntityController>();
    private List<EntityController> enemies = new List<EntityController>();
    private Sequencer sequencer;
    private IEnumerator current;

    void Awake()
    {
        sequencer = GetComponent<Sequencer>();

        EventManager.Instance.GetGameEvent(EventConstants.ON_TRANSITION_IN_COMPLETE).AddListener(StartGame);
        EventManager.Instance.GetGameEvent(EventConstants.ON_BATTLE_BEGIN).AddListener(OnBattleBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).AddListener(OnTurnBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_END).AddListener(OnTurnEnd);
        EventManager.Instance.GetGameEvent(EventConstants.ON_MOVE_SELECTED).AddListener(OnMoveSelected);

        // Placeholder. The TurnManager WILL NOT handle scene transitions in the final game.
        EventManager.Instance.GetGameEvent(EventConstants.ON_TRANSITION_OUT_COMPLETE).AddListener(Reload);

        EventManager.Instance.GetSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE).AddListener(QueueSequence);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_INITIALIZE).AddListener(AddEnemy);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_PLAYER_INITIALIZE).AddListener(AddPlayer);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_DEFEAT).AddListener(EnemyDefeated);
    }

    
    public void StartGame()
    {
        // Begin Battle through event
        EventManager.Instance.RaiseGameEvent(EventConstants.ON_BATTLE_BEGIN);
    }

    // Placeholder function
    public void Reload()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }


    /// <summary>
    /// Returns the number of active entities
    /// </summary>
    public int GetNumEntities() { return entities.Count; }


    /// <summary>
    /// Add a player controlled entity
    /// </summary>
    public void AddPlayer(EntityController ec)
    {
        ec.SetTurnManager(this);
        entities.Add(ec);
        players.Add(ec);
    }


    /// <summary>
    /// Add an AI controlled entity
    /// </summary>
    public void AddEnemy(EntityController ec)
    {
        // Note that we need to add A/B/C etc. for duplicate enemies.
        ec.SetTurnManager(this);
        entities.Add(ec);
        enemies.Add(ec);
    }


    /// <summary>
    /// Returns the first EntityController in the list that is not the given EntityController
    /// </summary>
    public EntityController GetOther(EntityController val)
    {
        foreach(EntityController ec in entities)
        {
            if (ec != val)
                return ec;
        }

        return null;
    }


    public void QueueSequence(Sequence s)
    {
        sequencer.AddSequence(s);
    }


    #region Battle Start

    public void OnBattleBegin()
    {
        if (current != null)
            StopCoroutine(current);

        current = BattleStartSequence();
        StartCoroutine(current);
    }

    private IEnumerator BattleStartSequence()
    {
        while (entities.Count < 2)
            yield return null;
        
        string battleStart = "";
        var e = enemies[0].GetEntity().vals;

        // May eventually change this to a loop/account for duplicates/variety
        battleStart = e.GetEntityName();
        battleStart += " approaches!";

        EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, battleStart);
        sequencer.StartSequence();

        // Start a coroutine that waits for the sequence to end. Once it ends, invoke the next event.
        if (current != null)
            StopCoroutine(current);

        current = InvokeOnSequenceEnd(EventConstants.ON_TURN_BEGIN);
        StartCoroutine(current);
    }

    #endregion


    #region Turn Start

    public void OnTurnBegin()
    {
        if (current != null)
            StopCoroutine(current);

        current = TurnStartSequence();
        StartCoroutine(current);
    }

    private IEnumerator TurnStartSequence()
    {
        // Reset all entity actions for the turn.
        foreach (EntityController ec in entities)
        {
            ec.target = null;
            ec.ResetDamageTaken();
            ec.ResetAction();
            ec.acceptTouch = true;
        }

        int index = 0;

        // Loop while a player has not chosen their action for the turn
        while(index < players.Count)
        {
            if (players[index].action != null)
                index++;

            yield return null;
        }

        // Select an action for all AI entities
        foreach (EntityController ec in enemies)
        {
            ec.target = players[Random.Range(0, players.Count)];
            ec.SelectAction();
        }

        EventManager.Instance.RaiseGameEvent(EventConstants.ON_MOVE_SELECTED);
    }

    #endregion


    #region Turn

    public void OnMoveSelected()
    {
        foreach (EntityController ec in entities)
        {
            ec.ExecuteTurnStartEffects();
            ec.acceptTouch = false;
        }


        if (current != null)
            StopCoroutine(current);

        current = ActionSequence();
        StartCoroutine(current);
    }


    private IEnumerator ActionSequence()
    {
        // Determine turn order using our custom compare method
        List<EntityController> turnOrder = new List<EntityController>();

        foreach (EntityController ec in entities)
            turnOrder.Add(ec);

        turnOrder.Sort((x, y) => x.CompareTo(y));


        // Execute each entity's action
        foreach(EntityController ec in turnOrder)
        {
            // We need to check for target death later.
            if (ec.dead)
                continue;

            // Tweak later so that the target can change
            if (ec.target.dead)
                continue;

            ec.ExecuteMoveSelectedEffects();

            // Cast the spell
            SpellCast spellCast = ec.action.Cast(ec, ec.target);
            ec.actionResult = spellCast;
            ec.target.IncreaseDamageTaken(spellCast.GetDamageApplied());

            // NOTE: Replace with a spell specific method later because "Player casts attack!" sounds awful.
            string dialogueSeq = spellCast.GetCastMessage();
            EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, dialogueSeq);

            // If spell is successful, queue up its animation
            if (spellCast.success)
            {
                Sequence animationSeq = new AnimationSequence(spellCast.spell.spellAnimation, ec, ec.target, spellCast);
                EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, animationSeq);
            }
            // Otherwise, output a failure message
            else if(!spellCast.GetFailMessage().Equals(""))
                EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, spellCast.GetFailMessage());

            // Start the sequence
            sequencer.StartSequence();

            // Output damage messages
            if(spellCast.GetDamageApplied() > 0)
            {
                if (spellCast.critical)
                {
                    string critSeq = "Critical Hit!";
                    EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, critSeq);
                }
                
                string damageSeq = ec.target.param.GetEntityName() + " takes " + spellCast.GetDamageApplied() + " damage!";
                EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, damageSeq);
            }

            // Wait until sequence is done
            while (sequencer.active)
                yield return null;

            // apply effects from the spell if the target is still alive.

            List<EffectInstance> effects = spellCast.GetEffects();

            foreach(EffectInstance ef in effects)
            {
                if (ef.castSuccess)
                    ef.OnActivate();
                else
                    ef.OnFailedToActivate();
            }

            sequencer.StartSequence();

            while (sequencer.active)
                yield return null;
        }

        yield return null;

        if (current != null)
            StopCoroutine(current);

        current = InvokeOnSequenceEnd(EventConstants.ON_TURN_END);
        StartCoroutine(current);
    }

    #endregion


    #region End Turn

    /// <summary>
    /// Queues up enemy defeated dialogue sequences
    /// </summary>
    public void EnemyDefeated(EntityController ec)
    {
        // NOTE: Replace with special dialogue method later for better/more interesting writing
        string dialogueSeq = ec.param.entityName + " is defeated!";
        EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, dialogueSeq);
    }


    public void OnTurnEnd()
    {
        if (current != null)
            StopCoroutine(current);

        current = TurnEndSequence();
        StartCoroutine(current);
    }


    private IEnumerator TurnEndSequence()
    {
        // Do turn end things
        foreach (EntityController ec in entities)
        {
            // Remove effects that need to be removed
            ec.ExecuteRemainActiveCheck();

            // turn end stuff
            ec.ExecuteTurnEndEffects();
        }

        sequencer.StartSequence();

        while (sequencer.active)
            yield return null;

        // Multiplayer: check for a winner

        // Call game over if the player is still dead. Player is able to be revived through certain spells.
        if (players[0].dead)
        {
            // This will trigger a Game Over sequence on the UI controller. For now, I'm just going to reload the scene.
            EventManager.Instance.RaiseGameEvent(EventConstants.ON_PLAYER_DEFEAT);
            EventManager.Instance.RaiseGameEvent(EventConstants.BEGIN_TRANSITION_OUT);
        }
        else
        {
            bool spawnEnemies = true;

            foreach(EntityController ec in enemies)
            {
                if (!ec.dead)
                    spawnEnemies = false;
            }

            // Load enemies if the player is not dead and enemies should be loaded
            // Invoke battle start if new enemies are spawned, turn begin otherwise
            if (spawnEnemies)
            {
                enemies[0].SetEntity(EnemyManager.Instance.GetRandomEnemy());
                EventManager.Instance.RaiseGameEvent(EventConstants.ON_BATTLE_BEGIN);
            }
            else
            {
                EventManager.Instance.RaiseGameEvent(EventConstants.ON_TURN_BEGIN);
            }
        }

        Resources.UnloadUnusedAssets();
    }

    #endregion


    /// <summary>
    /// Used to suspend processing of a turn until a sequence ends
    /// </summary>
    /// <param name="eventName">Event to invoke upon sequence completion</param>
    private IEnumerator InvokeOnSequenceEnd(string eventName)
    {
        while (sequencer.active)
            yield return null;
        
        EventManager.Instance.RaiseGameEvent(eventName);
    }


    /// <summary>
    /// Remove all event listeners
    /// </summary>
    private void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_TRANSITION_IN_COMPLETE).RemoveListener(StartGame);
        EventManager.Instance.GetGameEvent(EventConstants.ON_BATTLE_BEGIN).RemoveListener(OnBattleBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).RemoveListener(OnTurnBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_END).RemoveListener(OnTurnEnd);
        EventManager.Instance.GetGameEvent(EventConstants.ON_MOVE_SELECTED).RemoveListener(OnMoveSelected);

        // Placeholder. The TurnManager WILL NOT handle scene transitions in the final game.
        EventManager.Instance.GetGameEvent(EventConstants.ON_TRANSITION_OUT_COMPLETE).RemoveListener(Reload);

        EventManager.Instance.GetSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE).RemoveListener(QueueSequence);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_INITIALIZE).RemoveListener(AddEnemy);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_PLAYER_INITIALIZE).RemoveListener(AddPlayer);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_DEFEAT).RemoveListener(EnemyDefeated);
    }
}
