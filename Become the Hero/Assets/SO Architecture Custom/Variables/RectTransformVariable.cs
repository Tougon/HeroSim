using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[CreateAssetMenu(
	    fileName = "RectTransformVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "RectTransform",
	    order = 120)]
	public class RectTransformVariable : BaseVariable<RectTransform>
	{
	}
}