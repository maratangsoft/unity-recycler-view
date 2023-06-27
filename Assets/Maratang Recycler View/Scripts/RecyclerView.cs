using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Maratangsoft.RecyclerView
{
	[RequireComponent(typeof(ScrollRect))]
	[RequireComponent (typeof(RectTransform))]
	public class RecyclerView : MonoBehaviour
	{
		[SerializeField]
		private RectTransform baseCell;

		// parameters about design
		[SerializeField]
		private float invisibleRate;
		[SerializeField]
		private float spacingHeight = 4.0f;
		[SerializeField]
		private float spacingWidth = 2.0f;

		private ScrollRect scrollRect;
		private RecyclingSystem _recyclingSystem;

		private Vector2 _prevAnchoredPos;

		/// <summary>
		/// 테이블뷰를 초기화하는 함수
		/// </summary>
		public void Initialize(IRecyclerViewDataSource dataSource)
		{
			scrollRect = GetComponent<ScrollRect>();
			_prevAnchoredPos = scrollRect.content.anchoredPosition;

			// recyclingSystem 초기화
			_recyclingSystem = new RecyclingSystem(scrollRect, 
												   baseCell, 
												   dataSource,
												   invisibleRate, 
												   spacingHeight, 
												   spacingWidth);

			_recyclingSystem.Initialize(
				() => scrollRect.onValueChanged.AddListener(OnScrollPosChanged)
			);
		}

		/// <summary>
		/// 스크롤뷰가 움직였을 때 호출되는 함수
		/// </summary>
		/// <param name="scrollPos">스크롤의 x, y 위치</param>
		public void OnScrollPosChanged(Vector2 scrollPos)
		{
			Vector2 scrollDirection = scrollRect.content.anchoredPosition - _prevAnchoredPos;
			_recyclingSystem.OnScrollPosChanged((int)scrollDirection.y);
			_prevAnchoredPos = scrollRect.content.anchoredPosition;
		}
	}
}

