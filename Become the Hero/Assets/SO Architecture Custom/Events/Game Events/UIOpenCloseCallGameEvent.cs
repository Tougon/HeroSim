using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "UICallGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "UI Callback",
	    order = 120)]
	public sealed class UIOpenCloseCallGameEvent : GameEventBase<UIOpenCloseCall>
	{
	}
}