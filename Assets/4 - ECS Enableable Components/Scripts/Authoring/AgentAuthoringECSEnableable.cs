using UnityEngine;
using Unity.Entities;

public class AgentAuthoringECSEnableable : MonoBehaviour
{
	[SerializeField]
	private float m_TimeUntilZombie;

	class Baker : Baker<AgentAuthoringECSEnableable>
	{
		public override void Bake(AgentAuthoringECSEnableable authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new CalmStateEnableable());
			AddComponent(entity, new HumanStateEnableable());

			AddComponent(entity, new InfectedStateEnableable{ IncubationEndTime = authoring.m_TimeUntilZombie });
			AddComponent(entity, new ChaseStateEnableable());
			AddComponent(entity, new PanicStateEnableable());
			AddComponent(entity, new ZombieStateEnableable());

			SetComponentEnabled<InfectedStateEnableable>(entity, authoring.m_TimeUntilZombie > 0.0f);
			SetComponentEnabled<ChaseStateEnableable>(entity, false);
			SetComponentEnabled<PanicStateEnableable>(entity, false);
			SetComponentEnabled<ZombieStateEnableable>(entity, false);

			AddComponent(entity, new AgentFOVResults());
		}
	}
}
