using System.Collections;
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
    private int playerIndex;

    void Awake()
    {
        sequencer = GetComponent<Sequencer>();

        EventManager.Instance.GetGameEvent(EventConstants.ON_TRANSITION_IN_COMPLETE).AddListener(StartGame);
        EventManager.Instance.GetGameEvent(EventConstants.ON_BATTLE_BEGIN).AddListener(OnBattleBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).AddListener(OnTurnBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_END).AddListener(OnTurnEnd);
        EventManager.Instance.GetGameEvent(EventConstants.ON_ACTION_PHASE_BEGIN).AddListener(OnActionPhaseBegin);

        EventManager.Instance.GetSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE).AddListener(QueueSequence);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_INITIALIZE).AddListener(AddEnemy);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_PLAYER_INITIALIZE).AddListener(AddPlayer);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_DEFEAT).AddListener(EnemyDefeated);

        // Player action events
        EventManager.Instance.GetGameEvent(EventConstants.CANCEL_PLAYER_SELECTION).AddListener(OnCancelPlayerSelection);
        EventManager.Instance.GetGameEvent(EventConstants.ATTACK_SELECTED).AddListener(SetActionToAttack);
        EventManager.Instance.GetGameEvent(EventConstants.DEFEND_SELECTED).AddListener(SetActionToDefend);
        EventManager.Instance.GetIntEvent(EventConstants.SPELL_SELECTED).AddListener(SetActionToSpell);

        // Placeholder. The TurnManager WILL NOT handle scene transitions in the final game.
        EventManager.Instance.GetGameEvent(EventConstants.ON_TRANSITION_OUT_COMPLETE).AddListener(Reload);
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
    /// Returns the number of hostile entities
    /// </summary>
    public int GetNumEnemies() { return enemies.Count; }


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


    /// <summary>
    /// Returns the first Enemy in the list
    /// </summary>
    public EntityController GetEnemy(EntityController val)
    {
        foreach (EntityController ec in enemies)
        {
            if (ec != val)
                return ec;
        }

        return null;
    }


    private bool AllPlayersDefeated()
    {
        foreach(var player in players)
        {
            if (!player.dead)
                return false;
        }

        return true;
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

        for(int i=0; i<50; i++)
        {
            battleStart += " blah";
        }

        EventManager.Instance.RaiseUIGameEvent(EventConstants.SHOW_SCREEN,
            new UIOpenCloseCall
            {
                MenuName = ScreenConstants.TextDisplay.ToString()
            });

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
        // Await any other actions
        yield return null;

        List<EntityController> allies = new List<EntityController>();
        List<EntityController> targets = new List<EntityController>();

        // Reset all entity actions for the turn.
        foreach (EntityController ec in entities)
        {
            ec.target.Clear();
            ec.ResetDamageTaken();
            ec.ResetAction();
            ec.acceptTouch = true;
            ec.ready = false;

            if (!ec.dead)
            {
                if (players.Contains(ec)) allies.Add(ec);
                if (enemies.Contains(ec)) targets.Add(ec);
            }
        }

        playerIndex = 0;
        if (players.Count > 0)
        {
            players[playerIndex].allies = allies;
            players[playerIndex].enemies = targets;
            RefreshUIForPlayer(players[playerIndex]);
        }

        // Loop while a player has not chosen their action for the turn
        while (playerIndex < players.Count)
        {
            if (players[playerIndex].ready || (!players[playerIndex].ready && players[playerIndex].dead))
            {
                playerIndex++;

                if (playerIndex < players.Count)
                {
                    players[playerIndex].allies = allies;
                    players[playerIndex].enemies = targets;
                    RefreshUIForPlayer(players[playerIndex]);
                }
            }

            yield return null;
        }

        // Select an action for all AI entities
        foreach (EntityController ec in enemies)
        {
            // TODO: modify targetting behavior
            ec.allies = targets;
            ec.enemies = allies;

            // See above. This is not a substitute for proper targetting behavior and will be removed
            List<EntityController> targetTemp = new List<EntityController>();

            foreach (var player in players)
                targetTemp.Add(player);

            ec.target = targetTemp;
            ec.SelectAction();
            ec.SetTarget();
            ec.ready = true;
        }

        EventManager.Instance.RaiseGameEvent(EventConstants.ON_MOVE_SELECTED);
        EventManager.Instance.RaiseUIGameEvent(EventConstants.HIDE_ALL_SCREENS,
            new UIOpenCloseCall
        {
            Callback = () =>
            {
                EventManager.Instance.RaiseGameEvent(EventConstants.ON_ACTION_PHASE_BEGIN);
            }
        });
    }

    #endregion


    #region Player Action

    public void SetActionToSpell(int index)
    {
        if (playerIndex >= players.Count)
            return;

        players[playerIndex].SelectAction(index);
        
        EventManager.Instance.RaiseUIGameEvent(EventConstants.HIDE_ALL_SCREENS,
            new UIOpenCloseCall
        {
            Callback = () =>
            {
                EventManager.Instance.RaiseEntityControllerEvent
                    (EventConstants.INITIALIZE_TARGET_MENU, players[playerIndex]);
                EventManager.Instance.RaiseUIGameEvent(EventConstants.SHOW_SCREEN,
                    new UIOpenCloseCall
                {
                    MenuName = ScreenConstants.TargetMenu.ToString()
                });
            }
        });
    }


    public void SetActionToAttack()
    {
        if (playerIndex >= players.Count)
            return;

        players[playerIndex].SelectAction("attack");
        
        EventManager.Instance.RaiseUIGameEvent(EventConstants.HIDE_ALL_SCREENS,
            new UIOpenCloseCall
        {
            Callback = () =>
            {
                EventManager.Instance.RaiseEntityControllerEvent
                    (EventConstants.INITIALIZE_TARGET_MENU, players[playerIndex]);
                EventManager.Instance.RaiseUIGameEvent(EventConstants.SHOW_SCREEN,
                    new UIOpenCloseCall
                {
                    MenuName = ScreenConstants.TargetMenu.ToString()
                });
            }
        });

        // TODO: Remove when targetting is a thing
        //players[playerIndex].ready = true;

    }


    public void SetActionToDefend()
    {
        if (playerIndex >= players.Count)
            return;

        players[playerIndex].SelectAction("defend");
    }


    private void RefreshUIForPlayer(EntityController controller)
    {
        // First, close the current menu. This is so that there's any feedback at all, but may be reworked.
        // It will look a little silly to open and immediately reopen the same menu.
        EventManager.Instance.RaiseUIGameEvent(EventConstants.HIDE_ALL_SCREENS,
            new UIOpenCloseCall
        {
            Callback = () =>
            {
                // Set if first player
                VariableManager.Instance.SetBoolVariableValue(VariableConstants.IS_FIRST_PLAYER, playerIndex == 0);

                // Refresh the spell menu
                EventManager.Instance.RaiseEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE, controller);

                // Open the root menu
                EventManager.Instance.RaiseUIGameEvent(EventConstants.SHOW_SCREEN,
                    new UIOpenCloseCall
                {
                    MenuName = ScreenConstants.ActionMenu.ToString()
                });
            }
        });
    }


    private void OnCancelPlayerSelection()
    {
        Debug.Log("Canceling selection for last player");

        playerIndex = Mathf.Clamp(playerIndex - 1, 0, players.Count);
        players[playerIndex].ready = false;

        // Refresh the UI after canceling
        RefreshUIForPlayer(players[playerIndex]);
    }

    #endregion


    #region Turn

    public void OnActionPhaseBegin()
    {
        foreach (EntityController ec in entities)
        {
            ec.ExecuteTurnStartEffects();
            ec.acceptTouch = false;
        }

        // Cancel current routine and begin action phase
        if (current != null)
            StopCoroutine(current);

        current = ActionSequence();
        StartCoroutine(current);
    }


    private IEnumerator ActionSequence()
    {
        EventManager.Instance.RaiseUIGameEvent(EventConstants.SHOW_SCREEN,
        new UIOpenCloseCall
        {
            MenuName = ScreenConstants.TextDisplay.ToString()
        });

        // Determine turn order using our custom compare method
        List<EntityController> turnOrder = new List<EntityController>();

        foreach (EntityController ec in entities)
        {
            turnOrder.Add(ec);
        }

        turnOrder.Sort((x, y) => x.CompareTo(y));


        // Execute each entity's action
        foreach(EntityController ec in turnOrder)
        {
            // We need to check for target death later.
            if (ec.dead)
                continue;

            bool bValidTargets = true;

            foreach(var target in ec.target)
            {
                if(target.dead)
                {
                    bValidTargets = true;
                    break;
                }
            }

            // Tweak later so that the target can change if there are no valid targets
            if (!bValidTargets)
                continue;

            ec.ExecuteMoveSelectedEffects();

            // Cast the spell
            List<SpellCast> spellCast = ec.action.Cast(ec, ec.target);
            ec.actionResult = spellCast;

            List<string> preAnimDialogue = new List<string>();
            List<string> postAnimDialogue = new List<string>();
            // Originally checked for a spell success state before animating. May restore this, but should be separate
            bool bWillPlayAnimation = true;

            foreach(var cast in spellCast)
            {
                cast.target.IncreaseDamageTaken(cast.GetDamageApplied());

                string dialogueSeq = cast.GetCastMessage();
                if (!preAnimDialogue.Contains(dialogueSeq))
                    preAnimDialogue.Add(dialogueSeq);

                if (!cast.GetFailMessage().Equals(""))
                {
                    postAnimDialogue.Add(cast.GetFailMessage());
                }

                if(cast.spell is OffensiveSpell)
                {
                    // Prepare damage messages
                    if (cast.GetDamageApplied() > 0)
                    {
                        if (cast.critical)
                        {
                            string critSeq = spellCast.Count > 1 ? $"Critical Hit on {cast.target.param.GetEntityName()}!" : "Critical Hit!";
                            postAnimDialogue.Add(critSeq);
                        }

                        string damageSeq = cast.target.param.GetEntityName() + " takes " + cast.GetDamageApplied() + " damage!";
                        postAnimDialogue.Add(damageSeq);
                    }
                    else
                    {
                        // Check if all hits of the spell missed
                        if(cast.SpellHit())
                        {
                            string critSeq = $"{cast.target.param.GetEntityName()} blocks the attack!";
                            postAnimDialogue.Add(critSeq);
                        }
                    }
                }
            }

            foreach(var msg in preAnimDialogue)
                EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, msg);

            if(bWillPlayAnimation)
            {
                Sequence animationSeq = new AnimationSequence(ec.action.spellAnimation, ec, ec.target, spellCast);
                EventManager.Instance.RaiseSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE, animationSeq);
            }

            foreach (var msg in postAnimDialogue)
                EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, msg);

            // Start the sequence
            sequencer.StartSequence();

            // Wait until sequence is done
            while (sequencer.active)
                yield return null;

            // apply effects from the spell if the target is still alive.
            // TODO: try and get this within the main loop if at all possible
            foreach (var spell in spellCast)
            {
                List<EffectInstance> effects = spell.GetEffects();

                foreach (EffectInstance ef in effects)
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

        // Call game over all players are defeated. Player is able to be revived through certain spells.
        if (AllPlayersDefeated())
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
        EventManager.Instance.GetGameEvent(EventConstants.ON_MOVE_SELECTED).RemoveListener(OnActionPhaseBegin);

        EventManager.Instance.GetSequenceGameEvent(EventConstants.ON_SEQUENCE_QUEUE).RemoveListener(QueueSequence);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_INITIALIZE).RemoveListener(AddEnemy);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_PLAYER_INITIALIZE).RemoveListener(AddPlayer);
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_ENEMY_DEFEAT).RemoveListener(EnemyDefeated);

        EventManager.Instance.GetGameEvent(EventConstants.CANCEL_PLAYER_SELECTION).RemoveListener(OnCancelPlayerSelection);
        EventManager.Instance.GetGameEvent(EventConstants.ATTACK_SELECTED).RemoveListener(SetActionToAttack);
        EventManager.Instance.GetGameEvent(EventConstants.DEFEND_SELECTED).RemoveListener(SetActionToDefend);
        EventManager.Instance.GetIntEvent(EventConstants.SPELL_SELECTED).RemoveListener(SetActionToSpell);

        // Placeholder. The TurnManager WILL NOT handle scene transitions in the final game.
        EventManager.Instance.GetGameEvent(EventConstants.ON_TRANSITION_OUT_COMPLETE).RemoveListener(Reload);
    }
}
