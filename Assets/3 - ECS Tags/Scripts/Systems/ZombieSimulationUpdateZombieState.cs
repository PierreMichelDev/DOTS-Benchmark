using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateZombieState : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationECSTags>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettings>();

		var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (localTransform, fovResults, movement, zombieState, entity) in
				SystemAPI.Query<LocalTransform, ZombieSimulationAgentFOVResults, RefRW<ZombieSimulationMovement>, RefRW<ZombieSimulationZombieState>>().WithEntityAccess())
		{
			if (fovResults.CanSeeHealthyHuman)
			{
				if (SystemAPI.HasComponent<ZombieSimulationCalmState>(entity))
				{
					ecb.RemoveComponent<ZombieSimulationCalmState>(entity);
					ecb.AddComponent(entity, new ZombieSimulationChaseState());
				}

				movement.ValueRW.Direction = math.normalize(fovResults.NearestVisibleHealthyPosition - localTransform.Position);
				movement.ValueRW.Speed = settings.PanicSpeed;

				float foundAgentDistanceSq = math.distancesq(fovResults.NearestVisibleHealthyPosition, localTransform.Position);
				if (foundAgentDistanceSq < settings.AttackDistance * settings.AttackDistance && currentTime > zombieState.ValueRO.NextAttackTime)
				{
					ecb.SetComponent(fovResults.NearestVisibleHealthyEntity, new URPMaterialPropertyBaseColor { Value = settings.InfectedColor });
					ecb.AddComponent(fovResults.NearestVisibleHealthyEntity, new ZombieSimulationInfectedState { IncubationEndTime = currentTime + settings.IncubationTime });
					zombieState.ValueRW.NextAttackTime = currentTime + settings.AttackCooldown;
				}
			}
			else if (SystemAPI.HasComponent<ZombieSimulationChaseState>(entity))
			{
				ecb.RemoveComponent<ZombieSimulationChaseState>(entity);
				ecb.AddComponent(entity, new ZombieSimulationCalmState());
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}
