using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct UpdateZombieStateEnableable : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSEnableable>();

		m_Marker = new ProfilerMarker("UpdateZombieState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		foreach (var (localTransform, fovResults, movement, zombieState, entity) in
				SystemAPI.Query<LocalTransform, AgentFOVResults, RefRW<AgentMovement>, RefRW<ZombieStateEnableable>>().WithEntityAccess())
		{
			if (fovResults.CanSeeHealthyHuman)
			{
				if (SystemAPI.IsComponentEnabled<CalmStateEnableable>(entity))
				{
					SystemAPI.SetComponentEnabled<CalmStateEnableable>(entity, false);
					SystemAPI.SetComponentEnabled<ChaseStateEnableable>(entity, true);
				}

				movement.ValueRW.Direction = math.normalize(fovResults.NearestVisibleHealthyPosition - localTransform.Position);
				movement.ValueRW.Speed = settings.PanicSpeed;

				float foundAgentDistanceSq = math.distancesq(fovResults.NearestVisibleHealthyPosition, localTransform.Position);
				if (foundAgentDistanceSq < settings.AttackDistance * settings.AttackDistance && currentTime > zombieState.ValueRO.NextAttackTime)
				{
					SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, new URPMaterialPropertyBaseColor { Value = settings.InfectedColor });
					SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, new InfectedStateEnableable { IncubationEndTime = currentTime + settings.IncubationTime });
					SystemAPI.SetComponentEnabled<InfectedStateEnableable>(fovResults.NearestVisibleHealthyEntity, true);
					zombieState.ValueRW.NextAttackTime = currentTime + settings.AttackCooldown;
				}
			}
			else if (SystemAPI.IsComponentEnabled<ChaseStateEnableable>(entity))
			{
				SystemAPI.SetComponentEnabled<ChaseStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<CalmStateEnableable>(entity, true);
			}
		}

		m_Marker.End();
	}
}
