using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains string representations of each event name.
/// This class was constructed in order to collect all events
/// into the same place.
/// </summary>
public static class EventConstants
{
    #region Game Event Constants
    public const string ON_BATTLE_BEGIN = "OnBattleBegin";
    public const string ON_TURN_BEGIN = "OnTurnBegin";
    public const string ON_MOVE_SELECTED = "OnMoveSelected";
    public const string ON_TURN_END = "OnTurnEnd";
    public const string ON_PLAYER_DEFEAT = "OnPlayerDefeat";
    public const string BEGIN_SEQUENCE = "BeginSequence";
    public const string ATTACK_SELECTED = "AttackSelected";
    public const string DEFEND_SELECTED = "DefendSelected";
    public const string ON_BUTTON_RELEASED = "OnButtonReleased";
    public const string DESELECT_BUTTON = "DeselectButton";
    public const string BEGIN_TRANSITION_IN = "BeginTransitionIn";
    public const string ON_TRANSITION_IN_COMPLETE = "OnTransitionInComplete";
    public const string BEGIN_TRANSITION_OUT = "BeginTransitionOut";
    public const string ON_TRANSITION_OUT_COMPLETE = "OnTransitionOutComplete";
    public const string HIDE_UI = "HideUI";
    #endregion

    #region Bool Event Constants
    public const string SET_UI_INPUT_STATE = "SetUIInputState";
    #endregion

    #region Int Event Constants
    public const string SPELL_SELECTED = "SpellSelected";
    #endregion

    #region String Event Constants
    public const string ON_DIALOGUE_QUEUE = "OnDialogueQueue";
    #endregion

    #region GameObject Event Constants
    public const string ON_BUTTON_PRESSED = "OnButtonPressed";
    #endregion

    #region Entity Controller Event Constants
    public const string ON_PLAYER_INITIALIZE = "OnPlayerInitialize";
    public const string ON_ENEMY_INITIALIZE = "OnEnemyInitialize";
    public const string ON_SPELL_LIST_INITIALIZE = "OnSpellListInitialize";
    public const string ON_ENEMY_DEFEAT = "OnEnemyDefeat";
    public const string OPEN_STATUS_SCREEN = "OpenStatusScreen";
    #endregion

    #region Sequence Event Constants
    public const string ON_SEQUENCE_QUEUE = "OnSequenceQueue";
    #endregion

    #region Spell Event Constants
    public const string SPELL_INFO_DISPLAY = "SpellInfoDisplay";
    #endregion

    #region Rect Transform Constants
    public const string ON_SPELL_INFO_APPEAR = "OnSpellInfoAppear";
    #endregion
}
