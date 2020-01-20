using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UISpellInfoDisplay : MonoBehaviour
{
    private Spell current;

    private TextMeshProUGUI power;
    private TextMeshProUGUI accuracy;
    private TextMeshProUGUI description;


    void Awake()
    {
        power = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        accuracy = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        description = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        EventManager.Instance.GetSpellGameEvent(EventConstants.SPELL_INFO_DISPLAY).AddListener(SetSpellInfo);
    }


    public void SetSpellInfo(Spell s)
    {
        current = s;

        int pow = current.GetPower();
        int hit = current.GetAccuracy();

        power.text = pow < 0 ? "POW: --" : "POW: " + current.GetPower();
        accuracy.text = hit < 0 ? "HIT: --" : "HIT: " + current.GetAccuracy();
        description.text = current.spellDescription;
    }


    public void OnDestroy()
    {
        EventManager.Instance.GetSpellGameEvent(EventConstants.SPELL_INFO_DISPLAY).RemoveListener(SetSpellInfo);
    }
}
