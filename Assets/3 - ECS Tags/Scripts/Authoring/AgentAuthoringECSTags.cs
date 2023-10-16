using UnityEngine;
using Unity.Entities;

public class AgentAuthoringECSTags : MonoBehaviour
{
	[SerializeField]
	private float m_TimeUntilZombie;

	class Baker : Baker<AgentAuthoringECSTags>
	{
		public override void Bake(AgentAuthoringECSTags authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new CalmState());
			AddComponent(entity, new HumanState());
			if (authoring.m_TimeUntilZombie > 0.0f)
			{
				AddComponent(entity, new InfectedState{ IncubationEndTime = authoring.m_TimeUntilZombie });
			}

			AddComponent(entity, new AgentFOVResults());
		}
	}
}
