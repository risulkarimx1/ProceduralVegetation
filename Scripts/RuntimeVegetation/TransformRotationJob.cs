using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[BurstCompile(CompileSynchronously = true)]
	public struct TransformRotationJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<Quad> Regions;
		[WriteOnly] public NativeArray<RegionData> RegionData;

		public void Execute(int index)
		{
			var region = Regions[index];

			var localOrigin = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
			var originIndex = 0;
			if (region.V0.x < localOrigin.x)
			{
				localOrigin = region.V0;
				originIndex = 0;
			}

			if (region.V1.x < localOrigin.x)
			{
				localOrigin = region.V1;
				originIndex = 1;
			}

			if (region.V2.x < localOrigin.x)
			{
				localOrigin = region.V2;
				originIndex = 2;
			}

			if (region.V3.x < localOrigin.x)
			{
				localOrigin = region.V3;
				originIndex = 3;
			}

			var v0 = region[originIndex];
			var v1 = region[(originIndex + 1) % 4];
			var v3 = region[(originIndex + 3) % 4];

			var localXAxis = (region[(originIndex + 3) % 4] - region[originIndex]).normalized;
			var localZAxis = (region[(originIndex + 1) % 4] - region[originIndex]).normalized;

			var transMatrix = new Matrix4x4();
			transMatrix.SetRow(0, localZAxis);
			transMatrix.SetRow(1, (Vector3) math.cross(localZAxis, localXAxis));
			transMatrix.SetRow(2, localXAxis);
			transMatrix.SetColumn(3, new Vector4(localOrigin.x, localOrigin.y, localOrigin.z, 1));
			RegionData[index] = new RegionData
			{
				TransformMatrix = transMatrix,
				Width = math.distance(v1, v0),
				Height = math.distance(v3, v0)
			};
		}
	}
}
