using UnityEngine;
using Unity.Entities;

public class ZombieSimulationAuthoringECSEnableable : MonoBehaviour
{
	class Baker : Baker<ZombieSimulationAuthoringECSEnableable>
	{
		public override void Bake(ZombieSimulationAuthoringECSEnableable authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new ZombieSimulationECSEnableable());
		}
	}
}

public struct ZombieSimulationECSEnableable : IComponentData
{
}
