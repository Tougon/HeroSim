using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[CreateAssetMenu(
	    fileName = "MenuOpenCloseCallVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "UI Callback",
	    order = 120)]
	public class UIOpenCloseCallVariable : BaseVariable<UIOpenCloseCall>
	{
	}
}