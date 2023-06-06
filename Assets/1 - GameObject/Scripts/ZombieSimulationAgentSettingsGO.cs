using UnityEngine;

[CreateAssetMenu(menuName = "Zombie Simulation/1 - Game Object/Agent Settings")]
public class ZombieSimulationAgentSettingsGO : ScriptableObject
{
	[SerializeField]
	private float m_CalmSpeed;
	[SerializeField]
	private float m_PanicSpeed;
	[SerializeField]
	private float m_ChaseSpeed;

	[SerializeField]
	private float m_DirectionChangeInterval;
	[SerializeField]
	private float m_DirectionChangeIntervalVariance;
	[SerializeField]
	private float m_PanicDuration;
	[SerializeField]
	private float m_IncubationDuration;

	[SerializeField]
	private float m_AttackDistance;
	[SerializeField]
	private float m_AttackCooldown;

	[SerializeField]
	private float m_FOVDistance;
	[SerializeField]
	private float m_FOVAngle;

	public float CalmSpeed => m_CalmSpeed;
	public float PanicSpeed => m_PanicSpeed;
	public float DirectionChangeInterval => m_DirectionChangeInterval;
	public float DirectionChangeIntervalVariance => m_DirectionChangeIntervalVariance;
	public float ChaseSpeed => m_ChaseSpeed;
	public float PanicDuration => m_PanicDuration;
	public float IncubationDuration => m_IncubationDuration;
	public float AttackDistance => m_AttackDistance;
	public float AttackCooldown => m_AttackCooldown;
	public float FOVDistance => m_FOVDistance;
	public float FOVAngle => m_FOVAngle;
}
