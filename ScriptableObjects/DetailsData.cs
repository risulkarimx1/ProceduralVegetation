using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[CreateAssetMenu(fileName = "Details Object List", menuName = "AAIMapTools/Details Object List", order = 1)]
	public class DetailsData : ScriptableObject
	{
		[SerializeField] private List<Detail> _details;

		public int Count => _details.Count;
		public Detail GetObjectAt(int index)
		{
			return _details[index];
		}
	}
}
