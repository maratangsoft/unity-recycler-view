using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace Maratangsoft.RecyclerView
{
	[RequireComponent(typeof(ScrollRect))]
	[RequireComponent (typeof(RectTransform))]
	public class RecyclerViewController<T> : MonoBehaviour
	{
		private const int SCROLL_DOWNWARD = 1;
		private const int SCROLL_UPWARD = -1;

		protected List<T> _itemDataList = new List<T>(); // ����Ʈ �׸��� �����͸� ����
		[SerializeField] 
		protected GameObject cellBase; // ��� ���� ���̽��� �� ��
		[SerializeField] 
		private RectOffset padding; // ��ũ���� ������ �е�
		[SerializeField]
		private float spacingHeight = 4.0f; // �� ���� ���ΰ���
		[SerializeField]
		private float spacingWidth = 2.0f; // �� ���� ���ΰ���
		[SerializeField]
		private RectOffset visibleRectPadding; // visibleRect�� �е�

		private LinkedList<RecyclerViewCell<T>> _cells = new LinkedList<RecyclerViewCell<T>>(); // �� ���� ����Ʈ

		private Rect visibleRect; // ����Ʈ �׸��� ���� ���·� ǥ���ϴ� ������ ��Ÿ���� �簢��

		private Vector2 prevScrollPos; // �ٷ� ���� ��ũ�� ��ġ�� ����

		// 
		public RectTransform CachedRectTransform => GetComponent<RectTransform>();
		public ScrollRect CachedScrollRect => GetComponent<ScrollRect>();

		protected virtual void Start()
		{
			// ���̽� ���� ��Ȱ��ȭ
			cellBase.SetActive(false);
			// Scroll Rect ������Ʈ�� On Value Changed �̺�Ʈ�� �̺�Ʈ �����ʸ� �����Ѵ�
			CachedScrollRect.onValueChanged.AddListener(OnScrollPosChanged);
		}

		/// <summary>
		/// ���̺�並 �ʱ�ȭ�ϴ� �Լ�
		/// </summary>
		protected void InitializeTableView()
		{
			UpdateScrollViewSize();
			UpdateVisibleRect();
			
			// ���� �ϳ��� ���� ���� visibleRect�� ������ ���� ù ��° ����Ʈ �׸��� ã�Ƽ�
			// �׿� �����ϴ� ���� �ۼ��Ѵ�
			if (_cells.Count < 1)
			{
				Vector2 cellTop = new Vector2(0.0f, -padding.top);
				for (int i = 0; i < _itemDataList.Count; i++)
				{
					float cellHeight = GetCellHeightAtIndex(i);
					Vector2 cellBottom = cellTop + new Vector2(0.0f, -cellHeight);

					// GUI Ŭ������ x, y ���� ���� �� �𼭸� ��ǥ��, ������ �Ʒ��� ������
					// �� ����� visibleRect ��ܺ��� ���̰� �ϴܺ��� �Ʒ��̰ų�
					// �� �ϴ��� visibleRect ��ܺ��� ���̰� �ϴܺ��� �Ʒ��� ��� (== ù ��)
					// �����Ϸ��� ���� ��� ��ġ�� visibleRect ����⿡�� padding.top��ŭ ������ ��ġ�� ����
					if ((cellTop.y <= visibleRect.y && cellTop.y >= visibleRect.y - visibleRect.height) ||
						(cellBottom.y <= visibleRect.y && cellBottom.y >= visibleRect.y - visibleRect.height))
					{
						RecyclerViewCell<T> cell = CreateCellForIndex(i);
						cell.Top = cellTop;
						break;
					}
					// �� ��ܰ� �ϴ� ��� visibleRect ���� �ȿ� ���� ��� (== �ι�° �� ����)
					// �����Ϸ��� ���� ��� ��ġ�� ���� �ٿ��� spacingHeight��ŭ ������ ��ġ�� ����
					cellTop = cellBottom + new Vector2(0.0f, spacingHeight);
				}
				// visibleRect�� ������ �� ���� ������ ���� �ۼ��Ѵ�
				FillVisibleRectWithCells();
			}
			else
			{
				// �̹� ���� ���� ���� ù ��° ������ ������� �����ϴ� ����Ʈ �׸��� �ε����� �ٽ� �����ϰ�
				// ��ġ�� ������ �����Ѵ�
				LinkedListNode<RecyclerViewCell<T>> node = _cells.First;
				BindCellForIndex(node.Value, node.Value.Index);
				node = node.Next;

				while (node != null)
				{
					BindCellForIndex(node.Value, node.Value.Index + 1);
					node.Value.Top = node.Previous.Value.Bottom + new Vector2(0.0f, -spacingHeight);
					node = node.Next;
				}

				// visibleRect�� ������ �� ���� ������ ���� �ۼ��Ѵ�
				FillVisibleRectWithCells();
			}
		}

		/// <summary>
		/// ��ũ�Ѻ��� ����Ʈ ���̸� �����ϴ� �Լ�
		/// </summary>
		protected void UpdateScrollViewSize()
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
			Vector2 sizeDelta = CachedScrollRect.content.sizeDelta;
			sizeDelta.y = padding.top + contentHeight + padding.bottom;
			CachedScrollRect.content.sizeDelta = sizeDelta;
		}

		/// <summary>
		/// VisibleRect�� �����ϱ� ���� �Լ�
		/// </summary>
		private void UpdateVisibleRect()
		{
			// visibleRect�� x, y ��ġ == ����Ʈ�� �ǹ��� �������� �� ��� ��ġ + �е�
			visibleRect.x = CachedScrollRect.content.anchoredPosition.x + visibleRectPadding.left;
			visibleRect.y = -CachedScrollRect.content.anchoredPosition.y + visibleRectPadding.top;

			// visibleRect�� ũ�� == ��ũ�Ѻ��� ũ�� + �е�
			visibleRect.width = visibleRectPadding.left + CachedRectTransform.rect.width + visibleRectPadding.right;
			visibleRect.height = visibleRectPadding.top + CachedRectTransform.rect.height + visibleRectPadding.bottom;
		}

		/// <summary>
		/// ���� ���̰��� �����ϴ� �Լ�
		/// </summary>
		/// <param name="index">ã�� ���� �ε���</param>
		/// <returns>�ش� �ε����� ���� ���̰�</returns>
		protected virtual float GetCellHeightAtIndex(int index)
		{
			// ����Ʈ�� ���̽� ���� ���̰� ����
			// ������ ũ�Ⱑ �ٸ� ��� ��ӹ��� Ŭ�������� �������̵��� ��
			return cellBase.GetComponent<RectTransform>().sizeDelta.y;
		}

		/// <summary>
		/// ���� �����ϴ� �Լ�
		/// </summary>
		/// <param name="index">������ ���� dataList �� �ε���</param>
		/// <returns>������ ��</returns>
		private RecyclerViewCell<T> CreateCellForIndex(int index)
		{
			// ���̽� ���� �̿��� ���ο� �� ����
			GameObject obj = Instantiate(cellBase);
			obj.SetActive(true);
			RecyclerViewCell<T> cell = obj.GetComponent<RecyclerViewCell<T>>();

			// �θ� ��Ҹ� �ٲٸ� �������̳� ũ�⸦ �Ҿ�����Ƿ� ������ �����صд�
			Vector3 scale = cell.transform.localScale;
			Vector2 sizeDelta = cell.CachedRectTransform.sizeDelta;
			Vector2 offsetMin = cell.CachedRectTransform.offsetMin;
			Vector2 offsetMax = cell.CachedRectTransform.offsetMax;

			// ���ο� ���� �θ� ��Ҹ� ���̽� ���� �θ�� ����
			cell.transform.SetParent(cellBase.transform.parent);

			// ���� �����ϰ� ũ�⸦ �����Ѵ�
			cell.transform.localScale = scale;
			cell.CachedRectTransform.sizeDelta = sizeDelta;
			cell.CachedRectTransform.offsetMin = offsetMin;
			cell.CachedRectTransform.offsetMax = offsetMax;

			// ������ �ε����� ���� ����Ʈ �׸� �����ϴ� ���� ������ �����Ѵ�
			BindCellForIndex(cell, index);

			_cells.AddLast(cell);

			return cell;
		}

		/// <summary>
		/// ���� �����Ϳ� ���ε��ϴ� �Լ�
		/// </summary>
		/// <param name="cell">���ε��� ��</param>
		/// <param name="index">���ε��� ���� dataList �� �ε���</param>
		private void BindCellForIndex(RecyclerViewCell<T> cell, int index)
		{
			// ���� Index ������Ƽ���� dataList �� �ε��������� �����Ѵ�
			cell.Index = index;

			// dataList�� �ش� �ε����� ���� �������� �����Ѵٸ� ���� Ȱ��ȭ�ؼ� �����͸� ���ε��ϰ� ���̸� �����Ѵ�
			if (cell.Index >= 0 && cell.Index <= _itemDataList.Count - 1)
			{
				cell.BindView(_itemDataList[cell.Index]);
				cell.Height = GetCellHeightAtIndex(cell.Index);
				cell.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// VisibleRect ������ ǥ�õ� ��ŭ�� ���� �����Ͽ� ��ġ�ϴ� �Լ�
		/// </summary>
		private void FillVisibleRectWithCells()
		{
			// ���� ���ٸ� �ƹ� �ϵ� ���� �ʴ´�
			if (_cells.Count < 1) return;

			// ǥ�õ� ������ ���� �����ϴ� dataList �׸��� ���� �׸��� �����ϰ�
			// ���� �� ���� visibleRect�� ������ ���´ٸ� �����ϴ� ���� �ۼ��Ѵ�
			RecyclerViewCell<T> lastCell = _cells.Last.Value;
			int nextCellDataIndex = lastCell.Index + 1;
			Vector2 nextCellTop = lastCell.Bottom + new Vector2(0.0f, -spacingHeight);

			while (nextCellDataIndex < _itemDataList.Count &&
				   nextCellTop.y >= visibleRect.y - visibleRect.height)
			{
				RecyclerViewCell<T> cell = CreateCellForIndex(nextCellDataIndex);
				cell.Top = nextCellTop;
			}
		}

		/// <summary>
		/// ��ũ�Ѻ䰡 �������� �� ȣ��Ǵ� �Լ�
		/// </summary>
		/// <param name="scrollPos">��ũ���� x, y ��ġ</param>
		public void OnScrollPosChanged(Vector2 scrollPos)
		{
			// visibleRect�� �����Ѵ�
			UpdateVisibleRect();
			// ���� �����Ѵ�
			UpdateCells((scrollPos.y < prevScrollPos.y) ? SCROLL_DOWNWARD : SCROLL_UPWARD);
		}

		/// <summary>
		/// ���� �����Ͽ� ǥ�ø� �����ϴ� �Լ�
		/// </summary>
		/// <param name="scrollDirection">��ũ�� ����. 1: �Ʒ���, -1: ����</param>
		private void UpdateCells(int scrollDirection)
		{
			if (_cells.Count < 1) return;

			// �Ʒ��� ��ũ���ϰ� ���� ���� visibleRect�� ������ �������� ���� �ִ� ����
			// �Ʒ��� ���� ������� �̵����� ������ �����Ѵ�
			if (scrollDirection == SCROLL_DOWNWARD)
			{
				RecyclerViewCell<T> firstCell = _cells.First.Value;
				while (firstCell.Bottom.y > visibleRect.y)
				{
					RecyclerViewCell<T> lastCell = _cells.Last.Value;
					BindCellForIndex(firstCell, lastCell.Index + 1);
					firstCell.Top = lastCell.Bottom + new Vector2(0.0f, -spacingHeight);

					_cells.AddLast(firstCell);
					_cells.RemoveFirst();
					firstCell = _cells.First.Value;
				}

				// visibleRect�� ������ ���� �ȿ� �� ���� ������ ���� �ۼ��Ѵ�
				FillVisibleRectWithCells();
			}
			else if (scrollDirection == SCROLL_UPWARD)
			{
				// ���� ��ũ���ϰ� ���� ���� visibleRect�� ������ �������� �Ʒ��� �ִ� ����
				// ���� ���� ������� �̵����� ������ �����Ѵ�
				RecyclerViewCell<T> lastCell = _cells.Last.Value;
				while (lastCell.Top.y < visibleRect.y - visibleRect.height)
				{
					RecyclerViewCell<T> firstCell = _cells.First.Value;
					BindCellForIndex(lastCell, firstCell.Index - 1);
					lastCell.Bottom = firstCell.Top + new Vector2(0.0f, spacingHeight);

					_cells.AddFirst(lastCell);
					_cells.RemoveLast();
					lastCell = _cells.Last.Value;
				}
			}
		}
	}
}

