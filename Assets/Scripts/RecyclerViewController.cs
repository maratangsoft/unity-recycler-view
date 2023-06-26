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

		protected List<T> _itemDataList = new List<T>(); // 리스트 항목의 데이터를 저장
		[SerializeField] 
		protected GameObject cellBase; // 모든 셀의 베이스가 될 셀
		[SerializeField] 
		private RectOffset padding; // 스크롤할 내용의 패딩
		[SerializeField]
		private float spacingHeight = 4.0f; // 각 셀의 세로간격
		[SerializeField]
		private float spacingWidth = 2.0f; // 각 셀의 가로간격
		[SerializeField]
		private RectOffset visibleRectPadding; // visibleRect의 패딩

		private LinkedList<RecyclerViewCell<T>> _cells = new LinkedList<RecyclerViewCell<T>>(); // 셀 저장 리스트

		private Rect visibleRect; // 리스트 항목을 셀의 형태로 표시하는 범위를 나타내는 사각형

		private Vector2 prevScrollPos; // 바로 전의 스크롤 위치를 저장

		// 
		public RectTransform CachedRectTransform => GetComponent<RectTransform>();
		public ScrollRect CachedScrollRect => GetComponent<ScrollRect>();

		protected virtual void Start()
		{
			// 베이스 셀은 비활성화
			cellBase.SetActive(false);
			// Scroll Rect 컴포넌트의 On Value Changed 이벤트의 이벤트 리스너를 설정한다
			CachedScrollRect.onValueChanged.AddListener(OnScrollPosChanged);
		}

		/// <summary>
		/// 테이블뷰를 초기화하는 함수
		/// </summary>
		protected void InitializeTableView()
		{
			UpdateScrollViewSize();
			UpdateVisibleRect();
			
			// 셀이 하나도 없을 때는 visibleRect의 범위에 들어가는 첫 번째 리스트 항목을 찾아서
			// 그에 대응하는 셀을 작성한다
			if (_cells.Count < 1)
			{
				Vector2 cellTop = new Vector2(0.0f, -padding.top);
				for (int i = 0; i < _itemDataList.Count; i++)
				{
					float cellHeight = GetCellHeightAtIndex(i);
					Vector2 cellBottom = cellTop + new Vector2(0.0f, -cellHeight);

					// GUI 클래스의 x, y 값은 좌측 위 모서리 좌표값, 위에서 아래로 증가함
					// 셀 상단이 visibleRect 상단보다 위이고 하단보다 아래이거나
					// 셀 하단이 visibleRect 상단보다 위이고 하단보다 아래인 경우 (== 첫 줄)
					// 생성하려는 셀의 상단 위치를 visibleRect 꼭대기에서 padding.top만큼 떨어진 위치로 지정
					if ((cellTop.y <= visibleRect.y && cellTop.y >= visibleRect.y - visibleRect.height) ||
						(cellBottom.y <= visibleRect.y && cellBottom.y >= visibleRect.y - visibleRect.height))
					{
						RecyclerViewCell<T> cell = CreateCellForIndex(i);
						cell.Top = cellTop;
						break;
					}
					// 셀 상단과 하단 모두 visibleRect 영역 안에 있을 경우 (== 두번째 줄 이하)
					// 생성하려는 셀의 상단 위치를 이전 줄에서 spacingHeight만큼 떨어진 위치로 지정
					cellTop = cellBottom + new Vector2(0.0f, spacingHeight);
				}
				// visibleRect의 범위에 빈 곳이 있으면 셀을 작성한다
				FillVisibleRectWithCells();
			}
			else
			{
				// 이미 셀이 있을 때는 첫 번째 셀부터 순서대로 대응하는 리스트 항목의 인덱스를 다시 설정하고
				// 위치와 내용을 갱신한다
				LinkedListNode<RecyclerViewCell<T>> node = _cells.First;
				BindCellForIndex(node.Value, node.Value.Index);
				node = node.Next;

				while (node != null)
				{
					BindCellForIndex(node.Value, node.Value.Index + 1);
					node.Value.Top = node.Previous.Value.Bottom + new Vector2(0.0f, -spacingHeight);
					node = node.Next;
				}

				// visibleRect의 범위에 빈 곳이 있으면 셀을 작성한다
				FillVisibleRectWithCells();
			}
		}

		/// <summary>
		/// 스크롤뷰의 컨텐트 높이를 갱신하는 함수
		/// </summary>
		protected void UpdateScrollViewSize()
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
			Vector2 sizeDelta = CachedScrollRect.content.sizeDelta;
			sizeDelta.y = padding.top + contentHeight + padding.bottom;
			CachedScrollRect.content.sizeDelta = sizeDelta;
		}

		/// <summary>
		/// VisibleRect를 갱신하기 위한 함수
		/// </summary>
		private void UpdateVisibleRect()
		{
			// visibleRect의 x, y 위치 == 컨텐트의 피벗을 기준으로 한 상대 위치 + 패딩
			visibleRect.x = CachedScrollRect.content.anchoredPosition.x + visibleRectPadding.left;
			visibleRect.y = -CachedScrollRect.content.anchoredPosition.y + visibleRectPadding.top;

			// visibleRect의 크기 == 스크롤뷰의 크기 + 패딩
			visibleRect.width = visibleRectPadding.left + CachedRectTransform.rect.width + visibleRectPadding.right;
			visibleRect.height = visibleRectPadding.top + CachedRectTransform.rect.height + visibleRectPadding.bottom;
		}

		/// <summary>
		/// 셀의 높이값을 리턴하는 함수
		/// </summary>
		/// <param name="index">찾을 셀의 인덱스</param>
		/// <returns>해당 인덱스의 셀의 높이값</returns>
		protected virtual float GetCellHeightAtIndex(int index)
		{
			// 디폴트로 베이스 셀의 높이값 리턴
			// 셀마다 크기가 다를 경우 상속받은 클래스에서 오버라이드할 것
			return cellBase.GetComponent<RectTransform>().sizeDelta.y;
		}

		/// <summary>
		/// 셀을 생성하는 함수
		/// </summary>
		/// <param name="index">생성할 셀의 dataList 상 인덱스</param>
		/// <returns>생성된 셀</returns>
		private RecyclerViewCell<T> CreateCellForIndex(int index)
		{
			// 베이스 셀을 이용해 새로운 셀 생성
			GameObject obj = Instantiate(cellBase);
			obj.SetActive(true);
			RecyclerViewCell<T> cell = obj.GetComponent<RecyclerViewCell<T>>();

			// 부모 요소를 바꾸면 스케일이나 크기를 잃어버리므로 변수에 저장해둔다
			Vector3 scale = cell.transform.localScale;
			Vector2 sizeDelta = cell.CachedRectTransform.sizeDelta;
			Vector2 offsetMin = cell.CachedRectTransform.offsetMin;
			Vector2 offsetMax = cell.CachedRectTransform.offsetMax;

			// 새로운 셀의 부모 요소를 베이스 셀의 부모로 변경
			cell.transform.SetParent(cellBase.transform.parent);

			// 셀의 스케일과 크기를 설정한다
			cell.transform.localScale = scale;
			cell.CachedRectTransform.sizeDelta = sizeDelta;
			cell.CachedRectTransform.offsetMin = offsetMin;
			cell.CachedRectTransform.offsetMax = offsetMax;

			// 지정된 인덱스가 붙은 리스트 항목에 대응하는 셀로 내용을 갱신한다
			BindCellForIndex(cell, index);

			_cells.AddLast(cell);

			return cell;
		}

		/// <summary>
		/// 셀을 데이터와 바인드하는 함수
		/// </summary>
		/// <param name="cell">바인드할 셀</param>
		/// <param name="index">바인드할 셀의 dataList 상 인덱스</param>
		private void BindCellForIndex(RecyclerViewCell<T> cell, int index)
		{
			// 셀의 Index 프로퍼티값을 dataList 상 인덱스값으로 설정한다
			cell.Index = index;

			// dataList에 해당 인덱스를 가진 아이템이 존재한다면 셀을 활성화해서 데이터를 바인드하고 높이를 설정한다
			if (cell.Index >= 0 && cell.Index <= _itemDataList.Count - 1)
			{
				cell.BindView(_itemDataList[cell.Index]);
				cell.Height = GetCellHeightAtIndex(cell.Index);
				cell.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// VisibleRect 범위에 표시될 만큼의 셀을 생성하여 배치하는 함수
		/// </summary>
		private void FillVisibleRectWithCells()
		{
			// 셀이 없다면 아무 일도 하지 않는다
			if (_cells.Count < 1) return;

			// 표시된 마지막 셀에 대응하는 dataList 항목의 다음 항목이 존재하고
			// 또한 그 셀이 visibleRect의 범위에 들어온다면 대응하는 셀을 작성한다
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
		/// 스크롤뷰가 움직였을 때 호출되는 함수
		/// </summary>
		/// <param name="scrollPos">스크롤의 x, y 위치</param>
		public void OnScrollPosChanged(Vector2 scrollPos)
		{
			// visibleRect를 갱신한다
			UpdateVisibleRect();
			// 셀을 재사용한다
			UpdateCells((scrollPos.y < prevScrollPos.y) ? SCROLL_DOWNWARD : SCROLL_UPWARD);
		}

		/// <summary>
		/// 셀을 재사용하여 표시를 갱신하는 함수
		/// </summary>
		/// <param name="scrollDirection">스크롤 방향. 1: 아래로, -1: 위로</param>
		private void UpdateCells(int scrollDirection)
		{
			if (_cells.Count < 1) return;

			// 아래로 스크롤하고 있을 때는 visibleRect에 지정된 범위보다 위에 있는 셀을
			// 아래를 향해 순서대로 이동시켜 내용을 갱신한다
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

				// visibleRect에 지정된 범위 안에 빈 곳이 있으면 셀을 작성한다
				FillVisibleRectWithCells();
			}
			else if (scrollDirection == SCROLL_UPWARD)
			{
				// 위로 스크롤하고 있을 때는 visibleRect에 지정된 범위보다 아래에 있는 셀을
				// 위를 향해 순서대로 이동시켜 내용을 갱신한다
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

