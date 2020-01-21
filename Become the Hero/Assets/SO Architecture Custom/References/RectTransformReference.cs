using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class RectTransformReference : BaseReference<RectTransform, RectTransformVariable>
	{
	    public RectTransformReference() : base() { }
	    public RectTransformReference(RectTransform value) : base(value) { }
	}
}