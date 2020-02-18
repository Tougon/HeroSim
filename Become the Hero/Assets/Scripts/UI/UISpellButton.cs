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
    private RectTransform textPos;
    [SerializeField]
    private TextMeshProUGUI cost;
    private RectTransform costPos;
    public Button b { get; private set; }
    public EventTrigger e { get; private set; }
    public UISpellListController controller { get; set; }

    private int index;
    public bool selected { get; private set; }
    private float buttonDownTime = 0.0f;
    private RectTransform rect;
    private IEnumerator buttonHold;

    [SerializeField]
    private Spell current;


    void Awake()
    {
        b = GetComponent<Button>();
        e = GetComponent<EventTrigger>();
        rect = GetComponent<RectTransform>();

        if(text != null)
            textPos = text.rectTransform;

        if(cost != null)
            costPos = cost.rectTransform;
    }


    public void InitializeButton(Spell s, int i, int mp)
    {
        current = s;
        text.text = s.spellName;
        cost.text = s.spellCost.ToString();
        index = i;

        var data = controller.GetButtonData(s.GetSpellType());

        
        b.enabled = s.spellCost <= mp ? true : false;
        e.enabled = b.enabled;

        if (b.enabled)
            b.image.color = data.color;
        else
        {
            Color c = data.color;
            float hue, sat, val;

            Color.RGBToHSV(c, out hue, out sat, out val);
            sat *= 0.5f;
            c = Color.HSVToRGB(hue, sat, val);

            b.image.color = c;
        }
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

        if (text != null)
            textPos.offsetMin = new Vector2(textPos.offsetMin.x, 2);
        if (cost != null)
            costPos.offsetMin = new Vector2(costPos.offsetMin.x, 0);

        if (b.interactable) selected = true;
    }


    public void OnButtonRelease()
    {
        if(selected)
            EventManager.Instance.RaiseGameEvent(EventConstants.DESELECT_BUTTON);

        EventManager.Instance.RaiseGameEvent(EventConstants.ON_BUTTON_RELEASED);
        selected = false;
        buttonDownTime = 0.0f;

        if(buttonHold != null)
            StopCoroutine(buttonHold);

        if (text != null)
            textPos.offsetMin = new Vector2(textPos.offsetMin.x, 6);
        if (cost != null)
            costPos.offsetMin = new Vector2(costPos.offsetMin.x, 4);
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
        {
            SelectSpell();
            OnButtonRelease();
        }
        else
        {
            EventManager.Instance.RaiseSpellGameEvent(EventConstants.SPELL_INFO_DISPLAY, current);
            EventManager.Instance.RaiseRectTransformGameEvent(EventConstants.ON_SPELL_INFO_APPEAR, rect);
        }
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
