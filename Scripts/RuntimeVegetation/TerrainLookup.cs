using System.Collections.Generic;
using UnityEngine;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	public class TerrainLookup
	{
		private readonly int _terrainWidth;
		private readonly int _terrainsAcross;
		private List<Terrain> _terrains;
		private Vector3 _gridOffset;

		public TerrainLookup(int terrainWidth, int terrainsAcross)
		{
			_terrainWidth = terrainWidth;
			_terrainsAcross = terrainsAcross;
			_terrains = new List<Terrain>();
		}

		public void AddTerrain(Terrain terrain)
		{
			if (_terrains.Count == 0)
				_gridOffset = terrain.transform.position;

			_terrains.Add(terrain);
		}

		public Terrain GetTerrain(Vector3 worldSpacePosition)
		{
			var terrainIndex = GetTerrainKey(worldSpacePosition, _terrainWidth);
			return _terrains[terrainIndex];
		}

		private int GetTerrainKey(Vector3 worldSpacePosition, int terrainWidth)
		{
			var xIndex = Mathf.FloorToInt((worldSpacePosition.x - _gridOffset.x) / terrainWidth);
			var yIndex = Mathf.FloorToInt((worldSpacePosition.z - _gridOffset.z) / terrainWidth);

			return yIndex * _terrainsAcross + xIndex;
		}
	}
}
