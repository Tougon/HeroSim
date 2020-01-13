using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "EntityController")]
	public sealed class EntityControllerGameEventListener : BaseGameEventListener<EntityController, EntityControllerGameEvent, EntityControllerUnityEvent>
	{
	}
}