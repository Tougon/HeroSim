using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpellListController : MonoBehaviour
{
    public class ColorSpritePair
    {
        public Color color;
        public Sprite sprite;
    }


    [SerializeField]
    private List<UISpellButton> spellButtons = new List<UISpellButton>(4);

    [SerializeField]
    private List<SpellButtonData> buttonData = new List<SpellButtonData>(6);
    private Dictionary<Spell.SpellType, ColorSpritePair> buttonTypeDataMap = new Dictionary<Spell.SpellType, ColorSpritePair>();


    // Start is called before the first frame update
    void Awake()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE).AddListener(UpdateSpellButtons);

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


    void OnDestroy()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE).RemoveListener(UpdateSpellButtons);
    }
}
