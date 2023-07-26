using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToUI;

public class UIPlayerBattleMenu : UIMenu
{
    [Header("Player Battle Menu Properties")]
    [SerializeField]
    protected UIMenuButton BackButton;

    protected override void Awake()
    {
        base.Awake();
    }


    public override void Show()
    {
        // Hide the back button if the player we're currently choosing actions for is the first
        bool hideBack = VariableManager.Instance.GetBoolVariableValue(VariableConstants.IS_FIRST_PLAYER);
        BackButton.gameObject.SetActive(!hideBack);

        if(hideBack && CurrentSelection == BackButton)
        {
            BackButton.SetSelected(false);
            SetSelection(InitialSelection);
            CurrentSelection.SetSelected(true);
        }

        base.Show();
    }


    public void CancelSelection()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.CANCEL_PLAYER_SELECTION);
    }
}
