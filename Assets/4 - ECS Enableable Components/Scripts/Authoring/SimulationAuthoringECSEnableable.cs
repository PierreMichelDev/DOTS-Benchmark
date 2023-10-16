using UnityEngine;
using Unity.Entities;

public class SimulationAuthoringECSEnableable : MonoBehaviour
{
	class Baker : Baker<SimulationAuthoringECSEnableable>
	{
		public override void Bake(SimulationAuthoringECSEnableable authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new SimulationECSEnableable());
		}
	}
}

public struct SimulationECSEnableable : IComponentData
{
}
