using UnityEngine;

public enum AgentMovementState
{
	AimlessWalking,
	MovingTowardsTarget,
	RunningAwayFromDanger
}

public enum AgentHealthState
{
	Healthy,
	Infected,
	Zombie
}

public class ZombieSimulationAgentGO : MonoBehaviour
{
	[SerializeField]
	private AgentMovementState m_MovementState;
	[SerializeField]
	private AgentHealthState m_HealthState;

	private MeshRenderer m_Renderer;

	public float InfectionStartTime { get; set; }
	public float NextDirectionChangeTime { get; set; }
	public float NextPossibleAttackTime { get; set; }
	public Vector3 Direction { get; set; }
	public ZombieSimulationAgentGO ClosestVisibleHealthyAgent { get; set; }
	public ZombieSimulationAgentGO ClosestVisibleZombieAgent { get; set; }

	public MeshRenderer Renderer => m_Renderer;

	public AgentMovementState MovementState
	{
		get => m_MovementState;
		set => m_MovementState = value;
	}

	public AgentHealthState HealthState
	{
		get => m_HealthState;
		set => m_HealthState = value;
	}

	void Awake()
	{
		m_Renderer = GetComponent<MeshRenderer>();
	}
}
