using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[BurstCompile(CompileSynchronously = true)]
	public struct PointTransformationJob : IJob
	{
		public Matrix4x4 TransformMatrix;
		[ReadOnly] public NativeList<float3> Points;
		[WriteOnly] public NativeList<float3> Output;

		public void Execute()
		{
			for (var i = 0; i < Points.Length; i++)
			{
				Output.Add(math.transform(TransformMatrix, Points[i]));
			}
		}
	}
}
