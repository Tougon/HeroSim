using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventButton : Button
{
    protected override void Awake()
    {
        base.Awake();

        //  Assign event listeners
        EventManager.Instance.GetGameEvent(EventConstants.ON_BUTTON_RELEASED).AddListener(OnButtonReleased);
        EventManager.Instance.GetGameObjectEvent(EventConstants.ON_BUTTON_PRESSED).AddListener(OnButtonPressed);
    }


    public void OnButtonPressed(GameObject button)
    {
        if (!interactable) return;

        if (button != this.gameObject)
            interactable = false;
    }


    public void OnButtonReleased()
    {
        interactable = true;
        DoStateTransition(SelectionState.Normal, false);
    }


    protected override void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_BUTTON_RELEASED).RemoveListener(OnButtonReleased);
        EventManager.Instance.GetGameObjectEvent(EventConstants.ON_BUTTON_PRESSED).RemoveListener(OnButtonPressed);

        base.OnDestroy();
    }
}
