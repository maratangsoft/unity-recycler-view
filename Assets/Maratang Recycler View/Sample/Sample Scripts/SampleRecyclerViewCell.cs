using Maratangsoft.RecyclerView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
	public class SampleRecyclerViewCell : MonoBehaviour, ICell
	{
		[SerializeField]
		private TextMeshProUGUI indexText;
		[SerializeField]
		private TextMeshProUGUI nameText;
		[SerializeField]
		private TextMeshProUGUI ageText;

		private SampleData _sampleData;
		private int _cellIndex;

		public void BindCell(SampleData data, int index)
		{
			_sampleData = data;
			_cellIndex = index;

			indexText.text = data.Index.ToString();
			nameText.text = data.Name;
			ageText.text = data.Age.ToString();
		}

		public void OnClicked()
		{
			Debug.Log("cell clicked // cell index: " + _cellIndex);
			Debug.Log("item index: " + _sampleData.Index + ", name: " + _sampleData.Name + ", age: " + _sampleData.Age);
		}
	}
}
