using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Maratangsoft.RecyclerView
{
	[RequireComponent(typeof(ScrollRect))]
	[RequireComponent(typeof(RectTransform))]
	public class RecyclerView : MonoBehaviour
	{
		[SerializeField]
		private RectTransform baseCell;
		[SerializeField]
		private float additionalCoverage;
		[SerializeField]
		private float spacingHeight = 4.0f;
		[SerializeField]
		private float spacingWidth = 2.0f;

        private ScrollRect scrollRect;
		private RectTransform content;

		private RecyclingSystem _recyclingSystem;

		private Vector2 _prevScrollPos;

		/// <summary>
		/// create object pool and prepare scroll listener
		/// </summary>
		public void Initialize(IRecyclerViewDataSource dataSource)
		{
            RectTransform rectTransform = GetComponent<RectTransform>();
            scrollRect = GetComponent<ScrollRect>();
			content = scrollRect.content;

			_prevScrollPos = scrollRect.content.anchoredPosition;

			_recyclingSystem = new RecyclingSystem(rectTransform,
												   scrollRect, 
												   baseCell, 
												   dataSource,
												   additionalCoverage, 
												   spacingHeight, 
												   spacingWidth);

			_recyclingSystem.Initialize(
				() => scrollRect.onValueChanged.AddListener(OnScrollPosChanged)
			);
		}

		/// <summary>
		/// a callback invoked on the scroll position was changed
		/// </summary>
		/// <param name="scrollPos">moved position of scroll</param>
		public void OnScrollPosChanged(Vector2 scrollPos)
		{
			Vector2 scrollDirection = scrollPos - _prevScrollPos;
			_recyclingSystem.OnScrollPosChanged(scrollDirection);

			_prevScrollPos = scrollPos;
		}
	}
}

