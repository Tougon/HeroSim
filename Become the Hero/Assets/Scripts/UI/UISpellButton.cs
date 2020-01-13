using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISpellButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;
    private Button b;

    private int index;

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
}
