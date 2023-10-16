using UnityEngine;
using Unity.Entities;

public class AgentAuthoringECSEnum : MonoBehaviour
{
	[SerializeField]
	private AgentState.HealthState m_StartState;
	[SerializeField]
	private float m_DefaultIncubationTime;

	class Baker : Baker<AgentAuthoringECSEnum>
	{
		public override void Bake(AgentAuthoringECSEnum authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new AgentState()
			{
				Health = authoring.m_StartState,
				NextDirectionChangeTime = 0,
				IncubationEndTime = authoring.m_DefaultIncubationTime
			});
			AddComponent(entity, new AgentFOVResults());
		}
	}
}
