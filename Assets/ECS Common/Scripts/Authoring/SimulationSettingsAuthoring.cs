using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class SimulationSettingsAuthoring : MonoBehaviour
{
	[Header("Simulation Settings")]
	[SerializeField]
	private Bounds m_SimulationBounds;
	[SerializeField]
	private int m_AgentCount;
	[SerializeField]
	[Range(0.0f, 1.0f)]
	private float m_InfectedRatio;
	[SerializeField]
	private GameObject m_AgentPrefab;
	[SerializeField]
	private GameObject m_InfectedPrefab;

	[Header("Time Settings")]
	[SerializeField]
	private float m_MinDirectionChangeTime;
	[SerializeField]
	private float m_MaxDirectionChangeTime;
	[SerializeField]
	private float m_IncubationTime;
	[SerializeField]
	private float m_PanicDuration;

	[Header("FOV Settings")]
	[SerializeField]
	private float m_FOVDistance;
	[SerializeField]
	private float m_FOVAngle;

	[Header("Attack Settings")]
	[SerializeField]
	private float m_AttackDistance;
	[SerializeField]
	private float m_AttackCooldown;

	[Header("Speed Settings")]
	[SerializeField]
	private float m_CalmSpeed;
	[SerializeField]
	private float m_PanicSpeed;
	[SerializeField]
	private float m_ChaseSpeed;

	[Header("Color Settings")]
	[SerializeField]
	private Color m_HealthyColor;
	[SerializeField]
	private Color m_InfectedColor;
	[SerializeField]
	private Color m_ZombieColor;

	class Baker : Baker<SimulationSettingsAuthoring>
	{
		public override void Bake(SimulationSettingsAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			float3 minBounds = new float3(authoring.m_SimulationBounds.min.x, 0.0f, authoring.m_SimulationBounds.min.z);
			float3 maxBounds = new float3(authoring.m_SimulationBounds.max.x, 0.0f, authoring.m_SimulationBounds.max.z);

			AddComponent(entity, new SimulationSettings
			{
				MinBounds = minBounds,
				MaxBounds = maxBounds,
				AgentCount = authoring.m_AgentCount,
				InfectedRatio = authoring.m_InfectedRatio,
				AgentPrefab = GetEntity(authoring.m_AgentPrefab, TransformUsageFlags.Dynamic),
				InfectedPrefab = GetEntity(authoring.m_InfectedPrefab, TransformUsageFlags.Dynamic),

				MinDirectionChangeTime = authoring.m_MinDirectionChangeTime,
				MaxDirectionChangeTime = authoring.m_MaxDirectionChangeTime,
				IncubationTime = authoring.m_IncubationTime,
				PanicDuration = authoring.m_PanicDuration,

				FOVAngle = authoring.m_FOVAngle,
				FOVDistance = authoring.m_FOVDistance,

				AttackDistance = authoring.m_AttackDistance,
				AttackCooldown = authoring.m_AttackCooldown,

				CalmSpeed = authoring.m_CalmSpeed,
				PanicSpeed = authoring.m_PanicSpeed,
				ChaseSpeed = authoring.m_ChaseSpeed,

				HealthyColor = new float4(authoring.m_HealthyColor.r, authoring.m_HealthyColor.g, authoring.m_HealthyColor.b, authoring.m_HealthyColor.a),
				InfectedColor = new float4(authoring.m_InfectedColor.r, authoring.m_InfectedColor.g, authoring.m_InfectedColor.b, authoring.m_InfectedColor.a),
				ZombieColor = new float4(authoring.m_ZombieColor.r, authoring.m_ZombieColor.g, authoring.m_ZombieColor.b, authoring.m_ZombieColor.a)
			});
		}
	}
}
