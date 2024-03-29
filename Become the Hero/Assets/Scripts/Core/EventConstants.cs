﻿using System.Collections;
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
    public const string RESET_BACKGROUND_COLOR = "ResetBackgroundColor";
    public const string ON_DIALOGUE_ADVANCE = "OnDialogueAdvance";
    public const string ON_ACTION_PHASE_BEGIN = "OnActionPhaseBegin";
    public const string CANCEL_PLAYER_SELECTION = "CancelPlayerSelection";
    public const string INITIALIZE_ALL_ENEMY_INFO = "InitializeAllEnemyInfo";
    #endregion

    #region Bool Event Constants
    public const string SET_UI_INPUT_STATE = "SetUIInputState";
    #endregion

    #region Int Event Constants
    public const string SPELL_SELECTED = "SpellSelected";
    public const string SPELL_SEALED = "SpellSealed";
    #endregion

    #region String Event Constants
    public const string ON_DIALOGUE_QUEUE = "OnDialogueQueue";
    public const string ON_MESSAGE_QUEUE = "OnMessageQueue";
    #endregion

    #region Vector2 Event Constants
    public const string START_BACKGROUND_FADE = "StartBackgroundFade";
    #endregion

    #region Vector3 Event Constants
    public const string SET_BACKGROUND_COLOR = "SetBackgroundColor";
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
    public const string INITIALIZE_TARGET_MENU = "InitializeTargetMenu";
    public const string INITIALIZE_PLAYER_INFO = "InitializePlayerInfo";
    public const string INITIALIZE_SELECTED_ENEMY_INFO = "InitializeSelectedEnemyInfo";
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

    #region UI Event Constants
    public const string SHOW_SCREEN = "ShowScreen";
    public const string HIDE_SCREEN = "HideScreen";
    public const string HIDE_ALL_SCREENS = "HideAllScreens";
    #endregion
}
