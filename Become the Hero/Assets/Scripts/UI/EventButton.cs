using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventButton : Button
{
    protected override void Start()
    {
        base.Start();

        //  Assign event listeners
        if(EventManager.Instance != null)
        {
            EventManager.Instance.GetGameEvent(EventConstants.ON_BUTTON_RELEASED).AddListener(OnButtonReleased);
            EventManager.Instance.GetGameObjectEvent(EventConstants.ON_BUTTON_PRESSED).AddListener(OnButtonPressed);
        }
    }


    public void OnButtonPressed(GameObject button)
    {
        if (!interactable) return;

        if (button != this.gameObject)
            interactable = false;
    }


    public void OnButtonReleased()
    {
        if(enabled && !interactable)
        {
            interactable = true;
            DoStateTransition(SelectionState.Normal, false);
        }
    }


    protected override void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.GetGameEvent(EventConstants.ON_BUTTON_RELEASED).RemoveListener(OnButtonReleased);
            EventManager.Instance.GetGameObjectEvent(EventConstants.ON_BUTTON_PRESSED).RemoveListener(OnButtonPressed);
        }

        base.OnDestroy();
    }
}
