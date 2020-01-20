using UnityEngine;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[CreateAssetMenu(fileName = "VegetatorSettings", menuName = "AAIMapTools/VegetatorSettings", order = 1)]
	public class VegetatorSettings : ScriptableObject
	{
		[Tooltip("Distance away from the edge of the road to move before placing any vegetation")]
		[SerializeField]
		public int RoadBoundOffset;

		[Tooltip("Maximum distance away from the road that trees will be placed")]
		[SerializeField]
		public int PlacementDepth;

		[Tooltip("The minimum distance between any two trees within a region")]
		[SerializeField]
		public float MinimumDistanceBetweenTrees;

		[Tooltip("The minimum distance between any two pieces of grass within a region")]
		[SerializeField]
		public float MinimumDistanceBetweenGrass;

		[Tooltip("The number of samples to take when calculating vegetation placement points.")]
		[SerializeField]
		public int PlacementSampleCount;
	}
}
