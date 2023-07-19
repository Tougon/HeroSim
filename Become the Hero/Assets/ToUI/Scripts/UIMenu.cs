using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        protected RectTransform ScrollArea;
        [SerializeField]
        protected RectTransform Viewport;
        [SerializeField]
        protected float ScrollSpeed = 5.0f;
        [SerializeField]
        protected float ScrollBuffer = 20.0f;
        [SerializeField]
        protected float InitialRepeatDelay = 1.0f;
        [SerializeField]
        protected float RepeatRate = 0.3f;
        [SerializeField]
        protected bool WrapSelection = true;
        [SerializeField]//[HideIf("@this.GetType() != typeof(UIMenu)")]
        [InfoBox("Drag UIMenuItems into the matrix to define navigation. Right click to remove an element.")]
        [TableMatrix(HorizontalTitle = "Menu Selection", DrawElementMethod = "DrawElement")]
        protected UIMenuItem[,] SelectionMatrix = new UIMenuItem[0, 0];
        [SerializeField][HideIf("@this.GetType() != typeof(UIMenu)")]
        protected UIMenuItem InitialSelection;

        protected UIMenuItem CurrentSelection;
        protected int column;
        protected int row;
        protected bool bAllowInput;

        protected enum SelectionFallbackType { Next, SameRow, SameColumn }
        protected SelectionFallbackType Fallback = SelectionFallbackType.Next;

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

            if(CurrentSelection != null)
                CurrentSelection.SetSelected(true);
        }


        protected virtual void Update()
        {
            if (!bAllowInput) return;

            if (lastDirection.sqrMagnitude > 0)
            {
                if(CurrentSelection != null)
                {
                    if(lastDirection.x > 0) CurrentSelection.OnRightPressed();
                    if(lastDirection.x < 0) CurrentSelection.OnLeftPressed();
                    if(lastDirection.y > 0) CurrentSelection.OnUpPressed();
                    if(lastDirection.y < 0) CurrentSelection.OnDownPressed();
                }

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
        protected virtual void ChangeSelection(Vector2 Direction)
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

            } while ((SelectionIsInvalid(AttemptedSelection, targetRow, targetColumn) || 
                AttemptedSelection == CurrentSelection) && !(targetRow == row && targetColumn == column));


            // Handle fallbacks
            while(Fallback == SelectionFallbackType.SameRow && !AttemptedSelection.gameObject.activeInHierarchy)
            {
                targetColumn -= 1;
                AttemptedSelection = SelectionMatrix[targetColumn, targetRow];

                if (targetColumn < 0)
                    break;
            }

            while (Fallback == SelectionFallbackType.SameColumn && !AttemptedSelection.gameObject.activeInHierarchy)
            {
                targetRow -= 1;
                AttemptedSelection = SelectionMatrix[targetColumn, targetRow];

                if (targetRow < 0)
                    break;
            }

            // Select the element and change the row and column
            if (AttemptedSelection != CurrentSelection && !SelectionIsInvalid(AttemptedSelection))
            {
                CurrentSelection.SetSelected(false);

                CurrentSelection = AttemptedSelection;
                row = targetRow;
                column = targetColumn;

                CurrentSelection.SetSelected(true);
            }

            ScrollToTarget();
        }


        protected virtual Vector2 ScrollToTarget()
        {
            if (ScrollArea != null)
            {
                if(!RectFullyContainsElement(Viewport, CurrentSelection.transform as RectTransform,
                    out Vector2 dif))
                {
                    Vector2 offset = new Vector2(
                        dif.x == 0 ? 0 : dif.x / Mathf.Abs(dif.x), 
                        dif.y == 0 ? 0 : dif.y / Mathf.Abs(dif.y));

                    ScrollArea.DOKill();
                    ScrollArea.DOAnchorPos(ScrollArea.anchoredPosition + dif + (offset * ScrollBuffer), 
                        1 / ScrollSpeed);

                    return dif + (offset * ScrollBuffer);
                }
            }

            return Vector2.zero;
        }


        /// <summary>
        /// Checks if a rect transform fully contains another rect transform and calculates the difference
        /// </summary>
        protected bool RectFullyContainsElement(RectTransform rect, RectTransform target, out Vector2 difference)
        {
            var sourceRect = GetWorldRect(rect);
            var otherRect = GetWorldRect(target);

            difference = Vector2.zero;
            bool result = true;

            if(sourceRect.xMin > otherRect.xMin)
            {
                result = false;
                difference.x = -((otherRect.xMin) - sourceRect.xMin);
            }
            if (sourceRect.yMin > otherRect.yMin)
            {
                result = false;
                difference.y = -((otherRect.yMin) - sourceRect.yMin);
            }
            if (sourceRect.xMax < otherRect.xMax)
            {
                result = false;
                difference.x = -((otherRect.xMax) - sourceRect.xMax);
            }
            if (sourceRect.yMax < otherRect.yMax)
            {
                result = false;
                difference.y = -((otherRect.yMax) - sourceRect.yMax);
            }

            if (!result)
            {
                float ratio = target.rect.width / otherRect.width;
                difference *= ratio;
            }

            return result;
        }


        private Rect GetWorldRect(RectTransform rectTransform)
        {
            // This returns the world space positions of the corners in the order
            // [0] bottom left,
            // [1] top left
            // [2] top right
            // [3] bottom right
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector2 min = corners[0];
            Vector2 max = corners[2];
            Vector2 size = max - min;

            return new Rect(min, size);
        }


        protected bool SelectionIsInvalid(UIMenuItem AttemptedSelection, int AttemptedRow = -1, int AttemptedColumn = -1)
        {
            switch (Fallback)
            {
                case SelectionFallbackType.SameRow:

                    if(AttemptedSelection == null || !AttemptedSelection.enabled ||
                        !AttemptedSelection.gameObject.activeInHierarchy)
                    {
                        // Return false if there are no active elements in the row.
                        for(int i= SelectionMatrix.GetLength(0) - 1; i>=0; i--)
                        {
                            if (SelectionMatrix[i, AttemptedRow].gameObject.activeInHierarchy &&
                                SelectionMatrix[i, AttemptedRow].enabled)
                                return false;
                        }

                        return true;
                    }

                    return false;

                //case SelectionFallbackType.SameColumn: return AttemptedSelection == null;
            }
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
