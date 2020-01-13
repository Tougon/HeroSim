using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	[CreateAssetMenu(
	    fileName = "EntityControllerGameEvent.asset",
	    menuName = SOArchitecture_Utility.GAME_EVENT + "EntityController",
	    order = 120)]
	public sealed class EntityControllerGameEvent : GameEventBase<EntityController>
	{
	}
}