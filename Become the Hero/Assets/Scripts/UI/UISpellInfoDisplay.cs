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

    private RectTransform rect;
    private Vector2 startPos;

    private bool reset = true;


    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;

        power = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        accuracy = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        description = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        EventManager.Instance.GetGameEvent(EventConstants.ON_BUTTON_RELEASED).AddListener(ResetPosition);
        EventManager.Instance.GetSpellGameEvent(EventConstants.SPELL_INFO_DISPLAY).AddListener(SetSpellInfo);
        EventManager.Instance.GetRectTransformGameEvent(EventConstants.ON_SPELL_INFO_APPEAR).AddListener(SetPosition);
    }


    public void SetPosition(RectTransform other)
    {
        reset = false;

        Vector3 target = other.transform.position;
        float offset = (other.rect.height);
        //target.y += offset;
        
        transform.position = target;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + offset + 6.6667f);
    }


    public void ResetPosition()
    {
        if (!reset) rect.anchoredPosition = startPos;
        reset = true;
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
        EventManager.Instance.GetGameEvent(EventConstants.ON_BUTTON_RELEASED).RemoveListener(ResetPosition);
        EventManager.Instance.GetSpellGameEvent(EventConstants.SPELL_INFO_DISPLAY).RemoveListener(SetSpellInfo);
        EventManager.Instance.GetRectTransformGameEvent(EventConstants.ON_SPELL_INFO_APPEAR).RemoveListener(SetPosition);
    }
}
