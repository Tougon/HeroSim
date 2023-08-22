using DG.Tweening;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;

namespace ToUI
{
    public class UIDynamicMenu : UIMenu
    {
        [Header("Dynamic Menu Properties")]
        [SerializeField] protected RectTransform Grid;
        [SerializeField] protected UIMenuItem DynamicItem;
        [SerializeField] private Vector2 ItemSize = new Vector2(100, 100);
        [SerializeField] private Vector2Int GridSize = new Vector2Int(5, 5);
        [SerializeField] private Vector2 GridSpacing = new Vector2(0, 0);
        [SerializeField] private RectOffset Padding;
        [SerializeField] private TextAnchor ChildAlignment;
        [SerializeField] private GridLayoutGroup.Corner StartCorner;
        [SerializeField] private GridLayoutGroup.Axis StartAxis;

        protected int ItemsToDisplay;
        protected int[,] VirtualSelectionMatrix;
        protected List<UIMenuItem> DynamicItems = new List<UIMenuItem>();

        // Virtual column and row used to scroll the grid
        protected int vColumn;
        protected int vRow;

        // Cached Y (or X) offsets used for setting and resetting position;
        private float offsetInitial;
        private float offset;
        private int cachedRowDelta;
        private int cachedColumnDelta;
        private List<List<UIMenuItem>> GroupPool;

        private string data = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";


        protected override void Start()
        {
            if(DynamicItem != null && GridSize.x > 0 && GridSize.y > 0)
            {
                Fallback = StartAxis == GridLayoutGroup.Axis.Horizontal ? 
                    SelectionFallbackType.SameRow : SelectionFallbackType.SameColumn;

                // This is testing nonsense, aight?
                SetMaxItems(52);
                // End testing

                CalculateGridBounds();
                SpawnMenuItems();
                RefreshAllData();
            }

            base.Start();

            // More testing nonsense
            ScrollToTarget();
            // End testing
        }


        /// <summary>
        /// Calculates the bounds of the scrollable area
        /// </summary>
        protected virtual void CalculateGridBounds()
        {
            float width = (ItemSize.x * GridSize.x) + 
                (GridSpacing.x * (GridSize.x - 1)) + 
                Padding.left + Padding.right;

            float height = (ItemSize.y * GridSize.y) +
                (GridSpacing.y * (GridSize.y - 1)) +
                Padding.top + Padding.bottom;

            Grid.sizeDelta = new Vector2(width, height);
            Grid.anchoredPosition = new Vector2(width / 2, -height / 2);
        }


        /// <summary>
        /// Sets the maximum number of visible items to the given amount and recalculates bounds
        /// </summary>
        public void SetMaxItems(int itemsToDisplay)
        {
            ItemsToDisplay = itemsToDisplay;

            int columns = StartAxis == GridLayoutGroup.Axis.Vertical ?
                Mathf.CeilToInt((float)itemsToDisplay / GridSize.y) :
                Mathf.Min(itemsToDisplay, GridSize.x);
            int rows = StartAxis == GridLayoutGroup.Axis.Vertical ?
                Mathf.Min(itemsToDisplay, GridSize.y) : 
                Mathf.CeilToInt((float)itemsToDisplay / GridSize.x);

            float width = (ItemSize.x * columns) +
                (GridSpacing.x * (columns - 1)) +
                Padding.left + Padding.right;

            float height = (ItemSize.y * rows) +
                (GridSpacing.y * (rows - 1)) +
                Padding.top + Padding.bottom;

            ScrollArea.sizeDelta = new Vector2(width, height);

            // Populate the virtual matrix
            VirtualSelectionMatrix = new int[columns, rows];

            int offset = StartAxis == GridLayoutGroup.Axis.Horizontal ? columns : rows;
            int primary = 0; int secondary = 0;

            for (int i = 0; i < rows * columns; i++)
            {
                if(StartAxis == GridLayoutGroup.Axis.Horizontal)
                {
                    VirtualSelectionMatrix[primary, secondary] =
                        i < ItemsToDisplay ? i : -1;
                }
                else
                {
                    VirtualSelectionMatrix[secondary, primary] =
                        i < ItemsToDisplay ? i : -1;
                }

                primary++;

                if(primary >= offset)
                {
                    secondary++;
                    primary = 0;
                }
            }
        }


