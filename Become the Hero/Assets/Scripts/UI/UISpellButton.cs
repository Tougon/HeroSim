using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpellButton : MonoBehaviour
{
    // Time the button must be held down before info is displayed
    private const float infoHoldTime = 1.0f;

    [SerializeField]
    private TextMeshProUGUI text;
    public Button b { get; private set; }

    private int index;
    public bool selected { get; private set; }
    private float buttonDownTime = 0.0f;
    private IEnumerator buttonHold;


    void Awake()
    {
        b = GetComponent<Button>();
    }


    public void InitializeButton(Spell s, int i, int mp)
    {
        text.text = s.spellName;
        index = i;

        b.enabled = s.spellCost <= mp ? true : false;
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
            Debug.Log("Attack data");
            // Display the thing
        }
        
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
