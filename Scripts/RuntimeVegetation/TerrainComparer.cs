using Assets.Scripts.Tools.AAIMapTools.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	public class TerrainComparer : IComparer<RaycastHit>
	{
		public int Compare(RaycastHit x, RaycastHit y)
		{
			return x.collider.GetComponent<TerrainIndex>().Index
				.CompareTo(y.collider.GetComponent<TerrainIndex>().Index);
		}
	}
}
