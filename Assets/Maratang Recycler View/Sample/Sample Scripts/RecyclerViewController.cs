using Maratangsoft.RecyclerView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
	public class RecyclerViewController : MonoBehaviour, IRecyclerViewDataSource
	{
		[SerializeField]
		private RecyclerView recyclerView;

		private List<SampleData> _itemDataList;

		void Start()
		{
			LoadData();
			recyclerView.Initialize(this);
		}

		void LoadData()
		{
			_itemDataList = new List<SampleData>()
			{
				new SampleData { Index=1, Name="aaa", Age=32 },
				new SampleData { Index=2, Name="bbb", Age=20 },
				new SampleData { Index=3, Name="ccc", Age=56 },
				new SampleData { Index=4, Name="ddd", Age=34 },
				new SampleData { Index=5, Name="eee", Age=74 },
				new SampleData { Index=6, Name="fff", Age=45 },
				new SampleData { Index=7, Name="ggg", Age=12 },
				new SampleData { Index=8, Name="hhh", Age=29 },
				new SampleData { Index=9, Name="iii", Age=73 },
				new SampleData { Index=10, Name="jjj", Age=56 },
				new SampleData { Index=11, Name="kkk", Age=23 },
				new SampleData { Index=12, Name="lll", Age=14 },
				new SampleData { Index=13, Name="mmm", Age=25 },
				new SampleData { Index=14, Name="nnn", Age=45 },
				new SampleData { Index=15, Name="ooo", Age=24 },
				new SampleData { Index=16, Name="ppp", Age=14 },
				new SampleData { Index=17, Name="qqq", Age=37 },
				new SampleData { Index=18, Name="rrr", Age=45 },
				new SampleData { Index=19, Name="sss", Age=97 },
			};
		}

		public int GetItemCount()
		{
			return _itemDataList.Count;
		}

		public void BindCell(ICell cell, int index)
		{
			var item = cell as SampleRecyclerViewCell;
			item.BindCell(_itemDataList[index], index);
		}
	}
}

