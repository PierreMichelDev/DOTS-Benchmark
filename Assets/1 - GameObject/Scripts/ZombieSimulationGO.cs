using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieSimulationGO : MonoBehaviour
{
    [SerializeField]
    private Bounds m_SimulationBounds;
    [SerializeField]
    private ZombieSimulationAgentGO m_AgentPrefab;
    [SerializeField]
    private int m_HumanCount;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_InfectedRatio;

    [SerializeField]
    private Material m_InfectedMaterial;
    [SerializeField]
    private Material m_ZombieMaterial;

    [SerializeField]
    private ZombieSimulationAgentSettingsGO m_HumanSettings;
    [SerializeField]
    private ZombieSimulationAgentSettingsGO m_ZombieSettings;

    private List<ZombieSimulationAgentGO> m_Agents = new();

    private static readonly ProfilerMarker s_AgentUpdateProfilerMarker = new ("SingleZombieSimulationAgentUpdate");

    private void Start()
    {
        int infectedCount = Mathf.RoundToInt(m_HumanCount * m_InfectedRatio);
        for (int i = 0; i < m_HumanCount; ++i)
        {
            Vector3 entityPosition = new Vector3(
                Random.Range(m_SimulationBounds.min.x, m_SimulationBounds.max.x),
                Random.Range(m_SimulationBounds.min.y, m_SimulationBounds.max.y),
                Random.Range(m_SimulationBounds.min.z, m_SimulationBounds.max.z)
            );
            ZombieSimulationAgentGO newAgent = Instantiate(m_AgentPrefab, entityPosition, Quaternion.identity);
            if (i < infectedCount) { InfectAgent(newAgent); }

            ApplyDirection(newAgent, m_HumanSettings.DirectionChangeInterval, m_HumanSettings.DirectionChangeIntervalVariance,
                new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)));

            m_Agents.Add(newAgent);
        }
    }

    private void Update()
    {
        foreach (var agent in m_Agents)
        {
            using (s_AgentUpdateProfilerMarker.Auto())
            {
                UpdateAgentFOV(agent);
                UpdateAgentDirection(agent);
                UpdateAgentMovement(agent);
                UpdateAgentAttack(agent);
                UpdateAgentHealth(agent);
            }
        }
    }

    private void InfectAgent(ZombieSimulationAgentGO agent)
    {
        agent.InfectionStartTime = Time.time;
        agent.HealthState = AgentHealthState.Infected;
        agent.Renderer.material = m_InfectedMaterial;
    }

    private void TurnAgentToZombie(ZombieSimulationAgentGO agent)
    {
        agent.HealthState = AgentHealthState.Zombie;
        agent.Renderer.material = m_ZombieMaterial;
    }

    private void UpdateAgentDirection(ZombieSimulationAgentGO agent)
    {
        if (agent.MovementState == AgentMovementState.AimlessWalking)
        {
            if (Time.time > agent.NextDirectionChangeTime)
            {
                ZombieSimulationAgentSettingsGO settings = agent.HealthState == AgentHealthState.Zombie ? m_ZombieSettings : m_HumanSettings;
                ApplyDirection(agent, settings.DirectionChangeInterval, settings.DirectionChangeIntervalVariance,
                    new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)));
            }
        }

        if (agent.HealthState == AgentHealthState.Zombie)
        {
            if (agent.ClosestVisibleHealthyAgent != null)
            {
                agent.MovementState = AgentMovementState.MovingTowardsTarget;
                ApplyDirection(agent, float.PositiveInfinity, 0.0f,
                    agent.ClosestVisibleHealthyAgent.transform.position - agent.transform.position);
            }
            else
            {
                agent.MovementState = AgentMovementState.AimlessWalking;
            }
        }
        else
        {
            if (agent.ClosestVisibleZombieAgent != null)
            {
                agent.MovementState = AgentMovementState.RunningAwayFromDanger;
                ApplyDirection(agent, m_HumanSettings.PanicDuration, 0.0f,
                    agent.transform.position - agent.ClosestVisibleZombieAgent.transform.position);
            }
            else if (Time.time > agent.NextDirectionChangeTime)
            {
                agent.MovementState = AgentMovementState.AimlessWalking;
            }
        }
    }

    private void UpdateAgentMovement(ZombieSimulationAgentGO agent)
    {
        ZombieSimulationAgentSettingsGO settings = agent.HealthState == AgentHealthState.Zombie ? m_ZombieSettings : m_HumanSettings;
        float speed = agent.MovementState switch
        {
            AgentMovementState.AimlessWalking => settings.CalmSpeed,
            AgentMovementState.MovingTowardsTarget => settings.ChaseSpeed,
            AgentMovementState.RunningAwayFromDanger => settings.PanicSpeed,
            _ => 0.0f
        };

        Vector3 newPosition = agent.transform.position + agent.Direction * (Time.deltaTime * speed);

        if (m_SimulationBounds.Contains(newPosition))
        {
            agent.transform.position = newPosition;
        }
        else
        {
            if (newPosition.x < m_SimulationBounds.min.x || newPosition.x > m_SimulationBounds.max.x)
            {
                agent.Direction = new Vector3(-agent.Direction.x, agent.Direction.y, agent.Direction.z);
            }
            if (newPosition.z < m_SimulationBounds.min.z || newPosition.z > m_SimulationBounds.max.z)
            {
                agent.Direction = new Vector3(agent.Direction.x, agent.Direction.y, -agent.Direction.z);
            }
        }
    }

    private static void ApplyDirection(ZombieSimulationAgentGO agent, float duration, float durationVariation, Vector3 direction)
    {
        agent.NextDirectionChangeTime = Time.time + duration + Random.value * durationVariation;
        agent.Direction = direction.normalized;
    }

    private void UpdateAgentHealth(ZombieSimulationAgentGO agent)
    {
        if (agent.HealthState == AgentHealthState.Infected && Time.time > agent.InfectionStartTime + m_HumanSettings.IncubationDuration)
        {
            TurnAgentToZombie(agent);
        }
    }

    private void UpdateAgentFOV(ZombieSimulationAgentGO agent)
    {
        agent.ClosestVisibleZombieAgent = null;
        agent.ClosestVisibleHealthyAgent = null;

        float closestVisibleHealthySqrDistance = float.MaxValue;
        float closestVisibleZombieSqrDistance = float.MaxValue;

        foreach (var otherAgent in m_Agents)
        {
            if (agent != otherAgent && IsInFOV(agent.transform.position, agent.Direction, otherAgent.transform.position, m_ZombieSettings.FOVDistance, m_ZombieSettings.FOVAngle, out float sqrDistance))
            {
                if (otherAgent.HealthState == AgentHealthState.Infected && sqrDistance < closestVisibleZombieSqrDistance)
                {
                    closestVisibleZombieSqrDistance = sqrDistance;
                    agent.ClosestVisibleZombieAgent = otherAgent;
                }
                else if (otherAgent.HealthState == AgentHealthState.Healthy && sqrDistance < closestVisibleHealthySqrDistance)
                {
                    closestVisibleHealthySqrDistance = sqrDistance;
                    agent.ClosestVisibleHealthyAgent = otherAgent;
                }
            }
        }
    }

    private static bool IsInFOV(Vector3 origin, Vector3 direction, Vector3 position, float maxDistance, float maxAngle, out float actualSqrDistance)
    {
        Vector3 diff = position - origin;
        float angle = Vector3.Angle(direction, diff);
        actualSqrDistance = diff.sqrMagnitude;
        return angle <= maxAngle && actualSqrDistance <= maxDistance * maxDistance;
    }

    private void UpdateAgentAttack(ZombieSimulationAgentGO agent)
    {
        if (agent.HealthState == AgentHealthState.Zombie && agent.ClosestVisibleHealthyAgent != null && Time.time > agent.NextPossibleAttackTime)
        {
            Vector3 positionDiff = agent.transform.position - agent.ClosestVisibleHealthyAgent.transform.position;
            if (positionDiff.sqrMagnitude < m_ZombieSettings.AttackDistance * m_ZombieSettings.AttackDistance)
            {
                agent.NextPossibleAttackTime = Time.time + m_ZombieSettings.AttackCooldown;
                InfectAgent(agent.ClosestVisibleHealthyAgent);
            }
        }
    }
}
