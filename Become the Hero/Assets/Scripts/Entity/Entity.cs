using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Represents any kind of creature in the game.
/// </summary>
[CreateAssetMenu(fileName = "NewEntity", menuName = "Entity/Entity", order = 0)]
public class Entity : ScriptableObject
{
    // Weakness, Resistance, and Immunity go in Entity Params
    public EntityParams vals;

    public List<Spell> moveList;
    public EntityBehaviorObject behavior;
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
