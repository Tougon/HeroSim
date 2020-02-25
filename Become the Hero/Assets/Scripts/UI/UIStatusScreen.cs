using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIStatusScreen : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI creatureName;
    [SerializeField]
    private Image creatureImage;
    [SerializeField]
    private TextMeshProUGUI creatureDescription;

    [SerializeField]
    private UIStatusScreenStatDisplay statDisplay;
    [SerializeField]
    private UIEffectDisplayController effectDisplay;

    private CanvasGroup group;

    // Start is called before the first frame update
    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        HideStatusScreen();

        EventManager.Instance.GetEntityControllerEvent(EventConstants.OPEN_STATUS_SCREEN).AddListener(ShowStatusScreen);
    }


    public void ShowStatusScreen(EntityController ec)
    {
        EntityParams ep = ec.GetEntity().vals;

        creatureName.text = ep.entityName;
        creatureImage.sprite = ep.entitySprite;
        creatureDescription.text = ep.entityDescription;

        if (ec.IsIdentified())
        {
            statDisplay.SetStatDisplay(true);
            statDisplay.UpdateStats(ec);
        }
        else
        {
            statDisplay.SetStatDisplay(false);
            statDisplay.UpdateStats(ec);
        }

        var effects = ec.GetEffects();

        if (effects.Count == 0)
            effectDisplay.gameObject.SetActive(false);
        else
        {
            effectDisplay.gameObject.SetActive(true);
            effectDisplay.UpdateEffectDisplay(effects);
        }

        group.blocksRaycasts = true;
        group.alpha = 1;
    }


    public void HideStatusScreen()
    {
        group.blocksRaycasts = false;
        group.alpha = 0;
    }


    private void OnDestroy()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.OPEN_STATUS_SCREEN).RemoveListener(ShowStatusScreen);
    }
}
