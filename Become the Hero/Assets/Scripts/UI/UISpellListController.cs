using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpellListController : MonoBehaviour
{
    [SerializeField]
    private List<UISpellButton> spellButtons = new List<UISpellButton>(4);

    // Start is called before the first frame update
    void Awake()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE).AddListener(UpdateSpellButtons);
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


    void OnDestroy()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE).RemoveListener(UpdateSpellButtons);
    }
}
