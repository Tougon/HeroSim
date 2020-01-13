using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[CreateAssetMenu(
	    fileName = "EntityControllerVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "EntityController",
	    order = 120)]
	public class EntityControllerVariable : BaseVariable<EntityController>
	{
	}
}