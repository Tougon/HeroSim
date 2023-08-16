using UnityEngine;
using UnityEditor;
using Hero.SpellEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;

public class AnimationEditor : OdinMenuEditorWindow
{
    private CreateNewAnimation createNewAnimation;
    
    [MenuItem("Tools/Animation Editor", false, 7)]
    private static void OpenWindow()
    {
        GetWindow<AnimationEditor>().Show();
        
    }


    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree()
        {

        };

        createNewAnimation = new CreateNewAnimation();
        tree.Add("Create New Animation", createNewAnimation);
        var res = tree.AddAllAssetsAtPath("Animations", "Assets/Animations/Sequences", 
            typeof(AnimationSequenceObject), true, true);
        res.SortMenuItemsByName();
        return tree;
    }


    protected override void OnBeginDrawEditors()
    {
        base.OnBeginDrawEditors();

        OdinMenuTreeSelection selected = this.MenuTree != null ? this.MenuTree.Selection : null;

        SirenixEditorGUI.BeginHorizontalToolbar();
        {
            GUILayout.FlexibleSpace();

            if(SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                AnimationSequenceObject asset = selected.SelectedValue as AnimationSequenceObject;

                if (asset == null)
                {
                    this.ShowNotification(new GUIContent("Animation does not exist!"), 1.5f);
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

        if (createNewAnimation != null)
        {
            DestroyImmediate(createNewAnimation.animation);
        }

        SpellEditorUtilities.CleanUp();
    }


    public class CreateNewAnimation
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
        public AnimationSequenceObject animation;

        private string parent = "Assets/Animations";
        private bool showError = false;
        private string errorMessage = "";


        public CreateNewAnimation()
        {
            ResetWindow();
        }

        [PropertySpace(25)]
        [InfoBox("$errorMessage", InfoMessageType.Error, "showError")]
        [EnableIf("CanCreate")]
        [Button("Create New Animation", ButtonSizes.Large)]
        public void Create()
        {
            string finalPath = path.Trim().Equals("") ? parent : parent + "/" + path;

            SpellEditorUtilities.CreateAsset(animation, finalPath, animation.animationName.Trim() + "Anim");

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
                errorMessage += "\nSpell is not in a valid directory. Entities must be in the Entities folder.";
                showError = true;
                return false;
            }

            // If the spell's name isn't empty
            if (animation.animationName.Trim() == "")
            {
                errorMessage += "\nEntity's name is empty.";
                showError = true;
                return false;
            }

            // Check if spell does not already exist

            if (SpellEditorUtilities.CheckIfAssetExists(animation.animationName.Trim(), parent + "/" + path + "/"))
            {
                errorMessage += "\nEntity already exists in the given directory.";
                showError = true;
                return false;
            }

            showError = false;
            return true;
        }

        #endregion


        public void ResetWindow()
        {
            animation = ScriptableObject.CreateInstance<AnimationSequenceObject>();
            animation.animationName = "New Entity";
        }
    }
}