using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "Spell")]
	public sealed class SpellGameEventListener : BaseGameEventListener<Spell, SpellGameEvent, SpellUnityEvent>
	{
	}
}