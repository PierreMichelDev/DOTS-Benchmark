using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdatePanicState : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float currentTime = (float)SystemAPI.Time.ElapsedTime;

		var ecb = new EntityCommandBuffer(Allocator.Temp);
		foreach (var (panicState, entity) in SystemAPI.Query<ZombieSimulationPanicState>().WithEntityAccess())
		{
			if (currentTime > panicState.PanicEndTime)
			{
				ecb.RemoveComponent<ZombieSimulationPanicState>(entity);
				ecb.AddComponent(entity, new ZombieSimulationCalmState());
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}
