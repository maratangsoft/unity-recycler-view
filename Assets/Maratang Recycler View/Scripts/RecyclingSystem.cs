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
		private Bounds _recyclingArea; // ����Ʈ �׸��� ���� ���·� ǥ���ϴ� ������ ��Ÿ���� �簢��. 3D�� Bounds, 2D�� Rect
		private float invisibleRate = 0.2f;
		private float spacingHeight = 4.0f; // �� ���� ���ΰ���
		private float spacingWidth = 2.0f; // �� ���� ���ΰ���

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
		/// ��Ȱ���� �̷������ ������ ��ǥ�� ����
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
		/// �� ������Ʈ Ǯ ����
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
				// ���̽� ���� �̿��� ���ο� �� ����
				RectTransform cell = UnityEngine.Object.Instantiate(baseCell.gameObject)
					.GetComponent<RectTransform>();
				cell.name = "Recycler View Cell";
				cell.sizeDelta = new Vector2(_cellWidth, _cellHeight);
				_cellPool.Add(cell);
				cell.SetParent(scrollRect.content, false);

				// ���� �� ��ġ ����
				cell.anchoredPosition = new Vector2(0, pointerY);
				// ���� �� ��ġ�� ������ �̵�
				// GUI�� x, y ���� top-left ��ǥ����. ������ �Ʒ��� ������
				pointerY = cell.anchoredPosition.y - cell.rect.height;
				currentPoolHeight += cell.rect.height;

				// ���� ������ ����
				_cachedCells.Add(cell.GetComponent<ICell>());
				dataSource.BindCell(_cachedCells[_cachedCells.Count - 1], currentPoolCount);

				currentPoolCount++;
			}
		}

		/// <summary>
		/// ��ũ�Ѻ��� ����Ʈ ���̸� �����ϴ� �Լ�
		/// </summary>
		/*public void UpdateScrollViewSize()
		{
			// ����Ʈ�� ���� ���
			float contentHeight = 0.0f;
			for (int i = 0; i < _itemDataList.Count; i++)
			{
				contentHeight += GetCellHeightAtIndex(i);
				if (i > 0)
				{
					contentHeight += spacingHeight;
				}
			}

			// �е� ��ġ�� ������ ����Ʈ ���̸� ��ũ�ѷ�Ʈ�� ����
			Vector2 sizeDelta = scrollRect.content.sizeDelta;
			sizeDelta.y = contentPadding.top + contentHeight + contentPadding.bottom;
			scrollRect.content.sizeDelta = sizeDelta;
		}*/

		/// <summary>
		/// VisibleRect ������ ǥ�õ� ��ŭ�� ���� �����Ͽ� ��ġ�ϴ� �Լ�
		/// </summary>
		private void FillVisibleRectWithCells()
		{
			// ���� ���ٸ� �ƹ� �ϵ� ���� �ʴ´�
			/*if (_cachedCells.Count < 1) return;

			// ǥ�õ� ������ ���� �����ϴ� dataList �׸��� ���� �׸��� �����ϰ�
			// ���� �� ���� visibleRect�� ������ ���´ٸ� �����ϴ� ���� �ۼ��Ѵ�
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
		/// ��ũ�Ѻ䰡 �������� �� ȣ��Ǵ� �Լ�
		/// </summary>
		/// <param name="scrollDirection">��ũ���� x, y ��ġ</param>
		public void OnScrollPosChanged(int scrollDirection)
		{
			if (_cellPool != null && _cellPool.Count != 0)
			{
				//Update Recycler view boundary since it can change with resolution changes.
				SetRecyclingArea();

				// ���� �����Ѵ�
				RecycleCells(scrollDirection);
			}
		}

		/// <summary>
		/// ���� �����Ͽ� ǥ�ø� �����ϴ� �Լ�
		/// </summary>
		/// <param name="scrollDirection">��ũ�� ����. ���: �Ʒ���, ����: ����</param>
		private void RecycleCells(int scrollDirection)
		{
			if (_cachedCells.Count < 1) return;

			// �Ʒ��� ��ũ���ϰ� ���� ���� visibleRect�� ������ �������� ���� �ִ� ����
			// �Ʒ��� ���� ������� �̵����� ������ �����Ѵ�
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

				// visibleRect�� ������ ���� �ȿ� �� ���� ������ ���� �ۼ��Ѵ�
				FillVisibleRectWithCells();*/
			}
			else if (scrollDirection < 0)
			{
				// ���� ��ũ���ϰ� ���� ���� visibleRect�� ������ �������� �Ʒ��� �ִ� ����
				// ���� ���� ������� �̵����� ������ �����Ѵ�
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
		/// ��Ŀ�� �̸� ��-�߰����� ��Ƽ� �� ��ġ�� ���� �� �� ����
		/// </summary>
		/// <param name="rectTransform">��Ŀ �����Ϸ��� ui</param>
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