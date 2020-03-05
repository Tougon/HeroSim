using UnityEngine;
using UnityEditor;
using Hero.SpellEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;

public class EntityEditor : OdinMenuEditorWindow
{
    private CreateNewEntity createNewEntity;
    private AddEntityToListPopup entityManagerPopup;
    
    [MenuItem("Tools/Entity Editor", false, 6)]
    private static void OpenWindow()
    {
        GetWindow<EntityEditor>().Show();
        
    }


    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree()
        {

        };

        createNewEntity = new CreateNewEntity();
        tree.Add("Create New Entity", createNewEntity);
        var res = tree.AddAllAssetsAtPath("Entities", "Assets/Entities", typeof(Entity), true, true);
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

            if(SirenixEditorGUI.ToolbarButton("Add To Enemy List"))
            {
                Entity asset = selected.SelectedValue as Entity;

                if (asset != null && SpellEditorUtilities.DoesAssetExist(asset))
                {
                    entityManagerPopup = new AddEntityToListPopup();
                    entityManagerPopup.entity = asset;
                    PopupWindow.Show(GUILayoutUtility.GetLastRect(), entityManagerPopup);
                }
                else
                {
                    this.ShowNotification(new GUIContent("Entity does not exist!"), 1.5f);
                }
            }

            if(SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                Entity asset = selected.SelectedValue as Entity;

                if (asset == null)
                {
                    this.ShowNotification(new GUIContent("Entity does not exist!"), 1.5f);
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

        if (createNewEntity != null)
        {
            DestroyImmediate(createNewEntity.entity);
        }

        SpellEditorUtilities.CleanUp();
    }


    public class CreateNewEntity
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
        public Entity entity;

        private string parent = "Assets/Entities";
        private bool showError = false;
        private string errorMessage = "";


        public CreateNewEntity()
        {
            ResetWindow();
        }

        [PropertySpace(25)]
        [InfoBox("$errorMessage", InfoMessageType.Error, "showError")]
        [EnableIf("CanCreate")]
        [Button("Create New Entity", ButtonSizes.Large)]
        public void Create()
        {
            SpellEditorUtilities.CreateAsset(entity, parent + "/" + path + "/" + entity.vals.entityName.Trim());

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
            if (entity.vals.entityName.Trim() == "")
            {
                errorMessage += "\nEntity's name is empty.";
                showError = true;
                return false;
            }

            // If the spell's name isn't empty
            if (entity.vals.entitySprite == null)
            {
                errorMessage += "\nEntity's sprite is null.";
                showError = true;
                return false;
            }

            // Check if spell does not already exist

            if (SpellEditorUtilities.CheckIfAssetExists(entity.vals.entityName.Trim(), parent + "/" + path + "/"))
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
            entity = ScriptableObject.CreateInstance<Entity>();
            entity.vals = new EntityParams();
            entity.vals.entityName = "New Entity";
        }
    }
}


public class AddEntityToListPopup : PopupWindowContent
{
    public Entity entity;
    public int entityOdds = 50;

    public override Vector2 GetWindowSize()
    {
        return new Vector2(400, 150);
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("Enter Relative Odds", EditorStyles.boldLabel);
        entityOdds = EditorGUILayout.IntSlider(entityOdds, 1, 100);
        
        if (GUILayout.Button("Add"))
        {
            EnemyManager manager = Resources.Load<EnemyManager>("Enemy Manager");

            if(manager != null)
            {
                // Check if manager already has the entity. If it does, update its odds, otherwise, add it
                var odds = manager.GetEnemy(entity);

                if (odds == null)
                    manager.AddNewEnemy(entity, entityOdds);
                else
                    manager.UpdateEnemy(entity, entityOdds);
                this.editorWindow.Close();
            }
            else
            {
                Debug.LogError("Cannot find the Enemy Manager! Check to see if an Enemy Manager exists in Resources.");
            }
        }
    }
}