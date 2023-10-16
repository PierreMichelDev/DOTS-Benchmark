using UnityEngine;
using Unity.Entities;

public class SimulationAuthoringECSEnum : MonoBehaviour
{
	class Baker : Baker<SimulationAuthoringECSEnum>
	{
		public override void Bake(SimulationAuthoringECSEnum authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new SimulationECSEnum());
		}
	}
}

public struct SimulationECSEnum : IComponentData
{
}
