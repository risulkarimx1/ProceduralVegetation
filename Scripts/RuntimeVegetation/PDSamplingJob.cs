using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	[BurstCompile(CompileSynchronously = true)]
	public struct PDSamplingJob : IJob
	{
		// Inputs - General
		public int SamplingCount; // Maximum number of attempts before marking a sample as inactive.
		public float Radius;
		public float Width;
		public float Height;
		public float CellSize;
		public Random Random;

		public NativeList<float2> ActiveSamples;

		// Input - Grids
		public int GridWidth;
		public int GridHeight;
		[DeallocateOnJobCompletion] public NativeArray<float2> GridArray;

		// Output
		[WriteOnly] public NativeList<float3> Result;

		public void Execute()
		{
			// First sample is chosen randomly
			var firstSample = new float2(Random.NextFloat() * Width, Random.NextFloat() * Height);
			AddSample(firstSample);

			while (ActiveSamples.Length > 0)
			{
				// Pick a Random active sample
				var index = (int) (Random.NextFloat() * (ActiveSamples.Length - 1));
				var sample = ActiveSamples[index];

				// Try `SamplingCount` Random candidates between [radius, 2 * radius] from that sample.
				var found = false;
				for (var j = 0; j < SamplingCount; j++)
				{
					var angle = 2f * math.PI * Random.NextFloat();
					var radius = Radius * (Random.NextFloat() + 1f);

					var sampleX = math.max(0, math.min(Width, sample.x + math.cos(angle) * radius));
					var sampleY = math.max(0, math.min(Height, sample.y + math.sin(angle) * radius));
					var candidate = new float2(sampleX, sampleY);

					// Accept candidates if it's inside the Width and Height and farther than 2 * radius to any existing sample.
					if (IsFarEnough(candidate))
					{
						found = true;
						AddSample(candidate);
						break;
					}
				}

				// If we couldn't find a valid candidate after SamplingCount attempts, remove this sample from the active samples queue
				if (!found)
					ActiveSamples.RemoveAtSwapBack(index);
			}
		}

		private bool IsFarEnough(float2 sample)
		{
			var pos = GetGridPosition(sample);

			var xmin = math.max(pos.x - 2, 0);
			var ymin = math.max(pos.y - 2, 0);

			var xmax = math.min(pos.x + 2, GridWidth - 1);
			var ymax = math.min(pos.y + 2, GridHeight - 1);

			for (var y = ymin; y <= ymax; y++)
			{
				for (var x = xmin; x <= xmax; x++)
				{
					var s = GetValueFromGrid(x, y);
					if (s.x != float.MinValue)
					{
						if (math.distance(s, sample) < Radius)
							return false;
					}
				}
			}

			return true;
		}

		/// Adds the sample to the active samples queue and the gridArray
		private void AddSample(float2 sample)
		{
			ActiveSamples.Add(sample);
			var pos = GetGridPosition(sample);
			AddValueToGrid(pos.x, pos.y, sample);
			Result.Add(new float3(sample.x, 0, sample.y));
		}

		private int2 GetGridPosition(float2 sample)
		{
			var x = (int) (sample.x / CellSize);
			var y = (int) (sample.y / CellSize);
			return new int2(x, y);
		}

		private void AddValueToGrid(int col, int row, float2 value)
		{
			var index = GetLinearIndex(col, row);
			GridArray[index] = value;
		}

		private int GetLinearIndex(int col, int row)
		{
			return col + row * GridWidth;
		}

		private float2 GetValueFromGrid(int col, int row)
		{
			return GridArray[GetLinearIndex(col, row)];
		}
	}
}
