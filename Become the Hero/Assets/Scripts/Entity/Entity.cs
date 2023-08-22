using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Represents any kind of creature in the game.
/// </summary>
[CreateAssetMenu(fileName = "NewEntity", menuName = "Entity/Entity", order = 0)]
public class Entity : ScriptableObject
{
    // Weakness, Resistance, and Immunity go in Entity Params
    public EntityParams vals;

    [Title("Moveset and AI Behavior")]
    [ListDrawerSettings(DraggableItems = true, ShowPaging = false, AlwaysAddDefaultValue = true)]
    [OnValueChanged("AssignIDs", true)]
    public List<SpellLevel> moveListLevel;
    public EntityBehaviorObject behavior;

    public List<Spell> GetMoveList(int level)
    {
        List<Spell> list = new List<Spell>();

        foreach(var spell in moveListLevel)
        {
            if (spell.level <= level) list.Add(spell.spell);
        }

        return list;
    }


    private void AssignIDs()
    {
        for(int i=0; i<moveListLevel.Count; i++)
        {
            var data = moveListLevel[i];
            data.id = i;
            moveListLevel[i] = data;
        }
    }
}


[System.Serializable]
public class EntityParams
{
    [Title("Entity Identification")]
    public string entityName;
    public string article;
    public bool useArticle;

    [Title("Entity Battle Stats")]
    public int entityHP;
    public int entityMP;
    public int entityAtk;
    public int entityDef;
    public int entityMgAtk;
    public int entityMgDef;
    public int entitySpeed;
    public int entityCritModifier = 1;
    public int entityDodgeModifier = 1;

    [Title("Entity Visuals")]
    public Sprite entitySprite;
    public Sprite[] additionalEntitySprites;
    public string entityDescription;


    public string GetEntityName()
    {
        return useArticle ? article + " " + entityName.ToLower() : entityName;
    }
}


[System.Serializable]
public struct SpellLevel
{
    [ReadOnly]
    [HorizontalGroup("Primary")]
    public int id;
    [HorizontalGroup("Primary")]
    public Spell spell;
    [HorizontalGroup("Primary")]
    public int level;
}