        /// <summary>
        /// Spawns all menu items
        /// </summary>
        protected virtual void SpawnMenuItems()
        {
            // Add a grid layout. This make sure the padding, etc. is respected without rewriting the whole system.
            GridLayoutGroup grid = Grid.gameObject.AddComponent<GridLayoutGroup>();

            grid.padding = Padding;
            grid.cellSize = ItemSize;
            grid.spacing = GridSpacing;
            grid.startCorner = StartCorner;
            grid.startAxis = StartAxis;
            grid.childAlignment = ChildAlignment;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            // Initialize selection matrix
            SelectionMatrix = new UIMenuItem[GridSize.x, GridSize.y];

            int selV = StartAxis == GridLayoutGroup.Axis.Horizontal ? GridSize.y : GridSize.x;
            int selH = StartAxis == GridLayoutGroup.Axis.Horizontal ? GridSize.x : GridSize.y;

            GroupPool = new List<List<UIMenuItem>>(selV);

            for (int i=0; i<selV; i++)
            {
                List<UIMenuItem> ItemGroup = new List<UIMenuItem>(selH);

                for(int j=0; j<selH; j++)
                {
                    var menuItem = InstantiateMenuItem();

                    menuItem.name = $"{j},{i}";

                    if (StartAxis == GridLayoutGroup.Axis.Horizontal)
                        SelectionMatrix[j, i] = menuItem;
                    else
                        SelectionMatrix[i, j] = menuItem;

                    DynamicItems.Add(menuItem);
                    ItemGroup.Add(menuItem);

                    if (InitialSelection == null)
                        InitialSelection = menuItem;
                }

                GroupPool.Add(ItemGroup);
            }

            // Force rebuild the layout. If we destroy the grid before doing this it will not work.
            LayoutRebuilder.ForceRebuildLayoutImmediate(Grid);

            if(DynamicItems.Count > 0)
            {
                offsetInitial = StartAxis == GridLayoutGroup.Axis.Horizontal ?
                    (DynamicItems[0].transform as RectTransform).anchoredPosition.y :
                    (DynamicItems[0].transform as RectTransform).anchoredPosition.x;

                if (selV > 0)
                {
                    float o = StartAxis == GridLayoutGroup.Axis.Horizontal ?
                        (DynamicItems[selH].transform as RectTransform).anchoredPosition.y :
                        (DynamicItems[selH].transform as RectTransform).anchoredPosition.x;

                    offset = StartAxis == GridLayoutGroup.Axis.Horizontal ?
                        offsetInitial - o : o - offsetInitial;
                }
            }

            // Remove the grid, as it will interfere heavily with scrolling.
            Destroy(grid);
        }


        protected UIMenuItem InstantiateMenuItem()
        {
            var Item = Instantiate(DynamicItem, Grid);

            // In for testing. TODO: Remove
            Item.gameObject.SetActive(true);
            // End Testing
            (Item.transform as RectTransform).sizeDelta = ItemSize;

            return Item;
        }

        
        protected override void ChangeSelection(Vector2 Direction)
        {
            // Initialize selection variables
            int targetRow = vRow;
            int targetColumn = vColumn;

            targetRow -= Mathf.RoundToInt(Direction.y);
            targetColumn += Mathf.RoundToInt(Direction.x);

            // Wrap around the target row
            if (targetRow >= VirtualSelectionMatrix.GetLength(1))
            {
                if (WrapSelection) targetRow = 0;
                else targetRow = VirtualSelectionMatrix.GetLength(1);
            }

            if (targetRow < 0)
            {
                if (WrapSelection) targetRow = VirtualSelectionMatrix.GetLength(1) - 1;
                else targetRow = 0;
            }

            // Wrap around the target column
            if (targetColumn >= VirtualSelectionMatrix.GetLength(0))
            {
                if (WrapSelection) targetColumn = 0;
                else targetColumn = VirtualSelectionMatrix.GetLength(0) - 1;
            }

            if (targetColumn < 0)
            {
                if (WrapSelection) targetColumn = VirtualSelectionMatrix.GetLength(0) - 1;
                else targetColumn = 0;
            }

            cachedRowDelta = targetRow - vRow;
            cachedColumnDelta = targetColumn - vColumn;

            vRow = targetRow;
            vColumn = targetColumn;

            base.ChangeSelection(Direction);

            // This corrects an error where if a row/column is not entirely filled,
            // it can desync the virtual selection.
            if (StartAxis == GridLayoutGroup.Axis.Horizontal && vColumn != column)
            {
                vColumn = column;
            }
            if (StartAxis == GridLayoutGroup.Axis.Vertical && vRow != row)
            {
                vRow = row;
            }
        }


