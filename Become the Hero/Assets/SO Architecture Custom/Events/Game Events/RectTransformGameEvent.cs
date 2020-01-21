using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "RectTransformGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "RectTransform",
	    order = 120)]
	public sealed class RectTransformGameEvent : GameEventBase<RectTransform>
	{
	}
}