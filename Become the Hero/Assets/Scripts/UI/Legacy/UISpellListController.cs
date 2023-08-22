using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToUI;

public class UISpellListController : UIDynamicMenu
{
    public class ColorSpritePair
    {
        public Color color;
        public Sprite sprite;
    }

    //private List<UISpellButton> spellButtons = new List<UISpellButton>(4);
    private EntityController currentEntity;

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

        // Create a Dictionary for easier lookup of button data for Spells
        // NOTE: Will likely be scrapped.
        foreach(SpellButtonData sbd in buttonData)
        {
            ColorSpritePair csp = new ColorSpritePair();
            csp.color = sbd.buttonColor;
            csp.sprite = sbd.buttonIcon;

            buttonTypeDataMap.Add(sbd.buttonType, csp);
        }
    }


    protected override void SpawnMenuItems()
    {
        base.SpawnMenuItems();

        foreach (var button in SelectionMatrix)
        {
            if (button is UISpellButton)
                (button as UISpellButton).controller = this;
        }
    }


    public void UpdateSpellButtons(EntityController ec)
    {
        //May need to refresh all
        currentEntity = ec;
        SetMaxItems(currentEntity.moveList.Count);
        RefreshAllData();
    }


    protected override bool RefreshData(UIMenuItem Item, int index)
    {
        Item.gameObject.SetActive(index >= 0);

        if(index >= 0)
        {
            if(Item is UISpellButton && currentEntity != null)
            {
                (Item as UISpellButton).InitializeButton(currentEntity.moveList[index],
                    index, currentEntity.GetCurrentMP());
            }
        }

        return index >= 0;
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
