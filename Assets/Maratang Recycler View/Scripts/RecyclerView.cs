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
		/// ���̺�並 �ʱ�ȭ�ϴ� �Լ�
		/// </summary>
		public void Initialize(IRecyclerViewDataSource dataSource)
		{
			scrollRect = GetComponent<ScrollRect>();
			_prevAnchoredPos = scrollRect.content.anchoredPosition;

			// recyclingSystem �ʱ�ȭ
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
		/// ��ũ�Ѻ䰡 �������� �� ȣ��Ǵ� �Լ�
		/// </summary>
		/// <param name="scrollPos">��ũ���� x, y ��ġ</param>
		public void OnScrollPosChanged(Vector2 scrollPos)
		{
			Vector2 scrollDirection = scrollRect.content.anchoredPosition - _prevAnchoredPos;
			_recyclingSystem.OnScrollPosChanged((int)scrollDirection.y);
			_prevAnchoredPos = scrollRect.content.anchoredPosition;
		}
	}
}

