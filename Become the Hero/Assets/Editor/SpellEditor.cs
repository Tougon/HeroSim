using UnityEngine;
using UnityEditor;
using Hero.SpellEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;

public class SpellEditor : OdinMenuEditorWindow
{
    private CreateNewSpell createNewSpell;
    private CreateEffectPopup createNewEffect;
    
    [MenuItem("Tools/Spell Editor", false, 5)]
    private static void OpenWindow()
    {
        GetWindow<SpellEditor>().Show();
        
    }


    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree()
        {

        };

        createNewSpell = new CreateNewSpell();
        tree.Add("Create New Spell", createNewSpell);
        var res = tree.AddAllAssetsAtPath("Spells", "Assets/Spells", typeof(Spell), true, true);
        res.SortMenuItemsByName();
        return tree;
    }


    protected override void OnBeginDrawEditors()
    {
        base.OnBeginDrawEditors();
        
        OdinMenuTreeSelection selected = this.MenuTree.Selection;

        SirenixEditorGUI.BeginHorizontalToolbar();
        {
            GUILayout.FlexibleSpace();

            if(SirenixEditorGUI.ToolbarButton("Create Effect"))
            {
                createNewEffect = new CreateEffectPopup();
                PopupWindow.Show(GUILayoutUtility.GetLastRect(), createNewEffect);
            }

            if(SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                Spell asset = selected.SelectedValue as Spell;

                if (asset == null)
                {
                    this.ShowNotification(new GUIContent("Spell does not exist!"), 1.5f);
                    return;
                }

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
        {
            if (createNewSpell.spell.spellFamily != null &&
                    !SpellEditorUtilities.CheckIfAssetExists(createNewSpell.spell.spellFamily.name, "Assets/Spells/Families/"))
                DestroyImmediate(createNewSpell.spell.spellFamily);

            if (createNewSpell.spell.spellAnimation != null && !SpellEditorUtilities.DoesAssetExist(createNewSpell.spell.spellAnimation) &&
                    (!SpellEditorUtilities.CheckIfAssetExists(createNewSpell.spell.spellAnimation.name, "Assets/Spells/" +
                    SpellEditorUtilities.currentPath + "/")))
                DestroyImmediate(createNewSpell.spell.spellAnimation);
            
            if(createNewSpell.spell.spellEffects != null)
            {

                foreach (var chance in createNewSpell.spell.spellEffects)
                {
                    foreach (var effect in chance.effects)
                    {
                        if (effect.effect.display != null && !SpellEditorUtilities.DoesAssetExist(effect.effect.display))
                        {
                            DestroyImmediate(effect.effect.display);
                            effect.effect.display = null;
                        }
                    }
                }
            }

            if (createNewSpell.spell.spellProperties != null)
            {
                foreach (var property in createNewSpell.spell.spellProperties)
                {
                    if (property.display != null && !SpellEditorUtilities.DoesAssetExist(property.display))
                    {
                        DestroyImmediate(property.display);
                        property.display = null;
                    }
                }
            }

            DestroyImmediate(createNewSpell.spell);
        }

        SpellEditorUtilities.CleanUp();
    }


    public class CreateNewSpell
    {
        [FolderPath(RequireExistingPath = true, ParentFolder = "$parent", AbsolutePath = false)]
        [OnValueChanged("SetCurrentPath")]
        [InfoBox("@GetFullPath()")]
        public string path = "";
        private void SetCurrentPath() { SpellEditorUtilities.currentPath = path; }
        private string GetFullPath()
        {
            if (!DirectoryIsValid())
                return "BAD PATH";
            else
                return parent + "/" + path;
        }


        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        [PropertySpace(20)]
        [ShowIf("DirectoryIsValid")]
        public Spell spell;

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
            string finalPath = path.Trim().Equals("") ? parent : parent + "/" + path;

            SpellEditorUtilities.CreateAsset(spell, finalPath, spell.spellName.Trim());

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

            if(spell.spellFamily != null && !SpellEditorUtilities.CheckIfAssetExists
                (spell.spellFamily.name, "Assets/Spells/Families/"))
            {
                errorMessage += "\nSpell's family does not exist in assets. Click Create to create it.";
                showError = true;
                return false;
            }

            if (spell.spellAnimation == null)
            {
                errorMessage += "\nSpell's animation is null.";
                showError = true;
                return false;
            }
            else if (spell.spellAnimation != null && (!SpellEditorUtilities.DoesAssetExist(spell.spellAnimation) &&
                !SpellEditorUtilities.CheckIfAssetExists
                (spell.spellAnimation.name, "Assets/Spells/" + SpellEditorUtilities.currentPath + "/")))
            {
                errorMessage += "\nSpell's animation does not exist in assets. Click Create to create it.";
                showError = true;
                return false;
            }

            // Check if spell does not already exist

            if (SpellEditorUtilities.CheckIfAssetExists(spell.spellName.Trim(), parent + "/" + path + "/"))
            {
                errorMessage += "\nSpell already exists in the given directory.";
                showError = true;
                return false;
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
        [GUIColor(0.85f, 1.0f, 0.95f)]
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
        [GUIColor(0.85f, 1.0f, 0.95f)]
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
        [GUIColor(0.85f, 1.0f, 0.95f)]
        public void SetSpellToStatus()
        {
            spell = ScriptableObject.CreateInstance<StatusSpell>();
            spell.spellName = "New Status Spell";
            spellType = CreatingSpellType.Status;
        }
    }
}


public class CreateEffectPopup : PopupWindowContent
{
    public string effectName = "";

    public override Vector2 GetWindowSize()
    {
        return new Vector2(400, 150);
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("Enter Effect Name", EditorStyles.boldLabel);
        effectName = EditorGUILayout.TextField("Effect Name: ", effectName);

        if(effectName.Replace(" ", "") != "" && !SpellEditorUtilities.CheckIfAssetExists(effectName, "Assets/Spells/Common/Effects/"))
        {
            if (GUILayout.Button("Create"))
            {
                SpellEditorUtilities.CreateAsset(ScriptableObject.CreateInstance<Effect>(), 
                    "Assets/Spells/Common/Effects", (effectName.Replace(" ", "")));

                this.editorWindow.Close();
            }
        }
        else
        {
            if (effectName.Replace(" ", "") == "")
                GUILayout.Label("Invalid Effect name!", EditorStyles.boldLabel);
            else
                GUILayout.Label("An Effect with the given name already exists!", EditorStyles.boldLabel);
        }
    }
}