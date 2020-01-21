using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "RectTransform")]
	public sealed class RectTransformGameEventListener : BaseGameEventListener<RectTransform, RectTransformGameEvent, RectTransformUnityEvent>
	{
	}
}