using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;

public class SpellEditor : OdinMenuEditorWindow
{
    private CreateNewSpell createNewSpell;


    [MenuItem("Tools/Spell Editor")]
    private static void OpenWindow()
    {
        GetWindow<SpellEditor>().Show();
    }


    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();

        createNewSpell = new CreateNewSpell();
        tree.Add("Create New Spell", createNewSpell);
        tree.AddAllAssetsAtPath("Spells", "Assets/Spells", typeof(Spell), true, false);

        return tree;
    }


    protected override void OnBeginDrawEditors()
    {
        base.OnBeginDrawEditors();

        OdinMenuTreeSelection selected = this.MenuTree.Selection;

        SirenixEditorGUI.BeginHorizontalToolbar();
        {
            GUILayout.FlexibleSpace();

            if(SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                Spell asset = selected.SelectedValue as Spell;
                string path = AssetDatabase.GetAssetPath(asset);

                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (createNewSpell != null)
            DestroyImmediate(createNewSpell.spell);
    }


    public class CreateNewSpell
    {
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [PropertySpace(15)]
        [ShowIf("DirectoryIsValid")]
        public Spell spell;

        [PropertySpace(20)]
        [FolderPath(RequireExistingPath = true, ParentFolder = "$parent", AbsolutePath = false)]
        public string path = "";

        private string parent = "Assets/Spells";
        private bool showError = false;
        private string errorMessage = "";

        private enum CreatingSpellType { Default, Offensive, Status }
        private CreatingSpellType spellType = CreatingSpellType.Default;


        public CreateNewSpell()
        {
            ResetWindow();
        }

        [PropertySpace(25)]
        [InfoBox("$errorMessage", InfoMessageType.Error, "showError")]
        [EnableIf("CanCreate")]
        [Button("Create New Spell", ButtonSizes.Large)]
        public void Create()
        {
            AssetDatabase.CreateAsset(spell, parent + "/" + path + "/" + spell.spellName.Trim() + ".asset");
            AssetDatabase.SaveAssets();

            // Reset
            ResetWindow();
        }

        #region Creation Validation


        private bool DirectoryIsValid()
        {
            return !(parent + "/" + path).Contains("../");
        }


        private bool CanCreate()
        {
            errorMessage = "Cannot Create!";


            if (!DirectoryIsValid())
            {
                errorMessage += "\nSpell is not in a valid directory. Spells must be in the Spells folder.";
                showError = true;
                return false;
            }

            // If the spell's name isn't empty
            if (spell.spellName.Trim() == "")
            {
                errorMessage += "\nSpell's name is empty.";
                showError = true;
                return false;
            }

            if (spell.spellAnimation == null)
            {
                errorMessage += "\nSpell's animation is null.";
                showError = true;
                return false;
            }

            // Check if spell does not already exist
            var assets = AssetDatabase.FindAssets(spell.spellName.Trim());

            if (assets.Length > 1)
            {
                foreach(var asset in assets)
                {
                    string targetPath = AssetDatabase.GUIDToAssetPath(asset);

                    if (targetPath.Replace(spell.spellName.Trim() + ".asset", "").Equals(parent + "/" + path + "/"))
                    {
                        errorMessage += "\nSpell already exists in the given directory.";
                        showError = true;
                        return false;
                    }
                }
            }

            showError = false;
            return true;
        }

        #endregion


        public void ResetWindow()
        {
            switch (spellType)
            {
                case CreatingSpellType.Default:
                    SetSpellToDefault();
                    break;
                case CreatingSpellType.Offensive:
                    SetSpellToOffensive();
                    break;
                case CreatingSpellType.Status:
                    SetSpellToStatus();
                    break;
                default:
                    SetSpellToDefault();
                    break;
            }
        }


        [PropertyOrder(-1)]
        [ButtonGroup("ResetButtons")]
        [Button(ButtonSizes.Large)]
        [LabelText("Standard Spell")]
        public void SetSpellToDefault()
        {
            spell = ScriptableObject.CreateInstance<Spell>();
            spell.spellName = "New Spell";
            spellType = CreatingSpellType.Default;
        }


        [PropertyOrder(-1)]
        [ButtonGroup("ResetButtons")]
        [Button(ButtonSizes.Large)]
        [LabelText("Offensive Spell")]
        public void SetSpellToOffensive()
        {
            spell = ScriptableObject.CreateInstance<OffensiveSpell>();
            spell.spellName = "New Offensive Spell";
            spellType = CreatingSpellType.Offensive;
        }


        [PropertyOrder(-1)]
        [ButtonGroup("ResetButtons")]
        [Button(ButtonSizes.Large)]
        [LabelText("Status Spell")]
        public void SetSpellToStatus()
        {
            spell = ScriptableObject.CreateInstance<StatusSpell>();
            spell.spellName = "New Status Spell";
            spellType = CreatingSpellType.Status;
        }
    }
}
