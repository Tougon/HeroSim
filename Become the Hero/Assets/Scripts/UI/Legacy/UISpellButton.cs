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
    // Currently deprecated but may see use later
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
    [SerializeField] private bool canSeal = false;
    private bool canSealCurrent = false;


    protected override void Awake()
    {
        if(image == null)
            image = GetComponent<Image>();

        base.Awake();
    }


    public void InitializeButton(RuntimeDynamicSpell s, int i, int mp)
    {
        if(s == null)
        {
            Debug.LogError($"ERROR: INITIALIZING WITH NULL SPELL {this.name}");
        }

        current = s.spell;
        text.text = s.spell.spellName;
        cost.text = s.spell.spellCost.ToString();
        index = i;

        canSealCurrent = canSeal && Mathf.RoundToInt(s.spell.spellCost *
            Spell.SEAL_COST_MULTIPLIER) <= mp; 

        //var data = controller.GetButtonData(s.GetSpellType());

        
        this.enabled = s.spell.spellCost <= mp ? true : false;

        /*if (this.enabled)
            image.color = data.color;
        else
        {
            Color c = data.color;
            float hue, sat, val;

            Color.RGBToHSV(c, out hue, out sat, out val);
            sat *= 0.5f;
            c = Color.HSVToRGB(hue, sat, val);

            image.color = c;
        }*/
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


    public void SelectSpellSeal()
    {
        EventManager.Instance.RaiseIntEvent(EventConstants.SPELL_SEALED, index);
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


    #region Input Responses

    public override void OnAux1Pressed()
    {
        if (!canSeal) return;
        if (!canSealCurrent) return;

        // TODO: Maybe don't just have it be a button press?
        // I'm thinking the best way to do this would be to swap the menu outright?
        // Could give more info too.
        // The above check will also become redundant if this happens.

        // TODO: Check if same spell was already attempted to be sealed by another entity in the party
        base.OnAux1Pressed();
        SelectSpellSeal();
    }

    #endregion
}
