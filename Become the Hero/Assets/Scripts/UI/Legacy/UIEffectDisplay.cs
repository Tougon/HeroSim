using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEffectDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI effectName;
    [SerializeField]
    private TextMeshProUGUI effectDescription;
    [SerializeField]
    private TextMeshProUGUI effectLimit;
    [SerializeField]
    private Image effectIcon;


    /// <summary>
    /// Initializes the display
    /// </summary>
    public void Init(EffectInstance eff)
    {
        EffectDisplay ed = eff.effect.display;

        effectName.text = ed.displayName;
        effectDescription.text = ed.description;
        effectIcon.sprite = ed.icon;

        if (ed.displayTurnLimit)
        {
            effectLimit.enabled = true;
            effectLimit.text = "Turns Left: " + (eff.limit - eff.numTurnsActive);
        }
        else
            effectLimit.enabled = false;
    }
}
