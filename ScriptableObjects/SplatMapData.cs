using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[CreateAssetMenu(fileName = "Splatmap List", menuName = "AAIMapTools/SplatmapData", order = 2)]
	public class SplatMapData : ScriptableObject
	{
		[SerializeField] private List<Splatmap> _splatmaps;

		public int Count => _splatmaps.Count;
		public Splatmap GetObjectAt(int index)
		{
			return _splatmaps[index];
		}
	}
}
