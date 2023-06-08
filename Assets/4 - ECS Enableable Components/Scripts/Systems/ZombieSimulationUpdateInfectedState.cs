using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateInfectedStateEnableable : ISystem
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

		foreach (var (infectedState, color, entity)
				in SystemAPI.Query<ZombieSimulationInfectedStateEnableable, RefRW<URPMaterialPropertyBaseColor>>().WithEntityAccess())
		{
			if (currentTime > infectedState.IncubationEndTime)
			{
				SystemAPI.SetComponentEnabled<ZombieSimulationHumanStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<ZombieSimulationInfectedStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<ZombieSimulationZombieStateEnableable>(entity, true);

				color.ValueRW.Value = settings.ZombieColor;

				//TODO: force calm state ?
			}
		}
	}
}
