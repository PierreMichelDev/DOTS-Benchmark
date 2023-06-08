using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateHumanEnableable : ISystem
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

		foreach (var (localTransform, fovResults, movement, entity) in
					SystemAPI.Query<LocalTransform, ZombieSimulationAgentFOVResults, RefRW<ZombieSimulationMovement>>()
					.WithAll<ZombieSimulationHumanStateEnableable>().WithEntityAccess())
		{
			if (fovResults.CanSeeZombie)
			{
				movement.ValueRW.Direction = math.normalize(localTransform.Position - fovResults.NearestVisibleZombiePosition);
				movement.ValueRW.Speed = settings.PanicSpeed;

				SystemAPI.SetComponent(entity, new ZombieSimulationPanicStateEnableable{ PanicEndTime = currentTime + settings.PanicDuration});
				SystemAPI.SetComponentEnabled<ZombieSimulationCalmStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<ZombieSimulationPanicStateEnableable>(entity, true);
			}
		}
	}
}
