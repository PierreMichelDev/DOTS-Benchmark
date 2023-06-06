using UnityEngine;
using Unity.Entities;

public class ZombieSimulationAgentAuthoringECSEnum : MonoBehaviour
{
	[SerializeField]
	private ZombieSimulationAgentState.HealthState m_StartState;
	[SerializeField]
	private float m_DefaultIncubationTime;

	class Baker : Baker<ZombieSimulationAgentAuthoringECSEnum>
	{
		public override void Bake(ZombieSimulationAgentAuthoringECSEnum authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new ZombieSimulationAgentState()
			{
				Health = authoring.m_StartState,
				NextDirectionChangeTime = 0,
				IncubationEndTime = authoring.m_DefaultIncubationTime
			});
			AddComponent(entity, new ZombieSimulationAgentFOVResults());
		}
	}
}
