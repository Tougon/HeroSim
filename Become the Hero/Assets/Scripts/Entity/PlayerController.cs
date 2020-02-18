using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController
{
    [SerializeField]
    private Spell attack;

    [SerializeField]
    private Spell defend;

    [SerializeField]
    private PlayerSpellList spellList;
    private Spell[] availableSpells = new Spell[4];

    [SerializeField]
    private int amountMPGainPerTurn = 5;


    protected override void Awake()
    {
        base.Awake();

        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).AddListener(OnTurnBegin);

        EventManager.Instance.GetGameEvent(EventConstants.ATTACK_SELECTED).AddListener(SetActionToAttack);
        EventManager.Instance.GetGameEvent(EventConstants.DEFEND_SELECTED).AddListener(SetActionToDefend);
        EventManager.Instance.GetIntEvent(EventConstants.SPELL_SELECTED).AddListener(SetActionToSpell);

        // The player should always be ID'd.
        isIdentified = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        entityUI.ShowText();
        spellList.Initialize();

        // initialize the player 
        EventManager.Instance.RaiseEntityControllerEvent(EventConstants.ON_PLAYER_INITIALIZE, this);
    }


    public void OnTurnBegin()
    {
        ModifyMP(amountMPGainPerTurn);
        PopulateSpellList();
    }


    public void PopulateSpellList()
    {
        availableSpells = spellList.GetSpellListForTurn();

        EventManager.Instance.RaiseEntityControllerEvent(EventConstants.ON_SPELL_LIST_INITIALIZE, this);
    }


    public Spell[] GetAvailableSpells()
    {
        return availableSpells;
    }


    /// <summary>
    /// Call death event
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();

        string dialogueSeq = param.entityName + " falls...";
        EventManager.Instance.RaiseStringEvent(EventConstants.ON_DIALOGUE_QUEUE, dialogueSeq);
    }


    #region Action Selection

    public void SetActionToSpell(int index)
    {
        index = Mathf.Clamp(index, 0, availableSpells.Length);
        action = availableSpells[index];

        // Don't do this if the spell affects the user.
        SetTarget();
    }


    public void SetActionToAttack()
    {
        action = attack;

        SetTarget();
    }


    public void SetActionToDefend()
    {
        action = defend;
        target = this;
    }

    
    public void SetTarget()
    {
        if (turnManger.GetNumEntities() > 2)
        {
            // This is where we add targeting options
        }
        else
            target = turnManger.GetOther(this);
    }

    #endregion
    
    void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).RemoveListener(OnTurnBegin);

        EventManager.Instance.GetGameEvent(EventConstants.ATTACK_SELECTED).RemoveListener(SetActionToAttack);
        EventManager.Instance.GetGameEvent(EventConstants.DEFEND_SELECTED).RemoveListener(SetActionToDefend);
        EventManager.Instance.GetIntEvent(EventConstants.SPELL_SELECTED).RemoveListener(SetActionToSpell);
    }
}
