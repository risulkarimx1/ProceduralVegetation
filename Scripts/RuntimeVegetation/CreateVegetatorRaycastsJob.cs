using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[BurstCompile(CompileSynchronously = true)]
	public struct CreateVegetatorRaycastsJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<float3> Positions;
		[WriteOnly] public NativeArray<RaycastCommand> Output;
		public int LayerMask;

		public void Execute(int index)
		{
			Output[index] = new RaycastCommand(Positions[index] + new float3(0, 1, 0) * 10000, Vector3.down, layerMask: LayerMask);
		}
	}
}
