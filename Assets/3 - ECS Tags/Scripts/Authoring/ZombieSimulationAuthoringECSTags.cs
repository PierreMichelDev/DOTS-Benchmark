using UnityEngine;
using Unity.Entities;

public class ZombieSimulationAuthoringECSTags : MonoBehaviour
{
	class Baker : Baker<ZombieSimulationAuthoringECSTags>
	{
		public override void Bake(ZombieSimulationAuthoringECSTags authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new ZombieSimulationECSTags());
		}
	}
}

public struct ZombieSimulationECSTags : IComponentData
{
}
