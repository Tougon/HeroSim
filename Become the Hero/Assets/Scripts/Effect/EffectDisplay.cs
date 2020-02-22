using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEffectDisplay", menuName = "Effect/UI/Effect Display", order = 3)]
public class EffectDisplay : ScriptableObject
{
    public string displayName;
    public bool displayTurnLimit;
    public string description;
    public Sprite icon;
}
