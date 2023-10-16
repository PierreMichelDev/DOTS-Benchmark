using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct UpdateStateSystem : ISystem
{
	private ProfilerMarker m_Marker;
	private Random m_Random;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		m_Random = new Random(1234);
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSEnum>();

		m_Marker = new ProfilerMarker("UpdateEnumState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		foreach (var (agentState, movement, fovResults, color, localTransform) in
				SystemAPI.Query<RefRW<AgentState>, RefRW<AgentMovement>, AgentFOVResults, RefRW<URPMaterialPropertyBaseColor>, LocalTransform>())
		{
			if (agentState.ValueRO.Health == AgentState.HealthState.Zombie)
			{
				if (fovResults.CanSeeHealthyHuman)
				{
					agentState.ValueRW.Movement = AgentState.MovementState.Chase;

					if (currentTime > agentState.ValueRO.NextAttackTime &&
						math.distancesq(localTransform.Position, fovResults.NearestVisibleHealthyPosition) < settings.AttackDistance * settings.AttackDistance)
					{
						var victimAgentState = SystemAPI.GetComponent<AgentState>(fovResults.NearestVisibleHealthyEntity);
						victimAgentState.Health = AgentState.HealthState.Infected;
						victimAgentState.IncubationEndTime = currentTime + settings.IncubationTime;
						SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, victimAgentState);
						SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, new URPMaterialPropertyBaseColor { Value = settings.InfectedColor });

						agentState.ValueRW.NextAttackTime = currentTime + settings.AttackCooldown;
					}
				}
				else
				{
					agentState.ValueRW.Movement = AgentState.MovementState.Calm;
					agentState.ValueRW.NextDirectionChangeTime = currentTime + m_Random.NextFloat(settings.MinDirectionChangeTime, settings.MaxDirectionChangeTime);
				}
			}
			else
			{
				if (fovResults.CanSeeZombie)
				{
					movement.ValueRW.Direction = math.normalize(localTransform.Position - fovResults.NearestVisibleZombiePosition);
					movement.ValueRW.Speed = settings.PanicSpeed;
					agentState.ValueRW.Movement = AgentState.MovementState.Panic;
					agentState.ValueRW.PanicEndTime = currentTime + settings.PanicDuration;
				}
			}

			switch (agentState.ValueRO.Movement)
			{
				case AgentState.MovementState.Calm:
				{
					if (currentTime > agentState.ValueRO.NextDirectionChangeTime)
					{
						float2 nextDirection = m_Random.NextFloat2Direction();
						movement.ValueRW.Direction = math.normalize(new float3(nextDirection.x, 0.0f, nextDirection.y));
						movement.ValueRW.Speed = settings.CalmSpeed;
						agentState.ValueRW.NextDirectionChangeTime = currentTime + m_Random.NextFloat(settings.MinDirectionChangeTime, settings.MaxDirectionChangeTime);
					}
					break;
				}
				case AgentState.MovementState.Panic:
				{
					if (currentTime > agentState.ValueRO.PanicEndTime)
					{
						agentState.ValueRW.Movement = AgentState.MovementState.Calm;
					}
					break;
				}
				case AgentState.MovementState.Chase:
				{
					movement.ValueRW.Direction = math.normalize(fovResults.NearestVisibleHealthyPosition - localTransform.Position);
					movement.ValueRW.Speed = settings.ChaseSpeed;
					break;
				}
			}

			if (agentState.ValueRO.Health == AgentState.HealthState.Infected && currentTime > agentState.ValueRO.IncubationEndTime)
			{
				agentState.ValueRW.Health = AgentState.HealthState.Zombie;
				agentState.ValueRW.Movement = AgentState.MovementState.Calm;
				movement.ValueRW.Speed = settings.CalmSpeed;
				color.ValueRW.Value = settings.ZombieColor;
			}
		}

		m_Marker.End();
	}
}
