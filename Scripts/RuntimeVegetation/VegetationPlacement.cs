using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Random = Unity.Mathematics.Random;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	public class VegetationPlacement
	{
		private VegetationData _vegetationData;
		private DetailsData _detailsData;
		private readonly VegetatorSettings _settings;
		private Random _random;

		public VegetationPlacement(VegetationData vegetationData,
			DetailsData detailsData,
			VegetatorSettings settings,
			Random random)
		{
			_vegetationData = vegetationData;
			_detailsData = detailsData;
			_settings = settings;
			_random = random;
		}

		public void Vegetate(List<DrawableMap.DrawableRoad> roads, LayerMask terrainLayerMask, TerrainLookup terrainLookup)
		{
			var placementAreas = GetPlacementAreas(roads, _settings.PlacementDepth, _settings.RoadBoundOffset);
			var placementAreasNativeArray = new NativeArray<Quad>(placementAreas.Count, Allocator.TempJob);
			placementAreasNativeArray.CopyFrom(placementAreas.ToArray());

			Profiler.BeginSample("Place Trees");

			PlaceTrees(GetPlacementPositions(terrainLayerMask, placementAreasNativeArray, _settings.MinimumDistanceBetweenTrees), terrainLookup);

			Profiler.EndSample();

			Profiler.BeginSample("Place Grass");
			for (var i = 0; i < _detailsData.Count; i++)
			{
				PlaceGrass(GetPlacementPositions(terrainLayerMask, placementAreasNativeArray, _settings.MinimumDistanceBetweenGrass), i, terrainLookup);
			}

			Profiler.EndSample();
			placementAreasNativeArray.Dispose();
		}

		private List<Vector3> GetPlacementPositions(LayerMask terrainLayerMask, NativeArray<Quad> placementAreasNativeArray, float minimumDistanceBetweenPositions)
		{
			Profiler.BeginSample("Vegetator Raycasts");
			var positions = new NativeList<float3>(Allocator.TempJob);
			GetRaycastPoints(placementAreasNativeArray, minimumDistanceBetweenPositions, positions);

			var commands = new NativeArray<RaycastCommand>(positions.Length, Allocator.TempJob);

			var createRaycastsJob = new CreateVegetatorRaycastsJob
			{
				LayerMask = terrainLayerMask,
				Positions = positions,
				Output = commands
			};

			createRaycastsJob.Schedule(positions.Length, 128).Complete();
			var raycastResults = new NativeArray<RaycastHit>(positions.Length, Allocator.TempJob);
			RaycastCommand.ScheduleBatch(commands, raycastResults, 128).Complete();

			commands.Dispose();
			positions.Dispose();

			if (raycastResults.Length == 0)
			{
				Debug.LogWarning($"[VegetationPlacement] Couldn't place trees");
				raycastResults.Dispose();
				return new List<Vector3>();
			}

			var placementPositions = new List<Vector3>();
			Profiler.BeginSample("Copying Raycast results");
			for (var i = 0; i < raycastResults.Length; i++)
			{
				placementPositions.Add(raycastResults[i].point);
			}

			Profiler.EndSample();

			raycastResults.Dispose();

			Profiler.EndSample();
			return placementPositions;
		}

		private void PlaceTrees(List<Vector3> placementPositions, TerrainLookup terrainLookup)
		{
			var treeInstances = new Dictionary<Terrain, List<TreeInstance>>();

			for (var i = 0; i < placementPositions.Count; i++)
			{
				var terrain = terrainLookup.GetTerrain(placementPositions[i]);
				var treePositionOnTerrain = GetTreePosition(terrain, placementPositions[i]);

				var protoIndex = UnityEngine.Random.Range(0,
					_vegetationData.Count); //TODO: Added Weighted Probability based on Tree Probability value - Risul

				var treeInstance = _vegetationData.GetObjectAt(protoIndex);

				if (treeInstances.TryGetValue(terrain, out var trees) == false)
				{
					trees = new List<TreeInstance>();
					treeInstances.Add(terrain, trees);
				}

				var proportionalScale = UnityEngine.Random.Range(treeInstance.MinScale, treeInstance.MaxScale);
				var tree = new TreeInstance
				{
					position = treePositionOnTerrain,
					rotation = UnityEngine.Random.Range(treeInstance.MinRotation,
						treeInstance.MaxRotation),
					prototypeIndex = protoIndex,
					color = Color.Lerp(treeInstance.Color1,
						treeInstance.Color2, UnityEngine.Random.Range(0, 1)),
					lightmapColor = treeInstance.LightColor,
					heightScale = proportionalScale,
					widthScale = proportionalScale
				};

				trees.Add(tree);
			}

			foreach (var treesPerTerrain in treeInstances)
			{
				treesPerTerrain.Key.terrainData.treeInstances = treesPerTerrain.Value.ToArray();
			}
		}

		public void PlaceGrass(List<Vector3> placementPositions, int detailLayerIndex, TerrainLookup terrainLookup)
		{
			var terrainDetail = new Dictionary<Terrain, int[,]>();

			for (var i = 0; i < placementPositions.Count; i++)
			{
				var terrain = terrainLookup.GetTerrain(placementPositions[i]);
				if (terrainDetail.TryGetValue(terrain, out var detailData) == false)
				{
					detailData = new int[terrain.terrainData.detailWidth, terrain.terrainData.detailHeight];
					terrainDetail.Add(terrain, detailData);
				}

				var detailCord = GetDetailCoordinate(terrain, placementPositions[i]);
				detailData[detailCord.y, detailCord.x] = 1;
			}

			foreach (var terrain in terrainDetail)
			{
				terrain.Key.terrainData.SetDetailLayer(0, 0, detailLayerIndex, terrain.Value);
			}
		}

		private void GetRaycastPoints(NativeArray<Quad> regions, float radius, NativeList<float3> raycastPoints)
		{
			Profiler.BeginSample("GetRaycastPoints");

			var pdSamplingJobs = new List<PDSamplingJob>();
			var jobHandles = new NativeArray<JobHandle>(regions.Length, Allocator.TempJob);

			// Getting the Right rotation for the transform
			var regionData = new NativeArray<RegionData>(regions.Length, Allocator.TempJob);
			var transformRotationJob = new TransformRotationJob
			{
				RegionData = regionData,
				Regions = regions
			};

			transformRotationJob.Schedule(regionData.Length, 128).Complete();
			var fittedPointData = new List<NativeList<float3>>();
			for (var i = 0; i < regions.Length; i++)
			{
				fittedPointData.Add(new NativeList<float3>(Allocator.TempJob));
			}

			for (var i = 0; i < regionData.Length; i++)
			{
				var activeSamples = new NativeList<float2>(Allocator.TempJob);
				var noiseResults = new NativeList<float3>(Allocator.TempJob);

				var cellSize = radius / math.sqrt(2);
				var gridWidth = (int) math.ceil(regionData[i].Width / cellSize);
				var gridHeight = (int) math.ceil(regionData[i].Height / cellSize);

				var gridArray = new NativeArray<float2>(gridWidth * gridHeight, Allocator.TempJob);
				var pdSampler = new PDSamplingJob
				{
					// Area Parameters
					SamplingCount = _settings.PlacementSampleCount,
					Width = regionData[i].Width,
					Height = regionData[i].Height,
					Radius = radius,
					CellSize = cellSize,
					Random = _random,
					// Grid Parameters
					GridWidth = gridWidth,
					GridHeight = gridHeight,
					GridArray = gridArray,
					// Sample and Results
					ActiveSamples = activeSamples,
					Result = noiseResults
				};

				pdSamplingJobs.Add(pdSampler);
				var noiseJobHandle = pdSampler.Schedule();

				var pointTransformationJob = new PointTransformationJob
				{
					Points = noiseResults,
					Output = fittedPointData[i],
					TransformMatrix = regionData[i].TransformMatrix
				};

				var pointTransformationHandle = pointTransformationJob.Schedule(noiseJobHandle);
				jobHandles[i] = pointTransformationHandle;
			}

			JobHandle.CompleteAll(jobHandles);

			foreach (var points in fittedPointData)
			{
				raycastPoints.AddRange(points);
				points.Dispose();
			}

			foreach (var pdSamplingJob in pdSamplingJobs)
			{
				pdSamplingJob.ActiveSamples.Dispose();
				pdSamplingJob.Result.Dispose();
			}

			regionData.Dispose();
			jobHandles.Dispose();
			Profiler.EndSample();
		}

		private List<Quad> GetPlacementAreas(List<DrawableMap.DrawableRoad> roads, int placementDepth, int roadBoundOffset)
		{
			var quad = new List<Quad>();
			foreach (var road in roads)
			{
				if (road.travelDirection == RoadTravelDirection.Left)
				{
					CreateBoundsFromRoadPoints(road, quad, RoadTravelDirection.Left, placementDepth, roadBoundOffset);
				}
				else if (road.travelDirection == RoadTravelDirection.Right)
				{
					CreateBoundsFromRoadPoints(road, quad, RoadTravelDirection.Right, placementDepth, roadBoundOffset);
				}
				else if (road.travelDirection == RoadTravelDirection.Both)
				{
					CreateBoundsFromRoadPoints(road, quad, RoadTravelDirection.Left, placementDepth, roadBoundOffset);
					CreateBoundsFromRoadPoints(road, quad, RoadTravelDirection.Right, placementDepth, roadBoundOffset);
				}
			}

			return quad;
		}

		private static void CreateBoundsFromRoadPoints(DrawableMap.DrawableRoad road, List<Quad> allBounds, RoadTravelDirection direction, int placementDepth,
			int roadBoundOffset)
		{
			var boundaryPoints = direction == RoadTravelDirection.Right ? road.rightBoundaryLine : road.leftBoundaryLine;

			for (var i = 1; i < road.roadReferenceLine.Count - 1; i++)
			{
				var nextSegmentMagnitude = (road.roadReferenceLine[i + 1] - road.roadReferenceLine[i]).magnitude;
				var prevSegmentMagnitude = (road.roadReferenceLine[i] - road.roadReferenceLine[i - 1]).magnitude;

				var p = boundaryPoints[i];
				var offset = p - road.roadReferenceLine[i];
				p += offset.normalized * roadBoundOffset;

				var roadTangent = Vector3.Cross(offset, Vector3.up);
				var v0 = p + roadTangent.normalized * nextSegmentMagnitude / 2;
				var v1 = v0 + offset.normalized * placementDepth;

				var v3 = p - roadTangent.normalized * prevSegmentMagnitude / 2;
				var v2 = v3 + offset.normalized * placementDepth;

				allBounds.Add(new Quad
				{
					V0 = v0,
					V1 = v1,
					V2 = v2,
					V3 = v3
				});
			}
		}

		private Vector3 GetTreePosition(Terrain terrain, Vector3 treeRootPosition)
		{
			var terrainPosition = terrain.transform.position;
			var terrainData = terrain.terrainData;
			var width = (treeRootPosition.x - terrainPosition.x) / terrainData.size.x;
			var depth = (treeRootPosition.z - terrainPosition.z) / terrainData.size.z;
			var height = (treeRootPosition.y - terrain.gameObject.transform.position.y) / terrainData.size.y;
			return new Vector3(width, height, depth);
		}

		private Vector2Int GetDetailCoordinate(Terrain terrain, Vector3 grassWorldSpace)
		{
			var terrainPosition = terrain.transform.position;
			var terrainData = terrain.terrainData;
			var width = (grassWorldSpace.x - terrainPosition.x) / terrainData.size.x;
			var depth = (grassWorldSpace.z - terrainPosition.z) / terrainData.size.z;

			var detailMapCordX = (int) (width * (terrainData.detailWidth - 1));
			var detailMapCordY = (int) (depth * (terrainData.detailHeight - 1));
			return new Vector2Int(detailMapCordX, detailMapCordY);
		}
	}
}
