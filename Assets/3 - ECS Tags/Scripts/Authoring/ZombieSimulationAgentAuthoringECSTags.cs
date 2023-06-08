using UnityEngine;
using Unity.Entities;

public class ZombieSimulationAgentAuthoringECSTags : MonoBehaviour
{
	[SerializeField]
	private float m_TimeUntilZombie;

	class Baker : Baker<ZombieSimulationAgentAuthoringECSTags>
	{
		public override void Bake(ZombieSimulationAgentAuthoringECSTags authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ZombieSimulationCalmState());
			AddComponent(entity, new ZombieSimulationHumanState());
			if (authoring.m_TimeUntilZombie > 0.0f)
			{
				AddComponent(entity, new ZombieSimulationInfectedState{ IncubationEndTime = authoring.m_TimeUntilZombie });
			}

			AddComponent(entity, new ZombieSimulationAgentFOVResults());
		}
	}
}
