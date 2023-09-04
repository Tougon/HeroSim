using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TFlag : SerializedScriptableObject
{
    private TFlagManager _manager;

    [ReadOnly]
    [HorizontalGroup("Primary")]
    public int ID;
    [HorizontalGroup("Primary")]
    [OnValueChanged("ValidateName")]
    public string Name;


#if UNITY_EDITOR
    public void Initialize(TFlagManager manager)
    {
        _manager = manager;
        Name = name;
    }


    [HorizontalGroup("Primary")]
    [Button("Delete")]
    public void DeleteFlag()
    {
        _manager.DeleteFlag(this);
    }


    private void ValidateName()
    {
        this.Name = _manager.ValidateFlag(this, Name);
        this.name = Name.ToLower().Replace(" ", "_");
    }
#endif
}
