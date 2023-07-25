using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "UIOpenCloseCall")]
	public sealed class UIOpenCloseCallGameEventListener : BaseGameEventListener<UIOpenCloseCall, UIOpenCloseCallGameEvent, UIOpenCloseCallUnityEvent>
	{
	}
}