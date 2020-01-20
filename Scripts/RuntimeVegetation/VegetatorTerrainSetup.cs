using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using Zenject;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	public class VegetatorTerrainSetup
	{
		[Inject] private VegetationData _vegetationData;
		[Inject] private DetailsData _detailsData;
		[Inject] private SplatMapData _splatMapData;

		public void AddTreeDetailPrototypes(List<Terrain> terrains)
		{
			var treePrototypes = GetTreePrototypes();
			var detailPrototypes = GetDetailPrototypes();
			var splatMapsLayers = GetSplatmapLayers();

			// Push GetTreePrototypes, DetailPrototypes & Splatmat layers to the terrain
			foreach (var terrain in terrains)
			{
				var terrainData = terrain.terrainData;
				terrainData.treePrototypes = treePrototypes;
				terrainData.terrainLayers = splatMapsLayers;

				float[,,] alphamaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

				for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
				{
					for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
					{
						alphamaps[x, y, 0] = 1;
					}

				}
				terrain.terrainData.SetAlphamaps(0,0, alphamaps);
				try
				{
					terrainData.detailPrototypes = detailPrototypes; // TODO: Make the texture file's Read Write enabled+ Find a way to handle exception - Risul
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					Debug.LogWarning($"[{nameof(VegetatorTerrainSetup)}] - Make detail texture's Read Write enabled from inspector in DetailsData");
					throw;
				}
			}
		}

		private TerrainLayer[] GetSplatmapLayers()
		{
			var terrainLayers = new TerrainLayer[_splatMapData.Count];
			for (var i = 0; i < _splatMapData.Count; i++)
			{
				var splatMap = _splatMapData.GetObjectAt(i);
				terrainLayers[i] = new TerrainLayer()
				{
					diffuseTexture = splatMap.Texture,
					tileOffset = splatMap.TileOffset,
					tileSize = splatMap.TileSize
				};
			}

			return terrainLayers;
		}


		private DetailPrototype[] GetDetailPrototypes()
		{
			var detailPrototypes = new DetailPrototype[_detailsData.Count];
			for (var i = 0; i < _detailsData.Count; i++)
			{
				var item = _detailsData.GetObjectAt(i);
				detailPrototypes[i] = new DetailPrototype
				{
					prototype = item.Prototype,
					prototypeTexture = item.PrototypeTexture,
					healthyColor = item.HealthyColor,
					dryColor = item.DryColor,
					minHeight = item.HeightRange.x,
					maxHeight = item.HeightRange.y,
					minWidth = item.WidthRange.x,
					maxWidth = item.WidthRange.y,
					noiseSpread = item.NoiseSpread
				};

				if (detailPrototypes[i].prototype)
				{
					detailPrototypes[i].usePrototypeMesh = true;
					detailPrototypes[i].renderMode = DetailRenderMode.VertexLit;
				}
				else
				{
					detailPrototypes[i].usePrototypeMesh = false;
					detailPrototypes[i].renderMode = DetailRenderMode.GrassBillboard;
				}
			}

			return detailPrototypes;
		}

		private TreePrototype[] GetTreePrototypes()
		{
			var treePrototypes = new TreePrototype[_vegetationData.Count];
			for (var i = 0; i < _vegetationData.Count; i++)
			{
				var item = _vegetationData.GetObjectAt(i);
				treePrototypes[i] = new TreePrototype
				{
					prefab = item.Mesh
				};
			}

			return treePrototypes;
		}
	}
}
