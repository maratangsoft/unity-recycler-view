using UnityEngine;

namespace Maratangsoft.RecyclerView
{
	//[RequireComponent(typeof(RectTransform))]
	public interface ICell
	{
		/*public RectTransform CachedRectTransform => GetComponent<RectTransform>();

		// �����ۿ� �����ϴ� ����Ʈ �׸��� �ε���
		public int Index { get; set; }

		// �������� ����
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

		// �����͸� UI�� ��Ī��Ű�� �޼���
		// ��ӹ��� Ŭ�������� ����
		public abstract void BindView(T itemData);

		// �� ����� y��ǥ
		public Vector2 Top
		{
			get
			{
				// RectTransform.GetLocalCorners �� ���ϰ��� bottom-left���� �ð���� ����
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

		// �� �ϴ��� y��ǥ
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
