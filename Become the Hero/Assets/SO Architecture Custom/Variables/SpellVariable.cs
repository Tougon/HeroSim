using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[CreateAssetMenu(
	    fileName = "SpellVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "Spell",
	    order = 120)]
	public class SpellVariable : BaseVariable<Spell>
	{
	}
}