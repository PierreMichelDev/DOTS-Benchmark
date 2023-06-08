using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateZombieStateEnableable : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationECSEnableable>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettings>();

		foreach (var (localTransform, fovResults, movement, zombieState, entity) in
				SystemAPI.Query<LocalTransform, ZombieSimulationAgentFOVResults, RefRW<ZombieSimulationMovement>, RefRW<ZombieSimulationZombieStateEnableable>>().WithEntityAccess())
		{
			if (fovResults.CanSeeHealthyHuman)
			{
				if (SystemAPI.IsComponentEnabled<ZombieSimulationCalmStateEnableable>(entity))
				{
					SystemAPI.SetComponentEnabled<ZombieSimulationCalmStateEnableable>(entity, false);
					SystemAPI.SetComponentEnabled<ZombieSimulationChaseStateEnableable>(entity, true);
				}

				movement.ValueRW.Direction = math.normalize(fovResults.NearestVisibleHealthyPosition - localTransform.Position);
				movement.ValueRW.Speed = settings.PanicSpeed;

				float foundAgentDistanceSq = math.distancesq(fovResults.NearestVisibleHealthyPosition, localTransform.Position);
				if (foundAgentDistanceSq < settings.AttackDistance * settings.AttackDistance && currentTime > zombieState.ValueRO.NextAttackTime)
				{
					SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, new URPMaterialPropertyBaseColor { Value = settings.InfectedColor });
					SystemAPI.SetComponent(fovResults.NearestVisibleHealthyEntity, new ZombieSimulationInfectedStateEnableable { IncubationEndTime = currentTime + settings.IncubationTime });
					SystemAPI.SetComponentEnabled<ZombieSimulationInfectedStateEnableable>(fovResults.NearestVisibleHealthyEntity, true);
					zombieState.ValueRW.NextAttackTime = currentTime + settings.AttackCooldown;
				}
			}
			else if (SystemAPI.IsComponentEnabled<ZombieSimulationChaseStateEnableable>(entity))
			{
				SystemAPI.SetComponentEnabled<ZombieSimulationChaseStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<ZombieSimulationCalmStateEnableable>(entity, true);
			}
		}
	}
}
