using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UISpellButton : MonoBehaviour
{
    // Time the button must be held down before info is displayed
    private const float infoHoldTime = 0.75f;

    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private TextMeshProUGUI cost;
    public Button b { get; private set; }
    public EventTrigger e { get; private set; }

    private int index;
    public bool selected { get; private set; }
    private float buttonDownTime = 0.0f;
    private IEnumerator buttonHold;

    [SerializeField]
    private Spell current;


    void Awake()
    {
        b = GetComponent<Button>();
        e = GetComponent<EventTrigger>();
    }


    public void InitializeButton(Spell s, int i, int mp)
    {
        current = s;
        text.text = s.spellName;
        cost.text = s.spellCost.ToString();
        index = i;

        b.enabled = s.spellCost <= mp ? true : false;
        e.enabled = b.enabled;
    }


    public void SelectAttack()
    {
        if (!b.interactable || !selected) return;

        EventManager.Instance.RaiseGameEvent(EventConstants.ATTACK_SELECTED);
    }


    public void SelectDefend()
    {
        if (!b.interactable || !selected) return;

        EventManager.Instance.RaiseGameEvent(EventConstants.DEFEND_SELECTED);
    }


    public void SelectSpell()
    {
        if (!b.interactable || !selected) return;

        EventManager.Instance.RaiseIntEvent(EventConstants.SPELL_SELECTED, index);
    }


    public void OnButtonPress()
    {
        EventManager.Instance.RaiseGameObjectEvent(EventConstants.ON_BUTTON_PRESSED, this.gameObject);

        if (b.interactable) selected = true;
    }


    public void OnButtonRelease()
    {
        if(selected)
            EventManager.Instance.RaiseGameEvent(EventConstants.DESELECT_BUTTON);

        EventManager.Instance.RaiseGameEvent(EventConstants.ON_BUTTON_RELEASED);
        selected = false;
    }


    public void ButtonHoldForInfo()
    {
        if (!b.interactable || !selected) return;

        buttonHold = ButtonDown();
        StartCoroutine(buttonHold);
    }


    public void ButtonReleaseForInfo()
    {
        if (!b.interactable || !selected) return;

        if (buttonHold != null) StopCoroutine(buttonHold);

        if (buttonDownTime < infoHoldTime)
            SelectSpell();
        else
        {
            EventManager.Instance.RaiseSpellGameEvent(EventConstants.SPELL_INFO_DISPLAY, current);
        }

        OnButtonRelease();
        buttonDownTime = 0.0f;
    }


    private IEnumerator ButtonDown()
    {
        while (buttonDownTime < infoHoldTime)
        {
            buttonDownTime += Time.deltaTime;
            yield return null;
        }

        ButtonReleaseForInfo();
    }
}
