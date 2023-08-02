using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToUI;

public class UISpellListController : UIMenu
{
    public class ColorSpritePair
    {
        public Color color;
        public Sprite sprite;
    }

    private List<UISpellButton> spellButtons = new List<UISpellButton>(4);

    // Note: may be deprecated later on
    [Header("Spell List Properties")]
    [SerializeField]
    private List<SpellButtonData> buttonData = new List<SpellButtonData>(6);
    private Dictionary<Spell.SpellType, ColorSpritePair> buttonTypeDataMap = new Dictionary<Spell.SpellType, ColorSpritePair>();


    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE).AddListener(UpdateSpellButtons);

        foreach(var button in SelectionMatrix)
        {
            if(button is UISpellButton)
                spellButtons.Add(button as UISpellButton);
        }

        foreach (UISpellButton sb in spellButtons)
            sb.controller = this;

        // Create a Dictionary for easier lookup of button data for Spells
        foreach(SpellButtonData sbd in buttonData)
        {
            ColorSpritePair csp = new ColorSpritePair();
            csp.color = sbd.buttonColor;
            csp.sprite = sbd.buttonIcon;

            buttonTypeDataMap.Add(sbd.buttonType, csp);
        }
    }


    public void UpdateSpellButtons(EntityController ec)
    {
        PlayerController pc = (PlayerController)ec;
        Spell[] spells = pc.GetAvailableSpells();

        for(int i=0; i<spells.Length; i++)
        {
            spellButtons[i].InitializeButton(spells[i], i, pc.GetCurrentMP());
        }
    }


    public ColorSpritePair GetButtonData(Spell.SpellType type)
    {
        if (buttonTypeDataMap.ContainsKey(type))
            return buttonTypeDataMap[type];

        return null;
    }


    public override void OnCancelPressed()
    {
        base.OnCancelPressed();

        EventManager.Instance.RaiseUIGameEvent(EventConstants.HIDE_ALL_SCREENS,
            new UIOpenCloseCall
            {
                Callback = () =>
                {
                    EventManager.Instance.RaiseUIGameEvent(EventConstants.SHOW_SCREEN,
                        new UIOpenCloseCall
                        {
                            MenuName = ScreenConstants.ActionMenu.ToString()
                        });
                }
            });
    }


    void OnDestroy()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE).RemoveListener(UpdateSpellButtons);
    }
}
