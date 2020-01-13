using UnityEngine;
using Hero.Core;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "Sequence")]
	public sealed class SequenceGameEventListener : BaseGameEventListener<Sequence, SequenceGameEvent, SequenceUnityEvent>
	{
	}
}