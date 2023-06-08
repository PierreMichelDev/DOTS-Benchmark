using UnityEngine;
using Unity.Entities;

public class ZombieSimulationAuthoringECSEnum : MonoBehaviour
{
	class Baker : Baker<ZombieSimulationAuthoringECSEnum>
	{
		public override void Bake(ZombieSimulationAuthoringECSEnum authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new ZombieSimulationECSEnum());
		}
	}
}

public struct ZombieSimulationECSEnum : IComponentData
{
}
