using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewButtonData", menuName = "System/UI/Spell Button Data", order = 1)]
public class SpellButtonData : ScriptableObject
{
    public Spell.SpellType buttonType = Spell.SpellType.Other;
    public Color buttonColor = Color.white;
    public Sprite buttonIcon;
}
