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
public partial struct UpdateZombieState : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSTags>();

		m_Marker = new ProfilerMarker("UpdateZombieState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (localTransform, fovResults, movement, zombieState, entity) in
				SystemAPI.Query<LocalTransform, AgentFOVResults, RefRW<AgentMovement>, RefRW<ZombieState>>().WithEntityAccess())
		{
			if (fovResults.CanSeeHealthyHuman)
			{
				if (SystemAPI.HasComponent<CalmState>(entity))
				{
					ecb.RemoveComponent<CalmState>(entity);
					ecb.AddComponent(entity, new ChaseState());
				}

				movement.ValueRW.Direction = math.normalize(fovResults.NearestVisibleHealthyPosition - localTransform.Position);
				movement.ValueRW.Speed = settings.PanicSpeed;

				float foundAgentDistanceSq = math.distancesq(fovResults.NearestVisibleHealthyPosition, localTransform.Position);
				if (foundAgentDistanceSq < settings.AttackDistance * settings.AttackDistance && currentTime > zombieState.ValueRO.NextAttackTime)
				{
					ecb.SetComponent(fovResults.NearestVisibleHealthyEntity, new URPMaterialPropertyBaseColor { Value = settings.InfectedColor });
					ecb.AddComponent(fovResults.NearestVisibleHealthyEntity, new InfectedState { IncubationEndTime = currentTime + settings.IncubationTime });
					zombieState.ValueRW.NextAttackTime = currentTime + settings.AttackCooldown;
				}
			}
			else if (SystemAPI.HasComponent<ChaseState>(entity))
			{
				ecb.RemoveComponent<ChaseState>(entity);
				ecb.AddComponent(entity, new CalmState());
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();

		m_Marker.End();
	}
}
