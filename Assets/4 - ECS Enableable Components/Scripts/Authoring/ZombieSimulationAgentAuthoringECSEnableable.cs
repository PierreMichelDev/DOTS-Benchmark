using UnityEngine;
using Unity.Entities;

public class ZombieSimulationAgentAuthoringECSEnableable : MonoBehaviour
{
	[SerializeField]
	private float m_TimeUntilZombie;

	class Baker : Baker<ZombieSimulationAgentAuthoringECSEnableable>
	{
		public override void Bake(ZombieSimulationAgentAuthoringECSEnableable authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ZombieSimulationCalmStateEnableable());
			AddComponent(entity, new ZombieSimulationHumanStateEnableable());

			AddComponent(entity, new ZombieSimulationInfectedStateEnableable{ IncubationEndTime = authoring.m_TimeUntilZombie });
			AddComponent(entity, new ZombieSimulationChaseStateEnableable());
			AddComponent(entity, new ZombieSimulationPanicStateEnableable());
			AddComponent(entity, new ZombieSimulationZombieStateEnableable());

			SetComponentEnabled<ZombieSimulationInfectedStateEnableable>(entity, authoring.m_TimeUntilZombie > 0.0f);
			SetComponentEnabled<ZombieSimulationChaseStateEnableable>(entity, false);
			SetComponentEnabled<ZombieSimulationPanicStateEnableable>(entity, false);
			SetComponentEnabled<ZombieSimulationZombieStateEnableable>(entity, false);

			AddComponent(entity, new ZombieSimulationAgentFOVResults());
		}
	}
}
