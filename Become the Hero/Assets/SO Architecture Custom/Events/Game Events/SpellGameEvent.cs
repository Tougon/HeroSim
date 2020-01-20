using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "SpellGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "Spell",
	    order = 120)]
	public sealed class SpellGameEvent : GameEventBase<Spell>
	{
	}
}