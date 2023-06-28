using UnityEngine;

namespace Maratangsoft.RecyclerView
{
	internal static class UiExtensions
	{
		internal static float TopY(this RectTransform rectTransform)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			return corners[0].y;
		}

		internal static float BottomY(this RectTransform rectTransform)
		{
			Vector3[] corners = new Vector3[4];
			rectTransform.GetWorldCorners(corners);
			return corners[2].y;
		}
	}
}
