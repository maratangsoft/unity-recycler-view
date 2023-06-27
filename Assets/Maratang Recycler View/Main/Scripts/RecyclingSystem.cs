using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Maratangsoft.RecyclerView
{
	public class RecyclingSystem
	{
		// UI components
		private RectTransform recyclerView;
		private ScrollRect scrollRect;
		private RectTransform content;

		// Boundary
		private Rect _populationArea; // boundaries of scrollable area. Bounds for 3D, Rect for 2D
		private float populationCoverage = 0.2f;
		private float spacingHeight = 4.0f; // space between each cell on Y axis
		private float spacingWidth = 2.0f; // space between each cell on X axis
		private readonly int _numOfColumns = 1;

        // Cell
        private RectTransform baseCell;
        private IRecyclerViewDataSource dataSource;
        private float _baseCellWidth, _baseCellHeight;

		// Cell pool
		private List<RectTransform> _cellPool;
		private List<ICell> _cachedCells;

		// Trackers
		private int currentItemCount;
		private int populatedTopCellIndex, populatedBottomCellIndex;

		public RecyclingSystem(RectTransform recyclerView,
							   ScrollRect scrollRect,
							   RectTransform content,
                               RectTransform baseCell,
							   IRecyclerViewDataSource dataSource,
							   float populationCoverage,
							   float spacingHeight,
							   float spacingWidth)
		{
			this.recyclerView = recyclerView;
			this.scrollRect = scrollRect;
			this.content = content;
			this.baseCell = baseCell;
			this.dataSource = dataSource;
			this.populationCoverage = populationCoverage;
			this.spacingHeight = spacingHeight;
			this.spacingWidth = spacingWidth;
		}

		public void Initialize(Action onInitialized)
		{
			SetAnchorTop(content);
			scrollRect.content.anchoredPosition = Vector3.zero;
			SetAnchorTop(baseCell);
			SetPopulationArea();

			CreateCellPool();
			currentItemCount = _cellPool.Count;
			populatedTopCellIndex = 0;
			populatedBottomCellIndex = _cellPool.Count - 1;

			int numOfRows = 
				(int)Mathf.Ceil((float)_cellPool.Count / (float)_numOfColumns);
			float contentHeight = 
				numOfRows * _baseCellHeight + (numOfRows - 1) * spacingHeight;
			content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);

			if (onInitialized != null) onInitialized();
		}

		/// <summary>
		/// 
		/// </summary>
		private void SetPopulationArea()
		{
			_populationArea.x = content.anchoredPosition.x;
			_populationArea.y = -content.anchoredPosition.y;

			_populationArea.width = recyclerView.rect.width;
			_populationArea.height = recyclerView.rect.height * populationCoverage;
		}

		/// <summary>
		/// creates cell pool and instantiate cells
		/// </summary>
		private void CreateCellPool()
		{
			// reset existing pool
			if (_cellPool != null)
			{
				_cellPool.ForEach((RectTransform cell) => UnityEngine.Object.Destroy(cell.gameObject));
				_cellPool.Clear();
				_cachedCells.Clear();
			}
			else
			{
				_cellPool = new List<RectTransform>();
				_cachedCells = new List<ICell>();
			}

			_baseCellWidth = baseCell.sizeDelta.x;
			_baseCellHeight = baseCell.sizeDelta.y;
            Debug.Log("baseCell width: " + _baseCellWidth + ", height: " + _baseCellHeight);

            float currentPoolHeight = 0;
            int currentPoolCount = 0;
            float pointerY = 0;

            //create cells untill the Pool area is covered and pool size is the minimum required
            while (currentPoolCount < dataSource.GetItemCount() && 
				   currentPoolHeight < _populationArea.height)
			{
				// instantiate cell object
				RectTransform cell = UnityEngine.Object.Instantiate(baseCell.gameObject)
					.GetComponent<RectTransform>();

				cell.name = "Cell";
				cell.sizeDelta = new Vector2(_baseCellWidth, _baseCellHeight);
				cell.SetParent(scrollRect.content, false);

                _cellPool.Add(cell);

                // move to proper position
                cell.anchoredPosition = new Vector2(0, pointerY);
				// In GUI classes, the x, y position start from top-left edge of the rect.
				// When the y value increases, y position go to downward.
				pointerY = cell.anchoredPosition.y - cell.rect.height - spacingHeight;
				currentPoolHeight += cell.rect.height + spacingHeight;

				// store ICell object for later use
				_cachedCells.Add(cell.GetComponent<ICell>());
				dataSource.BindCell(_cachedCells[_cachedCells.Count - 1], currentPoolCount);

				currentPoolCount++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scrollDirection">positive == downward, negative == upward</param>
		public void OnScrollPosChanged(int scrollDirection)
		{
			if (_cellPool != null && _cellPool.Count != 0)
			{
				// Update Recycler view boundary since it can change with resolution changes.
				SetPopulationArea();

				if (scrollDirection > 0)
				{
                    RecycleDownward();
                }
				else if (scrollDirection < 0)
				{
					RecycleUpward();
				}
				
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scrollDirection"></param>
		private void RecycleDownward()
		{
            

            /*ICell firstCell = _cachedCells.First.Value;
				while (firstCell.Bottom.y > _recyclingArea.y)
				{
					ICell lastCell = _cachedCells.Last.Value;
					BindCellForIndex(firstCell, lastCell.Index + 1);
					firstCell.Top = lastCell.Bottom + new Vector3(0.0f, -spacingHeight);

					_cachedCells.AddLast(firstCell);
					_cachedCells.RemoveFirst();
					firstCell = _cachedCells.First.Value;
				}
				FillVisibleRectWithCells();*/
        }

        private void RecycleUpward()
		{
            

            /*ICell lastCell = _cachedCells.Last.Value;
            while (lastCell.Top.y < _recyclingArea.y - _recyclingArea.height)
            {
                ICell firstCell = _cachedCells.First.Value;
                BindCellForIndex(lastCell, firstCell.Index - 1);
                lastCell.Bottom = firstCell.Top + new Vector3(0.0f, spacingHeight);

                _cachedCells.AddFirst(lastCell);
                _cachedCells.RemoveLast();
                lastCell = _cachedCells.Last.Value;
            }
			FillVisibleRectWithCells();*/
        }

        /// <summary>
        /// fix the anchor of gameObjects to make position easily
        /// </summary>
        /// <param name="rectTransform"></param>
        private void SetAnchorTop(RectTransform rectTransform)
		{
			float tempWidth = rectTransform.rect.width;
			float tempHeight = rectTransform.rect.height;

			rectTransform.anchorMin = new Vector2(0, 1);
			rectTransform.anchorMax = new Vector2(1, 1);
			rectTransform.pivot = new Vector2(0, 1);

			rectTransform.sizeDelta = new Vector2(tempWidth, tempHeight);
			Debug.Log("sizeDelta: " + rectTransform.sizeDelta.x + ", " + rectTransform.sizeDelta.y);
		}
	}
}