using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace ToUI
{
    public class UIMenu : UIScreen
    {
        [Header("Menu Properties")]
        [SerializeField]
        protected float InitialRepeatDelay = 1.0f;
        [SerializeField]
        protected float RepeatRate = 0.3f;
        [SerializeField]
        protected bool WrapSelection = true;
        [SerializeField]
        [InfoBox("Drag UIMenuItems into the matrix to define navigation. Right click to remove an element.")]
        [TableMatrix(HorizontalTitle = "Menu Selection", DrawElementMethod = "DrawElement")]
        protected UIMenuItem[,] SelectionMatrix = new UIMenuItem[0, 0];
        [SerializeField]
        protected UIMenuItem InitialSelection;

        protected UIMenuItem CurrentSelection;
        protected int column;
        protected int row;
        protected bool bAllowInput;

        private Vector2 lastDirection;
        private float repeatTimer;
        private bool bWaitInitial = true;


        #region Editor Only
#if UNITY_EDITOR
        private UIMenuItem DrawElement(Rect rect, UIMenuItem value)
        {
            Event evt = Event.current;

            bool inBounds = rect.Contains(evt.mousePosition);
            UIMenuItem target = null;

            foreach (var item in DragAndDrop.objectReferences)
            {
                if (item is GameObject)
                {
                    if ((item as GameObject).GetComponent<UIMenuItem>())
                    {
                        target = (item as GameObject).GetComponent<UIMenuItem>();
                        break;
                    }
                }
            }

            string name = "None";
            Color color = new Color(0, 0, 0, 0.5f);

            if (inBounds && target != null)
            {
                name = target.name;
                color = new Color(0.25f, 0.85f, 0.76f);
            }
            else if(value != null)
            {
                name = value.name;

                if(!value.enabled || !value.gameObject.activeInHierarchy)
                    color = new Color(0.8f, 0.2f, 0.1f);
                else
                    color = new Color(0.1f, 0.8f, 0.2f);
            }

            EditorGUI.DrawRect(rect.Padding(1), color);
            GUI.Box(rect, name);

            switch (evt.type)
            {
                case EventType.MouseDown:

                    if (!inBounds)
                        break;

                    if (Event.current.button == 1)
                    {
                        value = null;
                    }

                    GUI.changed = true;
                    Event.current.Use();

                    break;
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                    break;
                case EventType.DragPerform:

                    if (!inBounds || target == null)
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        value = target;

                        GUI.changed = true;
                        Event.current.Use();
                    }
                    break;
            }

            return value;
        }
#endif
        #endregion


        protected override void Start()
        {
            base.Start();

            if(SelectionMatrix != null)
            {
                for(int c=0; c < SelectionMatrix.GetLength(0); c++)
                {
                    for(int r=0; r < SelectionMatrix.GetLength(1); r++)
                    {
                        // If no initial selection is assigned, go for the first available item
                        if (SelectionIsInvalid(InitialSelection) && SelectionMatrix[c, r] != null)
                        {
                            InitialSelection = SelectionMatrix[c, r];
                            row = r;
                            column = c;
                            break;
                        }
                        // Otherwise, find the row and column of the first selection
                        else if (!SelectionIsInvalid(InitialSelection) && SelectionMatrix[c, r] == InitialSelection)
                        {
                            row = r;
                            column = c;
                            break;
                        }
                    }
                }

                if(InitialSelection == null)
                {
                    Debug.LogWarning("UI Menu lacks options. Consider using UIScreen.");
                }
            }
            else
            {
                Debug.LogWarning("Selection matrix is empty. Consider using UIScreen.");
            }

            CurrentSelection = InitialSelection;
            CurrentSelection.SetSelected(true);
        }


        protected virtual void Update()
        {
            if (!bAllowInput) return;

            if (lastDirection.sqrMagnitude > 0)
            {
                repeatTimer += Time.deltaTime;

                if ((bWaitInitial && repeatTimer >= InitialRepeatDelay) ||
                    (!bWaitInitial && repeatTimer >= RepeatRate))
                {
                    ChangeSelection(lastDirection);

                    repeatTimer = 0;
                    bWaitInitial = false;
                }
            }
        }


        public override void Show()
        {
            base.Show();
            bAllowInput = false;
        }

        protected override void OnScreenShown()
        {
            base.OnScreenShown();

            bAllowInput = true;
            bWaitInitial = true;
            repeatTimer = 0;
        }

        public override void FocusChanged(bool bFocus)
        {
            base.FocusChanged(bFocus);

            bAllowInput = bFocus;
        }


        /// <summary>
        /// Moves the selection in the given direction to select a new item if possible.
        /// </summary>
        protected void ChangeSelection(Vector2 Direction)
        {
            // Initialize selection variables
            int targetRow = row;
            int targetColumn = column;

            var AttemptedSelection = CurrentSelection;

            do
            {
                targetRow -= Mathf.RoundToInt(Direction.y);
                targetColumn += Mathf.RoundToInt(Direction.x);

                // Wrap around the target row
                if (targetRow >= SelectionMatrix.GetLength(1))
                {
                    if (WrapSelection) targetRow = 0;
                    else break;
                }
                    
                if (targetRow < 0)
                {
                    if (WrapSelection) targetRow = SelectionMatrix.GetLength(1) - 1;
                    else break;
                }

                // Wrap around the target column
                if (targetColumn >= SelectionMatrix.GetLength(0))
                {
                    if (WrapSelection) targetColumn = 0;
                    else break;
                }

                if (targetColumn < 0)
                {
                    if (WrapSelection) targetColumn = SelectionMatrix.GetLength(0) - 1;
                    else break;
                }

                AttemptedSelection = SelectionMatrix[targetColumn, targetRow];

            } while ((SelectionIsInvalid(AttemptedSelection) || AttemptedSelection == CurrentSelection) &&
                !(targetRow == row && targetColumn == column));

            if(AttemptedSelection != CurrentSelection && !SelectionIsInvalid(AttemptedSelection))
            {
                CurrentSelection.SetSelected(false);

                CurrentSelection = AttemptedSelection;
                row = targetRow;
                column = targetColumn;

                CurrentSelection.SetSelected(true);
            }
        }


        protected bool SelectionIsInvalid(UIMenuItem AttemptedSelection)
        {
            return AttemptedSelection == null || !AttemptedSelection.enabled ||
                !AttemptedSelection.gameObject.activeInHierarchy;
        }


        #region Input Responses

        /// <summary>
        /// Executs the proper movement functions based on provided value
        /// </summary>
        public override void OnMovementUpdate(Vector2 movement)
        {
            if(lastDirection != movement)
            {
                lastDirection = movement;
                base.OnMovementUpdate(movement);

                bWaitInitial = true;
                repeatTimer = 0;
            }
        }

        /// <summary>
        /// Action to execute when up is pressed
        /// </summary>
        public override void OnUpPressed()
        {
            if (!bAllowInput) return;

            if (CurrentSelection != null)
                CurrentSelection.OnUpPressed();

            ChangeSelection(Vector2.up);
        }

        /// <summary>
        /// Action to execute when down is pressed
        /// </summary>
        public override void OnDownPressed()
        {
            if (!bAllowInput) return;

            if (CurrentSelection != null)
                CurrentSelection.OnDownPressed();

            ChangeSelection(Vector2.down);
        }

        /// <summary>
        /// Action to execute when left is pressed
        /// </summary>
        public override void OnLeftPressed()
        {
            if (!bAllowInput) return;

            if (CurrentSelection != null)
                CurrentSelection.OnLeftPressed();

            ChangeSelection(Vector2.left);
        }

        /// <summary>
        /// Action to execute when right is pressed
        /// </summary>
        public override void OnRightPressed()
        {
            if (!bAllowInput) return;

            if (CurrentSelection != null)
                CurrentSelection.OnRightPressed();

            ChangeSelection(Vector2.right);
        }

        /// <summary>
        /// Action to execute when Confirm is pressed
        /// </summary>
        public override void OnConfirmPressed()
        {
            if (!bAllowInput) return;

            if (CurrentSelection != null)
                CurrentSelection.OnConfirmPressed();
        }

        /// <summary>
        /// Action to execute when Cancel is pressed
        /// </summary>
        public override void OnCancelPressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");

            if (CurrentSelection != null)
                CurrentSelection.OnCancelPressed();
        }

        /// <summary>
        /// Action to execute when Aux1 is pressed
        /// </summary>
        public override void OnAux1Pressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");

            if (CurrentSelection != null)
                CurrentSelection.OnAux1Pressed();
        }

        /// <summary>
        /// Action to execute when Aux2 is pressed
        /// </summary>
        public override void OnAux2Pressed()
        {
            if (!bAllowInput) return;
            Debug.Log("Pressed");

            if (CurrentSelection != null)
                CurrentSelection.OnAux2Pressed();
        }

        #endregion
    }
}
