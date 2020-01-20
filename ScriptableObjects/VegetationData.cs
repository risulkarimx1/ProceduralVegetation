using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[CreateAssetMenu(fileName = "VegetationObjects", menuName = "AAIMapTools/Vegetation Object List", order = 1)]
	public class VegetationData : ScriptableObject
	{
		[SerializeField] private List<Vegetation> _vegetations;

		public int Count => _vegetations.Count;
		public Vegetation GetObjectAt(int index)
		{
			return _vegetations[index];
		}
	}
}