        protected override Vector2 ScrollToTarget()
        {
            var dif = base.ScrollToTarget();
            bool bWarped = false;

            if (dif.sqrMagnitude > 0 && 
                ((StartAxis == GridLayoutGroup.Axis.Horizontal && dif.y != 0) ||
                (StartAxis == GridLayoutGroup.Axis.Vertical && dif.x != 0)))
            {
                int delta = StartAxis == GridLayoutGroup.Axis.Horizontal ?
                    cachedRowDelta : cachedColumnDelta;
                int direction = delta / Mathf.Abs(delta);

                if(Mathf.Abs(delta) > 1 && 
                    ((StartAxis == GridLayoutGroup.Axis.Horizontal && Mathf.Abs(delta) > GridSize.y) ||
                    (StartAxis == GridLayoutGroup.Axis.Vertical && Mathf.Abs(delta) > GridSize.x)))
                {
                    if (delta > 0) WarpToEnd(StartAxis == GridLayoutGroup.Axis.Horizontal);
                    else WarpToStart(StartAxis == GridLayoutGroup.Axis.Horizontal);
                    bWarped = true;
                }

                // Only scroll if we reach the end, horizontally or vertically
                bool bScroll = (direction > 0 && (GroupPool[GroupPool.Count - 1].Contains(CurrentSelection) ||
                    (GroupPool.Count > 1 && GroupPool[GroupPool.Count - 2].Contains(CurrentSelection)))) ||
                    (direction < 0 && (GroupPool[0].Contains(CurrentSelection) ||
                    (GroupPool.Count > 1 && GroupPool[1].Contains(CurrentSelection))));

                // Check if nearing the ends of the list vertically
                if (StartAxis == GridLayoutGroup.Axis.Horizontal)
                {
                    // Check if at the end of the list vertically
                    bScroll &= Mathf.Abs(delta) <= 1 && vRow != VirtualSelectionMatrix.GetLength(1) - 1 &&
                        (GroupPool.Count > 1 && vRow != VirtualSelectionMatrix.GetLength(1) - 2);
                    bScroll &= Mathf.Abs(delta) <= 1 && vRow != 0 && (GroupPool.Count > 1 && vRow != 1);
                }
                else
                {
                    // Check if at the end of the list horizontally
                    bScroll &= Mathf.Abs(delta) <= 1 && vColumn != VirtualSelectionMatrix.GetLength(0) - 1 &&
                        (GroupPool.Count > 1 && vColumn != VirtualSelectionMatrix.GetLength(0) - 2);
                    bScroll &= Mathf.Abs(delta) <= 1 && vColumn != 0 && (GroupPool.Count > 1 && vColumn != 1);
                }

                if (bScroll)
                {
                    int amount = GroupPool.Count > 1 ? 2 : 1;

                    // Pull the topmost group from the pool
                    var group = delta > 0 ? GroupPool[0] : GroupPool[GroupPool.Count - 1];

                    if (delta > 0) GroupPool.RemoveAt(0);
                    else GroupPool.RemoveAt(GroupPool.Count - 1);

                    var last = delta > 0 ?
                        GroupPool[GroupPool.Count - 1] : GroupPool[0];
                    Vector2 referenceOffset = (last[0].transform as RectTransform).anchoredPosition;

                    for (int j = 0; j < group.Count; j++)
                    {
                        var item = group[j];
                        var rect = (item.transform as RectTransform);
                        rect.anchoredPosition = StartAxis == GridLayoutGroup.Axis.Horizontal ?
                            new Vector2(rect.anchoredPosition.x, referenceOffset.y) + (Vector2.down * offset * direction) :
                            new Vector2(referenceOffset.x, rect.anchoredPosition.y) + (Vector2.right * offset * direction);

                        // Refresh the element since it now displays different data
                        if (StartAxis == GridLayoutGroup.Axis.Horizontal)
                            RefreshData(item, VirtualSelectionMatrix[j, vRow + (amount * direction)]);
                        else
                            RefreshData(item, VirtualSelectionMatrix[vColumn + (amount * direction), j]);
                    }

                    // Add this group back into the pool
                    // End of the pool if going down, front if going up
                    if (delta > 0) GroupPool.Add(group);
                    else GroupPool.Insert(0, group);
                }
            }

            // Move the grid if we didn't warp
            if (!bWarped)
            {
                Grid.DOKill();
                Grid.DOAnchorPos(Grid.anchoredPosition + dif, 1 / ScrollSpeed);
            }

            //RefreshData();

            return dif;
        }


