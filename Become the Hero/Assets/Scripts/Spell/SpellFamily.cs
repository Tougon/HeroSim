using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NewSpellList", menuName = "Spell/Utilities/Spell Family", order = 9)]
public class SpellFamily : ScriptableObject
{
    public string familyName = "";
}