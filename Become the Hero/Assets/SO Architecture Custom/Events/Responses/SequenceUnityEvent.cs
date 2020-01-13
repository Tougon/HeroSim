using UnityEngine;
using UnityEngine.Events;
using Hero.Core;

namespace ScriptableObjectArchitecture
{
	[System.Serializable]
	public sealed class SequenceUnityEvent : UnityEvent<Sequence>
	{
	}
}