using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeselectOnButtonExit : MonoBehaviour
{
    EventSystem es;

    // Start is called before the first frame update
    void Awake()
    {
        es = GetComponent<EventSystem>();
        EventManager.Instance.GetGameEvent(EventConstants.DESELECT_BUTTON).AddListener(OnButtonReleased);
    }


    public void OnButtonReleased()
    {
        es.SetSelectedGameObject(null);
    }


    void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.DESELECT_BUTTON).RemoveListener(OnButtonReleased);
    }
}
