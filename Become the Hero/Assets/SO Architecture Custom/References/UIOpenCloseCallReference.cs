using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class UIOpenCloseCallReference : BaseReference<UIOpenCloseCall, UIOpenCloseCallVariable>
	{
	    public UIOpenCloseCallReference() : base() { }
	    public UIOpenCloseCallReference(UIOpenCloseCall value) : base(value) { }
	}
}