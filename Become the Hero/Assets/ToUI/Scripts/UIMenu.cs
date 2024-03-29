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
        [SerializeField][HideIf("@this is UIDynamicMenu")]
        [InfoBox("Drag UIMenuItems into the matrix to define navigation. Right click to remove an element.")]
        [TableMatrix(HorizontalTitle = "Menu Selection", DrawElementMethod = "DrawElement")]
        protected UIMenuItem[,] SelectionMatrix = new UIMenuItem[0, 0];
        [SerializeField][HideIf("@this is UIDynamicMenu")]
        protected UIMenuItem InitialSelection;
        [SerializeField]
        protected bool highlightOnOpen = true;

        protected UIMenuItem CurrentSelection;
        protected int column;
        protected int row;
        protected bool bAllowInput;
        private bool bPlayingShowAnim;

        protected enum SelectionFallbackType { Next, SameRow, SameColumn }
        protected SelectionFallbackType Fallback = SelectionFallbackType.Next;

        private Vector2 lastDirection;
        private float repeatTimer;
        private bool bWaitInitial = true;
        private bool[] scrollDirections = new bool[4];

        [Header("Scroll Visibility Properties")]
        [SerializeField] protected TweenSystem TopScrollIndicator;
        [SerializeField] protected TweenSystem BottomScrollIndicator;
        [SerializeField] protected TweenSystem LeftScrollIndicator;
        [SerializeField] protected TweenSystem RightScrollIndicator;

        private TweenSystem[] scrollIndicators = new TweenSystem[4];


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
                SetSelection(InitialSelection, true);

                if (InitialSelection == null)
                {
                    Debug.LogWarning("UI Menu lacks options. Consider using UIScreen.");
                }
            }
            else
            {
                Debug.LogWarning("Selection matrix is empty. Consider using UIScreen.");
            }

            if(CurrentSelection != null)
                CurrentSelection.SetSelected(true);

            scrollIndicators[0] = TopScrollIndicator;
            scrollIndicators[1] = BottomScrollIndicator;
            scrollIndicators[2] = LeftScrollIndicator;
            scrollIndicators[3] = RightScrollIndicator;
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
            bAllowInput = false;
            bPlayingShowAnim = true;
            base.Show();

            OnViewChanged();

            if (CurrentSelection && highlightOnOpen)
            {
                CurrentSelection.SetSelected(true);
            }
        }

        protected override void OnScreenShown()
        {
            base.OnScreenShown();

            bPlayingShowAnim = false;
            bAllowInput = true;
            bWaitInitial = true;
            repeatTimer = 0;
        }


        public override void Hide()
        {
            bAllowInput = false;
            base.Hide();
        }


        public override void FocusChanged(bool bFocus)
        {
            base.FocusChanged(bFocus);

            if (!bPlayingShowAnim)
            {
                bAllowInput = bFocus;
            }
        }


        public virtual void SetSelection(UIMenuItem Item)
        {
            SetSelection(Item, false);
        }


        protected virtual void SetSelection(UIMenuItem Item, bool initialize = false)
        {
            for (int c = 0; c < SelectionMatrix.GetLength(0); c++)
            {
                for (int r = 0; r < SelectionMatrix.GetLength(1); r++)
                {
                    // If no initial selection is assigned, go for the first available item
                    if (initialize && SelectionIsInvalid(Item) && SelectionMatrix[c, r] != null)
                    {
                        InitialSelection = SelectionMatrix[c, r];
                        CurrentSelection = InitialSelection;
                        row = r;
                        column = c;
                        break;
                    }
                    // Otherwise, find the row and column of the first selection
                    else if (!SelectionIsInvalid(Item) && SelectionMatrix[c, r] == Item)
                    {
                        CurrentSelection = Item;
                        row = r;
                        column = c;
                        break;
                    }
                }
            }
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
                    if(ScrollSpeed <= 0)
                    {
                        ScrollArea.anchoredPosition = ScrollArea.anchoredPosition + dif + (offset * ScrollBuffer);
                        OnViewChanged();
                    }
                    else
                    {
                        ScrollArea.DOAnchorPos(ScrollArea.anchoredPosition + dif + (offset * ScrollBuffer),
                            1 / ScrollSpeed).OnComplete(() => OnViewChanged());
                    }

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
                // Ignore infinitesimally small values.
                // There exists an edge case due to floating point precision.
                if (difference.sqrMagnitude < 0.0001)
                {
                    difference = Vector2.zero;
                    return true;
                }


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


        #region Row/Column Visibility Checks
        protected virtual bool IsFirstRowVisible()
        {
            for (int i = 0; i < SelectionMatrix.GetLength(0); i++)
            {
                if (RectFullyContainsElement(Viewport, SelectionMatrix[i, 0].transform as RectTransform,
                    out Vector2 dif))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool IsLastRowVisible()
        {
            for (int i = 0; i < SelectionMatrix.GetLength(0); i++)
            {
                if (RectFullyContainsElement(Viewport, 
                    SelectionMatrix[i, SelectionMatrix.GetLength(1) - 1].transform as RectTransform,
                    out Vector2 dif))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool IsFirstColumnVisible()
        {
            for (int i = 0; i < SelectionMatrix.GetLength(1); i++)
            {
                if (RectFullyContainsElement(Viewport, SelectionMatrix[0, i].transform as RectTransform,
                    out Vector2 dif))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool IsLastColumnVisible()
        {
            for (int i = 0; i < SelectionMatrix.GetLength(1); i++)
            {
                if (RectFullyContainsElement(Viewport,
                    SelectionMatrix[SelectionMatrix.GetLength(0) - 1, i].transform as RectTransform,
                    out Vector2 dif))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void OnViewChanged()
        {
            bool[] newScrollDirs = new bool[4];

            newScrollDirs[0] = IsFirstRowVisible();
            newScrollDirs[1] = IsLastRowVisible();
            newScrollDirs[2] = IsFirstColumnVisible();
            newScrollDirs[3] = IsLastColumnVisible();

            for(int i=0; i<4; i++)
            {
                if (scrollDirections[i] != newScrollDirs[i] && scrollIndicators[i] != null)
                {
                    scrollIndicators[i].PlayAnimation(newScrollDirs[i] ? "Hide" : "Show");
                }
            }

            scrollDirections = newScrollDirs;
        }
        #endregion


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


        protected void RegenerateSelectionMatrix(List<UIMenuItem> menuItems, float xRange = 50, float yRange = 50)
        {
            if (CurrentSelection) CurrentSelection.SetSelected(false);
            CurrentSelection = null;
            row = 0; column = 0;

            if(menuItems.Count == 0)
            {
                SelectionMatrix = new UIMenuItem[0, 0];
                return;
            }

            List<float> rowY = new List<float>();
            List<float> colX = new List<float>();
            Dictionary<Vector2, UIMenuItem> tempMatrix = new Dictionary<Vector2, UIMenuItem>();

            for(int i=0; i < menuItems.Count; i++)
            {
                float x = (menuItems[i].transform as RectTransform).anchoredPosition.x;
                float y = (menuItems[i].transform as RectTransform).anchoredPosition.y;

                for(int n = 0; n < rowY.Count; n++)
                {
                    if (Mathf.Abs(rowY[n] - y) <= yRange)
                    {
                        y = rowY[n];
                    }
                }

                for (int n = 0; n < colX.Count; n++)
                {
                    if (Mathf.Abs(colX[n] - x) <= xRange)
                    {
                        x = colX[n];
                    }
                }

                if (!rowY.Contains(y)) rowY.Add(y);
                if (!colX.Contains(x)) colX.Add(x);

                if (tempMatrix.ContainsKey(new Vector2(x, y))) continue;
                tempMatrix.Add(new Vector2(x, y), menuItems[i]);
            }

            rowY.Sort();
            rowY.Reverse();
            colX.Sort();

            SelectionMatrix = new UIMenuItem[colX.Count, rowY.Count];

            for (int y = 0; y < rowY.Count; y++)
            {
                for (int x = 0; x < colX.Count; x++)
                {
                    if(tempMatrix.TryGetValue(new Vector2(colX[x], rowY[y]), out UIMenuItem item))
                    {
                        SelectionMatrix[x, y] = item;

                        if(CurrentSelection == null)
                        {
                            CurrentSelection = item;
                            item.SetSelected(true);
                            row = y;
                            column = x;
                        }
                    }
                }
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

            if (CurrentSelection != null)
                CurrentSelection.OnCancelPressed();
        }

        /// <summary>
        /// Action to execute when Aux1 is pressed
        /// </summary>
        public override void OnAux1Pressed()
        {
            if (!bAllowInput) return;

            if (CurrentSelection != null)
                CurrentSelection.OnAux1Pressed();
        }

        /// <summary>
        /// Action to execute when Aux2 is pressed
        /// </summary>
        public override void OnAux2Pressed()
        {
            if (!bAllowInput) return;

            if (CurrentSelection != null)
                CurrentSelection.OnAux2Pressed();
        }

        #endregion
    }
}
