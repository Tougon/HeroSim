using UnityEngine;
using Hero.Core;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "SequenceGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "Sequence",
	    order = 120)]
	public sealed class SequenceGameEvent : GameEventBase<Sequence>
	{
	}
}