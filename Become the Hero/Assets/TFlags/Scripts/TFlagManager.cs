using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "NewTFlagManager", menuName = "TFlags/TFlag Manager", order = 10)]
public class TFlagManager : SerializedScriptableObject
{
    public static TFlagManager Instance;

    [SerializeField]
    [HideReferenceObjectPicker, ListDrawerSettings(ShowPaging = false, HideAddButton = true, HideRemoveButton = true, 
        ShowIndexLabels = false, DraggableItems = false, IsReadOnly = true, ShowFoldout = false)]
    [DisableContextMenu(disableForMember: false, disableCollectionElements: true)]
    [InlineEditor(DrawHeader = false, Expanded = true, ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
    private List<TFlag> Flags = new List<TFlag>();

    [SerializeField]
    [OnValueChanged("SetGlobal")]
    [InfoBox("A Global Manager is a self-initializing TFlagManager instance. " +
        "Only one of these can exist at a time, having more than one may cause unexpected behavior. " +
        "It is recommended to use this only if one TFlagManager should exist for the entire game.")]
    private bool isGlobalManager = false;

    public List<TFlag> GetFlags() { return Flags; }


#if UNITY_EDITOR
    [ContextMenu("Add New Flag")]
    [Button("Add New Flag")]
    [PropertyOrder(-10)]
    public virtual void AddNewFlag()
    {
        TFlag flag = ScriptableObject.CreateInstance<TFlag>();
        // Generate unique name for the flag
        flag.name = $"Flag {flag.GetInstanceID().ToString()}";
        flag.Initialize(this);
        flag.ID = Flags.Count;
        Flags.Add(flag);

        AssetDatabase.AddObjectToAsset(flag, this);
        AssetDatabase.SaveAssets();

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(flag);
    }


    public virtual void DeleteFlag(TFlag flag)
    {
        if(EditorUtility.DisplayDialog("Hold Up", $"The Flag, {flag.Name} will be deleted.\n" +
            $"Deleted flags will be lost forever and cannot be recovered. " +
            $"This may also cause null references if not handled properly.\n" +
            $"Are you sure you wish to do this?", "Delete", "Cancel"))
        {
            Flags.Remove(flag);
            AssetDatabase.RemoveObjectFromAsset(flag);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(flag));

            RefreshIDs();

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(this);
        }
    }


    public virtual string ValidateFlag(TFlag flag, string name, int iterations = 1)
    {
        foreach(TFlag f in Flags)
        {
            if (f == flag) continue;

            if(f.Name.ToLower() == flag.Name.ToLower())
            {
                flag.Name = name + $"({iterations + 1})";
                return ValidateFlag(flag, name, iterations + 1);
            }
        }

        return flag.Name;
    }


    protected virtual void RefreshIDs()
    {
        for (int i=0; i<Flags.Count; i++)
        {
            Flags[i].ID = i;
        }
    }


    private void SetGlobal()
    {
        if (this.isGlobalManager)
        {
            string path = AssetDatabase.GetAssetPath(this);

            if (!path.Contains("Resources"))
            {
                Debug.LogError($"ATTENTION: {this.name} is not in the Resources folder. " +
                    $"Please move it to Resources or it will not initialize.");
            }

            AssetDatabase.RenameAsset(path, "Global Flag Manager");

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(this);
        }
    }
#endif

    #region On Enable
    /// <summary>
    /// Populate all our maps upon startup.
    /// </summary>
    private void OnEnable()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }
    #endregion

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void E()
    {
        Resources.Load<TFlagManager>("Global Flag Manager");
    }
}