        /// <summary>
        /// Warps list to the beginning
        /// </summary>
        protected virtual void WarpToStart(bool horizontal)
        {
            int amountToMove = horizontal ? (cachedRowDelta + GridSize.y - 1) : cachedColumnDelta - GridSize.x;

            float distancePerItem = horizontal ? ItemSize.y + GridSpacing.y :
                ItemSize.x + GridSpacing.x;

            float delta = amountToMove * distancePerItem;

            for (int i = 0; i < GroupPool.Count; i++)
            {
                for (int j = 0; j < GroupPool[i].Count; j++)
                {
                    var rect = (GroupPool[i][j].transform as RectTransform);
                    rect.anchoredPosition = StartAxis == GridLayoutGroup.Axis.Horizontal ?
                        new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - delta) :
                        new Vector2(rect.anchoredPosition.x + delta, rect.anchoredPosition.y);

                    // Refresh the element sense it now displays different data
                    if (horizontal)
                        RefreshData(GroupPool[i][j], VirtualSelectionMatrix[j, i]);
                    else
                        RefreshData(GroupPool[i][j], VirtualSelectionMatrix[i, j]);
                }
            }

            var dif = base.ScrollToTarget();
            Grid.DOKill();
            Grid.DOAnchorPos(Grid.anchoredPosition + dif, 1 / ScrollSpeed);
        }


        /// <summary>
        /// Warps list to the end
        /// </summary>
        protected virtual void WarpToEnd(bool horizontal)
        {
            int amountToMove = horizontal ? (cachedRowDelta - GridSize.y + 1) : 
                (cachedColumnDelta - GridSize.x + 1);
            float distancePerItem = horizontal ? ItemSize.y + GridSpacing.y : 
                ItemSize.x + GridSpacing.x;

            float delta = amountToMove * distancePerItem;

            for (int i = 0; i < GroupPool.Count; i++)
            {
                for(int j=0; j < GroupPool[i].Count; j++)
                {
                    var rect = (GroupPool[i][j].transform as RectTransform);
                    rect.anchoredPosition = StartAxis == GridLayoutGroup.Axis.Horizontal ?
                        new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - delta) :
                        new Vector2(rect.anchoredPosition.x + delta, rect.anchoredPosition.y);

                    // Refresh the element since it now displays different data
                    if (horizontal)
                        RefreshData(GroupPool[i][j], VirtualSelectionMatrix[j, amountToMove + i]);
                    else
                        RefreshData(GroupPool[i][j], VirtualSelectionMatrix[amountToMove + i, j]);
                }
            }

            // Handle fallbacks
            while (Fallback == SelectionFallbackType.SameRow && !CurrentSelection.gameObject.activeInHierarchy)
            {
                column -= 1;
                CurrentSelection.SetSelected(false);
                CurrentSelection = SelectionMatrix[column, row];
                CurrentSelection.SetSelected(true);

                if (column <= 0)
                    break;
            }

            while (Fallback == SelectionFallbackType.SameColumn && !CurrentSelection.gameObject.activeInHierarchy)
            {
                row -= 1;
                CurrentSelection.SetSelected(false);
                CurrentSelection = SelectionMatrix[column, row];
                CurrentSelection.SetSelected(true);

                if (row <= 0)
                    break;
            }

            var dif = base.ScrollToTarget();
            Grid.DOKill();
            Grid.DOAnchorPos(Grid.anchoredPosition + dif, 1 / ScrollSpeed);
        }

        /// <summary>
        /// Refreshes all data in the list based on the virtual selection matrix
        /// </summary>
        protected virtual void RefreshAllData()
        {
            for(int i=0; i<GroupPool.Count; i++)
            {
                for(int j=0; j < GroupPool[i].Count; j++)
                {
                    if ((StartAxis == GridLayoutGroup.Axis.Horizontal && (VirtualSelectionMatrix.GetLength(0) <= j ||
                        VirtualSelectionMatrix.GetLength(1) <= i)) || (StartAxis == GridLayoutGroup.Axis.Vertical &&
                        (VirtualSelectionMatrix.GetLength(0) <= i || VirtualSelectionMatrix.GetLength(1) <= j)))
                    {
                        RefreshData(GroupPool[i][j], -1);
                    }
                    else
                    {
                        int index = StartAxis == GridLayoutGroup.Axis.Horizontal ? 
                            VirtualSelectionMatrix[j, i] : VirtualSelectionMatrix[i, j];

                        RefreshData(GroupPool[i][j], index);
                    }
                }
            }
        }


        /// <summary>
        /// Refreshes the data for the given menu item, using the data at the given index
        /// Returns true if the object is active
        /// </summary>
        protected virtual bool RefreshData(UIMenuItem Item, int index)
        {
            Item.gameObject.SetActive(index >= 0);
                
            // Stinky test code aaaa
            if(index >= 0)
                Item.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "" + data.ToCharArray()[index];
            // End test

            return index >= 0;
        }
    }
}
