using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using ToUI;

public class BattleUIController : MonoBehaviour
{
    private GraphicRaycaster input;

    [SerializeField]
    private DialogueManager dialogueManager;

    [SerializeField]
    private UIMenu playerMenu;

    [SerializeField]
    private UIMenu spellMenu;

    private bool spellOpen = false;


    // Start is called before the first frame update
    void Awake()
    {
        input = GetComponent<GraphicRaycaster>();

        VariableManager.Instance.SetBoolVariableValue(VariableConstants.TEXT_BOX_IS_ACTIVE, false);
        EventManager.Instance.GetGameEvent(EventConstants.ON_BATTLE_BEGIN).AddListener(OnBattleBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).AddListener(OnTurnBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_MOVE_SELECTED).AddListener(OnMoveSelected);

        Input.multiTouchEnabled = false;
    }


    public void OnBattleBegin()
    {
        spellOpen = false;

        dialogueManager.OnScreenShowDelegate += OnTextBoxAppear;
        dialogueManager.Show();
        //textBoxRect.DOAnchorPosY(activePosY, 1.0f).
            //OnComplete(OnTextBoxAppear);
    }


    public void OnTextBoxAppear()
    {
        SetTextBoxActiveState(true);
    }


    private void SetTextBoxActiveState(bool val)
    {
        VariableManager.Instance.SetBoolVariableValue(VariableConstants.TEXT_BOX_IS_ACTIVE, val);
    }


    public void OnTurnBegin()
    {
        spellOpen = false;
        SetInputState(false);
        SetTextBoxActiveState(false);

        dialogueManager.OnScreenHideDelegate += OpenPlayerMenu;
        dialogueManager.Hide();
        //textBoxRect.DOAnchorPosY(-activePosY, 0.5f).
            //OnComplete(OpenPlayerMenu).SetEase(Ease.InSine);
    }


    public void OnMoveSelected()
    {
        if (spellOpen)
        {
            spellMenu.OnScreenHideDelegate += OpenTextBox;
            spellMenu.Hide();
        }
        else
        {
            playerMenu.OnScreenHideDelegate += OpenTextBox;
            playerMenu.Hide();
        }
    }


    public void OpenTextBox()
    {
        dialogueManager.OnScreenShowDelegate += EnableInput;
        dialogueManager.OnScreenShowDelegate += OnTextBoxAppear;
        dialogueManager.Show();
    }

    public void OpenPlayerMenu()
    {
        playerMenu.OnScreenShowDelegate += EnableInput;
        playerMenu.Show();
    }

    public void OpenSpellMenu()
    {
        spellMenu.OnScreenShowDelegate += EnableInput;
        spellMenu.Show();
    }


    #region Button Functions

    public void SpellButtonClick()
    {
        spellOpen = true;
        SetInputState(false);

        playerMenu.OnScreenHideDelegate += OpenSpellMenu;
        playerMenu.Hide();
    }

    public void BackButtonClick()
    {
        spellOpen = false;
        SetInputState(false);

        spellMenu.OnScreenHideDelegate += OpenPlayerMenu;
        spellMenu.Hide();
    }

    #endregion


    #region Input

    public void EnableInput() { SetInputState(true); } 

    private void SetInputState(bool val)
    {
        input.enabled = val;
    }

    #endregion


    void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_BATTLE_BEGIN).RemoveListener(OnBattleBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).RemoveListener(OnTurnBegin);
        EventManager.Instance.GetGameEvent(EventConstants.ON_MOVE_SELECTED).RemoveListener(OnMoveSelected);
    }
}
