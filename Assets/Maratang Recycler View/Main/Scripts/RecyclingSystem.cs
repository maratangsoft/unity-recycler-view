using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
		private float additionalCoverage = 0.2f;
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
		private int topCellIndex, bottomCellIndex;
		private int lastPopulatedItem;

		private bool _isRecycling = false;

		public RecyclingSystem(RectTransform recyclerView,
							   ScrollRect scrollRect,
                               RectTransform baseCell,
							   IRecyclerViewDataSource dataSource,
							   float additionalCoverage,
							   float spacingHeight,
							   float spacingWidth)
		{
			this.recyclerView = recyclerView;
			this.scrollRect = scrollRect;
			content = scrollRect.content;
			this.baseCell = baseCell;
			this.dataSource = dataSource;
			this.additionalCoverage = additionalCoverage;
			this.spacingHeight = spacingHeight;
			this.spacingWidth = spacingWidth;
		}

		public void Initialize(Action onInitialized)
		{
			SetAnchorTop(baseCell);
			SetAnchorTop(content);
			scrollRect.content.anchoredPosition = Vector3.zero;

			SetPopulationArea();

			// Cell pool initialization
			CreateCellPool();
			lastPopulatedItem = _cellPool.Count;
			topCellIndex = 0;
			bottomCellIndex = _cellPool.Count - 1;

			// Content initialization
			SetContentSize();

			if (onInitialized != null) onInitialized();

			Vector3[] corners = new Vector3[4];
			recyclerView.GetWorldCorners(corners);
			Debug.Log("recyclerView world position: " + corners[0] + ", " + corners[1] + ", " + corners[2] + ", " + corners[3]);
			Debug.Log("populationArea Min: " + _populationArea.min);
			Debug.Log("populationArea Max: " + _populationArea.max);
		}

		/// <summary>
		/// set the position and size of the population area
		/// </summary>
		private void SetPopulationArea()
		{
			// Get current recycler view world position
			Vector3[] corners = new Vector3[4];
			recyclerView.GetWorldCorners(corners);

			// calculate additional height
			float additionalHeight = recyclerView.sizeDelta.y * additionalCoverage;

			// Position the population area at the same coordinate with Content
			// corners are world position, So this code will make _populationArea to a world position
			// which only used for recycling calculation.
			// min == bottom-left, max == top-right
			_populationArea.min = new Vector2(corners[0].x, corners[0].y - additionalHeight);
			_populationArea.max = new Vector2(corners[2].x, corners[2].y + additionalHeight);

			// Set the size of population area based on the size of Scroll Rect
			/*_populationArea.width = recyclerView.rect.width;
			_populationArea.height = recyclerView.rect.height * populationCoverage;*/
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

            float poolHeight = 0;
            int poolCount = 0;
            float pointerY = 0;

            //create cells untill the Pool area is covered and pool size is the minimum required
            while (poolCount < dataSource.GetItemCount() && poolHeight < _populationArea.height)
			{
				// instantiate cell object
				RectTransform cell = UnityEngine.Object.Instantiate(baseCell.gameObject)
					.GetComponent<RectTransform>();


				cell.sizeDelta = new Vector2(_baseCellWidth, _baseCellHeight);
				cell.SetParent(scrollRect.content, false);

                _cellPool.Add(cell);
				cell.name = "Cell " + (_cellPool.Count - 1);

				// move to proper position
				cell.anchoredPosition = new Vector2(0, pointerY);
				pointerY = cell.anchoredPosition.y - cell.rect.height - spacingHeight;
				poolHeight += cell.rect.height + spacingHeight;

				// store ICell object for later use
				_cachedCells.Add(cell.GetComponent<ICell>());
				dataSource.BindCell(_cachedCells[_cachedCells.Count - 1], poolCount);

				poolCount++;
			}
		}

		/// <summary>
		/// Set content size to the sum of all items
		/// </summary>
		private void SetContentSize()
		{
			int numOfRows = (int)Mathf.Ceil(_cellPool.Count / _numOfColumns);
			float contentHeight = numOfRows * _baseCellHeight + (numOfRows - 1) * spacingHeight;
			content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scrollDirection">positive == upward, negative == downward</param>
		public void OnScrollPosChanged(Vector2 scrollDirection)
		{
			if (_isRecycling || _cellPool == null || _cellPool.Count == 0) return;

			// Update Recycler view boundary since it can change with resolution changes.
			SetPopulationArea();

			// Recycle cells when the last cell appears in population area
			if (scrollDirection.y < 0 && _cellPool[bottomCellIndex].TopY() > _populationArea.yMin)
			{
				RecycleTopToBottom();
			}
			else if (scrollDirection.y > 0 && _cellPool[topCellIndex].BottomY() < _populationArea.yMax)
			{
				RecycleBottomToTop();
			}
		}

		/// <summary>
		/// Recycles cells on the top of the population area and moves to the bottom
		/// </summary>
		private void RecycleTopToBottom()
		{
			_isRecycling = true;

			int recyclingCount = 0;
			float pointerX = 0;
			float pointerY = 0;

			// to determine if content size needs to be updated
			int additionalRows = 0;

			// Recycle while cell at the top is completely go out of population area,
			// and current item count is smaller than datasource.
			while (_cellPool[topCellIndex].BottomY() > _populationArea.yMax &&
				   lastPopulatedItem < dataSource.GetItemCount())
			{
				Debug.Log("top cell TopY: " + _cellPool[topCellIndex].TopY());
				Debug.Log("top cell BottomY: " + _cellPool[topCellIndex].BottomY());

				// Move top cell to bottom
				pointerY = _cellPool[bottomCellIndex].anchoredPosition.y - _cellPool[bottomCellIndex].sizeDelta.y - spacingHeight;
				_cellPool[topCellIndex].anchoredPosition = 
					new Vector2(_cellPool[topCellIndex].anchoredPosition.x, pointerY);

				Debug.Log("posY: " + pointerY);

				// Bind the moved cell with new data
				dataSource.BindCell(_cachedCells[topCellIndex], lastPopulatedItem);

				// Update indices of the cell pool
				bottomCellIndex = topCellIndex;
				topCellIndex = (topCellIndex + 1) % _cellPool.Count;

				recyclingCount++;
				lastPopulatedItem++;
			}

			// Adjust anchor position of the content
			Vector2 yDelta = recyclingCount * Vector2.up * (_cellPool[topCellIndex].sizeDelta.y + spacingHeight);

			_cellPool.ForEach((RectTransform cell) => cell.anchoredPosition += yDelta);
			content.anchoredPosition -= yDelta;
			_isRecycling = false;

			float recycledHeight =
				recyclingCount * _cellPool[topCellIndex].sizeDelta.y + (recyclingCount - 1) * spacingHeight;

			return -new Vector2(0, recycledHeight);
		}

		/// <summary>
		/// Recycles cells on the bottom of the population area and moves to the top
		/// </summary>
		private Vector2 RecycleBottomToTop()
		{
			_isRecycling = true;

			int recyclingCount = 0;
			float pointerX = 0;
			float pointerY = 0;

			// to determine if content size needs to be updated
			int additionalRows = 0;

			// Recycle while cell at the bottom is completely go out of population area,
			// and current item count is greater than cell pool.
			while (_cellPool[bottomCellIndex].TopY() < _populationArea.yMin &&
				   lastPopulatedItem > _cellPool.Count)
			{
				Debug.Log("bottom cell TopY: " + _cellPool[bottomCellIndex].TopY());
				Debug.Log("bottom cell BottomY: " + _cellPool[bottomCellIndex].BottomY());

				// Move the bottom cell to the top
				pointerY = _cellPool[topCellIndex].anchoredPosition.y + _cellPool[topCellIndex].sizeDelta.y + spacingHeight;
				_cellPool[bottomCellIndex].anchoredPosition =
					new Vector2(_cellPool[bottomCellIndex].anchoredPosition.x, pointerY);

				recyclingCount++;
				lastPopulatedItem--;

				// Bind the moved cell with new data
				dataSource.BindCell(_cachedCells[bottomCellIndex], lastPopulatedItem - _cellPool.Count);

				// Update indices of the cell pool
				topCellIndex = bottomCellIndex;
				bottomCellIndex = (bottomCellIndex + _cellPool.Count - 1) % _cellPool.Count;
			}

			// Adjust anchor position of the content
			Vector2 yDelta = recyclingCount * Vector2.up * (_cellPool[topCellIndex].sizeDelta.y + spacingHeight);

			_cellPool.ForEach((RectTransform cell) => cell.anchoredPosition -= yDelta);
			content.anchoredPosition += yDelta;
			_isRecycling = false;

			float recycledHeight = 
				recyclingCount * _cellPool[topCellIndex].sizeDelta.y + (recyclingCount - 1) * spacingHeight;

			return new Vector2(0, recycledHeight);
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