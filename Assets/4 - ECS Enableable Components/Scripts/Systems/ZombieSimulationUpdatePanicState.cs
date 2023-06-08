using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdatePanicStateEnableable : ISystem
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

		foreach (var (panicState, entity) in SystemAPI.Query<ZombieSimulationPanicStateEnableable>().WithEntityAccess())
		{
			if (currentTime > panicState.PanicEndTime)
			{
				SystemAPI.SetComponentEnabled<ZombieSimulationPanicStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<ZombieSimulationCalmStateEnableable>(entity, true);
			}
		}
	}
}
