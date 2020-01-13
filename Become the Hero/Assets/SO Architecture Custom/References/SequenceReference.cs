using UnityEngine;
using Hero.Core;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class SequenceReference : BaseReference<Sequence, SequenceVariable>
	{
	    public SequenceReference() : base() { }
	    public SequenceReference(Sequence value) : base(value) { }
	}
}