using Maratangsoft.RecyclerView;
using TMPro;
using UnityEngine;

namespace Sample
{
	public class SampleRecyclerViewCell : RecyclerViewCell<SampleData>
	{
		[SerializeField]
		private TextMeshProUGUI indexText;
		[SerializeField]
		private TextMeshProUGUI nameText;
		[SerializeField]
		private TextMeshProUGUI ageText;

		public override void BindView(SampleData itemData)
		{
			indexText.text = itemData.Index.ToString();
			nameText.text = itemData.Name;
			ageText.text = itemData.Age.ToString();
		}
	}
}
