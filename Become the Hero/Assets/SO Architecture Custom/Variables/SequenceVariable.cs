using UnityEngine;
using Hero.Core;

namespace ScriptableObjectArchitecture
{
	[CreateAssetMenu(
	    fileName = "SequenceVariable.asset",
	    menuName = SOArchitecture_Utility.VARIABLE_SUBMENU + "Sequence",
	    order = 120)]
	public class SequenceVariable : BaseVariable<Sequence>
	{
	}
}