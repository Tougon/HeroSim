using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIStatusScreen : MonoBehaviour
{
    private CanvasGroup group;

    // Start is called before the first frame update
    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.alpha = 0;

        EventManager.Instance.GetEntityControllerEvent(EventConstants.OPEN_STATUS_SCREEN).AddListener(ShowStatusScreen);
    }


    public void ShowStatusScreen(EntityController ec)
    {
        group.blocksRaycasts = true;
        group.alpha = 1;
    }


    private void OnDestroy()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.OPEN_STATUS_SCREEN).RemoveListener(ShowStatusScreen);
    }
}
