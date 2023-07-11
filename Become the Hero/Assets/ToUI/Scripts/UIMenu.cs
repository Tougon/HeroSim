using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToUI
{
    public class UIMenu : UIScreen
    {
        [Header("Menu Properties")]
        [SerializeField]
        [TableMatrix(HorizontalTitle = "Menu Selection", DrawElementMethod = "DrawElement", ResizableColumns = false)]
        protected UIMenuItem[,] SelectionMatrix = new UIMenuItem[0, 0];
        [SerializeField]
        protected UIMenuItem InitialSelection;

        protected UIMenuItem CurrentSelection;
        protected int column;
        protected int row;
        protected bool bAllowInput;

        private Vector2 lastDirection;


        #region Editor Only
#if UNITY_EDITOR
        private static UIMenuItem DrawElement(Rect rect, UIMenuItem value)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0, 0, 0, 0.5f));
            UnityEditor.EditorGUI.DropShadowLabel(rect.Padding(2), value ? value.name : "None");

            return value;
        }
#endif
        #endregion


        protected override void Awake()
        {
            base.Awake();

            if(SelectionMatrix != null)
            {
                for(int c=0; c < SelectionMatrix.GetLength(0); c++)
                {
                    for(int r=0; r < SelectionMatrix.GetLength(1); r++)
                    {
                        // If no initial selection is assigned, go for the first available item
                        if (InitialSelection == null && SelectionMatrix[c, r] != null)
                        {
                            InitialSelection = SelectionMatrix[c, r];
                            row = r;
                            column = c;
                            break;
                        }
                        // Otherwise, find the row and column of the first selection
                        else if (InitialSelection != null && SelectionMatrix[c, r] == InitialSelection)
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


        public override void Show()
        {
            base.Show();
            bAllowInput = false;
        }

        protected override void OnScreenShown()
        {
            base.OnScreenShown();

            bAllowInput = true;
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
                targetRow -= (int)Direction.y;
                targetColumn += (int)Direction.x;

                // Wrap around the target row
                if (targetRow >= SelectionMatrix.GetLength(1))
                    targetRow = 0;
                else if (targetRow < 0)
                    targetRow = SelectionMatrix.GetLength(1) - 1;

                // Wrap around the target column
                if (targetColumn >= SelectionMatrix.GetLength(0))
                    targetColumn = 0;
                else if (targetColumn < 0)
                    targetColumn = SelectionMatrix.GetLength(0) - 1;

                AttemptedSelection = SelectionMatrix[targetColumn, targetRow];

            } while (AttemptedSelection == null);

            if(AttemptedSelection != CurrentSelection)
            {
                CurrentSelection.SetSelected(false);

                CurrentSelection = AttemptedSelection;
                row = targetRow;
                column = targetColumn;

                CurrentSelection.SetSelected(true);
            }
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
            Debug.Log("Pressed");

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
