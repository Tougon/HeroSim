using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ToUI;

public class UISpellButton : UIMenuButton
{
    // Time the button must be held down before info is displayed
    private const float infoHoldTime = 0.75f;

    [SerializeField]
    [Header("Spell Button Properties")]
    private Image image;
    [SerializeField]
    private TextMeshProUGUI text;
    [SerializeField]
    private TextMeshProUGUI cost;
    public UISpellListController controller { get; set; }

    private int index;

    [SerializeField]
    private Spell current;


    protected override void Awake()
    {
        if(image == null)
            image = GetComponent<Image>();

        base.Awake();
    }


    public void InitializeButton(Spell s, int i, int mp)
    {
        if(s == null)
        {
            Debug.LogError($"ERROR: INITIALIZING WITH NULL SPELL {this.name}");
        }

        current = s;
        text.text = s.spellName;
        cost.text = s.spellCost.ToString();
        index = i;

        var data = controller.GetButtonData(s.GetSpellType());

        
        this.enabled = s.spellCost <= mp ? true : false;

        if (this.enabled)
            image.color = data.color;
        else
        {
            Color c = data.color;
            float hue, sat, val;

            Color.RGBToHSV(c, out hue, out sat, out val);
            sat *= 0.5f;
            c = Color.HSVToRGB(hue, sat, val);

            image.color = c;
        }
    }


    public void SelectAttack()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.ATTACK_SELECTED);
    }


    public void SelectDefend()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.DEFEND_SELECTED);
    }


    public void SelectSpell()
    {
        EventManager.Instance.RaiseIntEvent(EventConstants.SPELL_SELECTED, index);
    }


    // Archived code that may be useful again someday.
    /*public void ButtonReleaseForInfo()
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
    }*/   
}
