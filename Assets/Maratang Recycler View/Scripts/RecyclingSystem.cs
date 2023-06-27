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
		private ScrollRect scrollRect;
		private RectTransform baseCell;

		// Data
		private IRecyclerViewDataSource dataSource;

		// Boundary
		private Bounds _recyclingArea; // 리스트 항목을 셀의 형태로 표시하는 범위를 나타내는 사각형. 3D는 Bounds, 2D는 Rect
		private float invisibleRate = 0.2f;
		private float spacingHeight = 4.0f; // 각 셀의 세로간격
		private float spacingWidth = 2.0f; // 각 셀의 가로간격

		// Cell size
		private float _cellWidth, _cellHeight;

		// Cell pool
		private List<RectTransform> _cellPool;
		private List<ICell> _cachedCells;

		// Trackers
		private int currentItemCount;
		private int viewableTopCellIndex, viewableBottomCellIndex;

		public RecyclingSystem(ScrollRect scrollRect,
							   RectTransform baseCell,
							   IRecyclerViewDataSource dataSource,
							   float invisibleRate,
							   float spacingHeight,
							   float spacingWidth)
		{
			this.scrollRect = scrollRect;
			this.baseCell = baseCell;
			this.dataSource = dataSource;
			this.invisibleRate = invisibleRate;
			this.spacingHeight = spacingHeight;
			this.spacingWidth = spacingWidth;
		}

		public void Initialize(Action onInitialized)
		{
			SetAnchorTopMiddle(scrollRect.content);
			scrollRect.content.anchoredPosition = Vector3.zero;
			SetAnchorTopMiddle(baseCell);
			SetRecyclingArea();

			CreateCellPool();
			currentItemCount = _cellPool.Count;
			viewableTopCellIndex = 0;
			viewableBottomCellIndex = _cellPool.Count - 1;

			if (onInitialized != null) onInitialized();
		}

		/// <summary>
		/// 재활용이 이루어지는 공간의 좌표값 설정
		/// </summary>
		private void SetRecyclingArea()
		{
			Vector3[] _corners = new Vector3[4];

			scrollRect.viewport.GetWorldCorners(_corners);
			Debug.Log("viewport corners: " + _corners[0] + ", " + _corners[1] + ", " + _corners[2] + ", " + _corners[3]);

			float invisibleHeight = invisibleRate * (_corners[2].y - _corners[0].y);
			_recyclingArea.min = new Vector3(_corners[0].x, _corners[0].y - invisibleHeight);
			_recyclingArea.max = new Vector3(_corners[2].x, _corners[2].y + invisibleHeight);

			Debug.Log("recycling area position: " + _recyclingArea.min.y + ", " + _recyclingArea.max.y);
			Debug.Log("invisible rate: " + invisibleRate);
			Debug.Log("invisible height" + invisibleHeight);
		}

		/// <summary>
		/// 셀 오브젝트 풀 생성
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

			float currentPoolHeight = 0;
			int currentPoolCount = 0;
			float pointerY = 0;

			_cellWidth = scrollRect.content.rect.width;
			Debug.Log(_cellWidth);
			_cellHeight = _cellWidth * baseCell.sizeDelta.y / baseCell.sizeDelta.x;

			//create cells untill the Pool area is covered and pool size is the minimum required
			while (currentPoolCount < dataSource.GetItemCount() && currentPoolHeight < _recyclingArea.size.y)
			{
				Debug.Log("create cell pool while");
				// 베이스 셀을 이용해 새로운 셀 생성
				RectTransform cell = UnityEngine.Object.Instantiate(baseCell.gameObject)
					.GetComponent<RectTransform>();
				cell.name = "Recycler View Cell";
				cell.sizeDelta = new Vector2(_cellWidth, _cellHeight);
				_cellPool.Add(cell);
				cell.SetParent(scrollRect.content, false);

				// 현재 셀 위치 지정
				cell.anchoredPosition = new Vector2(0, pointerY);
				// 다음 셀 위치로 포인터 이동
				// GUI의 x, y 값은 top-left 좌표값임. 위에서 아래로 증가함
				pointerY = cell.anchoredPosition.y - cell.rect.height;
				currentPoolHeight += cell.rect.height;

				// 셀에 데이터 매핑
				_cachedCells.Add(cell.GetComponent<ICell>());
				dataSource.BindCell(_cachedCells[_cachedCells.Count - 1], currentPoolCount);

				currentPoolCount++;
			}
		}

		/// <summary>
		/// 스크롤뷰의 컨텐트 높이를 갱신하는 함수
		/// </summary>
		/*public void UpdateScrollViewSize()
		{
			// 컨텐트의 높이 계산
			float contentHeight = 0.0f;
			for (int i = 0; i < _itemDataList.Count; i++)
			{
				contentHeight += GetCellHeightAtIndex(i);
				if (i > 0)
				{
					contentHeight += spacingHeight;
				}
			}

			// 패딩 수치를 포함한 컨텐트 높이를 스크롤렉트에 설정
			Vector2 sizeDelta = scrollRect.content.sizeDelta;
			sizeDelta.y = contentPadding.top + contentHeight + contentPadding.bottom;
			scrollRect.content.sizeDelta = sizeDelta;
		}*/

		/// <summary>
		/// VisibleRect 범위에 표시될 만큼의 셀을 생성하여 배치하는 함수
		/// </summary>
		private void FillVisibleRectWithCells()
		{
			// 셀이 없다면 아무 일도 하지 않는다
			/*if (_cachedCells.Count < 1) return;

			// 표시된 마지막 셀에 대응하는 dataList 항목의 다음 항목이 존재하고
			// 또한 그 셀이 visibleRect의 범위에 들어온다면 대응하는 셀을 작성한다
			ICell lastCell = _cachedCells.Last.Value;
			int nextCellDataIndex = lastCell.Index + 1;
			Vector3 nextCellTop = lastCell.Bottom + new Vector3(0.0f, -spacingHeight);

			while (nextCellDataIndex < _itemDataList.Count &&
				   nextCellTop.y >= _recyclingArea.y - _recyclingArea.height)
			{
				ICell cell = CreateCellForIndex(nextCellDataIndex);
				cell.Top = nextCellTop;
			}*/
		}

		/// <summary>
		/// 스크롤뷰가 움직였을 때 호출되는 함수
		/// </summary>
		/// <param name="scrollDirection">스크롤의 x, y 위치</param>
		public void OnScrollPosChanged(int scrollDirection)
		{
			if (_cellPool != null && _cellPool.Count != 0)
			{
				//Update Recycler view boundary since it can change with resolution changes.
				SetRecyclingArea();

				// 셀을 재사용한다
				RecycleCells(scrollDirection);
			}
		}

		/// <summary>
		/// 셀을 재사용하여 표시를 갱신하는 함수
		/// </summary>
		/// <param name="scrollDirection">스크롤 방향. 양수: 아래로, 음수: 위로</param>
		private void RecycleCells(int scrollDirection)
		{
			if (_cachedCells.Count < 1) return;

			// 아래로 스크롤하고 있을 때는 visibleRect에 지정된 범위보다 위에 있는 셀을
			// 아래를 향해 순서대로 이동시켜 내용을 갱신한다
			if (scrollDirection > 0)
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

				// visibleRect에 지정된 범위 안에 빈 곳이 있으면 셀을 작성한다
				FillVisibleRectWithCells();*/
			}
			else if (scrollDirection < 0)
			{
				// 위로 스크롤하고 있을 때는 visibleRect에 지정된 범위보다 아래에 있는 셀을
				// 위를 향해 순서대로 이동시켜 내용을 갱신한다
				/*ICell lastCell = _cachedCells.Last.Value;
				while (lastCell.Top.y < _recyclingArea.y - _recyclingArea.height)
				{
					ICell firstCell = _cachedCells.First.Value;
					BindCellForIndex(lastCell, firstCell.Index - 1);
					lastCell.Bottom = firstCell.Top + new Vector3(0.0f, spacingHeight);

					_cachedCells.AddFirst(lastCell);
					_cachedCells.RemoveLast();
					lastCell = _cachedCells.Last.Value;
				}*/
			}
		}

		/// <summary>
		/// 앵커를 미리 위-중간으로 잡아서 셀 배치시 수고를 덜 수 있음
		/// </summary>
		/// <param name="rectTransform">앵커 설정하려는 ui</param>
		private void SetAnchorTopMiddle(RectTransform rectTransform)
		{
			float tempWidth = rectTransform.rect.width;
			float tempHeight = rectTransform.rect.height;

			rectTransform.anchorMin = new Vector2(0.5f, 1);
			rectTransform.anchorMax = new Vector2(0.5f, 1);
			rectTransform.pivot = new Vector2(0.5f, 1);

			rectTransform.sizeDelta = new Vector2(tempWidth, tempHeight);
		}
	}
}