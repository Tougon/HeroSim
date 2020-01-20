using UnityEngine;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class SpellReference : BaseReference<Spell, SpellVariable>
	{
	    public SpellReference() : base() { }
	    public SpellReference(Spell value) : base(value) { }
	}
}