using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct ZombieSimulationUpdateStateSystem : ISystem
{
	private Random m_Random;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		m_Random = new Random(1234);
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationECSEnum>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettings>();

		foreach (var (agentState, movement, fovResults, color, localTransform) in
				SystemAPI.Query<RefRW<ZombieSimulationAgentState>, RefRW<ZombieSimulationMovement>, ZombieSimulationAgentFOVResults, RefRW<URPMaterialPropertyBaseColor>, LocalTransform>())
		{
			if (agentState.ValueRO.Health == ZombieSimulationAgentState.HealthState.Zombie)
			{
				if (fovResults.CanSeeHealthyHuman)
				{
					agentState.ValueRW.Movement = ZombieSimulationAgentState.MovementState.Chase;

					if (currentTime > agentState.ValueRO.NextAttackTime &&
						math.distancesq(localTransform.Position, fovResults.NearestVisibleHealthyPosition) < settings.AttackDistance * settings.AttackDistance)
					{
						var victimAgentState = SystemAPI.GetComponent<ZombieSimulationAgentState>(fovResults.NearestVisibleHealthyEntity);
						victimAgentState.Health = ZombieSimulationAgentState.HealthState.Infected;
						victimAgentState.IncubationEndTime = currentTime + settings.IncubationTime;
						SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, victimAgentState);
						SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, new URPMaterialPropertyBaseColor { Value = settings.InfectedColor });

						agentState.ValueRW.NextAttackTime = currentTime + settings.AttackCooldown;
					}
				}
				else
				{
					agentState.ValueRW.Movement = ZombieSimulationAgentState.MovementState.Calm;
					agentState.ValueRW.NextDirectionChangeTime = currentTime + m_Random.NextFloat(settings.MinDirectionChangeTime, settings.MaxDirectionChangeTime);
				}
			}
			else
			{
				if (fovResults.CanSeeZombie)
				{
					movement.ValueRW.Direction = math.normalize(localTransform.Position - fovResults.NearestVisibleZombiePosition);
					movement.ValueRW.Speed = settings.PanicSpeed;
					agentState.ValueRW.Movement = ZombieSimulationAgentState.MovementState.Panic;
					agentState.ValueRW.PanicEndTime = currentTime + settings.PanicDuration;
				}
			}

			switch (agentState.ValueRO.Movement)
			{
				case ZombieSimulationAgentState.MovementState.Calm:
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
				case ZombieSimulationAgentState.MovementState.Panic:
				{
					if (currentTime > agentState.ValueRO.PanicEndTime)
					{
						agentState.ValueRW.Movement = ZombieSimulationAgentState.MovementState.Calm;
					}
					break;
				}
				case ZombieSimulationAgentState.MovementState.Chase:
				{
					movement.ValueRW.Direction = math.normalize(fovResults.NearestVisibleHealthyPosition - localTransform.Position);
					movement.ValueRW.Speed = settings.ChaseSpeed;
					break;
				}
			}

			if (agentState.ValueRO.Health == ZombieSimulationAgentState.HealthState.Infected && currentTime > agentState.ValueRO.IncubationEndTime)
			{
				agentState.ValueRW.Health = ZombieSimulationAgentState.HealthState.Zombie;
				agentState.ValueRW.Movement = ZombieSimulationAgentState.MovementState.Calm;
				movement.ValueRW.Speed = settings.CalmSpeed;
				color.ValueRW.Value = settings.ZombieColor;
			}
		}
	}
}
