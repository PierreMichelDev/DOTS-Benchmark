using UnityEngine;
using Unity.Entities;

public class SimulationAuthoringECSTags : MonoBehaviour
{
	class Baker : Baker<SimulationAuthoringECSTags>
	{
		public override void Bake(SimulationAuthoringECSTags authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new SimulationECSTags());
		}
	}
}

public struct SimulationECSTags : IComponentData
{
}
