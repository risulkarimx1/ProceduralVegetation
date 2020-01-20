using System;
using UnityEngine;

namespace AAI.VDTSimulator.EditorTools.MapTools
{
	public struct Quad
	{
		public Vector3 V0;
		public Vector3 V1;
		public Vector3 V2;
		public Vector3 V3;

		public Vector3 this[int index]
		{
			get
			{
				if (index == 0)
					return V0;
				if (index == 1)
					return V1;
				if (index == 2)
					return V2;
				if (index == 3)
					return V3;

				throw new ArgumentOutOfRangeException(nameof(index));
			}
		}
	}
}
