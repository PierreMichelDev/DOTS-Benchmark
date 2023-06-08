using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateHuman : ISystem
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

		foreach (var (localTransform, fovResults, movement, entity) in SystemAPI.Query<LocalTransform, ZombieSimulationAgentFOVResults, RefRW<ZombieSimulationMovement>>()
					.WithAll<ZombieSimulationHumanState>().WithEntityAccess())
		{
			if (fovResults.CanSeeZombie)
			{
				movement.ValueRW.Direction = math.normalize(localTransform.Position - fovResults.NearestVisibleZombiePosition);
				movement.ValueRW.Speed = settings.PanicSpeed;

				ecb.RemoveComponent<ZombieSimulationCalmState>(entity);
				ecb.AddComponent(entity, new ZombieSimulationPanicState { PanicEndTime = currentTime + settings.PanicDuration });
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}
