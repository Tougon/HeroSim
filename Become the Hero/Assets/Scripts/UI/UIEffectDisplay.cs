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
        
    }
}
