using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maratangsoft.RecyclerView
{
	public interface IRecyclerViewDataSource
	{
		int GetItemCount();
		void BindCell(ICell cell, int index);
	}
}