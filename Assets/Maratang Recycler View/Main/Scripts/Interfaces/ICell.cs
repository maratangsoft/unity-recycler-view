using UnityEngine;

namespace Maratangsoft.RecyclerView
{
	//[RequireComponent(typeof(RectTransform))]
	public interface ICell
	{
		/*public RectTransform CachedRectTransform => GetComponent<RectTransform>();

		// 아이템에 대응하는 리스트 항목의 인덱스
		public int Index { get; set; }

		// 아이템의 높이
		public float Height
		{
			get => CachedRectTransform.sizeDelta.y;
			set
			{
				Vector2 sizeDelta = CachedRectTransform.sizeDelta;
				sizeDelta.y = value;
				CachedRectTransform.sizeDelta = sizeDelta;
			}
		}

		// 데이터를 UI와 매칭시키는 메서드
		// 상속받은 클래스에서 구현
		public abstract void BindView(T itemData);

		// 셀 상단의 y좌표
		public Vector2 Top
		{
			get
			{
				// RectTransform.GetLocalCorners 의 리턴값은 bottom-left부터 시계방향 순서
				Vector3[] corners = new Vector3[4];
				CachedRectTransform.GetLocalCorners(corners);
				return CachedRectTransform.anchoredPosition + new Vector2(0.0f, corners[1].y);
			}
			set
			{
				Vector3[] corners = new Vector3[4];
				CachedRectTransform.GetLocalCorners(corners);
				CachedRectTransform.anchoredPosition = value - new Vector2(0.0f, corners[1].y);
			}
		}

		// 셀 하단의 y좌표
		public Vector2 Bottom
		{
			get
			{
				Vector3[] corners = new Vector3[4];
				CachedRectTransform.GetLocalCorners(corners);
				return CachedRectTransform.anchoredPosition + new Vector2(0.0f, corners[3].y);
			}
			set
			{
				Vector3[] corners = new Vector3[4];
				CachedRectTransform.GetLocalCorners(corners);
				CachedRectTransform.anchoredPosition = value - new Vector2(0.0f, corners[3].y);
			}
		}*/
	}
}
