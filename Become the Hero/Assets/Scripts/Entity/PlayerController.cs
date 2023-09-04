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

    [HideInInspector]
    public List<RuntimeDynamicSpell> dynamicMoves;


    protected override void Awake()
    {
        base.Awake();

        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).AddListener(OnTurnBegin);

        // The player should always be ID'd.
        isIdentified = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        spellList.Initialize();

        // initialize the player 
        EventManager.Instance.RaiseEntityControllerEvent(EventConstants.ON_PLAYER_INITIALIZE, this);
    }


    protected override void CreateMoveList()
    {
        moveList.Clear();

        // TODO: Load the dynamic spell list from saved data per character
        // Temp Code
        var tempMoves = current.GetMoveList(50);
        dynamicMoves = new List<RuntimeDynamicSpell>();

        for (int i=0; i<tempMoves.Count; i++)
        {
            var dynamicMove = new RuntimeDynamicSpell()
            {
                spell = tempMoves[i],
                overrideName = tempMoves[i].spellName,
                seal = i < current.seals.Count ? current.seals[i].seal : null
            };

            dynamicMoves.Add(dynamicMove);
        }
        // End Temp

        foreach (var spell in dynamicMoves)
        {
            moveList.Add(spell.spell);
            spellToSeal.Add(spell.spell, spell.seal);
        }
    }


    public void OnTurnBegin()
    {
        // TODO: Remove RNG Spell List and MP gain. This is only relevant for Hero Sim.
        //ModifyMP(amountMPGainPerTurn);
        PopulateSpellList();
    }


    public void PopulateSpellList()
    {
        availableSpells = spellList.GetSpellListForTurn();
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

    public override void SelectAction(int index)
    {
        index = Mathf.Clamp(index, 0, moveList.Count);
        action = moveList[index];
    }

    public override void SelectAction(string name)
    {
        name = name.ToLower();

        if (name == "defend")
        {
            action = defend;
            target.Add(this);
            ready = true;
        }
        else if (name == "attack")
        {
            action = attack;
        }
        else
            base.SelectAction(name);
    }

    #endregion
    
    void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).RemoveListener(OnTurnBegin);
    }
}
